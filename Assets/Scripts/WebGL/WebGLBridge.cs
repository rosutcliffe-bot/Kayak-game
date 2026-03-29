using UnityEngine;
using System.Runtime.InteropServices;

namespace KayakSimulator.WebGL
{
    /// <summary>
    /// Bridge between Unity C# code and the browser JavaScript environment.
    /// Provides static helpers for fullscreen, mobile detection, and
    /// filesystem sync.  A MonoBehaviour instance is placed in the scene
    /// so the HTML toolbar can call <c>SendMessage("WebGLBridge", …)</c>.
    /// </summary>
    public class WebGLBridge : MonoBehaviour
    {
        // ---------------------------------------------------------------
        // Singleton (auto-created on first access)
        // ---------------------------------------------------------------
        public static WebGLBridge Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            gameObject.name = "WebGLBridge"; // must match SendMessage target
        }

        // ---------------------------------------------------------------
        // JS function imports (only compiled for WebGL)
        // ---------------------------------------------------------------
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void WebGL_RequestFullscreen();
        [DllImport("__Internal")] private static extern void WebGL_ExitFullscreen();
        [DllImport("__Internal")] private static extern int  WebGL_IsFullscreen();
        [DllImport("__Internal")] private static extern int  WebGL_IsMobileDevice();
        [DllImport("__Internal")] private static extern void WebGL_Alert(string message);
        [DllImport("__Internal")] private static extern void WebGL_OpenURL(string url);
        [DllImport("__Internal")] private static extern void WebGL_SyncFilesystem();
#endif

        // ---------------------------------------------------------------
        // Public API – safe to call on all platforms
        // ---------------------------------------------------------------

        /// <summary>Toggle browser fullscreen.</summary>
        public static void ToggleFullscreen()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (WebGL_IsFullscreen() == 1) WebGL_ExitFullscreen();
            else                           WebGL_RequestFullscreen();
#else
            Screen.fullScreen = !Screen.fullScreen;
#endif
        }

        /// <summary>Returns true when running on a mobile browser.</summary>
        public static bool IsMobile()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return WebGL_IsMobileDevice() == 1;
#else
            return Application.isMobilePlatform;
#endif
        }

        /// <summary>Show a browser alert (or Debug.Log in editor).</summary>
        public static void Alert(string message)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGL_Alert(message);
#else
            Debug.Log($"[WebGLBridge.Alert] {message}");
#endif
        }

        /// <summary>Flush IndexedDB after a PlayerPrefs save.</summary>
        public static void SyncFiles()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGL_SyncFilesystem();
#endif
        }

        // ---------------------------------------------------------------
        // SendMessage targets (called from HTML toolbar JS)
        // ---------------------------------------------------------------

        /// <summary>Called from HTML mute button.  1 = mute, 0 = unmute.</summary>
        public void SetMuted(int muted)
        {
            AudioListener.volume = muted == 1 ? 0f : 1f;
        }
    }
}
