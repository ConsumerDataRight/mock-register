using CDR.Register.Admin.API.Business.Model;
using CDR.Register.Domain.Entities;
using FluentValidation;
using System;
using static CDR.Register.API.Infrastructure.Constants;

namespace CDR.Register.Admin.API.Business.Validators
{
    public class DataHolderAuthenticationValidator : AbstractValidator<DataHolderAuthenticationModel>
    {
        public DataHolderAuthenticationValidator()
        {
            // Mandatory Field Validations
            RuleFor(x => x.RegisterUType).NotEmpty().WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);
            RuleFor(x => x.JwksEndpoint).NotEmpty().WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);

            // Enum Validations
            RuleFor(x => x.RegisterUType).Must(x => Enum.TryParse(x.Replace("-", string.Empty), true, out RegisterUTypeEnum result)).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid);

            // Length Validations
            RuleFor(x => x.JwksEndpoint).MaximumLength(1000).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid);
        }
    }
}
