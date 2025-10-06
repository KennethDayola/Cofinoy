const CustomizationAPI = {
    async getAllCustomizations() {
        const response = await fetch('/Menu/GetAllCustomizations');
        return await response.json();
    },

    async getCustomization(id) {
        const response = await fetch(`/Menu/GetCustomization?id=${id}`);
        return await response.json();
    },

    async addCustomization(data) {
        const response = await fetch('/Menu/AddCustomization', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        return await response.json();
    },

    async updateCustomization(id, data) {
        const response = await fetch(`/Menu/UpdateCustomization?id=${id}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        return await response.json();
    },

    async deleteCustomization(id) {
        const response = await fetch(`/Menu/DeleteCustomization?id=${id}`, {
            method: 'POST'
        });
        return await response.json();
    }
};

document.addEventListener('DOMContentLoaded', function () {
    const modal = document.getElementById('addAddonModal');
    const addAddonBtn = document.querySelector('.add-addon-btn');
    const closeModalBtn = document.getElementById('closeModal');
    const cancelBtn = document.getElementById('cancelBtn');
    const form = document.getElementById('addAddonForm');
    const searchInput = document.querySelector('.search-input');
    const customizationTypeSelect = document.getElementById('addonType');
    const quantitySettings = document.getElementById('quantitySettings');
    const optionsSection = document.getElementById('optionsSection');
    const addOptionBtn = document.querySelector('.add-option-btn');
    const optionsList = document.getElementById('optionsList');
    const optionsError = document.getElementById('optionsError');

    const filterModal = document.getElementById('filterModal');
    const closeFilterModalBtn = document.getElementById('closeFilterModal');
    const filterBtn = document.querySelector('.filter-btn');
    const typeFilter = document.getElementById('typeFilter');
    const requiredFilter = document.getElementById('requiredFilter');
    const clearFiltersBtn = document.querySelector('.clear-filters-btn');
    const applyFiltersBtn = document.querySelector('.apply-filters-btn');

    const modalTitle = document.getElementById('modalTitle');
    const saveAddonBtn = document.getElementById('saveAddonBtn');

    let currentEditingId = null;
    let optionCounter = 0;
    let isSubmitting = false;
    let allAddons = [];

    initializeApp();

    async function initializeApp() {
        try {
            updateNoAddonsMessage(); // Add this line
            await loadCustomizations();
            setupEventListeners();
        } catch (error) {
            console.error('Error initializing app:', error);
            showToastMessage('Error initializing application: ' + error.message, 'Error');
        }
    }

    function setupEventListeners() {
        addAddonBtn.addEventListener('click', openModal);
        closeModalBtn.addEventListener('click', closeModalFunc);
        cancelBtn.addEventListener('click', closeModalFunc);

        modal.addEventListener('click', function (e) {
            if (e.target === modal) closeModalFunc();
        });

        closeFilterModalBtn.addEventListener('click', closeFilterModal);
        filterModal.addEventListener('click', function (e) {
            if (e.target === filterModal) closeFilterModal();
        });

        customizationTypeSelect.addEventListener('change', handleTypeChange);
        filterBtn.addEventListener('click', openFilterModal);
        clearFiltersBtn.addEventListener('click', clearAllFilters);
        applyFiltersBtn.addEventListener('click', applyFiltersAndClose);
        searchInput.addEventListener('input', applyFilters);

        addOptionBtn.addEventListener('click', function () {
            addOptionField();
            clearValidationErrors();
        });

        form.addEventListener('submit', handleFormSubmit);
        document.addEventListener('keydown', handleKeyboardShortcuts);
    }

    function openModal() {
        modal.classList.add('active');
        document.body.style.overflow = 'hidden';
    }

    function closeModalFunc() {
        modal.classList.remove('active');
        document.body.style.overflow = 'auto';
        resetForm();
        resetModalState();
        clearValidationErrors();
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
        optionsList.innerHTML = '';
        optionCounter = 0;
        quantitySettings.style.display = 'none';
        optionsSection.style.display = 'none';
        document.getElementById('maxQuantity').required = false;
        document.getElementById('pricePerUnit').required = false;
    }

    function resetModalState() {
        modalTitle.textContent = 'Add new customization';
        saveAddonBtn.innerHTML = '<i class="fas fa-plus"></i> Add Customization';
        currentEditingId = null;
    }

    function clearValidationErrors() {
        optionsError.style.display = 'none';
        document.querySelectorAll('.error').forEach(el => el.classList.remove('error'));
    }

    function handleTypeChange() {
        const selectedType = this.value;

        if (selectedType === 'quantity') {
            quantitySettings.style.display = 'block';
            optionsSection.style.display = 'none';
            document.getElementById('maxQuantity').required = true;
            document.getElementById('pricePerUnit').required = true;
        } else if (selectedType === 'single_select' || selectedType === 'multi_select') {
            quantitySettings.style.display = 'none';
            optionsSection.style.display = 'block';
            document.getElementById('maxQuantity').required = false;
            document.getElementById('pricePerUnit').required = false;
        } else {
            quantitySettings.style.display = 'none';
            optionsSection.style.display = 'none';
            document.getElementById('maxQuantity').required = false;
            document.getElementById('pricePerUnit').required = false;
        }

        clearValidationErrors();
    }

    function clearAllFilters() {
        typeFilter.value = '';
        requiredFilter.value = '';
        searchInput.value = '';
        applyFilters();
    }

    function applyFiltersAndClose() {
        applyFilters();
        closeFilterModal();
    }

    function applyFilters() {
        const searchTerm = searchInput.value.toLowerCase().trim();
        const typeFilterValue = typeFilter.value;
        const requiredFilterValue = requiredFilter.value;

        const allCards = document.querySelectorAll('.addon-card');
        let visibleCount = 0;

        allCards.forEach(card => {
            const id = card.getAttribute('data-id');
            const addon = allAddons.find(a => a.id === id);

            if (!addon) {
                card.style.display = 'none';
                return;
            }

            const matchesSearch = !searchTerm ||
                addon.name.toLowerCase().includes(searchTerm) ||
                addon.type.toLowerCase().includes(searchTerm);

            const matchesType = !typeFilterValue || addon.type === typeFilterValue;
            const matchesRequired = !requiredFilterValue ||
                addon.required.toString() === requiredFilterValue;

            if (matchesSearch && matchesType && matchesRequired) {
                card.style.display = 'block';
                visibleCount++;
            } else {
                card.style.display = 'none';
            }
        });

        updateAddonsCount(visibleCount);

        // Update no addons message based on filter results
        if (visibleCount === 0) {
            updateNoAddonsMessage();
        } else {
            const existingMessage = document.querySelector('.no-addons-message');
            if (existingMessage) {
                existingMessage.remove();
            }
        }
    }
    function addOptionField(existingOption = null) {
        optionCounter++;
        const optionItem = document.createElement('div');
        optionItem.className = 'option-item';
        optionItem.setAttribute('data-option-id', optionCounter);

        optionItem.innerHTML = `
        <div class="option-header">
            <span class="option-title">Option ${optionCounter}</span>
            <button type="button" class="remove-option-btn" onclick="removeOption(${optionCounter})">
                <i class="fas fa-times"></i>
            </button>
        </div>
        <div class="option-fields">
            <div class="option-field">
                <label>Option Name *</label>
                <input type="text" name="option_name_${optionCounter}" 
                       placeholder="e.g., Hot, Large, Whole Milk" 
                       required maxlength="100" 
                       value="${existingOption ? existingOption.name : ''}">
            </div>
            <div class="option-field">
                <label>Price Modifier (₱)</label>
                <input type="number" name="option_price_${optionCounter}" 
                       placeholder="0.00" step="0.01" min="0"
                       value="${existingOption ? existingOption.priceModifier : ''}">
            </div>
            <div class="option-field">
                <label>Description (optional)</label>
                <input type="text" name="option_description_${optionCounter}" 
                       placeholder="Brief description" maxlength="200"
                       value="${existingOption ? existingOption.description || '' : ''}">
            </div>
            <div class="default-checkbox">
                <input type="radio" name="default_option" 
                       id="default_${optionCounter}" 
                       value="${optionCounter}"
                       ${existingOption && existingOption.default ? 'checked' : ''}>
                <label for="default_${optionCounter}">Set as default option</label>
            </div>
        </div>
    `;

        optionsList.appendChild(optionItem);

        const defaultRadio = optionItem.querySelector('input[type="radio"]');
        defaultRadio.addEventListener('change', handleDefaultOptionChange);

        if (!existingOption && optionCounter === 1) {
            defaultRadio.checked = true;
        }
    }

    function handleDefaultOptionChange(event) {
        if (event.target.checked) {
            const allDefaultRadios = document.querySelectorAll('input[name="default_option"]');
            allDefaultRadios.forEach(radio => {
                if (radio !== event.target) {
                    radio.checked = false;
                }
            });
        }
    }

    window.removeOption = function (optionId) {
        const optionItem = document.querySelector(`[data-option-id="${optionId}"]`);
        if (optionItem) {
            const isDefault = optionItem.querySelector('input[type="radio"]').checked;
            optionItem.remove();
            clearValidationErrors();

            if (isDefault) {
                const remainingOptions = document.querySelectorAll('.option-item');
                if (remainingOptions.length > 0) {
                    const firstOption = remainingOptions[0];
                    const firstRadio = firstOption.querySelector('input[type="radio"]');
                    if (firstRadio) {
                        firstRadio.checked = true;
                    }
                }
            }
        }
    };

    function validateForm() {
        clearValidationErrors();
        let isValid = true;
        let firstError = null;

        const addonType = document.getElementById('addonType').value;
        const addonName = document.getElementById('addonName');
        const displayOrder = document.getElementById('displayOrder');

        if (!addonName.value.trim()) {
            addonName.classList.add('error');
            if (!firstError) firstError = addonName;
            isValid = false;
        }

        if (!displayOrder.value || displayOrder.value < 1) {
            displayOrder.classList.add('error');
            if (!firstError) firstError = displayOrder;
            isValid = false;
        }

        if (!addonType) {
            customizationTypeSelect.classList.add('error');
            if (!firstError) firstError = customizationTypeSelect;
            isValid = false;
        }

        if (addonType === 'single_select' || addonType === 'multi_select') {
            const optionItems = document.querySelectorAll('.option-item');

            if (optionItems.length === 0) {
                optionsError.style.display = 'block';
                if (!firstError) firstError = optionsSection;
                isValid = false;
            } else {
                optionItems.forEach(item => {
                    const optionId = item.getAttribute('data-option-id');
                    const nameInput = item.querySelector(`[name="option_name_${optionId}"]`);
                    if (!nameInput.value.trim()) {
                        nameInput.classList.add('error');
                        if (!firstError) firstError = nameInput;
                        isValid = false;
                    }
                });
            }
        }

        if (addonType === 'quantity') {
            const maxQuantity = document.getElementById('maxQuantity');
            const pricePerUnit = document.getElementById('pricePerUnit');

            if (!maxQuantity.value || maxQuantity.value < 1 || maxQuantity.value > 100) {
                maxQuantity.classList.add('error');
                if (!firstError) firstError = maxQuantity;
                isValid = false;
            }

            if (pricePerUnit.value === '' || pricePerUnit.value < 0 || pricePerUnit.value > 1000) {
                pricePerUnit.classList.add('error');
                if (!firstError) firstError = pricePerUnit;
                isValid = false;
            }
        }

        if (firstError) {
            firstError.focus();
            firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }

        return isValid;
    }

    async function handleFormSubmit(e) {
        e.preventDefault();

        if (isSubmitting) return;

        if (!validateForm()) {
            showToastMessage('Please fix the validation errors before submitting.', 'Validation Error');
            return;
        }

        isSubmitting = true;

        const originalBtnContent = saveAddonBtn.innerHTML;
        saveAddonBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Processing...';
        saveAddonBtn.disabled = true;

        try {
            const formData = {
                name: document.getElementById('addonName').value.trim(),
                type: document.getElementById('addonType').value,
                required: document.getElementById('isRequired').value === 'true',
                displayOrder: parseInt(document.getElementById('displayOrder').value),
                description: document.getElementById('addonDescription').value.trim(),
                options: []
            };

            if (formData.type === 'quantity') {
                formData.maxQuantity = parseInt(document.getElementById('maxQuantity').value);
                formData.pricePerUnit = parseFloat(document.getElementById('pricePerUnit').value);
            }

            if (formData.type === 'single_select' || formData.type === 'multi_select') {
                const optionItems = document.querySelectorAll('.option-item');
                const selectedDefaultId = document.querySelector('input[name="default_option"]:checked')?.value;

                optionItems.forEach(item => {
                    const optionId = item.getAttribute('data-option-id');
                    const optionName = item.querySelector(`[name="option_name_${optionId}"]`).value.trim();

                    if (optionName) {
                        const optionPrice = parseFloat(item.querySelector(`[name="option_price_${optionId}"]`).value) || 0;
                        const optionDescription = item.querySelector(`[name="option_description_${optionId}"]`).value.trim();
                        const isDefault = optionId === selectedDefaultId;

                        formData.options.push({
                            name: optionName,
                            priceModifier: optionPrice,
                            description: optionDescription,
                            default: isDefault
                        });
                    }
                });
            }

            let result;

            if (currentEditingId !== null) {
                result = await CustomizationAPI.updateCustomization(currentEditingId, formData);
                if (result.success) {
                    showToastMessage('Customization updated successfully!');
                } else {
                    throw new Error(result.error || 'Failed to update customization');
                }
            } else {
                result = await CustomizationAPI.addCustomization(formData);
                if (result.success) {
                    showToastMessage('Customization added successfully!');
                } else {
                    throw new Error(result.error || 'Failed to add customization');
                }
            }

            closeModalFunc();
            await loadCustomizations();

        } catch (error) {
            console.error('Error saving customization:', error);
            showToastMessage('Error saving customization: ' + error.message, 'Error');
        } finally {
            saveAddonBtn.innerHTML = originalBtnContent;
            saveAddonBtn.disabled = false;
            isSubmitting = false;
        }
    }

    async function loadCustomizations() {
        try {
            const result = await CustomizationAPI.getAllCustomizations();

            if (result.success) {
                allAddons = result.data;
                displayAddons(result.data);
                updateAddonsCount(result.data.length);
            } else {
                console.error('Error loading customizations:', result.error);
                showToastMessage('Error loading customizations: ' + result.error, 'Error');
                displayAddons([]);
            }
        } catch (error) {
            console.error('Error loading customizations:', error);
            showToastMessage('Error loading customizations: ' + error.message, 'Error');
            displayAddons([]);
        }
    }

    function displayAddons(addons) {
        const addonsGrid = document.querySelector('.addons-grid');
        addonsGrid.innerHTML = '';

        if (addons.length === 0) {
            updateNoAddonsMessage();
        } else {
            addonsGrid.style.display = 'grid';
            addonsGrid.style.justifyContent = '';
            addonsGrid.style.alignItems = '';

            addons.forEach(addon => {
                addAddonToGrid(addon);
            });
        }
    }

    function addAddonToGrid(addonData) {
        const addonsGrid = document.querySelector('.addons-grid');
        const addonCard = document.createElement('div');
        addonCard.className = 'addon-card';
        addonCard.setAttribute('data-id', addonData.id);

        const optionsCount = addonData.options ? addonData.options.length : 0;

        addonCard.innerHTML = `
            <div class="addon-header">
                <h3 class="addon-name">${addonData.name}</h3>
                <span class="addon-type">${formatTypeName(addonData.type)}</span>
            </div>
            <div class="addon-info">
                <div class="addon-details">
                    <div class="detail-item">
                        <span class="detail-label">Display Order</span>
                        <span class="detail-value">#${addonData.displayOrder}</span>
                    </div>
                    <div class="detail-item">
                        <span class="detail-label">Type</span>
                        <span class="detail-value">${formatTypeName(addonData.type)}</span>
                    </div>
                    <div class="detail-item">
                        <span class="detail-label">Required</span>
                        <span class="detail-value">
                            ${addonData.required ?
                '<span class="required-badge">Required</span>' :
                '<span class="optional-badge">Optional</span>'}
                        </span>
                    </div>
                    <div class="detail-item">
                        <span class="detail-label">${addonData.type === 'quantity' ? 'Max Quantity' : 'Options Count'}</span>
                        <span class="detail-value">${addonData.type === 'quantity' ?
                addonData.maxQuantity || 100 :
                `${optionsCount} ${optionsCount === 1 ? 'option' : 'options'}`}</span>
                    </div>
                </div>
                ${addonData.type !== 'quantity' && optionsCount > 0 ? `
                <div class="addon-options">
                    <div class="options-title">Options Preview</div>
                    <div class="options-preview">
                        ${addonData.options.slice(0, 4).map(option =>
                    `<span class="option-tag">${option.name}</span>`
                ).join('')}
                        ${optionsCount > 4 ? `<span class="option-tag">+${optionsCount - 4} more</span>` : ''}
                    </div>
                </div>
                ` : ''}
                <div class="addon-footer">
                    <span class="addon-stats">
                        ${addonData.type === 'quantity' ?
                `Max: ${addonData.maxQuantity || 100} | ₱${addonData.pricePerUnit || 0}/unit` :
                `${optionsCount} options available`}
                    </span>
                    <div class="addon-actions">
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

        addonsGrid.appendChild(addonCard);
        addActionButtonListeners(addonCard, addonData);
    }

    function editAddon(card, addonData) {
        resetForm();
        clearValidationErrors();

        document.getElementById('addonName').value = addonData.name;
        document.getElementById('addonType').value = addonData.type;
        document.getElementById('isRequired').value = addonData.required.toString();
        document.getElementById('displayOrder').value = addonData.displayOrder;
        document.getElementById('addonDescription').value = addonData.description || '';

        if (addonData.type === 'quantity') {
            quantitySettings.style.display = 'block';
            optionsSection.style.display = 'none';
            document.getElementById('maxQuantity').value = addonData.maxQuantity || 100;
            document.getElementById('pricePerUnit').value = addonData.pricePerUnit || 0;
            document.getElementById('maxQuantity').required = true;
            document.getElementById('pricePerUnit').required = true;
        } else if (addonData.type === 'single_select' || addonData.type === 'multi_select') {
            quantitySettings.style.display = 'none';
            optionsSection.style.display = 'block';
            document.getElementById('maxQuantity').required = false;
            document.getElementById('pricePerUnit').required = false;

            if (addonData.options && addonData.options.length > 0) {
                addonData.options.forEach(option => {
                    addOptionField(option);
                });

                const defaultOptionIndex = addonData.options.findIndex(opt => opt.default);
                if (defaultOptionIndex !== -1) {
                    const defaultOptionId = defaultOptionIndex + 1;
                    const defaultRadio = document.querySelector(`input[value="${defaultOptionId}"]`);
                    if (defaultRadio) {
                        defaultRadio.checked = true;
                    }
                }
            }
        }

        modalTitle.textContent = 'Edit customization';
        saveAddonBtn.innerHTML = '<i class="fas fa-save"></i> Save Changes';
        currentEditingId = addonData.id;

        openModal();
    }

    async function deleteAddon(card) {
        const id = card.getAttribute('data-id');
        const addonName = card.querySelector('.addon-name').textContent;

        if (confirm(`Are you sure you want to delete the "${addonName}" customization? This action cannot be undone.`)) {
            try {
                card.style.opacity = '0.5';
                card.style.pointerEvents = 'none';

                const result = await CustomizationAPI.deleteCustomization(id);

                if (result.success) {
                    showToastMessage('Customization deleted successfully!');
                    await loadCustomizations();
                } else {
                    throw new Error(result.error || 'Failed to delete customization');
                }

            } catch (error) {
                console.error('Error deleting customization:', error);
                showToastMessage('Error deleting customization: ' + error.message, 'Error');

                card.style.opacity = '1';
                card.style.pointerEvents = 'auto';
            }
        }
    }

    function addActionButtonListeners(card, addonData) {
        const editBtn = card.querySelector('.edit-btn');
        const deleteBtn = card.querySelector('.delete-btn');

        editBtn.addEventListener('click', function () {
            editAddon(card, addonData);
        });

        deleteBtn.addEventListener('click', function () {
            deleteAddon(card);
        });
    }

    function handleKeyboardShortcuts(e) {
        if (e.key === 'Escape') {
            if (modal.classList.contains('active')) {
                closeModalFunc();
            } else if (filterModal.classList.contains('active')) {
                closeFilterModal();
            }
        }

        if ((e.ctrlKey || e.metaKey) && e.key === 'n') {
            e.preventDefault();
            openModal();
        }

        if ((e.ctrlKey || e.metaKey) && e.key === 'f') {
            e.preventDefault();
            searchInput.focus();
        }
    }

    function showToastMessage(message, title = 'Cofinoy') {
        let toastContainer = document.getElementById('toastStackContainer');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.id = 'toastStackContainer';
            toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
            toastContainer.style.zIndex = '9999';
            document.body.appendChild(toastContainer);
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

    function updateNoAddonsMessage() {
        const addonsGrid = document.querySelector('.addons-grid');

        // Remove existing message if any
        const existingMessage = addonsGrid.querySelector('.no-addons-message');
        if (existingMessage) {
            existingMessage.remove();
        }

        // Create new message
        const noAddonsMessage = document.createElement('div');
        noAddonsMessage.className = 'no-addons-message';
        noAddonsMessage.innerHTML = `
        <div class="no-addons-icon">
            <i class="fas fa-cube"></i>
        </div>
        <div class="no-addons-text">No addon categories yet</div>
        <div class="no-addons-subtext">Create your first addon category to enhance your menu</div>
    `;

        // Add message if grid is empty
        if (addonsGrid.children.length === 0) {
            addonsGrid.appendChild(noAddonsMessage);
            addonsGrid.style.display = 'flex';
            addonsGrid.style.justifyContent = 'center';
            addonsGrid.style.alignItems = 'center';
        }
    }


    function updateAddonsCount(count) {
        const countElement = document.querySelector('.addons-count');
        if (!countElement) return;

        if (count === 0) {
            countElement.textContent = 'No customizations yet';
        } else if (count === 1) {
            countElement.textContent = `${count} customization in your menu.`;
        } else {
            countElement.textContent = `${count} customizations in your menu.`;
        }
    }

    function formatTypeName(type) {
        return type.replace(/_/g, ' ')
            .replace(/\b\w/g, l => l.toUpperCase());
    }
});