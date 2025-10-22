document.addEventListener('DOMContentLoaded', function () {
    console.log('Cart page initialized');

    calculateCartTotals();
});

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

    // Calculate subtotal from cart items
    cartItems.forEach(item => {
        const quantity = parseInt(item.querySelector('.quantity-value').textContent);
        const unitPrice = parseFloat(item.querySelector('.item-price').textContent.replace('₱', ''));
        const itemTotal = quantity * unitPrice;

        subtotal += itemTotal;
    });

    const total = subtotal - 60;

    const subtotalElement = document.getElementById('subtotalAmount');
    const totalElement = document.getElementById('totalAmount');

    if (subtotalElement) {
        subtotalElement.textContent = '₱' + subtotal.toFixed(2);
    }
    if (totalElement) {
        totalElement.textContent = '₱' + total.toFixed(2);
    }
}

async function updateQuantity(productId, action) {
    const quantityElement = document.getElementById(`quantity-${productId}`);
    let quantity = parseInt(quantityElement.textContent);

    if (action === 'increase') {
        quantity++;
    } else if (action === 'decrease' && quantity > 1) {
        quantity--;
    } else {
        return;
    }

    try {
        const response = await fetch('/Cart/UpdateQuantity', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify({ productId: productId, quantity: quantity })
        });

        const result = await response.json();

        if (result.success) {
            quantityElement.textContent = quantity;

            // Safely update elements only if they exist
            const itemTotalElement = document.getElementById(`total-${productId}`);
            const subtotalElement = document.getElementById('subtotalAmount');
            const totalElement = document.getElementById('totalAmount');

            if (itemTotalElement) {
                itemTotalElement.textContent = `₱ ${result.itemTotal.toFixed(2)}`;
            }
            if (subtotalElement) {
                subtotalElement.textContent = `₱ ${result.subtotal.toFixed(2)}`;
            }
            if (totalElement) {
                totalElement.textContent = `₱ ${result.total.toFixed(2)}`;
            }

            updateCartSummary(result.cartCount);
        } else {
            alert('Error updating quantity: ' + result.error);
        }
    } catch (error) {
        console.error('Error updating quantity:', error);
        alert('Error updating quantity');
    }
}

async function removeFromCart(productId) {
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
            body: JSON.stringify({ productId: productId })
        });

        const result = await response.json();

        if (result.success) {
            document.getElementById(`cartItem-${productId}`).remove();

            // Safely update elements only if they exist
            const subtotalElement = document.getElementById('subtotalAmount');
            const totalElement = document.getElementById('totalAmount');

            if (subtotalElement) {
                subtotalElement.textContent = `₱ ${result.subtotal.toFixed(2)}`;
            }
            if (totalElement) {
                totalElement.textContent = `₱ ${result.total.toFixed(2)}`;
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
        cartSummary.textContent = `${count} item(s) in your cart.`;
    }
}