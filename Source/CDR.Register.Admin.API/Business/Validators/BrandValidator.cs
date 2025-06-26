using System;
using System.Collections.Generic;
using System.Linq;
using CDR.Register.Admin.API.Business.Model;
using CDR.Register.Repository.Enums;
using FluentValidation;
using static CDR.Register.Domain.Constants;

namespace CDR.Register.Admin.API.Business.Validators
{
    public class BrandValidator : AbstractValidator<Brand>
    {
        public BrandValidator()
        {
            // mandatory checks
            this.RuleFor(x => x.DataRecipientBrandId).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.BrandName).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.LogoUri).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);
            this.RuleFor(x => x.Status).NotEmpty().WithErrorCode(ErrorCodes.Cds.MissingRequiredField).WithMessage(ErrorTitles.MissingRequiredField);

            // Field lengths
            this.RuleFor(x => x.BrandName).MaximumLength(200).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(b => $"Value '{b.BrandName}' is not allowed for BrandName");
            this.RuleFor(x => x.LogoUri).MaximumLength(1000).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(b => $"Value '{b.LogoUri}' is not allowed for LogoUri");

            // invalid field
            this.RuleFor(x => x.Status).Must(x => Enum.TryParse(x, true, out BrandStatusType _)).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(b => $"Value '{b.Status}' is not allowed for Status");

            this.RuleForEach(x => x.SoftwareProducts).SetValidator(new SoftwareProductValidator());

            // duplicate check
            this.RuleFor(x => x.SoftwareProducts).Must(HaveUniqueIds).WithErrorCode(ErrorCodes.Cds.InvalidField).WithMessage(ErrorTitles.InvalidField).WithState(GetDuplicateId);
        }

        private static bool HaveUniqueIds(ICollection<SoftwareProduct>? softwareProducts)
        {
            return softwareProducts?.Select(s => s.SoftwareProductId).Distinct().Count() == softwareProducts?.Count;
        }

        private static object? GetDuplicateId(Brand brand)
        {
            var distinctIds = new HashSet<Guid>();
            for (var i = 0; i < brand.SoftwareProducts?.Count; i++)
            {
                if (!distinctIds.Add(brand.SoftwareProducts.ElementAt(i).SoftwareProductId))
                {
                    // Duplicate found, return false
                    return $"Duplicate softwareProductId '{brand.SoftwareProducts.ElementAt(i).SoftwareProductId}' is not allowed in the same request";
                }
            }

            return null;
        }
    }
}
