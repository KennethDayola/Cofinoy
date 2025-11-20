document.addEventListener('DOMContentLoaded', function () {
    const modal = document.getElementById('addCategoryModal');
    const addCategoryBtn = document.querySelector('.add-category-btn');
    const closeModal = document.getElementById('closeModal');
    const cancelBtn = document.getElementById('cancelBtn');
    const form = document.getElementById('addCategoryForm');
    const searchInput = document.querySelector('.search-input');
    const loadingProgress = document.getElementById('loadingProgress');

    const filterBtn = document.querySelector('.filter-btn');
    const filterModal = document.getElementById('filterModal');
    const closeFilterModalBtn = document.getElementById('closeFilterModal');
    const statusFilter = document.getElementById('statusFilter');
    const clearFiltersBtn = document.querySelector('.clear-filters-btn');
    const applyFiltersBtn = document.querySelector('.apply-filters-btn');

    const deleteModal = document.getElementById('deleteConfirmModal');
    const cancelDeleteBtn = document.getElementById('cancelDeleteBtn');
    const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');
    const deleteCategoryNameSpan = document.getElementById('deleteCategoryName');

    let currentEditingRow = null;
    let currentEditingId = null;
    let allCategories = [];
    let currentDeletingRow = null;
    let currentDeletingCategory = null;
    let isInitialLoad = true;
    let draggedRow = null;

    if (loadingProgress && isInitialLoad) {
        loadingProgress.style.display = 'flex';
    }

    loadCategories();

    function getNextAvailableDisplayOrder() {
        if (allCategories.length === 0) {
            return 1;
        }
        
        // Get the maximum display order and add 1
        const maxDisplayOrder = Math.max(...allCategories.map(cat => cat.displayOrder || 0));
        return maxDisplayOrder + 1;
    }

    function openModal() {
        modal.classList.add('active');
        document.body.style.overflow = 'hidden';

        // If this is a new category (not editing), autofill the display order
        if (!currentEditingId) {
            const nextDisplayOrder = getNextAvailableDisplayOrder();
            document.getElementById('categoryDisplayOrder').value = nextDisplayOrder;
        }
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

    function openDeleteModal(row, category) {
        currentDeletingRow = row;
        currentDeletingCategory = category;
        
        const categoryName = row.querySelector('.category-name').textContent;
        deleteCategoryNameSpan.textContent = categoryName;
        
        deleteModal.classList.add('active');
        document.body.style.overflow = 'hidden';
    }

    function closeDeleteModal() {
        deleteModal.classList.remove('active');
        document.body.style.overflow = 'auto';
        currentDeletingRow = null;
        currentDeletingCategory = null;
    }

    function resetForm() {
        form.reset();
        const helpText = document.getElementById('displayOrderHelp');
        helpText.textContent = '';
        helpText.classList.remove('validation-error-text', 'validation-success-text');
    }

    function resetModalState() {
        document.querySelector('.modal-header h2').textContent = 'Add new category';
        document.getElementById('addCategoryBtn').innerHTML = '<i class="fas fa-plus"></i> Add Category';
        currentEditingRow = null;
        currentEditingId = null;
    }

    function validateDisplayOrder(displayOrder, excludeId = null) {
        const duplicate = allCategories.find(cat => 
            cat.displayOrder === displayOrder && cat.id !== excludeId
        );
        
        if (duplicate) {
            return {
                isValid: false,
                message: `Display order ${displayOrder} is already used by "${duplicate.name}"`
            };
        }
        
        return { isValid: true };
    }

    document.getElementById('categoryDisplayOrder').addEventListener('input', function() {
        const displayOrder = parseInt(this.value);
        const helpText = document.getElementById('displayOrderHelp');
        
        if (isNaN(displayOrder) || displayOrder < 0) {
            helpText.textContent = '';
            helpText.classList.remove('validation-error-text', 'validation-success-text');
            return;
        }
        
        const validation = validateDisplayOrder(displayOrder, currentEditingId);
        
        if (!validation.isValid) {
            helpText.textContent = validation.message;
            helpText.classList.remove('validation-success-text');
            helpText.classList.add('validation-error-text');
        } else {
            helpText.textContent = 'Display order is available';
            helpText.classList.remove('validation-error-text');
            helpText.classList.add('validation-success-text');
        }
    });

    form.addEventListener('submit', async function (e) {
        e.preventDefault();

        const submitBtn = document.getElementById('addCategoryBtn');
        const originalBtnContent = submitBtn.innerHTML;
        
        const displayOrderInput = document.getElementById('categoryDisplayOrder');
        const displayOrder = parseInt(displayOrderInput.value) || 0;
        
        const validation = validateDisplayOrder(displayOrder, currentEditingId);
        if (!validation.isValid) {
            const helpText = document.getElementById('displayOrderHelp');
            helpText.textContent = validation.message;
            helpText.classList.remove('validation-success-text');
            helpText.classList.add('validation-error-text');
            displayOrderInput.focus();
            displayOrderInput.scrollIntoView({ behavior: 'smooth', block: 'center' });
            return;
        }

        submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Processing...';
        submitBtn.disabled = true;

        const formData = {
            name: document.getElementById('categoryName').value.trim(),
            description: document.getElementById('categoryDescription').value.trim(),
            status: document.getElementById('categoryStatus').value,
            displayOrder: displayOrder
        };

        try {
            let url = currentEditingId !== null 
                ? `/Menu/UpdateCategory?id=${currentEditingId}` 
                : '/Menu/AddCategory';

            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(formData)
            });

            const result = await response.json();

            if (result.success) {
                showToast(
                    currentEditingId ? 'Category updated successfully!' : 'Category added successfully!',
                    'light',
                    'Cofinoy'
                );
                closeModalFunc();
                await loadCategories();
            } else {
                throw new Error(result.error || 'Failed to save category');
            }

        } catch (error) {
            console.error('Error saving category:', error);
            showToast('Error saving category: ' + error.message, 'light', 'Error');
        } finally {
            submitBtn.innerHTML = originalBtnContent;
            submitBtn.disabled = false;
        }
    });

    async function loadCategories() {
        try {
            if (loadingProgress && isInitialLoad) {
                loadingProgress.style.display = 'flex';
            }

            const response = await fetch('/Menu/GetAllCategories', {
                method: 'GET'
            });
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const result = await response.json();

            if (result.success) {
                allCategories = result.data || [];
                
                // Sort categories by display order
                allCategories.sort((a, b) => (a.displayOrder || 0) - (b.displayOrder || 0));
                
                const tbody = document.querySelector('.categories-table tbody');
                tbody.innerHTML = '';

                if (allCategories.length > 0) {
                    allCategories.forEach(category => {
                        addCategoryToTable(category);
                    });
                    
                    // Setup drag and drop after all rows are added
                    setupDragAndDrop();
                } else {
                    const emptyStateRow = document.createElement('tr');
                    emptyStateRow.className = 'empty-table-state';
                    emptyStateRow.innerHTML = `
                        <td colspan="6">
                            <div class="empty-table-content">
                                <div class="empty-table-icon">
                                    <img src="https://cdn-icons-png.flaticon.com/512/2603/2603910.png" />
                                </div>
                                <h3 class="empty-table-message">No categories yet</h3>
                                <p class="empty-table-submessage">Add your first category to get started</p>
                            </div>
                        </td>
                    `;
                    tbody.appendChild(emptyStateRow);
                }

                updateResultsCount(allCategories.length);
                applyFilters(); 
            } else {
                console.error('Error loading categories:', result.error);
                showToast('Error loading categories: ' + result.error, 'light', 'Error');
            }
        } catch (error) {
            console.error('Error loading categories:', error);
            showToast('Error loading categories: ' + error.message, 'light', 'Error');
        } finally {
            if (loadingProgress) {
                loadingProgress.style.display = 'none';
            }
            isInitialLoad = false; 
        }
    }

    function addCategoryToTable(categoryData) {
        const tbody = document.querySelector('.categories-table tbody');
        const newRow = document.createElement('tr');

        newRow.setAttribute('data-category-id', categoryData.id);
        newRow.setAttribute('draggable', 'true');

        newRow.innerHTML = `
            <td>
                <div class="category-info">
                    <span class="category-name">${categoryData.name}</span>
                </div>
            </td>
            <td class="category-description">${categoryData.description || ''}</td>
            <td><span class="items-count">${categoryData.itemsCount || 0} items</span></td>
            <td><span class="status-badge ${categoryData.status.toLowerCase()}">${categoryData.status}</span></td>
            <td class="display-order">
                <div class="display-order-cell">
                    <i class="fas fa-grip-vertical drag-handle"></i>
                    <span class="display-order-text">${categoryData.displayOrder || 0}</span>
                </div>
            </td>
            <td class="actions">
                <button class="action-btn edit-btn" title="Edit Category">
                    <i class="fas fa-edit"></i>
                </button>
                <button class="action-btn delete-btn" title="Delete Category">
                    <i class="fas fa-trash"></i>
                </button>
            </td>
        `;

        tbody.appendChild(newRow);
        addActionButtonListeners(newRow, categoryData);
    }

    function setupDragAndDrop() {
        const tbody = document.querySelector('.categories-table tbody');
        const rows = tbody.querySelectorAll('tr:not(.empty-table-state)');

        rows.forEach(row => {
            row.addEventListener('dragstart', handleDragStart);
            row.addEventListener('dragend', handleDragEnd);
            row.addEventListener('dragover', handleDragOver);
            row.addEventListener('drop', handleDrop);
            row.addEventListener('dragenter', handleDragEnter);
            row.addEventListener('dragleave', handleDragLeave);
        });
    }

    function handleDragStart(e) {
        draggedRow = this;
        this.classList.add('dragging');
        e.dataTransfer.effectAllowed = 'move';
        e.dataTransfer.setData('text/html', this.innerHTML);
    }

    function handleDragEnd(e) {
        this.classList.remove('dragging');
        
        // Remove all drag-over classes
        const tbody = document.querySelector('.categories-table tbody');
        const rows = tbody.querySelectorAll('tr');
        rows.forEach(row => row.classList.remove('drag-over'));
    }

    function handleDragOver(e) {
        if (e.preventDefault) {
            e.preventDefault();
        }
        e.dataTransfer.dropEffect = 'move';
        return false;
    }

    function handleDragEnter(e) {
        if (this !== draggedRow) {
            this.classList.add('drag-over');
        }
    }

    function handleDragLeave(e) {
        this.classList.remove('drag-over');
    }

    async function handleDrop(e) {
        if (e.stopPropagation) {
            e.stopPropagation();
        }

        if (draggedRow !== this) {
            const tbody = document.querySelector('.categories-table tbody');
            const allRows = Array.from(tbody.querySelectorAll('tr:not(.empty-table-state)'));
            
            const draggedIndex = allRows.indexOf(draggedRow);
            const targetIndex = allRows.indexOf(this);

            // Reorder in DOM
            if (draggedIndex < targetIndex) {
                this.parentNode.insertBefore(draggedRow, this.nextSibling);
            } else {
                this.parentNode.insertBefore(draggedRow, this);
            }

            // Update display orders in the backend
            await updateDisplayOrders();
        }

        this.classList.remove('drag-over');
        return false;
    }

    async function updateDisplayOrders() {
        const tbody = document.querySelector('.categories-table tbody');
        const rows = Array.from(tbody.querySelectorAll('tr:not(.empty-table-state)'));
        
        const updates = rows.map((row, index) => {
            const categoryId = row.getAttribute('data-category-id');
            const displayOrder = index + 1;
            
            // Update the display in the UI immediately
            const displayOrderText = row.querySelector('.display-order-text');
            if (displayOrderText) {
                displayOrderText.textContent = displayOrder;
            }
            
            return {
                id: categoryId,
                displayOrder: displayOrder
            };
        });

        try {
            const response = await fetch('/Menu/UpdateCategoriesDisplayOrder', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(updates)
            });

            const result = await response.json();

            if (result.success) {
                // Update local cache
                updates.forEach(update => {
                    const category = allCategories.find(c => c.id === update.id);
                    if (category) {
                        category.displayOrder = update.displayOrder;
                    }
                });
                
                showToast('Display order updated successfully!', 'light', 'Cofinoy');
            } else {
                throw new Error(result.error || 'Failed to update display order');
            }
        } catch (error) {
            console.error('Error updating display order:', error);
            showToast('Error updating display order: ' + error.message, 'light', 'Error');
            // Reload to revert changes
            await loadCategories();
        }
    }

    function editCategory(row, categoryData) {
        const categoryId = row.getAttribute('data-category-id');
        const categoryName = row.querySelector('.category-name').textContent;
        const categoryDescription = row.querySelector('.category-description').textContent;
        const categoryStatus = row.querySelector('.status-badge').textContent;
        const displayOrder = row.querySelector('.display-order-text').textContent;

        document.getElementById('categoryName').value = categoryName;
        document.getElementById('categoryDescription').value = categoryDescription;
        document.getElementById('categoryStatus').value = categoryStatus.charAt(0).toUpperCase() + categoryStatus.slice(1);
        document.getElementById('categoryDisplayOrder').value = displayOrder;

        document.querySelector('.modal-header h2').textContent = 'Edit Category';
        document.getElementById('addCategoryBtn').innerHTML = '<i class="fas fa-save"></i> Save Changes';

        currentEditingRow = row;
        currentEditingId = categoryId;

        openModal();
    }

    function deleteCategory(row, categoryData) {
        openDeleteModal(row, categoryData);
    }

    async function confirmDelete() {
        if (!currentDeletingRow || !currentDeletingCategory) {
            closeDeleteModal();
            return;
        }

        const categoryId = currentDeletingRow.getAttribute('data-category-id');
        
        const originalBtnContent = confirmDeleteBtn.innerHTML;
        confirmDeleteBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Deleting...';
        confirmDeleteBtn.disabled = true;

        try {
            currentDeletingRow.style.opacity = '0.5';
            currentDeletingRow.style.pointerEvents = 'none';

            const response = await fetch(`/Menu/DeleteCategory?id=${categoryId}`, {
                method: 'POST'
            });

            const result = await response.json();

            if (result.success) {
                showToast('Category deleted successfully!', 'light', 'Cofinoy');
                closeDeleteModal();
                await loadCategories();
            } else {
                throw new Error(result.error || 'Failed to delete category');
            }

        } catch (error) {
            console.error('Error deleting category:', error);
            showToast('Error deleting category: ' + error.message, 'light', 'Error');

            currentDeletingRow.style.opacity = '1';
            currentDeletingRow.style.pointerEvents = 'auto';
        } finally {
            confirmDeleteBtn.innerHTML = originalBtnContent;
            confirmDeleteBtn.disabled = false;
        }
    }

    function addActionButtonListeners(row, categoryData) {
        const editBtn = row.querySelector('.edit-btn');
        const deleteBtn = row.querySelector('.delete-btn');

        editBtn.addEventListener('click', function (e) {
            e.stopPropagation();
            editCategory(row, categoryData);
        });

        deleteBtn.addEventListener('click', function (e) {
            e.stopPropagation();
            deleteCategory(row, categoryData);
        });
    }

    function clearAllFilters() {
        if (statusFilter) statusFilter.value = '';
        searchInput.value = '';
        applyFilters();
    }

    function applyFiltersAndClose() {
        applyFilters();
        closeFilterModal();
    }

    function applyFilters() {
        const searchTerm = searchInput.value.toLowerCase().trim();
        const statusFilterValue = statusFilter?.value || '';

        const allRows = document.querySelectorAll('.categories-table tbody tr:not(.empty-table-state)');
        let visibleCount = 0;

        allRows.forEach(row => {
            const categoryName = row.querySelector('.category-name')?.textContent.toLowerCase() || '';
            const categoryDescription = row.querySelector('.category-description')?.textContent.toLowerCase() || '';
            const categoryStatus = row.querySelector('.status-badge')?.textContent || '';

            const matchesSearch = !searchTerm || 
                categoryName.includes(searchTerm) || 
                categoryDescription.includes(searchTerm);

            const matchesStatus = !statusFilterValue || categoryStatus === statusFilterValue;

            if (matchesSearch && matchesStatus) {
                row.style.display = '';
                visibleCount++;
            } else {
                row.style.display = 'none';
            }
        });

        updateResultsCount(visibleCount);

        const tbody = document.querySelector('.categories-table tbody');
        const existingEmptyState = tbody.querySelector('.empty-table-state');
        
        if (visibleCount === 0 && !existingEmptyState) {
            const emptyStateRow = document.createElement('tr');
            emptyStateRow.className = 'empty-table-state';
            emptyStateRow.innerHTML = `
                <td colspan="6">
                    <div class="empty-table-content">
                        <div class="empty-table-icon">
                            <img src="https://cdn-icons-png.flaticon.com/512/2603/2603910.png" />
                        </div>
                        <h3 class="empty-table-message">No categories found</h3>
                        <p class="empty-table-submessage">Try adjusting your filters</p>
                    </div>
                </td>
            `;
            tbody.appendChild(emptyStateRow);
        } else if (visibleCount > 0 && existingEmptyState) {
            existingEmptyState.remove();
        }
    }

    function updateResultsCount(count) {
        const countElement = document.querySelector('.drinks-count');
        if (count === 0) {
            countElement.textContent = `No categories yet`;
        } else if (count === 1) {
            countElement.textContent = `${count} category in your system.`;
        } else {
            countElement.textContent = `${count} categories in your system.`;
        }
    }

    addCategoryBtn.addEventListener('click', openModal);
    closeModal.addEventListener('click', closeModalFunc);
    cancelBtn.addEventListener('click', closeModalFunc);

    if (filterBtn) filterBtn.addEventListener('click', openFilterModal);
    if (closeFilterModalBtn) closeFilterModalBtn.addEventListener('click', closeFilterModal);
    if (clearFiltersBtn) clearFiltersBtn.addEventListener('click', clearAllFilters);
    if (applyFiltersBtn) applyFiltersBtn.addEventListener('click', applyFiltersAndClose);
    
    if (cancelDeleteBtn) cancelDeleteBtn.addEventListener('click', closeDeleteModal);
    if (confirmDeleteBtn) confirmDeleteBtn.addEventListener('click', confirmDelete);

    modal.addEventListener('click', function (e) {
        if (e.target === modal) {
            closeModalFunc();
        }
    });

    if (filterModal) {
        filterModal.addEventListener('click', function (e) {
            if (e.target === filterModal) {
                closeFilterModal();
            }
        });
    }

    if (deleteModal) {
        deleteModal.addEventListener('click', function (e) {
            if (e.target === deleteModal) {
                closeDeleteModal();
            }
        });
    }

    searchInput.addEventListener('input', applyFilters);

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            if (modal.classList.contains('active')) {
                closeModalFunc();
            }
            if (filterModal && filterModal.classList.contains('active')) {
                closeFilterModal();
            }
            if (deleteModal && deleteModal.classList.contains('active')) {
                closeDeleteModal();
            }
        }

        if ((e.ctrlKey || e.metaKey) && e.key === 'n') {
            e.preventDefault();
            openModal();
        }

        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            searchInput.focus();
        }
    });
});