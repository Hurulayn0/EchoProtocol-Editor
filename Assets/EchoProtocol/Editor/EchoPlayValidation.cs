using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EchoProtocol.Editor
{
    // Runs the exact F6 entry point in a real Editor Play Mode session.
    // SessionState survives the assembly reload when Play Mode starts.
    [InitializeOnLoad]
    public static class EchoPlayValidation
    {
        private const string Active = "EchoValidation.Active";
        private const string Started = "EchoValidation.Started";
        private const string Deadline = "EchoValidation.Deadline";
        private const string Output = "Verification/Logs";

        static EchoPlayValidation()
        {
            EditorApplication.update += Tick;
            Application.logMessageReceived += ObserveErrors;
        }

        public static void BatchSmokeChecks()
        {
            EchoProjectTools.CheckTimeline();
            EditorSceneManager.OpenScene(EchoProjectTools.ScenePath);
            // Reserialize the existing scene with the installed editor after a successful load.
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            SessionState.SetBool(Active, true);
            SessionState.SetBool(Started, false);
            SessionState.SetFloat(Deadline, (float)EditorApplication.timeSinceStartup + 300);
            EditorApplication.isPlaying = true;
        }

        private static void Tick()
        {
            if (!SessionState.GetBool(Active, false)) return;
            if (EditorApplication.timeSinceStartup > SessionState.GetFloat(Deadline, 0))
            {
                Finish(false, "Play Mode validation timed out.");
                return;
            }
            if (!EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode && !Application.isPlaying) return;
            EchoPrototypeGame game = UnityEngine.Object.FindObjectOfType<EchoPrototypeGame>();
            if (game == null) return;
            if (!SessionState.GetBool(Started, false))
            {
                SessionState.SetBool(Started, true);
                game.SmokeCheckpoint += CaptureCheckpoint;
                game.StartSmokeChecks();
            }
            if (game.SmokeTestFinished) Finish(game.SmokeTestPassed, game.SmokeTestSummary + "\n" + game.SmokeTestDetails);
        }

        private static void ObserveErrors(string message, string trace, LogType type)
        {
            if (!SessionState.GetBool(Active, false)) return;
            if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
                Finish(false, message + "\n" + trace);
        }

        private static void Finish(bool passed, string details)
        {
            SessionState.SetBool(Active, false);
            Directory.CreateDirectory(Output);
            File.WriteAllText(Path.Combine(Output, "playmode-result.txt"),
                "Unity " + Application.unityVersion + "\n" +
                (passed ? "PASS" : "FAIL") + "\n" + details + "\n");
            Debug.Log("ECHO PLAY VALIDATION " + (passed ? "PASS: " : "FAIL: ") + details);
            EditorApplication.Exit(passed ? 0 : 1);
        }

        private static void CaptureCheckpoint(string name)
        {
            Camera source = Camera.main;
            if (source == null) throw new InvalidOperationException("No game camera for verification capture.");
            GameObject captureObject = new GameObject("Verification camera");
            Camera camera = captureObject.AddComponent<Camera>();
            camera.CopyFrom(source);
            camera.enabled = false;
            if (name.Contains("echo-open-gate") || name.Contains("dual-echo"))
            {
                camera.transform.position = new Vector3(0, 3.4f, -7.5f);
                camera.transform.LookAt(new Vector3(0, .9f, 1));
                camera.cullingMask &= ~(1 << 2);
            }
            else camera.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
            RenderTexture target = new RenderTexture(1280, 720, 24);
            Texture2D pixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            RenderTexture previous = RenderTexture.active;
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                pixels.Apply();
                Directory.CreateDirectory(Output);
                File.WriteAllBytes(Path.Combine(Output, name + ".png"), pixels.EncodeToPNG());
                Debug.Log("ECHO CAPTURE: " + name);
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                target.Release();
                UnityEngine.Object.DestroyImmediate(pixels);
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(captureObject);
            }
        }

        public static void BatchBuildPrototype()
        {
            EchoProjectTools.ConfigureBuild();
            EnsureShaderResources();
            PlayerSettings.productName = "Echo Protocol";
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { EchoProjectTools.ScenePath },
                locationPathName = "Builds/EchoProtocol/EchoProtocol.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            bool passed = report.summary.result == BuildResult.Succeeded;
            Directory.CreateDirectory(Output);
            File.WriteAllText(Path.Combine(Output, "build-result.txt"), report.summary.result +
                "\nErrors: " + report.summary.totalErrors + "\nWarnings: " + report.summary.totalWarnings + "\n");
            Debug.Log("ECHO BUILD " + report.summary.result);
            EditorApplication.Exit(passed ? 0 : 1);
        }

        private static void EnsureShaderResources()
        {
            const string folder = "Assets/EchoProtocol/Resources/EchoShaders";
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
            Shader shader = Shader.Find("Standard");
            if (shader == null) throw new InvalidOperationException("Built-in Standard shader is missing in the Editor.");
            foreach (string name in new[] { "Default", "Glow" })
            {
                string path = folder + "/" + name + ".mat";
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    material = new Material(shader);
                    AssetDatabase.CreateAsset(material, path);
                }
                material.shader = shader;
                if (name == "Glow")
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", Color.white);
                }
                EditorUtility.SetDirty(material);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
