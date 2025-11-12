// Admin table sorting functionality
(function () {
    'use strict';

    function initTableSorting() {
        const tables = document.querySelectorAll('.admin-table-wrapper table');
        
        tables.forEach(table => {
            const headers = table.querySelectorAll('thead th.sortable-header');
            
            headers.forEach((header, index) => {
                header.addEventListener('click', function () {
                    const tbody = table.querySelector('tbody');
                    const rows = Array.from(tbody.querySelectorAll('tr'));
                    
                    // Determine sort direction
                    const currentSort = header.dataset.sort || 'none';
                    let newSort = 'asc';
                    
                    if (currentSort === 'asc') {
                        newSort = 'desc';
                    } else if (currentSort === 'desc') {
                        newSort = 'asc';
                    }
                    
                    // Reset all headers
                    table.querySelectorAll('thead th').forEach(th => {
                        th.classList.remove('sort-asc', 'sort-desc', 'sort-none');
                        delete th.dataset.sort;
                    });
                    
                    // Set current header
                    header.classList.add(`sort-${newSort}`);
                    header.dataset.sort = newSort;
                    
                    // Sort rows
                    rows.sort((a, b) => {
                        const aCell = a.cells[index];
                        const bCell = b.cells[index];
                        
                        if (!aCell || !bCell) return 0;
                        
                        const aText = aCell.textContent.trim();
                        const bText = bCell.textContent.trim();
                        
                        // Try to parse as number
                        const aNum = parseFloat(aText.replace(/[^\d.-]/g, ''));
                        const bNum = parseFloat(bText.replace(/[^\d.-]/g, ''));
                        
                        if (!isNaN(aNum) && !isNaN(bNum)) {
                            return newSort === 'asc' ? aNum - bNum : bNum - aNum;
                        }
                        
                        // String comparison
                        const comparison = aText.localeCompare(bText, 'vi', { numeric: true, sensitivity: 'base' });
                        return newSort === 'asc' ? comparison : -comparison;
                    });
                    
                    // Re-append sorted rows
                    rows.forEach(row => tbody.appendChild(row));
                });
            });
        });
    }

    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initTableSorting);
    } else {
        initTableSorting();
    }
})();

