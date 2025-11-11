document.addEventListener('DOMContentLoaded', function () {
    const modal = document.getElementById('addCategoryModal');
    const addCategoryBtn = document.querySelector('.add-category-btn');
    const closeModal = document.getElementById('closeModal');
    const cancelBtn = document.getElementById('cancelBtn');
    const form = document.getElementById('addCategoryForm');
    const searchInput = document.querySelector('.search-input');

    let currentEditingRow = null;
    let currentEditingId = null;

    loadCategories();

    function openModal() {
        modal.style.display = 'flex';
        document.body.style.overflow = 'hidden';
    }

    function closeModalFunc() {
        modal.style.display = 'none';
        document.body.style.overflow = 'auto';
        resetForm();
        resetModalState();
    }

    function resetForm() {
        form.reset();
    }

    function resetModalState() {
        document.querySelector('.modal-header h2').textContent = 'Add new category';
        document.getElementById('addCategoryBtn').innerHTML = '<i class="fas fa-plus"></i> Add Category';
        currentEditingRow = null;
        currentEditingId = null;
    }

    form.addEventListener('submit', async function (e) {
        e.preventDefault();

        const submitBtn = document.getElementById('addCategoryBtn');
        const originalBtnContent = submitBtn.innerHTML;
        submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Processing...';
        submitBtn.disabled = true;

        const formData = {
            name: document.getElementById('categoryName').value.trim(),
            description: document.getElementById('categoryDescription').value.trim(),
            status: document.getElementById('categoryStatus').value,
            displayOrder: parseInt(document.getElementById('categoryDisplayOrder').value) || 0
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
                showToastMessage(currentEditingId ? 'Category updated successfully!' : 'Category added successfully!');
                closeModalFunc();
                loadCategories();
            } else {
                throw new Error(result.error || 'Failed to save category');
            }

        } catch (error) {
            console.error('Error saving category:', error);
            showToastMessage('Error saving category: ' + error.message, 'Error');
        } finally {
            submitBtn.innerHTML = originalBtnContent;
            submitBtn.disabled = false;
        }
    });

    async function loadCategories() {
        try {
            const response = await fetch('/Menu/GetAllCategories', {
                method: 'GET'
            });
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const result = await response.json();

            if (result.success) {
                const tbody = document.querySelector('.categories-table tbody');
                tbody.innerHTML = '';

                if (result.data && Array.isArray(result.data)) {
                    result.data.forEach(category => {
                        addCategoryToTable(category);
                    });
                }

                updateEmptyState();
                updateResultsCount(result.data ? result.data.length : 0);
            } else {
                console.error('Error loading categories:', result.error);
                showToastMessage('Error loading categories: ' + result.error, 'Error');
            }
        } catch (error) {
            console.error('Error loading categories:', error);
            showToastMessage('Error loading categories: ' + error.message, 'Error');
        }
    }

    function addCategoryToTable(categoryData) {
        const tbody = document.querySelector('.categories-table tbody');
        const newRow = document.createElement('tr');

        newRow.setAttribute('data-category-id', categoryData.id);

        newRow.innerHTML = `
            <td>
                <div class="category-info">
                    <span class="category-name">${categoryData.name}</span>
                </div>
            </td>
            <td class="category-description">${categoryData.description}</td>
            <td><span class="items-count">${categoryData.itemsCount || 0} items</span></td>
            <td><span class="status-badge ${categoryData.status.toLowerCase()}">${categoryData.status}</span></td>
            <td class="display-order">
                <span class="display-order-text">${categoryData.displayOrder || 0}</span>
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
        addActionButtonListeners(newRow);
    }

    function editCategory(row) {
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

    async function deleteCategory(row) {
        const categoryId = row.getAttribute('data-category-id');
        const categoryName = row.querySelector('.category-name').textContent;

        if (confirm(`Are you sure you want to delete the "${categoryName}" category? This action cannot be undone.`)) {
            try {
                row.style.opacity = '0.5';
                row.style.pointerEvents = 'none';

                const response = await fetch(`/Menu/DeleteCategory?id=${categoryId}`, {
                    method: 'POST'
                });

                const result = await response.json();

                if (result.success) {
                    showToastMessage('Category deleted successfully!');
                    loadCategories();
                } else {
                    throw new Error(result.error || 'Failed to delete category');
                }

            } catch (error) {
                console.error('Error deleting category:', error);
                showToastMessage('Error deleting category: ' + error.message, 'Error');

                row.style.opacity = '1';
                row.style.pointerEvents = 'auto';
            }
        }
    }

    function addActionButtonListeners(row) {
        const editBtn = row.querySelector('.edit-btn');
        const deleteBtn = row.querySelector('.delete-btn');

        editBtn.addEventListener('click', function () {
            editCategory(row);
        });

        deleteBtn.addEventListener('click', function () {
            deleteCategory(row);
        });
    }

    function showToastMessage(message, title = 'Cofinoy') {
        let toastContainer = document.getElementById('toastStackContainer');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.id = 'toastStackContainer';
            toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
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

        toastElement.innerHTML = `
        <div class="toast-header">
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

        // Use the correct Bootstrap Toast initialization
        try {
            const toastBootstrap = new bootstrap.Toast(toastElement, {
                delay: 4000,
                autohide: true
            });
            toastBootstrap.show();

            toastElement.addEventListener('hidden.bs.toast', function () {
                toastElement.remove();
            });
        } catch (error) {
            console.error('Toast initialization error:', error);
            // Fallback: manual display
            toastElement.classList.add('show');
            setTimeout(() => {
                toastElement.classList.remove('show');
                setTimeout(() => toastElement.remove(), 300);
            }, 4000);
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

    function updateEmptyState() {
        const tbody = document.querySelector('.categories-table tbody');
        const rows = tbody.querySelectorAll('tr:not(.empty-table-state)');
        const emptyState = tbody.querySelector('.empty-table-state');

        if (rows.length === 0) {
            if (!emptyState) {
                const emptyStateRow = document.createElement('tr');
                emptyStateRow.className = 'empty-table-state';
                emptyStateRow.innerHTML = `
                <td colspan="6">
                    <div class="empty-table-content">
                        <div class="empty-table-icon">
                            <i class="fas fa-folder-open"></i>
                        </div>
                        <h3 class="empty-table-message">No categories yet</h3>
                        <p class="empty-table-submessage">Add your first category to get started</p>
                    </div>
                </td>
            `;
                tbody.appendChild(emptyStateRow);
            }
        } else if (rows.length > 0) {
            if (emptyState) {
                emptyState.remove();
            }
        }
    }

    addCategoryBtn.addEventListener('click', openModal);
    closeModal.addEventListener('click', closeModalFunc);
    cancelBtn.addEventListener('click', closeModalFunc);

    modal.addEventListener('click', function (e) {
        if (e.target === modal) {
            closeModalFunc();
        }
    });

    searchInput.addEventListener('input', function () {
        const searchTerm = this.value.toLowerCase();
        const allRows = document.querySelectorAll('.categories-table tbody tr:not(.empty-table-state)');

        allRows.forEach(row => {
            const categoryName = row.querySelector('.category-name')?.textContent.toLowerCase() || '';
            const categoryDescription = row.querySelector('.category-description')?.textContent.toLowerCase() || '';

            if (categoryName.includes(searchTerm) || categoryDescription.includes(searchTerm)) {
                row.style.display = '';
            } else {
                row.style.display = 'none';
            }
        });

        const visibleRows = Array.from(allRows).filter(row => row.style.display !== 'none');
        updateResultsCount(visibleRows.length);
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && modal.style.display === 'flex') {
            closeModalFunc();
        }

        if ((e.ctrlKey || e.metaKey) && e.key === 'n') {
            e.preventDefault();
            openModal();
        }
    });
});