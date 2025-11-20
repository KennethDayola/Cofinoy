// cart.js - Complete fixed version with stock validation
document.addEventListener("DOMContentLoaded", function () {
    console.log("Cart initialized with stock validation");
    initializeCart();
});

function initializeCart() {
    document.querySelectorAll('.quantity-btn').forEach(btn => {
        btn.addEventListener('click', handleQuantityChange);
    });

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

    await updateQuantity(cartItemId, productId, action);
}

async function updateQuantity(cartItemId, productId, action, setValue = null) {
    try {
        const quantityElement = document.getElementById(`quantity-${cartItemId}`);
        const totalElement = document.getElementById(`total-${cartItemId}`);

        if (!quantityElement) {
            console.error('Quantity element not found for:', cartItemId);
            return;
        }

        let currentQuantity = parseInt(quantityElement.textContent);
        let newQuantity = currentQuantity;

        const stock = await getProductStock(productId);

        if (action === 'increase') {
            if (currentQuantity >= stock) {
                showToast(`Only ${stock} in stock`, "danger", "Cofinoy");
                return;
            }
            newQuantity = currentQuantity + 1;
        } else if (action === 'decrease') {
            newQuantity = Math.max(1, currentQuantity - 1);
        } else if (action === 'set' && setValue !== null) {
            newQuantity = Math.min(setValue, stock);
        }

        if (newQuantity === currentQuantity) return;

        const result = await updateCartItemQuantity(cartItemId, newQuantity);

        if (result.success) {
            quantityElement.textContent = newQuantity;

            const unitPrice = parseFloat(result.newUnitPrice || getUnitPriceFromItem(cartItemId));
            const newTotal = unitPrice * newQuantity;
            if (totalElement) {
                totalElement.textContent = `₱${newTotal.toFixed(2)}`;
            }

            updateCartSummary(result.cartCount, result.newTotal);

        } else {
            if (result.error && result.error.includes('Only') && result.maxAllowed) {
                quantityElement.textContent = result.maxAllowed;
                showToast(result.error, "warning", "Stock Limit");

                const unitPrice = parseFloat(result.newUnitPrice || getUnitPriceFromItem(cartItemId));
                const newTotal = unitPrice * result.maxAllowed;
                if (totalElement) {
                    totalElement.textContent = `₱${newTotal.toFixed(2)}`;
                }
            } else {
                throw new Error(result.error || 'Failed to update quantity');
            }
        }

    } catch (error) {
        console.error('Error updating quantity:', error);
        if (typeof showToast === 'function') {
            showToast('Error updating quantity: ' + error.message, 'danger', 'Error');
        }
    }
}

function getUnitPriceFromItem(cartItemId) {
    const cartItem = document.getElementById(`cartItem-${cartItemId}`);
    const priceElement = cartItem.querySelector('.item-price');
    if (priceElement) {
        const priceText = priceElement.textContent.replace('₱', '').trim();
        return parseFloat(priceText) || 0;
    }
    return 0;
}

async function getProductStock(productId) {
    try {
        const response = await fetch(`/Menu/GetProductStock?productId=${productId}`, {
            method: 'GET',
            headers: { 'Accept': 'application/json' }
        });

        if (response.ok) {
            const result = await response.json();
            if (result.success) {
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
    const cartSummary = document.getElementById('cartSummary');
    if (cartSummary) {
        cartSummary.textContent = `${itemCount} item(s) in your cart`;
    }

    const totalAmountElement = document.getElementById('totalAmount');
    if (totalAmountElement) {
        totalAmountElement.textContent = `₱${parseFloat(totalAmount).toFixed(2)}`;
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

            updateCartSummary(result.cartCount, result.newTotal);

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