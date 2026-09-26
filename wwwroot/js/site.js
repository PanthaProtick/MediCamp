// MediCamp Custom Client-Side Script
// =========================================================================
// BANGLADESHI MOBILE PHONE & NATIONAL ID (NID) VALIDATION ADAPTERS
// =========================================================================
(function ($) {
    function setupValidators() {
        if (typeof $ !== 'undefined' && $.validator) {
            // 1. Bangladeshi Phone Validation (11 digits: 013-019XXXXXXXX, or +88/88/0088 prefix)
            if (!$.validator.methods.bdphone) {
                $.validator.addMethod("bdphone", function (value, element) {
                    if (this.optional(element)) return true;
                    if (!value) return true;
                    var clean = value.replace(/[\s\-\(\)]/g, '');
                    return /^(?:\+88|88|0088)?(01[3-9]\d{8})$/.test(clean);
                }, "Please enter a valid Bangladeshi mobile number (e.g. 017XXXXXXXX, 018XXXXXXXX). Must be 11 digits starting with 013-019.");
            }

            if ($.validator.unobtrusive && $.validator.unobtrusive.adapters) {
                $.validator.unobtrusive.adapters.addBool("bdphone");
            }

            // 2. Bangladeshi NID Validation (Strictly 10, 13, or 17 numeric digits)
            if (!$.validator.methods.bdnid) {
                $.validator.addMethod("bdnid", function (value, element) {
                    if (this.optional(element)) return true;
                    if (!value) return true;
                    var clean = value.trim();
                    return /^\d{10}$|^\d{13}$|^\d{17}$/.test(clean);
                }, "Please enter a valid Bangladeshi National ID (NID). Must be exactly 10 digits (Smart NID), 13 digits (Old NID), or 17 digits (Old NID with Birth Year).");
            }

            if ($.validator.unobtrusive && $.validator.unobtrusive.adapters) {
                $.validator.unobtrusive.adapters.addBool("bdnid");
            }
        }
    }

    if (typeof document !== 'undefined') {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', setupValidators);
        } else {
            setupValidators();
        }
    }
})(typeof jQuery !== 'undefined' ? jQuery : window.jQuery);
