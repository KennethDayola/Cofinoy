document.addEventListener('DOMContentLoaded', function () {
    loadOrders();
    setupEventListeners();
    startAutoRefresh();
});

let autoRefreshInterval;
let currentFilter = 'All';

function setupEventListeners() {
    document.getElementById('searchOrders').addEventListener('keyup', function () {
        loadOrders(currentFilter === 'All' ? null : currentFilter);
    });

    document.querySelector('.btn-filter').addEventListener('click', function (e) {
        e.stopPropagation();
        showStatusFilter();
    });
}

function startAutoRefresh() {
    autoRefreshInterval = setInterval(() => {
        loadOrders(currentFilter === 'All' ? null : currentFilter, null, true);
    }, 5000);
}

window.addEventListener('beforeunload', () => {
    if (autoRefreshInterval) {
        clearInterval(autoRefreshInterval);
    }
});

function loadOrders(status = null, searchTerm = null, silentRefresh = false) {
    const search = document.getElementById('searchOrders').value || searchTerm || '';

    fetch(`${window.location.origin}/Menu/GetAllOrders?status=${encodeURIComponent(status || '')}&searchTerm=${encodeURIComponent(search)}`)
        .then(response => {
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            return response.json();
        })
        .then(data => {
            if (data.success) {
                displayOrders(data.data);
                updateOrderCount(data.count, currentFilter);
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

    orders.forEach(order => {
        let status = (order.status || '').trim();
        if (status === '' || status.toLowerCase() === 'pending') {
            status = 'Brewing';
        }

        const statusClass = status.toLowerCase().replace(/\s+/g, '-');

        const row = document.createElement('tr');
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
    window.location.href = `/Menu/ViewOrder?orderId=${orderId}`;
}

function showStatusFilter() {
    const existingDropdown = document.querySelector('.status-filter-dropdown');
    if (existingDropdown) {
        existingDropdown.remove();
        return;
    }

    const statuses = [
        { value: 'All', label: 'All Orders', icon: 'fa-list' },
        { value: 'Brewing', label: 'Brewing', icon: 'fa-fire' },
        { value: 'Ready', label: 'Ready', icon: 'fa-clock' },
        { value: 'Serving', label: 'Serving', icon: 'fa-hand-holding' },
        { value: 'Served', label: 'Served', icon: 'fa-check-double' },
        { value: 'Cancelled', label: 'Cancelled', icon: 'fa-times-circle' }
    ];

    const dropdown = document.createElement('div');
    dropdown.className = 'status-filter-dropdown';

    statuses.forEach(status => {
        const option = document.createElement('div');
        option.className = 'filter-option' + (currentFilter === status.value ? ' active' : '');
        option.innerHTML = `
            <i class="fas ${status.icon}"></i>
            <span>${status.label}</span>
            ${currentFilter === status.value ? '<i class="fas fa-check check-icon"></i>' : ''}
        `;

        option.onclick = function (e) {
            e.stopPropagation();
            currentFilter = status.value;

            const filterBtn = document.querySelector('.btn-filter');
            filterBtn.innerHTML = `<i class="fas fa-filter"></i> ${status.label}`;

            loadOrders(status.value === 'All' ? null : status.value);

            dropdown.remove();
        };

        dropdown.appendChild(option);
    });

    document.body.appendChild(dropdown);

    const filterButton = document.querySelector('.btn-filter');
    const rect = filterButton.getBoundingClientRect();

    dropdown.style.position = 'fixed';
    dropdown.style.top = `${rect.bottom + 5}px`;
    dropdown.style.left = `${rect.left}px`;
    dropdown.style.minWidth = `${rect.width}px`;
    dropdown.style.zIndex = 1000;

    setTimeout(() => {
        document.addEventListener('click', function handleOutsideClick(e) {
            if (!dropdown.contains(e.target) && !filterButton.contains(e.target)) {
                dropdown.remove();
                document.removeEventListener('click', handleOutsideClick);
            }
        });
    }, 0);
}