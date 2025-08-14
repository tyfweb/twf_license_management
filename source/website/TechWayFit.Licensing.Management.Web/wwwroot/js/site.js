// LicenseHub Site-wide JavaScript

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    console.log('Site.js loaded - Initializing components...');
    
    // Initialize Bootstrap components
    initializeBootstrapComponents();
    
    // Initialize dropdowns specifically
    initializeDropdowns();
    
    // Initialize tooltips
    initializeTooltips();
    
    // Initialize font accessibility controls
    initializeFontControls();
});

// Initialize Bootstrap dropdowns
function initializeDropdowns() {
    try {
        // Find all dropdown toggles
        const dropdownElementList = document.querySelectorAll('[data-bs-toggle="dropdown"]');
        console.log('Found dropdown elements:', dropdownElementList.length);
        
        // Initialize each dropdown
        const dropdownList = [...dropdownElementList].map(dropdownToggleEl => {
            const dropdown = new bootstrap.Dropdown(dropdownToggleEl);
            console.log('Initialized dropdown for:', dropdownToggleEl.id || 'unnamed element');
            return dropdown;
        });
        
        console.log('Successfully initialized', dropdownList.length, 'dropdowns');
        
        // Add click event listener as fallback
        dropdownElementList.forEach(element => {
            element.addEventListener('click', function(e) {
                console.log('Dropdown clicked:', this.id || 'unnamed');
                e.preventDefault();
                
                // Toggle dropdown manually if needed
                const dropdownMenu = this.nextElementSibling;
                if (dropdownMenu && dropdownMenu.classList.contains('dropdown-menu')) {
                    dropdownMenu.classList.toggle('show');
                }
            });
        });
        
    } catch (error) {
        console.error('Error initializing dropdowns:', error);
        
        // Fallback manual dropdown handling
        console.log('Using fallback dropdown implementation');
        const dropdowns = document.querySelectorAll('[data-bs-toggle="dropdown"]');
        dropdowns.forEach(dropdown => {
            dropdown.addEventListener('click', function(e) {
                e.preventDefault();
                const menu = this.nextElementSibling;
                if (menu) {
                    // Close other dropdowns
                    document.querySelectorAll('.dropdown-menu.show').forEach(otherMenu => {
                        if (otherMenu !== menu) {
                            otherMenu.classList.remove('show');
                        }
                    });
                    // Toggle current dropdown
                    menu.classList.toggle('show');
                }
            });
        });
        
        // Close dropdowns when clicking outside
        document.addEventListener('click', function(e) {
            if (!e.target.closest('.dropdown')) {
                document.querySelectorAll('.dropdown-menu.show').forEach(menu => {
                    menu.classList.remove('show');
                });
            }
        });
    }
}

// Initialize Bootstrap tooltips
function initializeTooltips() {
    try {
        const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
        const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));
        console.log('Initialized', tooltipList.length, 'tooltips');
    } catch (error) {
        console.error('Error initializing tooltips:', error);
    }
}

// Initialize other Bootstrap components
function initializeBootstrapComponents() {
    console.log('Checking Bootstrap availability...');
    
    if (typeof bootstrap === 'undefined') {
        console.error('Bootstrap JavaScript is not loaded!');
        return;
    }
    
    console.log('Bootstrap is available, version:', bootstrap.Modal ? 'Modal available' : 'Modal not available');
}

// Mobile sidebar toggle
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    if (sidebar) {
        sidebar.classList.toggle('show');
        console.log('Sidebar toggled');
    }
}

// Export functions for global use
window.toggleSidebar = toggleSidebar;

// Font Accessibility Controls
function initializeFontControls() {
    console.log('Initializing font accessibility controls...');
    
    // Load saved font size from localStorage
    const savedFontSize = localStorage.getItem('licensehub-font-size') || 'normal';
    applyFontSize(savedFontSize);
    updateActiveButton(savedFontSize);
    
    // Add click event listeners to font size buttons
    const fontButtons = document.querySelectorAll('.font-size-btn');
    fontButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            const size = this.getAttribute('data-size');
            console.log('Font size changed to:', size);
            
            // Update active state
            fontButtons.forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');
            
            // Apply font size
            applyFontSize(size);
            
            // Save to localStorage
            localStorage.setItem('licensehub-font-size', size);
        });
    });
}

function applyFontSize(size) {
    const root = document.documentElement;
    
    switch(size) {
        case 'small':
            root.style.setProperty('--bs-font-size-base', '0.85rem');
            root.style.setProperty('--font-size-multiplier', '0.85');
            break;
        case 'normal':
            root.style.setProperty('--bs-font-size-base', '1rem');
            root.style.setProperty('--font-size-multiplier', '1');
            break;
        case 'large':
            root.style.setProperty('--bs-font-size-base', '1.15rem');
            root.style.setProperty('--font-size-multiplier', '1.15');
            break;
        case 'extra-large':
            root.style.setProperty('--bs-font-size-base', '1.3rem');
            root.style.setProperty('--font-size-multiplier', '1.3');
            break;
        default:
            root.style.setProperty('--bs-font-size-base', '1rem');
            root.style.setProperty('--font-size-multiplier', '1');
    }
    
    // Apply to body for immediate effect
    document.body.style.fontSize = `var(--bs-font-size-base)`;
    
    console.log(`Font size applied: ${size}`);
}

function updateActiveButton(size) {
    const fontButtons = document.querySelectorAll('.font-size-btn');
    fontButtons.forEach(btn => {
        btn.classList.remove('active');
        if (btn.getAttribute('data-size') === size) {
            btn.classList.add('active');
        }
    });
}
