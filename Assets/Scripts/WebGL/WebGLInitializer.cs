using UnityEngine;

namespace KayakSimulator.WebGL
{
    /// <summary>
    /// Bootstraps the WebGL bridge on scene load.
    /// Attach this to the Managers GameObject in the MainMenu scene so
    /// <see cref="WebGLBridge"/> is available before any other system
    /// tries to use it.  On non-WebGL platforms the component is a no-op.
    /// </summary>
    public class WebGLInitializer : MonoBehaviour
    {
        private void Awake()
        {
            // Create the bridge singleton if it does not already exist
            if (WebGLBridge.Instance == null)
            {
                var go = new GameObject("WebGLBridge");
                go.AddComponent<WebGLBridge>();
            }

            // On WebGL, disable the hardware cursor lock warning
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput = true;
#endif
        }
    }
}
