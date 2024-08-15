using CDR.Register.Admin.API.Business.Model;
using FluentValidation;
using static CDR.Register.Domain.Constants;

namespace CDR.Register.Admin.API.Business.Validators
{
    public class SoftwareProductCertificateValidator : AbstractValidator<SoftwareProductCertificate>
    {
        public SoftwareProductCertificateValidator()
        {
            //mandatory field checks
            RuleFor(x => x.CommonName).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            RuleFor(x => x.Thumbprint).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);

            RuleFor(x => x.CommonName).MaximumLength(2000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(b => $"Value '{b.CommonName}' is not allowed for CommonName");
            RuleFor(x => x.Thumbprint).MaximumLength(2000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(b => $"Value '{b.Thumbprint}' is not allowed for Thumbprint");
        }        
    }
}
