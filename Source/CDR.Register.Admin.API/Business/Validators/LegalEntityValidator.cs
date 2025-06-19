using System;
using System.Collections.Generic;
using System.Linq;
using CDR.Register.Admin.API.Business.Model;
using CDR.Register.Repository.Enums;
using FluentValidation;
using static CDR.Register.Domain.Constants;

namespace CDR.Register.Admin.API.Business.Validators
{
    public class LegalEntityValidator : AbstractValidator<LegalEntity>
    {
        public LegalEntityValidator()
        {
            // check all mandatory fields
            this.RuleFor(x => x.LegalEntityId).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.LegalEntityName).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.AccreditationNumber).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.AccreditationLevel).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.LogoUri).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.Status).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.DataRecipientBrands).Must(x => x != null && x.Count > 0).WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);

            // enum validations
            this.RuleFor(x => x.AccreditationLevel).Must(x => Enum.TryParse(x, true, out AccreditationLevelType _)).WithErrorCode(ErrorCodes.Cds.InvalidField).
                WithMessage(ErrorTitles.InvalidField).
                WithState(le => $"Value '{le.AccreditationLevel}' is not allowed for AccreditationLevel");
            this.RuleFor(x => x.Status).Must(x => Enum.TryParse(x, true, out ParticipationStatusType _)).
                WithErrorCode(ErrorCodes.Cds.InvalidField).
                WithMessage(ErrorTitles.InvalidField).WithState(le => $"Value '{le.Status}' is not allowed for Status");
            this.RuleFor(x => x.OrganisationType).Must(x => x == null || Enum.TryParse(x?.Replace("_", string.Empty), true, out OrganisationTypes _)).
                WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).
                WithState(le => $"Value '{le.OrganisationType}' is not allowed for OrganisationType");

            // field lengths
            this.RuleFor(x => x.LegalEntityName).MaximumLength(200).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(le => $"Value '{le.LegalEntityName}' is not allowed for LegalEntityName");
            this.RuleFor(x => x.AccreditationNumber).MaximumLength(100).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(le => $"Value '{le.AccreditationNumber}' is not allowed for AccreditationNumber");
            this.RuleFor(x => x.AccreditationLevel).MaximumLength(13).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(le => $"Value '{le.AccreditationLevel}' is not allowed for AccreditationLevel");
            this.RuleFor(x => x.LogoUri).MaximumLength(1000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(le => $"Value '{le.LogoUri}' is not allowed for LogoUri");
            this.RuleFor(x => x.RegistrationNumber).MaximumLength(100).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(le => $"Value '{le.RegistrationNumber}' is not allowed for RegistrationNumber");
            this.RuleFor(x => x.RegisteredCountry).MaximumLength(100).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(le => $"Value '{le.RegisteredCountry}' is not allowed for RegisteredCountry");
            this.RuleFor(x => x.Abn).MaximumLength(11).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(le => $"Value '{le.Abn}' is not allowed for Abn");
            this.RuleFor(x => x.Acn).MaximumLength(9).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(le => $"Value '{le.Acn}' is not allowed for Acn");
            this.RuleFor(x => x.Arbn).MaximumLength(9).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(le => $"Value '{le.Arbn}' is not allowed for Arbn");
            this.RuleFor(x => x.AnzsicDivision).MaximumLength(100).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(le => $"Value '{le.AnzsicDivision}' is not allowed for AnzsicDivision");

            this.RuleForEach(x => x.DataRecipientBrands).SetValidator(new BrandValidator());

            this.RuleFor(x => x.DataRecipientBrands).Must(HaveUniqueIds).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(GetDuplicateId);
        }

        private static object? GetDuplicateId(LegalEntity legalEntity)
        {
            var distinctIds = new HashSet<Guid>();
            for (var i = 0; i < legalEntity.DataRecipientBrands?.Count; i++)
            {
                if (!distinctIds.Add(legalEntity.DataRecipientBrands.ElementAt(i).DataRecipientBrandId))
                {
                    // Duplicate found
                    return $"Duplicate DataRecipientBrandId '{legalEntity.DataRecipientBrands.ElementAt(i).DataRecipientBrandId}' is not allowed in the same request";
                }
            }

            return null;
        }

        private static bool HaveUniqueIds(ICollection<Brand>? brands)
        {
            return brands?.Select(b => b.DataRecipientBrandId).Distinct().Count() == brands?.Count;
        }
    }
}
