using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoProtocol
{
    public enum EchoScreen { MainMenu, Controls, Settings, Playing, Paused, LevelComplete, GameWon, GameLost, Archives }

    // The validated recorder/reset/gate prototype, extended into a three-room campaign.
    public sealed partial class EchoPrototypeGame : MonoBehaviour
    {
        public const float LoopLength = 20;
        [SerializeField, Range(1, 2)] private int maxEchoes = 2;
        [SerializeField, Range(.02f, .25f)] private float recordingInterval = .05f;
        private int MaxEchoes => Mathf.Clamp(maxEchoes, 1, 2);
        [SerializeField] private EchoReplayActor[] replayActors;
        private readonly List<ReplayTimeline<EchoPose, EchoAction>> recordings = new List<ReplayTimeline<EchoPose, EchoAction>>();
        private readonly List<Ghost> ghosts = new List<Ghost>();
        private readonly List<EchoEnemy> enemies = new List<EchoEnemy>();
        private readonly List<Vector3> decoyPositions = new List<Vector3>();
        private readonly List<ConsoleState> consoles = new List<ConsoleState>();
        private ReplayTimeline<EchoPose, EchoAction> current;
        private EchoPlayer player;
        private EchoPose spawn = new EchoPose { Position = new Vector3(0, .02f, -7), Rotation = Quaternion.identity };
        private Transform worldRoot, door;
        private Collider doorCollider;
        private float clock, nextSample, doorFraction, elapsedTotal;
        private bool power, plateHeld, doorOpen, paused = true, won, testing;
        private int MaxPlayerHealth => player.MaxHealth;
        private int levelIndex, playerHealth, echoActivations, echoShots, savedLoops;
        private EchoScreen screen = EchoScreen.MainMenu;
        private string message = "Geçmişin, takım arkadaşın.";
        private string testResult = "F6 / otomatik oyun testi";
        private float notificationRemaining;
        private readonly EchoPreferences preferences = new EchoPreferences();
        private EchoScreen settingsReturn = EchoScreen.MainMenu;
        private bool automatedRun;
        public bool SmokeTestFinished { get; private set; }
        public bool SmokeTestPassed { get; private set; }
        public string SmokeTestDetails { get; private set; }
        public int SmokeTestCount { get; private set; }
        public string SmokeTestSummary { get; private set; }
        public event Action<string> SmokeCheckpoint;
        private EchoLevelDefinition Level { get { return EchoLevelDefinition.Campaign[levelIndex]; } }

        private sealed class ConsoleState
        {
            public int Id;
            public Vector3 Position;
            public BoxCollider PlateArea;
            public Vector3 Plate
            {
                get
                {
                    Vector3 center = PlateArea.transform.TransformPoint(PlateArea.center);
                    center.y = PlateVisual.position.y + PlateFloorOffset;
                    return center;
                }
            }
            public float PlateFloorOffset;
            public Transform Visual, PlateVisual;
            public bool Powered, Held;
        }

        private sealed class Ghost
        {
            public GameObject Body;
            public Transform Head;
            public EchoRobotMotion Motion;
            public ReplayTimeline<EchoPose, EchoAction> Tape;
            public ReplayTimeline<EchoPose, EchoAction>.Cursor Cursor;
            public int ConsoleMask;
            public EchoPose FinalPose;
            public float SensorStart = -1, SensorEnd = -1;
            public LineRenderer ShotPreview;
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            automatedRun = Array.IndexOf(Environment.GetCommandLineArgs(), "-echo-smoke-test") >= 0;
            preferences.Load(authoredPlayer.LookSensitivity, authoredPlayer.View.fieldOfView);
            InitializeSession();
            authoredRoomDisplay.SetActive(false);
            player = authoredPlayer;
            player.CaptureSceneSettings();
            spawn = player.Pose;
            weaponRoot = authoredWeapon;
            weaponSettings = weaponRoot.GetComponent<EchoWeaponSettings>();
            weaponMotion = weaponSettings.Motion;
            motionRestPosition = weaponMotion.localPosition;
            motionRestRotation = weaponMotion.localRotation;
            muzzle = weaponSettings.Muzzle;
            CreateImmersion();
            CreateAudioBank();
            ApplyPreferences();
            InitializeSceneEntry();
        }

        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (sceneEntry != LegacyScene && automatedRun)
            {
                SceneManager.LoadScene(ValidationScene);
                yield break;
            }
            if (runValidationOnEntry)
            {
                runValidationOnEntry = false;
                yield return null;
                StartSmokeChecks();
                yield break;
            }
            if (!automatedRun) yield break;
            Application.runInBackground = true;
            int captureIndex = Array.IndexOf(args, "-echo-capture");
            string capturePath = captureIndex >= 0 && captureIndex + 1 < args.Length ? args[captureIndex + 1] : null;
            if (capturePath != null)
                SmokeCheckpoint += checkpoint => ScreenCapture.CaptureScreenshot(capturePath + "-" + checkpoint + ".png");
            yield return null;
            StartSmokeChecks();
            float deadline = Time.realtimeSinceStartup + 180;
            while (!SmokeTestFinished && Time.realtimeSinceStartup < deadline) yield return null;
            bool passed = SmokeTestFinished && SmokeTestPassed;
            Debug.Log("ECHO STANDALONE " + (passed ? "PASS: " : "FAIL: ") + SmokeTestDetails);
            if (capturePath != null && SmokeTestFinished)
            {
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(capturePath + "-result.png");
                yield return new WaitForSeconds(.2f);
            }
            Application.Quit(passed ? 0 : 1);
        }

        private void Update()
        {
            RefreshCanvasMenus();
            if (testing) return;
            float dt = Mathf.Min(Time.deltaTime, .1f);
            TickAudioMusic(dt);
            TickMenuShowcase(dt);
            ProcessControls(dt, EchoInput.Read(), Cursor.lockState == CursorLockMode.Locked);
        }

        private void ProcessControls(float dt, EchoInputFrame input, bool canPlay = true)
        {
            if (input.Test) { OpenValidation(); return; }
            if (screen == EchoScreen.Playing && storyBriefingActive)
            {
                if (input.Confirm || input.Pause || Input.GetKeyDown(KeyCode.Space)) DismissStoryBriefing();
                return;
            }
            if (input.Confirm)
            {
                if (sampleActive && screen == EchoScreen.Playing && planning)
                {
                    if (echoTeaching) FinishEchoTeaching();
                    return;
                }
                if (screen == EchoScreen.GameWon && sampleActive) ContinueToReactor();
                else if (screen == EchoScreen.MainMenu || screen == EchoScreen.GameWon) BeginCampaign();
                else if (screen == EchoScreen.GameLost) RetryLevel();
                else if (screen == EchoScreen.LevelComplete) ContinueCampaign();
                return;
            }
            if (input.Pause)
            {
                if (creditsOverlayActive) { creditsOverlayActive = false; return; }
                if (screen == EchoScreen.Playing) SetScreen(EchoScreen.Paused);
                else if (screen == EchoScreen.Paused) SetScreen(EchoScreen.Playing);
                else if (screen == EchoScreen.Controls || screen == EchoScreen.Archives) SetScreen(EchoScreen.MainMenu);
                else if (screen == EchoScreen.Settings) CloseSettings();
                return;
            }
            if (screen != EchoScreen.Playing) return;
            if (input.Plan) { if (sampleActive && pendingEchoRecord) CancelEchoRecordReview(); else TogglePlanning(); return; }
            if (input.Retry && !echoTeaching) { RetryLevel(); return; }
            if (planning) return;
            if (!canPlay) return;
            if (input.Clear) { ClearRun(); return; }
            if (input.Undo) { UndoLastEcho(); return; }
            if (input.Record)
            {
                if (sampleActive && recordings.Count == MaxEchoes && replacementEchoSlot < 0 && clock >= .25f)
                    OpenEchoRecordReview();
                else CommitLoop(replacementEchoSlot);
                return;
            }
            player.Tick(dt, input);
            AdvancePlayingWorld(dt);
            if (screen != EchoScreen.Playing) return;
            if (input.Interact) Interact();
            if (input.Fire) TryFirePlayer();
            UpdateDoor(dt);
            CheckExit();
            FinishExpiredLoop();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!testing) HandleFocus(focused);
        }

        private void HandleFocus(bool focused)
        {
            if (!focused && screen == EchoScreen.Playing) SetScreen(EchoScreen.Paused);
        }

        private void SetScreen(EchoScreen value)
        {
            EchoScreen previous = screen;
            if (value == EchoScreen.Paused) SaveSession();
            planning = false;
            echoTeaching = false; pendingEchoRecord = false;
            if (value != EchoScreen.Playing) storyBriefingActive = false;
            screen = value;
            paused = value != EchoScreen.Playing;
            won = value == EchoScreen.LevelComplete || value == EchoScreen.GameWon;
            if (value == EchoScreen.Playing) RestorePlayerViewFromShowcase();
            SetCursor(value == EchoScreen.Playing && !testing);
            if (value == EchoScreen.Playing && pendingStoryBriefing && IsLiveDifficulty) OpenStoryBriefing();
            if (previous != value)
            {
                if (value == EchoScreen.LevelComplete || value == EchoScreen.GameWon) PlayUiSound(checkpointWinAudio, 0.72f);
                else if (value == EchoScreen.GameLost) PlayUiSound(loopFailAudio, 0.72f);
            }
            TickImmersion();
            TickAudioMusic(0);
        }

        private void BeginCampaign()
        {
            elapsedTotal = 0; savedLoops = 0;
            if (IsLiveDifficulty) { preferences.ArchiveMask = 0; preferences.SampleMemory = false; }
            if (CanPersist) preferences.Save();
            if (ChangeScene(LaboratoryScene, saveOnArrival: true)) return;
            LoadLevel(0); SetScreen(EchoScreen.Playing);
            SaveCheckpoint(0); SaveSession();
        }

        private void ContinueSavedCampaign()
        {
            if (preferences.ResumeLevel < 0) return;
            if (resumeSave != null && resumeSave.Level == preferences.ResumeLevel)
            {
                int target = resumeSave.Layout == 1 ? SynchronizationScene : CampaignScene(resumeSave.Level);
                if (!ChangeScene(target, resumeSave)) RestoreSession(resumeSave);
                return;
            }
            elapsedTotal = 0; savedLoops = 0;
            if (ChangeScene(CampaignScene(preferences.ResumeLevel), saveOnArrival: true)) return;
            LoadLevel(preferences.ResumeLevel); SetScreen(EchoScreen.Playing); SaveSession();
        }

        private void ApplyPreferences()
        {
            player.LookSensitivity = preferences.Sensitivity;
            player.InvertLook = preferences.InvertLook;
            player.BaseFieldOfView = preferences.FieldOfView;
            player.View.fieldOfView = preferences.FieldOfView;
            TickImmersion();
        }

        private void SaveCheckpoint(int nextLevel)
        {
            if (testing || automatedRun || Application.isBatchMode) return;
            resumeSave = null;
            try { if (saveStore != null) saveStore.Clear(); }
            catch (Exception ex) when (ex is System.IO.IOException || ex is UnauthorizedAccessException) { Debug.LogWarning("ECHO SAVE: " + ex.Message); }
            preferences.ResumeLevel = nextLevel;
            if (nextLevel < 0) preferences.Completed = true;
            preferences.Save();
        }

        private void OpenSettings()
        {
            settingsReturn = screen == EchoScreen.Paused ? EchoScreen.Paused : EchoScreen.MainMenu;
            SetScreen(EchoScreen.Settings);
        }

        private void CloseSettings()
        {
            ApplyPreferences();
            if (CanPersist) { SaveSession(); preferences.Save(); }
            SetScreen(settingsReturn);
        }

        private void RetryLevel()
        {
            NewLoop(); SetScreen(EchoScreen.Playing); SaveSession();
            Notify("Echo kayıtların korundu. Yeni döngüde tekrar dene.");
        }

        private void ContinueCampaign()
        {
            if (screen != EchoScreen.LevelComplete) return;
            if (!testing && levelIndex == 1) { BeginSample(); return; }
            if (ChangeScene(CampaignScene(levelIndex + 1), saveOnArrival: true)) return;
            LoadLevel(levelIndex + 1); SetScreen(EchoScreen.Playing); SaveSession();
        }

        private void ReturnToMenu()
        {
            SaveSession();
            if (ChangeScene(MenuScene)) return;
            LoadLevel(0); SetScreen(EchoScreen.MainMenu);
            message = "Geçmişin, takım arkadaşın.";
        }

        private const int LegacyScene = -2, MenuScene = -1, LaboratoryScene = 0;
        private const int MaintenanceScene = 1, SynchronizationScene = 2, ReactorScene = 3;
        private const string ValidationScene = "EchoPrototype";

        [SerializeField] private string[] sceneNames =
        {
            "MainMenu", "Level1_ArastirmaLabi", "Level2_BakimHatti",
            "Level3_Eszamanlilik", "Level4_Reaktor"
        };

        [SerializeField, Range(LegacyScene, ReactorScene)] private int sceneEntry = LegacyScene;
        private static SceneEntryRequest pendingSceneEntry;
        private static bool runValidationOnEntry;
        private bool loadingScene;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSceneRequests()
        {
            pendingSceneEntry = null;
            runValidationOnEntry = false;
        }

        private sealed class SceneEntryRequest
        {
            public int Entry;
            public EchoSessionSave Resume;
            public float Elapsed;
            public int Loops;
            public bool SaveOnArrival;
        }

        private static int CampaignScene(int index) => index == 2 ? ReactorScene : index;

        private bool ChangeScene(int entry, EchoSessionSave resume = null, bool saveOnArrival = false)
        {
            if (sceneEntry == LegacyScene || testing || sceneEntry == entry) return false;
            if (loadingScene) return true;

            pendingSceneEntry = new SceneEntryRequest
            {
                Entry = entry,
                Resume = resume,
                Elapsed = elapsedTotal,
                Loops = savedLoops,
                SaveOnArrival = saveOnArrival
            };
            loadingScene = true;
            SceneManager.LoadScene(sceneNames[entry + 1]);
            return true;
        }

        private void InitializeSceneEntry()
        {
            if (sceneEntry == LegacyScene || sceneEntry == MenuScene)
            {
                pendingSceneEntry = null;
                LoadLevel(0);
                SetScreen(EchoScreen.MainMenu);
                return;
            }

            SceneEntryRequest requested = pendingSceneEntry;
            pendingSceneEntry = null;
            if (requested != null && requested.Entry != sceneEntry) requested = null;
            if (requested != null && requested.Resume != null)
            {
                RestoreSession(requested.Resume);
                return;
            }

            if (sceneEntry == SynchronizationScene)
            {
                StartSampleStage();
            }
            else
            {
                LoadLevel(sceneEntry == ReactorScene ? 2 : sceneEntry);
                SetScreen(EchoScreen.Playing);
            }

            if (requested == null) return;
            elapsedTotal = requested.Elapsed;
            savedLoops = requested.Loops;
            if (!requested.SaveOnArrival) return;
            if (!sampleActive) SaveCheckpoint(levelIndex);
            SaveSession();
        }

        private void ContinueToReactor()
        {
            if (ChangeScene(ReactorScene, saveOnArrival: true)) return;
            LoadLevel(2);
            SetScreen(EchoScreen.Playing);
            SaveSession();
        }

        private void OpenValidation()
        {
            if (sceneEntry == LegacyScene)
            {
                StartSmokeChecks();
                return;
            }

            if (loadingScene) return;
            SaveSession();
            runValidationOnEntry = true;
            loadingScene = true;
            SceneManager.LoadScene(ValidationScene);
        }

        private void LoadLevel(int index)
        {
            sampleActive = false; sampleDevices.Clear(); sampleEvents.Clear(); ClearWreckage(); ClearShockwaves();
            foreach (Ghost ghost in ghosts) { ghost.Body.SetActive(false); Destroy(ghost.Body); }
            UnloadAuthoredRoom();
            ghosts.Clear(); recordings.Clear(); savedTapes.Clear(); enemies.Clear(); consoles.Clear();
            sectorElapsed = 0; sectorLoops = 0; sectorDamageTaken = 0;
            levelIndex = Mathf.Clamp(index, 0, EchoLevelDefinition.Campaign.Length - 1);
            LoadAuthoredRoom(campaignRooms, levelIndex);
            NewLoop(); introRemaining = 6;
            TriggerChapterBanner();
            QueueStoryBriefing();
            message = Level.Objective;
        }

        private void StepWorld(float dt)
        {
            Advance(dt);
            elapsedTotal += dt; sectorElapsed += dt;
            TickResonance(dt); TickImmersion();
            decoyPositions.Clear();
            foreach (Ghost ghost in ghosts) decoyPositions.Add(ghost.Body.transform.position);
            foreach (EchoEnemy enemy in enemies)
            {
                Vector3 enemyPos = enemy.transform.position;
                enemy.Tick(dt, player, decoyPositions, dmg => { RegisterDamageSource(enemyPos); DamagePlayer(dmg); }, (from, to) => { ShowBeam(from, to, enemyBeam); PlayAttachedSound(WorldCue.SentryFire, from, 0.72f, owner: enemy.transform); });
                if (playerHealth == 0) break;
            }
            UpdateDoor(dt);
            damageFlash = Mathf.Max(0, damageFlash - dt);
            notificationRemaining = Mathf.Max(0, notificationRemaining - dt);
            TickWeapon(dt);
            TickWeaponEnergy(dt);
            TickCameraShake(dt);
            TickEchoTactics(dt);
            TickGameplayAudio(dt);
        }

        private void AdvancePlayingWorld(float dt)
        {
            if (!paused && !won && !planning && !storyBriefingActive) StepWorld(dt);
        }

        private void FinishExpiredLoop()
        {
            if (screen != EchoScreen.Playing || clock < ActiveLoopLength) return;
            if (sampleActive && recordings.Count < MaxEchoes)
            {
                OpenEchoRecordReview();
                return;
            }
            if (recordings.Count < MaxEchoes) CommitLoop();
            else
            {
                NewLoop();
                Notify("Süre doldu. İki Echo korundu. Q ile son kaydı geri alabilir, R ile değiştirebilirsin.");
            }
        }

        private void Advance(float dt)
        {
            clock = Mathf.Min(clock + dt, ActiveLoopLength);
            if (sampleActive)
            {
                AdvanceSampleEchoes(clock);
                UpdateEchoAnimationStates();
                if (clock >= nextSample) { RecordPose(clock, player.Pose); nextSample = clock + recordingInterval; }
                return;
            }
            for (int i = 0; i < ghosts.Count; i++)
            {
                Ghost ghost = ghosts[i];
                int ghostIndex = i;
                ApplyGhostPose(ghost, ghost.Tape.Sample(clock, EchoPose.Lerp));
                ghost.Cursor.Advance(clock, (eventTime, action) =>
                {
                    replayingEcho = ghostIndex;
                    EchoPose eventPose = ghost.Tape.Sample(eventTime, EchoPose.Lerp);
                    if (action.Kind == EchoActionKind.Console && TryActivate(eventPose.Position, action.TargetId))
                    { echoActivations++; TriggerGhostInteractAnimation(ghostIndex, eventPose.Position + Vector3.up); PlayAttachedSound(WorldCue.EchoAction, eventPose.Position, 0.65f); }
                    else if (action.Kind == EchoActionKind.Fire)
                    { ExecuteShot(action, true); echoShots++; }
                    else if (action.Kind == EchoActionKind.Archive && action.TargetId == levelIndex)
                    { if (TryArchive(eventPose.Position, true)) TriggerGhostInteractAnimation(ghostIndex, eventPose.Position + Vector3.up); }
                    replayingEcho = -1;
                });
            }
            UpdateEchoAnimationStates();
            if (clock >= nextSample)
            {
                RecordPose(clock, player.Pose);
                nextSample = clock + recordingInterval;
            }
        }

        private void Interact()
        {
            if (sampleActive) { InteractSample(); return; }
            Physics.SyncTransforms();
            RaycastHit hit;
            if (!Physics.Raycast(player.View.transform.position, player.View.transform.forward, out hit, 2.6f,
                ~(1 << 2), QueryTriggerInteraction.Ignore)) { PlayUiSound(consoleErrorAudio, 0.35f); return; }
            if (hit.transform == archiveTerminal) { if (TryArchive(player.transform.position, false)) { PlayUiSound(archiveUnlockAudio, 0.72f); RecordPose(clock, player.Pose); RecordAction(clock, EchoAction.Archive(levelIndex)); SaveSession(); } else PlayUiSound(consoleErrorAudio, 0.45f); return; }
            foreach (ConsoleState item in consoles)
            {
                if (hit.transform != item.Visual || !TryActivate(player.transform.position, item.Id)) continue;
                RecordPose(clock, player.Pose);
                RecordAction(clock, EchoAction.Console(item.Id));
                PlayAttachedSound(WorldCue.ConsoleInteract, item.Position, 0.72f, owner: item.Visual);
                Notify("Güç açıldı. Plakada kal ve R ile bir Echo kaydet.");
                return;
            }
            PlayUiSound(consoleErrorAudio, 0.35f);
        }

        private bool TryActivate(Vector3 actorPosition, int id)
        {
            if (id < 0 || id >= consoles.Count) return false;
            ConsoleState item = consoles[id];
            if (Vector3.Distance(actorPosition + Vector3.up * 1.2f, item.Position) > 2.6f) return false;
            item.Powered = true;
            item.Visual.GetComponent<Renderer>().sharedMaterial = powerOn;
            return true;
        }

        private static bool OnPlate(Vector3 position, BoxCollider area)
        {
            if (area == null || !area.enabled) return false;
            Vector3 offset = area.transform.InverseTransformPoint(position) - area.center;
            Vector3 half = area.size * .5f;
            return Mathf.Abs(offset.x) < half.x && Mathf.Abs(offset.y) < half.y && Mathf.Abs(offset.z) < half.z;
        }

        private void UpdateDoor(float dt)
        {
            if (sampleActive) { UpdateSampleDoor(dt); return; }
            power = true; plateHeld = true;
            foreach (ConsoleState item in consoles)
            {
                item.Held = OnPlate(player.transform.position, item.PlateArea);
                foreach (Ghost ghost in ghosts) item.Held |= OnPlate(ghost.Body.transform.position, item.PlateArea);
                power &= item.Powered; plateHeld &= item.Held;
                item.PlateVisual.GetComponent<Renderer>().sharedMaterial = item.Held ? plateOn : IdleMaterial(item.PlateVisual);
            }
            doorOpen = power && plateHeld;
            doorFraction = Mathf.MoveTowards(doorFraction, doorOpen ? 1 : 0, dt * 2);
            door.localPosition = closedDoorPosition + doorOpenOffset * doorFraction;
            doorCollider.enabled = closedDoorCollision && doorFraction < .92f;
        }

        private void CheckExit()
        {
            if (sampleActive) { CheckSampleExit(); return; }
            if (screen != EchoScreen.Playing || playerHealth <= 0 || player.transform.position.z <= 9 || !doorOpen || LivingEnemies() > 0) return;
            CompleteSectorProfile();
            RecordCurrentPerformanceRank(levelIndex, sectorElapsed, sectorLoops);
            SetScreen(levelIndex == 2 ? EchoScreen.GameWon : EchoScreen.LevelComplete);
            SaveCheckpoint(levelIndex == 2 ? -1 : levelIndex + 1);
            message = levelIndex == 2 ? "PROTOCOL VERIFIED. Tum sektorler tamamlandi." : "Sektor temiz. Sonraki protokole hazirsin.";
        }

        private int LivingEnemies()
        {
            int alive = 0; foreach (EchoEnemy enemy in enemies) if (enemy.Alive) alive++; return alive;
        }

        private void CommitLoop(int replacementIndex = -1)
        {
            if (clock < .25f) { Notify("Kaydetmek için kısa bir rota oluştur; en az 0,25 saniye gerekir."); return; }
            RecordPose(clock, player.Pose); current.Freeze();
            EchoSavedTape saved = new EchoSavedTape { Frames = capturedFrames.ToArray(), Actions = capturedActions.ToArray() };
            bool selectedReplacement = replacementIndex >= 0 && replacementIndex < recordings.Count;
            if (selectedReplacement) { recordings[replacementIndex] = current; savedTapes[replacementIndex] = saved; }
            else { recordings.Add(current); savedTapes.Add(saved); }
            bool replaced = selectedReplacement || recordings.Count > MaxEchoes;
            if (!selectedReplacement && replaced) { recordings.RemoveAt(0); if (savedTapes.Count > MaxEchoes) savedTapes.RemoveAt(0); }
            PlayUiSound(echoRecordAudio, 0.65f);
            PlayUiSound(echoResetAudio, 0.6f);
            savedLoops++; sectorLoops++; NewLoop(); SetScreen(EchoScreen.Playing); SaveSession();
            Notify(selectedReplacement ? "Echo " + (replacementIndex + 1) + " değiştirildi. Diğer Echo korundu; ikisi de otomatik oynuyor."
                : replaced ? "Yeni Echo kaydedildi. İki Echo sınırı nedeniyle en eski kayıt silindi."
                : "Echo " + recordings.Count + " kaydedildi. O artık otomatik oynuyor; sen başka göreve gidebilirsin.");
        }

        private void ClearRun()
        {
            recordings.Clear(); savedTapes.Clear(); NewLoop(); SetScreen(EchoScreen.Playing); SaveSession();
            Notify("Bu alanın Echo kayıtları silindi. Alan başlangıç durumuna döndü; tamamlanan kontrol noktaları korundu.");
        }

        private void UndoLastEcho()
        {
            if (recordings.Count == 0) { Notify("Silinecek Echo yok. X bu alanın kayıtlarını temizler."); return; }
            recordings.RemoveAt(recordings.Count - 1);
            if (savedTapes.Count > 0) savedTapes.RemoveAt(savedTapes.Count - 1);
            NewLoop(); SaveSession();
            Notify("Son Echo silindi. Önceki kayıt korundu; yeni bir rota deneyebilirsin.");
        }

        private void Notify(string text) { message = text; notificationRemaining = 3; }

        private void NewLoop()
        {
            planning = false; echoTeaching = false; pendingEchoRecord = false; replacementEchoSlot = -1;
            capturedFrames.Clear(); capturedActions.Clear();
            ClearBeams(); ClearImpacts(); ClearWreckage(); ClearShockwaves(); ResetResonance(); ResetWeaponEnergy(); ResetCameraShake();
            foreach (Ghost ghost in ghosts) { ghost.Body.SetActive(false); Destroy(ghost.Body); }
            ghosts.Clear();
            clock = 0; nextSample = recordingInterval; power = false; plateHeld = false; doorOpen = false;
            doorFraction = 0; echoActivations = 0; echoShots = 0;
            playerHealth = MaxPlayerHealth; nextPlayerShot = 0; damageFlash = 0;
            notificationRemaining = 0;
            recoil = 0; muzzleTimer = 0; hitTimer = 0; killTimer = 0;
            teamHitTimer = 0; teamHits = 0;
            door.localPosition = closedDoorPosition; doorCollider.enabled = closedDoorCollision;
            foreach (ConsoleState item in consoles)
            {
                item.Powered = false; item.Held = false;
                item.Visual.GetComponent<Renderer>().sharedMaterial = IdleMaterial(item.Visual);
                item.PlateVisual.GetComponent<Renderer>().sharedMaterial = IdleMaterial(item.PlateVisual);
            }
            foreach (EchoEnemy enemy in enemies) enemy.ResetEnemy();
            player.SetPose(spawn); if (sampleActive) ResetSampleLoop(); TickWeapon(0);
            current = new ReplayTimeline<EchoPose, EchoAction>(); RecordPose(0, player.Pose);
            for (int i = 0; i < recordings.Count; i++) ghosts.Add(CreateGhost(recordings[i], i));
            ResetAudioTracking();
            if (ghosts.Count > 0 && screen == EchoScreen.Playing) PlayUiSound(echoSpawnAudio, 0.5f);
            Physics.SyncTransforms();
        }

        private Ghost CreateGhost(ReplayTimeline<EchoPose, EchoAction> tape, int index)
        {
            EchoReplayActor actor = Instantiate(replayActors[index], worldRoot);
            actor.gameObject.name = "Echo " + (index + 1);
            Ghost ghost = new Ghost { Body = actor.gameObject, Head = actor.Aim, Motion = actor.Motion, Tape = tape, Cursor = tape.CreateCursor() };
            ghost.FinalPose = tape.Sample(tape.Duration, EchoPose.Lerp);
            if (sampleActive && sampleStage == 1)
                for (float t = 0; t <= tape.Duration; t += .05f)
                    if (OnPlate(tape.Sample(t, EchoPose.Lerp).Position, sampleSensorArea))
                    { if (ghost.SensorStart < 0) ghost.SensorStart = t; ghost.SensorEnd = t; }
            tape.CreateCursor().Advance(tape.Duration, (time, action) => {
                if (action.Kind == EchoActionKind.Console && action.TargetId >= 0 && action.TargetId < consoles.Count)
                    ghost.ConsoleMask |= 1 << action.TargetId;
            });
            ApplyGhostPose(ghost, tape.Sample(0, EchoPose.Lerp));
            return ghost;
        }

        private static void ApplyGhostPose(Ghost ghost, EchoPose pose)
        {
            ghost.Body.transform.SetPositionAndRotation(pose.Position, pose.Rotation);
            ghost.Head.localRotation = Quaternion.Euler(pose.Pitch, 0, 0);
        }

        private static void SetCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void OnDestroy()
        {
            if (minimapMesh != null) Destroy(minimapMesh);
            Application.logMessageReceived -= ObserveValidationError;
            SetCursor(false);
        }

        private void OnApplicationQuit()
        {
            if (CanPersist) { SaveSession(); preferences.Save(); }
        }
    }
}

