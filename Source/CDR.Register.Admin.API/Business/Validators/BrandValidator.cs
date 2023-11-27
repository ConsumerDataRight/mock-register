﻿using CDR.Register.Admin.API.Business.Model;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using static CDR.Register.API.Infrastructure.Constants;

namespace CDR.Register.Admin.API.Business.Validators
{
    public class BrandValidator : AbstractValidator<Brand>
    {
        public BrandValidator()
        {
            //mandatory checks
            RuleFor(x => x.DataRecipientBrandId).NotEmpty().WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);
            RuleFor(x => x.BrandName).NotEmpty().WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);
            RuleFor(x => x.LogoUri).NotEmpty().WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);
            RuleFor(x => x.Status).NotEmpty().WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);

            //Field lengths            
            RuleFor(x => x.BrandName).MaximumLength(200).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid).WithState(b => $"Value '{b.BrandName}' is not allowed for BrandName");
            RuleFor(x => x.LogoUri).MaximumLength(1000).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid).WithState(b => $"Value '{b.LogoUri}' is not allowed for LogoUri");
            
            //invalid field
            RuleFor(x => x.Status).Must(x => Enum.TryParse(x, true, out Repository.Entities.BrandStatusType result)).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid).WithState(b => $"Value '{b.Status}' is not allowed for Status");
            
            RuleForEach(x => x.SoftwareProducts).SetValidator(new SoftwareProductValidator());

            //duplicate check
            RuleFor(x => x.SoftwareProducts).Must(HaveUniqueIds).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid).WithState(GetDuplicateId);
        }


        private bool HaveUniqueIds(ICollection<SoftwareProduct>? softwareProducts)
        {
            return softwareProducts?.Select(s => s.SoftwareProductId).Distinct().Count() == softwareProducts?.Count;
        }

        private object GetDuplicateId(Brand brand)
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
