// QSS Platform - Core JS

// Utility: get JWT from cookie/session
function getToken() {
    return document.cookie.split(';')
        .find(c => c.trim().startsWith('qss_token='))
        ?.split('=')[1] ?? '';
}

// API helper — reads window.API_BASE at call time so it always reflects the
// value injected by _Layout.cshtml, never a stale module-level capture.
// Contract: callers pass paths like '/users', '/tasks', etc. (without /api prefix).
// Paths that already start with '/api' are used as-is to avoid double-prefixing.
async function apiRequest(method, path, body = null) {
    const base = window.API_BASE || window.location.origin.replace('5001', '5000');
    const normalizedPath = path.startsWith('/api')
        ? path
        : '/api' + (path.startsWith('/') ? path : '/' + path);
    const finalUrl = `${base}${normalizedPath}`;
    console.log('apiRequest final url', finalUrl);
    const token = getToken();
    const opts = {
        method,
        headers: {
            'Content-Type': 'application/json',
            ...(token ? { 'Authorization': `Bearer ${token}` } : {})
        }
    };
    if (body) opts.body = JSON.stringify(body);
    const res = await fetch(finalUrl, opts);
    if (!res.ok) {
        const err = await res.text();
        throw new Error(err || `HTTP ${res.status}`);
    }
    if (res.status === 204) return null;
    return res.json();
}

// Notifications panel Alpine component
function notificationsPanel() {
    return {
        open: false,
        notifications: [],
        unreadCount: 0,
        async toggle() {
            this.open = !this.open;
            if (this.open) await this.load();
        },
        async load() {
            try {
                this.notifications = await apiRequest('GET', '/chat/notifications');
                this.unreadCount = this.notifications.filter(n => !n.isRead).length;
            } catch (e) { console.warn('Failed to load notifications', e); }
        },
        async markRead(id) {
            try {
                await apiRequest('PATCH', `/chat/notifications/${id}/read`);
                const n = this.notifications.find(x => x.id === id);
                if (n) { n.isRead = true; this.unreadCount = Math.max(0, this.unreadCount - 1); }
            } catch (e) {}
        }
    };
}

// Toast notification utility
function showToast(message, type = 'success') {
    const colors = { success: 'bg-green-500', error: 'bg-red-500', warning: 'bg-yellow-500', info: 'bg-blue-500' };
    const toast = document.createElement('div');
    toast.className = `fixed bottom-4 right-4 z-50 ${colors[type]} text-white px-4 py-3 rounded-lg shadow-lg text-sm font-medium transform transition-all duration-300`;
    toast.textContent = message;
    document.body.appendChild(toast);
    setTimeout(() => { toast.style.opacity = '0'; setTimeout(() => toast.remove(), 300); }, 3000);
}

// Format date utility
function formatDate(dateStr) {
    if (!dateStr) return '—';
    const d = new Date(dateStr);
    return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
}

function formatDateTime(dateStr) {
    if (!dateStr) return '—';
    const d = new Date(dateStr);
    return d.toLocaleString('en-GB', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' });
}

// Status badge
function statusBadge(status) {
    const map = {
        'Open': 'badge-open',
        'InProgress': 'badge-inprogress',
        'Completed': 'badge-completed',
        'Overdue': 'badge-overdue'
    };
    return map[status] || 'bg-gray-100 text-gray-700';
}
