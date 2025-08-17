/**
 * Settings Management JavaScript
 * Handles dynamic popup editing for settings based on data types
 */

// Global variables
let originalValues = new Map();
let modifiedSettings = new Set();

// Initialize settings page
$(document).ready(function() {
    initializeSettingsPage();
    initializeThemeSelector();
});

function initializeSettingsPage() {
    // Store original values for tracking changes
    $('.setting-row').each(function() {
        const settingId = $(this).data('setting-id');
        const valueCell = $(this).find('.setting-value-display span');
        let originalValue = '';
        
        if (valueCell.hasClass('badge')) {
            // Boolean value
            originalValue = valueCell.text() === 'Enabled' ? 'true' : 'false';
        } else {
            originalValue = valueCell.attr('title') || valueCell.text();
        }
        
        originalValues.set(settingId, originalValue);
    });

    // Initialize category filtering
    $('.nav-link[data-category]').on('click', function(e) {
        e.preventDefault();
        const category = $(this).data('category');
        filterByCategory(category);
        
        // Update active state
        $('.nav-link[data-category]').removeClass('active');
        $(this).addClass('active');
    });

    // Initialize search functionality
    $('#searchBtn').on('click', performSearch);
    $('#searchInput').on('keypress', function(e) {
        if (e.which === 13) {
            performSearch();
        }
    });
    
    $('#clearFiltersBtn').on('click', clearFilters);
    $('#showModifiedOnly').on('change', applyFilters);

    updateModifiedCount();
}
$(document).ready(function() {
    initializeSettingsPage();
    initializeThemeSelector();
});

function initializeSettingsPage() {
    // Store original values for tracking changes
    $('.setting-row').each(function() {
        const settingId = $(this).data('setting-id');
        const valueSpan = $(this).find('.setting-value-display span');
        if (valueSpan.length > 0) {
            const value = valueSpan.hasClass('badge') ? 
                (valueSpan.text() === 'Enabled' ? 'true' : 'false') : 
                valueSpan.attr('title') || valueSpan.text();
            originalValues.set(settingId, value);
        }
    });

    // Initialize search functionality
    $('#searchInput').on('input', debounce(performSearch, 300));
    $('#searchBtn').on('click', performSearch);
    $('#clearFiltersBtn').on('click', clearSearch);
    $('#showModifiedOnly').on('change', performSearch);
    
    // Initialize category filtering
    $('.category-tab').on('click', function() {
        const category = $(this).data('category');
        filterByCategory(category);
    });

    updateModifiedCount();
}

// Debounce function for search
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Search functionality
function performSearch() {
    const searchTerm = $('#searchInput').val().toLowerCase();
    const showModifiedOnly = $('#showModifiedOnly').is(':checked');
    
    $('.setting-row').each(function() {
        const row = $(this);
        const searchText = row.data('search-text') || '';
        const isModified = row.hasClass('table-warning');
        
        const matchesSearch = !searchTerm || searchText.includes(searchTerm);
        const matchesModified = !showModifiedOnly || isModified;
        
        if (matchesSearch && matchesModified) {
            row.show();
        } else {
            row.hide();
        }
    });
}

function clearSearch() {
    $('#searchInput').val('');
    $('#showModifiedOnly').prop('checked', false);
    $('.setting-row').show();
}

function filterByCategory(category) {
    $('.category-tab').removeClass('active');
    $(`.category-tab[data-category="${category}"]`).addClass('active');
    
    if (category === 'all') {
        $('.setting-row').show();
    } else {
        $('.setting-row').each(function() {
            const rowCategory = $(this).data('category');
            if (rowCategory === category) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    }
}

// Main edit setting function
function editSetting(settingId) {
    // Get setting data from the row
    const row = document.querySelector(`tr[data-setting-id="${settingId}"]`);
    if (!row) return;
    
    // Extract setting information from the row
    const settingData = extractSettingDataFromRow(row);
    if (!settingData) return;
    
    // Show the dynamic edit modal
    showEditSettingModal(settingData);
}

function extractSettingDataFromRow(row) {
    try {
        const nameCell = row.querySelector('td:first-child');
        const categoryCell = row.querySelector('td:nth-child(2)');
        const valueCell = row.querySelector('td:nth-child(3)');
        const typeCell = row.querySelector('td:nth-child(4)');
        
        if (!nameCell || !categoryCell || !valueCell || !typeCell) return null;
        
        const displayName = nameCell.querySelector('.fw-semibold')?.textContent?.replace('*', '').trim();
        const key = nameCell.querySelector('small')?.textContent?.trim();
        const description = nameCell.querySelectorAll('small')[1]?.textContent?.trim() || '';
        const category = categoryCell.querySelector('.badge')?.textContent?.trim();
        const dataType = typeCell.querySelector('.badge')?.textContent?.trim();
        const isRequired = nameCell.querySelector('.text-danger') !== null;
        const isReadOnly = row.querySelector('td:nth-child(5) .badge')?.textContent?.includes('Read-Only') || false;
        
        // Extract current value
        let currentValue = '';
        const valueSpan = valueCell.querySelector('span');
        if (valueSpan) {
            if (valueSpan.classList.contains('badge')) {
                // Boolean value
                currentValue = valueSpan.textContent === 'Enabled' ? 'true' : 'false';
            } else {
                // Regular value
                currentValue = valueSpan.getAttribute('title') || valueSpan.textContent?.trim() || '';
            }
        }
        
        return {
            settingId: row.getAttribute('data-setting-id'),
            displayName,
            key,
            description,
            category,
            dataType: dataType?.toLowerCase(),
            currentValue,
            isRequired,
            isReadOnly,
            possibleValues: row.getAttribute('data-possible-values') || ''
        };
    } catch (error) {
        console.error('Error extracting setting data:', error);
        return null;
    }
}

function showEditSettingModal(settingData) {
    // Create modal HTML
    const modalHtml = createEditSettingModalHtml(settingData);
    
    // Remove existing modal if any
    const existingModal = document.getElementById('editSettingModal');
    if (existingModal) {
        existingModal.remove();
    }
    
    // Add modal to body
    document.body.insertAdjacentHTML('beforeend', modalHtml);
    
    // Initialize modal
    const modal = new bootstrap.Modal(document.getElementById('editSettingModal'));
    
    // Setup form controls based on data type
    setupModalFormControls(settingData);
    
    // Show modal
    modal.show();
    
    // Focus on first input
    setTimeout(() => {
        const firstInput = document.querySelector('#editSettingModal input, #editSettingModal textarea, #editSettingModal select');
        if (firstInput) firstInput.focus();
    }, 300);
}

function createEditSettingModalHtml(settingData) {
    return `
        <div class="modal fade" id="editSettingModal" tabindex="-1" aria-labelledby="editSettingModalLabel" aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="editSettingModalLabel">
                            <i class="fas fa-edit me-2"></i>Edit Setting: ${settingData.displayName}
                        </h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        ${createSettingInfoSection(settingData)}
                        ${createSettingFormSection(settingData)}
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                            <i class="fas fa-times me-1"></i>Cancel
                        </button>
                        <button type="button" class="btn btn-outline-warning" onclick="resetToDefault('${settingData.settingId}')">
                            <i class="fas fa-undo me-1"></i>Reset to Default
                        </button>
                        <button type="button" class="btn btn-primary" onclick="saveSettingFromModal('${settingData.settingId}')" ${settingData.isReadOnly ? 'disabled' : ''}>
                            <i class="fas fa-save me-1"></i>Save Changes
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;
}

function createSettingInfoSection(settingData) {
    return `
        <div class="row mb-4">
            <div class="col-12">
                <div class="card bg-light">
                    <div class="card-body py-3">
                        <div class="row">
                            <div class="col-md-6">
                                <div class="mb-2">
                                    <strong>Key:</strong> <code>${settingData.key}</code>
                                </div>
                                <div class="mb-2">
                                    <strong>Category:</strong> <span class="badge bg-secondary">${settingData.category}</span>
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="mb-2">
                                    <strong>Type:</strong> <span class="badge bg-info">${settingData.dataType}</span>
                                </div>
                                <div class="mb-2">
                                    <strong>Required:</strong> <span class="badge ${settingData.isRequired ? 'bg-danger' : 'bg-success'}">${settingData.isRequired ? 'Yes' : 'No'}</span>
                                </div>
                            </div>
                        </div>
                        ${settingData.description ? `<div class="mt-2"><strong>Description:</strong> ${settingData.description}</div>` : ''}
                    </div>
                </div>
            </div>
        </div>
    `;
}

function createSettingFormSection(settingData) {
    return `
        <div class="row">
            <div class="col-12">
                <div class="mb-3">
                    <label class="form-label fw-semibold">Current Value</label>
                    <div id="settingValueContainer">
                        ${createFormControlForType(settingData)}
                    </div>
                </div>
                
                <div class="mb-3">
                    <label class="form-label fw-semibold">Default Value</label>
                    <div class="alert alert-info py-2">
                        <i class="fas fa-info-circle me-2"></i>
                        <span id="defaultValueDisplay">Loading default value...</span>
                    </div>
                </div>
                
                ${settingData.isReadOnly ? '<div class="alert alert-warning"><i class="fas fa-lock me-2"></i>This setting is read-only and cannot be modified.</div>' : ''}
            </div>
        </div>
    `;
}

function createFormControlForType(settingData) {
    const type = settingData.dataType;
    const value = settingData.currentValue;
    const disabled = settingData.isReadOnly ? 'disabled' : '';
    
    console.log('Creating form control for type:', type, 'value:', value, 'settingData:', settingData);
    
    // Special debug for currency setting
    if (settingData.key === 'AvailableCurrencies' || settingData.displayName === 'Available Currencies') {
        console.log('*** DEBUGGING CURRENCY SETTING ***');
        console.log('Setting data:', JSON.stringify(settingData, null, 2));
        console.log('Possible values raw:', settingData.possibleValues);
        console.log('Data type:', settingData.dataType);
    }
    
    switch (type) {
        case 'boolean':
        case 'bool':
            const isChecked = value === 'true' || value?.toLowerCase() === 'true';
            return `
                <div class="form-check form-switch">
                    <input type="checkbox" class="form-check-input" id="settingValue" ${isChecked ? 'checked' : ''} ${disabled}>
                    <label class="form-check-label" for="settingValue" id="booleanLabel">${isChecked ? 'Enabled' : 'Disabled'}</label>
                </div>
            `;
        
        case 'int':
        case 'integer':
        case 'number':
            return `<input type="number" class="form-control" id="settingValue" value="${value}" ${disabled}>`;
        
        case 'decimal':
        case 'float':
        case 'double':
            return `<input type="number" step="0.01" class="form-control" id="settingValue" value="${value}" ${disabled}>`;
        
        case 'email':
            return `<input type="email" class="form-control" id="settingValue" value="${value}" ${disabled}>`;
        
        case 'url':
            return `<input type="url" class="form-control" id="settingValue" value="${value}" ${disabled}>`;
        
        case 'password':
            return `
                <div class="input-group">
                    <input type="password" class="form-control" id="settingValue" value="${value}" ${disabled}>
                    <button class="btn btn-outline-secondary" type="button" onclick="togglePasswordVisibility()">
                        <i class="fas fa-eye" id="passwordToggleIcon"></i>
                    </button>
                </div>
            `;
        
        case 'json':
            return `
                <textarea class="form-control font-monospace" id="settingValue" rows="8" ${disabled}>${value}</textarea>
                <div class="form-text">Enter valid JSON format</div>
            `;
        
        case 'list':
        case 'array':
        case 'select':
        case 'dropdown':
        case 'enum':
            return createListDropdown(value, disabled, settingData);
        
        case 'multi-list':
        case 'multilist':
        case 'multiple':
        case 'checkbox-list':
            return createMultiListDropdown(value, disabled, settingData);
        
        case 'image':
        case 'file':
            return createImageUpload(value, disabled, settingData);
        
        case 'multiline':
        case 'text':
            if (value && value.length > 100) {
                return `<textarea class="form-control" id="settingValue" rows="4" ${disabled}>${value}</textarea>`;
            }
            // Fall through to default for short text
        
        default:
            return `<input type="text" class="form-control" id="settingValue" value="${value}" ${disabled}>`;
    }
}

function createListDropdown(value, disabled, settingData) {
    // Extract possible values from setting data
    let possibleValues = [];
    console.log('Creating dropdown for:', settingData);
    console.log('possibleValues attribute:', settingData.possibleValues);
    
    try {
        const possibleValuesAttr = settingData.possibleValues;
        if (possibleValuesAttr) {
            // First try to parse as JSON
            try {
                possibleValues = JSON.parse(possibleValuesAttr);
            } catch {
                // If JSON parsing fails, try splitting by comma/semicolon
                possibleValues = possibleValuesAttr.split(/[,;|]/).map(v => v.trim()).filter(v => v);
            }
            
            // Handle both array of strings and array of objects
            if (possibleValues.length > 0 && typeof possibleValues[0] === 'object') {
                // For objects, use the first string property as the value
                possibleValues = possibleValues.map(item => {
                    const firstStringProp = Object.keys(item).find(key => typeof item[key] === 'string');
                    return firstStringProp ? item[firstStringProp] : JSON.stringify(item);
                });
            }
        }
    } catch (error) {
        console.error('Error parsing possible values:', error);
        // Fallback to basic list if no possible values
        possibleValues = ['USD', 'EUR', 'GBP', 'JPY', 'CAD'];
    }
    
    console.log('Possible values after processing:', possibleValues);
    
    // If no possible values for currencies specifically, provide defaults
    if (possibleValues.length === 0) {
        if (settingData.key && settingData.key.toLowerCase().includes('currenc')) {
            possibleValues = ['USD', 'EUR', 'GBP', 'JPY', 'CAD', 'AUD', 'CHF', 'CNY'];
        } else if (settingData.displayName && settingData.displayName.toLowerCase().includes('currenc')) {
            possibleValues = ['USD', 'EUR', 'GBP', 'JPY', 'CAD', 'AUD', 'CHF', 'CNY'];
        } else {
            possibleValues = ['Option 1', 'Option 2', 'Option 3'];
        }
    }
    
    console.log('Final possible values:', possibleValues);
    
    return `
        <select class="form-select" id="settingValue" ${disabled}>
            <option value="">-- Select an option --</option>
            ${possibleValues.map(option => `
                <option value="${option}" ${option === value ? 'selected' : ''}>${option}</option>
            `).join('')}
        </select>
        <div class="form-text">Select a single value from the available options</div>
    `;
}

function createMultiListDropdown(value, disabled, settingData) {
    // Extract possible values from setting data
    let possibleValues = [];
    let selectedValues = [];
    
    try {
        const possibleValuesAttr = settingData.possibleValues;
        if (possibleValuesAttr) {
            try {
                possibleValues = JSON.parse(possibleValuesAttr);
            } catch {
                possibleValues = possibleValuesAttr.split(/[,;|]/).map(v => v.trim()).filter(v => v);
            }
        }
        
        // Parse current value as array
        if (value) {
            try {
                selectedValues = JSON.parse(value);
                if (!Array.isArray(selectedValues)) {
                    selectedValues = value.split(/[,;|]/).map(v => v.trim()).filter(v => v);
                }
            } catch {
                selectedValues = value.split(/[,;|]/).map(v => v.trim()).filter(v => v);
            }
        }
    } catch (error) {
        console.error('Error parsing multi-list values:', error);
        possibleValues = ['Option 1', 'Option 2', 'Option 3'];
        selectedValues = [];
    }
    
    if (possibleValues.length === 0) {
        possibleValues = ['Option 1', 'Option 2', 'Option 3'];
    }
    
    console.log('Multi-list possible values:', possibleValues);
    console.log('Multi-list selected values:', selectedValues);
    
    const checkboxes = possibleValues.map((option, index) => `
        <div class="form-check">
            <input class="form-check-input multi-list-option" type="checkbox" 
                   value="${option}" id="multiOption${index}" 
                   ${selectedValues.includes(option) ? 'checked' : ''} ${disabled}>
            <label class="form-check-label" for="multiOption${index}">
                ${option}
            </label>
        </div>
    `).join('');
    
    return `
        <div class="multi-list-container">
            <input type="hidden" id="settingValue" value='${JSON.stringify(selectedValues)}'>
            <div class="border rounded p-3" style="max-height: 200px; overflow-y: auto;">
                ${checkboxes}
            </div>
            <div class="form-text">Select multiple values from the available options</div>
        </div>
    `;
}

function createImageUpload(value, disabled, settingData) {
    const hasImage = value && value.length > 0;
    let imagePreview = '';
    
    if (hasImage) {
        // Check if value already contains data URI prefix
        const imageSrc = value.startsWith('data:') ? value : `data:image/jpeg;base64,${value}`;
        imagePreview = `<div class="image-preview mb-3">
            <img src="${imageSrc}" alt="Current Image" 
                 style="max-width: 200px; max-height: 200px; border: 1px solid #ddd; border-radius: 4px;">
        </div>`;
    }
    
    return `
        <div class="image-upload-container">
            <input type="hidden" id="settingValue" value="${value || ''}">
            ${imagePreview}
            <div class="mb-3">
                <input type="file" class="form-control" id="imageFileInput" 
                       accept="image/*" ${disabled} onchange="handleImageUpload(this)">
            </div>
            <div class="form-text">
                Upload an image file (JPG, PNG, GIF). The image will be converted to base64 and stored in the database.
                ${hasImage ? '<br><strong>Current:</strong> Image uploaded' : ''}
            </div>
            ${hasImage ? `
                <button type="button" class="btn btn-sm btn-outline-danger" 
                        onclick="clearImage()" ${disabled}>
                    <i class="fas fa-trash"></i> Remove Image
                </button>
            ` : ''}
        </div>
    `;
}

function setupModalFormControls(settingData) {
    // Setup boolean toggle label update
    const booleanInput = document.getElementById('settingValue');
    if (booleanInput && booleanInput.type === 'checkbox') {
        booleanInput.addEventListener('change', function() {
            const label = document.getElementById('booleanLabel');
            if (label) {
                label.textContent = this.checked ? 'Enabled' : 'Disabled';
            }
        });
    }
    
    // Setup multi-list checkbox handlers
    if (settingData.dataType === 'multi-list' || settingData.dataType === 'multilist' || settingData.dataType === 'multiple') {
        setupMultiListHandlers();
    }
    
    // Load default value
    loadDefaultValue(settingData.settingId);
    
    // Setup JSON validation for JSON type
    if (settingData.dataType === 'json') {
        const jsonInput = document.getElementById('settingValue');
        if (jsonInput) {
            jsonInput.addEventListener('blur', validateJsonInput);
        }
    }
}

function setupMultiListHandlers() {
    const checkboxes = document.querySelectorAll('.multi-list-option');
    const hiddenInput = document.getElementById('settingValue');
    
    checkboxes.forEach(checkbox => {
        checkbox.addEventListener('change', function() {
            const selectedValues = [];
            checkboxes.forEach(cb => {
                if (cb.checked) {
                    selectedValues.push(cb.value);
                }
            });
            hiddenInput.value = JSON.stringify(selectedValues);
        });
    });
}

function handleImageUpload(fileInput) {
    const file = fileInput.files[0];
    if (!file) return;
    
    // Validate file type
    if (!file.type.startsWith('image/')) {
        if (typeof showToast === 'function') {
            showToast('Please select a valid image file', 'error');
        } else {
            alert('Please select a valid image file');
        }
        fileInput.value = '';
        return;
    }
    
    // Validate file size (max 5MB)
    if (file.size > 5 * 1024 * 1024) {
        if (typeof showToast === 'function') {
            showToast('Image file must be smaller than 5MB', 'error');
        } else {
            alert('Image file must be smaller than 5MB');
        }
        fileInput.value = '';
        return;
    }
    
    const reader = new FileReader();
    reader.onload = function(e) {
        const dataUri = e.target.result; // Full data URI (data:image/jpeg;base64,...)
        const hiddenInput = document.getElementById('settingValue');
        hiddenInput.value = dataUri; // Store full data URI for consistency
        
        // Update preview
        updateImagePreview(dataUri);
    };
    reader.readAsDataURL(file);
}

function updateImagePreview(dataUri) {
    const container = document.querySelector('.image-upload-container');
    if (!container) return;
    
    // Remove existing preview
    const existingPreview = container.querySelector('.image-preview');
    if (existingPreview) {
        existingPreview.remove();
    }
    
    // Add new preview - dataUri should already have the full data URI format
    const preview = document.createElement('div');
    preview.className = 'image-preview mb-3';
    preview.innerHTML = `
        <img src="${dataUri}" alt="Image Preview" 
             style="max-width: 200px; max-height: 200px; border: 1px solid #ddd; border-radius: 4px;">
    `;
    
    const fileInput = container.querySelector('#imageFileInput');
    container.insertBefore(preview, fileInput.parentNode);
    
    // Add remove button if not exists
    if (!container.querySelector('.btn-outline-danger')) {
        const formText = container.querySelector('.form-text');
        const removeBtn = document.createElement('button');
        removeBtn.type = 'button';
        removeBtn.className = 'btn btn-sm btn-outline-danger';
        removeBtn.onclick = clearImage;
        removeBtn.innerHTML = '<i class="fas fa-trash"></i> Remove Image';
        formText.parentNode.insertBefore(removeBtn, formText.nextSibling);
    }
}

function clearImage() {
    const hiddenInput = document.getElementById('settingValue');
    const fileInput = document.getElementById('imageFileInput');
    const container = document.querySelector('.image-upload-container');
    
    hiddenInput.value = '';
    fileInput.value = '';
    
    // Remove preview
    const preview = container.querySelector('.image-preview');
    if (preview) {
        preview.remove();
    }
    
    // Remove button
    const removeBtn = container.querySelector('.btn-outline-danger');
    if (removeBtn) {
        removeBtn.remove();
    }
}

function loadDefaultValue(settingId) {
    const defaultDisplay = document.getElementById('defaultValueDisplay');
    if (!defaultDisplay) return;
    
    $.ajax({
        url: '/Settings/GetDefaultValue',
        type: 'GET',
        data: { settingId: settingId },
        success: function(response) {
            if (response.success) {
                const defaultValue = response.defaultValue || 'Not set';
                defaultDisplay.innerHTML = `<code>${defaultValue}</code>`;
            } else {
                defaultDisplay.textContent = 'Unable to load default value';
            }
        },
        error: function() {
            defaultDisplay.textContent = 'Unable to load default value';
        }
    });
}

function validateJsonInput() {
    const input = this;
    const value = input.value.trim();
    
    if (!value) return;
    
    try {
        JSON.parse(value);
        input.classList.remove('is-invalid');
        input.classList.add('is-valid');
    } catch (error) {
        input.classList.remove('is-valid');
        input.classList.add('is-invalid');
        
        // Show error message
        let feedback = input.parentNode.querySelector('.invalid-feedback');
        if (!feedback) {
            feedback = document.createElement('div');
            feedback.className = 'invalid-feedback';
            input.parentNode.appendChild(feedback);
        }
        feedback.textContent = 'Invalid JSON format: ' + error.message;
    }
}

function togglePasswordVisibility() {
    const passwordInput = document.getElementById('settingValue');
    const toggleIcon = document.getElementById('passwordToggleIcon');
    
    if (passwordInput.type === 'password') {
        passwordInput.type = 'text';
        toggleIcon.classList.remove('fa-eye');
        toggleIcon.classList.add('fa-eye-slash');
    } else {
        passwordInput.type = 'password';
        toggleIcon.classList.remove('fa-eye-slash');
        toggleIcon.classList.add('fa-eye');
    }
}

function getFormValue(settingData) {
    const input = document.getElementById('settingValue');
    if (!input) return null;
    
    const type = settingData.dataType;
    
    switch (type) {
        case 'boolean':
        case 'bool':
            return input.checked.toString();
        
        case 'list':
        case 'array':
            // For dropdown selections, return the selected value directly
            return input.value;
            
        case 'multi-list':
        case 'multilist':
        case 'multiple':
        case 'checkbox-list':
            // For multi-list, return the JSON array value
            return input.value;
            
        case 'image':
        case 'file':
            // For images, return the base64 string
            return input.value;
        
        case 'json':
            try {
                const parsed = JSON.parse(input.value);
                return JSON.stringify(parsed); // Normalize the JSON
            } catch {
                return input.value; // Return as-is if invalid, let server handle validation
            }
        
        case 'int':
        case 'integer':
        case 'number':
        case 'decimal':
        case 'float':
        case 'double':
            return input.value;
        
        default:
            return input.value;
    }
}

function saveSettingFromModal(settingId) {
    const modal = document.getElementById('editSettingModal');
    const settingData = extractSettingDataFromModal();
    const newValue = getFormValue(settingData);
    
    if (newValue === null) {
        showToast('Please provide a valid value', 'error');
        return;
    }
    
    // Validate JSON if needed
    if (settingData.dataType === 'json') {
        try {
            JSON.parse(newValue);
        } catch {
            showToast('Please provide valid JSON format', 'error');
            return;
        }
    }
    
    // Show loading state
    const saveBtn = modal.querySelector('.btn-primary');
    const originalText = saveBtn.innerHTML;
    saveBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Saving...';
    saveBtn.disabled = true;
    
    $.ajax({
        url: '/Settings/UpdateSetting',
        type: 'POST',
        data: JSON.stringify({ settingId: settingId, value: newValue }),
        contentType: 'application/json',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                showToast('Setting saved successfully', 'success');
                
                // Update the table row
                updateTableRow(settingId, newValue, settingData.dataType);
                
                // Close modal
                bootstrap.Modal.getInstance(modal).hide();
                
                // Update counters
                updateModifiedCount();
            } else {
                showToast(response.message || 'Error saving setting', 'error');
            }
        },
        error: function() {
            showToast('Error saving setting', 'error');
        },
        complete: function() {
            saveBtn.innerHTML = originalText;
            saveBtn.disabled = false;
        }
    });
}

function extractSettingDataFromModal() {
    const modal = document.getElementById('editSettingModal');
    const title = modal.querySelector('.modal-title').textContent;
    const typeSpan = modal.querySelector('.badge.bg-info');
    
    return {
        dataType: typeSpan ? typeSpan.textContent.toLowerCase() : 'string'
    };
}

function updateTableRow(settingId, newValue, dataType) {
    const row = document.querySelector(`tr[data-setting-id="${settingId}"]`);
    if (!row) return;
    
    const valueCell = row.querySelector('.setting-value-display');
    if (!valueCell) return;
    
    // Update value display based on type
    let displayHtml = '';
    if (dataType === 'boolean' || dataType === 'bool') {
        const isEnabled = newValue === 'true' || newValue?.toLowerCase() === 'true';
        displayHtml = `<span class="badge ${isEnabled ? 'bg-success' : 'bg-secondary'}">${isEnabled ? 'Enabled' : 'Disabled'}</span>`;
    } else if (dataType === 'multi-list' || dataType === 'multilist' || dataType === 'multiple' || dataType === 'checkbox-list') {
        try {
            const selectedValues = JSON.parse(newValue);
            if (Array.isArray(selectedValues)) {
                const displayText = selectedValues.length > 0 ? selectedValues.join(', ') : 'No items selected';
                const truncatedText = displayText.length > 50 ? displayText.substring(0, 50) + '...' : displayText;
                displayHtml = `<span class="text-truncate d-inline-block" style="max-width: 200px;" title="${displayText}">${truncatedText}</span>`;
            } else {
                displayHtml = `<span class="text-muted">Invalid multi-list value</span>`;
            }
        } catch (e) {
            displayHtml = `<span class="text-danger">Invalid JSON format</span>`;
        }
    } else if (dataType === 'image' || dataType === 'file') {
        if (newValue && newValue.startsWith('data:image/')) {
            displayHtml = `<div class="d-flex align-items-center">
                <img src="${newValue}" class="img-thumbnail me-2" style="max-width: 40px; max-height: 40px;">
                <span class="text-muted">Image uploaded</span>
            </div>`;
        } else {
            displayHtml = `<span class="text-muted">No image</span>`;
        }
    } else {
        const displayValue = newValue.length > 50 ? newValue.substring(0, 50) + '...' : newValue;
        displayHtml = `<span class="text-truncate d-inline-block" style="max-width: 200px;" title="${newValue}">${displayValue}</span>`;
    }
    
    valueCell.innerHTML = displayHtml;
    
    // Update status
    const statusCell = row.querySelector('td:nth-child(5)');
    if (statusCell) {
        statusCell.innerHTML = '<span class="badge bg-warning">Modified</span>';
    }
    
    // Add modified styling
    row.classList.add('table-warning');
    
    // Add modified indicator
    const nameCell = row.querySelector('td:first-child .fw-semibold');
    if (nameCell && !nameCell.querySelector('.fa-circle')) {
        nameCell.insertAdjacentHTML('beforeend', '<i class="fas fa-circle text-warning ms-1" title="Modified"></i>');
    }
    
    // Update tracking
    modifiedSettings.add(settingId);
}

function resetToDefault(settingId) {
    if (!confirm('Are you sure you want to reset this setting to its default value?')) {
        return;
    }
    
    const modal = document.getElementById('editSettingModal');
    const resetBtn = modal.querySelector('.btn-outline-warning');
    const originalText = resetBtn.innerHTML;
    resetBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Resetting...';
    resetBtn.disabled = true;
    
    $.ajax({
        url: '/Settings/ResetSetting',
        type: 'POST',
        data: JSON.stringify({ settingId: settingId }),
        contentType: 'application/json',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                showToast('Setting reset to default value', 'success');
                
                // Update the form with default value
                const defaultValue = response.defaultValue || '';
                updateModalFormValue(defaultValue);
                
                // Update the table row
                updateTableRow(settingId, defaultValue, extractSettingDataFromModal().dataType);
                
                updateModifiedCount();
            } else {
                showToast(response.message || 'Error resetting setting', 'error');
            }
        },
        error: function() {
            showToast('Error resetting setting', 'error');
        },
        complete: function() {
            resetBtn.innerHTML = originalText;
            resetBtn.disabled = false;
        }
    });
}

function updateModalFormValue(value) {
    const input = document.getElementById('settingValue');
    if (!input) return;
    
    if (input.type === 'checkbox') {
        const isChecked = value === 'true' || value?.toLowerCase() === 'true';
        input.checked = isChecked;
        const label = document.getElementById('booleanLabel');
        if (label) label.textContent = isChecked ? 'Enabled' : 'Disabled';
    } else if (input.classList.contains('list-editor')) {
        // Handle list editor
        try {
            const items = JSON.parse(value);
            rebuildListEditor(items);
        } catch {
            input.value = value;
        }
    } else {
        input.value = value;
    }
}

function rebuildListEditor(items) {
    const listItems = document.getElementById('listItems');
    if (!listItems) return;
    
    listItems.innerHTML = '';
    
    items.forEach(item => {
        const itemHtml = `
            <div class="input-group mb-2 list-item">
                <input type="text" class="form-control" value="${item}">
                <button class="btn btn-outline-danger" type="button" onclick="removeListItem(this)">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `;
        listItems.insertAdjacentHTML('beforeend', itemHtml);
    });
}

// Utility functions
function showLoadingState(button, loading) {
    if (loading) {
        button.disabled = true;
        const icon = button.querySelector('i');
        if (icon) icon.classList.add('fa-spin');
    } else {
        button.disabled = false;
        const icon = button.querySelector('i');
        if (icon) icon.classList.remove('fa-spin');
    }
}

function showToast(message, type = 'info') {
    const alertClass = type === 'success' ? 'alert-success' : 
                     type === 'error' ? 'alert-danger' : 
                     type === 'warning' ? 'alert-warning' : 'alert-info';
    
    const toast = $(`<div class="alert ${alertClass} alert-dismissible fade show toast-notification" style="position: fixed; top: 20px; right: 20px; z-index: 9999; min-width: 300px;">
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    </div>`);
    
    $('body').append(toast);
    setTimeout(() => toast.alert('close'), 5000);
}

function updateModifiedCount() {
    const modifiedCount = $('.setting-row.table-warning').length;
    $('.modified-count').text(modifiedCount);
}

// Placeholder functions for quick actions
function exportSettings() {
    showToast('Export functionality coming soon', 'info');
}

function importSettings() {
    showToast('Import functionality coming soon', 'info');
}

function validateAllSettings() {
    showToast('Validation functionality coming soon', 'info');
}

function resetAllModified() {
    if (!confirm('Are you sure you want to reset all modified settings to their default values?')) {
        return;
    }
    showToast('Reset all functionality coming soon', 'info');
}

function resetSetting(settingId) {
    if (!confirm('Are you sure you want to reset this setting to its default value?')) {
        return;
    }
    
    $.ajax({
        url: '/Settings/ResetSetting',
        type: 'POST',
        data: JSON.stringify({ settingId: settingId }),
        contentType: 'application/json',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                showToast('Setting reset successfully', 'success');
                location.reload(); // Simple reload for now
            } else {
                showToast(response.message || 'Error resetting setting', 'error');
            }
        },
        error: function() {
            showToast('Error resetting setting', 'error');
        }
    });
}

// Theme Management Functions
let currentTheme = 'default';           // The saved/persisted theme
let previewedTheme = 'default';         // The currently previewed theme
let availableThemes = ['default', 'dark', 'blue', 'green', 'clean', 'purple', 'modern1', 'modern2'];

// Initialize theme selector
function initializeThemeSelector() {
    // Load current theme settings
    loadCurrentThemeSettings();
    
    // Bind theme option clicks - only preview, don't apply
    $('.theme-option').on('click', function() {
        const selectedTheme = $(this).data('theme');
        previewTheme(selectedTheme);
    });

    // Bind auto-detect toggle
    $('#themeAutoDetect').on('change', function() {
        const autoDetect = $(this).is(':checked');
        setThemeAutoDetect(autoDetect);
    });

    // Bind transition duration slider
    $('#themeTransitionDuration').on('input', function() {
        const duration = $(this).val();
        $('#transitionValue').text(duration + 'ms');
        updateTransitionDuration(duration);
    });

    // Bind apply theme button - actually save and apply
    $('#applyThemeBtn').on('click', function() {
        applyCurrentTheme();
    });

    // Bind reset theme button
    $('#resetThemeBtn').on('click', function() {
        resetThemeToDefault();
    });
}

// Load current theme settings
function loadCurrentThemeSettings() {
    $.ajax({
        url: '/Settings/GetCurrentTheme',
        type: 'GET',
        success: function(response) {
            if (response.success) {
                currentTheme = response.theme || 'default';
                previewedTheme = currentTheme; // Initialize preview to current theme
                selectThemeInUI(currentTheme);
                
                // Apply the current theme to the document
                document.documentElement.setAttribute('data-theme', currentTheme);
                
                // Load other settings
                if (response.autoDetect !== undefined) {
                    $('#themeAutoDetect').prop('checked', response.autoDetect);
                }
                
                if (response.transitionDuration !== undefined) {
                    $('#themeTransitionDuration').val(response.transitionDuration);
                    $('#transitionValue').text(response.transitionDuration + 'ms');
                }
            }
        },
        error: function() {
            console.warn('Failed to load current theme settings');
        }
    });
}

// Preview a theme (only visual, not saved)
function previewTheme(themeName) {
    // Set the global preview theme variable
    previewedTheme = themeName;
    
    // Update UI selection
    selectThemeInUI(themeName);
    loadThemeCSS(themeName);
    // Apply theme to document for preview
    document.documentElement.setAttribute('data-theme', themeName);
}

// Update UI selection without applying theme
function selectThemeInUI(themeName) {
    $('.theme-option').removeClass('active');
    $(`.theme-option[data-theme="${themeName}"]`).addClass('active');
}

// Select a theme (backward compatibility)
function selectTheme(themeName, apply = true) {
    if (apply) {
        currentTheme = themeName;
        previewedTheme = themeName;
        selectThemeInUI(themeName);
        applyCurrentTheme();
    } else {
        previewTheme(themeName);
    }
}
function loadThemeCSS(themeName) {
    // Remove existing theme stylesheets
    $('link[data-theme]').remove();
    
    if (themeName !== 'default') {
        // Add new theme stylesheet
        const link = $('<link>');
        link.attr({
            'rel': 'stylesheet',
            'type': 'text/css',
            'href': `css/themes/${themeName}.css`,
            'data-theme': themeName
        });
        $('head').append(link);
    }
}

// Apply current theme
function applyCurrentTheme() {
    // Set the preview theme as the current theme
    currentTheme = previewedTheme;
    
    // Apply theme to document
    document.documentElement.setAttribute('data-theme', currentTheme);
    
    // Save theme preference
    $.ajax({
        url: '/Settings/SetCurrentTheme',
        type: 'POST',
        data: JSON.stringify({ theme: currentTheme }),
        contentType: 'application/json',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                if (typeof showToast === 'function') {
                    showToast(`Theme applied and saved: ${currentTheme}`, 'success');
                } else {
                    console.log(`Theme applied and saved: ${currentTheme}`);
                }
                
                // Optionally refresh the page to ensure full theme application
                setTimeout(() => {
                    window.location.reload();
                }, 1000);
            }
        },
        error: function() {
            if (typeof showToast === 'function') {
                showToast('Failed to save theme preference', 'error');
            } else {
                console.error('Failed to save theme preference');
            }
        }
    });
}

// Set theme auto-detect
function setThemeAutoDetect(autoDetect) {
    $.ajax({
        url: '/Settings/SetThemeAutoDetect',
        type: 'POST',
        data: JSON.stringify({ autoDetect: autoDetect }),
        contentType: 'application/json',
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                if (autoDetect) {
                    detectAndApplySystemTheme();
                    showToast('Auto-detect enabled', 'info');
                } else {
                    showToast('Auto-detect disabled', 'info');
                }
            }
        },
        error: function() {
            showToast('Failed to update auto-detect setting', 'error');
            $('#themeAutoDetect').prop('checked', !autoDetect);
        }
    });
}

// Detect system theme preference
function detectAndApplySystemTheme() {
    if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
        selectTheme('dark');
        applyCurrentTheme();
    } else {
        selectTheme('default');
        applyCurrentTheme();
    }
}

// Update transition duration
function updateTransitionDuration(duration) {
    document.documentElement.style.setProperty('--theme-transition-duration', duration + 'ms');
}

// Reset theme to default
function resetThemeToDefault() {
    selectTheme('default');
    $('#themeAutoDetect').prop('checked', false);
    $('#themeTransitionDuration').val(200);
    $('#transitionValue').text('200ms');
    
    applyCurrentTheme();
    showToast('Theme reset to default', 'info');
}
