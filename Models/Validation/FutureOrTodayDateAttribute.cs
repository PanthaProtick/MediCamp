using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MediCamp.Models.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class FutureOrTodayDateAttribute : ValidationAttribute, IClientModelValidator
    {
        public FutureOrTodayDateAttribute()
            : base("Camp Start Date must be today or a future date. Past dates are not permitted.")
        {
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is DateTime date)
            {
                // Compare date component against today (UTC)
                var today = DateTime.UtcNow.Date;

                // Also allow local today (in case server is UTC and client is in Bangladesh UTC+6)
                var localToday = DateTime.Today;
                var threshold = today < localToday ? today : localToday;

                if (date.Date < threshold)
                {
                    var displayName = validationContext.DisplayName ?? "Date";
                    return new ValidationResult(FormatErrorMessage(displayName));
                }
            }

            return ValidationResult.Success;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-futuredate", FormatErrorMessage(context.ModelMetadata.GetDisplayName()));
            MergeAttribute(context.Attributes, "min", DateTime.UtcNow.ToString("yyyy-MM-dd"));
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
