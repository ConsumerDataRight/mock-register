using CDR.Register.Admin.API.Business.Model;
using FluentValidation;
using static CDR.Register.API.Infrastructure.Constants;

namespace CDR.Register.Admin.API.Business.Validators
{
    public class SoftwareProductCertificateValidator : AbstractValidator<SoftwareProductCertificate>
    {
        public SoftwareProductCertificateValidator()
        {
            //mandatory field checks
            RuleFor(x => x.CommonName).NotEmpty().WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);
            RuleFor(x => x.Thumbprint).NotEmpty().WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);

            RuleFor(x => x.CommonName).MaximumLength(2000).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid).WithState(b => $"Value '{b.CommonName}' is not allowed for CommonName");
            RuleFor(x => x.Thumbprint).MaximumLength(2000).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid).WithState(b => $"Value '{b.Thumbprint}' is not allowed for Thumbprint");
        }        
    }
}
