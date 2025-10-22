// Using global ProductsService injected by product-service.js to avoid cache-mismatch on module imports
const ProductsService = (typeof window !== 'undefined' ? window.ProductsService : undefined);
if (!ProductsService) {
    console.error("ProductsService not found. Ensure product-service.js loads before product-page.js.");
}

document.addEventListener("DOMContentLoaded", async () => {
    const productsContainer = document.getElementById("productsContainer");
    const searchInput = document.getElementById("searchInput");
    const filterButton = document.querySelector(".filter-btn");
    const categoryList = document.getElementById("categoryList");
    const customizeModal = document.getElementById("customizeModal");
    const customizeCloseBtn = document.getElementById("customizeCloseBtn");
    const qtyValueEl = document.getElementById("qtyValue");
    const totalPriceEl = document.getElementById("totalPrice");
    const addToCartBtn = document.getElementById("addToCartBtn");
    const addonsContainer = document.getElementById("addonsContainer");

    let currentProduct = null;
    let allProducts = [];
    let currentCategoryProducts = [];
    let currentSort = "default";
    let currentCategory = "All";
    let allCustomizations = [];

    // Expose currentProduct globally for cart functionality
    window.currentProduct = null;

    async function renderCategories() {
        if (!categoryList) return;

        // Change "All Drinks" to "Drinks" but keep data-category="All" for functionality
        const allItem = document.createElement("li");
        allItem.innerHTML = '<a href="#" class="category-link" data-category="All">Drinks</a>';
        categoryList.appendChild(allItem);

        const result = await ProductsService.getAllCategories();
        if (!result.success || !Array.isArray(result.data)) {
            return; // silently keep just All if categories fail to load
        }

        // Expecting items with Name/Status from service; normalize name
        const categories = result.data
            .filter(c => (c.status?.toLowerCase?.() ?? c.Status?.toLowerCase?.() ?? "active") === "active")
            .sort((a, b) => {
                const ao = (a.displayOrder ?? a.DisplayOrder ?? 0);
                const bo = (b.displayOrder ?? b.DisplayOrder ?? 0);
                return ao - bo;
            });

        categories.forEach(cat => {
            const name = cat.name ?? cat.Name ?? "Unnamed";
            const li = document.createElement("li");
            li.innerHTML = `<a href="#" class="category-link" data-category="${name}">${name}</a>`;
            categoryList.appendChild(li);
        });

        // Wire events after rendering
        wireCategoryClickHandlers();

        // Set initial active state
        const initial = categoryList.querySelector('.category-link[data-category="All"]');
        initial?.classList.add('active');
    }

    function wireCategoryClickHandlers() {
        categoryList?.querySelectorAll('.category-link').forEach(link => {
            link.addEventListener('click', async (e) => {
                e.preventDefault();
                currentCategory = link.getAttribute('data-category') || 'All';

                // Update active state
                categoryList.querySelectorAll('.category-link').forEach(l => l.classList.remove('active'));
                link.classList.add('active');

                // Load products for the selected category
                await loadProductsByCategory(currentCategory);
            });
        });
    }

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
            card.querySelector(".add-btn").addEventListener("click", async () => {
                await openCustomize(product);
            });
        });
    }

    async function loadProductsByCategory(categoryName) {
        productsContainer.innerHTML = "<p>Loading products...</p>";

        console.log("Loading products for category:", categoryName);

        let result;
        if (categoryName === "All") {
            result = await ProductsService.getAllProducts();
        } else {
            result = await ProductsService.getProductsByCategory(categoryName);
        }

        console.log("Result for category", categoryName, ":", result);

        if (result.success) {
            currentCategoryProducts = result.data.filter(p => p.isActive);
            console.log("Filtered products:", currentCategoryProducts);
            renderProducts(currentCategoryProducts);
        } else {
            productsContainer.innerHTML = `<p class="error">Error loading products: ${result.error}</p>`;
        }
    }

    async function loadProducts() {
        productsContainer.innerHTML = "<p>Loading products...</p>";
        const result = await ProductsService.getAllProducts();

        if (result.success) {
            allProducts = result.data.filter(p => p.isActive);
            currentCategoryProducts = allProducts; // Set as current category products for "All"
            renderProducts(allProducts);
        } else {
            productsContainer.innerHTML = `<p class="error">Error loading products: ${result.error}</p>`;
        }
    }

    function applyFilters() {
        // Use current category products if we're filtering by category, otherwise use all products
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

    // Build category list from API and wire filtering
    await renderCategories();
    await loadCustomizations();

    // Modal behaviors
    async function openCustomize(product) {
        console.log('Opening customize modal for product:', product);

        if (!product) {
            console.error('No product provided to openCustomize');
            return;
        }

        currentProduct = product;
        window.currentProduct = product;

        if (!allCustomizations || allCustomizations.length === 0) {
            await loadCustomizations();
        }

        // Reset quantity
        if (qtyValueEl) qtyValueEl.textContent = '1';

        // Render only customizations linked to this product
        renderCustomizationsForProduct(product);
        recalcTotal();

        if (customizeModal) {
            customizeModal.style.display = 'flex';
            document.body.style.overflow = 'hidden';
        }
    }

    function closeCustomize() {
        if (customizeModal) {
            customizeModal.style.display = 'none';
            document.body.style.overflow = '';
        }
        currentProduct = null;
        window.currentProduct = null;
    }

    customizeCloseBtn?.addEventListener('click', closeCustomize);
    customizeModal?.addEventListener('click', (e) => {
        if (e.target === customizeModal) closeCustomize();
    });

    document.querySelectorAll('.stepper').forEach(stepper => {
        const minus = stepper.querySelector('.stepper-minus');
        const plus = stepper.querySelector('.stepper-plus');
        const valueEl = stepper.querySelector('.stepper-value');
        const type = stepper.getAttribute('data-type');

        if (minus && plus && valueEl) {
            minus.addEventListener('click', () => {
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
        const qty = parseInt(qtyValueEl?.textContent || '1');

        // Dynamic add-ons: sum selected prices
        let addonsDelta = 0;

        // Checkboxes
        addonsContainer?.querySelectorAll('input[type="checkbox"]:checked').forEach(cb => {
            addonsDelta += Number(cb.getAttribute('data-price') || '0');
        });

        // Radio buttons
        addonsContainer?.querySelectorAll('input[type="radio"]:checked').forEach(rb => {
            addonsDelta += Number(rb.getAttribute('data-price') || '0');
        });

        // Dropdown single-selects
        addonsContainer?.querySelectorAll('select.addon-select').forEach(sel => {
            const price = Number(sel.selectedOptions?.[0]?.getAttribute('data-price') || '0');
            addonsDelta += price;
        });

        // Quantity-based addons
        addonsContainer?.querySelectorAll('.stepper[data-type="addon-qty"]').forEach(step => {
            const perUnit = Number(step.getAttribute('data-price-per-unit') || '0');
            const units = Number(step.querySelector('.stepper-value')?.textContent || '0');
            addonsDelta += perUnit * units;
        });

        const total = (base + addonsDelta) * qty;
        if (totalPriceEl) {
            totalPriceEl.textContent = `₱${total.toFixed(2)}`;
        }
    }

    function validateRequiredCustomizations() {
        let valid = true;
        addonsContainer?.querySelectorAll('.addon-section').forEach(section => {
            const title = section.querySelector('.addon-title');
            const isRequired = !!title?.querySelector('.required-badge');
            const body = section.querySelector('.addon-body');
            const validation = body?.querySelector('.addon-validation');

            if (!isRequired) {
                if (validation) {
                    validation.textContent = '';
                    validation.classList.add('hidden');
                }
                return;
            }

            let satisfied = false;
            const qtyStepper = body?.querySelector('.stepper[data-type="addon-qty"] .stepper-value');
            if (qtyStepper) {
                satisfied = parseInt(qtyStepper.textContent || '0') > 0;
            } else if (body?.querySelector('input[type="radio"]')) {
                satisfied = !!body.querySelector('input[type="radio"]:checked');
            } else if (body?.querySelector('input[type="checkbox"]')) {
                satisfied = Array.from(body.querySelectorAll('input[type="checkbox"]')).some(cb => cb.checked);
            } else if (body?.querySelector('select.addon-select')) {
                satisfied = true; // Dropdowns always have a selection
            }

            if (!satisfied) {
                valid = false;
                if (validation) {
                    validation.textContent = 'This option is required.';
                    validation.classList.remove('hidden');
                }
            } else {
                if (validation) {
                    validation.textContent = '';
                    validation.classList.add('hidden');
                }
            }
        });
        return valid;
    }

    function getCustomizationData() {
        const customizations = [];

        addonsContainer?.querySelectorAll('.addon-section').forEach(section => {
            const title = section.querySelector('.addon-title');
            const customizationName = title?.textContent?.replace('Required', '').trim();
            const body = section.querySelector('.addon-body');

            // Quantity stepper
            const qtyStepper = body?.querySelector('.stepper[data-type="addon-qty"]');
            if (qtyStepper) {
                const units = parseInt(qtyStepper.querySelector('.stepper-value')?.textContent || '0');
                if (units > 0) {
                    customizations.push({
                        name: customizationName,
                        value: `${units}`,
                        type: 'quantity'
                    });
                }
            }

            // Radio buttons (including hidden ones for temperature)
            const checkedRadio = body?.querySelector('input[type="radio"]:checked');
            if (checkedRadio) {
                const label = body.querySelector(`label[for="${checkedRadio.id}"]`)?.textContent ||
                    body.querySelector(`.temp-btn.active span`)?.textContent || 'Selected';
                customizations.push({
                    name: customizationName,
                    value: label.trim(),
                    type: 'single_select'
                });
            }

            // Checkboxes
            const checkedBoxes = body?.querySelectorAll('input[type="checkbox"]:checked');
            if (checkedBoxes && checkedBoxes.length > 0) {
                const values = Array.from(checkedBoxes).map(cb => {
                    const label = body.querySelector(`label[for="${cb.id}"]`);
                    return label?.querySelector('.option-name')?.textContent || label?.textContent || 'Selected';
                });
                customizations.push({
                    name: customizationName,
                    value: values.join(', '),
                    type: 'multi_select'
                });
            }

            // Dropdown selects
            const select = body?.querySelector('select.addon-select');
            if (select && select.selectedOptions && select.selectedOptions[0]) {
                customizations.push({
                    name: customizationName,
                    value: select.selectedOptions[0].textContent.trim(),
                    type: 'single_select'
                });
            }
        });

        const qtyValueEl = document.getElementById('qtyValue');
        const quantity = qtyValueEl ? parseInt(qtyValueEl.textContent) : 1;

        return {
            customizations,
            quantity
        };
    }

    function calculateTotalPrice() {
        if (!currentProduct) return 0;

        const base = Number(currentProduct.price) || 0;
        const qty = parseInt(qtyValueEl?.textContent || '1');
        let addonsDelta = 0;

        addonsContainer?.querySelectorAll('input[type="checkbox"]:checked').forEach(cb => {
            addonsDelta += Number(cb.getAttribute('data-price') || '0');
        });
        addonsContainer?.querySelectorAll('input[type="radio"]:checked').forEach(rb => {
            addonsDelta += Number(rb.getAttribute('data-price') || '0');
        });
        addonsContainer?.querySelectorAll('select.addon-select').forEach(sel => {
            const price = Number(sel.selectedOptions?.[0]?.getAttribute('data-price') || '0');
            addonsDelta += price;
        });
        addonsContainer?.querySelectorAll('.stepper[data-type="addon-qty"]').forEach(step => {
            const perUnit = Number(step.getAttribute('data-price-per-unit') || '0');
            const units = Number(step.querySelector('.stepper-value')?.textContent || '0');
            addonsDelta += perUnit * units;
        });

        return (base + addonsDelta) * qty;
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
            box-shadow: 0 2px 5px rgba(0,0,0,0.2);
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

            // Validate required customizations first
            if (!validateRequiredCustomizations()) {
                const firstError = addonsContainer?.querySelector('.addon-validation:not(.hidden)');
                firstError?.scrollIntoView({ behavior: 'smooth', block: 'center' });
                return;
            }

            if (!window.currentProduct) {
                console.error('No product selected');
                showNotification('Please select a product first', 'error');
                return;
            }

            try {
                const customizationData = getCustomizationData();
                console.log('Customization data:', customizationData);

                const totalPrice = calculateTotalPrice();
                console.log('Total price:', totalPrice);

                const cartItem = {
                    productId: window.currentProduct.id,
                    name: window.currentProduct.name,
                    description: window.currentProduct.description,
                    unitPrice: totalPrice / customizationData.quantity,
                    quantity: customizationData.quantity,
                    imageUrl: window.currentProduct.imageUrl,
                    customizations: customizationData.customizations
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
                    closeCustomize();
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

    async function loadCustomizations() {
        const result = await ProductsService.getAllCustomizations();
        if (!result.success) return;
        allCustomizations = result.data
            .slice()
            .sort((a, b) => (a.displayOrder ?? 0) - (b.displayOrder ?? 0));
    }

    function renderCustomizationsForProduct(product) {
        if (!addonsContainer) return;

        if (!product || !Array.isArray(product.customizations)) {
            // If no mapping, show none
            addonsContainer.innerHTML = '';
            return;
        }

        const allowedIds = new Set(product.customizations);
        const filtered = allCustomizations.filter(cz => allowedIds.has(cz.id));

        addonsContainer.innerHTML = '';

        filtered.forEach(cz => {
            const type = (cz.type || '').toLowerCase();
            const section = document.createElement('div');
            section.className = 'addon-section';

            const title = document.createElement('div');
            title.className = 'addon-title';
            title.innerHTML = `${cz.name || 'Addon'}${cz.required ? ' <span class="required-badge">Required</span>' : ''}`;
            section.appendChild(title);

            const body = document.createElement('div');
            body.className = 'addon-body';

            if (type === 'quantity') {
                const maxQ = Number(cz.maxQuantity ?? 5);
                const pricePerUnit = Number(cz.pricePerUnit ?? 0);
                const wrapper = document.createElement('div');
                wrapper.className = 'stepper';
                wrapper.setAttribute('data-type', 'addon-qty');
                wrapper.setAttribute('data-price-per-unit', String(pricePerUnit));
                wrapper.innerHTML = `
                    <button class="stepper-minus" aria-label="Decrease">−</button>
                    <span class="stepper-value">0</span>
                    <button class="stepper-plus" aria-label="Increase">+</button>
                    <span class="addon-price-note">(+₱${pricePerUnit.toFixed(2)} each)</span>
                `;
                const minus = wrapper.querySelector('.stepper-minus');
                const plus = wrapper.querySelector('.stepper-plus');
                const valueEl = wrapper.querySelector('.stepper-value');
                minus.addEventListener('click', () => {
                    const val = Math.max(0, parseInt(valueEl.textContent || '0') - 1);
                    valueEl.textContent = String(val);
                    recalcTotal();
                });
                plus.addEventListener('click', () => {
                    const val = Math.min(maxQ, parseInt(valueEl.textContent || '0') + 1);
                    valueEl.textContent = String(val);
                    recalcTotal();
                });
                body.appendChild(wrapper);
            } else if (type === 'single_select') {
                const isTemperature = (cz.name || '').trim().toLowerCase() === 'temperature';
                let anyChecked = false;

                if (isTemperature) {
                    // Render as icon toggle buttons (Hot/Cold style)
                    const toggle = document.createElement('div');
                    toggle.className = 'temp-toggle';
                    toggle.setAttribute('role', 'tablist');

                    (cz.options || []).forEach((opt, idx) => {
                        const id = `cz_${cz.id}_${idx}`;
                        // hidden radio for pricing and form-like semantics
                        const hidden = document.createElement('input');
                        hidden.type = 'radio';
                        hidden.name = `cz_${cz.id}`;
                        hidden.id = id;
                        hidden.setAttribute('data-price', String(Number(opt.priceModifier || 0)));
                        hidden.style.display = 'none';
                        if (opt.default) { hidden.checked = true; anyChecked = true; }
                        hidden.addEventListener('change', recalcTotal);
                        body.appendChild(hidden);

                        const btn = document.createElement('button');
                        btn.className = 'temp-btn' + (hidden.checked ? ' active' : '');
                        btn.setAttribute('aria-pressed', hidden.checked ? 'true' : 'false');
                        btn.setAttribute('data-for', id);
                        const isCold = (opt.name || '').trim().toLowerCase() === 'cold';
                        const iconSrc = isCold ? '/images/cold-hot-icon.png' : '/images/flame-hot-icon.png';
                        btn.innerHTML = `<img src="${iconSrc}" alt="${opt.name}" /><span>${opt.name}</span>`;
                        btn.addEventListener('click', () => {
                            // toggle active
                            toggle.querySelectorAll('.temp-btn').forEach(b => {
                                b.classList.remove('active');
                                b.setAttribute('aria-pressed', 'false');
                            });
                            btn.classList.add('active');
                            btn.setAttribute('aria-pressed', 'true');
                            // check hidden radio and recalc
                            const target = body.querySelector(`#${btn.getAttribute('data-for')}`);
                            if (target) {
                                target.checked = true;
                                target.dispatchEvent(new Event('change'));
                            }
                        });
                        toggle.appendChild(btn);
                    });

                    // If required and no default, select first
                    if (cz.required && !anyChecked) {
                        const firstBtn = toggle.querySelector('.temp-btn');
                        firstBtn?.click();
                    }

                    body.appendChild(toggle);
                } else {
                    // Standard single select as dropdown
                    const select = document.createElement('select');
                    select.className = 'customize-select addon-select';
                    (cz.options || []).forEach((opt, idx) => {
                        const option = document.createElement('option');
                        option.value = String(idx);
                        option.textContent = `${opt.name}${Number(opt.priceModifier || 0) ? ` (₱${Number(opt.priceModifier).toFixed(2)})` : ''}`;
                        option.setAttribute('data-price', String(Number(opt.priceModifier || 0)));
                        if (opt.default) { option.selected = true; anyChecked = true; }
                        select.appendChild(option);
                    });
                    // If required and no default, select first option
                    if (cz.required && !anyChecked && select.options.length > 0) {
                        select.selectedIndex = 0;
                    }
                    select.addEventListener('change', recalcTotal);
                    body.appendChild(select);
                }
            } else if (type === 'multi_select') {
                const optionsGrid = document.createElement('div');
                optionsGrid.className = 'options-grid';
                (cz.options || []).forEach((opt, idx) => {
                    const id = `cz_${cz.id}_${idx}`;
                    const item = document.createElement('div');
                    item.className = 'checkbox-item';
                    item.innerHTML = `
                        <input type="checkbox" id="${id}" data-price="${Number(opt.priceModifier || 0)}">
                        <label for="${id}">
                            <span class="option-name">${opt.name}</span>
                            ${Number(opt.priceModifier || 0) ? `<span class="option-price">(₱${Number(opt.priceModifier).toFixed(2)})</span>` : ''}
                            ${opt.description ? `<div class="option-desc">${opt.description}</div>` : ''}
                        </label>
                    `;
                    item.querySelector('input')?.addEventListener('change', recalcTotal);
                    optionsGrid.appendChild(item);
                });
                if ((cz.options || []).length >= 4) optionsGrid.classList.add('two-cols');
                body.appendChild(optionsGrid);
            }

            // Inline validation placeholder
            const validation = document.createElement('div');
            validation.className = 'addon-validation hidden';
            body.appendChild(validation);

            section.appendChild(body);
            addonsContainer.appendChild(section);
        });
    }
});