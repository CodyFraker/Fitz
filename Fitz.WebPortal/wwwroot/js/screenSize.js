// Screen size interop for Blazor
window.screenSizeInterop = {
    // Initialize the screen size detection
    initialize: function (dotnetReference) {
        // Store the .NET reference for callbacks
        this.dotnetReference = dotnetReference;
        
        // Get the initial screen size
        const screenSize = this.getScreenSize();
        
        // Call the .NET method with the initial screen size
        this.dotnetReference.invokeMethodAsync('OnScreenSizeChanged', screenSize);
        
        // Add a resize event listener
        window.addEventListener('resize', this.handleResize.bind(this));
    },
    
    // Handle window resize events
    handleResize: function () {
        // Debounce the resize event to avoid too many calls
        if (this.resizeTimeout) {
            clearTimeout(this.resizeTimeout);
        }
        
        this.resizeTimeout = setTimeout(() => {
            const screenSize = this.getScreenSize();
            this.dotnetReference.invokeMethodAsync('OnScreenSizeChanged', screenSize);
        }, 100);
    },
    
    // Get the current screen size
    getScreenSize: function () {
        const width = window.innerWidth;
        
        if (width < 600) {
            return 'ExtraSmall';
        } else if (width < 960) {
            return 'Small';
        } else if (width < 1280) {
            return 'Medium';
        } else if (width < 1920) {
            return 'Large';
        } else {
            return 'ExtraLarge';
        }
    }
}; 