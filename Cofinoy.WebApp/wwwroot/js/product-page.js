import { ProductsService } from "./product-service.js";

document.addEventListener("DOMContentLoaded", async () => {
    const productsContainer = document.getElementById("productsContainer");
    const searchInput = document.getElementById("searchInput");
    const filterButton = document.querySelector(".filter-btn");
    const customizeModal = document.getElementById("customizeModal");
    const customizeCloseBtn = document.getElementById("customizeCloseBtn");
    const addToCartBtn = document.getElementById("addToCartBtn");

    let currentProduct = null;
    let allProducts = [];
    let currentCategoryProducts = [];
    let currentSort = "default";
    let currentCategory = "All";

  
    window.currentProduct = null;

    function renderProducts(products) {
        productsContainer.innerHTML = "";

        if (!products.length) {
            productsContainer.innerHTML = "<p>No products found.</p>";
            return;
        }

        products.forEach(product => {
            const card = document.createElement("div");
            card.className = "product-card";
            card.innerHTML = `
                <img src="${product.imageUrl}" alt="${product.name}" />
                <div class="product-info">
                    <h3>${product.name}</h3>
                    <p>${product.description}</p>
                    <div class="price-row">
                        <span class="price">₱${product.price.toFixed(2)}</span>
                        <button class="add-btn">+</button>
                    </div>
                </div>
            `;
            productsContainer.appendChild(card);

            // open customize modal
            card.querySelector(".add-btn").addEventListener("click", () => {
                openCustomize(product);
            });
        });
    }

    async function loadProductsByCategory(categoryName) {
        productsContainer.innerHTML = "<p>Loading products...</p>";

        let categoryResult;
        if (categoryName === "All") {
            categoryResult = await ProductsService.getAllProducts();
        } else {
            categoryResult = await ProductsService.getProductsByCategory(categoryName);
        }

        if (categoryResult.success) {
            currentCategoryProducts = categoryResult.data.filter(p => p.isActive);
            renderProducts(currentCategoryProducts);
        } else {
            productsContainer.innerHTML = `<p class="error">Error loading products: ${categoryResult.error}</p>`;
        }
    }

    async function loadProducts() {
        productsContainer.innerHTML = "<p>Loading products...</p>";
        const productsResult = await ProductsService.getAllProducts();

        if (productsResult.success) {
            allProducts = productsResult.data.filter(p => p.isActive);
            currentCategoryProducts = allProducts;
            renderProducts(allProducts);
        } else {
            productsContainer.innerHTML = `<p class="error">Error loading products: ${productsResult.error}</p>`;
        }
    }

    function applyFilters() {
        let baseProducts = currentCategory === "All" ? allProducts : currentCategoryProducts;
        let filtered = [...baseProducts];
        const query = searchInput.value.toLowerCase();

        if (query) {
            filtered = filtered.filter(p =>
                p.name.toLowerCase().includes(query) ||
                p.description.toLowerCase().includes(query)
            );
        }

        if (currentSort === "lowToHigh") {
            filtered.sort((a, b) => a.price - b.price);
        } else if (currentSort === "highToLow") {
            filtered.sort((a, b) => b.price - a.price);
        }

        renderProducts(filtered);
    }

    searchInput.addEventListener("input", applyFilters);

    filterButton.addEventListener("click", () => {
        if (currentSort === "default") {
            currentSort = "lowToHigh";
            filterButton.querySelector("img").style.filter = "hue-rotate(120deg)";
            filterButton.querySelector("span")?.remove();
            filterButton.insertAdjacentHTML("beforeend", "<span> Low → High</span>");
        } else if (currentSort === "lowToHigh") {
            currentSort = "highToLow";
            filterButton.querySelector("img").style.filter = "hue-rotate(60deg)";
            filterButton.querySelector("span")?.remove();
            filterButton.insertAdjacentHTML("beforeend", "<span> High → Low</span>");
        } else {
            currentSort = "default";
            filterButton.querySelector("img").style.filter = "none";
            filterButton.querySelector("span")?.remove();
            filterButton.insertAdjacentHTML("beforeend", "<span> Default</span>");
        }

        applyFilters();
    });

    await loadProducts();

 
    document.querySelectorAll('.category-link').forEach(link => {
        link.addEventListener('click', async (e) => {
            e.preventDefault();
            currentCategory = link.getAttribute('data-category');

            document.querySelectorAll('.category-link').forEach(l => l.classList.remove('active'));
            link.classList.add('active');

            await loadProductsByCategory(currentCategory);
        });
    });

    
    document.querySelector('.category-link[data-category="All"]').classList.add('active');

  
    function openCustomize(product) {
        console.log('Opening customize modal for product:', product);

        if (!product) {
            console.error('No product provided to openCustomize');
            return;
        }

        currentProduct = product;
        window.currentProduct = product;

        // Reset all values safely
        const tempButtons = document.querySelectorAll('.temp-btn');
        if (tempButtons.length > 0) {
            tempButtons.forEach((b, i) => {
                b.classList.toggle('active', i === 0);
                b.setAttribute('aria-pressed', i === 0 ? 'true' : 'false');
            });
        }

        const sizeSelect = document.getElementById('sizeSelect');
        if (sizeSelect) sizeSelect.value = 'Medium';

        const milkSelect = document.getElementById('milkSelect');
        if (milkSelect) milkSelect.value = 'Almond';

        const sweetnessSelect = document.getElementById('sweetnessSelect');
        if (sweetnessSelect) sweetnessSelect.value = '75%';

        const extrasValueEl = document.getElementById('extrasValue');
        if (extrasValueEl) extrasValueEl.textContent = '1';

        const qtyValueEl = document.getElementById('qtyValue');
        if (qtyValueEl) qtyValueEl.textContent = '1';

        recalcTotal();

        if (customizeModal) {
            customizeModal.style.display = 'flex';
            document.body.style.overflow = 'hidden';
        }
    }

    function closeCustomizeModal() {
        if (customizeModal) {
            customizeModal.style.display = 'none';
            document.body.style.overflow = '';
        }
        currentProduct = null;
        window.currentProduct = null;
    }

    if (customizeCloseBtn) {
        customizeCloseBtn.addEventListener('click', closeCustomizeModal);
    }

    if (customizeModal) {
        customizeModal.addEventListener('click', (e) => {
            if (e.target === customizeModal) closeCustomizeModal();
        });
    }

   
    document.querySelectorAll('.temp-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            document.querySelectorAll('.temp-btn').forEach(b => {
                b.classList.toggle('active', b === btn);
                b.setAttribute('aria-pressed', b === btn ? 'true' : 'false');
            });
            recalcTotal();
        });
    });

    
    ['sizeSelect', 'milkSelect', 'sweetnessSelect'].forEach(id => {
        const element = document.getElementById(id);
        if (element) {
            element.addEventListener('change', recalcTotal);
        }
    });

    
    document.querySelectorAll('.stepper').forEach(stepper => {
        const minus = stepper.querySelector('.stepper-minus');
        const plus = stepper.querySelector('.stepper-plus');
        const valueEl = stepper.querySelector('.stepper-value');

        if (minus && plus && valueEl) {
            minus.addEventListener('click', () => {
                const type = stepper.getAttribute('data-type');
                const min = type === 'quantity' ? 1 : 0;
                const val = Math.max(min, parseInt(valueEl.textContent || '0') - 1);
                valueEl.textContent = String(val);
                recalcTotal();
            });

            plus.addEventListener('click', () => {
                const val = Math.min(99, parseInt(valueEl.textContent || '0') + 1);
                valueEl.textContent = String(val);
                recalcTotal();
            });
        }
    });

    function recalcTotal() {
        if (!currentProduct) return;

        const base = Number(currentProduct.price) || 0;

        
        let sizeDelta = 0;
        const sizeSelect = document.getElementById('sizeSelect');
        if (sizeSelect && sizeSelect.selectedOptions && sizeSelect.selectedOptions[0]) {
            sizeDelta = Number(sizeSelect.selectedOptions[0].dataset.delta || 0);
        }

      
        let milkDelta = 0;
        const milkSelect = document.getElementById('milkSelect');
        if (milkSelect && milkSelect.selectedOptions && milkSelect.selectedOptions[0]) {
            milkDelta = Number(milkSelect.selectedOptions[0].dataset.delta || 0);
        }

        
        let extras = 0;
        const extrasValueEl = document.getElementById('extrasValue');
        if (extrasValueEl) {
            extras = parseInt(extrasValueEl.textContent || '0');
        }

     
        let qty = 1;
        const qtyValueEl = document.getElementById('qtyValue');
        if (qtyValueEl) {
            qty = parseInt(qtyValueEl.textContent || '1');
        }

        const extrasDelta = extras * 20;
        const total = (base + sizeDelta + milkDelta + extrasDelta) * qty;

        const totalPriceEl = document.getElementById('totalPrice');
        if (totalPriceEl) {
            totalPriceEl.textContent = `₱${total.toFixed(2)}`;
        }
    }

    
    function getCustomizationData() {
     
        const activeTempBtn = document.querySelector('.temp-btn.active');
        const temperature = activeTempBtn ? activeTempBtn.dataset.value : 'Hot';

        const sizeSelect = document.getElementById('sizeSelect');
        const size = sizeSelect ? sizeSelect.value : 'Medium';

        const milkSelect = document.getElementById('milkSelect');
        const milkType = milkSelect ? milkSelect.value : 'Almond';

        const sweetnessSelect = document.getElementById('sweetnessSelect');
        const sweetnessLevel = sweetnessSelect ? sweetnessSelect.value : '75%';

        const extrasValueEl = document.getElementById('extrasValue');
        const extraShots = extrasValueEl ? parseInt(extrasValueEl.textContent) : 1;

        const qtyValueEl = document.getElementById('qtyValue');
        const quantity = qtyValueEl ? parseInt(qtyValueEl.textContent) : 1;

        return {
            temperature,
            size,
            milkType,
            sweetnessLevel,
            extraShots,
            quantity
        };
    }

    function calculateTotalPrice(basePrice, customization) {
        let total = parseFloat(basePrice);

        const sizeSelect = document.getElementById('sizeSelect');
        if (sizeSelect && sizeSelect.selectedOptions && sizeSelect.selectedOptions[0]) {
            const sizeDelta = parseFloat(sizeSelect.selectedOptions[0].dataset.delta) || 0;
            total += sizeDelta;
        }

        const milkSelect = document.getElementById('milkSelect');
        if (milkSelect && milkSelect.selectedOptions && milkSelect.selectedOptions[0]) {
            const milkDelta = parseFloat(milkSelect.selectedOptions[0].dataset.delta) || 0;
            total += milkDelta;
        }


        total += (customization.extraShots * 20);

        return total * customization.quantity;
    }

    function showNotification(message, type) {
        const notification = document.createElement('div');
        notification.className = `notification ${type}`;
        notification.textContent = message;
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 15px 20px;
            background: ${type === 'success' ? '#4CAF50' : '#f44336'};
            color: white;
            border-radius: 5px;
            z-index: 10000;
        `;

        document.body.appendChild(notification);

        setTimeout(() => {
            notification.remove();
        }, 3000);
    }

    function updateCartCount(count) {
        const cartCountElement = document.querySelector('.cart-count');
        if (cartCountElement) {
            cartCountElement.textContent = count;
        }
    }

    if (addToCartBtn) {
        addToCartBtn.addEventListener('click', async function () {
            console.log('Add to cart button clicked');

            if (!window.currentProduct) {
                console.error('No product selected');
                showNotification('Please select a product first', 'error');
                return;
            }

            try {
                const customizationData = getCustomizationData();
                console.log('Customization data:', customizationData);

                const totalPrice = calculateTotalPrice(window.currentProduct.price, customizationData);
                console.log('Total price:', totalPrice);

                const cartItem = {
                    productId: window.currentProduct.id,
                    name: window.currentProduct.name,
                    description: window.currentProduct.description,
                    unitPrice: totalPrice / customizationData.quantity,
                    quantity: customizationData.quantity,
                    imageUrl: window.currentProduct.imageUrl,
                    size: customizationData.size,
                    milkType: customizationData.milkType,
                    temperature: customizationData.temperature,
                    extraShots: customizationData.extraShots,
                    sweetnessLevel: customizationData.sweetnessLevel
                };

                console.log('Sending cart item:', cartItem);

                const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
                if (!tokenElement) {
                    throw new Error('Anti-forgery token not found');
                }

      
                const response = await fetch('/Cart/AddToCart', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': tokenElement.value
                    },
                    body: JSON.stringify(cartItem),
                    credentials: 'include' 
                });

                console.log('Raw response status:', response.status);
                console.log('Raw response ok:', response.ok);

                const responseText = await response.text();
                console.log('Raw response text:', responseText);

               
                let result;
                try {
                    result = JSON.parse(responseText);
                    console.log('Parsed server response:', result);
                } catch (e) {
                    console.error('Failed to parse response as JSON:', e);
                    throw new Error('Invalid response from server');
                }

                if (result.success) {
                    showNotification('Item added to cart!', 'success');
                    closeCustomizeModal();
                    if (result.cartCount !== undefined) {
                        updateCartCount(result.cartCount);
                    }
                } else {
                    showNotification('Failed to add item to cart: ' + result.error, 'error');
                }
            } catch (error) {
                console.error('Error adding to cart:', error);
                showNotification('Error adding item to cart: ' + error.message, 'error');
            }
        });
    }
});