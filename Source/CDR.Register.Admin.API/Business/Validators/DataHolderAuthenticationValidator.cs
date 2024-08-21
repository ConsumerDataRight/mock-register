using CDR.Register.Admin.API.Business.Model;
using CDR.Register.Domain.Entities;
using FluentValidation;
using System;
using static CDR.Register.Domain.Constants;

namespace CDR.Register.Admin.API.Business.Validators
{
    public class DataHolderAuthenticationValidator : AbstractValidator<DataHolderAuthenticationModel>
    {
        public DataHolderAuthenticationValidator()
        {
            // Mandatory Field Validations
            RuleFor(x => x.RegisterUType).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            RuleFor(x => x.JwksEndpoint).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);

            // Enum Validations
            RuleFor(x => x.RegisterUType).Must(x => Enum.TryParse(x.Replace("-", string.Empty), true, out RegisterUType result)).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField);

            // Length Validations
            RuleFor(x => x.JwksEndpoint).MaximumLength(1000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField);
        }
    }
}
