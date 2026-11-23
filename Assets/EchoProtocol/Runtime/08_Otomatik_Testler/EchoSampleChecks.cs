using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace EchoProtocol
{
    public sealed partial class EchoPrototypeGame
    {
        private int CountSampleObjects(string exactName)
        {
            int count = 0;
            foreach (Transform item in worldRoot.GetComponentsInChildren<Transform>(true)) if (item.name == exactName) count++;
            return count;
        }

        private IEnumerator WalkSampleTo(Vector3 target, float seconds = 4)
        {
            int stage = sampleStage;
            for (int step = 0; step < Mathf.CeilToInt(seconds / .04f); step++)
            {
                if (sampleStage != stage || screen != EchoScreen.Playing) yield break;
                Vector3 direction = target - player.transform.position; direction.y = 0;
                if (direction.magnitude < .17f) yield break;
                direction.Normalize();
                ProcessControls(.04f, new EchoInputFrame { Sprint = true, Move = new Vector2(Vector3.Dot(direction, player.transform.right), Vector3.Dot(direction, player.transform.forward)) });
                yield return null;
            }
        }
        private void WaitSampleUntil(float time) { while (clock < time - .0001f && screen == EchoScreen.Playing) ProcessControls(Mathf.Min(.05f, time - clock), new EchoInputFrame()); }
        private void UseSampleThroughControls(int id) { AimThroughControls(sampleDevices[id].Position); ProcessControls(.02f, new EchoInputFrame { Interact = true }); }
        private void LogSampleRoute(string route) { Debug.Log("ECHO SAMPLE ROUTE: " + route + " / recorded duration=" + clock.ToString("0.00") + "s / actions=" + current.ActionCount); }
        private IEnumerator RecordSampleLens()
        {
            IEnumerator walk = WalkSampleTo(new Vector3(-4, 0, -2)); while (walk.MoveNext()) yield return walk.Current;
            WaitSampleUntil(6.5f);
            walk = WalkSampleTo(new Vector3(-5, 0, -.3f)); while (walk.MoveNext()) yield return walk.Current;
            walk = WalkSampleTo(new Vector3(-5, 1, 4)); while (walk.MoveNext()) yield return walk.Current;
            UseSampleThroughControls(0); LogSampleRoute("lens / hold then upper relay"); ProcessControls(.04f, new EchoInputFrame { Record = true });
        }
        private IEnumerator RecordSampleShot()
        {
            IEnumerator walk = WalkSampleTo(new Vector3(0, 0, -2)); while (walk.MoveNext()) yield return walk.Current;
            WaitSampleUntil(5.5f); AimThroughControls(ResonancePosition); ProcessControls(.02f, new EchoInputFrame { Fire = true });
            walk = WalkSampleTo(new Vector3(5, 0, -2)); while (walk.MoveNext()) yield return walk.Current;
            walk = WalkSampleTo(new Vector3(5, 0, 4)); while (walk.MoveNext()) yield return walk.Current;
            UseSampleThroughControls(1); LogSampleRoute("shot / resonance then lower relay"); ProcessControls(.04f, new EchoInputFrame { Record = true });
        }
        private IEnumerator RecordSampleDecoy()
        {
            IEnumerator walk = WalkSampleTo(new Vector3(-4, 0, -6)); while (walk.MoveNext()) yield return walk.Current;
            walk = WalkSampleTo(new Vector3(-4, 0, 0)); while (walk.MoveNext()) yield return walk.Current;
            WaitSampleUntil(5);
            walk = WalkSampleTo(new Vector3(-4, 0, 4)); while (walk.MoveNext()) yield return walk.Current;
            UseSampleThroughControls(0); LogSampleRoute("decoy / distraction then service relay"); ProcessControls(.04f, new EchoInputFrame { Record = true });
        }
        private IEnumerator RecordSampleOperator()
        {
            IEnumerator walk = WalkSampleTo(new Vector3(5, 0, -6)); while (walk.MoveNext()) yield return walk.Current;
            walk = WalkSampleTo(new Vector3(5, 1, 4)); while (walk.MoveNext()) yield return walk.Current;
            WaitSampleUntil(7); AimThroughControls(ResonancePosition); ProcessControls(.02f, new EchoInputFrame { Fire = true });
            UseSampleThroughControls(1); LogSampleRoute("operator / timed resonance then evacuation"); ProcessControls(.04f, new EchoInputFrame { Record = true });
        }
        private IEnumerator RunSampleChecks()
        {
            BeginSample(); ProcessControls(0, new EchoInputFrame { Confirm = true }); preferences.SampleMemory = false;
            Verify(sampleActive && sampleStage == 0 && enemies.Count == 0 && sampleDevices.Count == 1 && player.Pose.Position == SampleSpawn.Position,
                "maintenance sample begins beside first decision without combat or inherited campaign echoes");
            Verify(!doorOpen && doorCollider.enabled && sampleCompleted == 0, "service gate starts physically closed without a completed checkpoint");
            Verify(!SampleGoal().Contains("Echo") && !SampleGoal().Contains("E ile") && !SampleGoal().Contains("→"),
                "service objective states the destination without prescribing the Echo solution");
            float gateCheckClock = clock; clock = 1; sampleServiceUntil = 0; string unopenedGate = SampleGateReason();
            sampleServiceUntil = .5f; string expiredGate = SampleGateReason(); sampleServiceUntil = 0; clock = gateCheckClock;
            Verify(unopenedGate.Contains("E işlemi bekleniyor") && expiredGate.Contains("geçiş rotanı kısalt") && unopenedGate != expiredGate,
                "service status distinguishes an untouched panel from an expired 1.4-second crossing window");
            Verify(CountSampleObjects("Art / Prop_Locker") == 2 && CountSampleObjects("Facility sign / dark housing") == 1 && CountSampleObjects("Shadowed maintenance work light") == 2,
                "service bay uses textured storage, a wall-mounted facility sign and local shadowed work lights");
            Checkpoint("sample-service"); yield return new WaitForSeconds(.25f);
            IEnumerator walk = WalkSampleTo(new Vector3(5, 0, 4), 2); while (walk.MoveNext()) yield return walk.Current;
            Verify(player.transform.position.z < 3.2f && doorCollider.enabled && !doorOpen && sampleCompleted == 0,
                "approaching a closed sample threshold does not open it or permit a solo shortcut");
            EchoPose atThreshold = SampleSpawn; atThreshold.Position = new Vector3(5, .02f, 3);
            player.SetPose(atThreshold); sampleServiceUntil = clock + .1f; UpdateDoor(.05f); StepWorld(.15f);
            walk = WalkSampleTo(new Vector3(5, 0, 5.4f)); while (walk.MoveNext()) yield return walk.Current;
            Verify(sampleStage == 1 && sampleCompleted == 1, "expired service window cannot trap a player who entered its open threshold");
            Verify(sampleDevices.Count == 2 && sampleDevices[0].StandbyMaterial == powerOff && sampleDevices[1].StandbyMaterial == objectiveOff && sampleDevices[0].Accent != sampleDevices[1].Accent && sampleDevices[0].Visual.GetComponent<Renderer>().sharedMaterial == powerOff && sampleDevices[1].Visual.GetComponent<Renderer>().sharedMaterial == objectiveOff,
                "crossfire relay terminals use distinct objective colors separate from Echo identity colors");
            int objectiveStage = sampleStage; sampleStage = 1; string transferGoal = SampleGoal(); sampleStage = 2; string combatGoal = SampleGoal(); sampleStage = objectiveStage;
            Verify(!transferGoal.Contains("→") && !transferGoal.Contains("rezonatörü vur") && !transferGoal.Contains("röleyi aç") &&
                !combatGoal.Contains("Muhafızı durdur") && !combatGoal.Contains("operatörü kullan") && !SampleArchiveHint().Contains("Echo’ya ateş etsin") && !SampleArchiveHint().Contains("rezonans varken"),
                "later objectives describe the situation without listing the puzzle solution order");
            bool spentBeforeGateCheck = resonanceSpent; float untilBeforeGateCheck = resonanceUntil;
            resonanceSpent = true; resonanceUntil = clock; string expiredPulse = SampleGateReason();
            resonanceSpent = spentBeforeGateCheck; resonanceUntil = untilBeforeGateCheck;
            Verify(expiredPulse.Contains("T aynı zamanlamayı tekrarlar") && expiredPulse.Contains("geciken E kaydını değiştir") && !expiredPulse.Contains("T ile aynı planı yeniden dene"),
                "closed resonance window identifies relay outcomes and directs the player to change a late interaction");
            BeginSample(); ProcessControls(0, new EchoInputFrame { Confirm = true });
            NewLoop(); walk = WalkSampleTo(new Vector3(-5, 0, -3.8f)); while (walk.MoveNext()) yield return walk.Current;
            UseSampleThroughControls(0);
            Verify(doorOpen && current.ActionCount == 1 && capturedActions[0].Action.Kind == EchoActionKind.Device,
                "real service E ray records device intent and opens the short physical window");
            float serviceTime = capturedActions[0].Time; LogSampleRoute("service / remote gate"); ProcessControls(.04f, new EchoInputFrame { Record = true });
            Verify(ghosts.Count == 1 && !doorOpen && sampleServiceUntil == 0 && clock == 0,
                "service rewind clears the short window while preserving the remote operator tape");
            walk = WalkSampleTo(new Vector3(5, 0, 5.4f)); while (walk.MoveNext()) yield return walk.Current;
            Verify(sampleStage == 1 && sampleCompleted == 1 && ghosts.Count == 0 && clock == 0,
                "input-only service Echo lets the player cross and stores an independent clean checkpoint");
            Debug.Log("ECHO SAMPLE ROUTE: service E=" + serviceTime.ToString("0.00") + "s / safe gate crossed through controller");
            EchoSessionSave checkpoint = CloneSave(CaptureSession());
            Verify(checkpoint.Version == 2 && checkpoint.Layout == 1 && checkpoint.Stage == 1 && checkpoint.CompletedMask == 1 && checkpoint.Tapes.Length == 0,
                "checkpoint save explicitly identifies new layout stage and completed independent puzzle");
            RestoreSession(checkpoint);
            Verify(sampleStage == 1 && sampleCompleted == 1 && CountSampleObjects("Service ramp") == 1 && current.FrameCount == 1,
                "checkpoint restoration never replays old-room tapes or sends player back to service puzzle");
            Checkpoint("sample-transfer"); yield return new WaitForSeconds(.25f);
            AimThroughControls(ResonancePosition); ProcessControls(.02f, new EchoInputFrame { Fire = true });
            Verify(!resonanceSpent && sampleShutter.GetComponent<Collider>().enabled && current.ActionCount == 1,
                "closed aperture physically blocks recorded resonance fire");
            ProcessControls(.3f, new EchoInputFrame()); ProcessControls(.01f, new EchoInputFrame { Record = true }); ProcessControls(.1f, new EchoInputFrame());
            Verify(!resonanceSpent && echoShots == 1 && message.Contains("ATIŞ HATTI KAPALI") && message.Contains("atış anında rezonatörü göremiyor"),
                "failed Echo shot explains the closed line without inventing resonance success");
            ClearRun(); EchoPose side = SampleSpawn; side.Position = new Vector3(-5, .02f, 3); player.SetPose(side);
            AimAt(ResonancePosition); TryFirePlayer();
            Verify(!resonanceSpent, "capacitor side housing blocks bypass shots around the narrow front aperture");
            ClearRun();
            IEnumerator route = RecordSampleLens(); while (route.MoveNext()) yield return route.Current;
            Verify(savedTapes.Count == 1 && savedTapes[0].Actions.Length == 1 && savedTapes[0].Actions[0].Action.Kind == EchoActionKind.Device && !sampleDevices[0].Powered,
                "unpowered upper relay intent is retained for future cooperation instead of discarded");
            route = RecordSampleShot(); while (route.MoveNext()) yield return route.Current;
            Verify(ghosts.Count == 2 && savedTapes[1].Actions.Length == 2 && savedTapes[1].Actions[0].Action.Kind == EchoActionKind.Fire,
                "second real route records aperture-dependent shot followed by a different relay task");
            Verify(SampleRecordPreview().Contains("Değiştirilecek Echo’yu seç") && SampleRecordPreview().Contains("kayıtları koru"), "full Echo slots preview a player choice before replacing any record");
            var transfer = CloneSave(CaptureSession()); float shotTime = transfer.Tapes[1].Actions[0].Time;
            StepWorld(8.6f);
            Verify(doorOpen && sampleDevices[0].Powered && sampleDevices[1].Powered && echoShots == 1 && echoActivations == 2,
                "cross-Echo events execute by time so aperture shot powers both relay intents");
            Verify(Mathf.Abs(resonanceUntil - shotTime - 6) < .001f && !SampleSensorHeld() && sampleShutter.GetComponent<Collider>().enabled,
                "replayed pulse uses its recorded time and the first Echo moves on after holding the sensor");
            // Both tapes keep their frames; the sensor has closed but the earlier pulse remains valid.
            Verify(transfer.Tapes[0].Frames[transfer.Tapes[0].Frames.Length - 1].Pose.Position.y > .8f,
                "first Echo actually climbs the authored upper walkway before its second task");
            DamagePlayer(100);
            Verify(screen == EchoScreen.GameLost && sampleDeaths == 1 && savedTapes.Count == 2 && sampleCompleted == 1,
                "sample death preserves both plans and completed checkpoint");
            RetryLevel();
            Verify(clock == 0 && playerHealth == 100 && ghosts.Count == 2 && sampleStage == 1 && sampleCompleted == 1 && !resonanceSpent,
                "sample retry immediately returns control to current decision with both Echoes");
            var deathSave = CloneSave(CaptureSession());
            TogglePlanning(); float before = clock; AdvancePlayingWorld(2);
            Verify(planning && clock == before && sampleStage == 1, "sample event map freezes world and checkpoint timer");
            Checkpoint("sample-event-map"); yield return new WaitForSeconds(.25f); TogglePlanning();
            RestoreSession(transfer);
            walk = WalkSampleTo(new Vector3(4, 0, -6)); while (walk.MoveNext()) yield return walk.Current;
            walk = WalkSampleTo(new Vector3(4, 0, 6)); while (walk.MoveNext()) yield return walk.Current;
            walk = WalkSampleTo(new Vector3(0, 0, 6)); while (walk.MoveNext()) yield return walk.Current;
            WaitSampleUntil(8.6f);
            Verify(doorOpen && sampleCompleted == 1 && !SampleSensorHeld(), "recorded movement hold shot and relay tasks combine while player uses independent side route");
            Checkpoint("sample-transfer-open"); yield return new WaitForSeconds(.25f);
            walk = WalkSampleTo(new Vector3(0, 0, 10)); while (walk.MoveNext()) yield return walk.Current;
            Verify(sampleStage == 2 && sampleCompleted == 3 && ghosts.Count == 0 && enemies.Count == 1,
                "physical transfer bridge completion advances to combat checkpoint without repeating solved puzzles");
            Verify(sampleDevices.Count == 3 && sampleDevices[1].StandbyMaterial == objectiveOff && sampleDevices[2].StandbyMaterial == objectiveOff && sampleDevices[1].StandbyMaterial != ghostCyan && sampleDevices[2].StandbyMaterial != ghostPurple,
                "evacuation operator and optional archive terminals do not reuse cyan or violet Echo identity colors");
            var combatCheckpoint = CloneSave(CaptureSession());
            side = SampleSpawn; side.Position = new Vector3(5, 1.02f, 4); player.SetPose(side); UseSampleThroughControls(1);
            Verify(!sampleExtraction && current.ActionCount == 1 && message.Contains("Sol servis rölesi"),
                "early evacuation operator explains missing service relay while recording reusable E intent");
            NewLoop(); Checkpoint("sample-combat"); yield return new WaitForSeconds(.25f);
            route = RecordSampleDecoy(); while (route.MoveNext()) yield return route.Current;
            Verify(ghosts.Count == 1 && savedTapes[0].Actions.Length == 1 && sampleStage == 2,
                "real decoy route records visible distraction followed by service relay task");
            route = RecordSampleOperator(); while (route.MoveNext()) yield return route.Current;
            Verify(ghosts.Count == 2 && savedTapes[1].Actions.Length == 2 && enemies[0].Alive,
                "planned resonance plus operator route completes without requiring an unrelated sentry kill");
            var finalPlan = CloneSave(CaptureSession());
            walk = WalkSampleTo(new Vector3(-5, 0, -6)); while (walk.MoveNext()) yield return walk.Current;
            walk = WalkSampleTo(new Vector3(-5, 1, 4)); while (walk.MoveNext()) yield return walk.Current;
            WaitSampleUntil(7.5f);
            Verify(sampleDecoyObserved && enemies[0].Attacks > 0 && enemies[0].Alive && sampleExtraction && doorOpen && ResonanceRemaining > 0,
                "real guard attacks the decoy then timed Echo resonance and operator release evacuation");
            UseSampleThroughControls(2);
            Verify(preferences.SampleMemory && sampleDevices[2].Powered && sampleCompleted == 3,
                "optional observation memory uses guard target evidence and resonance instead of another plate puzzle");
            Checkpoint("sample-memory"); yield return new WaitForSeconds(.25f);
            SetScreen(EchoScreen.Archives); Checkpoint("sample-archive"); yield return new WaitForSeconds(.25f);
            SetScreen(EchoScreen.Playing);
            AimThroughControls(new Vector3(-9, 1.2f, 5)); Checkpoint("sample-observation"); yield return new WaitForSeconds(.25f);
            NewLoop();
            Verify(preferences.SampleMemory && !sampleDecoyObserved && !sampleExtraction && sampleCompleted == 3,
                "memory persists while tactical evidence evacuation and enemy state reset per loop");
            RestoreSession(finalPlan);
            walk = WalkSampleTo(new Vector3(-4, 0, -6)); while (walk.MoveNext()) yield return walk.Current;
            walk = WalkSampleTo(new Vector3(-4, 0, 6)); while (walk.MoveNext()) yield return walk.Current;
            walk = WalkSampleTo(new Vector3(0, 0, 6)); while (walk.MoveNext()) yield return walk.Current;
            WaitSampleUntil(7.5f);
            Verify(doorOpen && enemies[0].Alive && sampleFinalEvents.Count >= 3, "restored final plan opens evacuation and exposes its actual event history");
            Checkpoint("sample-final-plan"); yield return new WaitForSeconds(.25f);
            walk = WalkSampleTo(new Vector3(0, 0, 10)); while (walk.MoveNext()) yield return walk.Current;
            Verify(screen == EchoScreen.GameWon && sampleCompleted == 7 && player.transform.position.z >= 9.2f && enemies[0].Alive,
                "controller physically reaches sample finale through cooperation with a living stopped sentry");
            Checkpoint("sample-win"); yield return new WaitForSeconds(.25f);
            var finished = CloneSave(CaptureSession()); RestoreSession(finished);
            Verify(screen == EchoScreen.GameWon && sampleCompleted == 7 && sampleStage == 2 && sampleFinalEvents.Count >= 3,
                "completed sample save restores finale rather than reopening a solved chapter");
            RestoreSession(finalPlan);
            float[] sampleFrameTimes = new float[120];
            for (int frame = 0; frame < sampleFrameTimes.Length; frame++)
            {
                StepWorld(Mathf.Min(Time.unscaledDeltaTime, .1f)); yield return null;
                sampleFrameTimes[frame] = Time.unscaledDeltaTime * 1000;
            }
            Array.Sort(sampleFrameTimes);
            string samplePerformance = "ECHO BENCHMARK (120 frames, maintenance sample, 2 Echoes): median=" + sampleFrameTimes[60].ToString("0.0") +
                "ms p95=" + sampleFrameTimes[114].ToString("0.0") + "ms Unity allocated=" +
                (UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024)) + "MiB";
            Debug.Log(samplePerformance); PerformanceSummary += "\n" + samplePerformance;
            RestoreSession(finished);
            string path = Path.Combine(Application.temporaryCachePath, "echo-sample-" + Guid.NewGuid().ToString("N") + ".json");
            var store = new EchoSaveStore(path);
            try
            {
                store.Write(deathSave); EchoSessionSave read; bool recovered;
                Verify(store.TryRead(out read, out recovered) && read.Version == 2 && read.Stage == 1 && read.Deaths == 1 && read.Tapes.Length == 2,
                    "v2 sample checkpoint death count and two exact tapes survive real disk roundtrip");
                store.Write(combatCheckpoint); File.WriteAllText(path, "corrupt sample");
                Verify(store.TryRead(out read, out recovered) && recovered && read.Stage == 1,
                    "v2 corruption recovers a valid prior independent checkpoint");
                RestoreSession(read); StepWorld(8.6f);
                Verify(sampleStage == 1 && doorOpen && echoShots == 1 && sampleCompleted == 1,
                    "disk recovered sample Echoes perform actual aperture shot and relay cooperation");
            }
            finally { store.Clear(); }
            var bad = CloneSave(transfer); bad.Stage = 3; Verify(InvalidSession(bad), "v2 unknown maintenance room is rejected");
            bad = CloneSave(transfer); bad.CompletedMask = 2; Verify(InvalidSession(bad), "v2 skipped or inconsistent checkpoint progression is rejected");
            bad = CloneSave(transfer); bad.Tapes[0].Actions[0].Action.TargetId = 2; Verify(InvalidSession(bad), "v2 device from another room cannot enter a restored plan");
            bad = CloneSave(transfer); bad.Version = 1; Verify(InvalidSession(bad), "new layout tapes cannot masquerade as legacy v1 saves");
            bad = CloneSave(finished); bad.FinalEvents = new string[9]; Verify(InvalidSession(bad), "unbounded restored finale event history is rejected");
            bad = CloneSave(finished); bad.FinalEvents = new[] { new string('x', 201) }; Verify(InvalidSession(bad), "oversized restored event text is rejected before UI use");
            var legacy = new EchoSessionSave { Level = 0, Tapes = new EchoSavedTape[0] };
            var legacyStore = new EchoSaveStore(path + ".legacy"); var migratedStore = new EchoSaveStore(path + ".v2");
            try
            {
                legacyStore.Write(legacy); string originalJson = File.ReadAllText(legacyStore.Path);
                EchoSessionSave migrated; bool recovered;
                Verify(migratedStore.ImportLegacyIfMissing(legacyStore) && migratedStore.TryRead(out migrated, out recovered) && migrated.Version == 1,
                    "real v1 disk data imports once into the v2 location without changing its schema");
                Verify(File.ReadAllText(legacyStore.Path) == originalJson, "legacy import leaves original save bytes intact");
                migratedStore.Write(transfer);
                Verify(!migratedStore.ImportLegacyIfMissing(legacyStore) && migratedStore.TryRead(out migrated, out recovered) && migrated.Version == 2 && migrated.Stage == 1,
                    "existing sample progress wins over old legacy save during import");
                migratedStore.Clear(); File.WriteAllText(legacyStore.Path, "invalid legacy");
                Verify(!migratedStore.ImportLegacyIfMissing(legacyStore) && !File.Exists(migratedStore.Path), "invalid legacy file cannot seed new session progress");
            }
            finally { legacyStore.Clear(); migratedStore.Clear(); }
            legacy.Validate(); RestoreSession(legacy);
            Verify(!sampleActive && levelIndex == 0 && ghosts.Count == 0, "v1 campaign save remains readable without interpreting new maintenance state");
            BeginSample(); ProcessControls(0, new EchoInputFrame { Confirm = true }); sampleCompleted = 3; LoadSampleStage(2); SetScreen(EchoScreen.Playing);
            preferences.SampleMemory = false; enemies[0].TakeDamage(100);
            side = SampleSpawn; side.Position = new Vector3(-5, 1.02f, 4); player.SetPose(side); UseSampleThroughControls(2);
            Verify(!preferences.SampleMemory && message.Contains("Gözlem eksik"), "killing sentry without Echo target evidence cannot fake the optional observation");
            side.Position = new Vector3(-4, .02f, 4); player.SetPose(side); UseSampleThroughControls(0);
            side.Position = new Vector3(5, 1.02f, 4); player.SetPose(side); UseSampleThroughControls(1);
            Verify(sampleExtraction && doorOpen && !enemies[0].Alive, "direct combat is accepted as an alternate evacuation tactic");
            RestartSector();
            Verify(sampleStage == 2 && sampleCompleted == 3 && recordings.Count == 0 && enemies[0].Alive,
                "explicit current checkpoint restart clears local plan but retains both solved independent rooms");
            for (int repeat = 0; repeat < 15; repeat++)
            {
                sampleCompleted = (1 << (repeat % 3)) - 1; LoadSampleStage(repeat % 3); SetScreen(EchoScreen.Playing);
                for (int echo = 0; echo < 3; echo++) { ProcessControls(.3f, new EchoInputFrame()); ProcessControls(.02f, new EchoInputFrame { Record = true }); if (pendingEchoRecord) ChooseEchoReplacement(0); }
                yield return null; yield return null;
                if (transform.childCount != 1 || ghosts.Count != 2 || worldRoot.GetComponentsInChildren<EchoEnemy>(true).Length != (sampleStage == 2 ? 1 : 0) || CountSampleObjects("Service ramp") != (sampleStage == 0 ? 0 : sampleStage == 1 ? 1 : 2))
                    throw new InvalidOperationException("Sample object leak at repeat " + repeat);
            }
            Verify(sampleEvents.Count <= 1024 && impactObjects.Count <= 12,
                "fifteen sample transitions and forty-five commits keep events and impact effects bounded");
            LoadLevel(0); SetScreen(EchoScreen.MainMenu); yield return null; yield return null;
            Verify(!sampleActive && CountSampleObjects("Service ramp") == 0 && recordings.Count == 0 && transform.childCount == 1,
                "leaving sample releases ramp meshes and cleanly restores legacy menu world");
        }
    }
}
