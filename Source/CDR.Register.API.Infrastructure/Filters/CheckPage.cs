using System;
using System.ComponentModel.DataAnnotations;
using CDR.Register.Domain.Models;
using Newtonsoft.Json;

namespace CDR.Register.API.Infrastructure.Filters
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class CheckPageAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext? validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (!int.TryParse(value.ToString(), out int page) || page <= 0)
            {
                return new ValidationResult(JsonConvert.SerializeObject(ResponseErrorList.InvalidPage()));
            }

            return ValidationResult.Success;
        }
    }
}
