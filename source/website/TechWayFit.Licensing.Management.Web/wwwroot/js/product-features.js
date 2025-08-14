$(document).ready(function() {
    // Load features on page load
    loadProductFeatures();
    
    // Handle feature form submission
    $('#featureForm').on('submit', function(e) {
        e.preventDefault();
        saveFeature();
    });
    
    // Initialize view based on saved preference
    const savedView = localStorage.getItem('featureViewType') || 'card';
    switchView(savedView);
});

function loadProductFeatures() {
    $('#features-content').html('<div class="text-center py-5"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div><p class="mt-3 text-muted">Loading product features...</p></div>');
    
    const productId = $('#productId').val();
    const getFeaturesUrl = $('#getFeaturesUrl').val();
    
    $.get(getFeaturesUrl, { productId: productId })
        .done(function(response) {
            if (response.success && response.data) {
                const viewType = localStorage.getItem('featureViewType') || 'card';
                if (viewType === 'card') {
                    const html = renderProductFeaturesCard(response.data);
                    $('#features-content').html(html);
                } else {
                    const html = renderProductFeaturesList(response.data);
                    $('#features-content').html(html);
                }
                
                // Update counters
                updateFeatureStats(response.data);
            } else {
                $('#features-content').html('<div class="text-center py-5"><div class="text-muted"><i class="fas fa-star fa-3x mb-3"></i><p>No product features found.</p><p><small>Click "Add Feature" to create your first feature.</small></p></div></div>');
                updateFeatureStats([]);
            }
        })
        .fail(function(xhr, status, error) {
            console.error('Failed to load product features:', error);
            $('#features-content').html('<div class="alert alert-danger"><i class="fas fa-exclamation-triangle me-2"></i>Failed to load product features. Please try again.</div>');
            updateFeatureStats([]);
        });
}

function updateFeatureStats(features) {
    if (!features || features.length === 0) {
        $('#features-count').text(0);
        $('#active-features-count').text(0);
        $('#categories-count').text(0);
        $('#tiers-with-features-count').text(0);
        return;
    }
    
    // Total features
    $('#features-count').text(features.length);
    
    // Active features - using camelCase properties
    const activeFeatures = features.filter(f => f.isActive || f.isEnabled);
    $('#active-features-count').text(activeFeatures.length);
    
    // Unique categories
    const categories = [...new Set(features.map(f => f.category || 'Other'))];
    $('#categories-count').text(categories.length);
    
    // Tiers with features
    // This is a placeholder - in real implementation, you would calculate based on your data model
    $('#tiers-with-features-count').text(Math.min(3, features.length)); 
}

function renderProductFeaturesCard(features) {
    if (!features || features.length === 0) {
        return '<div class="text-center py-5"><div class="text-muted"><i class="fas fa-star fa-3x mb-3"></i><p>No product features found.</p><p><small>Click "Add Feature" to create your first feature.</small></p></div></div>';
    }

    // Group features by category
    const grouped = {};
    features.forEach(function(feature) {
        const category = feature.category || 'General';
        if (!grouped[category]) {
            grouped[category] = [];
        }
        grouped[category].push(feature);
    });

    let html = '<div class="row">';
    Object.keys(grouped).forEach(function(category) {
        html += '<div class="col-12 mb-4">';
        html += '  <div class="feature-category">';
        html += '    <h6 class="category-title">' + category + ' <span class="badge bg-light text-dark ms-2">' + grouped[category].length + '</span></h6>';
        html += '    <div class="row">';
        
        grouped[category].forEach(function(feature) {
            html += '<div class="col-lg-6 mb-3">';
            html += '  <div class="card feature-card h-100">';
            html += '    <div class="card-body">';
            html += '      <div class="d-flex justify-content-between align-items-start mb-2">';
            html += '        <div class="feature-icon me-3">';
            
            // Feature category icon
            html += getFeatureCategoryIcon(feature.category);
            
            html += '        </div>';
            html += '        <div class="flex-grow-1">';
            html += '          <h6 class="card-title mb-1">' + feature.name + '</h6>';
            if (feature.description) {
                html += '          <p class="card-text text-muted small mb-2">' + feature.description + '</p>';
            }
            html += '        </div>';
            html += '        <div class="dropdown">';
            html += '          <button class="btn btn-outline-light btn-sm dropdown-toggle" type="button" data-bs-toggle="dropdown">';
            html += '            <i class="fas fa-ellipsis-v"></i>';
            html += '          </button>';
            html += '          <ul class="dropdown-menu dropdown-menu-end">';
            html += '            <li><a class="dropdown-item" href="#" onclick="editFeature(\'' + feature.id + '\')"><i class="fas fa-edit me-2"></i>Edit</a></li>';
            if (feature.canDelete) {
                html += '            <li><a class="dropdown-item text-danger" href="#" onclick="deleteFeature(\'' + feature.id + '\')"><i class="fas fa-trash me-2"></i>Delete</a></li>';
            }
            html += '          </ul>';
            html += '        </div>';
            html += '      </div>';
            
            html += '      <div class="feature-badges">';
            html += '        <span class="badge bg-' + getFeatureCategoryBadgeColor(feature.category) + ' me-1">' + feature.category + '</span>';
            html += '        <span class="badge bg-secondary me-1">' + feature.minimumTier + '</span>';
            if (feature.isEnabled) {
                html += '        <span class="badge bg-success me-1">Enabled</span>';
            } 
            if (!feature.isActive) {
                html += '        <span class="badge bg-secondary me-1">Inactive</span>';
            }
            html += '      </div>';
            html += '    </div>';
            html += '  </div>';
            html += '</div>';
        });
        
        html += '    </div>';
        html += '  </div>';
        html += '</div>';
    });
    html += '</div>';
    return html;
}

function renderProductFeaturesList(features) {
    if (!features || features.length === 0) {
        return '<div class="text-center py-5"><div class="text-muted"><i class="fas fa-star fa-3x mb-3"></i><p>No product features found.</p><p><small>Click "Add Feature" to create your first feature.</small></p></div></div>';
    }
    
    let html = '<div class="table-responsive">';
    html += '<table class="table table-hover">';
    html += '<thead class="table-light">';
    html += '<tr>';
    html += '<th>Feature</th>';
    html += '<th>Category</th>';
    html += '<th>Min. Tier</th>';
    html += '<th>Status</th>';
    html += '<th>Actions</th>';
    html += '</tr>';
    html += '</thead>';
    html += '<tbody>';
    
    features.forEach(function(feature) {
        html += '<tr>';
        
        // Feature name and description
        html += '<td>';
        html += '<div class="d-flex align-items-center">';
        html += '<div class="feature-icon me-2" style="width: 30px; height: 30px;">';
        html += getFeatureCategoryIcon(feature.category);
        html += '</div>';
        html += '<div>';
        html += '<div class="fw-semibold">' + feature.name + '</div>';
        if (feature.description) {
            html += '<small class="text-muted">' + feature.description + '</small>';
        }
        html += '</div>';
        html += '</div>';
        html += '</td>';
        
        // Category
        html += '<td>';
        html += '<span class="badge bg-' + getFeatureCategoryBadgeColor(feature.category) + '">' + feature.category + '</span>';
        html += '</td>';
        
        // Min Tier
        html += '<td>';
        html += '<span class="badge bg-secondary">' + feature.minimumTier + '</span>';
        html += '</td>';
        
        // Status
        html += '<td>';
        if (feature.isEnabled && feature.isActive) {
            html += '<span class="badge bg-success">Enabled</span>';
        } else if (feature.isActive) {
            html += '<span class="badge bg-warning">Optional</span>';
        } else {
            html += '<span class="badge bg-secondary">Inactive</span>';
        }
        html += '</td>';
        
        // Actions
        html += '<td>';
        html += '<div class="btn-group btn-group-sm" role="group">';
        html += '<button type="button" class="btn btn-outline-primary" onclick="editFeature(\'' + feature.id + '\')"><i class="fas fa-edit"></i></button>';
        if (feature.canDelete) {
            html += '<button type="button" class="btn btn-outline-danger" onclick="deleteFeature(\'' + feature.id + '\')"><i class="fas fa-trash"></i></button>';
        }
        html += '</div>';
        html += '</td>';
        
        html += '</tr>';
    });
    
    html += '</tbody>';
    html += '</table>';
    html += '</div>';
    
    return html;
}

function getFeatureCategoryIcon(category) {
    switch(category) {
        case 'Core':
            return '<i class="fas fa-puzzle-piece text-primary"></i>';
        case 'Security':
            return '<i class="fas fa-shield-alt text-danger"></i>';
        case 'Performance':
            return '<i class="fas fa-tachometer-alt text-success"></i>';
        case 'BusinessIntelligence':
            return '<i class="fas fa-chart-line text-info"></i>';
        case 'Integration':
            return '<i class="fas fa-plug text-warning"></i>';
        default:
            return '<i class="fas fa-star text-secondary"></i>';
    }
}

function getFeatureCategoryBadgeColor(category) {
    switch(category) {
        case 'Core': return 'primary';
        case 'Security': return 'danger';
        case 'Performance': return 'success';
        case 'BusinessIntelligence': return 'info';
        case 'Integration': return 'warning';
        default: return 'secondary';
    }
}

function switchView(viewType) {
    const cardViewBtn = document.getElementById('cardViewBtn');
    const listViewBtn = document.getElementById('listViewBtn');

    if (viewType === 'card') {
        cardViewBtn.classList.add('active');
        listViewBtn.classList.remove('active');
        localStorage.setItem('featureViewType', 'card');
    } else {
        cardViewBtn.classList.remove('active');
        listViewBtn.classList.add('active');
        localStorage.setItem('featureViewType', 'list');
    }
    
    // Reload the features with the current view
    loadProductFeatures();
}

function showAddFeatureModal() {
    // Reset form and modal
    $('#featureForm')[0].reset();
    $('#featureId').val('');
    $('#featureModalLabel').html('<i class="fas fa-star me-2"></i>Add Product Feature');
    $('#featureSubmitBtn').html('<i class="fas fa-save me-2"></i>Add Feature');
    clearValidationErrors();
    
    // Load product tiers
    loadProductTiers();
    
    // Show modal
    const modal = new bootstrap.Modal(document.getElementById('featureModal'));
    modal.show();
}

function editFeature(featureId) {
    const productId = $('#productId').val();
    const getFeatureUrl = $('#getFeatureUrl').val();
    
    // Get feature data
    $.get(getFeatureUrl, { 
        productId: productId,
        featureId: featureId 
    })
    .done(function(response) {
        if (response.success && response.data) {
            const feature = response.data;            // Populate form with camelCase properties
            $('#featureId').val(feature.featureId);
            $('#featureName').val(feature.name);
            $('#featureDescription').val(feature.description);
            $('#category').val(feature.category);
            $('#minimumTier').val(feature.minimumTier);
            $('#maxUsage').val(feature.maxUsage);
            $('#isEnabled').prop('checked', feature.isEnabled);
            $('#isActive').prop('checked', feature.isActive);            // Load product tiers
            loadProductTiers(feature.minimumTier); // Pass current tier to select it
              // Update modal title and button
            $('#featureModalLabel').html('<i class="fas fa-edit me-2"></i>Edit Product Feature');
            $('#featureSubmitBtn').html('<i class="fas fa-save me-2"></i>Update Feature');
            clearValidationErrors();
            
            // Show modal
            const modal = new bootstrap.Modal(document.getElementById('featureModal'));
            modal.show();
        } else {
            showAlert('Error loading feature data', 'danger');
        }
    })
    .fail(function() {
        showAlert('Error loading feature data', 'danger');
    });
}

function deleteFeature(featureId) {
    if (confirm('Are you sure you want to delete this feature? This action cannot be undone.')) {
        const productId = $('#productId').val();
        const deleteFeatureUrl = $('#deleteFeatureUrl').val();
        
        const formData = new FormData();
        formData.append('productId', productId);
        formData.append('featureId', featureId);
        formData.append('__RequestVerificationToken', $('[name=__RequestVerificationToken]').val());

        $.ajax({
            url: deleteFeatureUrl,
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function(response) {
                if (response.success) {
                    showAlert(response.message, 'success');
                    loadProductFeatures(); // Reload the features list
                } else {
                    showAlert(response.message, 'danger');
                }
            },
            error: function() {
                showAlert('Error deleting feature', 'danger');
            }
        });
    }
}

function saveFeature() {
    const featureId = $('#featureId').val();
    const isEdit = featureId && featureId !== '';
    const addFeatureUrl = $('#addFeatureUrl').val();
    const editFeatureUrl = $('#editFeatureUrl').val();
    const url = isEdit ? editFeatureUrl : addFeatureUrl;
    
    const formData = new FormData(document.getElementById('featureForm'));
    if (isEdit) {
        formData.append('featureId', featureId);
    }
    formData.append('productId', $('#productId').val());
    
    // Disable submit button
    $('#featureSubmitBtn').prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i>Saving...');

    $.ajax({
        url: url,
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function(response) {
            if (response.success) {
                showAlert(response.message, 'success');
                $('#featureModal').modal('hide');
                loadProductFeatures(); // Reload the features list
            } else {
                if (response.errors && response.errors.length > 0) {
                    showValidationErrors(response.errors);
                } else {
                    showAlert(response.message, 'danger');
                }
            }
        },
        error: function() {
            showAlert('Error saving feature', 'danger');
        },
        complete: function() {
            // Re-enable submit button
            $('#featureSubmitBtn').prop('disabled', false).html(isEdit ? '<i class="fas fa-save me-2"></i>Update Feature' : '<i class="fas fa-save me-2"></i>Add Feature');
        }
    });
}

function clearValidationErrors() {
    $('.is-invalid').removeClass('is-invalid');
    $('.invalid-feedback').empty();
}

function showValidationErrors(errors) {
    clearValidationErrors();
    errors.forEach(function(error) {
        showAlert(error, 'danger');
    });
}

function showAlert(message, type) {
    const alertClass = 'alert-' + type;
    const icon = type === 'success' ? 'fa-check-circle' : 'fa-exclamation-triangle';
    const alertHtml = '<div class="alert ' + alertClass + ' alert-dismissible fade show" role="alert">' +
                    '<i class="fas ' + icon + ' me-2"></i>' +
                    message +
                    '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>' +
                    '</div>';
    
    // Remove existing alerts
    $('#alert-container .alert').remove();
    
    // Add new alert
    $('#alert-container').html(alertHtml);
    
    // Auto-hide success alerts after 5 seconds
    if (type === 'success') {
        setTimeout(function() {
            $('.alert-success').alert('close');
        }, 5000);
    }
}

function importFeaturesFromJson() {
    // Show import modal
    const modal = new bootstrap.Modal(document.getElementById('importModal'));
    modal.show();
    
    // Set up the import button handler
    $('#importBtn').off('click').on('click', function() {
        // Get the JSON data
        const jsonText = $('#importJson').val();
        let jsonData;
        
        try {
            jsonData = JSON.parse(jsonText);
            if (!Array.isArray(jsonData)) {
                throw new Error("JSON must be an array of features");
            }
            
            // Implement the import logic here
            // This is a placeholder - would need server-side support
            showAlert('Feature import functionality will be implemented in a future update.', 'info');
            $('#importModal').modal('hide');
            
        } catch (e) {
            showAlert('Invalid JSON format: ' + e.message, 'danger');
        }
    });
    
    // Handle file upload
    $('#importFile').off('change').on('change', function(e) {
        const file = e.target.files[0];
        if (!file) return;
        
        const reader = new FileReader();
        reader.onload = function(event) {
            $('#importJson').val(event.target.result);
        };
        reader.readAsText(file);
    });
}

function copyToAnotherProduct() {
    showAlert('Copy to Another Product functionality will be implemented in a future update.', 'info');
}

function exportToJson() {
    const productId = $('#productId').val();
    const getFeaturesUrl = $('#getFeaturesUrl').val();
    const productName = $('#productName').val();
    
    $.get(getFeaturesUrl, { productId: productId })
        .done(function(response) {
            if (response.success && response.data) {
                // Create a JSON string and download it
                const jsonData = JSON.stringify(response.data, null, 2);
                const blob = new Blob([jsonData], { type: 'application/json' });
                const url = window.URL.createObjectURL(blob);
                
                // Create temp link and click it
                const a = document.createElement('a');
                a.style.display = 'none';
                a.href = url;
                a.download = productName + '-features.json';
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                document.body.removeChild(a);
            } else {
                showAlert('No features to export', 'warning');
            }
        })        .fail(function() {
            showAlert('Failed to export features', 'danger');
        });
}

function loadProductTiers(selectedTier = null) {
    const productId = $('#productId').val();
    const getFeaturesUrl = $('#getFeaturesUrl').val();
    
    // Set loading state
    $('#minimumTier').html('<option value="">Loading tiers...</option>');
    
    // Get the base URL from getFeaturesUrl and change action to GetProductTiers
    const baseUrl = getFeaturesUrl.replace('GetProductFeatures', 'GetProductTiers');
    
    $.get(baseUrl, { productId: productId })
        .done(function(response) {
            if (response.success && response.data && response.data.length > 0) {
                let html = '<option value="">Select minimum tier</option>';
                
                response.data.forEach(function(tier) {
                    // Use camelCase property names as returned by the API
                    const tierName = tier.name || tier.Name; // Support both camelCase and PascalCase
                    const selected = selectedTier && selectedTier === tierName ? 'selected' : '';
                    html += `<option value="${tier.id}" ${selected}>${tierName}</option>`;
                });
                
                $('#minimumTier').html(html);
            } else {
                // Fallback to default tiers if no product tiers are available
                let html = '<option value="">Select minimum tier</option>';
                html += '<option value="Community">Community</option>';
                html += '<option value="Professional">Professional</option>';
                html += '<option value="Enterprise">Enterprise</option>';
                html += '<option value="Premium">Premium</option>';
                
                if (selectedTier) {
                    html = html.replace(`value="${selectedTier}"`, `value="${selectedTier}" selected`);
                }
                
                $('#minimumTier').html(html);
            }
        })
        .fail(function() {
            // Fallback to default tiers if API call fails
            let html = '<option value="">Select minimum tier</option>';
            html += '<option value="Community">Community</option>';
            html += '<option value="Professional">Professional</option>';
            html += '<option value="Enterprise">Enterprise</option>';
            html += '<option value="Premium">Premium</option>';
            
            if (selectedTier) {
                html = html.replace(`value="${selectedTier}"`, `value="${selectedTier}" selected`);
            }
            
            $('#minimumTier').html(html);
            console.warn('Failed to load product tiers, using default options');
        });
}
