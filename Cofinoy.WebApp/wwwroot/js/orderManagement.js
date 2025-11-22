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

    fetch(`${window.location.origin}/Order/GetAllOrders?status=${encodeURIComponent(status || '')}&searchTerm=${encodeURIComponent(search)}`)
        .then(response => {
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            return response.json();
        })
        .then(data => {
            if (data.success) {
                displayOrders(data.data);
                updateOrderCount(data.data.length, currentFilter);
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

    const sortedOrders = orders.sort((a, b) => {
        const aStatus = (a.status || '').trim().toLowerCase();
        const bStatus = (b.status || '').trim().toLowerCase();

        const aIsCompleted = aStatus === 'served' || aStatus === 'cancelled';
        const bIsCompleted = bStatus === 'served' || bStatus === 'cancelled';

        if (aIsCompleted && !bIsCompleted) return 1;
        if (!aIsCompleted && bIsCompleted) return -1;

        return new Date(b.orderDate) - new Date(a.orderDate);
    });

    sortedOrders.forEach(order => {
        let status = (order.status || '').trim();
        if (status === '') status = 'Pending'; 

        const statusClass = status.toLowerCase().replace(/\s+/g, '-');
        const isCompleted = status.toLowerCase() === 'served' || status.toLowerCase() === 'cancelled';

        const row = document.createElement('tr');
        if (isCompleted) row.classList.add('order-completed');

       
        const actionButtons = [
            `<button class="btn-view" onclick="viewOrderDetails(${order.id})">
                <i class="fas fa-eye"></i>
            </button>`
        ];

        row.innerHTML = `
            <td><strong>${order.invoiceNumber}</strong></td>
            <td>${order.customerName || order.nickname || 'Guest'}</td>
            <td>${order.orderDate}</td>
            <td>${order.itemCount}</td>
            <td>₱ ${parseFloat(order.totalPrice).toFixed(2)}</td>
            <td><span class="status ${statusClass}">${status}</span></td>
            <td>${actionButtons.join('')}</td>
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

function showStatusFilter() {
    const existingDropdown = document.querySelector('.status-filter-dropdown');
    if (existingDropdown) {
        existingDropdown.remove();
        return;
    }

    const statuses = [
        { value: 'All', label: 'All Orders', icon: 'fa-list' },
        { value: 'Pending', label: 'Pending', icon: 'fa-hourglass-start' },
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
            filterBtn.innerHTML = `
            <svg width="15" height="15" viewBox="0 0 31 21" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M1.83333 20.75C1.34931 20.75 0.943576 20.5863 0.616146 20.2589C0.288715 19.9314 0.125 19.5257 0.125 19.0417C0.125 18.5576 0.288715 18.1519 0.616146 17.8245C0.943576 17.497 1.34931 17.3333 1.83333 17.3333H8.66667C9.15069 17.3333 9.55642 17.497 9.88385 17.8245C10.2113 18.1519 10.375 18.5576 10.375 19.0417C0.375 19.5257 10.2113 19.9314 9.88385 20.2589C9.55642 20.5863 9.15069 20.75 8.66667 20.75H1.83333ZM1.83333 12.2083C1.34931 12.2083 0.943576 12.0446 0.616146 11.7172C0.288715 11.3898 0.125 10.984 0.125 10.5C0.125 10.016 0.288715 9.61024 0.616146 9.28281C0.943576 8.95538 1.34931 8.79167 1.83333 8.79167H18.9167C19.4007 8.79167 19.8064 8.95538 20.1339 9.28281C20.4613 9.61024 20.625 10.016 20.625 10.5C20.625 10.984 20.4613 11.3898 20.1339 11.7172C19.8064 12.0446 19.4007 12.2083 18.9167 12.2083H1.83333ZM1.83333 3.66667C1.34931 3.66667 0.943576 3.50295 0.616146 3.17552C0.288715 2.84809 0.125 2.44236 0.125 1.95833C0.125 1.47431 0.288715 1.06858 0.616146 0.741146C0.943576 0.413715 1.34931 0.25 1.83333 0.25H29.1667C29.6507 0.25 30.0564 0.413715 30.3839 0.741146C30.7113 1.06858 30.875 1.47431 30.875 1.95833C30.875 2.44236 30.7113 2.84809 30.3839 3.17552C30.0564 3.50295 29.6507 3.66667 29.1667 3.66667H1.83333Z" fill="#322708"/>
            </svg>
            ${status.label}
        `;

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
let searchTimeout;
document.getElementById('searchOrders').addEventListener('keyup', function () {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
        loadOrders(currentFilter === 'All' ? null : currentFilter);
    }, 300); // Wait 300ms after user stops typing
});
function startAutoRefresh() {
    autoRefreshInterval = setInterval(() => {
        // Skip refresh if user is actively searching
        if (document.getElementById('searchOrders').value.trim()) return;
        loadOrders(currentFilter === 'All' ? null : currentFilter, null, true);
    }, 15000); // 15 seconds instead of 5
}
