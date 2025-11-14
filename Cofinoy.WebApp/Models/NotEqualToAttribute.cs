using System;
using System.ComponentModel.DataAnnotations;

namespace Cofinoy.WebApp.Models
{
    public class NotEqualToAttribute : ValidationAttribute
    {
        private readonly string _otherPropertyName;

        public NotEqualToAttribute(string otherPropertyName)
        {
            _otherPropertyName = otherPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var otherProperty = validationContext.ObjectType.GetProperty(_otherPropertyName);
            if (otherProperty == null)
                throw new ArgumentException("Property with this name not found");

            var otherValue = otherProperty.GetValue(validationContext.ObjectInstance)?.ToString();
            var thisValue = value?.ToString();

            if (!string.IsNullOrEmpty(thisValue) && thisValue == otherValue)
                return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} should not be the same as {_otherPropertyName}");

            return ValidationResult.Success;
        }
    }
}
