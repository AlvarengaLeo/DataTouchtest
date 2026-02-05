/**
 * SwipeNavigation - Instagram Stories-style swipe navigation between modules
 * Only active on mobile/tablet (≤960px)
 * Limited to 4 main modules: Inicio, Leads, Agenda, Cotiz
 * "Más" is excluded from swipe navigation (tap only)
 *
 * IMPORTANT: Detects scrollable carousels and gives them gesture priority
 * with edge handoff - when carousel reaches edge, navigation takes over
 */
window.SwipeNavigation = (function () {
    'use strict';

    // Configuration
    const config = {
        threshold: 60,              // Reduced for faster response
        velocityThreshold: 0.4,     // More sensitive to velocity
        angleThreshold: 30,         // Slightly more permissive
        edgeResistance: 0.3,        // Resistance at navigation edges
        peekAmount: 50,             // Reduced peek
        animationDuration: 350,     // Faster animations
        edgeHandoffThreshold: 15,   // Pixels of "insistence" for handoff from carousel
        breakpoint: 960,            // Max width for swipe to be active
        maxIndex: 3,                // Maximum index (0-3 for 4 modules, excludes "Más")
        // Selectors for scrollable elements that should have gesture priority
        scrollableSelectors: [
            '.kpi-carousel',
            '.kpi-track',
            '.metrics-row',
            '.status-cards',
            '.horizontal-scroll',
            '[data-swipe-priority="true"]',
            '.mud-tabs-panels'
        ]
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
        isEnabled: false,
        isInsideScrollable: false,
        scrollableElement: null,
        // Edge handoff state
        edgeHandoffActive: false,
        carouselAtEdge: false,
        edgeAccumulator: 0,
        lastDeltaX: undefined
    };

    /**
     * Check if the touch started inside a scrollable element
     * that should have gesture priority
     */
    function isInsideScrollableElement(target) {
        let element = target;
        while (element && element !== document.body) {
            // Check if element matches any scrollable selector
            for (const selector of config.scrollableSelectors) {
                if (element.matches && element.matches(selector)) {
                    // Check if the element is actually scrollable horizontally
                    if (element.scrollWidth > element.clientWidth) {
                        return { isScrollable: true, element: element };
                    }
                }
            }
            element = element.parentElement;
        }
        return { isScrollable: false, element: null };
    }

    /**
     * Check if the scrollable element can still scroll in the swipe direction
     */
    function canScrollInDirection(element, deltaX) {
        if (!element) return false;

        const scrollLeft = element.scrollLeft;
        const maxScroll = element.scrollWidth - element.clientWidth;

        // Swiping right (content moves left) - check if can scroll left
        if (deltaX > 0) {
            return scrollLeft > 1; // > 1 for rounding tolerance
        }
        // Swiping left (content moves right) - check if can scroll right
        if (deltaX < 0) {
            return scrollLeft < maxScroll - 1; // -1 for rounding errors
        }
        return false;
    }

    /**
     * Check if carousel is at its edge (can't scroll further in swipe direction)
     */
    function isCarouselAtEdge(element, deltaX) {
        if (!element) return false;

        const scrollLeft = element.scrollLeft;
        const maxScroll = element.scrollWidth - element.clientWidth;

        // Swiping right (wants to go to previous module) - is at left edge?
        if (deltaX > 0 && scrollLeft <= 1) return true;

        // Swiping left (wants to go to next module) - is at right edge?
        if (deltaX < 0 && scrollLeft >= maxScroll - 1) return true;

        return false;
    }

    // Touch handlers
    function handleTouchStart(e) {
        if (!state.isEnabled || !e.touches.length) return;

        const touch = e.touches[0];

        // Check if touch is inside a scrollable element
        const scrollableCheck = isInsideScrollableElement(e.target);
        state.isInsideScrollable = scrollableCheck.isScrollable;
        state.scrollableElement = scrollableCheck.element;

        state.startX = touch.clientX;
        state.startY = touch.clientY;
        state.currentX = touch.clientX;
        state.deltaX = 0;
        state.startTime = Date.now();
        state.isSwiping = false;
        state.isHorizontalSwipe = null;

        // Reset edge handoff state
        state.edgeHandoffActive = false;
        state.carouselAtEdge = false;
        state.edgeAccumulator = 0;
        state.lastDeltaX = undefined;
    }

    function handleTouchMove(e) {
        if (!state.isEnabled || !e.touches.length) return;

        const touch = e.touches[0];
        const deltaX = touch.clientX - state.startX;
        const deltaY = touch.clientY - state.startY;

        // Determine direction on first significant movement
        if (state.isHorizontalSwipe === null) {
            const absX = Math.abs(deltaX);
            const absY = Math.abs(deltaY);

            // Need minimum movement to determine direction
            if (absX < 15 && absY < 15) return;

            // Calculate angle from horizontal
            const angle = Math.atan2(absY, absX) * (180 / Math.PI);

            // If angle is too steep, this is a vertical scroll
            if (angle > config.angleThreshold) {
                state.isHorizontalSwipe = false;
                return;
            }

            // If inside a scrollable element that CAN scroll in this direction
            if (state.isInsideScrollable && canScrollInDirection(state.scrollableElement, deltaX)) {
                // Delegate to carousel INITIALLY
                state.isHorizontalSwipe = false;
                state.carouselAtEdge = false;
                state.edgeAccumulator = 0;
                return;
            }

            // If carousel is already at edge, take control immediately
            if (state.isInsideScrollable && isCarouselAtEdge(state.scrollableElement, deltaX)) {
                state.isHorizontalSwipe = true;
                state.edgeHandoffActive = true;
            } else {
                state.isHorizontalSwipe = true;
            }
        }

        // === Edge Handoff during carousel scroll ===
        // If gesture was delegated to carousel, monitor if it reaches edge
        if (state.isHorizontalSwipe === false && state.isInsideScrollable) {
            if (isCarouselAtEdge(state.scrollableElement, deltaX)) {
                // Accumulate movement "beyond" the edge
                const movementDelta = Math.abs(deltaX - (state.lastDeltaX || deltaX));
                state.edgeAccumulator += movementDelta;
                state.lastDeltaX = deltaX;

                // If user insists enough, activate handoff
                if (state.edgeAccumulator > config.edgeHandoffThreshold) {
                    state.isHorizontalSwipe = true;
                    state.edgeHandoffActive = true;
                    // Adjust start position for smooth takeover
                    state.startX = touch.clientX - (deltaX > 0 ? 20 : -20);
                    // Continue to apply transform below
                }
            } else {
                // Reset accumulator if moving away from edge
                state.edgeAccumulator = 0;
                state.lastDeltaX = deltaX;
                return; // Let carousel handle
            }

            if (!state.isHorizontalSwipe) return;
        }

        // If not horizontal swipe, let normal scroll happen
        if (!state.isHorizontalSwipe) return;

        // Prevent vertical scrolling during horizontal swipe
        e.preventDefault();

        state.isSwiping = true;
        state.currentX = touch.clientX;

        // Recalculate deltaX after potential startX adjustment
        const currentDeltaX = touch.clientX - state.startX;

        // Calculate delta with edge resistance
        let adjustedDeltaX = currentDeltaX;

        // Apply resistance at navigation edges (first and last of 4 modules)
        const isAtStart = state.currentIndex === 0 && currentDeltaX > 0;
        const isAtEnd = state.currentIndex >= config.maxIndex && currentDeltaX < 0;

        if (isAtStart || isAtEnd) {
            adjustedDeltaX = currentDeltaX * config.edgeResistance;
        }

        state.deltaX = adjustedDeltaX;

        // Apply transform - ONLY translateX for smooth "follow finger" effect
        if (state.wrapper) {
            state.wrapper.classList.add('swiping');
            state.wrapper.style.transition = 'none'; // Immediate during drag
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
                navigateToIndex(state.currentIndex + 1, velocity);
            } else if (deltaX > 0 && state.currentIndex > 0) {
                // Swipe right - go to previous (only if not at first module)
                navigateToIndex(state.currentIndex - 1, velocity);
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

    function navigateToIndex(index, velocity = 0) {
        // Clamp index to valid range (0 to maxIndex, excludes "Más")
        index = Math.max(0, Math.min(index, config.maxIndex));

        if (!state.dotNetRef) return;

        // Calculate dynamic duration based on velocity (faster swipe = faster animation)
        const baseDuration = config.animationDuration;
        const speedFactor = Math.min(velocity * 0.3, 0.3); // Max 30% faster
        const duration = Math.max(baseDuration * (1 - speedFactor), 280);

        // Animate out - ONLY translateX, no scale for smooth slide
        if (state.wrapper) {
            const direction = index > state.currentIndex ? -1 : 1;
            // Use spring-like easing
            state.wrapper.style.transition = `transform ${duration}ms cubic-bezier(0.32, 0.72, 0, 1)`;
            // Slide completely off screen
            state.wrapper.style.transform = `translateX(${direction * window.innerWidth}px)`;
        }

        // Call Blazor to navigate after starting animation
        setTimeout(() => {
            state.dotNetRef.invokeMethodAsync('NavigateToIndex', index)
                .catch(err => console.error('SwipeNavigation: Failed to navigate', err));
        }, duration * 0.4);
    }

    function snapBack(withBounce) {
        if (state.wrapper) {
            // Spring-like easing
            const easing = 'cubic-bezier(0.32, 0.72, 0, 1)';

            if (withBounce) {
                // Subtle micro-bounce (8px max)
                const bounceDistance = state.deltaX > 0 ? 8 : -8;
                state.wrapper.style.transition = `transform 150ms ${easing}`;
                state.wrapper.style.transform = `translateX(${bounceDistance}px)`;

                setTimeout(() => {
                    if (state.wrapper) {
                        state.wrapper.style.transition = `transform 200ms ${easing}`;
                        state.wrapper.style.transform = 'translateX(0)';
                    }
                }, 150);
            } else {
                // Smooth return
                state.wrapper.style.transition = `transform 300ms ${easing}`;
                state.wrapper.style.transform = 'translateX(0)';
            }
        }
        hidePeekIndicators();
    }

    function resetState() {
        state.isSwiping = false;
        state.isHorizontalSwipe = null;
        state.deltaX = 0;
        state.isInsideScrollable = false;
        state.scrollableElement = null;
        state.edgeHandoffActive = false;
        state.carouselAtEdge = false;
        state.edgeAccumulator = 0;
        state.lastDeltaX = undefined;

        if (state.wrapper) {
            state.wrapper.classList.remove('swiping');
            // Clean up styles after animation completes
            setTimeout(() => {
                if (state.wrapper) {
                    state.wrapper.style.transition = '';
                    state.wrapper.style.transform = '';
                }
            }, 400);
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
            leftPeek.style.opacity = progress * 0.9;
            leftPeek.style.transform = `translateX(${Math.min(deltaX * 0.4, config.peekAmount)}px)`;
            rightPeek.style.opacity = '0';
        }
        // Show right peek only if swiping left AND not at end (maxIndex)
        else if (deltaX < 0 && !isAtEnd && state.currentIndex < config.maxIndex) {
            rightPeek.style.opacity = progress * 0.9;
            rightPeek.style.transform = `translateX(${Math.max(deltaX * 0.4, -config.peekAmount)}px)`;
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
            leftPeek.style.transition = 'opacity 200ms ease, transform 200ms ease';
            leftPeek.style.opacity = '0';
            leftPeek.style.transform = 'translateX(0)';
        }
        if (rightPeek) {
            rightPeek.style.transition = 'opacity 200ms ease, transform 200ms ease';
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

            console.log('SwipeNavigation: Initialized with edge handoff detection', {
                routes: state.routes,
                currentIndex: state.currentIndex,
                maxIndex: config.maxIndex,
                enabled: state.isEnabled,
                edgeHandoffThreshold: config.edgeHandoffThreshold
            });
        },

        updateIndex: function (index) {
            // Clamp to valid range (0-3 for main modules)
            state.currentIndex = Math.min(index, config.maxIndex);
            // Reset any lingering transform with smooth transition
            if (state.wrapper) {
                state.wrapper.style.transition = 'transform 300ms cubic-bezier(0.32, 0.72, 0, 1)';
                state.wrapper.style.transform = 'translateX(0)';

                setTimeout(() => {
                    if (state.wrapper) {
                        state.wrapper.style.transition = '';
                        state.wrapper.style.transform = '';
                    }
                }, 350);
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
