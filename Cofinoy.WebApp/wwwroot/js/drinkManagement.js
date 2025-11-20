const FirebaseStorageService = {
    async uploadImage(file, path) {
        try {
            const storageRef = window.firebaseStorage.ref(window.firebaseStorage.getStorage(), path);

            const uploadTask = await window.firebaseStorage.uploadBytes(storageRef, file);

            const downloadURL = await window.firebaseStorage.getDownloadURL(uploadTask.ref);

            return {
                success: true,
                url: downloadURL,
                path: path
            };
        } catch (error) {
            console.error('Error uploading image:', error);
            return {
                success: false,
                error: error.message
            };
        }
    },

    async deleteImage(path) {
        try {
            const storageRef = window.firebaseStorage.ref(window.firebaseStorage.getStorage(), path);
            await window.firebaseStorage.deleteObject(storageRef);

            return {
                success: true
            };
        } catch (error) {
            console.error('Error deleting image:', error);
            return {
                success: false,
                error: error.message
            };
        }
    },

    generateImagePath(drinkName, fileExtension) {
        const timestamp = Date.now();
        const sanitizedName = drinkName.toLowerCase()
            .replace(/[^a-z0-9]/g, '_')
            .replace(/_+/g, '_')
            .substring(0, 50);
        return `drinks/${sanitizedName}_${timestamp}.${fileExtension}`;
    }
};

function updateNoDrinksMessage() {
    const drinksGrid = document.querySelector('.drinks-grid');
    const loadingProgress = document.getElementById('loadingProgress');

    // Hide loading progress
    if (loadingProgress) {
        loadingProgress.style.display = 'none';
    }

    const existingMessage = drinksGrid.querySelector('.no-drinks-message');
    if (existingMessage) {
        existingMessage.remove();
    }

    const noDrinksMessage = document.createElement('div');
    noDrinksMessage.className = 'no-drinks-message';
    noDrinksMessage.innerHTML = `
        <div class="no-drinks-icon"></div>
        <div class="no-drinks-text">No drinks yet</div>
        <div class="no-drinks-subtext">Loading drinks...</div>
    `;

    if (drinksGrid.children.length === 0) {
        drinksGrid.appendChild(noDrinksMessage);
        noDrinksMessage.style.display = 'block';
    }
}

const DrinkAPIService = {
    async addDrink(drinkData) {
        try {
            const response = await fetch('/Menu/AddProduct', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    name: drinkData.name,
                    price: drinkData.price,
                    description: drinkData.description || '',
                    status: drinkData.status || 'Available',
                    stock: drinkData.stock || '0',
                    imageUrl: drinkData.imageUrl,
                    imagePath: drinkData.imagePath,
                    categories: drinkData.categories || [],
                    customizations: drinkData.customizations || [],
                    displayOrder: drinkData.displayOrder || 0,
                    isActive: true
                })
            });

            const result = await response.json();
            return result;
        } catch (error) {
            console.error('Error adding drink:', error);
            return {
                success: false,
                error: error.message
            };
        }
    },

    async updateDrink(drinkId, drinkData) {
        try {
            const response = await fetch(`/Menu/UpdateProduct?id=${drinkId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    name: drinkData.name,
                    price: drinkData.price,
                    description: drinkData.description || '',
                    status: drinkData.status || 'Available',
                    stock: drinkData.stock || '0',
                    imageUrl: drinkData.imageUrl,
                    imagePath: drinkData.imagePath,
                    categories: drinkData.categories || [],
                    customizations: drinkData.customizations || [],
                    displayOrder: drinkData.displayOrder || 0,
                    isActive: true
                })
            });

            const result = await response.json();
            return result;
        } catch (error) {
            console.error('Error updating drink:', error);
            return {
                success: false,
                error: error.message
            };
        }
    },

    async deleteDrink(drinkId) {
        try {
            const response = await fetch(`/Menu/DeleteProduct?id=${drinkId}`, {
                method: 'POST'
            });

            const result = await response.json();
            return result;
        } catch (error) {
            console.error('Error deleting drink:', error);
            return {
                success: false,
                error: error.message
            };
        }
    },

    async getAllDrinks() {
        try {
            const response = await fetch('/Menu/GetAllProducts');
            const result = await response.json();
            return result;
        } catch (error) {
            console.error('Error getting drinks:', error);
            return {
                success: false,
                error: error.message
            };
        }
    },

    async getAllCategories() {
        try {
            const response = await fetch('/Menu/GetAllCategories');
            const result = await response.json();
            return result;
        } catch (error) {
            console.error('Error getting categories:', error);
            return {
                success: false,
                error: error.message
            };
        }
    },

    async getAllCustomizations() {
        try {
            const response = await fetch('/Menu/GetAllCustomizations');
            const result = await response.json();
            return result;
        } catch (error) {
            console.error('Error getting customizations:', error);
            return {
                success: false,
                error: error.message
            };
        }
    }
};

document.addEventListener('DOMContentLoaded', function () {
    const modal = document.getElementById('addDrinkModal');
    const addDrinkBtn = document.querySelector('.add-drink-btn');
    const closeModalBtn = document.getElementById('closeModal');
    const cancelBtn = document.getElementById('cancelBtn');
    const form = document.getElementById('addDrinkForm');
    const searchInput = document.querySelector('.search-input');
    const imageUploadArea = document.querySelector('.image-upload-area');
    const drinkImageInput = document.getElementById('drinkImage');

    const filterBtn = document.querySelector('.btn-secondary');
    const filterModal = document.getElementById('filterModal');
    const closeFilterModalBtn = document.getElementById('closeFilterModal');
    const typeFilter = document.getElementById('typeFilter');
    const requiredFilter = document.getElementById('requiredFilter');
    const clearFiltersBtn = document.querySelector('.clear-filters-btn');
    const applyFiltersBtn = document.getElementById('apply-filters-btn');

    const modalTitle = document.getElementById('drinkModalTitle');
    const addDrinkBtnModal = document.getElementById('addDrinkBtn');

    let currentEditingId = null;
    let drinksListener = null;
    let isSubmitting = false;
    let allDrinks = [];
    let allCategories = [];
    let allCustomizations = [];
    let currentDeletingCard = null;
    let currentDeletingDrink = null;

    initializeApp();

    async function initializeApp() {
        try {
            const loadingProgress = document.getElementById('loadingProgress');
            const drinksCountElement = document.querySelector('.drinks-count');

            if (loadingProgress) {
                loadingProgress.style.display = 'flex';
            }

            // Set loading text
            if (drinksCountElement) {
                drinksCountElement.textContent = 'Loading drinks...';
            }

            await loadCategoriesFromAPI();
            await loadCustomizationsFromAPI();

            console.log('Categories loaded:', allCategories.length, allCategories);
            console.log('Customizations loaded:', allCustomizations.length, allCustomizations);

            await loadDrinksFromAPI();

            populateFilterCategories();
            setupEventListeners();

            if (loadingProgress) {
                loadingProgress.style.display = 'none';
            }

            setInterval(async () => {
                await loadDrinksFromAPI();
            }, 30000);
        } catch (error) {
            console.error('Error initializing app:', error);
            showToastMessage('Error initializing application: ' + error.message, 'Error');

            const loadingProgress = document.getElementById('loadingProgress');
            const drinksCountElement = document.querySelector('.drinks-count');

            if (loadingProgress) {
                loadingProgress.style.display = 'none';
            }

            if (drinksCountElement) {
                drinksCountElement.textContent = '0 drinks in your menu.';
            }
        }
    }

    function setupEventListeners() {
        addDrinkBtn.addEventListener('click', openModal);
        closeModalBtn.addEventListener('click', closeModalFunc);
        cancelBtn.addEventListener('click', closeModalFunc);

        modal.addEventListener('click', function (e) {
            if (e.target === modal) {
                closeModalFunc();
            }
        });

        filterBtn.addEventListener('click', openFilterModal);
        closeFilterModalBtn.addEventListener('click', closeFilterModal);
        clearFiltersBtn.addEventListener('click', clearAllFilters);
        applyFiltersBtn.addEventListener('click', applyFiltersAndClose);
        searchInput.addEventListener('input', applyFilters);

        filterModal.addEventListener('click', function (e) {
            if (e.target === filterModal) {
                closeFilterModal();
            }
        });

        imageUploadArea.addEventListener('click', function () {
            drinkImageInput.click();
        });

        drinkImageInput.addEventListener('change', handleImageUpload);

        form.addEventListener('submit', handleFormSubmit);

        document.addEventListener('keydown', handleKeyboardShortcuts);

        const deleteModal = document.getElementById('deleteConfirmModal');
        const cancelDeleteBtn = document.getElementById('cancelDeleteBtn');
        const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');

        if (deleteModal && cancelDeleteBtn && confirmDeleteBtn) {
            cancelDeleteBtn.addEventListener('click', closeDeleteModal);
            confirmDeleteBtn.addEventListener('click', confirmDelete);

            deleteModal.addEventListener('click', function (e) {
                if (e.target === deleteModal) {
                    closeDeleteModal();
                }
            });
        }

        window.addEventListener('beforeunload', function () {
            if (drinksListener) {
                drinksListener();
            }
        });
    }

    function openModal() {
        modal.classList.add('active');
        document.body.style.overflow = 'hidden';
        populateCategoriesAndCustomizations();
    }

    function closeModalFunc() {
        modal.classList.remove('active');
        document.body.style.overflow = 'auto';
        resetForm();
        resetModalState();
    }

    function openFilterModal() {
        filterModal.classList.add('active');
        document.body.style.overflow = 'hidden';
    }

    function closeFilterModal() {
        filterModal.classList.remove('active');
        document.body.style.overflow = 'auto';
    }

    function resetForm() {
        form.reset();
        document.getElementById('drinkDescription').value = '';
        resetImageUpload();
    }

    function resetModalState() {
        modalTitle.textContent = 'Add new drink';
        addDrinkBtnModal.innerHTML = '<i class="fas fa-plus"></i> Add Drink';
        currentEditingId = null;
    }

    function resetImageUpload() {
        const uploadPlaceholder = document.querySelector('.upload-placeholder');
        uploadPlaceholder.innerHTML = `
            <i class="fas fa-plus"></i>
            <p>Click to upload or drag a file</p>
        `;
    }

    function validateForm() {
        const drinkName = document.getElementById('drinkName');
        const drinkPrice = document.getElementById('drinkPrice');
        const drinkImage = document.getElementById('drinkImage');
        const drinkDescription = document.getElementById('drinkDescription');

        let isValid = true;
        let firstError = null;

        document.querySelectorAll('.error').forEach(el => el.classList.remove('error'));

        if (!drinkName.value.trim()) {
            drinkName.classList.add('error');
            if (!firstError) firstError = drinkName;
            isValid = false;
        }

        if (!drinkPrice.value || drinkPrice.value <= 0) {
            drinkPrice.classList.add('error');
            if (!firstError) firstError = drinkPrice;
            isValid = false;
        }

        if (!drinkDescription.value.trim()) {
            drinkDescription.classList.add('error');
            if (!firstError) firstError = drinkDescription;
            isValid = false;
        }

        if (!currentEditingId && !drinkImage.files[0]) {
            const imageUploadArea = document.querySelector('.image-upload-area');
            imageUploadArea.classList.add('error');
            if (!firstError) firstError = imageUploadArea;
            isValid = false;

            let errorMsg = imageUploadArea.querySelector('.error-message');
            if (!errorMsg) {
                errorMsg = document.createElement('div');
                errorMsg.className = 'error-message';
                errorMsg.style.color = '#dc3545';
                errorMsg.style.fontSize = '0.875rem';
                errorMsg.style.marginTop = '5px';
                imageUploadArea.appendChild(errorMsg);
            }
            errorMsg.textContent = 'Please select an image for the drink.';
        } else {
            const imageUploadArea = document.querySelector('.image-upload-area');
            imageUploadArea.classList.remove('error');
            const errorMsg = imageUploadArea.querySelector('.error-message');
            if (errorMsg) errorMsg.remove();
        }

        if (firstError) {
            firstError.focus();
            firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }

        return isValid;
    }

    function handleImageUpload(e) {
        const file = e.target.files[0];
        if (file) {
            const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
            if (!allowedTypes.includes(file.type)) {
                showToastMessage('Please select a valid image file (JPG, PNG, GIF, or WebP).', 'Invalid File Type');
                e.target.value = '';
                return;
            }

            const maxSize = 5 * 1024 * 1024;
            if (file.size > maxSize) {
                showToastMessage('Please select an image smaller than 5MB.', 'File Too Large');
                e.target.value = '';
                return;
            }

            const reader = new FileReader();
            reader.onload = function (e) {
                const uploadPlaceholder = document.querySelector('.upload-placeholder');
                uploadPlaceholder.innerHTML = `
                    <img src="${e.target.result}" alt="Preview" style="max-width: 100px; max-height: 100px; border-radius: 5px;">
                    <p style="margin-top: 10px;">Click to change image</p>
                `;

                const imageUploadArea = document.querySelector('.image-upload-area');
                imageUploadArea.classList.remove('error');
                const errorMsg = imageUploadArea.querySelector('.error-message');
                if (errorMsg) errorMsg.remove();
            };
            reader.readAsDataURL(file);
        }
    }

    function populateCategoriesAndCustomizations() {
        console.log('=== DEBUG: populateCategoriesAndCustomizations called ===');

        console.log('Categories data:', allCategories);
        const categoriesGrid = document.querySelector('.checkbox-grid');
        console.log('Categories grid element found:', categoriesGrid);

        if (categoriesGrid && allCategories.length > 0) {
            console.log('Replacing categories HTML...');
            categoriesGrid.innerHTML = '';
            allCategories.forEach(category => {
                console.log('Adding category:', category.name);
                const checkboxItem = document.createElement('div');
                checkboxItem.className = 'checkbox-item';
                checkboxItem.innerHTML = `
                <input type="checkbox" id="category_${category.id}" value="${category.id}" data-type="category">
                <label for="category_${category.id}">${category.name}</label>
            `;
                categoriesGrid.appendChild(checkboxItem);
            });
            console.log('Categories replaced successfully');
        } else {
            console.log('Categories NOT replaced. Grid found:', !!categoriesGrid, 'Categories count:', allCategories.length);
        }

        console.log('Customizations data:', allCustomizations);
        const addonsGrid = document.querySelectorAll('.checkbox-grid')[1];
        console.log('Add-ons grid element found:', addonsGrid);

        if (addonsGrid && allCustomizations.length > 0) {
            console.log('Replacing add-ons HTML...');
            addonsGrid.innerHTML = '';
            allCustomizations.forEach(customization => {
                console.log('Adding customization:', customization.name);
                const checkboxItem = document.createElement('div');
                checkboxItem.className = 'checkbox-item';
                checkboxItem.innerHTML = `
                <input type="checkbox" id="customization_${customization.id}" value="${customization.id}" data-type="customization">
                <label for="customization_${customization.id}">${customization.name}</label>
            `;
                addonsGrid.appendChild(checkboxItem);
            });
            console.log('Add-ons replaced successfully');
        } else {
            console.log('Add-ons NOT replaced. Grid found:', !!addonsGrid, 'Customizations count:', allCustomizations.length);
        }
    }

    function clearAllFilters() {
        const statusFilter = document.getElementById('statusFilter');
        const categoryFilter = document.getElementById('categoryFilter');

        if (statusFilter) statusFilter.value = '';
        if (categoryFilter) categoryFilter.value = '';
        searchInput.value = '';
        applyFilters();
    }

    function applyFiltersAndClose() {
        applyFilters();
        closeFilterModal();
    }

    function applyFilters() {
        const searchTerm = searchInput.value.toLowerCase().trim();
        const statusFilter = document.getElementById('statusFilter')?.value || '';
        const categoryFilter = document.getElementById('categoryFilter')?.value || '';

        const allCards = document.querySelectorAll('.drink-card');
        let visibleCount = 0;

        allCards.forEach(card => {
            const firebaseId = card.getAttribute('data-firebase-id');
            const drink = allDrinks.find(d => d.id === firebaseId);

            if (!drink) {
                card.style.display = 'none';
                return;
            }

            const drinkName = drink.name.toLowerCase();

            const matchesSearch = !searchTerm || drinkName.includes(searchTerm);

            const matchesStatus = !statusFilter || drink.status === statusFilter;

            const matchesCategory = !categoryFilter || drink.categories.includes(categoryFilter);

            if (matchesSearch && matchesStatus && matchesCategory) {
                card.style.display = 'block';
                visibleCount++;
            } else {
                card.style.display = 'none';
            }
        });

        updateDrinksCount(visibleCount);

        const drinksGrid = document.querySelector('.drinks-grid');
        const existingMessage = drinksGrid.querySelector('.no-drinks-message');

        if (visibleCount === 0) {
            if (!existingMessage) {
                const noDrinksMessage = document.createElement('div');
                noDrinksMessage.className = 'no-drinks-message';
                noDrinksMessage.innerHTML = `
                    <div class="no-drinks-icon"></div>
                    <div class="no-drinks-text">No drinks found</div>
                    <div class="no-drinks-subtext">Try adjusting your filters</div>
                `;
                noDrinksMessage.style.display = 'block';
                drinksGrid.appendChild(noDrinksMessage);
            }
        } else {
            if (existingMessage) {
                existingMessage.remove();
            }
        }
    }

    async function handleFormSubmit(e) {
        e.preventDefault();

        if (isSubmitting) return;

        if (!validateForm()) {
            showToastMessage('Please fill in all required fields and select an image.', 'Validation Error');
            return;
        }

        isSubmitting = true;

        const originalBtnContent = addDrinkBtnModal.innerHTML;
        addDrinkBtnModal.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Processing...';
        addDrinkBtnModal.disabled = true;

        try {
            const selectedCategories = [];
            const selectedCustomizations = [];

            document.querySelectorAll('input[type="checkbox"]:checked').forEach(checkbox => {
                if (checkbox.dataset.type === 'category') {
                    selectedCategories.push(checkbox.value);
                } else if (checkbox.dataset.type === 'customization') {
                    selectedCustomizations.push(checkbox.value);
                }
            });

            const drinkName = document.getElementById('drinkName').value.trim();
            let imageUrl = '';
            let imagePath = '';
            let oldImagePath = '';

            const imageFile = drinkImageInput.files[0];
            if (imageFile) {
                addDrinkBtnModal.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Uploading image...';

                const fileExtension = imageFile.name.split('.').pop().toLowerCase();
                imagePath = FirebaseStorageService.generateImagePath(drinkName, fileExtension);

                const uploadResult = await FirebaseStorageService.uploadImage(imageFile, imagePath);

                if (uploadResult.success) {
                    imageUrl = uploadResult.url;
                } else {
                    throw new Error('Failed to upload image: ' + uploadResult.error);
                }
            }

            if (currentEditingId) {
                const existingDrink = allDrinks.find(d => d.id === currentEditingId);
                oldImagePath = existingDrink ? existingDrink.imagePath : '';
            }

            const formData = {
                name: drinkName,
                price: parseFloat(document.getElementById('drinkPrice').value),
                status: document.getElementById('drinkStatus').value,
                stock: document.getElementById('drinkStock').value.trim(),
                description: document.getElementById('drinkDescription').value.trim(),
                imageUrl: imageUrl || (currentEditingId ? allDrinks.find(d => d.id === currentEditingId).imageUrl : ''),
                imagePath: imagePath || (currentEditingId ? allDrinks.find(d => d.id === currentEditingId).imagePath : ''),
                categories: selectedCategories,
                customizations: selectedCustomizations,
                displayOrder: currentEditingId ? allDrinks.find(d => d.id === currentEditingId).displayOrder : (allDrinks.length + 1)
            };

            addDrinkBtnModal.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Saving drink...';

            let result;

            if (currentEditingId !== null) {
                // Delete old image if new one was uploaded
                if (imagePath && oldImagePath && oldImagePath !== imagePath) {
                    await FirebaseStorageService.deleteImage(oldImagePath);
                }

                result = await DrinkAPIService.updateDrink(currentEditingId, formData);

                if (result.success) {
                    showToastMessage('Drink updated successfully!');
                } else {
                    throw new Error(result.error || 'Failed to update drink');
                }
            } else {
                result = await DrinkAPIService.addDrink(formData);

                if (result.success) {
                    showToastMessage('Drink added successfully!');
                } else {
                    throw new Error(result.error || 'Failed to add drink');
                }
            }

            await loadDrinksFromAPI();
            closeModalFunc();

        } catch (error) {
            console.error('Error saving drink:', error);
            showToastMessage('Error saving drink: ' + error.message, 'Error');

            if (imagePath && !currentEditingId) {
                try {
                    await FirebaseStorageService.deleteImage(imagePath);
                } catch (cleanupError) {
                    console.error('Error cleaning up uploaded image:', cleanupError);
                }
            }
        } finally {
            addDrinkBtnModal.innerHTML = originalBtnContent;
            addDrinkBtnModal.disabled = false;
            isSubmitting = false;
        }
    }

    async function loadDrinksFromAPI() {
        try {
            const result = await DrinkAPIService.getAllDrinks();

            if (result.success) {
                allDrinks = result.data;
                displayDrinks(result.data);
                updateDrinksCount(result.data.length);
            } else {
                console.error('Error loading drinks:', result.error);
                showToastMessage('Error loading drinks: ' + result.error, 'Error');
                displayDrinks([]);
            }
        } catch (error) {
            console.error('Error loading drinks:', error);
            showToastMessage('Error loading drinks: ' + error.message, 'Error');
            displayDrinks([]);
        } finally {
            const loadingProgress = document.getElementById('loadingProgress');
            if (loadingProgress) {
                loadingProgress.style.display = 'none';
            }
        }
    }

    function loadDrinksFromFirebase() {
        return loadDrinksFromAPI();
    }

    async function loadCategoriesFromAPI() {
        try {
            const result = await DrinkAPIService.getAllCategories();
            if (result.success) {
                allCategories = result.data;
            }
        } catch (error) {
            console.error('Error loading categories:', error);
        }
    }

    async function loadCustomizationsFromAPI() {
        try {
            const result = await DrinkAPIService.getAllCustomizations();
            if (result.success) {
                allCustomizations = result.data;
            }
        } catch (error) {
            console.error('Error loading customizations:', error);
        }
    }

    function populateFilterCategories() {
        const categoryFilter = document.getElementById('categoryFilter');
        if (categoryFilter && allCategories.length > 0) {
            categoryFilter.innerHTML = '<option value="">All Categories</option>';

            allCategories.forEach(category => {
                const option = document.createElement('option');
                option.value = category.id;
                option.textContent = category.name;
                categoryFilter.appendChild(option);
            });
        }
    }

    function displayDrinks(drinks) {
        const drinksGrid = document.querySelector('.drinks-grid');
        const loadingProgress = document.getElementById('loadingProgress');

        // Hide loading progress
        if (loadingProgress) {
            loadingProgress.style.display = 'none';
        }

        drinksGrid.innerHTML = '';

        if (drinks.length === 0) {
            updateNoDrinksMessage();
        } else {
            drinks.forEach(drink => {
                addDrinkToGrid(drink, drink.id);
            });
        }
    }

    function setupRealTimeListener() {
        if (drinksListener) {
            drinksListener();
        }

        // Note: DrinkFirebaseService is not defined in the original code
        // This function might need adjustment based on your Firebase implementation
        console.warn('setupRealTimeListener: DrinkFirebaseService is not defined');
    }

    function addDrinkToGrid(drinkData, firebaseId) {
        const drinksGrid = document.querySelector('.drinks-grid');

        const drinkCard = document.createElement('div');
        drinkCard.className = 'drink-card';
        drinkCard.setAttribute('data-firebase-id', firebaseId);

        let categoryNames = 'No categories';
        if (drinkData.categories && drinkData.categories.length > 0) {
            const names = drinkData.categories.map(catId => {
                const category = allCategories.find(c => c.id === catId);
                if (category) {
                    return category.name;
                } else {
                    console.warn('Category not found for ID:', catId);
                    return null;
                }
            }).filter(name => name !== null);

            if (names.length > 0) {
                categoryNames = names.join(', ');
            }
        }

        const imageUrl = drinkData.imageUrl || '/images/placeholder-drink.png';

        const statusClass = drinkData.status === 'Available' ? 'status-available' : 'status-unavailable';
        const statusText = drinkData.status || 'Available';

        drinkCard.innerHTML = `
            <div class="drink-image">
                <img src="${imageUrl}" alt="${drinkData.name}" onerror="this.src='/images/placeholder-drink.png'">
            </div>
            <div class="drink-info">
                <div class="drink-header">
                    <h3 class="drink-name">${drinkData.name}</h3>
                    <span class="status-badge ${statusClass}">${statusText}</span>
                </div>
               
                <div class="drink-details">
                    <span class="drink-description">${categoryNames}</span>
                    ${drinkData.stock ? `<span class="drink-stock">Stock: ${drinkData.stock}</span>` : ''}
                </div>
                <div class="drink-footer">
                    <span class="drink-price">₱ ${drinkData.price.toFixed(2)}</span>
                    <div class="drink-actions">
                        <button class="action-btn edit-btn" title="Edit">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="action-btn delete-btn" title="Delete">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </div>
            </div>
        `;

        drinksGrid.appendChild(drinkCard);
        addActionButtonListeners(drinkCard, drinkData);
    }

    function editDrink(card, drinkData) {
        const firebaseId = card.getAttribute('data-firebase-id');

        resetForm();

        document.getElementById('drinkName').value = drinkData.name;
        document.getElementById('drinkPrice').value = drinkData.price;
        document.getElementById('drinkStatus').value = drinkData.status;
        document.getElementById('drinkStock').value = drinkData.stock;
        document.getElementById('drinkDescription').value = drinkData.description || '';

        if (drinkData.imageUrl) {
            const uploadPlaceholder = document.querySelector('.upload-placeholder');
            uploadPlaceholder.innerHTML = `
                <img src="${drinkData.imageUrl}" alt="Current image" style="max-width: 100px; max-height: 100px; border-radius: 5px;">
                <p style="margin-top: 10px;">Click to change image</p>
            `;
        }

        modalTitle.textContent = 'Edit Drink';
        addDrinkBtnModal.innerHTML = '<i class="fas fa-save"></i> Save Changes';
        currentEditingId = firebaseId;

        setTimeout(() => {
            drinkData.categories.forEach(catId => {
                const checkbox = document.getElementById(`category_${catId}`);
                if (checkbox) checkbox.checked = true;
            });

            drinkData.customizations.forEach(custId => {
                const checkbox = document.getElementById(`customization_${custId}`);
                if (checkbox) checkbox.checked = true;
            });
        }, 100);

        openModal();
    }

    async function deleteDrink(card) {
        const drinkId = card.getAttribute('data-firebase-id');
        const drink = allDrinks.find(d => d.id === drinkId);
        const drinkName = card.querySelector('.drink-name').textContent;

        currentDeletingCard = card;
        currentDeletingDrink = drink;

        const deleteModal = document.getElementById('deleteConfirmModal');
        const deleteDrinkNameSpan = document.getElementById('deleteDrinkName');

        if (!deleteModal || !deleteDrinkNameSpan) {
            console.error('Delete modal elements not found');
            if (confirm(`Are you sure you want to delete "${drinkName}"? This action cannot be undone.`)) {
                await performDelete(card, drink, drinkId);
            }
            return;
        }

        deleteDrinkNameSpan.textContent = drinkName;
        deleteModal.classList.add('active');
        document.body.style.overflow = 'hidden';
    }

    function closeDeleteModal() {
        const deleteModal = document.getElementById('deleteConfirmModal');
        if (deleteModal) {
            deleteModal.classList.remove('active');
            document.body.style.overflow = 'auto';
        }
        currentDeletingCard = null;
        currentDeletingDrink = null;
    }

    async function performDelete(card, drink, drinkId) {
        try {
            card.style.opacity = '0.5';
            card.style.pointerEvents = 'none';

            const result = await DrinkAPIService.deleteDrink(drinkId);

            if (result.success) {
                if (drink?.imagePath) {
                    await FirebaseStorageService.deleteImage(drink.imagePath);
                }

                showToastMessage('Drink deleted successfully!');
                await loadDrinksFromAPI();
            } else {
                throw new Error(result.error || 'Failed to delete drink');
            }
        } catch (error) {
            console.error('Error deleting drink:', error);
            showToastMessage('Error deleting drink: ' + error.message, 'Error');
            card.style.opacity = '1';
            card.style.pointerEvents = 'auto';
        }
    }

    async function confirmDelete() {
        if (!currentDeletingCard || !currentDeletingDrink) {
            closeDeleteModal();
            return;
        }

        const card = currentDeletingCard;
        const drink = currentDeletingDrink;
        const drinkId = card.getAttribute('data-firebase-id');

        const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');
        if (confirmDeleteBtn) {
            const originalBtnContent = confirmDeleteBtn.innerHTML;
            confirmDeleteBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Deleting...';
            confirmDeleteBtn.disabled = true;
        }

        try {
            await performDelete(card, drink, drinkId);
            closeDeleteModal();
        } catch (error) {
            console.error('Error in confirmDelete:', error);
            if (confirmDeleteBtn) {
                confirmDeleteBtn.innerHTML = '<i class="fas fa-trash"></i> Delete';
                confirmDeleteBtn.disabled = false;
            }
        }
    }

    function addActionButtonListeners(card, drinkData) {
        const editBtn = card.querySelector('.edit-btn');
        const deleteBtn = card.querySelector('.delete-btn');

        editBtn.addEventListener('click', function (e) {
            e.stopPropagation();
            editDrink(card, drinkData);
        });

        deleteBtn.addEventListener('click', function (e) {
            e.stopPropagation();
            deleteDrink(card);
        });
    }

    function updateDrinksCount(count) {
        const drinksCountElement = document.querySelector('.drinks-count');
        if (drinksCountElement) {
            drinksCountElement.textContent = `${count} ${count === 1 ? 'drink' : 'drinks'} in your menu.`;
        }
    }

    function handleKeyboardShortcuts(e) {
        if (e.key === 'Escape') {
            if (modal.classList.contains('active')) {
                closeModalFunc();
            }
            if (filterModal.classList.contains('active')) {
                closeFilterModal();
            }
            const deleteModal = document.getElementById('deleteConfirmModal');
            if (deleteModal && deleteModal.classList.contains('active')) {
                closeDeleteModal();
            }
        }

        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            searchInput.focus();
        }

        if ((e.ctrlKey || e.metaKey) && e.key === 'n') {
            e.preventDefault();
            openModal();
        }
    }

    function showToastMessage(message, title = 'Success') {
        let toastContainer = document.getElementById('toastStackContainer');

        if (!toastContainer) {
            console.error('Toast container not found');
            console.log(title + ':', message);
            return;
        }

        const toastElement = document.createElement('div');
        toastElement.className = 'toast';
        toastElement.setAttribute('role', 'alert');
        toastElement.setAttribute('aria-live', 'assertive');
        toastElement.setAttribute('aria-atomic', 'true');

        const currentTime = new Date().toLocaleTimeString('en-US', {
            hour12: true,
            hour: 'numeric',
            minute: '2-digit'
        });

        const isError = title === 'Error' || title.toLowerCase().includes('error');
        const headerClass = isError ? 'bg-danger text-white' : '';

        toastElement.innerHTML = `
            <div class="toast-header ${headerClass}">
                <img src="/images/Cofinoy.png" class="rounded me-2" alt="Logo" width="20" height="20">
                <strong class="me-auto">${title}</strong>
                <small>${currentTime}</small>
                <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        `;

        toastContainer.appendChild(toastElement);

        if (typeof bootstrap !== 'undefined' && bootstrap.Toast) {
            const toastBootstrap = bootstrap.Toast.getOrCreateInstance(toastElement);
            toastBootstrap.show();

            toastElement.addEventListener('hidden.bs.toast', function () {
                toastElement.remove();
            });
        } else {
            toastElement.style.display = 'block';
            setTimeout(() => {
                toastElement.style.opacity = '0';
                setTimeout(() => toastElement.remove(), 300);
            }, 5000);
        }
    }
});