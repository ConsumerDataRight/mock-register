using System;
using System.ComponentModel.DataAnnotations;
using CDR.Register.Domain.Models;
using Newtonsoft.Json;

namespace CDR.Register.API.Infrastructure.Filters
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class CheckDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext? validationContext)
        {
            if (!DateTime.TryParse(value?.ToString(), out DateTime output))
            {
                return new ValidationResult(JsonConvert.SerializeObject(ResponseErrorList.InvalidDateTime()));
            }

            return ValidationResult.Success;
        }
    }
}
