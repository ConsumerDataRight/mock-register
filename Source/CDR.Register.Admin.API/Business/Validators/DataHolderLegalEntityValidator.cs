using CDR.Register.Admin.API.Business.Model;
using CDR.Register.Domain.Entities;
using FluentValidation;
using System;
using static CDR.Register.API.Infrastructure.Constants;

namespace CDR.Register.Admin.API.Business.Validators
{
    public class DataHolderLegalEntityValidator : AbstractValidator<DataHolderLegalEntityModel>
    {
        public DataHolderLegalEntityValidator()
        {
            // Mandatory Field Validations
            RuleFor(x => x.LegalEntityId).NotEmpty().WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);
            RuleFor(x => x.LegalEntityName).NotEmpty().WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);
            RuleFor(x => x.LogoUri).NotEmpty().WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);
            RuleFor(x => x.Status).NotEmpty().WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);

            // Enum Validations
            RuleFor(x => x.Status).Must(x => Enum.TryParse(x, true, out DhStatusEnum result)).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid);
            RuleFor(x => x.OrganisationType).Must(x => string.IsNullOrEmpty(x) || Enum.TryParse(x.Replace("_", string.Empty), true, out OrganisationTypeEnum result)).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid);

            // Length Validations
            RuleFor(x => x.LegalEntityName).MaximumLength(200).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid);
            RuleFor(x => x.LogoUri).MaximumLength(1000).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid);
            RuleFor(x => x.RegistrationNumber).MaximumLength(100).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid);
            RuleFor(x => x.RegisteredCountry).MaximumLength(100).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid);
            RuleFor(x => x.Abn).MaximumLength(11).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid);
            RuleFor(x => x.Acn).MaximumLength(9).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid);
            RuleFor(x => x.Arbn).MaximumLength(9).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid);
            RuleFor(x => x.AnzsicDivision).MaximumLength(100).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid);
        }
    }
}
