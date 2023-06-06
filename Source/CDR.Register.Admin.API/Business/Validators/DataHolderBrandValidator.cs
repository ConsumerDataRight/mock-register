﻿using CDR.Register.Admin.API.Business.Model;
using CDR.Register.Domain.Entities;
using FluentValidation;
using System;
using System.Linq;
using static CDR.Register.API.Infrastructure.Constants;

namespace CDR.Register.Admin.API.Business.Validators
{
    public class DataHolderBrandValidator : AbstractValidator<DataHolderBrandModel>
    {
        public DataHolderBrandValidator()
        {
            // Mandatory Field Validations
            RuleFor(x => x.DataHolderBrandId).NotEmpty().WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);
            RuleFor(x => x.BrandName).NotEmpty().WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);
            RuleFor(x => x.Industries).Must(x => x!= null && x.Length > 0).WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);
            RuleFor(x => x.LogoUri).NotEmpty().WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);
            RuleFor(x => x.Status).NotEmpty().WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);
            RuleFor(x => x.LegalEntity).Must(x => x!= null).WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);
            RuleFor(x => x.EndpointDetail).Must(x => x!= null).WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);
            RuleFor(x => x.AuthDetails).Must(x => x!= null).WithErrorCode(ErrorCodes.FieldMissing).WithMessage(ErrorTitles.FieldMissing);

            // Enum Validations
            RuleForEach(x => x.Industries).Must(x => Enum.TryParse(x, true, out IndustryEnum result)).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid);
            RuleFor(x => x.Status).Must(x => Enum.TryParse(x, true, out DhParticipationStatusEnum result)).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid);

            // Length Validations
            RuleFor(x => x.BrandName).MaximumLength(200).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid);
            RuleFor(x => x.LogoUri).MaximumLength(1000).WithErrorCode(ErrorCodes.FieldInvalid).WithMessage(ErrorTitles.FieldInvalid);

            // Child Validation
            RuleFor(x => x.EndpointDetail).SetValidator(new DataHolderEndpointValidator());
            RuleFor(x => x.AuthDetails).SetValidator(new DataHolderAuthenticationValidator());
            RuleFor(x => x.LegalEntity).SetValidator(new DataHolderLegalEntityValidator());
        }
    }
}
