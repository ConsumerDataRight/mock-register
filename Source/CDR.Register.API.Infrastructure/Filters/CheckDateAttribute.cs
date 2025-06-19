using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using CDR.Register.Domain.Models;
using Newtonsoft.Json;

namespace CDR.Register.API.Infrastructure.Filters
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class CheckDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext? validationContext)
        {
            if (!DateTime.TryParse(value?.ToString(), CultureInfo.InvariantCulture, out _))
            {
                return new ValidationResult(JsonConvert.SerializeObject(ResponseErrorList.InvalidDateTime()));
            }

            return ValidationResult.Success;
        }
    }
}
