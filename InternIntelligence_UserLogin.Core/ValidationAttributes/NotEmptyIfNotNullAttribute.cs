using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternIntelligence_UserLogin.Core.ValidationAttributes
{
    public class NotEmptyIfNotNullAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string str && str.Length == 0)
            {
                return new ValidationResult("The field cannot be empty if provided.");
            }

            return ValidationResult.Success;
        }
    }
}
