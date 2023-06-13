using CDR.Register.Admin.API.Business.Model;
using FluentValidation;
using System;
using static CDR.Register.API.Infrastructure.Constants;

namespace CDR.Register.Admin.API.Business.Validators
{
    public class DataHolderEndpointValidator : AbstractValidator<DataHolderEndpointModel>
    {
        public DataHolderEndpointValidator()
        {
            // Mandatory Field Validations
            RuleFor(x => x.Version).NotEmpty().WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);
            RuleFor(x => x.PublicBaseUri).NotEmpty().WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);
            RuleFor(x => x.ResourceBaseUri).NotEmpty().WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);
            RuleFor(x => x.InfosecBaseUri).NotEmpty().WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);
            RuleFor(x => x.WebsiteUri).NotEmpty().WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);

            // Length Validations
            RuleFor(x => x.Version).MaximumLength(25).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid);
            RuleFor(x => x.PublicBaseUri).MaximumLength(1000).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid);
            RuleFor(x => x.ResourceBaseUri).MaximumLength(1000).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid);
            RuleFor(x => x.InfosecBaseUri).MaximumLength(1000).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid);
            RuleFor(x => x.ExtensionBaseUri).MaximumLength(1000).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid);
            RuleFor(x => x.WebsiteUri).MaximumLength(1000).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid);
        }
    }
}
