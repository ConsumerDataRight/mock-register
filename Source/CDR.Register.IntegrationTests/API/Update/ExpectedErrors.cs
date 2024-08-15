using CDR.Register.IntegrationTests.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CDR.Register.IntegrationTests.API.Update
{
    //TODO: Work out why this class exists and errors are defined in multiple places in IntTests project
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
        private List<ExpectedApiErrors> _expectedErrors;
        private const string CDS_ERROR_PREFIX = "urn:au-cds:error:cds-all:";

        public List<ExpectedApiErrors> errors { get => _expectedErrors.ToList(); }

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
