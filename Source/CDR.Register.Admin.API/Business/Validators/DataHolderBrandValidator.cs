using System;
using CDR.Register.Admin.API.Business.Model;
using CDR.Register.Domain.Enums;
using FluentValidation;
using static CDR.Register.Domain.Constants;

namespace CDR.Register.Admin.API.Business.Validators
{
    public class DataHolderBrandValidator : AbstractValidator<DataHolderBrandModel>
    {
        public DataHolderBrandValidator()
        {
            // Mandatory Field Validations
            this.RuleFor(x => x.DataHolderBrandId).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.BrandName).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.Industries).Must(x => x != null && x.Length > 0).WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.LogoUri).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.Status).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.LegalEntity).Must(x => x != null).WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.EndpointDetail).Must(x => x != null).WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.AuthDetails).Must(x => x != null).WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);

            // Enum Validations
            this.RuleForEach(x => x.Industries).Must(x => Enum.TryParse(x, true, out Industry _)).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField);
            this.RuleFor(x => x.Status).Must(x => Enum.TryParse(x, true, out DhParticipationStatus _)).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField);

            // Length Validations
            this.RuleFor(x => x.BrandName).MaximumLength(200).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField);
            this.RuleFor(x => x.LogoUri).MaximumLength(1000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField);

            // Child Validation
            this.When(x => x != null, () =>
            {
                this.RuleFor(x => x.EndpointDetail!).SetValidator(new DataHolderEndpointValidator());
            });
            this.When(x => x != null, () =>
            {
                this.RuleFor(x => x.AuthDetails!).SetValidator(new DataHolderAuthenticationValidator());
            });
            this.When(x => x != null, () =>
            {
                this.RuleFor(x => x.LegalEntity!).SetValidator(new DataHolderLegalEntityValidator());
            });
        }
    }
}
