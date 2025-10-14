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
            document.getElementById(`total-${productId}`).textContent = `₱ ${result.itemTotal.toFixed(2)}`;
            document.getElementById('subtotalAmount').textContent = `₱ ${result.subtotal.toFixed(2)}`;
            document.getElementById('totalAmount').textContent = `₱ ${result.total.toFixed(2)}`;
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
            document.getElementById('subtotalAmount').textContent = `₱ ${result.subtotal.toFixed(2)}`;
            document.getElementById('totalAmount').textContent = `₱ ${result.total.toFixed(2)}`;
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
    return document.querySelector('input[name="__RequestVerificationToken"]').value;
}

function updateCartSummary(count) {
    const cartSummary = document.getElementById('cartSummary');
    if (cartSummary) {
        cartSummary.textContent = `${count} item(s) in your cart.`;
    }
}