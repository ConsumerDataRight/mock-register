using CDR.Register.Domain.Models;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace CDR.Register.API.Infrastructure.Filters
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class CheckPageSizeAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext? validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (!int.TryParse(value.ToString(), out int pageSize) || pageSize <= 0)
            {
                return new ValidationResult(JsonConvert.SerializeObject(ResponseErrorList.InvalidPageSize()));
            }

            if (pageSize > 1000)
            {
                return new ValidationResult(JsonConvert.SerializeObject(ResponseErrorList.PageSizeTooLarge()));
            }

            return ValidationResult.Success;
        }
    }
}
