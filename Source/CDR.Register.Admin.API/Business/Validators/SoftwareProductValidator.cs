using System;
using CDR.Register.Admin.API.Business.Model;
using CDR.Register.Repository.Enums;
using FluentValidation;
using static CDR.Register.Domain.Constants;

namespace CDR.Register.Admin.API.Business.Validators
{
    public class SoftwareProductValidator : AbstractValidator<SoftwareProduct>
    {
        public SoftwareProductValidator()
        {
            this.RuleFor(x => x.SoftwareProductId).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.SoftwareProductName).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.SoftwareProductDescription).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.LogoUri).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.ClientUri).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.RecipientBaseUri).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.RevocationUri).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.RedirectUris).Must(x => x != null && x.Length > 0).WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.JwksUri).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.Status).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.Certificates).Must(x => x != null && x.Count > 0).WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);

            // lengths
            this.RuleFor(x => x.SoftwareProductName).MaximumLength(200).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.SoftwareProductName}' is not allowed for SoftwareProductName");
            this.RuleFor(x => x.SoftwareProductDescription).MaximumLength(4000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.SoftwareProductDescription}' is not allowed for SoftwareProductDescription");
            this.RuleFor(x => x.LogoUri).MaximumLength(1000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.LogoUri}' is not allowed for LogoUri");
            this.RuleFor(x => x.ClientUri).MaximumLength(1000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.ClientUri}' is not allowed for ClientUri");
            this.RuleFor(x => x.SectorIdentifierUri).MaximumLength(2048).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.SectorIdentifierUri}' is not allowed for SectorIdentifierUri");
            this.RuleFor(x => x.TosUri).MaximumLength(1000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.TosUri}' is not allowed for TosUri");
            this.RuleFor(x => x.PolicyUri).MaximumLength(1000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.PolicyUri}' is not allowed for PolicyUri");
            this.RuleFor(x => x.RecipientBaseUri).MaximumLength(1000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.RecipientBaseUri}' is not allowed for RecipientBaseUri");
            this.RuleFor(x => x.RevocationUri).MaximumLength(1000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.RevocationUri}' is not allowed for RevocationUri");
            this.RuleForEach(x => x.RedirectUris).MaximumLength(2000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.RedirectUris}' is not allowed for RedirectUris");
            this.RuleFor(x => x.JwksUri).MaximumLength(1000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.JwksUri}' is not allowed for JwksUri");
            this.RuleFor(x => x.Scope).MaximumLength(1000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.Scope}' is not allowed for Scope");
            this.RuleFor(x => x.Status).MaximumLength(9).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.Status}' is not allowed for Status");

            // enum
            this.RuleFor(x => x.Status).Must(x => Enum.TryParse(x, true, out SoftwareProductStatusType _)).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.Status}' is not allowed for Status");

            this.RuleForEach(x => x.Certificates).SetValidator(new SoftwareProductCertificateValidator());
        }
    }
}
