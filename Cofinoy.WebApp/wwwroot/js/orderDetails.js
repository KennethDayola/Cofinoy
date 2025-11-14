function displayToast(message, type = 'success') {
    const typeMap = {
        'success': 'success',
        'error': 'danger',
        'warning': 'warning'
    };

    const bootstrapType = typeMap[type] || 'info';

    if (typeof showToast === 'function') {
        showToast(message, bootstrapType, 'Cofinoy');
    } else {
        console.error('Bootstrap toast function not available');
        alert(message); 
    }
}

// ✅ Validate Pending → Brewing
function validatePending(orderId) {
    fetch(`${window.location.origin}/Order/UpdateOrderStatus`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ orderId: orderId, newStatus: 'Brewing' })
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                displayToast(' Order validated and now Brewing', 'success');
                setTimeout(() => window.location.reload(), 1000);
            } else {
                displayToast('⚠️ ' + data.error, 'error');
            }
        })
        .catch(err => {
            console.error('Error:', err);
            displayToast('An unexpected error occurred', 'error');
        });
}

// ✅ Mark as Brewing → Ready
function markAsReady(orderId) {
    fetch(`${window.location.origin}/Order/UpdateOrderStatus`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ orderId: orderId, newStatus: 'Ready' })
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                displayToast(' Order marked as Ready', 'success');
                setTimeout(() => window.location.reload(), 1000);
            } else {
                displayToast('⚠️ ' + data.error, 'error');
            }
        })
        .catch(err => {
            console.error('Error:', err);
            displayToast('An unexpected error occurred', 'error');
        });
}

// ✅ Mark as Ready → Serving
function markAsServing(orderId) {
    fetch(`${window.location.origin}/Order/UpdateOrderStatus`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ orderId: orderId, newStatus: 'Serving' })
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                displayToast(' Order marked as Serving', 'success');
                setTimeout(() => window.location.reload(), 1000);
            } else {
                displayToast('⚠️ ' + data.error, 'error');
            }
        })
        .catch(err => {
            console.error('Error:', err);
            displayToast('An unexpected error occurred', 'error');
        });
}

// ✅ Mark as Serving → Served
function markAsServed(orderId) {
    fetch(`${window.location.origin}/Order/UpdateOrderStatus`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ orderId: orderId, newStatus: 'Served' })
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                displayToast(' Order marked as Served', 'success');
                setTimeout(() => window.location.reload(), 1000);
            } else {
                displayToast('⚠️ ' + data.error, 'error');
            }
        })
        .catch(err => {
            console.error('Error:', err);
            displayToast('An unexpected error occurred', 'error');
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
                displayToast('❌ Order cancelled successfully', 'warning');
                setTimeout(() => window.location.reload(), 1000);
            } else {
                displayToast('⚠️ ' + data.error, 'error');
            }
        })
        .catch(err => {
            console.error('Error:', err);
            displayToast('An unexpected error occurred', 'error');
        });
}