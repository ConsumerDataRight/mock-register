using CDR.Register.Admin.API.Business.Model;
using FluentValidation;
using System;
using System.Linq;
using static CDR.Register.Domain.Constants;

namespace CDR.Register.Admin.API.Business.Validators
{
    public class SoftwareProductValidator : AbstractValidator<SoftwareProduct>
    {
        public SoftwareProductValidator()
        {
            RuleFor(x => x.SoftwareProductId).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            RuleFor(x => x.SoftwareProductName).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            RuleFor(x => x.SoftwareProductDescription).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            RuleFor(x => x.LogoUri).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            RuleFor(x => x.ClientUri).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            RuleFor(x => x.RecipientBaseUri).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            RuleFor(x => x.RevocationUri).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            RuleFor(x => x.RedirectUris).Must(x => x!= null && x.Length>0).WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            RuleFor(x => x.JwksUri).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            RuleFor(x => x.Status).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            RuleFor(x => x.Certificates).Must(x => x!= null && x.Count>0).WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);

            //lengths
            RuleFor(x => x.SoftwareProductName).MaximumLength(200).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.SoftwareProductName}' is not allowed for SoftwareProductName");
            RuleFor(x => x.SoftwareProductDescription).MaximumLength(4000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.SoftwareProductDescription}' is not allowed for SoftwareProductDescription");
            RuleFor(x => x.LogoUri).MaximumLength(1000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.LogoUri}' is not allowed for LogoUri");
            RuleFor(x => x.ClientUri).MaximumLength(1000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.ClientUri}' is not allowed for ClientUri");
            RuleFor(x => x.SectorIdentifierUri).MaximumLength(2048).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.SectorIdentifierUri}' is not allowed for SectorIdentifierUri");
            RuleFor(x => x.TosUri).MaximumLength(1000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.TosUri}' is not allowed for TosUri");
            RuleFor(x => x.PolicyUri).MaximumLength(1000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.PolicyUri}' is not allowed for PolicyUri");
            RuleFor(x => x.RecipientBaseUri).MaximumLength(1000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.RecipientBaseUri}' is not allowed for RecipientBaseUri");
            RuleFor(x => x.RevocationUri).MaximumLength(1000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.RevocationUri}' is not allowed for RevocationUri");
            RuleForEach(x => x.RedirectUris).MaximumLength(2000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.RedirectUris}' is not allowed for RedirectUris");
            RuleFor(x => x.JwksUri).MaximumLength(1000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.JwksUri}' is not allowed for JwksUri");
            RuleFor(x => x.Scope).MaximumLength(1000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.Scope}' is not allowed for Scope");
            RuleFor(x => x.Status).MaximumLength(9).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.Status}' is not allowed for Status");

            //enum
            RuleFor(x => x.Status).Must(x => Enum.TryParse(x, true, out Repository.Entities.SoftwareProductStatusType result)).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(x => $"Value '{x.Status}' is not allowed for Status");

            RuleForEach(x => x.Certificates).SetValidator(new SoftwareProductCertificateValidator());
        }        
    }
}
