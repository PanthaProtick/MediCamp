/**
 * MediCamp Real-Time Interactive Notification Center
 */
(function () {
    let notifCache = null;
    let isLoading = false;

    function getAntiForgeryToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }

    async function fetchNotifications() {
        if (isLoading) return;
        isLoading = true;
        try {
            const response = await fetch('/Notification/GetNotifications');
            if (response.ok) {
                const data = await response.json();
                notifCache = data;
                updateBadge(data.unreadCount);
                renderNotificationList(data.notifications);
            }
        } catch (e) {
            console.error('Error fetching notifications:', e);
        } finally {
            isLoading = false;
        }
    }

    function updateBadge(count) {
        document.querySelectorAll('.notification-badge-pulse').forEach(badge => {
            if (count > 0) {
                badge.textContent = count > 99 ? '99+' : count;
                badge.style.display = 'inline-block';
            } else {
                badge.style.display = 'none';
            }
        });
    }

    function renderNotificationList(items) {
        document.querySelectorAll('.notification-list').forEach(list => {
            if (!items || items.length === 0) {
                list.innerHTML = `
                    <li class="text-center py-4 px-3 text-muted">
                        <i class="fa-regular fa-bell-slash fs-2 d-block mb-2 text-secondary opacity-50"></i>
                        <span class="small fw-semibold">No notifications right now</span>
                        <p class="text-muted mb-0" style="font-size: 0.72rem;">You're all caught up with recent camp activities!</p>
                    </li>
                `;
                return;
            }

            list.innerHTML = items.map(item => {
                const isPending = item.status === 'Pending';
                let actionButtonsHtml = '';

                if (item.canApproveReject) {
                    actionButtonsHtml = `
                        <div class="d-flex align-items-center gap-2 mt-2 notif-action-btns" data-req-id="${item.requestId}" data-req-type="${item.type}">
                            <button type="button" class="btn btn-sm btn-success py-0 px-2 fw-bold text-white rounded-pill btn-approve-notif" style="font-size: 0.72rem;">
                                <i class="fa-solid fa-check me-1"></i> Approve
                            </button>
                            <button type="button" class="btn btn-sm btn-outline-danger py-0 px-2 fw-bold rounded-pill btn-deny-notif" style="font-size: 0.72rem;">
                                <i class="fa-solid fa-xmark me-1"></i> Deny
                            </button>
                            <a href="${item.actionUrl}" class="ms-auto text-muted small text-decoration-none" style="font-size: 0.7rem;">
                                Details <i class="fa-solid fa-chevron-right" style="font-size: 0.6rem;"></i>
                            </a>
                        </div>
                    `;
                }

                return `
                    <li class="notification-item ${item.isRead ? '' : 'unread'}" id="notif-item-${item.id}">
                        <div class="notification-icon-box ${item.badgeColor} text-white">
                            <i class="${item.iconClass}"></i>
                        </div>
                        <div class="notification-content">
                            <div class="d-flex justify-content-between align-items-start">
                                <span class="notification-title">${item.title}</span>
                                <span class="text-muted ms-2" style="font-size: 0.68rem; white-space: nowrap;">${item.timeAgo}</span>
                            </div>
                            <div class="notification-msg">${item.message}</div>
                            ${actionButtonsHtml}
                        </div>
                    </li>
                `;
            }).join('');

            // Attach Approve / Deny click handlers
            list.querySelectorAll('.btn-approve-notif').forEach(btn => {
                btn.addEventListener('click', (e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    const wrapper = btn.closest('.notif-action-btns');
                    const reqId = wrapper.dataset.reqId;
                    const reqType = wrapper.dataset.reqType;
                    respondToRequest(reqId, reqType, 'Approved', wrapper);
                });
            });

            list.querySelectorAll('.btn-deny-notif').forEach(btn => {
                btn.addEventListener('click', (e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    const wrapper = btn.closest('.notif-action-btns');
                    const reqId = wrapper.dataset.reqId;
                    const reqType = wrapper.dataset.reqType;
                    respondToRequest(reqId, reqType, 'Denied', wrapper);
                });
            });
        });
    }

    async function respondToRequest(requestId, requestType, status, containerElement) {
        let endpoint = '/Notification/RespondToStaffRequest';
        if (requestType === 'VolunteerRequest') endpoint = '/Notification/RespondToVolunteerRequest';
        if (requestType === 'PharmacistRequest') endpoint = '/Notification/RespondToPharmacistRequest';

        const token = getAntiForgeryToken();
        const formData = new FormData();
        formData.append('requestId', requestId);
        formData.append('status', status);
        if (token) formData.append('__RequestVerificationToken', token);

        containerElement.innerHTML = `<span class="spinner-border spinner-border-sm text-secondary" role="status"></span> <span class="small text-muted">Processing...</span>`;

        try {
            const response = await fetch(endpoint, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': token
                },
                body: formData
            });

            const resData = await response.json();
            if (resData.success) {
                const badgeColor = status === 'Approved' ? 'bg-success' : 'bg-danger';
                containerElement.innerHTML = `
                    <span class="badge ${badgeColor} px-2 py-1 small rounded-pill">
                        <i class="fa-solid ${status === 'Approved' ? 'fa-circle-check' : 'fa-circle-xmark'} me-1"></i> ${status}
                    </span>
                `;
                // Refresh list in background
                setTimeout(fetchNotifications, 1200);
            } else {
                containerElement.innerHTML = `<span class="badge bg-warning text-dark small">${resData.message || 'Action failed'}</span>`;
            }
        } catch (err) {
            console.error('Error updating request status:', err);
            containerElement.innerHTML = `<span class="badge bg-danger small">Network Error</span>`;
        }
    }

    function toggleNotificationDropdown(panel) {
        const isOpen = panel.classList.contains('show');
        // Close all open panels first
        document.querySelectorAll('.notification-dropdown-panel.show').forEach(p => p.classList.remove('show'));

        if (!isOpen) {
            panel.classList.add('show');
            fetchNotifications();
        }
    }

    // Global document ready listener
    document.addEventListener('DOMContentLoaded', () => {
        // Initial fetch to populate badge count
        fetchNotifications();

        // Attach click triggers to all bell buttons
        document.querySelectorAll('.notification-bell-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.stopPropagation();
                const wrapper = btn.closest('.notification-bell-wrapper');
                if (wrapper) {
                    const panel = wrapper.querySelector('.notification-dropdown-panel');
                    if (panel) toggleNotificationDropdown(panel);
                }
            });
        });

        // Auto-refresh notifications every 15 seconds in background
        setInterval(fetchNotifications, 15000);

        // Refresh when window gains focus or tab becomes visible
        window.addEventListener('focus', fetchNotifications);
        document.addEventListener('visibilitychange', () => {
            if (document.visibilityState === 'visible') {
                fetchNotifications();
            }
        });

        // Close dropdown when clicking outside
        document.addEventListener('click', (e) => {
            if (!e.target.closest('.notification-bell-wrapper')) {
                document.querySelectorAll('.notification-dropdown-panel.show').forEach(panel => {
                    panel.classList.remove('show');
                });
            }
        });
    });
})();

