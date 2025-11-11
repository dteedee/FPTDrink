// ============================================
// FPTDrink Website - Main JavaScript
// Optimized for Performance & UX
// ============================================

(function () {
    'use strict';

    // === Toast Notification System ===
    const Toast = {
        show: function (message, type = 'info', duration = 3000) {
            const toastContainer = this.getOrCreateContainer();
            const toastId = 'toast-' + Date.now();
            const iconMap = {
                success: '✓',
                error: '✕',
                warning: '⚠',
                info: 'ℹ'
            };
            const bgMap = {
                success: 'bg-success',
                error: 'bg-danger',
                warning: 'bg-warning',
                info: 'bg-info'
            };

            const toastHtml = `
                <div id="${toastId}" class="toast show" role="alert" aria-live="assertive" aria-atomic="true">
                    <div class="toast-header ${bgMap[type] || 'bg-info'} text-white">
                        <strong class="me-auto">${iconMap[type] || 'ℹ'} ${type.charAt(0).toUpperCase() + type.slice(1)}</strong>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
                    </div>
                    <div class="toast-body">${message}</div>
                </div>
            `;

            toastContainer.insertAdjacentHTML('beforeend', toastHtml);
            const toastElement = document.getElementById(toastId);
            const bsToast = new bootstrap.Toast(toastElement, { delay: duration });
            bsToast.show();

            toastElement.addEventListener('hidden.bs.toast', function () {
                toastElement.remove();
            });
        },
        getOrCreateContainer: function () {
            let container = document.querySelector('.toast-container');
            if (!container) {
                container = document.createElement('div');
                container.className = 'toast-container';
                document.body.appendChild(container);
            }
            return container;
        }
    };

    // === Lazy Loading Images ===
    function initLazyLoading() {
        if ('loading' in HTMLImageElement.prototype) {
            // Native lazy loading supported
            const images = document.querySelectorAll('img[data-src]');
            images.forEach(img => {
                img.src = img.dataset.src;
                img.removeAttribute('data-src');
                img.loading = 'lazy';
                img.addEventListener('load', function () {
                    this.classList.add('loaded');
                });
            });
        } else {
            // Fallback: Intersection Observer
            const imageObserver = new IntersectionObserver((entries, observer) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const img = entry.target;
                        img.src = img.dataset.src;
                        img.removeAttribute('data-src');
                        img.classList.add('loaded');
                        observer.unobserve(img);
                    }
                });
            });

            document.querySelectorAll('img[data-src]').forEach(img => {
                imageObserver.observe(img);
            });
        }
    }

    // === Form Validation Enhancement ===
    function enhanceFormValidation() {
        const forms = document.querySelectorAll('form[method="post"]');
        forms.forEach(form => {
            form.addEventListener('submit', function (e) {
                if (!form.checkValidity()) {
                    e.preventDefault();
                    e.stopPropagation();
                }
                form.classList.add('was-validated');
            }, false);
        });
    }

    // === Quantity Control ===
    function initQuantityControls() {
        document.querySelectorAll('[data-quantity]').forEach(function (container) {
            const input = container.querySelector('input[type="number"]');
            if (!input) return;

            const btnMinus = container.querySelector('.btn-minus');
            const btnPlus = container.querySelector('.btn-plus');

            if (btnMinus) {
                btnMinus.addEventListener('click', function () {
                    const current = parseInt(input.value || '1', 10);
                    input.value = Math.max(1, current - 1);
                    input.dispatchEvent(new Event('change'));
                });
            }

            if (btnPlus) {
                btnPlus.addEventListener('click', function () {
                    const current = parseInt(input.value || '1', 10);
                    input.value = current + 1;
                    input.dispatchEvent(new Event('change'));
                });
            }
        });
    }

    // === Add to Cart with Loading State ===
    function enhanceAddToCart() {
        document.querySelectorAll('form[action*="ShoppingCart/Add"]').forEach(form => {
            form.addEventListener('submit', function (e) {
                const btn = form.querySelector('button[type="submit"]');
                if (btn) {
                    const originalText = btn.innerHTML;
                    btn.disabled = true;
                    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Đang thêm...';

                    // Re-enable after 3 seconds if no redirect happens
                    setTimeout(() => {
                        btn.disabled = false;
                        btn.innerHTML = originalText;
                    }, 3000);
                }
            });
        });
    }

    // === Loading Overlay ===
    const LoadingOverlay = {
        show: function () {
            let overlay = document.querySelector('.loading-overlay');
            if (!overlay) {
                overlay = document.createElement('div');
                overlay.className = 'loading-overlay';
                overlay.innerHTML = '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div>';
                document.body.appendChild(overlay);
            }
            overlay.classList.add('active');
        },
        hide: function () {
            const overlay = document.querySelector('.loading-overlay');
            if (overlay) {
                overlay.classList.remove('active');
            }
        }
    };

    // === Smooth Scroll to Anchor ===
    function initSmoothScroll() {
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', function (e) {
                const href = this.getAttribute('href');
                if (href === '#' || href === '') return;

                const target = document.querySelector(href);
                if (target) {
                    e.preventDefault();
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            });
        });
    }

    // === Debounce Function ===
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

    // === Search Enhancement ===
    function enhanceSearch() {
        const searchInputs = document.querySelectorAll('input[type="search"], input[name="q"]');
        searchInputs.forEach(input => {
            const form = input.closest('form');
            if (form) {
                const debouncedSubmit = debounce(() => {
                    // Optional: Auto-submit after typing stops (uncomment if needed)
                    // form.submit();
                }, 500);

                input.addEventListener('input', debouncedSubmit);
            }
        });
    }

    // === Initialize on DOM Ready ===
    function init() {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', init);
            return;
        }

        initLazyLoading();
        enhanceFormValidation();
        initQuantityControls();
        enhanceAddToCart();
        initSmoothScroll();
        enhanceSearch();
    }

    // === Expose to Global Scope ===
    window.FPTDrink = {
        Toast: Toast,
        LoadingOverlay: LoadingOverlay
    };

    // Start initialization
    init();
})();
