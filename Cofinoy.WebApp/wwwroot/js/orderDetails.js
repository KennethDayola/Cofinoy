// ✅ Toast Popup Helper
function showToast(message, type = 'success') {
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.textContent = message;
    document.body.appendChild(toast);

    setTimeout(() => toast.classList.add('show'), 100);
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 300);
    }, 2000);
}

// ✅ Mark as Ready
function markAsReady(orderId) {
    fetch(`${window.location.origin}/Order/UpdateOrderStatus`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ orderId: orderId, newStatus: 'Ready' })
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                showToast('✅ Order marked as Ready', 'success');
                setTimeout(() => window.location.href = '/Order/OrderManagement', 1800);
            } else {
                showToast('⚠️ ' + data.error, 'error');
            }
        })
        .catch(err => {
            console.error('Error:', err);
            showToast('An unexpected error occurred', 'error');
        });
}

// ✅ Cancel Order
function cancelOrder(orderId) {
    if (!confirm('Are you sure you want to cancel this order?')) return;

    fetch(`${window.location.origin}/Order/CancelOrder`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ orderId: orderId })
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                showToast('❌ Order cancelled successfully', 'warning');
                setTimeout(() => window.location.href = '/Order/OrderManagement', 1800);
            } else {
                showToast('⚠️ ' + data.error, 'error');
            }
        })
        .catch(err => {
            console.error('Error:', err);
            showToast('An unexpected error occurred', 'error');
        });
}

// ✅ Mark as Served
function markAsServed(orderId) {
    fetch(`${window.location.origin}/Order/UpdateOrderStatus`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ orderId: orderId, newStatus: 'Served' })
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                showToast('✅ Order marked as Served', 'success');
                setTimeout(() => window.location.href = '/Order/OrderManagement', 1800);
            } else {
                showToast('⚠️ ' + data.error, 'error');
            }
        })
        .catch(err => {
            console.error('Error:', err);
            showToast('An unexpected error occurred', 'error');
        });
}

// ✅ Mark as Serving
function markAsServing(orderId) {
    fetch(`${window.location.origin}/Order/UpdateOrderStatus`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ orderId: orderId, newStatus: 'Serving' })
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                showToast('✅ Order marked as Serving', 'success');
                setTimeout(() => window.location.reload(), 1800);
            } else {
                showToast('⚠️ ' + data.error, 'error');
            }
        })
        .catch(err => {
            console.error('Error:', err);
            showToast('An unexpected error occurred', 'error');
        });
}