namespace CDR.Register.Domain
{
    public static class Constants
    {
        public static class ErrorCodes
        {
            /// <summary>
            /// The error codes in this class area defined by the CDR program (not CDS)
            /// </summary>
            public static class Generic
            {
                public const string UnsupportedGrantType = "unsupported_grant_type";
                public const string InvalidClient = "invalid_client";
                public const string InvalidRequest = "invalid_request";
                public const string InvalidRequestUri = "invalid_request_uri";
                public const string InvalidGrant = "invalid_grant";
                public const string AccessDenied = "access_denied";
                public const string InvalidRequestObject = "invalid_request_object";
                public const string UnauthorizedClient = "unauthorized_client";
                public const string UnsupportedResponseType = "unsupported_response_type";
                public const string InvalidScope = "invalid_scope";
                public const string InvalidRedirectUri = "invalid_redirect_uri";
                public const string InvalidClientMetadata = "invalid_client_metadata";
                public const string InvalidSoftwareStatement = "invalid_software_statement";
                public const string UnapprovedSoftwareStatement = "unapproved_software_statement";
            }
            /// <summary>
            /// The error codes in this class must match the definition in CDS
            /// </summary>
            public static class Cds
            {
                public const string MissingRequiredHeader = "urn:au-cds:error:cds-all:Header/Missing";
                public const string MissingRequiredField = "urn:au-cds:error:cds-all:Field/Missing";
                public const string InvalidField = "urn:au-cds:error:cds-all:Field/Invalid";
                public const string InvalidDateTime = "urn:au-cds:error:cds-all:Field/InvalidDateTime";
                public const string InvalidPageSize = "urn:au-cds:error:cds-all:Field/InvalidPageSize";
                public const string InvalidPage = "urn:au-cds:error:cds-all:Field/InvalidPage";
                public const string InvalidBrand = "urn:au-cds:error:cds-register:Field/InvalidBrand";
                public const string InvalidIndustry = "urn:au-cds:error:cds-register:Field/InvalidIndustry";
                public const string InvalidSoftwareProduct = "urn:au-cds:error:cds-register:Field/InvalidSoftwareProduct";
                public const string InvalidResource = "urn:au-cds:error:cds-all:Resource/Invalid";
                public const string InvalidHeader = "urn:au-cds:error:cds-all:Header/Invalid";
                public const string InvalidVersion = "urn:au-cds:error:cds-all:Header/InvalidVersion";
                public const string InvalidConsentArrangement = "urn:au-cds:error:cds-all:Authorisation/InvalidArrangement";
                public const string UnexpectedError = "urn:au-cds:error:cds-all:GeneralError/Unexpected";
                public const string ExpectedError = "urn:au-cds:error:cds-all:GeneralError/Expected";
                public const string ServiceUnavailable = "urn:au-cds:error:cds-all:Service/Unavailable";
                public const string AdrStatusNotActive = "urn:au-cds:error:cds-all:Authorisation/AdrStatusNotActive";
                public const string RevokedConsent = "urn:au-cds:error:cds-all:Authorisation/RevokedConsent";
                public const string InvalidConsent = "urn:au-cds:error:cds-all:Authorisation/InvalidConsent";
                public const string ResourceNotImplemented = "urn:au-cds:error:cds-all:Resource/NotImplemented";
                public const string ResourceNotFound = "urn:au-cds:error:cds-all:Resource/NotFound";
                public const string UnsupportedVersion = "urn:au-cds:error:cds-all:Header/UnsupportedVersion";
                public const string UnavailableResource = "urn:au-cds:error:cds-all:Resource/Unavailable";
            }
        }

        public static class ErrorTitles
        {
            public const string MissingVersion = "Missing Version";
            public const string UnsupportedVersion = "Unsupported Version";
            public const string InvalidVersion = "Invalid Version";
            public const string ExpectedError = "Expected Error Encountered";
            public const string UnexpectedError = "Unexpected Error Encountered";
            public const string ServiceUnavailable = "Service Unavailable";
            public const string MissingRequiredField = "Missing Required Field";
            public const string MissingRequiredHeader = "Missing Required Header";
            public const string InvalidField = "Invalid Field";
            public const string InvalidHeader = "Invalid Header";
            public const string InvalidDate = "Invalid Date";
            public const string InvalidDateTime = "Invalid DateTime";
            public const string InvalidPageSize = "Invalid Page Size";
            public const string ADRStatusNotActive = "ADR Status Is Not Active";
            public const string RevokedConsent = "Consent Is Revoked";
            public const string InvalidConsent = "Consent Is Invalid";
            public const string ResourceNotImplemented = "Resource Not Implemented";
            public const string ResourceNotFound = "Resource Not Found";
            public const string InvalidConsentArrangement = "Invalid Consent Arrangement";
            public const string InvalidPage = "Invalid Page";
            public const string InvalidResource = "Invalid Resource";
            public const string UnavailableResource = "Unavailable Resource";
            public const string InvalidBrand = "Invalid Brand";
            public const string InvalidIndustry = "Invalid Industry";
            public const string InvalidSoftwareProduct = "Invalid Software Product";
        }
    }
}
