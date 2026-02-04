// Chart ResizeObserver for Dynamic ViewBox
// Measures .analytics-chart container and invokes Blazor callback with dimensions

window.chartResizeObserver = {
    observers: new Map(),

    observe: function (elementId, dotNetRef) {
        const element = document.getElementById(elementId);
        if (!element) {
            console.warn('chartResizeObserver: element not found:', elementId);
            return;
        }

        // Cleanup existing observer for this element
        if (this.observers.has(elementId)) {
            this.observers.get(elementId).disconnect();
        }

        const observer = new ResizeObserver((entries) => {
            for (const entry of entries) {
                const width = entry.contentRect.width;
                const height = entry.contentRect.height;
                
                if (width > 0 && height > 0) {
                    dotNetRef.invokeMethodAsync('OnChartContainerResized', width, height);
                }
            }
        });

        observer.observe(element);
        this.observers.set(elementId, observer);

        // Trigger initial measurement
        const rect = element.getBoundingClientRect();
        if (rect.width > 0 && rect.height > 0) {
            dotNetRef.invokeMethodAsync('OnChartContainerResized', rect.width, rect.height);
        }
    },

    unobserve: function (elementId) {
        if (this.observers.has(elementId)) {
            this.observers.get(elementId).disconnect();
            this.observers.delete(elementId);
        }
    }
};
