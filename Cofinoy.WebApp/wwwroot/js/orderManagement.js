document.addEventListener('DOMContentLoaded', function () {
    loadOrders();
    setupEventListeners();
    setupFilterModal();
    startAutoRefresh();
});

let autoRefreshInterval;
let currentFilters = {
    status: 'All',
    dateFrom: null,
    dateTo: null
};

function setupEventListeners() {
    document.getElementById('searchOrders').addEventListener('keyup', function () {
        loadOrders();
    });

    document.querySelector('.btn-filter').addEventListener('click', function (e) {
        e.stopPropagation();
        openFilterModal();
    });
}

function setupFilterModal() {
    // Status filter buttons
    document.querySelectorAll('[data-status]').forEach(btn => {
        btn.addEventListener('click', function() {
            document.querySelectorAll('[data-status]').forEach(b => b.classList.remove('active'));
            this.classList.add('active');
        });
    });

    // Close modal on overlay click
    document.getElementById('filterModal').addEventListener('click', function(e) {
        if (e.target === this) {
            closeFilterModal();
        }
    });
}

function openFilterModal() {
    document.getElementById('filterModal').classList.add('active');
    document.body.style.overflow = 'hidden';
}

function closeFilterModal() {
    document.getElementById('filterModal').classList.remove('active');
    document.body.style.overflow = '';
}

function clearFilters() {
    currentFilters = {
        status: 'All',
        dateFrom: null,
        dateTo: null
    };
    
    // Reset UI
    document.querySelectorAll('[data-status]').forEach(b => b.classList.remove('active'));
    document.querySelector('[data-status="All"]').classList.add('active');
    
    document.getElementById('filterDateFrom').value = '';
    document.getElementById('filterDateTo').value = '';
    
    document.querySelector('.btn-filter').innerHTML = '<i class="fas fa-filter"></i> Filters';
    
    loadOrders();
    closeFilterModal();
}

function applyFilters() {
    const selectedStatus = document.querySelector('[data-status].active').dataset.status;
    const dateFrom = document.getElementById('filterDateFrom').value;
    const dateTo = document.getElementById('filterDateTo').value;
    
    currentFilters = {
        status: selectedStatus,
        dateFrom: dateFrom || null,
        dateTo: dateTo || null
    };
    
    // Update filter button text
    let filterText = 'Filters';
    if (selectedStatus !== 'All') {
        filterText = selectedStatus;
    }
    document.querySelector('.btn-filter').innerHTML = `<i class="fas fa-filter"></i> ${filterText}`;
    
    loadOrders();
    closeFilterModal();
}

function startAutoRefresh() {
    autoRefreshInterval = setInterval(() => {
        loadOrders(true);
    }, 5000);
}

window.addEventListener('beforeunload', () => {
    if (autoRefreshInterval) {
        clearInterval(autoRefreshInterval);
    }
});

function loadOrders(silentRefresh = false) {
    const search = document.getElementById('searchOrders').value || '';
    const status = currentFilters.status !== 'All' ? currentFilters.status : '';

    fetch(`${window.location.origin}/Order/GetAllOrders?status=${encodeURIComponent(status)}&searchTerm=${encodeURIComponent(search)}`)
        .then(response => {
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            return response.json();
        })
        .then(data => {
            if (data.success) {
                let filteredOrders = data.data;
                
                // Apply date range filter
                if (currentFilters.dateFrom) {
                    const fromDate = new Date(currentFilters.dateFrom);
                    filteredOrders = filteredOrders.filter(order => 
                        new Date(order.orderDate) >= fromDate
                    );
                }
                
                if (currentFilters.dateTo) {
                    const toDate = new Date(currentFilters.dateTo);
                    toDate.setHours(23, 59, 59, 999); // Include entire day
                    filteredOrders = filteredOrders.filter(order => 
                        new Date(order.orderDate) <= toDate
                    );
                }
                
                displayOrders(filteredOrders);
                updateOrderCount(filteredOrders.length, currentFilters.status);
            } else {
                console.error('Error loading orders:', data.error);
            }
        })
        .catch(error => {
            if (!silentRefresh) {
                console.error('Fetch error:', error);
            }
        });
}

function displayOrders(orders) {
    const orderList = document.getElementById('orderList');
    orderList.innerHTML = '';

    if (!orders || orders.length === 0) {
        orderList.innerHTML = '<tr><td colspan="7" style="text-align:center; padding: 20px;">No orders found</td></tr>';
        return;
    }

    // Sort orders: active orders first, then completed/cancelled orders
    const sortedOrders = orders.sort((a, b) => {
        const aStatus = (a.status || '').trim().toLowerCase();
        const bStatus = (b.status || '').trim().toLowerCase();
        
        const aIsCompleted = aStatus === 'served' || aStatus === 'cancelled';
        const bIsCompleted = bStatus === 'served' || bStatus === 'cancelled';
        
        // If one is completed and other is not, completed goes to bottom
        if (aIsCompleted && !bIsCompleted) return 1;
        if (!aIsCompleted && bIsCompleted) return -1;
        
        // Otherwise maintain original order (or sort by date)
        return new Date(b.orderDate) - new Date(a.orderDate);
    });

    sortedOrders.forEach(order => {
        let status = (order.status || '').trim();
        if (status === '' || status.toLowerCase() === 'pending') {
            status = 'Brewing';
        }

        const statusClass = status.toLowerCase().replace(/\s+/g, '-');
        const isCompleted = status.toLowerCase() === 'served' || status.toLowerCase() === 'cancelled';

        const row = document.createElement('tr');
        if (isCompleted) {
            row.classList.add('order-completed');
        }
        
        row.innerHTML = `
            <td><strong>${order.invoiceNumber}</strong></td>
            <td>${order.customerName || order.nickname || 'Guest'}</td>
            <td>${order.orderDate}</td>
            <td>${order.itemCount}</td>
            <td>₱ ${parseFloat(order.totalPrice).toFixed(2)}</td>
            <td><span class="status ${statusClass}">${status}</span></td>
            <td>
                <button class="btn-view" onclick="viewOrderDetails(${order.id})">
                    <i class="fas fa-eye"></i>
                </button>
            </td>
        `;
        orderList.appendChild(row);
    });
}

function updateOrderCount(count, filter) {
    const filterText = filter === 'All' ? 'total' : filter.toLowerCase();
    document.querySelector('.orders-count').textContent = `${count} ${filterText} order${count !== 1 ? 's' : ''} in your list.`;
}

function viewOrderDetails(orderId) {
    window.location.href = `/Order/ViewOrder?orderId=${orderId}`;
}