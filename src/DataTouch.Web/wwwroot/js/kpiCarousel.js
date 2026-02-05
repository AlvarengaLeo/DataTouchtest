/**
 * KPI Carousel - Premium swipe carousel for mobile
 * Handles scroll snapping and indicator updates with smooth animations
 */
window.KpiCarousel = {
    initialized: false,
    carousel: null,
    track: null,
    indicators: null,
    cards: null,
    currentIndex: 0,
    isScrolling: false,
    scrollTimeout: null,

    init: function () {
        if (this.initialized) return;

        this.carousel = document.getElementById('kpi-carousel');
        if (!this.carousel) return;

        this.track = this.carousel.querySelector('.kpi-track');
        this.indicators = document.querySelectorAll('.kpi-indicators .indicator');
        this.cards = this.carousel.querySelectorAll('.kpi-card-premium');

        if (this.cards.length === 0 || this.indicators.length === 0) return;

        // Listen for scroll events with throttling for smooth updates
        this.carousel.addEventListener('scroll', this.handleScroll.bind(this), { passive: true });

        // Click on indicators to navigate
        this.indicators.forEach((indicator, index) => {
            indicator.addEventListener('click', () => this.scrollToCard(index));
        });

        // Touch events for better mobile experience
        this.carousel.addEventListener('touchstart', this.onTouchStart.bind(this), { passive: true });
        this.carousel.addEventListener('touchend', this.onTouchEnd.bind(this), { passive: true });

        // Initial state
        this.updateIndicators(0);
        this.initialized = true;
    },

    onTouchStart: function () {
        this.isScrolling = true;
    },

    onTouchEnd: function () {
        this.isScrolling = false;
        // Ensure final position is updated after touch ends
        setTimeout(() => this.handleScroll(), 100);
    },

    handleScroll: function () {
        if (!this.carousel || !this.cards.length) return;

        // Clear any pending timeout
        if (this.scrollTimeout) {
            clearTimeout(this.scrollTimeout);
        }

        const scrollLeft = this.carousel.scrollLeft;
        const cardWidth = this.cards[0].offsetWidth;
        const gap = 12; // Gap between cards
        const totalCardWidth = cardWidth + gap;

        // Calculate progress and index
        const exactProgress = scrollLeft / totalCardWidth;
        const activeIndex = Math.round(exactProgress);
        const clampedIndex = Math.max(0, Math.min(activeIndex, this.cards.length - 1));

        // Update indicators immediately for responsive feel
        if (clampedIndex !== this.currentIndex) {
            this.currentIndex = clampedIndex;
            this.updateIndicators(clampedIndex);
        }

        // Also update on scroll end for final position accuracy
        this.scrollTimeout = setTimeout(() => {
            this.finalizePosition();
        }, 150);
    },

    finalizePosition: function () {
        if (!this.carousel || !this.cards.length) return;

        const scrollLeft = this.carousel.scrollLeft;
        const cardWidth = this.cards[0].offsetWidth;
        const gap = 12;
        const totalCardWidth = cardWidth + gap;

        const finalIndex = Math.round(scrollLeft / totalCardWidth);
        const clampedIndex = Math.max(0, Math.min(finalIndex, this.cards.length - 1));

        if (clampedIndex !== this.currentIndex) {
            this.currentIndex = clampedIndex;
            this.updateIndicators(clampedIndex);
        }
    },

    updateIndicators: function (activeIndex) {
        if (!this.indicators) return;

        this.indicators.forEach((indicator, index) => {
            if (index === activeIndex) {
                indicator.classList.add('active');
            } else {
                indicator.classList.remove('active');
            }
        });
    },

    scrollToCard: function (index) {
        if (!this.carousel || !this.cards.length) return;
        if (index < 0 || index >= this.cards.length) return;

        const cardWidth = this.cards[0].offsetWidth;
        const gap = 12;
        const totalCardWidth = cardWidth + gap;

        this.carousel.scrollTo({
            left: totalCardWidth * index,
            behavior: 'smooth'
        });

        // Update indicators immediately for responsive feedback
        this.currentIndex = index;
        this.updateIndicators(index);
    },

    destroy: function () {
        if (this.scrollTimeout) {
            clearTimeout(this.scrollTimeout);
        }
        this.initialized = false;
        this.carousel = null;
        this.track = null;
        this.indicators = null;
        this.cards = null;
        this.currentIndex = 0;
    }
};

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    // Delay to ensure Blazor has rendered
    setTimeout(() => window.KpiCarousel.init(), 300);
});

// Re-initialize on Blazor navigation
if (typeof Blazor !== 'undefined') {
    Blazor.addEventListener('enhancedload', () => {
        window.KpiCarousel.destroy();
        setTimeout(() => window.KpiCarousel.init(), 300);
    });
}

// Also try to initialize after a longer delay in case of slow render
setTimeout(() => {
    if (!window.KpiCarousel.initialized) {
        window.KpiCarousel.init();
    }
}, 1000);
