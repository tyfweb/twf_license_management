/**
 * LicenseHub Layout JavaScript
 * Core functionality for the LicenseHub layout including sidebar, alerts, and Bootstrap components
 */

// Mobile sidebar toggle functionality
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    sidebar.classList.toggle('show');
}

// Close sidebar when clicking outside on mobile
function initializeSidebarClickOutside() {
    document.addEventListener('click', function(event) {
        const sidebar = document.getElementById('sidebar');
        const toggleBtn = event.target.closest('button');
        
        if (window.innerWidth <= 768 && 
            !sidebar.contains(event.target) && 
            !toggleBtn?.onclick?.toString().includes('toggleSidebar')) {
            sidebar.classList.remove('show');
        }
    });
}

// Auto-hide alerts after 5 seconds
function initializeAutoHideAlerts() {
    const alerts = document.querySelectorAll('.alert:not(.alert-persistent)');
    alerts.forEach(function(alert) {
        setTimeout(function() {
            if (alert && !alert.classList.contains('d-none')) {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            }
        }, 5000);
    });
}

// Initialize Bootstrap tooltips
function initializeTooltips() {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    const tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
    console.log('Initialized ' + tooltipList.length + ' tooltips');
    return tooltipList;
}

// Initialize Bootstrap popovers
function initializePopovers() {
    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    const popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
    console.log('Initialized ' + popoverList.length + ' popovers');
    return popoverList;
}

// Enhanced dropdown initialization
function initializeDropdowns() {
    const dropdownElementList = [].slice.call(document.querySelectorAll('[data-bs-toggle="dropdown"]'));
    console.log('Found ' + dropdownElementList.length + ' dropdown elements:', dropdownElementList);
    
    const dropdownList = dropdownElementList.map(function (dropdownToggleEl) {
        console.log('Initializing dropdown:', dropdownToggleEl);
        return new bootstrap.Dropdown(dropdownToggleEl);
    });
    console.log('Initialized ' + dropdownList.length + ' dropdowns');
    
    // Enhanced dropdown functionality with manual toggle
    dropdownElementList.forEach(function(dropdownEl, index) {
        console.log('Setting up enhanced dropdown ' + index + ':', dropdownEl);
        
        // Find the associated dropdown menu
        let dropdownMenu = dropdownEl.nextElementSibling;
        if (!dropdownMenu || !dropdownMenu.classList.contains('dropdown-menu')) {
            dropdownMenu = dropdownEl.parentElement.querySelector('.dropdown-menu');
        }
        
        if (dropdownMenu) {
            console.log('Found dropdown menu for element ' + index + ':', dropdownMenu);
            
            // Add click event with manual toggle
            dropdownEl.addEventListener('click', function(e) {
                e.preventDefault();
                e.stopPropagation();
                
                console.log('Manual dropdown toggle clicked:', e.target);
                
                // Close all other dropdowns first
                document.querySelectorAll('.dropdown-menu.show').forEach(function(otherMenu) {
                    if (otherMenu !== dropdownMenu) {
                        otherMenu.classList.remove('show');
                        otherMenu.parentElement.classList.remove('show');
                    }
                });
                
                // Toggle current dropdown
                const isShown = dropdownMenu.classList.contains('show');
                if (isShown) {
                    dropdownMenu.classList.remove('show');
                    dropdownEl.parentElement.classList.remove('show');
                    dropdownEl.setAttribute('aria-expanded', 'false');
                    console.log('Dropdown closed manually');
                } else {
                    dropdownMenu.classList.add('show');
                    dropdownEl.parentElement.classList.add('show');
                    dropdownEl.setAttribute('aria-expanded', 'true');
                    console.log('Dropdown opened manually');
                }
            });
            
            // Close dropdown when clicking outside
            document.addEventListener('click', function(e) {
                if (!dropdownEl.contains(e.target) && !dropdownMenu.contains(e.target)) {
                    dropdownMenu.classList.remove('show');
                    dropdownEl.parentElement.classList.remove('show');
                    dropdownEl.setAttribute('aria-expanded', 'false');
                }
            });
        } else {
            console.warn('No dropdown menu found for element ' + index);
        }
    });
    
    return dropdownList;
}

// Table row click handling
function initializeTableRowClicks() {
    const tableRows = document.querySelectorAll('.licensehub-table tbody tr[data-href]');
    tableRows.forEach(function(row) {
        row.style.cursor = 'pointer';
        row.addEventListener('click', function() {
            window.location.href = this.dataset.href;
        });
    });
}

// Form validation enhancement
function initializeFormValidation() {
    const forms = document.querySelectorAll('.needs-validation');
    forms.forEach(function(form) {
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });
}

// Search functionality
function initializeSearch() {
    const searchInputs = document.querySelectorAll('[data-search-target]');
    searchInputs.forEach(function(input) {
        input.addEventListener('keyup', function() {
            const searchTerm = this.value.toLowerCase();
            const targetSelector = this.dataset.searchTarget;
            const targets = document.querySelectorAll(targetSelector);
            
            targets.forEach(function(target) {
                const text = target.textContent.toLowerCase();
                const row = target.closest('tr');
                if (row) {
                    row.style.display = text.includes(searchTerm) ? '' : 'none';
                }
            });
        });
    });
}

// Initialize all Bootstrap components
function initializeBootstrapComponents() {
    console.log('DOM loaded, initializing Bootstrap components...');
    
    try {
        // Initialize all components
        initializeTooltips();
        initializePopovers();
        initializeDropdowns();
        
        console.log('Bootstrap components initialization completed');
    } catch (error) {
        console.error('Error initializing Bootstrap components:', error);
    }
}

// Main initialization function
function initializeLicenseHubLayout() {
    console.log('Layout DOM Content Loaded - initializing functionality...');
    
    try {
        // Initialize all layout functionality
        initializeSidebarClickOutside();
        initializeAutoHideAlerts();
        initializeBootstrapComponents();
        initializeTableRowClicks();
        initializeFormValidation();
        initializeSearch();
        
        console.log('LicenseHub layout initialization completed successfully');
    } catch (error) {
        console.error('Error during LicenseHub layout initialization:', error);
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', initializeLicenseHubLayout);

// Export functions for global access (if needed)
window.LicenseHubLayout = {
    toggleSidebar: toggleSidebar,
    initializeBootstrapComponents: initializeBootstrapComponents,
    initializeSearch: initializeSearch,
    initializeFormValidation: initializeFormValidation
};
