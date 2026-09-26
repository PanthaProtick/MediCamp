using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MediCamp.Models.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class BangladeshiPhoneNumberAttribute : ValidationAttribute, IClientModelValidator
    {
        private static readonly Regex BdPhoneRegex = new(@"^(?:\+88|88|0088)?(01[3-9]\d{8})$", RegexOptions.Compiled);

        public BangladeshiPhoneNumberAttribute()
            : base("Please enter a valid Bangladeshi mobile number (e.g. 017XXXXXXXX, 018XXXXXXXX). Must be 11 digits starting with 013-019.")
        {
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            var stringValue = value as string;
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return ValidationResult.Success;
            }

            // Remove spaces, hyphens, and brackets
            var cleaned = Regex.Replace(stringValue.Trim(), @"[\s\-\(\)]", "");

            if (!BdPhoneRegex.IsMatch(cleaned))
            {
                var displayName = validationContext.DisplayName ?? "Phone Number";
                return new ValidationResult(FormatErrorMessage(displayName));
            }

            return ValidationResult.Success;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-bdphone", FormatErrorMessage(context.ModelMetadata.GetDisplayName()));
        }

        private static void MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (!attributes.ContainsKey(key))
            {
                attributes.Add(key, value);
            }
        }
    }
}
