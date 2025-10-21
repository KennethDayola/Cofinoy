document.addEventListener('DOMContentLoaded', function () {
    loadOrders();
    setupEventListeners();
});

function setupEventListeners() {
    // 🔍 Search functionality
    document.getElementById('searchOrders').addEventListener('keyup', function () {
        loadOrders();
    });

    // 🧭 Filter button
    document.querySelector('.btn-filter').addEventListener('click', function () {
        showStatusFilter();
    });
}

// ✅ Fetch and display all orders
function loadOrders(status = null, searchTerm = null) {
    const search = document.getElementById('searchOrders').value || searchTerm || '';

    fetch(`${window.location.origin}/Menu/GetAllOrders?status=${encodeURIComponent(status || '')}&searchTerm=${encodeURIComponent(search)}`)
        .then(response => {
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            return response.json();
        })
        .then(data => {
            if (data.success) {
                displayOrders(data.data);
                updateOrderCount(data.count);
            } else {
                console.error('Error loading orders:', data.error);
            }
        })
        .catch(error => console.error('Fetch error:', error));
}

function displayOrders(orders) {
    const orderList = document.getElementById('orderList');
    orderList.innerHTML = '';

    if (!orders || orders.length === 0) {
        orderList.innerHTML = '<tr><td colspan="7" style="text-align:center; padding: 20px;">No orders found</td></tr>';
        return;
    }

    orders.forEach(order => {
        // Normalize status (default to Brewing)
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

function updateOrderCount(count) {
    document.querySelector('.orders-count').textContent = `${count} orders in your list.`;
}

// ✅ Redirects to full Order Details page
function viewOrderDetails(orderId) {
    window.location.href = `/Menu/ViewOrder?orderId=${orderId}`;
}

// ✅ Filter dropdown (for All Orders button)
function showStatusFilter() {
    const existingDropdown = document.querySelector('.status-filter-dropdown');
    if (existingDropdown) {
        existingDropdown.remove();
        return;
    }

    const statuses = ['All', 'Brewing', 'Confirmed', 'Preparing', 'Ready', 'Completed', 'Cancelled'];
    const dropdown = document.createElement('div');
    dropdown.className = 'status-filter-dropdown';

    statuses.forEach(status => {
        const option = document.createElement('div');
        option.className = 'filter-option';
        option.textContent = status;
        option.onclick = function () {
            loadOrders(status === 'All' ? null : status);
            dropdown.remove();
        };
        dropdown.appendChild(option);
    });

    document.body.appendChild(dropdown);

    const filterButton = document.querySelector('.btn-filter');
    const rect = filterButton.getBoundingClientRect();

    dropdown.style.position = 'absolute';
    dropdown.style.top = `${rect.bottom + window.scrollY}px`;
    dropdown.style.left = `${rect.left + window.scrollX}px`;
    dropdown.style.minWidth = `${rect.width}px`;
    dropdown.style.zIndex = 1000;

    // Close dropdown when clicking outside
    document.addEventListener('click', function handleOutsideClick(e) {
        if (!dropdown.contains(e.target) && !filterButton.contains(e.target)) {
            dropdown.remove();
            document.removeEventListener('click', handleOutsideClick);
        }
    });
}
