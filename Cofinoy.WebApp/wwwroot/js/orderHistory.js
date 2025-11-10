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
    let previousStatuses = new Map(); // Track previous statuses for animation

    if (orderCards.length > 0) {
        const firstOrderCard = document.querySelector('.order-card.active');
        if (firstOrderCard) {
            currentActiveOrderId = firstOrderCard.dataset.orderId;
            const statusElement = firstOrderCard.querySelector('.status-text');
            const initialStatus = statusElement ? statusElement.textContent.trim() : 'Placed';
            previousStatuses.set(currentActiveOrderId, initialStatus);
            // Initialize progress bar immediately
            updateTrackOrderProgress(initialStatus, false);
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

    if (sortButton) {
        sortButton.addEventListener('click', function () {
            toggleSortOrder();
        });
    }

    async function loadOrderDetails(orderId) {
        try {
            showLoadingOverlay();

            const response = await fetch(`/OrderHistory/GetOrderDetails?id=${orderId}`);

            if (!response.ok) {
                throw new Error('Network response was not ok');
            }

            const result = await response.json();

            if (result.success) {
                // Small delay for smoother UX
                await new Promise(resolve => setTimeout(resolve, 300));
                displayOrderDetails(result.order);
                updateProgressBar(orderId);
                hideLoadingOverlay();
            } else {
                hideLoadingOverlay();
                showErrorState('Order not found');
            }

        } catch (error) {
            console.error('Error loading order details:', error);
            hideLoadingOverlay();
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
                
                // Check if item has customizations array
                if (item.customizations && item.customizations.length > 0) {
                    // Sort customizations by displayOrder, then by name
                    const sortedCustomizations = item.customizations.sort((a, b) => {
                        const orderA = a.displayOrder ?? Number.MAX_SAFE_INTEGER;
                        const orderB = b.displayOrder ?? Number.MAX_SAFE_INTEGER;
                        if (orderA !== orderB) return orderA - orderB;
                        return (a.name || '').localeCompare(b.name || '');
                    });

                    const customizationBadges = sortedCustomizations.map(custom => {
                        const hasPrice = custom.price > 0;
                        const priceClass = hasPrice ? ' has-price' : '';
                        const priceText = hasPrice ? `<span class="item-attribute-price">+₱${parseFloat(custom.price).toFixed(2)}</span>` : '';
                        return `<span class="item-attribute${priceClass}">${custom.name}: ${custom.value}${priceText}</span>`;
                    });

                    if (customizationBadges.length > 0) {
                        description = `<div class="item-desc">${customizationBadges.join('')}</div>`;
                    }
                } else {
                    // Fallback to legacy fields if no customizations array
                    const details = [];

                    if (item.size) details.push(`<span class="item-attribute">Size: ${item.size}</span>`);
                    if (item.temperature) details.push(`<span class="item-attribute">Temp: ${item.temperature}</span>`);
                    if (item.milkType) details.push(`<span class="item-attribute">Milk: ${item.milkType}</span>`);
                    if (item.extraShots > 0) details.push(`<span class="item-attribute has-price">Extra Shots: ${item.extraShots}</span>`);
                    if (item.sweetnessLevel) details.push(`<span class="item-attribute">Sweetness: ${item.sweetnessLevel}</span>`);

                    if (details.length > 0) {
                        description = `<div class="item-desc">${details.join('')}</div>`;
                    }
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

        updateTrackOrderProgress(status, false);
    }

    function updateTrackOrderProgress(status, animated = false) {
        const progressSteps = document.querySelectorAll('#progressbar li');
        
        // Remove all active and animation classes first
        progressSteps.forEach(step => {
            step.classList.remove('active', 'newly-active');
        });

        const statusOrder = ['Placed', 'Brewing', 'Ready', 'Serving', 'Served'];
        const currentIndex = statusOrder.indexOf(status);

        // For cancelled orders, don't activate any steps
        if (status === 'Cancelled') {
            if (trackOrderText) {
                trackOrderText.style.opacity = '0';
                trackOrderText.style.transform = 'translateY(-10px)';
                
                setTimeout(() => {
                    trackOrderText.textContent = `"Your order has been cancelled"`;
                    trackOrderText.style.opacity = '1';
                    trackOrderText.style.transform = 'translateY(0)';
                }, 150);
            }
            return;
        }

        // Always activate Placed step for non-cancelled orders
        progressSteps[0].classList.add('active');

        // If status is found in our order, activate all steps up to it
        if (currentIndex !== -1) {
            for (let i = 1; i <= currentIndex; i++) {
                progressSteps[i].classList.add('active');
                
                // Add animation to the newly activated step
                if (animated && i === currentIndex) {
                    progressSteps[i].classList.add('newly-active');
                    // Remove the class after animation completes
                    setTimeout(() => {
                        progressSteps[i].classList.remove('newly-active');
                    }, 600);
                }
            }
        }

        // Update track order text with animation
        if (trackOrderText) {
            trackOrderText.style.opacity = '0';
            trackOrderText.style.transform = 'translateY(-10px)';
            
            setTimeout(() => {
                trackOrderText.textContent = `"Your order is ${status.toLowerCase()}"`;
                trackOrderText.style.opacity = '1';
                trackOrderText.style.transform = 'translateY(0)';
            }, 150);
        }
    }

    function toggleSortOrder() {
        const orderList = document.querySelector('.order-list');
        const orders = Array.from(orderList.querySelectorAll('.order-card'));

        orders.reverse().forEach(order => {
            orderList.appendChild(order);
        });

        isNewestFirst = !isNewestFirst;
        if (sortText) {
            sortText.textContent = isNewestFirst ? 'Newest First' : 'Oldest First';
        }

        const activeOrder = document.querySelector('.order-card.active');
        if (activeOrder) {
            orders.forEach(c => c.classList.remove('active'));
            activeOrder.classList.add('active');
        }
    }

    function showLoadingOverlay() {
        // Remove any existing overlay
        hideLoadingOverlay();
        
        const overlay = document.createElement('div');
        overlay.className = 'loading-overlay';
        overlay.innerHTML = `
            <div class="loading-spinner"></div>
            <div class="loading-text">Loading order details...</div>
        `;
        orderDetailSection.style.position = 'relative';
        orderDetailSection.appendChild(overlay);
    }

    function hideLoadingOverlay() {
        const overlay = orderDetailSection.querySelector('.loading-overlay');
        if (overlay) {
            overlay.style.opacity = '0';
            setTimeout(() => {
                overlay.remove();
            }, 200);
        }
    }

    function showLoadingState() {
        orderItemsContainer.innerHTML = '<div class="loading-items">Loading order details...</div>';
    }

    function showErrorState(message) {
        orderItemsContainer.innerHTML = `<div class="error-loading">${message}</div>`;
    }

    function showStatusNotification(newStatus) {
        // Remove any existing notification
        const existingNotification = document.querySelector('.status-notification');
        if (existingNotification) {
            existingNotification.remove();
        }

        const notification = document.createElement('div');
        notification.className = 'status-notification';
        notification.innerHTML = `
            <span class="status-notification-icon">🎉</span>
            <span class="status-notification-text">Your order status changed to: <strong>${newStatus}</strong></span>
        `;
        document.body.appendChild(notification);

        // Auto-remove after 3 seconds
        setTimeout(() => {
            notification.remove();
        }, 3000);
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
                    const previousStatus = previousStatuses.get(order.id);
                    const hasStatusChanged = previousStatus && previousStatus !== order.status;

                    updateOrderCardStatus(order.id, order.status);

                    // Update for currently viewed order
                    if (order.id == currentActiveOrderId) {
                        if (hasStatusChanged) {
                            // Show notification and animate progress bar
                            showStatusNotification(order.status);
                            updateTrackOrderProgress(order.status, true);
                        } else {
                            updateTrackOrderProgress(order.status, false);
                        }
                    }

                    // Store the new status
                    previousStatuses.set(order.id, order.status);
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

            if (statusDot && statusText) {
                // Get previous classes
                const previousClass = statusText.className;
                
                // Reset classes
                statusDot.className = 'status-dot';
                statusText.className = 'status-text';

                // Apply new status class
                const statusClass = `status-${newStatus.toLowerCase().replace(' ', '-')}`;
                statusDot.classList.add(statusClass);
                statusText.classList.add(statusClass);
                statusText.textContent = newStatus;

                // Add a brief highlight effect if status changed
                if (previousClass !== statusText.className) {
                    orderCard.style.backgroundColor = '#fff9f2';
                    setTimeout(() => {
                        if (!orderCard.classList.contains('active')) {
                            orderCard.style.backgroundColor = '';
                        }
                    }, 1000);
                }
            }
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

    // Add transition styles to track text
    if (trackOrderText) {
        trackOrderText.style.transition = 'all 0.3s ease';
    }
});