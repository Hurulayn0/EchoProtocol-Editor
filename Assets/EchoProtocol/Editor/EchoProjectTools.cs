using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoProtocol.Editor
{
    public static class EchoProjectTools
    {
        public const string ScenePath = "Assets/Scenes/EchoPrototype.unity";

        [MenuItem("Echo Protocol/Open Prototype")]
        public static void OpenPrototype()
        {
            if (EditorApplication.isPlaying) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists(ScenePath)) CreateScene();
            EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("Echo Protocol/Run Timeline Checks")]
        public static void CheckTimeline()
        {
            try { Debug.Log("ECHO TIMELINE PASS: " + TimelineSelfChecks.Run()); }
            catch (Exception ex) { Debug.LogException(ex); throw; }
        }

        // Suitable for a licensed Unity installation's -batchmode -executeMethod.
        public static void BatchTimelineChecks()
        {
            try { CheckTimeline(); EditorApplication.Exit(0); }
            catch { EditorApplication.Exit(1); }
        }

        [MenuItem("Echo Protocol/Configure Build Scene")]
        public static void ConfigureBuild()
        {
            if (!File.Exists(ScenePath)) CreateScene();
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Debug.Log("EchoPrototype is configured as the build scene.");
        }

        private static void CreateScene()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Scene previous = SceneManager.GetActiveScene();
            Scene created = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            EditorSceneManager.SaveScene(created, ScenePath);
            EditorSceneManager.CloseScene(created, true);
            if (previous.IsValid()) SceneManager.SetActiveScene(previous);
            AssetDatabase.Refresh();
        }
    }
}
