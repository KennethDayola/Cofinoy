document.addEventListener('DOMContentLoaded', function () {
    const orderCards = document.querySelectorAll('.order-card');
    const orderDetailSection = document.getElementById('orderDetailSection');
    const orderItemsContainer = document.getElementById('orderItemsContainer');
    const trackOrderText = document.getElementById('trackOrderText');
    const progressbar = document.getElementById('progressbar');
    const sortButton = document.getElementById('sortButton');
    const sortText = document.getElementById('sortText');
    const mobileToggleList = document.getElementById('mobileToggleList');
    const orderListContainer = document.querySelector('.order-list-container');

    let isNewestFirst = true;
    let isMobileListOpen = false;
    let autoRefreshInterval;
    let currentActiveOrderId = null;

    if (orderCards.length > 0) {
        const firstOrderCard = document.querySelector('.order-card.active');
        if (firstOrderCard) {
            currentActiveOrderId = firstOrderCard.dataset.orderId;
            updateProgressBar(currentActiveOrderId);
        }
    }

    startAutoRefresh();

    // Mobile toggle
    if (mobileToggleList) {
        mobileToggleList.addEventListener('click', function () {
            isMobileListOpen = !isMobileListOpen;
            orderListContainer.classList.toggle('mobile-open', isMobileListOpen);
            this.querySelector('i').classList.toggle('fa-chevron-down', !isMobileListOpen);
            this.querySelector('i').classList.toggle('fa-chevron-up', isMobileListOpen);
        });
    }


    orderCards.forEach(card => {
        card.addEventListener('click', function () {
            orderCards.forEach(c => c.classList.remove('active'));
            this.classList.add('active');

            currentActiveOrderId = this.dataset.orderId;
            loadOrderDetails(currentActiveOrderId);

            if (window.innerWidth <= 768) {
                orderListContainer.classList.remove('mobile-open');
                isMobileListOpen = false;
                if (mobileToggleList) {
                    mobileToggleList.querySelector('i').classList.remove('fa-chevron-up');
                    mobileToggleList.querySelector('i').classList.add('fa-chevron-down');
                }
            }
        });
    });

    
    sortButton.addEventListener('click', function () {
        toggleSortOrder();
    });

   
    async function loadOrderDetails(orderId) {
        try {
            showLoadingState();

            const response = await fetch(`/OrderHistory/GetOrderDetails?id=${orderId}`);

            if (!response.ok) {
                throw new Error('Network response was not ok');
            }

            const result = await response.json();

            if (result.success) {
                displayOrderDetails(result.order);
                updateProgressBar(orderId);
            } else {
                showErrorState('Order not found');
            }

        } catch (error) {
            console.error('Error loading order details:', error);
            showErrorState('Failed to load order details');
        }
    }

    function displayOrderDetails(order) {
        const header = orderDetailSection.querySelector('.order-detail-header');
        header.innerHTML = `
            <div class="order-header-info">
                <p class="order-invoice">Order #${order.invoiceNumber}</p> 
            </div>
            <p class="order-date-time">${order.orderDate}</p>
        `;

        let itemsHTML = '';
        if (order.items && order.items.length > 0) {
            order.items.forEach(item => {
                let description = '';
                const details = [];

                if (item.size) details.push(item.size);
                if (item.temperature) details.push(item.temperature);
                if (item.milkType) details.push(item.milkType);
                if (item.extraShots > 0) details.push(`${item.extraShots} extra shot(s)`);
                if (item.sweetnessLevel) details.push(`${item.sweetnessLevel} sweetness`);

                if (details.length > 0) {
                    description = `<p class="item-desc">${details.map(detail => `<span class="item-attribute">${detail}</span>`).join('')}</p>`;
                }

                const productImage = item.productImageUrl || item.imageUrl || item.product?.imageUrl;
                const imageHTML = productImage
                    ? `<img src="${productImage}" alt="${item.productName}" class="product-image" />`
                    : `<i class="fas fa-coffee"></i>`;

                itemsHTML += `
                    <article class="order-item">
                        <div class="item-image">
                            ${imageHTML}
                        </div>
                        <div class="item-info">
                            <h3 class="item-name">${item.productName}</h3>
                            ${description}
                        </div>
                        <div class="item-pricing">
                            <p class="item-price">₱${parseFloat(item.unitPrice).toFixed(2)}</p>
                            <p class="item-quantity">${item.quantity}</p>
                            <p class="item-total">₱${parseFloat(item.totalPrice).toFixed(2)}</p>
                        </div>
                    </article>
                `;
            });
        } else {
            itemsHTML = '<div class="no-items">No items found for this order</div>';
        }

        orderItemsContainer.innerHTML = itemsHTML;

        const orderTotal = orderDetailSection.querySelector('.order-total .amount');
        if (orderTotal) {
            orderTotal.textContent = `₱${parseFloat(order.totalPrice).toFixed(2)}`;
        }

        if (trackOrderText) {
            trackOrderText.textContent = `"Your order is ${order.status.toLowerCase()}"`;
        }
    }

    function updateProgressBar(orderId) {
        const activeCard = document.querySelector('.order-card.active');
        if (!activeCard) return;

        const statusElement = activeCard.querySelector('.status-text');
        const status = statusElement ? statusElement.textContent : 'Placed';

        updateTrackOrderProgress(status);
    }

    function updateTrackOrderProgress(status) {
        const progressSteps = document.querySelectorAll('#progressbar li');
        progressSteps.forEach(step => step.classList.remove('active'));

        if (status !== 'Cancelled') {
            progressSteps[0].classList.add('active'); 

            if (['Brewing', 'Ready', 'Serving', 'Served'].includes(status)) {
                progressSteps[1].classList.add('active');
            }
            if (['Ready', 'Serving', 'Served'].includes(status)) {
                progressSteps[2].classList.add('active'); 
            }
            if (['Serving', 'Served'].includes(status)) {
                progressSteps[3].classList.add('active'); 
            }
            if (status === 'Served') {
                progressSteps[4].classList.add('active'); 
            }
        }

 
        if (trackOrderText) {
            trackOrderText.textContent = `"Your order is ${status.toLowerCase()}"`;
        }
    }

    function toggleSortOrder() {
        const orderList = document.querySelector('.order-list');
        const orders = Array.from(orderList.querySelectorAll('.order-card'));

        orders.reverse().forEach(order => {
            orderList.appendChild(order);
        });

        isNewestFirst = !isNewestFirst;
        sortText.textContent = isNewestFirst ? 'Newest First' : 'Oldest First';

        const activeOrder = document.querySelector('.order-card.active');
        if (activeOrder) {
            orders.forEach(c => c.classList.remove('active'));
            activeOrder.classList.add('active');
        }
    }

    function showLoadingState() {
        orderItemsContainer.innerHTML = '<div class="loading-items">Loading order details...</div>';
    }

    function showErrorState(message) {
        orderItemsContainer.innerHTML = `<div class="error-loading">${message}</div>`;
    }

    
    function startAutoRefresh() {
        autoRefreshInterval = setInterval(async () => {
            await refreshOrderStatuses();
        }, 3000);
    }

    async function refreshOrderStatuses() {
        try {
            const response = await fetch('/OrderHistory/GetOrderStatuses');

            if (!response.ok) {
                throw new Error('Failed to fetch order statuses');
            }

            const result = await response.json();

            if (result.success) {
                result.data.forEach(order => {
                    updateOrderCardStatus(order.id, order.status);

                   
                    if (order.id == currentActiveOrderId) {
                        updateTrackOrderProgress(order.status);
                    }
                });
            }
        } catch (error) {
            console.error('Error refreshing order statuses:', error);
        }
    }

    function updateOrderCardStatus(orderId, newStatus) {
        const orderCard = document.querySelector(`[data-order-id="${orderId}"]`);
        if (orderCard) {
            const statusDot = orderCard.querySelector('.status-dot');
            const statusText = orderCard.querySelector('.status-text');

            statusDot.className = 'status-dot';
            statusText.className = 'status-text';

            const statusClass = `status-${newStatus.toLowerCase().replace(' ', '-')}`;
            statusDot.classList.add(statusClass);
            statusText.classList.add(statusClass);
            statusText.textContent = newStatus;
        }
    }

    window.addEventListener('beforeunload', () => {
        if (autoRefreshInterval) {
            clearInterval(autoRefreshInterval);
        }
    });

    window.addEventListener('resize', function () {
        if (window.innerWidth > 768) {
            orderListContainer.classList.remove('mobile-open');
            isMobileListOpen = false;
        }
    });
});