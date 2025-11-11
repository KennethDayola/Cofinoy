document.addEventListener('DOMContentLoaded', function () {
    console.log('Cart page initialized');
    calculateCartTotals();
});

// Debounce timer for quantity updates
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

    // Prevent multiple rapid clicks
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

    // Update UI immediately for better responsiveness
    quantityElement.textContent = quantity;
    
    // Disable buttons to prevent rapid clicking
    const cartItem = document.getElementById(`cartItem-${cartItemId}`);
    const buttons = cartItem.querySelectorAll('.quantity-btn');
    buttons.forEach(btn => btn.disabled = true);
    
    // Add pending state
    pendingUpdates.add(cartItemId);

    // Clear existing timer for this item
    if (quantityUpdateTimers[cartItemId]) {
        clearTimeout(quantityUpdateTimers[cartItemId]);
    }

    // Debounce the API call - wait 300ms after last click
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
                // Update from server response to ensure accuracy
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
                // Revert on error
                const previousQuantity = action === 'increase' ? quantity - 1 : quantity + 1;
                quantityElement.textContent = previousQuantity;
                alert('Error updating quantity: ' + result.error);
            }
        } catch (error) {
            console.error('Error updating quantity:', error);
            // Revert on error
            const previousQuantity = action === 'increase' ? quantity - 1 : quantity + 1;
            quantityElement.textContent = previousQuantity;
            alert('Error updating quantity');
        } finally {
            // Re-enable buttons
            buttons.forEach(btn => btn.disabled = false);
            pendingUpdates.delete(cartItemId);
            delete quantityUpdateTimers[cartItemId];
        }
    }, 300); // 300ms debounce delay
}

async function removeFromCart(cartItemId) {
    console.log('removeFromCart called with cartItemId:', cartItemId);
    
    if (!confirm('Are you sure you want to remove this item from your cart?')) {
        return;
    }

    try {
        const response = await fetch('/Cart/RemoveFromCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify({ cartItemId: cartItemId })
        });

        const result = await response.json();

        if (result.success) {
            const cartItemElement = document.getElementById(`cartItem-${cartItemId}`);
            if (cartItemElement) {
                cartItemElement.remove();
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

            if (result.cartCount === 0) {
                location.reload();
            }
        } else {
            alert('Error removing item: ' + result.error);
        }
    } catch (error) {
        console.error('Error removing item:', error);
        alert('Error removing item');
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