using UnityEditor;
using UnityEngine;
using System.IO;

namespace KayakSimulator.Editor
{
    /// <summary>
    /// One-click WebGL build script.
    /// Run from <b>Kayak Simulator → Build WebGL</b> or from the command line:
    /// <code>
    /// Unity -batchmode -projectPath . -executeMethod KayakSimulator.Editor.WebGLBuilder.Build -quit
    /// </code>
    /// </summary>
    public static class WebGLBuilder
    {
        private const string OutputDir = "WebGLBuild";

        [MenuItem("Kayak Simulator/Build WebGL", priority = 100)]
        public static void Build()
        {
            // Ensure output directory exists
            if (!Directory.Exists(OutputDir))
                Directory.CreateDirectory(OutputDir);

            // Collect scenes that are enabled in Build Settings
            var scenes = new System.Collections.Generic.List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    scenes.Add(scene.path);
            }

            if (scenes.Count == 0)
            {
                Debug.LogError("[WebGLBuilder] No scenes are listed in Build Settings. " +
                    "Add MainMenu and GameScene before building.");
                return;
            }

            // Configure WebGL player settings
            PlayerSettings.WebGL.template = "PROJECT:KayakSimulator";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
            PlayerSettings.WebGL.memorySize = 256;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;

            var options = new BuildPlayerOptions
            {
                scenes = scenes.ToArray(),
                locationPathName = OutputDir,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
                Debug.Log($"[WebGLBuilder] Build succeeded → {Path.GetFullPath(OutputDir)}");
            else
                Debug.LogError("[WebGLBuilder] Build failed. See Console for details.");
        }
    }
}
