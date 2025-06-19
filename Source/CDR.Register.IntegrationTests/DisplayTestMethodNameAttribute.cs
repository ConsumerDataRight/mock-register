using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using CDR.Register.Domain.Models;
using CDR.Register.IntegrationTests.API.Update;
using CDR.Register.IntegrationTests.Extensions;
using CDR.Register.Repository.Infrastructure;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.SystemConsole.Themes;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

#nullable enable

namespace CDR.Register.IntegrationTests
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class DisplayTestMethodNameAttribute : BeforeAfterTestAttribute
    {
        private static int count = 0;

        public override void Before(MethodInfo methodUnderTest)
        {
            Log.Information($"********** Test #{++count} - {methodUnderTest.DeclaringType?.Name}.{methodUnderTest.Name} **********");
            Console.WriteLine($"Test #{++count} - {methodUnderTest.DeclaringType?.Name}.{methodUnderTest.Name}");
        }
    }
}
