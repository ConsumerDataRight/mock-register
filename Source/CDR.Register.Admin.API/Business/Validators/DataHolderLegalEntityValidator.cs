using CDR.Register.Admin.API.Business.Model;
using CDR.Register.Domain.Entities;
using FluentValidation;
using System;
using static CDR.Register.Domain.Constants;

namespace CDR.Register.Admin.API.Business.Validators
{
    public class DataHolderLegalEntityValidator : AbstractValidator<DataHolderLegalEntityModel>
    {
        public DataHolderLegalEntityValidator()
        {
            // Mandatory Field Validations
            RuleFor(x => x.LegalEntityId).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            RuleFor(x => x.LegalEntityName).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            RuleFor(x => x.LogoUri).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            RuleFor(x => x.Status).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);

            // Enum Validations
            RuleFor(x => x.Status).Must(x => Enum.TryParse(x, true, out DhStatus result)).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField);
            RuleFor(x => x.OrganisationType).Must(x => string.IsNullOrEmpty(x) || Enum.TryParse(x.Replace("_", string.Empty), true, out OrganisationType result)).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField);

            // Length Validations
            RuleFor(x => x.LegalEntityName).MaximumLength(200).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField);
            RuleFor(x => x.LogoUri).MaximumLength(1000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField);
            RuleFor(x => x.RegistrationNumber).MaximumLength(100).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField);
            RuleFor(x => x.RegisteredCountry).MaximumLength(100).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField);
            RuleFor(x => x.Abn).MaximumLength(11).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField);
            RuleFor(x => x.Acn).MaximumLength(9).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField);
            RuleFor(x => x.Arbn).MaximumLength(9).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField);
            RuleFor(x => x.AnzsicDivision).MaximumLength(100).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField);
        }
    }
}
