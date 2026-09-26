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

            // 3. Future Or Today Date Validation
            if (!$.validator.methods.futuredate) {
                $.validator.addMethod("futuredate", function (value, element) {
                    if (this.optional(element)) return true;
                    if (!value) return true;
                    var selected = new Date(value);
                    selected.setHours(0, 0, 0, 0);
                    var today = new Date();
                    today.setHours(0, 0, 0, 0);
                    return selected >= today;
                }, "Camp Start Date must be today or a future date. Past dates are not permitted.");
            }

            if ($.validator.unobtrusive && $.validator.unobtrusive.adapters) {
                $.validator.unobtrusive.adapters.addBool("futuredate");
            }
        }
    }

    // =========================================================================
    // ADMIN COLLAPSIBLE SIDEBAR LOGIC (With localStorage persistence)
    // =========================================================================
    function initAdminSidebar() {
        var toggleBtn = document.getElementById('adminSidebarToggle');
        var mobileToggleBtn = document.getElementById('adminMobileSidebarToggle');
        var layoutCard = document.getElementById('adminLayoutCard');

        // Apply saved preference immediately
        try {
            var isCollapsed = localStorage.getItem('admin_sidebar_collapsed') === 'true';
            if (isCollapsed) {
                document.documentElement.classList.add('admin-sidebar-collapsed');
                if (layoutCard) layoutCard.classList.add('admin-sidebar-collapsed');
            }
        } catch (e) {}

        function toggleSidebar() {
            var isNowCollapsed = document.documentElement.classList.toggle('admin-sidebar-collapsed');
            if (layoutCard) {
                layoutCard.classList.toggle('admin-sidebar-collapsed', isNowCollapsed);
            }
            try {
                localStorage.setItem('admin_sidebar_collapsed', isNowCollapsed ? 'true' : 'false');
            } catch (e) {}

            // Re-render / update tooltips
            if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
                var tooltips = document.querySelectorAll('[data-bs-toggle="tooltip"]');
                tooltips.forEach(function (el) {
                    var instance = bootstrap.Tooltip.getInstance(el);
                    if (instance) {
                        instance.hide();
                    }
                });
            }
        }

        if (toggleBtn) {
            toggleBtn.removeEventListener('click', toggleSidebar);
            toggleBtn.addEventListener('click', function (e) {
                e.preventDefault();
                toggleSidebar();
            });
        }

        if (mobileToggleBtn) {
            mobileToggleBtn.removeEventListener('click', toggleSidebar);
            mobileToggleBtn.addEventListener('click', function (e) {
                e.preventDefault();
                toggleSidebar();
            });
        }

        // Initialize all Bootstrap tooltips on the page
        if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
            var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
            tooltipTriggerList.forEach(function (tooltipTriggerEl) {
                if (!bootstrap.Tooltip.getInstance(tooltipTriggerEl)) {
                    new bootstrap.Tooltip(tooltipTriggerEl, {
                        trigger: 'hover',
                        container: 'body'
                    });
                }
            });
        }
    }

    if (typeof document !== 'undefined') {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', function () {
                setupValidators();
                initAdminSidebar();
            });
        } else {
            setupValidators();
            initAdminSidebar();
        }
    }
})(typeof jQuery !== 'undefined' ? jQuery : window.jQuery);

