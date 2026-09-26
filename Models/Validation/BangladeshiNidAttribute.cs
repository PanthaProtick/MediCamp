using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MediCamp.Models.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class BangladeshiNidAttribute : ValidationAttribute, IClientModelValidator
    {
        private static readonly Regex BdNidRegex = new(@"^(\d{10}|\d{13}|\d{17})$", RegexOptions.Compiled);

        public BangladeshiNidAttribute()
            : base("Please enter a valid Bangladeshi National ID (NID). Must be exactly 10 digits (Smart NID), 13 digits (Old NID), or 17 digits (Old NID with Birth Year).")
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

            var trimmed = stringValue.Trim();

            if (!BdNidRegex.IsMatch(trimmed))
            {
                var displayName = validationContext.DisplayName ?? "National ID (NID)";
                return new ValidationResult(FormatErrorMessage(displayName));
            }

            return ValidationResult.Success;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-bdnid", FormatErrorMessage(context.ModelMetadata.GetDisplayName()));
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
