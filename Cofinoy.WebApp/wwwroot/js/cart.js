document.addEventListener('DOMContentLoaded', function () {
    console.log('Cart page initialized');
    calculateCartTotals();
});

let quantityUpdateTimers = {};
let pendingUpdates = new Set();

function calculateCartTotals() {
    let subtotal = 0;

    const cartItems = document.querySelectorAll('.cart-item');

    if (cartItems.length === 0) {
        const subtotalElement = document.getElementById('subtotalAmount');
        const totalElement = document.getElementById('totalAmount');

        if (subtotalElement) {
            subtotalElement.textContent = '₱0.00';
        }
        if (totalElement) {
            totalElement.textContent = '₱0.00';
        }
        return;
    }

    cartItems.forEach(item => {
        const quantity = parseInt(item.querySelector('.quantity-value').textContent);
        const unitPrice = parseFloat(item.querySelector('.item-price').textContent.replace('₱', '').replace(',', ''));
        const itemTotal = quantity * unitPrice;

        const itemTotalElement = item.querySelector('.item-total');
        if (itemTotalElement) {
            itemTotalElement.textContent = '₱' + itemTotal.toFixed(2);
        }

        subtotal += itemTotal;
    });

    
    const total = subtotal; 

    const subtotalElement = document.getElementById('subtotalAmount');
    const totalElement = document.getElementById('totalAmount');

    if (subtotalElement) {
        subtotalElement.textContent = '₱' + subtotal.toFixed(2);
    }
    if (totalElement) {
        totalElement.textContent = '₱' + total.toFixed(2);
    }
}


async function updateQuantity(cartItemId, action) {
    console.log('updateQuantity called with cartItemId:', cartItemId, 'action:', action);
    
    const quantityElement = document.getElementById(`quantity-${cartItemId}`);
    if (!quantityElement) {
        console.error('Quantity element not found for cartItemId:', cartItemId);
        return;
    }

    if (pendingUpdates.has(cartItemId)) {
        console.log('Update already pending for:', cartItemId);
        return;
    }
    
    let quantity = parseInt(quantityElement.textContent);

    if (action === 'increase') {
        quantity++;
    } else if (action === 'decrease' && quantity > 1) {
        quantity--;
    } else {
        return;
    }

    quantityElement.textContent = quantity;
    
    const cartItem = document.getElementById(`cartItem-${cartItemId}`);
    const buttons = cartItem.querySelectorAll('.quantity-btn');
    buttons.forEach(btn => btn.disabled = true);
    
    pendingUpdates.add(cartItemId);

    if (quantityUpdateTimers[cartItemId]) {
        clearTimeout(quantityUpdateTimers[cartItemId]);
    }

    quantityUpdateTimers[cartItemId] = setTimeout(async () => {
        try {
            const response = await fetch('/Cart/UpdateQuantity', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: JSON.stringify({ cartItemId: cartItemId, quantity: quantity })
            });

            const result = await response.json();

            if (result.success) {
                quantityElement.textContent = quantity;

                const itemTotalElement = document.getElementById(`total-${cartItemId}`);
                if (itemTotalElement) {
                    itemTotalElement.textContent = `₱${result.itemTotal.toFixed(2)}`;
                }

                const subtotalElement = document.getElementById('subtotalAmount');
                const totalElement = document.getElementById('totalAmount');

                if (subtotalElement) {
                    subtotalElement.textContent = `₱${result.subtotal.toFixed(2)}`;
                }
                if (totalElement) {
                    totalElement.textContent = `₱${result.total.toFixed(2)}`;
                }

                updateCartSummary(result.cartCount);
            } else {
                const previousQuantity = action === 'increase' ? quantity - 1 : quantity + 1;
                quantityElement.textContent = previousQuantity;
                alert('Error updating quantity: ' + result.error);
            }
        } catch (error) {
            console.error('Error updating quantity:', error);
            const previousQuantity = action === 'increase' ? quantity - 1 : quantity + 1;
            quantityElement.textContent = previousQuantity;
            alert('Error updating quantity');
        } finally {
            buttons.forEach(btn => btn.disabled = false);
            pendingUpdates.delete(cartItemId);
            delete quantityUpdateTimers[cartItemId];
        }
    }, 300); 
}

async function removeFromCart(cartItemId) {
    try {
        const response = await fetch('/Cart/RemoveFromCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({ cartItemId: cartItemId })
        });

        const data = await response.json();

        if (data.success) {
            const itemElement = document.getElementById(`cartItem-${cartItemId}`);
            if (itemElement) {
                itemElement.style.transition = 'opacity 0.3s ease';
                itemElement.style.opacity = '0';
                setTimeout(() => {
                    itemElement.remove();

                    const cartItemsList = document.querySelector('.cart-items-list');
                    if (cartItemsList && cartItemsList.children.length === 0) {
                        location.reload();
                    }
                }, 300);
            }

            document.getElementById('subtotalAmount').textContent = `₱${data.subtotal.toFixed(2)}`;
            document.getElementById('totalAmount').textContent = `₱${data.total.toFixed(2)}`;

            const cartCountElement = document.getElementById('cartCount');
            if (cartCountElement) {
                cartCountElement.textContent = data.cartCount;
            }

      
            document.getElementById('cartSummary').textContent = `${data.cartCount} item(s) in your cart`;

    
            showToast('Item removed from cart', 'success');
        } else {
            showToast('Failed to remove item', 'error');
        }
    } catch (error) {
        console.error('Error removing item:', error);
        showToast('An error occurred', 'error');
    }
}
function getAntiForgeryToken() {
    const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenElement ? tokenElement.value : '';
}

function updateCartSummary(count) {
    const cartSummary = document.getElementById('cartSummary');
    if (cartSummary) {
        cartSummary.textContent = `${count} item(s) in your cart`;
    }
}

function showToast(message, type = 'success') {
    const toast = document.getElementById('cartToast');
    const toastMessage = toast.querySelector('.toast-message');
    const toastIcon = toast.querySelector('.toast-icon');

   
    toastMessage.textContent = message;

    if (type === 'success') {
        toastIcon.textContent = 'check_circle';
        toast.classList.remove('error');
        toast.classList.add('success');
    } else if (type === 'error') {
        toastIcon.textContent = 'error';
        toast.classList.remove('success');
        toast.classList.add('error');
    }

    toast.classList.add('show');

    setTimeout(() => {
        toast.classList.remove('show');
    }, 3000);
}