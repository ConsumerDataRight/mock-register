using CDR.Register.IntegrationTests.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CDR.Register.IntegrationTests.API.Update
{
    public class ExpectedErrors
    {
        public enum ErrorType
        {
            MissingField,
            MissingHeader,
            InvalidField,
            InvalidVersion,
            UnsupportedVersion,
            Unauthorized
        }

        private const string CDS_ERROR_PREFIX = "urn:au-cds:error:cds-all:";
        private readonly List<ExpectedApiErrors> _expectedErrors;

        /// <summary>
        /// This is used in serialised JSON to compare against the actual JSON. Do not remove.
        /// </summary>
        [JsonProperty(PropertyName = "errors")]
        public List<ExpectedApiErrors> Errors { get => _expectedErrors.ToList(); }

        public ExpectedErrors()
        {
            _expectedErrors = new List<ExpectedApiErrors>();
        }

        public void AddExpectedError(ErrorType errorType, string detail)
        {
            switch (errorType)
            {
                case ErrorType.MissingField:
                    _expectedErrors.Add(new ExpectedApiErrors()
                    {
                        Code = $"{CDS_ERROR_PREFIX}Field/Missing",
                        Title = "Missing Required Field",
                        Detail = detail,
                    });
                    return;
                case ErrorType.MissingHeader:
                    _expectedErrors.Add(new ExpectedApiErrors()
                    {
                        Code = $"{CDS_ERROR_PREFIX}Header/Missing",
                        Title = "Missing Required Header",
                        Detail = detail,
                    });
                    return;
                case ErrorType.InvalidField:
                    _expectedErrors.Add(new ExpectedApiErrors()
                    {
                        Code = $"{CDS_ERROR_PREFIX}Field/Invalid",
                        Title = "Invalid Field",
                        Detail = detail,
                    });
                    return;
                case ErrorType.InvalidVersion:
                    _expectedErrors.Add(new ExpectedApiErrors()
                    {
                        Code = $"{CDS_ERROR_PREFIX}Header/InvalidVersion",
                        Title = "Invalid Version",
                        Detail = detail,
                    });
                    return;
                case ErrorType.UnsupportedVersion:
                    _expectedErrors.Add(new ExpectedApiErrors()
                    {
                        Code = $"{CDS_ERROR_PREFIX}Header/UnsupportedVersion",
                        Title = "Unsupported Version",
                        Detail = detail,
                    });
                    return;
                case ErrorType.Unauthorized:
                    _expectedErrors.Add(new ExpectedApiErrors()
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
