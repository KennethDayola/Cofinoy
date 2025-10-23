document.addEventListener('DOMContentLoaded', function() {
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


    if (orderCards.length > 0) {
        const firstOrderCard = document.querySelector('.order-card.active');
        if (firstOrderCard) {
            updateProgressBar(firstOrderCard.dataset.orderId);
        }
    }


    if (mobileToggleList) {
        mobileToggleList.addEventListener('click', function() {
            isMobileListOpen = !isMobileListOpen;
            orderListContainer.classList.toggle('mobile-open', isMobileListOpen);
            this.querySelector('i').classList.toggle('fa-chevron-down', !isMobileListOpen);
            this.querySelector('i').classList.toggle('fa-chevron-up', isMobileListOpen);
        });
    }


    orderCards.forEach(card => {
        card.addEventListener('click', function() {

            orderCards.forEach(c => c.classList.remove('active'));
            
            this.classList.add('active');
            
            loadOrderDetails(this.dataset.orderId);
            
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


    sortButton.addEventListener('click', function() {
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
        
        
        const progressSteps = document.querySelectorAll('#progressbar li');
        progressSteps.forEach(step => step.classList.remove('active'));
        
        switch(status.toLowerCase()) {
            case 'placed':
                progressSteps[0].classList.add('active');
                break;
            case 'brewing':
                progressSteps[0].classList.add('active');
                progressSteps[1].classList.add('active');
                break;
            case 'ready':
                progressSteps[0].classList.add('active');
                progressSteps[1].classList.add('active');
                progressSteps[2].classList.add('active');
                break;
            case 'serving':
                progressSteps[0].classList.add('active');
                progressSteps[1].classList.add('active');
                progressSteps[2].classList.add('active');
                progressSteps[3].classList.add('active');
                break;
            case 'served':
                progressSteps.forEach(step => step.classList.add('active'));
                break;
            default:
                progressSteps[0].classList.add('active');
                break;
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

    window.addEventListener('resize', function() {
        if (window.innerWidth > 768) {
            orderListContainer.classList.remove('mobile-open');
            isMobileListOpen = false;
        }
    });
});