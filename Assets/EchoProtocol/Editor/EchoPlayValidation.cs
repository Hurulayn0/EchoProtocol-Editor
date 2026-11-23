using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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

        private const string Campaign = "EchoValidation.Campaign";
        private static IEnumerator campaignChecks;

        static EchoPlayValidation()
        {
            EditorApplication.update += Tick;
            Application.logMessageReceived += ObserveErrors;
        }

        public static void BatchSmokeChecks()
        {
            EchoProjectTools.CheckTimeline();
            EditorSceneManager.OpenScene(EchoProjectTools.ScenePath);
            SessionState.SetBool(Campaign, false);
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
            if (SessionState.GetBool(Campaign, false)) { TickCampaign(); return; }
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


        public static void BatchCampaignChecks()
        {
            foreach (var scene in EditorBuildSettings.scenes.Where(scene => scene.enabled))
            {
                var loaded = EditorSceneManager.OpenScene(scene.path);
                CheckReferences(loaded);
                ReportHierarchy(loaded);
            }
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
            campaignChecks = null;
            SessionState.SetBool(Campaign, true);
            SessionState.SetBool(Active, true);
            SessionState.SetBool(Started, false);
            SessionState.SetFloat(Deadline, (float)EditorApplication.timeSinceStartup + 60);
            EditorApplication.isPlaying = true;
        }

        private static void TickCampaign()
        {
            try
            {
                if (campaignChecks == null) campaignChecks = CheckCampaign().GetEnumerator();
                if (!campaignChecks.MoveNext()) Finish(true, "Split campaign Play Mode checks passed.");
            }
            catch (Exception ex) { Finish(false, ex.ToString()); }
        }

        private static IEnumerable CheckCampaign()
        {
            yield return null;
            var game = CheckScene("MainMenu", EchoScreen.MainMenu);
            Call(game, "ProcessControls", 0f, new EchoInputFrame { Confirm = true }, true);
            while (SceneManager.GetActiveScene().name != "Level1_ArastirmaLabi") yield return null;
            yield return null;
            game = CheckScene("Level1_ArastirmaLabi", EchoScreen.Playing);
            Call(game, "DismissStoryBriefing");
            var playerSettings = new SerializedObject(Field<EchoPlayer>(game, "player"));
            var health = playerSettings.FindProperty("maxHealth");
            int originalHealth = health.intValue;
            health.intValue = 137;
            playerSettings.ApplyModifiedPropertiesWithoutUndo();
            Call(game, "NewLoop");
            Require(Field<int>(game, "playerHealth") == 137, "Player Operator Inspector max health drives rewind");
            health.intValue = originalHealth;
            playerSettings.ApplyModifiedPropertiesWithoutUndo();
            var carbine = new SerializedObject(Field<EchoWeaponSettings>(game, "weaponSettings"));
            var interval = carbine.FindProperty("fireInterval");
            float originalInterval = interval.floatValue;
            interval.floatValue = .75f;
            carbine.ApplyModifiedPropertiesWithoutUndo();
            Require((bool)Call(game, "TryFirePlayer") && Mathf.Abs(Field<float>(game, "nextPlayerShot") - .75f) < .001f,
                "Pulse Carbine Inspector fire interval drives combat");
            interval.floatValue = originalInterval;
            carbine.ApplyModifiedPropertiesWithoutUndo();
            Call(game, "NewLoop");
            var roomSettings = Field<EchoRoomLayout>(game, "activeRoom");
            float originalLoop = roomSettings.LoopSeconds;
            roomSettings.LoopSeconds = 28;
            Call(game, "AdvancePlayingWorld", 24f);
            Require(Field<float>(game, "clock") == 24, "Room Inspector permits a loop beyond 20 seconds");
            roomSettings.LoopSeconds = originalLoop;
            Call(game, "NewLoop");
            for (int step = 0; step < 8; step++)
                Call(game, "ProcessControls", .05f, new EchoInputFrame { Move = Vector2.up }, true);
            Call(game, "ProcessControls", 0f, new EchoInputFrame { Record = true }, true);
            Require(Field<float>(game, "clock") == 0 && Field<IList>(game, "ghosts").Count == 1,
                "R records an Echo and resets the loop");
            Call(game, "ProcessControls", .1f, new EchoInputFrame(), true);
            Require(Field<float>(game, "clock") > 0 && Field<IList>(game, "ghosts").Count == 1,
                "Echo playback continues in the new loop");
            CheckMaps(game);
            yield return null;
            Call(game, "SetScreen", EchoScreen.LevelComplete);
            Call(game, "ProcessControls", 0f, new EchoInputFrame { Confirm = true }, true);
            while (SceneManager.GetActiveScene().name != "Level2_BakimHatti") yield return null;
            yield return null;
            game = CheckScene("Level2_BakimHatti", EchoScreen.Playing);
            var synchronizationEntry = (EchoSessionSave)Call(game, "SynchronizationCheckpoint");
            synchronizationEntry.Validate();
            Require(synchronizationEntry.Layout == 1 && synchronizationEntry.Stage == 0 && synchronizationEntry.Tapes.Length == 0,
                "Maintenance completion targets synchronization stage one");
            CheckMaps(game);
            yield return null;
            Call(game, "SetScreen", EchoScreen.LevelComplete);
            Call(game, "ProcessControls", 0f, new EchoInputFrame { Confirm = true }, true);
            while (SceneManager.GetActiveScene().name != "Level3_Eszamanlilik") yield return null;
            yield return null;
            game = CheckScene("Level3_Eszamanlilik", EchoScreen.Playing);
            var serviceRoom = Field<EchoRoomLayout>(game, "activeRoom");
            float originalWindow = serviceRoom.ServiceWindowSeconds;
            serviceRoom.ServiceWindowSeconds = 2.75f;
            var label = (string)typeof(EchoPrototypeGame).GetProperty("ServiceWindowText", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game);
            Require(label == "2,75", "Service window text reflects the room Inspector");
            serviceRoom.ServiceWindowSeconds = originalWindow;
            Vector3 originalDoorOffset = serviceRoom.DoorOpenOffset;
            serviceRoom.DoorOpenOffset = Vector3.up * 2.75f;
            Call(game, "LoadSampleStage", 0);
            SetField(game, "sampleServiceUntil", 1f);
            Call(game, "UpdateSampleDoor", .25f);
            Require(Vector3.Distance(serviceRoom.Door.localPosition, Field<Vector3>(game, "closedDoorPosition") + Vector3.up * 2.75f) < .001f,
                "Room door offset drives the physical door");
            serviceRoom.DoorOpenOffset = originalDoorOffset;
            Call(game, "LoadSampleStage", 0);
            Call(game, "SpawnResonanceShockwave", Vector3.zero, serviceRoom.ShockwaveAccent);
            Require(Field<IList>(game, "activeShockwaves").Count == 1 && serviceRoom.ShockwaveAnimation != null,
                "Room EMP prefab is instantiated and has its animation");
            Call(game, "ClearShockwaves");
            CheckMaps(game);
            yield return null;
            for (int stage = 1; stage <= 2; stage++)
            {
                var gate = (Vector3)typeof(EchoPrototypeGame).GetProperty("SampleGatePosition", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(game);
                var player = Field<EchoPlayer>(game, "player");
                player.transform.position = gate + Vector3.forward * 3;
                SetField(game, "doorOpen", true);
                Call(game, "CheckSampleExit");
                yield return null;
                CheckScene("Level3_Eszamanlilik", EchoScreen.Playing);
                CheckMaps(game);
                Require(Field<int>(game, "sampleStage") == stage, "Synchronization stage " + stage);
                if (stage == 1)
                {
                    var room = Field<EchoRoomLayout>(game, "activeRoom");
                    var renderer = room.Sensor.GetComponent<Renderer>();
                    var idle = renderer.sharedMaterial;
                    var active = room.PowerActive;
                    var original = room.SensorActive;
                    var sensorPlayer = Field<EchoPlayer>(game, "player");
                    var previousPose = sensorPlayer.Pose;
                    room.SensorActive = active;
                    var onSensor = previousPose;
                    onSensor.Position = room.SensorArea.transform.TransformPoint(room.SensorArea.center);
                    sensorPlayer.SetPose(onSensor);
                    Call(game, "UpdateSampleDoor", 0f);
                    Require(renderer.sharedMaterial == active, "Room sensor active material drives the held sensor");
                    sensorPlayer.SetPose(previousPose);
                    Call(game, "UpdateSampleDoor", 0f);
                    Require(renderer.sharedMaterial == idle, "Sensor restores its authored Renderer material");
                    room.SensorActive = original;
                }
            }
            var savePath = Path.Combine(Path.GetTempPath(), "EchoProtocol-" + Guid.NewGuid() + ".json");
            try
            {
                var store = new EchoSaveStore(savePath);
                var longTape = new EchoSavedTape
                {
                    Frames = new[]
                    {
                        new EchoSavedFrame { Time = 0, Pose = Field<EchoPose>(game, "spawn") },
                        new EchoSavedFrame { Time = 24, Pose = Field<EchoPose>(game, "spawn") }
                    },
                    Actions = Array.Empty<EchoSavedAction>()
                };
                synchronizationEntry.Tapes = new[] { longTape };
                store.Write(synchronizationEntry);
                Require(store.TryRead(out var longSaved, out _) && longSaved.Tapes[0].Frames[1].Time == 24,
                    "Echo recordings beyond 20 seconds survive a disk roundtrip");
                synchronizationEntry.Tapes = Array.Empty<EchoSavedTape>();
                store.Clear();
                var legacy = new EchoSaveStore(savePath + ".legacy");
                legacy.Write(new EchoSessionSave { Version = 1, Layout = 0, Level = 2, Loops = 3, Tapes = Array.Empty<EchoSavedTape>() });
                Require(store.ImportLegacyIfMissing(legacy), "Version 1 migration runs");
                Require(
                    store.TryRead(out EchoSessionSave migrated, out _) && migrated.Level == 2 && migrated.Loops == 3,
                    "Version 1 checkpoint migrates with progress intact");
                store.Write((EchoSessionSave)Call(game, "CaptureSession"));
                Require(store.TryRead(out EchoSessionSave saved, out _) && saved.Stage == 2, "Synchronization checkpoint roundtrip");
                Call(game, "ReturnToMenu");
                while (SceneManager.GetActiveScene().name != "MainMenu") yield return null;
                yield return null;
                game = CheckScene("MainMenu", EchoScreen.MainMenu);
                Field<EchoPreferences>(game, "preferences").ResumeLevel = saved.Level;
                SetField(game, "resumeSave", saved);
                Call(game, "ContinueSavedCampaign");
                while (SceneManager.GetActiveScene().name != "Level3_Eszamanlilik") yield return null;
                yield return null;
                game = CheckScene("Level3_Eszamanlilik", EchoScreen.Playing);
                Require(Field<int>(game, "sampleStage") == 2, "Continue loads the saved synchronization stage");
                Call(game, "SetScreen", EchoScreen.GameWon);
                Call(game, "ProcessControls", 0f, new EchoInputFrame { Confirm = true }, true);
                while (SceneManager.GetActiveScene().name != "Level4_Reaktor") yield return null;
                yield return null;
                game = CheckScene("Level4_Reaktor", EchoScreen.Playing);
                CheckMaps(game);
                Call(game, "ReturnToMenu");
                while (SceneManager.GetActiveScene().name != "MainMenu") yield return null;
                yield return null;
                game = CheckScene("MainMenu", EchoScreen.MainMenu);
                Field<EchoPreferences>(game, "preferences").ResumeLevel = migrated.Level;
                SetField(game, "resumeSave", migrated);
                Call(game, "ContinueSavedCampaign");
                while (SceneManager.GetActiveScene().name != "Level4_Reaktor") yield return null;
                yield return null;
                game = CheckScene("Level4_Reaktor", EchoScreen.Playing);
                Require(Field<int>(game, "savedLoops") == 3, "Continue restores the version 1 checkpoint");
                store.Write(synchronizationEntry);
                Require(store.TryRead(out var nextChapter, out _), "Maintenance exit checkpoint is readable from disk");
                Call(game, "ReturnToMenu");
                while (SceneManager.GetActiveScene().name != "MainMenu") yield return null;
                yield return null;
                game = CheckScene("MainMenu", EchoScreen.MainMenu);
                Field<EchoPreferences>(game, "preferences").ResumeLevel = nextChapter.Level;
                SetField(game, "resumeSave", nextChapter);
                Call(game, "ContinueSavedCampaign");
                while (SceneManager.GetActiveScene().name != "Level3_Eszamanlilik") yield return null;
                yield return null;
                game = CheckScene("Level3_Eszamanlilik", EchoScreen.Playing);
                Require(Field<int>(game, "sampleStage") == 0, "Continue after maintenance opens synchronization stage one");
            }
            finally
            {
                new EchoSaveStore(savePath).Clear();
                new EchoSaveStore(savePath + ".legacy").Clear();
            }
        }

        private static void ReportHierarchy(Scene scene)
        {
            var roots = scene.GetRootGameObjects();
            var rooms = roots.SelectMany(root => root.GetComponentsInChildren<EchoRoomLayout>(true)).ToArray();
            var roomNames = rooms.Select(room => room.name).OrderBy(name => name).ToArray();
            var canvas = roots.Single(root => root.GetComponent<EchoCanvasBindings>() != null);
            Require(PrefabUtility.IsPartOfPrefabInstance(canvas), "Shared Canvas prefab is linked: " + scene.name);
            foreach (var button in canvas.GetComponentsInChildren<UnityEngine.UI.Button>(true))
                for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                    if (!string.IsNullOrEmpty(button.onClick.GetPersistentMethodName(i)) && button.onClick.GetPersistentTarget(i) == null)
                        throw new InvalidOperationException("Missing button target: " + button.name);
            string[] expected;
            switch (scene.name)
            {
                case "MainMenu": expected = new[] { "Menu Backdrop" }; break;
                case "Level1_ArastirmaLabi": expected = new[] { "Sector 1" }; break;
                case "Level2_BakimHatti": expected = new[] { "Sector 2" }; break;
                case "Level3_Eszamanlilik": expected = new[] { "Maintenance 1", "Maintenance 2", "Maintenance 3" }; break;
                case "Level4_Reaktor": expected = new[] { "Sector 3" }; break;
                default: throw new InvalidOperationException(scene.name);
            }
            Require(roomNames.SequenceEqual(expected), "Only the authored chapter rooms are present: " + scene.name);
            string hierarchy = string.Join(", ", roots.Select(root =>
                $"{root.name}[{root.GetComponentsInChildren<Transform>(true).Length}]"));
            Debug.Log($"ECHO HIERARCHY: {scene.name} | {roots.Sum(root => root.GetComponentsInChildren<Transform>(true).Length)} | {hierarchy} | {string.Join(", ", roomNames)}");
        }

        private static void CheckReferences(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            foreach (var component in root.GetComponentsInChildren<Component>(true))
            {
                if (component == null) throw new InvalidOperationException("Missing script in " + root.name);
                if (!(component is MonoBehaviour) && !(component is AudioSource) && !(component is Animator)) continue;
                var property = new SerializedObject(component).GetIterator();
                while (property.Next(true))
                {
                    if (property.propertyType == SerializedPropertyType.ObjectReference &&
                        property.objectReferenceValue == null && property.objectReferenceInstanceIDValue != 0)
                        throw new InvalidOperationException($"Missing reference: {component.name}/{property.propertyPath}.");
                }
            }
        }

        private static EchoPrototypeGame CheckScene(string name, EchoScreen screen)
        {
            Require(SceneManager.GetActiveScene().name == name, "Scene loaded: " + name);
            var games = UnityEngine.Object.FindObjectsOfType<EchoPrototypeGame>();
            Require(games.Length == 1 &&
                UnityEngine.Object.FindObjectsOfType<EchoPlayer>().Length == 1 &&
                UnityEngine.Object.FindObjectsOfType<AudioListener>().Length == 1 &&
                UnityEngine.Object.FindObjectsOfType<EventSystem>().Length == 1, "Single manager, player, audio listener and event system: " + name);
            var game = games[0];
            Require(Field<EchoScreen>(game, "screen") == screen, "Screen mode: " + name);
            if (screen == EchoScreen.Playing)
            {
                var spawn = Field<Transform>(game, "roomPlayerSpawn");
                var expected = Field<bool>(game, "sampleActive") && spawn != null ? spawn.position : Field<EchoPose>(game, "spawn").Position;
                Require(Vector3.Distance(expected, Field<EchoPlayer>(game, "player").transform.position) < .15f,
                    "Authored spawn: " + name);
                Require(Field<Transform>(game, "weaponRoot").gameObject.activeInHierarchy, "Weapon is active: " + name);
                Require(Field<AudioSource>(Field<EchoWeaponSettings>(game, "weaponSettings"), "weaponFireAudio").clip != null, "Weapon fire audio is assigned: " + name);
                Require(Field<AudioSource>(Field<EchoAudioBindings>(game, "audioBindings"), "gameMusicSource").clip != null, "Game music is assigned: " + name);
                Require(Field<IList>(game, "enemies").Count == (name == "Level1_ArastirmaLabi" ? 0 : name == "Level4_Reaktor" ? 2 : 1), "Authored enemy count: " + name);
            }
            return game;
        }

        private static void CheckMaps(EchoPrototypeGame game)
        {
            Call(game, "DismissStoryBriefing");
            Call(game, "LateUpdate");
            var minimap = Field<GameObject>(Field<EchoCanvasBindings>(game, "canvasBindings"), "minimapPanel");
            if (Field<bool>(game, "planning"))
                Call(game, "ProcessControls", 0f, new EchoInputFrame { Plan = true, Confirm = Field<bool>(game, "echoTeaching") }, true);
            Call(game, "LateUpdate");
            Canvas.ForceUpdateCanvases();
            var rect = (RectTransform)minimap.transform;
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            var canvas = minimap.GetComponentInParent<Canvas>();
            Require(Field<Mesh>(game, "minimapMesh").vertexCount > 0, "Minimap geometry: " + game.gameObject.scene.name);
            Require(minimap.activeInHierarchy && canvas != null && canvas.enabled &&
                canvas.pixelRect.Contains(RectTransformUtility.WorldToScreenPoint(null, corners[0])) &&
                canvas.pixelRect.Contains(RectTransformUtility.WorldToScreenPoint(null, corners[2])),
                "Minimap is active and on screen: " + game.gameObject.scene.name);
            Call(game, "ProcessControls", 0f, new EchoInputFrame { Plan = true }, true);
            Call(game, "LateUpdate");
            Require(!minimap.activeInHierarchy &&
                (Field<GameObject>(Field<EchoCanvasBindings>(game, "canvasBindings"), "campaignPlan").activeInHierarchy || Field<GameObject>(Field<EchoCanvasBindings>(game, "canvasBindings"), "maintenancePlan").activeInHierarchy),
                "Tab swaps the minimap for the planning Canvas: " + game.gameObject.scene.name);
            Call(game, "ProcessControls", 0f, new EchoInputFrame { Plan = true }, true);
        }

        private static FieldInfo FindField(string name) =>
            typeof(EchoPrototypeGame).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        private static T Field<T>(EchoPrototypeGame game, string name) => (T)FindField(name).GetValue(game);
        private static T Field<T>(MonoBehaviour owner, string name) => (T)owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
        private static void SetField(EchoPrototypeGame game, string name, object value) => FindField(name).SetValue(game, value);
        private static object Call(EchoPrototypeGame game, string name, params object[] arguments) =>
            typeof(EchoPrototypeGame).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(game, arguments);

        private static void Require(bool condition, string check)
        {
            if (!condition) throw new InvalidOperationException(check);
            Debug.Log("ECHO CAMPAIGN PASS: " + check);
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
            if (campaignChecks is IDisposable pending) pending.Dispose();
            campaignChecks = null;
            Directory.CreateDirectory(Output);
            File.WriteAllText(Path.Combine(Output, SessionState.GetBool(Campaign, false) ? "campaign-result.txt" : "playmode-result.txt"),
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

        public static void BatchBuildPrototype() => BuildWindows(includeValidation: true);

        public static void BatchBuildCampaign() => BuildWindows(includeValidation: false);

        private static void BuildWindows(bool includeValidation)
        {
            EchoProjectTools.ConfigureBuild();
            EnsureShaderResources();
            PlayerSettings.productName = "Echo Protocol";
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled || includeValidation && scene.path == EchoProjectTools.ScenePath).Select(scene => scene.path).ToArray(),
                locationPathName = includeValidation ? "Builds/EchoProtocol/EchoProtocol.exe" : "Builds/Campaign/EchoProtocol.exe",
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
