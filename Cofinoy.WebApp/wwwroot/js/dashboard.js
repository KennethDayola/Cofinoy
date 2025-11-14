// Dashboard JavaScript
// Update last updated timestamp
function updateTimestamp() {
    const lastUpdatedElement = document.getElementById('lastUpdated');
    if (!lastUpdatedElement) {
        console.warn('lastUpdated element not found');
        return;
    }

    const now = new Date();
    const timeString = now.toLocaleTimeString('en-US', {
        hour: 'numeric',
        minute: '2-digit',
        hour12: true
    });
    lastUpdatedElement.textContent = timeString;
}

// Refresh dashboard data
async function refreshDashboard() {
    const refreshBtn = document.querySelector('.btn-refresh');
    if (!refreshBtn) return;

    const icon = refreshBtn.querySelector('i');

    // Add spinning animation
    if (icon) icon.classList.add('fa-spin');
    refreshBtn.disabled = true;

    try {
        // Reload the page to get fresh data
        window.location.reload();
    } catch (error) {
        console.error('Error refreshing dashboard:', error);
    } finally {
        if (icon) icon.classList.remove('fa-spin');
        refreshBtn.disabled = false;
    }
}

// View order details
function viewOrder(orderId) {
    window.location.href = `/Order/ViewOrder?orderId=${orderId}`;
}

// Auto-refresh every 30 seconds - only if element exists
let updateInterval;
if (document.getElementById('lastUpdated')) {
    updateInterval = setInterval(() => {
        updateTimestamp();
    }, 30000);
}

// Animate stat cards on load
document.addEventListener('DOMContentLoaded', function () {
    const statCards = document.querySelectorAll('.stat-card');
    statCards.forEach((card, index) => {
        setTimeout(() => {
            card.style.opacity = '0';
            card.style.transform = 'translateY(20px)';
            card.style.transition = 'all 0.5s ease';

            setTimeout(() => {
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';
            }, 50);
        }, index * 100);
    });

    // Animate stat values
    animateStatValues();

    // Initial timestamp update
    updateTimestamp();
});

// Animate counting up stat values
function animateStatValues() {
    const statValues = document.querySelectorAll('.stat-value');

    statValues.forEach(statValue => {
        const text = statValue.textContent.trim();
        // Check if it's a number (with or without currency symbol)
        const numberMatch = text.match(/[\d,]+\.?\d*/);
        if (!numberMatch) return;

        const targetValue = parseFloat(numberMatch[0].replace(/,/g, ''));
        if (isNaN(targetValue)) return;

        const duration = 1000; // 1 second
        const steps = 60;
        const increment = targetValue / steps;
        let current = 0;

        const isCurrency = text.includes('₱');
        const hasDecimal = text.includes('.');

        const interval = setInterval(() => {
            current += increment;
            if (current >= targetValue) {
                current = targetValue;
                clearInterval(interval);
            }

            let displayValue = hasDecimal ? current.toFixed(2) : Math.floor(current);
            displayValue = displayValue.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',');

            if (isCurrency) {
                statValue.textContent = '₱ ' + displayValue;
            } else {
                statValue.textContent = displayValue;
            }
        }, duration / steps);
    });
}

// Initialize tooltips if needed
document.querySelectorAll('[data-tooltip]').forEach(element => {
    element.addEventListener('mouseenter', function () {
        const tooltip = document.createElement('div');
        tooltip.className = 'tooltip';
        tooltip.textContent = this.getAttribute('data-tooltip');
        document.body.appendChild(tooltip);

        const rect = this.getBoundingClientRect();
        tooltip.style.top = rect.top - tooltip.offsetHeight - 10 + 'px';
        tooltip.style.left = rect.left + (rect.width / 2) - (tooltip.offsetWidth / 2) + 'px';
    });

    element.addEventListener('mouseleave', function () {
        document.querySelectorAll('.tooltip').forEach(t => t.remove());
    });
});

console.log('Dashboard initialized successfully');