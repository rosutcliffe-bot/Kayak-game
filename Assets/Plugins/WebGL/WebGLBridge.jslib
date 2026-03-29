mergeInto(LibraryManager.library, {

    /**
     * Request the browser to enter fullscreen mode.
     */
    WebGL_RequestFullscreen: function () {
        var elem = document.documentElement;
        if (elem.requestFullscreen)            elem.requestFullscreen();
        else if (elem.webkitRequestFullscreen) elem.webkitRequestFullscreen();
        else if (elem.msRequestFullscreen)     elem.msRequestFullscreen();
    },

    /**
     * Exit fullscreen mode.
     */
    WebGL_ExitFullscreen: function () {
        if (document.exitFullscreen)            document.exitFullscreen();
        else if (document.webkitExitFullscreen) document.webkitExitFullscreen();
        else if (document.msExitFullscreen)     document.msExitFullscreen();
    },

    /**
     * Returns 1 if the document is currently in fullscreen, 0 otherwise.
     */
    WebGL_IsFullscreen: function () {
        return document.fullscreenElement ? 1 : 0;
    },

    /**
     * Returns 1 if the page is being viewed on a mobile / touch device.
     */
    WebGL_IsMobileDevice: function () {
        return /Android|iPhone|iPad|iPod|Opera Mini|IEMobile|WPDesktop/i.test(navigator.userAgent) ? 1 : 0;
    },

    /**
     * Show a browser alert dialog with the supplied message.
     * Expects a C-string pointer.
     */
    WebGL_Alert: function (messagePtr) {
        window.alert(UTF8ToString(messagePtr));
    },

    /**
     * Open a URL in a new tab.
     * Expects a C-string pointer.
     */
    WebGL_OpenURL: function (urlPtr) {
        window.open(UTF8ToString(urlPtr), '_blank');
    },

    /**
     * Sync persistent data (IndexedDB).  Call after saving PlayerPrefs.
     */
    WebGL_SyncFilesystem: function () {
        if (typeof FS !== 'undefined' && typeof FS.syncfs === 'function') {
            FS.syncfs(false, function (err) {
                if (err) console.warn('[WebGLBridge] FS.syncfs error:', err);
            });
        }
    }
});
