using CDR.Register.Admin.API.Business.Model;
using CDR.Register.Domain.Entities;
using FluentValidation;
using System;
using static CDR.Register.Domain.Constants;

namespace CDR.Register.Admin.API.Business.Validators
{
    public class DataHolderBrandValidator : AbstractValidator<DataHolderBrandModel>
    {
        public DataHolderBrandValidator()
        {
            // Mandatory Field Validations
            RuleFor(x => x.DataHolderBrandId).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            RuleFor(x => x.BrandName).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            RuleFor(x => x.Industries).Must(x => x != null && x.Length > 0).WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            RuleFor(x => x.LogoUri).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            RuleFor(x => x.Status).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            RuleFor(x => x.LegalEntity).Must(x => x != null).WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            RuleFor(x => x.EndpointDetail).Must(x => x != null).WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            RuleFor(x => x.AuthDetails).Must(x => x != null).WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);

            // Enum Validations
            RuleForEach(x => x.Industries).Must(x => Enum.TryParse(x, true, out Industry result)).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField);
            RuleFor(x => x.Status).Must(x => Enum.TryParse(x, true, out DhParticipationStatus result)).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField);

            // Length Validations
            RuleFor(x => x.BrandName).MaximumLength(200).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField);
            RuleFor(x => x.LogoUri).MaximumLength(1000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField);

            // Child Validation
            RuleFor(x => x.EndpointDetail).SetValidator(new DataHolderEndpointValidator());
            RuleFor(x => x.AuthDetails).SetValidator(new DataHolderAuthenticationValidator());
            RuleFor(x => x.LegalEntity).SetValidator(new DataHolderLegalEntityValidator());
        }
    }
}
