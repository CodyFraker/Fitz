window.screenSizeInterop = {
    dotNetHelper: null,
    
    initialize: function (dotNetHelper) {
        this.dotNetHelper = dotNetHelper;
        
        // Add event listener for window resize
        window.addEventListener('resize', this.handleResize.bind(this));
        
        // Initial call to set the screen size
        this.handleResize();
    },
    
    handleResize: function () {
        const screenSize = this.getScreenSize();
        if (this.dotNetHelper) {
            this.dotNetHelper.invokeMethodAsync('OnScreenSizeChanged', screenSize);
        }
    },
    
    getScreenSize: function () {
        const width = window.innerWidth;
        
        if (width < 600) {
            return "ExtraSmall";
        } else if (width < 960) {
            return "Small";
        } else if (width < 1280) {
            return "Medium";
        } else if (width < 1920) {
            return "Large";
        } else {
            return "ExtraLarge";
        }
    }
}; 