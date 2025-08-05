/**
 * Product Version Management JavaScript
 * Handles all version-related operations including CRUD, view switching, and import/export
 */

class ProductVersionManager {
    constructor(options) {
        this.productId = options.productId;
        this.urls = options.urls;
        this.currentView = localStorage.getItem('versionViewType') || 'timeline';
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadProductVersions();
        this.initializeDefaults();
        this.switchView(this.currentView);
    }

    bindEvents() {
        // Form submission
        $('#versionForm').on('submit', (e) => {
            e.preventDefault();
            this.saveVersion();
        });

        // Stable checkbox handling
        $('#isStable').on('change', function() {
            if ($(this).is(':checked')) {
                $('#isApproved').prop('disabled', false);
            } else {
                $('#isApproved').prop('checked', false).prop('disabled', true);
            }
        });

        // Import button
        $('#importBtn').on('click', () => this.handleImport());

        // File upload for import
        $('#importFile').on('change', (e) => this.handleFileUpload(e));
    }

    initializeDefaults() {
        // Set default release date to today
        $('#releaseDate').val(new Date().toISOString().split('T')[0]);
    }

    loadProductVersions() {
        const loadingHtml = `
            <div class="text-center py-5">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <p class="mt-3 text-muted">Loading product versions...</p>
            </div>`;
        
        $('#versions-content').html(loadingHtml);
        
        $.get(this.urls.getVersions, { productId: this.productId })
            .done((response) => {
                if (response.success && response.data) {
                    const html = this.renderVersions(response.data);
                    $('#versions-content').html(html);
                    this.updateVersionStats(response.data);
                } else {
                    this.showEmptyState();
                    this.resetStats();
                }
            })
            .fail((xhr, status, error) => {
                console.error('Failed to load product versions:', error);
                this.showErrorState();
                this.resetStats();
            });
    }

    renderVersions(versions) {
        switch(this.currentView) {
            case 'timeline':
                return this.renderTimeline(versions);
            case 'card':
                return this.renderCards(versions);
            case 'list':
                return this.renderList(versions);
            default:
                return this.renderTimeline(versions);
        }
    }

    renderTimeline(versions) {
        if (!versions || versions.length === 0) {
            return this.getEmptyStateHtml();
        }

        const sortedVersions = this.sortVersionsByDate(versions);
        let html = '<div class="timeline">';
        
        sortedVersions.forEach((version) => {
            const markerClass = this.getTimelineMarkerClass(version);
            const markerIcon = this.getTimelineMarkerIcon(version);
            
            html += `
                <div class="timeline-item">
                    <div class="timeline-marker">
                        <div class="${markerClass}">
                            <i class="${markerIcon}"></i>
                        </div>
                    </div>
                    <div class="timeline-content">
                        <div class="card version-card">
                            <div class="card-body">
                                ${this.renderVersionHeader(version)}
                                ${this.renderVersionBadges(version)}
                                ${this.renderVersionNotes(version)}
                            </div>
                        </div>
                    </div>
                </div>`;
        });
        
        html += '</div>';
        return html;
    }

    renderCards(versions) {
        if (!versions || versions.length === 0) {
            return this.getEmptyStateHtml();
        }

        const sortedVersions = this.sortVersionsByDate(versions);
        let html = '<div class="version-card-grid">';
        
        sortedVersions.forEach((version) => {
            html += `
                <div class="card version-card h-100">
                    <div class="card-body">
                        <div class="d-flex justify-content-between align-items-start mb-3">
                            <div class="d-flex align-items-start">
                                <div class="version-icon me-3">
                                    ${this.getVersionIcon(version)}
                                </div>
                                <div>
                                    <h6 class="mb-0">Version ${version.version}</h6>
                                    ${version.versionName ? `<div class="text-primary small">${version.versionName}</div>` : ''}
                                </div>
                            </div>
                            ${this.renderActionsDropdown(version)}
                        </div>
                        ${this.renderVersionBadges(version)}
                        ${this.renderVersionDate(version)}
                        ${this.renderVersionNotesPreview(version)}
                    </div>
                </div>`;
        });
        
        html += '</div>';
        return html;
    }

    renderList(versions) {
        if (!versions || versions.length === 0) {
            return this.getEmptyStateHtml();
        }

        const sortedVersions = this.sortVersionsByDate(versions);
        let html = `
            <div class="table-responsive">
                <table class="table table-hover">
                    <thead class="table-light">
                        <tr>
                            <th>Version</th>
                            <th>Name</th>
                            <th>Release Date</th>
                            <th>Status</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>`;
        
        sortedVersions.forEach((version) => {
            html += `
                <tr>
                    <td>
                        <div class="d-flex align-items-center">
                            <div class="version-icon me-2" style="width: 30px; height: 30px;">
                                ${this.getVersionIcon(version)}
                            </div>
                            <div class="fw-semibold">${version.version}</div>
                        </div>
                    </td>
                    <td>${version.versionName || '-'}</td>
                    <td>${version.releaseDate ? new Date(version.releaseDate).toLocaleDateString() : '-'}</td>
                    <td>${this.renderVersionBadges(version)}</td>
                    <td>${this.renderActionButtons(version)}</td>
                </tr>`;
        });
        
        html += '</tbody></table></div>';
        return html;
    }

    renderVersionHeader(version) {
        return `
            <div class="d-flex justify-content-between align-items-start mb-2">
                <div>
                    <div class="version-label mb-1">Version ${version.version}</div>
                    ${version.versionName ? `<div class="text-primary fw-medium">${version.versionName}</div>` : ''}
                    ${this.renderVersionDate(version)}
                </div>
                ${this.renderActionsDropdown(version)}
            </div>`;
    }

    renderVersionBadges(version) {
        let badges = '';
        
        if (version.IsStable) {
            badges += '<span class="badge bg-success me-1">Stable</span>';
        } else {
            badges += '<span class="badge bg-warning me-1">Beta</span>';
        }
        
        if (version.IsApproved) {
            badges += '<span class="badge bg-info me-1">Approved</span>';
        }
        
        if (version.IsActive) {
            badges += '<span class="badge bg-primary me-1">Active</span>';
        } else {
            badges += '<span class="badge bg-secondary me-1">Inactive</span>';
        }
        
        return `<div class="version-badges mb-2">${badges}</div>`;
    }

    renderVersionDate(version) {
        if (!version.releaseDate) return '';
        return `<div class="release-date mt-1">
            <i class="fas fa-calendar-day me-1"></i>
            ${new Date(version.releaseDate).toLocaleDateString()}
        </div>`;
    }

    renderVersionNotes(version) {
        if (!version.ReleaseNotes && !version.Description) return '';
        const notes = version.ReleaseNotes || version.Description;
        return `
            <div class="border-top pt-2 mt-2">
                <div class="small text-muted mb-1">
                    <i class="fas fa-clipboard-list me-1"></i>Release Notes:
                </div>
                <p class="mb-0">${notes}</p>
            </div>`;
    }

    renderVersionNotesPreview(version) {
        if (!version.ReleaseNotes && !version.Description) return '';
        const notes = version.ReleaseNotes || version.Description;
        return `
            <div class="small">
                <div class="text-truncate-3">${notes}</div>
            </div>`;
    }

    renderActionsDropdown(version) {
        return `
            <div class="dropdown">
                <button class="btn btn-outline-light btn-sm dropdown-toggle" type="button" data-bs-toggle="dropdown">
                    <i class="fas fa-ellipsis-v"></i>
                </button>
                <ul class="dropdown-menu dropdown-menu-end">
                    <li><a class="dropdown-item" href="#" onclick="versionManager.editVersion('${version.id}')">
                        <i class="fas fa-edit me-2"></i>Edit
                    </a></li>
                    <li><a class="dropdown-item" href="#" onclick="versionManager.markCurrentVersion('${version.id}')">
                        <i class="fas fa-check-circle me-2"></i>Set as Current
                    </a></li>
                    ${!version.IsApproved ? `
                        <li><a class="dropdown-item" href="#" onclick="versionManager.approveVersion('${version.id}')">
                            <i class="fas fa-stamp me-2"></i>Approve
                        </a></li>` : ''}
                    ${version.CanDelete ? `
                        <li><hr class="dropdown-divider"></li>
                        <li><a class="dropdown-item text-danger" href="#" onclick="versionManager.deleteVersion('${version.id}')">
                            <i class="fas fa-trash me-2"></i>Delete
                        </a></li>` : ''}
                </ul>
            </div>`;
    }

    renderActionButtons(version) {
        let buttons = `
            <div class="btn-group btn-group-sm" role="group">
                <button type="button" class="btn btn-outline-primary" onclick="versionManager.editVersion('${version.id}')">
                    <i class="fas fa-edit"></i>
                </button>`;
        
        if (!version.IsApproved) {
            buttons += `
                <button type="button" class="btn btn-outline-info" onclick="versionManager.approveVersion('${version.id}')">
                    <i class="fas fa-stamp"></i>
                </button>`;
        }
        
        buttons += `
            <button type="button" class="btn btn-outline-success" onclick="versionManager.markCurrentVersion('${version.id}')">
                <i class="fas fa-check-circle"></i>
            </button>`;
        
        if (version.CanDelete) {
            buttons += `
                <button type="button" class="btn btn-outline-danger" onclick="versionManager.deleteVersion('${version.id}')">
                    <i class="fas fa-trash"></i>
                </button>`;
        }
        
        buttons += '</div>';
        return buttons;
    }

    getTimelineMarkerClass(version) {
        let markerClass = 'timeline-marker-icon';
        if (version.IsApproved && version.IsStable) {
            markerClass += ' approved';
        } else if (version.IsStable) {
            markerClass += ' stable';
        } else {
            markerClass += ' beta';
        }
        return markerClass;
    }

    getTimelineMarkerIcon(version) {
        if (version.IsApproved && version.IsStable) {
            return 'fas fa-check';
        } else if (version.IsStable) {
            return 'fas fa-code-branch';
        } else {
            return 'fas fa-flask';
        }
    }

    getVersionIcon(version) {
        if (version.IsApproved && version.IsStable) {
            return '<i class="fas fa-check text-success"></i>';
        } else if (version.IsStable) {
            return '<i class="fas fa-code-branch text-info"></i>';
        } else {
            return '<i class="fas fa-flask text-warning"></i>';
        }
    }

    sortVersionsByDate(versions) {
        return versions.sort((a, b) => {
            const dateA = a.ReleaseDate ? new Date(a.ReleaseDate) : new Date(0);
            const dateB = b.ReleaseDate ? new Date(b.ReleaseDate) : new Date(0);
            return dateB - dateA;
        });
    }

    getEmptyStateHtml() {
        return `
            <div class="text-center py-5">
                <div class="text-muted">
                    <i class="fas fa-code-branch fa-3x mb-3"></i>
                    <p>No product versions found.</p>
                    <p><small>Click "Add Version" to create your first version.</small></p>
                </div>
            </div>`;
    }

    showEmptyState() {
        $('#versions-content').html(this.getEmptyStateHtml());
    }

    showErrorState() {
        $('#versions-content').html(`
            <div class="alert alert-danger">
                <i class="fas fa-exclamation-triangle me-2"></i>
                Failed to load product versions. Please try again.
            </div>`);
    }

    resetStats() {
        $('#versions-count').text(0);
        $('#stable-versions-count').text(0);
        $('#beta-versions-count').text(0);
        $('#last-release-days').text(0);
    }

    updateVersionStats(versions) {
        if (!versions || versions.length === 0) {
            this.resetStats();
            return;
        }

        $('#versions-count').text(versions.length);
        
        const stableVersions = versions.filter(v => v.IsStable);
        $('#stable-versions-count').text(stableVersions.length);
        
        const betaVersions = versions.filter(v => !v.IsStable);
        $('#beta-versions-count').text(betaVersions.length);
        
        // Calculate days since last release
        let lastReleaseDate = null;
        versions.forEach((version) => {
            if (version.releaseDate) {
                const releaseDate = new Date(version.releaseDate);
                if (!lastReleaseDate || releaseDate > lastReleaseDate) {
                    lastReleaseDate = releaseDate;
                }
            }
        });
        
        if (lastReleaseDate) {
            const today = new Date();
            const diffTime = Math.abs(today - lastReleaseDate);
            const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
            $('#last-release-days').text(diffDays);
        } else {
            $('#last-release-days').text('N/A');
        }
    }

    switchView(viewType) {
        // Update button states
        ['timeline', 'card', 'list'].forEach(type => {
            const btn = document.getElementById(`${type}ViewBtn`);
            if (btn) {
                btn.classList.toggle('active', type === viewType);
            }
        });
        
        this.currentView = viewType;
        localStorage.setItem('versionViewType', viewType);
        this.loadProductVersions();
    }

    showAddVersionModal() {
        $('#versionForm')[0].reset();
        $('#versionId').val('');
        $('#versionModalLabel').text('Add Product Version');
        $('#versionSubmitBtn').text('Add Version');
        this.clearValidationErrors();
        
        $('#releaseDate').val(new Date().toISOString().split('T')[0]);
        
        const modal = new bootstrap.Modal(document.getElementById('versionModal'));
        modal.show();
    }

    editVersion(versionId) {
        $.get(this.urls.getVersion, { 
            productId: this.productId, 
            versionId: versionId 
        })
        .done((response) => {
            if (response.success && response.data) {
                this.populateVersionForm(response.data);
                this.showVersionModal('Edit Product Version', 'Update Version');
            } else {
                this.showAlert('Error loading version data', 'danger');
            }
        })
        .fail(() => {
            this.showAlert('Error loading version data', 'danger');
        });
    }

    populateVersionForm(version) {
        $('#versionId').val(version.VersionId);
        $('#versionNumber').val(version.version);
        $('#versionName').val(version.versionName);
        $('#releaseNotes').val(version.Description || version.ReleaseNotes);
        
        if (version.releaseDate) {
            $('#releaseDate').val(new Date(version.releaseDate).toISOString().split('T')[0]);
        }
        
        if (version.endOfLifeDate) {
            $('#endOfLifeDate').val(new Date(version.endOfLifeDate).toISOString().split('T')[0]);
        }
        
        $('#isStable').prop('checked', version.IsStable);
        $('#isActive').prop('checked', version.IsActive);
        $('#isApproved').prop('checked', version.IsApproved);
        
        if (!version.IsStable) {
            $('#isApproved').prop('checked', false).prop('disabled', true);
        } else {
            $('#isApproved').prop('disabled', false);
        }
    }

    showVersionModal(title, buttonText) {
        $('#versionModalLabel').text(title);
        $('#versionSubmitBtn').text(buttonText);
        this.clearValidationErrors();
        
        const modal = new bootstrap.Modal(document.getElementById('versionModal'));
        modal.show();
    }

    deleteVersion(versionId) {
        if (!confirm('Are you sure you want to delete this version? This action cannot be undone.')) {
            return;
        }

        this.makeAjaxRequest(this.urls.deleteVersion, {
            productId: this.productId,
            versionId: versionId
        }, 'POST')
        .then(() => {
            this.showAlert('Version deleted successfully', 'success');
            this.loadProductVersions();
        })
        .catch(() => {
            this.showAlert('Error deleting version', 'danger');
        });
    }

    approveVersion(versionId) {
        if (!confirm('Are you sure you want to approve this version? Approved versions cannot be deleted.')) {
            return;
        }

        this.makeAjaxRequest(this.urls.approveVersion, {
            productId: this.productId,
            versionId: versionId
        }, 'POST')
        .then((response) => {
            this.showAlert(response.message || 'Version approved successfully', 'success');
            this.loadProductVersions();
        })
        .catch(() => {
            this.showAlert('Error approving version', 'danger');
        });
    }

    markCurrentVersion(versionId) {
        if (!confirm('Set this as the current version?')) {
            return;
        }

        this.makeAjaxRequest(this.urls.setCurrentVersion, {
            productId: this.productId,
            versionId: versionId
        }, 'POST')
        .then((response) => {
            this.showAlert(response.message || 'Current version set successfully', 'success');
            this.loadProductVersions();
        })
        .catch(() => {
            this.showAlert('Error setting current version', 'danger');
        });
    }

    markVersionAsCurrent() {
        $.get(this.urls.getVersions, { productId: this.productId })
            .done((response) => {
                if (response.success && response.data && response.data.length > 0) {
                    // Create a dropdown dialog for selecting the current version
                    let html = '<div class="modal fade" id="selectCurrentVersionModal" tabindex="-1" aria-hidden="true">';
                    html += '<div class="modal-dialog modal-sm">';
                    html += '<div class="modal-content">';
                    html += '<div class="modal-header">';
                    html += '<h5 class="modal-title">Set Current Version</h5>';
                    html += '<button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>';
                    html += '</div>';
                    html += '<div class="modal-body">';
                    html += '<div class="mb-3">';
                    html += '<label for="currentVersionSelect" class="form-label">Select Version</label>';
                    html += '<select class="form-select" id="currentVersionSelect">';
                    
                    // Sort versions by version number for the dropdown
                    const sortedVersions = [...response.data].sort((a, b) => {
                        // Convert version strings to comparable arrays (e.g., "2.1.0" -> [2, 1, 0])
                        const verA = a.VersionNumber ? a.VersionNumber.split('.').map(Number) : a.version.split('.').map(Number);
                        const verB = b.VersionNumber ? b.VersionNumber.split('.').map(Number) : b.version.split('.').map(Number);
                        
                        // Compare each part of the version
                        for (let i = 0; i < Math.max(verA.length, verB.length); i++) {
                            const partA = verA[i] || 0;
                            const partB = verB[i] || 0;
                            if (partA !== partB) return partB - partA; // Newest first
                        }
                        return 0;
                    });
                    
                    sortedVersions.forEach(version => {
                        const versionNumber = version.VersionNumber || version.version;
                        const versionName = version.VersionName || version.versionName;
                        html += `<option value="${version.id}">${versionNumber}${versionName ? ' - ' + versionName : ''}</option>`;
                    });
                    
                    html += '</select>';
                    html += '</div>';
                    html += '</div>';
                    html += '<div class="modal-footer">';
                    html += '<button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>';
                    html += '<button type="button" class="btn btn-primary" id="setCurrentVersionBtn">Set as Current</button>';
                    html += '</div>';
                    html += '</div>';
                    html += '</div>';
                    html += '</div>';
                    
                    // Remove any existing modal
                    $('#selectCurrentVersionModal').remove();
                    
                    // Append and show the modal
                    $('body').append(html);
                    const modal = new bootstrap.Modal(document.getElementById('selectCurrentVersionModal'));
                    modal.show();
                    
                    // Handle the button click
                    $('#setCurrentVersionBtn').on('click', () => {
                        const selectedVersionId = $('#currentVersionSelect').val();
                        this.markCurrentVersion(selectedVersionId);
                        modal.hide();
                    });
                } else {
                    this.showAlert('No versions available to set as current', 'warning');
                }
            })
            .fail(() => {
                this.showAlert('Error loading versions', 'danger');
            });
    }

    saveVersion() {
        const versionId = $('#versionId').val();
        const isEdit = versionId && versionId !== '';
        const url = isEdit ? this.urls.editVersion : this.urls.addVersion;
        
        const formData = new FormData(document.getElementById('versionForm'));
        formData.append('productId', this.productId);
        
        if (isEdit) {
            formData.append('versionId', versionId);
        }
        
        if (formData.has('ReleaseNotes')) {
            formData.append('Description', formData.get('ReleaseNotes'));
        }

        $('#versionSubmitBtn').prop('disabled', true).text('Saving...');

        $.ajax({
            url: url,
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false
        })
        .done((response) => {
            if (response.success) {
                this.showAlert(response.message, 'success');
                $('#versionModal').modal('hide');
                this.loadProductVersions();
            } else {
                if (response.errors && response.errors.length > 0) {
                    this.showValidationErrors(response.errors);
                } else {
                    this.showAlert(response.message, 'danger');
                }
            }
        })
        .fail(() => {
            this.showAlert('Error saving version', 'danger');
        })
        .always(() => {
            $('#versionSubmitBtn').prop('disabled', false)
                .text(isEdit ? 'Update Version' : 'Add Version');
        });
    }

    importVersionsFromJson() {
        const modal = new bootstrap.Modal(document.getElementById('importModal'));
        modal.show();
    }

    handleImport() {
        const jsonText = $('#importJson').val();
        if (!jsonText.trim()) {
            this.showAlert('Please enter JSON data to import', 'warning');
            return;
        }
        
        try {
            const jsonData = JSON.parse(jsonText);
            if (!Array.isArray(jsonData)) {
                throw new Error("JSON must be an array of versions");
            }
            
            $('#importBtn').prop('disabled', true).text('Importing...');
            
            this.makeAjaxRequest(this.urls.importVersions, {
                productId: this.productId,
                versionsJson: jsonText
            }, 'POST')
            .then((response) => {
                this.showAlert(response.message || 'Versions imported successfully', 'success');
                $('#importModal').modal('hide');
                this.loadProductVersions();
            })
            .catch(() => {
                this.showAlert('Server error while importing versions', 'danger');
            })
            .finally(() => {
                $('#importBtn').prop('disabled', false).text('Import Versions');
            });
        } catch (e) {
            this.showAlert('Invalid JSON format: ' + e.message, 'danger');
        }
    }

    handleFileUpload(event) {
        const file = event.target.files[0];
        if (!file) return;
        
        const reader = new FileReader();
        reader.onload = (e) => {
            $('#importJson').val(e.target.result);
        };
        reader.readAsText(file);
    }

    exportToJson() {
        $.get(this.urls.getVersions, { productId: this.productId })
            .done((response) => {
                if (response.success && response.data) {
                    const jsonData = JSON.stringify(response.data, null, 2);
                    const blob = new Blob([jsonData], { type: 'application/json' });
                    const url = window.URL.createObjectURL(blob);
                    
                    const a = document.createElement('a');
                    a.style.display = 'none';
                    a.href = url;
                    a.download = `${this.productName || 'product'}-versions.json`;
                    document.body.appendChild(a);
                    a.click();
                    window.URL.revokeObjectURL(url);
                    document.body.removeChild(a);
                } else {
                    this.showAlert('No versions to export', 'warning');
                }
            })
            .fail(() => {
                this.showAlert('Failed to export versions', 'danger');
            });
    }

    makeAjaxRequest(url, data, method = 'GET') {
        const formData = new FormData();
        Object.keys(data).forEach(key => {
            formData.append(key, data[key]);
        });
        formData.append('__RequestVerificationToken', $('[name=__RequestVerificationToken]').val());

        return new Promise((resolve, reject) => {
            $.ajax({
                url: url,
                type: method,
                data: formData,
                processData: false,
                contentType: false
            })
            .done((response) => {
                if (response.success) {
                    resolve(response);
                } else {
                    reject(new Error(response.message || 'Request failed'));
                }
            })
            .fail(() => {
                reject(new Error('Network error'));
            });
        });
    }

    clearValidationErrors() {
        $('.is-invalid').removeClass('is-invalid');
        $('.invalid-feedback').empty();
    }

    showValidationErrors(errors) {
        this.clearValidationErrors();
        errors.forEach((error) => {
            this.showAlert(error, 'danger');
        });
    }

    showAlert(message, type) {
        const alertClass = 'alert-' + type;
        const icon = type === 'success' ? 'fa-check-circle' : 'fa-exclamation-triangle';
        const alertHtml = `
            <div class="alert ${alertClass} alert-dismissible fade show" role="alert">
                <i class="fas ${icon} me-2"></i>
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>`;
        
        $('#alert-container .alert').remove();
        $('#alert-container').html(alertHtml);
        
        if (type === 'success') {
            setTimeout(() => {
                $('.alert-success').alert('close');
            }, 5000);
        }
    }
}

// Global functions for backward compatibility and event handlers
function switchView(viewType) {
    console.log('switchView called with:', viewType);
    if (window.versionManager) {
        window.versionManager.switchView(viewType);
    } else {
        console.error('versionManager not found on window object');
    }
}

function showAddVersionModal() {
    console.log('showAddVersionModal called');
    if (window.versionManager) {
        window.versionManager.showAddVersionModal();
    } else {
        console.error('versionManager not found on window object');
    }
}

function importVersionsFromJson() {
    console.log('importVersionsFromJson called');
    if (window.versionManager) {
        window.versionManager.importVersionsFromJson();
    } else {
        console.error('versionManager not found on window object');
    }
}

function exportToJson() {
    console.log('exportToJson called');
    if (window.versionManager) {
        window.versionManager.exportToJson();
    } else {
        console.error('versionManager not found on window object');
    }
}

function editVersion(versionId) {
    console.log('editVersion called with:', versionId);
    if (window.versionManager) {
        window.versionManager.editVersion(versionId);
    } else {
        console.error('versionManager not found on window object');
    }
}

function deleteVersion(versionId) {
    console.log('deleteVersion called with:', versionId);
    if (window.versionManager) {
        window.versionManager.deleteVersion(versionId);
    } else {
        console.error('versionManager not found on window object');
    }
}

function approveVersion(versionId) {
    console.log('approveVersion called with:', versionId);
    if (window.versionManager) {
        window.versionManager.approveVersion(versionId);
    } else {
        console.error('versionManager not found on window object');
    }
}

function markCurrentVersion(versionId) {
    console.log('markCurrentVersion called with:', versionId);
    if (window.versionManager) {
        window.versionManager.markCurrentVersion(versionId);
    } else {
        console.error('versionManager not found on window object');
    }
}

function markVersionAsCurrent() {
    console.log('markVersionAsCurrent called');
    if (window.versionManager) {
        window.versionManager.markVersionAsCurrent();
    } else {
        console.error('versionManager not found on window object');
    }
}
