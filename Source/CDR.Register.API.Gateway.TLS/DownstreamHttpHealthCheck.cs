using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CDR.Register.API.Gateway.TLS;

/// <summary>
/// Health checks to ensure all configured mock Authorisation servers are available.
/// </summary>
public class DownstreamHttpHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
#pragma warning disable S1075 // URIs should not be hardcoded
    private readonly Dictionary<string, Uri> _endpoints = new()
    {
        { "InfoSec", new Uri("https://localhost:7002") },
        { "Discovery", new Uri("https://localhost:7003") },
        { "Status", new Uri("https://localhost:7004") },
        { "SSA", new Uri("https://localhost:7005") },
        { "Admin", new Uri("https://localhost:7006") },
    };
#pragma warning restore S1075 // URIs should not be hardcoded

    /// <summary>
    /// Initializes a new instance of the <see cref="DownstreamHttpHealthCheck"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The http client factory.</param>
    public DownstreamHttpHealthCheck(IHttpClientFactory httpClientFactory)
    {
        this._httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using var client = this._httpClientFactory.CreateClient(nameof(DownstreamHttpHealthCheck));
        client.Timeout = context.Registration.Timeout;

        var checks = this._endpoints.Select(x => CheckHealth(client, x.Key, x.Value));

        var results = await Task.WhenAll(checks);

        if (results.All(x => x.Exception == null && x.Response.IsSuccessStatusCode))
        {
            return new HealthCheckResult(HealthStatus.Healthy);
        }

        var data = results.Where(x => x.Exception != null || !x.Response.IsSuccessStatusCode).ToDictionary(x => x.Name, x => (object)new { x.Endpoint, x.Response?.StatusCode, x.Response?.ReasonPhrase, x.Exception?.Message });

        var agg = new AggregateException("One or more health checks failed.", results.Where(x => x.Exception is not null).Select(x => x.Exception));

        return new HealthCheckResult(HealthStatus.Degraded, "Not all APIs are available", agg, new ReadOnlyDictionary<string, object>(data));
    }

    /// <summary>
    /// Checks that the health endpoint for the supplied <paramref name="apiBaseUri"/> is available.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="name">The name of the API.</param>
    /// <param name="apiBaseUri">The URI of the downstream API.</param>
    /// <returns>The result of the check.</returns>
    private static async Task<(string Name, Uri Endpoint, HttpResponseMessage Response, Exception Exception)> CheckHealth(HttpClient client, string name, Uri apiBaseUri)
    {
        HttpResponseMessage result = null;
        var url = new Uri(apiBaseUri, "health");

        try
        {
            result = await client.GetAsync(url);
        }
        catch (HttpRequestException ex)
        {
            return (name, url, result, ex);
        }

        return (name, url, result, null);
    }
}
