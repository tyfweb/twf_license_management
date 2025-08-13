/**
 * TechWayFit Hangfire Dashboard Customization
 * This script customizes the Hangfire dashboard to match the application theme
 */

(function() {
    'use strict';

    // Wait for the page to load
    document.addEventListener('DOMContentLoaded', function() {
        customizeHangfireDashboard();
    });

    function customizeHangfireDashboard() {
        // Check if we're in the Hangfire dashboard
        if (!window.location.pathname.includes('/hangfire')) {
            return;
        }

        // Add custom CSS
        injectCustomCSS();
        
        // Add custom header
        addCustomHeader();
        
        // Customize navigation
        customizeNavigation();
        
        // Add refresh functionality
        addRefreshButton();
        
        // Monitor for dynamic content changes
        observeContentChanges();
    }

    function injectCustomCSS() {
        const link = document.createElement('link');
        link.rel = 'stylesheet';
        link.type = 'text/css';
        link.href = '/css/hangfire-custom.css';
        document.head.appendChild(link);
    }

    function addCustomHeader() {
        // Create custom header
        const header = document.createElement('div');
        header.id = 'techway-header';
        header.innerHTML = `
            <div style="background: linear-gradient(135deg, #3498db, #2c3e50); color: white; padding: 10px 0; text-align: center; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                <div style="max-width: 1200px; margin: 0 auto; display: flex; justify-content: space-between; align-items: center; padding: 0 20px;">
                    <div style="display: flex; align-items: center;">
                        <span style="font-size: 1.5rem; margin-right: 10px;">🔧</span>
                        <div>
                            <h4 style="margin: 0; font-weight: 600;">TechWayFit Licensing Management</h4>
                            <small style="opacity: 0.8;">Hangfire Job Dashboard</small>
                        </div>
                    </div>
                    <div style="display: flex; gap: 10px;">
                        <button onclick="window.open('/System', '_blank')" style="background: rgba(255,255,255,0.2); border: 1px solid rgba(255,255,255,0.3); color: white; padding: 5px 15px; border-radius: 4px; cursor: pointer; text-decoration: none; display: inline-flex; align-items: center; gap: 5px;">
                            <span>←</span> Back to System
                        </button>
                        <button onclick="location.reload()" style="background: rgba(255,255,255,0.2); border: 1px solid rgba(255,255,255,0.3); color: white; padding: 5px 15px; border-radius: 4px; cursor: pointer;">
                            ↻ Refresh
                        </button>
                    </div>
                </div>
            </div>
        `;
        
        // Insert at the beginning of the body
        document.body.insertBefore(header, document.body.firstChild);
    }

    function customizeNavigation() {
        // Find and enhance the navbar
        const navbar = document.querySelector('.navbar');
        if (navbar) {
            // Update brand text
            const brand = navbar.querySelector('.navbar-brand');
            if (brand) {
                brand.innerHTML = '<span style="margin-right: 8px;">🔧</span>TechWayFit Jobs';
            }

            // Add custom navigation items
            const navList = navbar.querySelector('.navbar-nav');
            if (navList) {
                const customNavItem = document.createElement('li');
                customNavItem.innerHTML = `
                    <a href="/System" target="_blank" style="color: #ecf0f1; padding: 15px;">
                        <i class="fa fa-external-link"></i> System Dashboard
                    </a>
                `;
                navList.appendChild(customNavItem);
            }
        }
    }

    function addRefreshButton() {
        // Add a floating refresh button
        const refreshBtn = document.createElement('div');
        refreshBtn.id = 'floating-refresh';
        refreshBtn.innerHTML = `
            <button onclick="location.reload()" title="Refresh Dashboard" style="
                position: fixed;
                bottom: 20px;
                right: 20px;
                width: 50px;
                height: 50px;
                border-radius: 50%;
                background: #3498db;
                border: none;
                color: white;
                font-size: 18px;
                cursor: pointer;
                box-shadow: 0 4px 8px rgba(0,0,0,0.2);
                z-index: 1000;
                transition: all 0.3s ease;
            " onmouseover="this.style.background='#2980b9'; this.style.transform='scale(1.1)'" onmouseout="this.style.background='#3498db'; this.style.transform='scale(1)'">
                ↻
            </button>
        `;
        document.body.appendChild(refreshBtn);
    }

    function observeContentChanges() {
        // Monitor for dynamic content changes and re-apply customizations
        const observer = new MutationObserver(function(mutations) {
            mutations.forEach(function(mutation) {
                if (mutation.type === 'childList') {
                    // Re-apply any necessary customizations to new content
                    applyJobStatusColors();
                    enhanceStatistics();
                }
            });
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    }

    function applyJobStatusColors() {
        // Enhance job status indicators with better colors
        const statusElements = document.querySelectorAll('.label, .badge');
        statusElements.forEach(function(element) {
            const text = element.textContent.toLowerCase();
            
            if (text.includes('succeeded') || text.includes('success')) {
                element.style.backgroundColor = '#27ae60';
            } else if (text.includes('failed') || text.includes('error')) {
                element.style.backgroundColor = '#e74c3c';
            } else if (text.includes('processing') || text.includes('running')) {
                element.style.backgroundColor = '#f39c12';
            } else if (text.includes('enqueued') || text.includes('waiting')) {
                element.style.backgroundColor = '#3498db';
            }
        });
    }

    function enhanceStatistics() {
        // Add visual enhancements to statistics
        const metrics = document.querySelectorAll('.metric');
        metrics.forEach(function(metric) {
            metric.style.transition = 'transform 0.2s ease';
            metric.addEventListener('mouseenter', function() {
                this.style.transform = 'translateY(-2px)';
                this.style.boxShadow = '0 4px 8px rgba(0,0,0,0.15)';
            });
            metric.addEventListener('mouseleave', function() {
                this.style.transform = 'translateY(0)';
                this.style.boxShadow = '0 2px 4px rgba(0,0,0,0.1)';
            });
        });
    }

    // Auto-refresh functionality
    function setupAutoRefresh() {
        let autoRefreshInterval;
        
        // Create auto-refresh toggle
        const autoRefreshToggle = document.createElement('div');
        autoRefreshToggle.innerHTML = `
            <div style="position: fixed; bottom: 80px; right: 20px; background: white; padding: 10px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.15); z-index: 1000;">
                <label style="display: flex; align-items: center; gap: 8px; margin: 0; cursor: pointer;">
                    <input type="checkbox" id="auto-refresh-toggle" onchange="toggleAutoRefresh(this)">
                    <span style="font-size: 12px; color: #666;">Auto-refresh (30s)</span>
                </label>
            </div>
        `;
        document.body.appendChild(autoRefreshToggle);

        // Add toggle function to global scope
        window.toggleAutoRefresh = function(checkbox) {
            if (checkbox.checked) {
                autoRefreshInterval = setInterval(function() {
                    location.reload();
                }, 30000);
            } else {
                clearInterval(autoRefreshInterval);
            }
        };
    }

    // Initialize auto-refresh
    setupAutoRefresh();

    // Add global styles for better mobile experience
    function addMobileStyles() {
        const mobileStyles = document.createElement('style');
        mobileStyles.textContent = `
            @media (max-width: 768px) {
                #techway-header > div > div {
                    flex-direction: column !important;
                    gap: 10px !important;
                }
                
                #floating-refresh {
                    bottom: 10px !important;
                    right: 10px !important;
                }
                
                .table-responsive {
                    font-size: 0.8rem !important;
                }
            }
        `;
        document.head.appendChild(mobileStyles);
    }

    addMobileStyles();

})();
