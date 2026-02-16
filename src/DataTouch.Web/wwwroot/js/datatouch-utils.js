/**
 * DataTouch Utility Functions
 * General-purpose JavaScript utilities for DataTouch Web
 */

/**
 * Download a file with the given content
 * @param {string} filename - Name of the file to download
 * @param {string} content - Content of the file
 * @param {string} mimeType - MIME type of the file
 */
window.downloadFile = function (filename, content, mimeType) {
    const blob = new Blob([content], { type: mimeType });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};

/**
 * Copy text to clipboard
 * @param {string} text - Text to copy
 * @returns {Promise<boolean>} - Whether the copy was successful
 */
window.copyToClipboard = async function (text) {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch (err) {
        console.error('Failed to copy:', err);
        return false;
    }
};

/**
 * Scroll a carousel container by one card width
 * @param {string} elementId - ID of the carousel container
 * @param {number} direction - -1 for left, 1 for right
 */
window.scrollCarousel = function (elementId, direction) {
    const el = document.getElementById(elementId);
    if (!el) return;
    const card = el.querySelector('.pgb-card');
    const scrollAmount = card ? card.offsetWidth + 10 : 200;
    el.scrollBy({ left: direction * scrollAmount, behavior: 'smooth' });
};

/**
 * Pause a video element by selector
 * @param {string} selector - CSS selector for the video element
 */
window.pauseVideo = function (selector) {
    const video = document.querySelector(selector);
    if (video && !video.paused) {
        video.pause();
    }
};

/**
 * Initialize a video player: attach error/canplay listeners, call .load()
 * Reports errors back to Blazor via dotNetRef
 * @param {string} selector - CSS selector for the video element
 * @param {object} dotNetRef - DotNet object reference for callbacks
 */
window.initVideoPlayer = function (selector, dotNetRef) {
    // Small delay to let Blazor finish rendering the DOM
    setTimeout(function () {
        const video = document.querySelector(selector);
        if (!video) return;

        // Error handler
        var onError = function () {
            if (dotNetRef) {
                try { dotNetRef.invokeMethodAsync('OnVideoPlayError'); } catch (e) { }
            }
        };

        // Attach error listener on the video element itself
        video.addEventListener('error', onError, { once: true });

        // Also listen on source children if they exist
        var sources = video.querySelectorAll('source');
        sources.forEach(function (src) {
            src.addEventListener('error', onError, { once: true });
        });

        // Auto-play when ready
        video.addEventListener('canplay', function () {
            try {
                var playPromise = video.play();
                if (playPromise !== undefined) {
                    playPromise.catch(function () {
                        // Autoplay blocked — user will click play manually
                    });
                }
            } catch (e) { }
        }, { once: true });

        // Force load the video
        try {
            video.load();
        } catch (e) {
            onError();
        }
    }, 100);
};

/**
 * Reload a video element after src change (used when navigating between videos)
 * @param {string} selector - CSS selector for the video element
 * @param {object} dotNetRef - DotNet object reference for error callbacks
 */
window.reloadVideo = function (selector, dotNetRef) {
    setTimeout(function () {
        var video = document.querySelector(selector);
        if (!video) return;

        // Pause and reset
        try { video.pause(); } catch (e) { }
        video.currentTime = 0;

        // Attach error handler
        if (dotNetRef) {
            var onError = function () {
                try { dotNetRef.invokeMethodAsync('OnVideoPlayError'); } catch (e) { }
            };
            video.addEventListener('error', onError, { once: true });
        }

        // Auto-play when ready
        video.addEventListener('canplay', function () {
            try {
                var playPromise = video.play();
                if (playPromise !== undefined) {
                    playPromise.catch(function () { });
                }
            } catch (e) { }
        }, { once: true });

        // Force reload with new src
        try {
            video.load();
        } catch (e) {
            if (dotNetRef) {
                try { dotNetRef.invokeMethodAsync('OnVideoPlayError'); } catch (e2) { }
            }
        }
    }, 100);
};

/**
 * Lightbox keyboard listener management
 */
window._lightboxKeyHandler = null;

window.addLightboxKeyListener = function (dotNetRef) {
    window.removeLightboxKeyListener();
    window._lightboxKeyHandler = function (e) {
        if (e.key === 'Escape') {
            dotNetRef.invokeMethodAsync('OnLightboxKeyClose');
        } else if (e.key === 'ArrowLeft') {
            e.preventDefault();
            dotNetRef.invokeMethodAsync('OnLightboxKeyPrev');
        } else if (e.key === 'ArrowRight') {
            e.preventDefault();
            dotNetRef.invokeMethodAsync('OnLightboxKeyNext');
        }
    };
    document.addEventListener('keydown', window._lightboxKeyHandler);
};

window.removeLightboxKeyListener = function () {
    if (window._lightboxKeyHandler) {
        document.removeEventListener('keydown', window._lightboxKeyHandler);
        window._lightboxKeyHandler = null;
    }
};

/**
 * Scroll to an element by CSS selector
 * @param {string} selector - CSS selector
 */
window.scrollToElement = function (selector) {
    const el = document.querySelector(selector);
    if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' });
};
