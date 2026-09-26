using System.Text.RegularExpressions;

namespace MediCamp.Services
{
    public static class BdIdentityValidator
    {
        private static readonly Regex PhoneRegex = new(@"^(?:\+88|88|0088)?(01[3-9]\d{8})$", RegexOptions.Compiled);
        private static readonly Regex NidRegex = new(@"^(\d{10}|\d{13}|\d{17})$", RegexOptions.Compiled);

        /// <summary>
        /// Validates that a string is a valid Bangladeshi mobile phone number (11 digits starting with 013-019, or with +88/88/0088 prefix).
        /// </summary>
        public static bool IsValidPhoneNumber(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            var clean = CleanPhone(phone);
            return PhoneRegex.IsMatch(clean);
        }

        /// <summary>
        /// Normalizes any valid Bangladeshi mobile number to the standard 11-digit domestic format '01XXXXXXXXX'.
        /// </summary>
        public static string NormalizePhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
            var clean = CleanPhone(phone);
            var match = PhoneRegex.Match(clean);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return clean;
        }

        /// <summary>
        /// Validates that a string is a valid Bangladeshi National ID (10, 13, or 17 numeric digits).
        /// </summary>
        public static bool IsValidNid(string? nid)
        {
            if (string.IsNullOrWhiteSpace(nid)) return false;
            return NidRegex.IsMatch(nid.Trim());
        }

        /// <summary>
        /// Normalizes an NID string by trimming whitespace. Returns null if empty.
        /// </summary>
        public static string? NormalizeNid(string? nid)
        {
            return string.IsNullOrWhiteSpace(nid) ? null : nid.Trim();
        }

        private static string CleanPhone(string phone)
        {
            return Regex.Replace(phone.Trim(), @"[\s\-\(\)]", "");
        }
    }
}
