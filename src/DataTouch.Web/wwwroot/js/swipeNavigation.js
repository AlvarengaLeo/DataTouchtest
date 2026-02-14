/**
 * SwipeNavigation - Instagram Stories-style swipe navigation between modules
 * Only active on mobile/tablet (≤960px)
 * Limited to 4 main modules: Inicio, Leads, Agenda, Cotiz
 * "Más" is excluded from swipe navigation (tap only)
 */
window.SwipeNavigation = (function () {
    'use strict';

    // Configuration
    const config = {
        threshold: 50,           // Minimum distance to trigger navigation
        velocityThreshold: 0.3,  // Minimum velocity for momentum navigation
        angleThreshold: 30,      // Maximum angle from horizontal (degrees)
        edgeResistance: 0.3,     // Resistance factor at edges
        peekAmount: 60,          // Pixels of peek visible during drag
        animationDuration: 300,  // Transition duration in ms
        breakpoint: 960,         // Max width for swipe to be active
        maxIndex: 3              // Maximum index (0-3 for 4 modules, excludes "Más")
    };

    // State
    let state = {
        dotNetRef: null,
        routes: [],
        currentIndex: 0,
        wrapper: null,
        startX: 0,
        startY: 0,
        currentX: 0,
        deltaX: 0,
        startTime: 0,
        isSwiping: false,
        isHorizontalSwipe: null,
        isEnabled: false
    };

    // Touch handlers
    function handleTouchStart(e) {
        if (!state.isEnabled || !e.touches.length) return;

        const touch = e.touches[0];
        state.startX = touch.clientX;
        state.startY = touch.clientY;
        state.currentX = touch.clientX;
        state.deltaX = 0;
        state.startTime = Date.now();
        state.isSwiping = false;
        state.isHorizontalSwipe = null;
    }

    function handleTouchMove(e) {
        if (!state.isEnabled || !e.touches.length) return;

        const touch = e.touches[0];
        const deltaX = touch.clientX - state.startX;
        const deltaY = touch.clientY - state.startY;

        // Determine if horizontal or vertical on first significant movement
        if (state.isHorizontalSwipe === null) {
            const absX = Math.abs(deltaX);
            const absY = Math.abs(deltaY);

            // Need minimum movement to determine direction
            if (absX < 10 && absY < 10) return;

            // Calculate angle from horizontal
            const angle = Math.atan2(absY, absX) * (180 / Math.PI);

            // If angle is too steep, this is a vertical scroll
            if (angle > config.angleThreshold) {
                state.isHorizontalSwipe = false;
                return;
            }

            state.isHorizontalSwipe = true;
        }

        // If not horizontal swipe, let normal scroll happen
        if (!state.isHorizontalSwipe) return;

        // Prevent vertical scrolling during horizontal swipe
        e.preventDefault();

        state.isSwiping = true;
        state.currentX = touch.clientX;

        // Calculate delta with edge resistance
        let adjustedDeltaX = deltaX;

        // Apply resistance at edges (first and last of 4 modules)
        const isAtStart = state.currentIndex === 0 && deltaX > 0;
        const isAtEnd = state.currentIndex >= config.maxIndex && deltaX < 0;

        if (isAtStart || isAtEnd) {
            adjustedDeltaX = deltaX * config.edgeResistance;
        }

        state.deltaX = adjustedDeltaX;

        // Apply transform for peek effect
        if (state.wrapper) {
            state.wrapper.classList.add('swiping');
            state.wrapper.style.transform = `translateX(${adjustedDeltaX}px)`;
        }

        // Update peek indicators (only if not at edges)
        updatePeekIndicators(adjustedDeltaX, isAtStart, isAtEnd);
    }

    function handleTouchEnd(e) {
        if (!state.isEnabled || !state.isSwiping) {
            resetState();
            return;
        }

        const deltaX = state.deltaX;
        const deltaTime = Date.now() - state.startTime;
        const velocity = Math.abs(deltaX) / deltaTime;

        // Determine if we should navigate
        const shouldNavigate = Math.abs(deltaX) > config.threshold || velocity > config.velocityThreshold;

        if (shouldNavigate) {
            if (deltaX < 0 && state.currentIndex < config.maxIndex) {
                // Swipe left - go to next (only if not at last of 4 modules)
                navigateToIndex(state.currentIndex + 1);
            } else if (deltaX > 0 && state.currentIndex > 0) {
                // Swipe right - go to previous (only if not at first module)
                navigateToIndex(state.currentIndex - 1);
            } else {
                // At edge - snap back with bounce effect
                snapBack(true);
            }
        } else {
            // Not enough movement, snap back
            snapBack(false);
        }

        resetState();
    }

    function navigateToIndex(index) {
        // Clamp index to valid range (0 to maxIndex, excludes "Más")
        index = Math.max(0, Math.min(index, config.maxIndex));

        if (!state.dotNetRef) return;

        // Animate out
        if (state.wrapper) {
            const direction = index > state.currentIndex ? -1 : 1;
            state.wrapper.style.transition = `transform ${config.animationDuration}ms cubic-bezier(0.25, 0.46, 0.45, 0.94)`;
            state.wrapper.style.transform = `translateX(${direction * 100}px)`;
        }

        // Call Blazor to navigate
        state.dotNetRef.invokeMethodAsync('NavigateToIndex', index)
            .catch(err => console.error('SwipeNavigation: Failed to navigate', err));
    }

    function snapBack(withBounce) {
        if (state.wrapper) {
            if (withBounce) {
                // Bounce effect for edge resistance
                const bounceDistance = state.deltaX > 0 ? 15 : -15;
                state.wrapper.style.transition = `transform 150ms cubic-bezier(0.25, 0.46, 0.45, 0.94)`;
                state.wrapper.style.transform = `translateX(${bounceDistance}px)`;

                setTimeout(() => {
                    if (state.wrapper) {
                        state.wrapper.style.transition = `transform 200ms cubic-bezier(0.25, 0.46, 0.45, 0.94)`;
                        state.wrapper.style.transform = 'translateX(0)';
                    }
                }, 150);
            } else {
                state.wrapper.style.transition = `transform ${config.animationDuration}ms cubic-bezier(0.25, 0.46, 0.45, 0.94)`;
                state.wrapper.style.transform = 'translateX(0)';
            }
        }
        hidePeekIndicators();
    }

    function resetState() {
        state.isSwiping = false;
        state.isHorizontalSwipe = null;
        state.deltaX = 0;

        if (state.wrapper) {
            state.wrapper.classList.remove('swiping');
            // Reset after animation
            setTimeout(() => {
                if (state.wrapper) {
                    state.wrapper.style.transition = '';
                    state.wrapper.style.transform = '';
                }
            }, config.animationDuration + 50);
        }

        hidePeekIndicators();
    }

    function updatePeekIndicators(deltaX, isAtStart, isAtEnd) {
        const leftPeek = document.getElementById('swipe-peek-left');
        const rightPeek = document.getElementById('swipe-peek-right');

        if (!leftPeek || !rightPeek) return;

        const progress = Math.min(Math.abs(deltaX) / config.peekAmount, 1);

        // Show left peek only if swiping right AND not at start
        if (deltaX > 0 && !isAtStart && state.currentIndex > 0) {
            leftPeek.style.opacity = progress * 0.8;
            leftPeek.style.transform = `translateX(${Math.min(deltaX * 0.3, config.peekAmount)}px)`;
            rightPeek.style.opacity = '0';
        }
        // Show right peek only if swiping left AND not at end (maxIndex)
        else if (deltaX < 0 && !isAtEnd && state.currentIndex < config.maxIndex) {
            rightPeek.style.opacity = progress * 0.8;
            rightPeek.style.transform = `translateX(${Math.max(deltaX * 0.3, -config.peekAmount)}px)`;
            leftPeek.style.opacity = '0';
        } else {
            // At edges - don't show peek indicators
            leftPeek.style.opacity = '0';
            rightPeek.style.opacity = '0';
        }
    }

    function hidePeekIndicators() {
        const leftPeek = document.getElementById('swipe-peek-left');
        const rightPeek = document.getElementById('swipe-peek-right');

        if (leftPeek) {
            leftPeek.style.opacity = '0';
            leftPeek.style.transform = 'translateX(0)';
        }
        if (rightPeek) {
            rightPeek.style.opacity = '0';
            rightPeek.style.transform = 'translateX(0)';
        }
    }

    function checkBreakpoint() {
        const wasEnabled = state.isEnabled;
        state.isEnabled = window.innerWidth <= config.breakpoint;

        if (wasEnabled && !state.isEnabled) {
            // Disable - remove swiping state
            resetState();
        }
    }

    function handleResize() {
        checkBreakpoint();
    }

    // Public API
    return {
        init: function (dotNetRef, routes, currentIndex) {
            state.dotNetRef = dotNetRef;
            // Only use first 4 routes (exclude "Más")
            state.routes = (routes || []).slice(0, 4);
            // Clamp currentIndex to valid range
            state.currentIndex = Math.min(currentIndex || 0, config.maxIndex);
            state.wrapper = document.getElementById('swipe-nav-wrapper');

            if (!state.wrapper) {
                console.warn('SwipeNavigation: Wrapper element not found');
                return;
            }

            // Check if should be enabled based on screen size
            checkBreakpoint();

            // Add event listeners
            state.wrapper.addEventListener('touchstart', handleTouchStart, { passive: true });
            state.wrapper.addEventListener('touchmove', handleTouchMove, { passive: false });
            state.wrapper.addEventListener('touchend', handleTouchEnd, { passive: true });
            state.wrapper.addEventListener('touchcancel', handleTouchEnd, { passive: true });

            window.addEventListener('resize', handleResize);

            console.log('SwipeNavigation: Initialized', {
                routes: state.routes,
                currentIndex: state.currentIndex,
                maxIndex: config.maxIndex,
                enabled: state.isEnabled
            });
        },

        updateIndex: function (index) {
            // Clamp to valid range (0-3 for main modules)
            state.currentIndex = Math.min(index, config.maxIndex);
            // Reset any lingering transform
            if (state.wrapper) {
                state.wrapper.style.transform = '';
            }
        },

        destroy: function () {
            if (state.wrapper) {
                state.wrapper.removeEventListener('touchstart', handleTouchStart);
                state.wrapper.removeEventListener('touchmove', handleTouchMove);
                state.wrapper.removeEventListener('touchend', handleTouchEnd);
                state.wrapper.removeEventListener('touchcancel', handleTouchEnd);
            }

            window.removeEventListener('resize', handleResize);

            state.dotNetRef = null;
            state.wrapper = null;
            state.isEnabled = false;

            console.log('SwipeNavigation: Destroyed');
        }
    };
})();
