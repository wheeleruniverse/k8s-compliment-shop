// Google Analytics 4 Integration

window.analyticsHelper = {
    initialized: false,
    measurementId: null,

    // Initialize GA4 with measurement ID
    initialize: function (measurementId) {
        if (this.initialized) {
            console.log('Analytics already initialized');
            return;
        }

        this.measurementId = measurementId;

        // Load gtag.js script
        const script = document.createElement('script');
        script.async = true;
        script.src = `https://www.googletagmanager.com/gtag/js?id=${measurementId}`;
        document.head.appendChild(script);

        // Initialize dataLayer and gtag function
        window.dataLayer = window.dataLayer || [];
        function gtag() { dataLayer.push(arguments); }
        window.gtag = gtag;

        gtag('js', new Date());
        gtag('config', measurementId);

        this.initialized = true;
        console.log(`Analytics initialized with ID: ${measurementId}`);
    },

    // Track page view
    trackPageView: function (pageTitle, pagePath) {
        if (!this.initialized) {
            console.warn('Analytics not initialized');
            return;
        }

        window.gtag('event', 'page_view', {
            page_title: pageTitle,
            page_path: pagePath
        });

        console.log(`Page view tracked: ${pageTitle} (${pagePath})`);
    },

    // Track product view
    trackProductView: function (productId, productName, productCategory) {
        if (!this.initialized) {
            console.warn('Analytics not initialized');
            return;
        }

        window.gtag('event', 'view_item', {
            items: [{
                item_id: productId.toString(),
                item_name: productName,
                item_category: productCategory
            }]
        });

        console.log(`Product view tracked: ${productName} (ID: ${productId})`);
    },

    // Track custom event
    trackEvent: function (eventName, eventParams) {
        if (!this.initialized) {
            console.warn('Analytics not initialized');
            return;
        }

        window.gtag('event', eventName, eventParams);
        console.log(`Event tracked: ${eventName}`, eventParams);
    }
};
