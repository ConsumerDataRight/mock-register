using System;
using CDR.Register.Admin.API.Business.Model;
using CDR.Register.Domain.Enums;
using FluentValidation;
using static CDR.Register.Domain.Constants;

namespace CDR.Register.Admin.API.Business.Validators
{
    public class DataHolderAuthenticationValidator : AbstractValidator<DataHolderAuthenticationModel>
    {
        public DataHolderAuthenticationValidator()
        {
            // Mandatory Field Validations
            this.RuleFor(x => x.RegisterUType).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.JwksEndpoint).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);

            // Enum Validations
            this.RuleFor(x => x.RegisterUType).Must(x => Enum.TryParse(x.Replace("-", string.Empty), true, out RegisterUType _)).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField);

            // Length Validations
            this.RuleFor(x => x.JwksEndpoint).MaximumLength(1000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField);
        }
    }
}
