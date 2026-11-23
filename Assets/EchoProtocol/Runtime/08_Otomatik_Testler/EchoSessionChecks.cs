using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace EchoProtocol
{
    public sealed partial class EchoPrototypeGame
    {
        private static bool InvalidSession(EchoSessionSave data)
        {
            try { data.Validate(); return false; }
            catch (InvalidDataException) { return true; }
        }
        private static EchoSessionSave CloneSave(EchoSessionSave data)
        {
            return JsonUtility.FromJson<EchoSessionSave>(JsonUtility.ToJson(data));
        }
        private IEnumerator RunSessionChecks()
        {
            bool routePixelsMatch = true;
            Rect mapRect = new Rect(282, 195, 256, 360);
            foreach (Vector2 size in new[] { new Vector2(800, 600), new Vector2(1280, 720), new Vector2(1920, 1080), new Vector2(2560, 1080) })
            {
                Matrix4x4 canvas = UiMatrix(size.x, size.y);
                Vector2 start = MapPoint(mapRect, spawn.Position);
                foreach (Vector3 destination in EchoLevelDefinition.Campaign[2].Plates)
                {
                    Vector2 end = MapPoint(mapRect, destination);
                    Matrix4x4 line = MapLineMatrix(canvas, start, end);
                    routePixelsMatch &= Vector3.Distance(line.MultiplyPoint3x4(Vector3.zero), canvas.MultiplyPoint3x4(start)) < .001f &&
                        Vector3.Distance(line.MultiplyPoint3x4(new Vector3(Vector2.Distance(start, end), 0, 0)), canvas.MultiplyPoint3x4(end)) < .001f;
                }
            }
            Verify(routePixelsMatch, "map route endpoints meet their pressure plates at four canvas resolutions");
            BeginCampaign();
            IEnumerator route = RecordThroughControls(0, false);
            while (route.MoveNext()) yield return route.Current;
            EchoSessionSave original = CaptureSession(); original.Validate();
            Verify(savedTapes.Count == 1 && original.Tapes[0].Frames.Length == recordings[0].FrameCount && original.Tapes[0].Actions.Length == recordings[0].ActionCount,
                "persistent tape contains every exact captured pose and console event");
            EchoSessionSave copy = CloneSave(original);
            var restoredTape = copy.Tapes[0].Restore(0); bool exactSamples = true;
            for (float t = 0; t < restoredTape.Duration + .1f; t += .07f)
            {
                EchoPose a = recordings[0].Sample(t, EchoPose.Lerp), b = restoredTape.Sample(t, EchoPose.Lerp);
                exactSamples &= Vector3.Distance(a.Position, b.Position) < .0001f && Quaternion.Angle(a.Rotation, b.Rotation) < .01f && Mathf.Abs(a.Pitch - b.Pitch) < .0001f;
            }
            Verify(exactSamples && restoredTape.Frozen && restoredTape.Duration == recordings[0].Duration, "JSON tape roundtrip preserves sampled movement rotation pitch and duration");
            resumeSave = copy; preferences.ResumeLevel = copy.Level;
            ReturnToMenu(); ContinueSavedCampaign();
            Verify(recordings.Count == 1 && ghosts.Count == 1 && clock == 0 && playerHealth == 100 && !power && !doorOpen, "continue restores the Echo but starts a clean living world");
            ControlSteps(100, .05f, new EchoInputFrame());
            Verify(echoActivations == 1 && power && plateHeld && doorOpen && !doorCollider.enabled, "disk-restored Echo activates console and holds the exit gate open");

            string path = Path.Combine(Application.temporaryCachePath, "echo-verification-" + Guid.NewGuid().ToString("N") + ".json");
            var store = new EchoSaveStore(path);
            try
            {
                EchoSessionSave loaded; bool recovered;
                Verify(!store.TryRead(out loaded, out recovered) && !recovered, "missing session is a safe fresh start");
                store.Write(original);
                Verify(store.TryRead(out loaded, out recovered) && !recovered && loaded.Tapes[0].Frames.Length == original.Tapes[0].Frames.Length,
                    "session survives an actual UTF8 file roundtrip outside user save files");
                store.Write(original); File.WriteAllText(path, "broken JSON");
                Verify(store.TryRead(out loaded, out recovered) && recovered && loaded.Tapes.Length == 1, "corrupt primary save recovers the previous validated backup");
                store.Write(loaded); File.WriteAllText(path, "broken again");
                Verify(store.TryRead(out loaded, out recovered) && recovered, "repairing a corrupt save preserves its good backup");
                store.Clear(); File.WriteAllText(path, "{\"Version\":88}");
                Verify(!store.TryRead(out loaded, out recovered), "invalid session without backup is ignored without crashing");
                File.WriteAllText(path, new string('x', 1024 * 1024 + 1));
                Verify(!store.TryRead(out loaded, out recovered), "oversized disk save is rejected before JSON parsing");
                store.Clear(); Verify(!File.Exists(path) && !File.Exists(path + ".bak") && !File.Exists(path + ".tmp"), "clearing a session removes only its own primary backup and temporary files");
            }
            finally { store.Clear(); }
            copy = CloneSave(original); copy.Version = 2;
            Verify(InvalidSession(copy), "unsupported future save version is rejected");
            copy = CloneSave(original); copy.Level = 3;
            Verify(InvalidSession(copy), "unknown saved sector is rejected");
            copy = CloneSave(original); copy.Tapes = new[] { copy.Tapes[0], copy.Tapes[0], copy.Tapes[0] };
            Verify(InvalidSession(copy), "disk save cannot exceed the two Echo cap");
            copy = CloneSave(original); copy.Tapes[0].Frames[1].Time = 0;
            Verify(InvalidSession(copy), "non-increasing saved frame times are rejected");
            copy = CloneSave(original); copy.Tapes[0].Frames[1].Pose.Position.x = float.NaN;
            Verify(InvalidSession(copy), "NaN pose cannot enter a restored game world");
            copy = CloneSave(original); copy.Tapes[0].Frames[1].Pose.Rotation = new Quaternion(0, 0, 0, 0);
            Verify(InvalidSession(copy), "invalid saved rotation is rejected");
            copy = CloneSave(original); copy.Tapes[0].Actions[0].Action.TargetId = 1;
            Verify(InvalidSession(copy), "saved interaction cannot target a console outside its sector");
            copy = CloneSave(original); copy.Tapes[0].Actions[0].Time = 21;
            Verify(InvalidSession(copy), "saved action cannot fire after the recording ends");
            copy = CloneSave(original); copy.SectorElapsed = copy.Elapsed + 1;
            Verify(InvalidSession(copy), "invalid saved counters are rejected");

            RestoreSession(original); var preserved = recordings[0];
            DamagePlayer(100); RetryLevel();
            Verify(screen == EchoScreen.Playing && playerHealth == 100 && clock == 0 && recordings[0] == preserved && ghosts.Count == 1,
                "death retry keeps the immutable Echo solution and restores health");
            ControlSteps(100, .05f, new EchoInputFrame());
            Verify(power && plateHeld && echoActivations == 1, "retained Echo works again after death retry");
            SetScreen(EchoScreen.Paused); RestartSector();
            Verify(recordings.Count == 0 && savedTapes.Count == 0 && ghosts.Count == 0 && clock == 0 && !power, "explicit sector restart clears both live and saved Echo data");

            LoadLevel(1); SetScreen(EchoScreen.Playing); ControlSteps(5, .05f, new EchoInputFrame());
            Vector3 position = player.transform.position, sentryPosition = enemies[0].transform.position;
            float stoppedClock = clock, stoppedElapsed = elapsedTotal; int frames = current.FrameCount;
            ProcessControls(.1f, new EchoInputFrame { Plan = true, Move = Vector2.up }, false);
            Verify(planning && screen == EchoScreen.Playing && clock == stoppedClock && player.transform.position == position && Cursor.lockState == CursorLockMode.None,
                "Tab opens planning with an unlocked cursor without consuming a gameplay frame");
            ProcessControls(1, new EchoInputFrame { Move = Vector2.up, Fire = true, Interact = true, Record = true, Clear = true, Undo = true, Jump = true });
            AdvancePlayingWorld(1);
            Verify(clock == stoppedClock && elapsedTotal == stoppedElapsed && current.FrameCount == frames && current.ActionCount == 0 && player.transform.position == position && enemies[0].transform.position == sentryPosition && playerHealth == 100,
                "planning freezes movement enemies time recording combat and destructive hotkeys");
            Verify(!TryFirePlayer(), "weapon rejects direct fire while planning");
            Checkpoint("planning"); yield return new WaitForSeconds(.25f);
            ProcessControls(.1f, new EchoInputFrame { Plan = true }, false);
            ProcessControls(.05f, new EchoInputFrame { Move = Vector2.up });
            Verify(!planning && clock > stoppedClock && player.transform.position.z > position.z, "Tab closes the map even with an unlocked cursor and gameplay resumes");
            ProcessControls(.1f, new EchoInputFrame { Plan = true }); ProcessControls(.1f, new EchoInputFrame { Pause = true });
            Verify(!planning && screen == EchoScreen.Paused, "Escape from planning enters normal pause without leaving a hidden freeze");
            SetScreen(EchoScreen.Playing); TogglePlanning(); ReturnToMenu();
            Verify(!planning && screen == EchoScreen.MainMenu && clock == 0, "menu transition resets planning state");

            const string prefix = "EchoProtocol.SessionVerification.";
            try
            {
                var profile = new EchoPreferences(prefix); profile.RecordClear(0, 25, 2); profile.Save();
                var read = new EchoPreferences(prefix); read.Load();
                Verify(read.UnlockedLevel == 1 && read.BestSeconds[0] == 25 && read.BestLoops[0] == 2, "sector unlock time and loop scores survive real PlayerPrefs reload");
                read.RecordClear(0, 30, 3); read.RecordClear(0, 24, 1);
                Verify(read.BestSeconds[0] == 24 && read.BestLoops[0] == 1, "sector replay only improves the personal bests");
                preferences.UnlockedLevel = 0; ReturnToMenu(); BeginSector(2);
                Verify(screen == EchoScreen.MainMenu && levelIndex == 0, "locked sector cannot be started through the level-select action");
                preferences.UnlockedLevel = 2; BeginSector(1);
                Verify(levelIndex == 1 && screen == EchoScreen.Playing && ghosts.Count == 0 && sectorElapsed == 0 && sectorLoops == 0, "unlocked level selection starts a clean selected sector");
            }
            finally
            {
                foreach (string key in new[] { "Sensitivity", "Volume", "FieldOfView", "InvertLook", "Completed", "ResumeLevel", "UnlockedLevel", "ArchiveMask", "SampleMemory", "V2Initialized", "BestSeconds0", "BestSeconds1", "BestSeconds2", "BestLoops0", "BestLoops1", "BestLoops2" }) PlayerPrefs.DeleteKey(prefix + key);
                PlayerPrefs.Save();
            }

            elapsedTotal = 0; savedLoops = 0; LoadLevel(2); SetScreen(EchoScreen.Playing);
            route = RecordThroughControls(0, true); while (route.MoveNext()) yield return route.Current;
            route = RecordThroughControls(1, false); while (route.MoveNext()) yield return route.Current;
            copy = CloneSave(CaptureSession()); RestoreSession(copy);
            Verify(recordings.Count == 2 && ghosts.Count == 2 && current.FrameCount == 1 && playerHealth == 100 && LivingEnemies() == 2, "two Echoes including recorded combat restore into a reset reactor");
            TogglePlanning(); Checkpoint("planning-dual-echo");
            yield return new WaitForSeconds(.25f); TogglePlanning();
            route = ExitThroughControls(); while (route.MoveNext()) yield return route.Current;
            Verify(screen == EchoScreen.GameWon && echoActivations == 2 && echoShots == 6 && playerHealth > 0 && player.transform.position.z > 9,
                "JSON-restored dual Echo console shots and final plate holds complete the real campaign exit");
            Checkpoint("restored-win"); yield return new WaitForSeconds(.25f);
        }
    }
}
