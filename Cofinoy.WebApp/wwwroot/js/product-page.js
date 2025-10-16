import { ProductsService } from "./product-service.js";

document.addEventListener("DOMContentLoaded", async () => {
    const productsContainer = document.getElementById("productsContainer");
    const searchInput = document.getElementById("searchInput");
    const filterButton = document.querySelector(".filter-btn");
    const categoryList = document.getElementById("categoryList");
    const customizeModal = document.getElementById("customizeModal");
    const customizeCloseBtn = document.getElementById("customizeCloseBtn");
    const sizeSelect = document.getElementById("sizeSelect");
    const milkSelect = document.getElementById("milkSelect");
    const sweetnessSelect = document.getElementById("sweetnessSelect");
    const extrasValueEl = document.getElementById("extrasValue");
    const qtyValueEl = document.getElementById("qtyValue");
    const totalPriceEl = document.getElementById("totalPrice");
    const addToCartBtn = document.getElementById("addToCartBtn");
    let currentProduct = null;
    let allProducts = [];
    let currentCategoryProducts = []; // Store products for current category
    let currentSort = "default";
    let currentCategory = "All";
    async function renderCategories() {
        if (!categoryList) return;

        // Keep heading item, clear the rest
        categoryList.innerHTML = "<li><strong>Drinks</strong></li>";

        // Always include an All category at the top
        const allItem = document.createElement("li");
        allItem.innerHTML = '<a href="#" class="category-link" data-category="All">All Drinks</a>';
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
            card.querySelector(".add-btn").addEventListener("click", () => {
                openCustomize(product);
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

    // Modal behaviors
    function openCustomize(product) {
        currentProduct = product;
        // reset defaults
        document.querySelectorAll('.temp-btn').forEach((b,i)=>{
            b.classList.toggle('active', i===0);
            b.setAttribute('aria-pressed', i===0 ? 'true' : 'false');
        });
        sizeSelect.value = 'Medium';
        milkSelect.value = 'Almond';
        sweetnessSelect.value = '75%';
        extrasValueEl.textContent = '1';
        qtyValueEl.textContent = '2';
        recalcTotal();
        customizeModal.style.display = 'flex';
        document.body.style.overflow = 'hidden';
    }

    function closeCustomize() {
        customizeModal.style.display = 'none';
        document.body.style.overflow = '';
        currentProduct = null;
    }

    customizeCloseBtn?.addEventListener('click', closeCustomize);
    customizeModal?.addEventListener('click', (e)=>{
        if (e.target === customizeModal) closeCustomize();
    });

    document.querySelectorAll('.temp-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            document.querySelectorAll('.temp-btn').forEach(b => {
                b.classList.toggle('active', b === btn);
                b.setAttribute('aria-pressed', b === btn ? 'true' : 'false');
            });
        });
    });

    [sizeSelect, milkSelect, sweetnessSelect].forEach(el => {
        el?.addEventListener('change', recalcTotal);
    });

    document.querySelectorAll('.stepper').forEach(stepper => {
        const minus = stepper.querySelector('.stepper-minus');
        const plus = stepper.querySelector('.stepper-plus');
        const valueEl = stepper.querySelector('.stepper-value');
        const type = stepper.getAttribute('data-type');
        minus?.addEventListener('click', () => {
            const val = Math.max( type === 'quantity' ? 1 : 0, parseInt(valueEl.textContent || '0') - 1 );
            valueEl.textContent = String(val);
            recalcTotal();
        });
        plus?.addEventListener('click', () => {
            const val = Math.min(99, parseInt(valueEl.textContent || '0') + 1);
            valueEl.textContent = String(val);
            recalcTotal();
        });
    });

    function recalcTotal() {
        if (!currentProduct) return;
        const base = Number(currentProduct.price) || 0;
        const sizeDelta = Number(sizeSelect?.selectedOptions?.[0]?.dataset?.delta || 0);
        const milkDelta = Number(milkSelect?.selectedOptions?.[0]?.dataset?.delta || 0);
        const extras = parseInt(extrasValueEl?.textContent || '0');
        const qty = parseInt(qtyValueEl?.textContent || '1');
        const extrasDelta = extras * 20;
        const total = (base + sizeDelta + milkDelta + extrasDelta) * qty;
        totalPriceEl.textContent = `₱${total.toFixed(2)}`;
    }

    addToCartBtn?.addEventListener('click', () => {
        // Placeholder: wire to cart later
        closeCustomize();
    });
});