const pendingCartUpdates = new Set();
let currentOverallTotal = 0;
const pendingRequests = new Map();


document.addEventListener("DOMContentLoaded", function () {
    console.log("Cart initialized with stock validation");
    initializeCart();
});

function initializeCart() {
    document.querySelectorAll('.quantity-btn').forEach(btn => {
        btn.addEventListener('click', handleQuantityChange);
    });

    currentOverallTotal = 0;
    updateOverallTotal();

    validateCartOnLoad();
}

async function validateCartOnLoad() {
    const cartItems = document.querySelectorAll('.cart-item');

    for (const item of cartItems) {
        const productId = item.dataset.productId;
        const currentQty = parseInt(item.querySelector('.quantity-value').textContent);
        const stock = await getProductStock(productId);

        if (currentQty > stock) {
            if (stock === 0) {
                showToast(`${item.querySelector('.item-name').textContent} is out of stock`, "danger", "Stock Update");
                await removeFromCart(item.dataset.cartItemId);
            } else {
                await updateQuantity(item.dataset.cartItemId, productId, 'set', stock);
                showToast(`Stock updated! ${item.querySelector('.item-name').textContent} quantity adjusted to ${stock}`, "warning", "Stock Update");
            }
        }
    }
}

async function handleQuantityChange(event) {
    const button = event.currentTarget;
    const cartItemElement = button.closest('.cart-item');
    const cartItemId = cartItemElement.dataset.cartItemId;
    const productId = cartItemElement.dataset.productId;
    const action = button.querySelector('.material-symbols-rounded').textContent === 'remove' ? 'decrease' : 'increase';

    if (action === 'increase') {
        button.style.opacity = '0.8';
        updateQuantity(cartItemId, productId, action);
        setTimeout(() => {
            button.style.opacity = '1';
        }, 150);
    } else {

        updateQuantity(cartItemId, productId, action);
    }
}


async function updateQuantity(cartItemId, productId, action, setValue = null) {
    try {
        const quantityElement = document.getElementById(`quantity-${cartItemId}`);
        const totalElement = document.getElementById(`total-${cartItemId}`);

        if (!quantityElement) return;

        let currentQuantity = parseInt(quantityElement.textContent) || 1;
        let newQuantity = currentQuantity;

        if (action === 'increase') {
            newQuantity = currentQuantity + 1;
        } else if (action === 'decrease') {
            newQuantity = Math.max(1, currentQuantity - 1);
        } else if (action === 'set' && setValue !== null) {
            newQuantity = setValue;
        }

        if (newQuantity === currentQuantity) return;

        const unitPrice = getUnitPriceFromItem(cartItemId);
        const newItemTotal = unitPrice * newQuantity;

        quantityElement.textContent = newQuantity;
        if (totalElement) {
            totalElement.textContent = `₱${newItemTotal.toFixed(2)}`;
        }
        updateOverallTotal();

        if (action === 'increase') {
            const stock = await getProductStock(productId);
            if (currentQuantity >= stock) {

                quantityElement.textContent = currentQuantity;
                const originalTotal = unitPrice * currentQuantity;
                if (totalElement) {
                    totalElement.textContent = `₱${originalTotal.toFixed(2)}`;
                }
                updateOverallTotal();
                showToast(`Only ${stock} in stock`, "danger", "Cofinoy");
                return;
            }
        }


        if (pendingRequests && pendingRequests.has(cartItemId)) {
            clearTimeout(pendingRequests.get(cartItemId));
        }

        const requestTimeout = setTimeout(async () => {
            try {
                const result = await updateCartItemQuantity(cartItemId, newQuantity);
                if (pendingRequests) pendingRequests.delete(cartItemId);

                if (!result.success && result.error && result.error.includes('Only') && result.maxAllowed) {
                    const serverQuantity = result.maxAllowed;
                    const serverItemTotal = unitPrice * serverQuantity;

                    quantityElement.textContent = serverQuantity;
                    if (totalElement) {
                        totalElement.textContent = `₱${serverItemTotal.toFixed(2)}`;
                    }
                    updateOverallTotal();
                    showToast(result.error, "warning", "Stock Limit");
                }
            } catch (error) {
                console.error('Background update failed:', error);
                if (pendingRequests) pendingRequests.delete(cartItemId);
            }
        }, 100);

        if (pendingRequests) pendingRequests.set(cartItemId, requestTimeout);

    } catch (error) {
        console.error('Error in updateQuantity:', error);
    }
}



function updateOverallTotal(changeAmount = null) {
    const totalAmountElement = document.getElementById('totalAmount');
    if (!totalAmountElement) return;

    let overallTotal = 0;
    let totalItems = 0;

    const cartItems = document.querySelectorAll('.cart-item');

    cartItems.forEach(item => {
        const totalElement = item.querySelector('[id^="total-"]');
        const quantityElement = item.querySelector('[id^="quantity-"]');

        if (totalElement && quantityElement) {
            const priceText = totalElement.textContent.replace('₱', '').replace(',', '').trim();
            const itemTotal = parseFloat(priceText) || 0;

            const quantity = parseInt(quantityElement.textContent) || 0;

            overallTotal += itemTotal;
            totalItems += quantity;
        }
    });


    totalAmountElement.textContent = `₱${overallTotal.toFixed(2)}`;

    const cartSummary = document.getElementById('cartSummary');
    if (cartSummary) {
        cartSummary.textContent = `${totalItems} item(s) in your cart`;
    }

    const cartCountElement = document.querySelector('.cart-count');
    if (cartCountElement) {
        cartCountElement.textContent = totalItems;
    }
}

function getUnitPriceFromItem(cartItemId) {
    const cartItem = document.querySelector(`[data-cart-item-id="${cartItemId}"]`);
    if (!cartItem) {
        console.error('Cart item not found:', cartItemId);
        return 0;
    }

    const priceElement = cartItem.querySelector('.item-price');
    if (priceElement) {
        return parseCurrency(priceElement.textContent);
    }

    console.error('Price element not found for cart item:', cartItemId);
    return 0;
}


const stockCache = new Map();

async function getProductStock(productId) {

    if (stockCache.has(productId)) {
        const cached = stockCache.get(productId);
        if (Date.now() - cached.timestamp < 10000) {
            return cached.stock;
        }
    }

    try {
        const response = await fetch(`/Menu/GetProductStock?productId=${productId}`, {
            method: 'GET',
            headers: { 'Accept': 'application/json' }
        });

        if (response.ok) {
            const result = await response.json();
            if (result.success) {

                stockCache.set(productId, {
                    stock: result.stock,
                    timestamp: Date.now()
                });
                return result.stock;
            }
        }

        console.warn('Failed to get stock for product:', productId);
        return 999;

    } catch (error) {
        console.error('Error getting product stock:', error);
        return 999;
    }
}


async function updateCartItemQuantity(cartItemId, newQuantity) {
    try {
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        if (!tokenElement) {
            throw new Error('Anti-forgery token not found');
        }

        const response = await fetch('/Cart/UpdateQuantity', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': tokenElement.value
            },
            body: JSON.stringify({
                cartItemId: cartItemId,
                quantity: newQuantity
            }),
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        const result = await response.json();
        return result;

    } catch (error) {
        console.error('Error updating cart quantity:', error);
        return { success: false, error: error.message };
    }
}

function updateCartSummary(itemCount, totalAmount) {
    const totalAmountElement = document.getElementById('totalAmount');
    if (totalAmountElement && totalAmount !== undefined) {
        let total = parseFloat(totalAmount);
        if (!isNaN(total)) {
            totalAmountElement.textContent = `₱${total.toFixed(2)}`;
        }
    }

    const cartSummary = document.getElementById('cartSummary');
    if (cartSummary) {
        cartSummary.textContent = `${itemCount} item(s) in your cart`;
    }

    const cartCountElement = document.querySelector('.cart-count');
    if (cartCountElement) {
        cartCountElement.textContent = itemCount;
    }
}


async function removeFromCart(cartItemId) {
    if (!confirm('Are you sure you want to remove this item from your cart?')) {
        return;
    }

    try {
        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        if (!tokenElement) {
            throw new Error('Anti-forgery token not found');
        }

        const response = await fetch('/Cart/RemoveFromCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': tokenElement.value
            },
            body: JSON.stringify({ cartItemId: cartItemId }),
            credentials: 'include'
        });

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        const result = await response.json();

        if (result.success) {
            const cartItemElement = document.getElementById(`cartItem-${cartItemId}`);
            if (cartItemElement) {
                cartItemElement.remove();
            }

            updateCartSummary(result.cartCount, result.total);

            const cartItemsContainer = document.getElementById('cartItemsContainer');
            const existingItems = cartItemsContainer.querySelectorAll('.cart-item');

            if (existingItems.length === 0) {
                showEmptyCartMessage();
                disableCheckout();
            }

            if (typeof showToast === 'function') {
                showToast('Item removed from cart', 'success', 'Cart Updated');
            }
        } else {
            throw new Error(result.error || 'Failed to remove item');
        }

    } catch (error) {
        console.error('Error removing from cart:', error);
        if (typeof showToast === 'function') {
            showToast('Error removing item: ' + error.message, 'danger', 'Error');
        }
    }
}

function showEmptyCartMessage() {
    const cartItemsContainer = document.getElementById('cartItemsContainer');
    cartItemsContainer.innerHTML = `
        <div class="empty-cart-message">
            <div class="empty-cart-icon">
                <span class="material-symbols-rounded">shopping_bag</span>
            </div>
            <p>Your cart is empty</p>
            <p class="empty-cart-subtitle">Start shopping to add some delicious items!</p>
        </div>
    `;
}

function disableCheckout() {
    const placeOrderBtn = document.getElementById('placeOrderBtn');
    if (placeOrderBtn) {
        placeOrderBtn.disabled = true;
        placeOrderBtn.classList.add('order-btn-disabled');
    }
}

function showToast(message, type, title = '') {
    if (typeof window.showToast === 'function') {
        window.showToast(message, type, title);
        return;
    }

    const toast = document.createElement('div');
    toast.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 12px 20px;
        background: ${type === 'success' ? '#28a745' : type === 'danger' ? '#dc3545' : type === 'warning' ? '#ffc107' : '#007bff'};
        color: ${type === 'warning' ? '#000' : 'white'};
        border-radius: 5px;
        z-index: 10000;
        box-shadow: 0 2px 10px rgba(0,0,0,0.2);
        font-family: 'Poppins', sans-serif;
    `;
    toast.innerHTML = `${title ? `<strong>${title}:</strong> ` : ''}${message}`;

    document.body.appendChild(toast);

    setTimeout(() => {
        toast.remove();
    }, 3000);
}

function formatNumber(value) {
    const num = Number(value);
    if (isNaN(num)) return '0.00';
    return num.toFixed(2);
}

function parseCurrency(value) {
    if (value === null || value === undefined) return NaN;
    if (typeof value === 'number') return value;
    const normalized = String(value).replace(/[^0-9.\-]/g, '');
    return normalized ? parseFloat(normalized) : NaN;
}

function calculateCartTotalFromDom() {
    let sum = 0;
    document.querySelectorAll('[id^="total-"]').forEach(el => {
        const val = parseCurrency(el.textContent);
        if (!isNaN(val)) {
            sum += val;
        }
    });
    return sum;
}

function calculateCartQuantityFromDom() {
    let total = 0;
    document.querySelectorAll('[id^="quantity-"]').forEach(el => {
        const val = parseInt(el.textContent);
        if (!isNaN(val)) {
            total += val;
        }
    });
    return total;
}

function refreshCartSummaryFromDom() {
    const totalAmountElement = document.getElementById('totalAmount');
    if (totalAmountElement) {
        totalAmountElement.textContent = `₱${formatNumber(calculateCartTotalFromDom())}`;
    }
    const cartSummary = document.getElementById('cartSummary');
    if (cartSummary) {
        cartSummary.textContent = `${calculateCartQuantityFromDom()} item(s) in your cart`;
    }
}
