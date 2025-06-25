using System;
using System.Collections.Generic;
using System.Linq;
using CDR.Register.IntegrationTests.Models;
using Newtonsoft.Json;

namespace CDR.Register.IntegrationTests.API.Update
{
    public partial class ExpectedErrors
    {
        private const string CDS_ERROR_PREFIX = "urn:au-cds:error:cds-all:";
        private readonly List<ExpectedApiErrors> _expectedErrors;

        public ExpectedErrors()
        {
            this._expectedErrors = new List<ExpectedApiErrors>();
        }

        /// <summary>
        /// Gets expected errors. This is used in serialised JSON to compare against the actual JSON. Do not remove.
        /// </summary>
        [JsonProperty(PropertyName = "errors")]
        public List<ExpectedApiErrors> Errors { get => this._expectedErrors.ToList(); }

        public void AddExpectedError(ErrorType errorType, string detail)
        {
            switch (errorType)
            {
                case ErrorType.MissingField:
                    this._expectedErrors.Add(new ExpectedApiErrors()
                    {
                        Code = $"{CDS_ERROR_PREFIX}Field/Missing",
                        Title = "Missing Required Field",
                        Detail = detail,
                    });
                    return;
                case ErrorType.MissingHeader:
                    this._expectedErrors.Add(new ExpectedApiErrors()
                    {
                        Code = $"{CDS_ERROR_PREFIX}Header/Missing",
                        Title = "Missing Required Header",
                        Detail = detail,
                    });
                    return;
                case ErrorType.InvalidField:
                    this._expectedErrors.Add(new ExpectedApiErrors()
                    {
                        Code = $"{CDS_ERROR_PREFIX}Field/Invalid",
                        Title = "Invalid Field",
                        Detail = detail,
                    });
                    return;
                case ErrorType.InvalidVersion:
                    this._expectedErrors.Add(new ExpectedApiErrors()
                    {
                        Code = $"{CDS_ERROR_PREFIX}Header/InvalidVersion",
                        Title = "Invalid Version",
                        Detail = detail,
                    });
                    return;
                case ErrorType.UnsupportedVersion:
                    this._expectedErrors.Add(new ExpectedApiErrors()
                    {
                        Code = $"{CDS_ERROR_PREFIX}Header/UnsupportedVersion",
                        Title = "Unsupported Version",
                        Detail = detail,
                    });
                    return;
                case ErrorType.Unauthorized:
                    this._expectedErrors.Add(new ExpectedApiErrors()
                    {
                        Code = $"401",
                        Title = "Unauthorized",
                        Detail = detail,
                    });
                    return;

                default:
                    throw new NotSupportedException($"{nameof(ErrorType)}={errorType}");
            }
        }
    }
}
