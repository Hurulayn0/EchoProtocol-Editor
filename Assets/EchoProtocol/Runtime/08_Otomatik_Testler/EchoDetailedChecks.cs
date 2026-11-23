using System;
using System.Collections;
using UnityEngine;

namespace EchoProtocol
{
    public sealed partial class EchoPrototypeGame
    {
        private void ControlSteps(int count, float dt, EchoInputFrame input)
        {
            for (int i = 0; i < count; i++) ProcessControls(dt, input);
        }

        private void AimThroughControls(Vector3 target)
        {
            Vector3 direction = target - player.View.transform.position;
            float yaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float pitch = -Mathf.Atan2(direction.y, new Vector2(direction.x, direction.z).magnitude) * Mathf.Rad2Deg;
            ProcessControls(.01f, new EchoInputFrame { Look = new Vector2(
                Mathf.DeltaAngle(player.transform.eulerAngles.y, yaw) / player.LookSensitivity,
                (player.Pose.Pitch - pitch) / player.LookSensitivity * (player.InvertLook ? -1 : 1)) });
        }

        private IEnumerator RecordThroughControls(int side, bool combat)
        {
            Vector3 target = consoles[side].Plate;
            int stepsX = Mathf.RoundToInt(Mathf.Abs(target.x - player.transform.position.x) / .16f);
            Vector2 horizontal = target.x < player.transform.position.x ? Vector2.left : Vector2.right;
            for (int i = 0; i < stepsX; i++) { ProcessControls(.04f, new EchoInputFrame { Move = horizontal }); yield return null; }
            int stepsZ = Mathf.RoundToInt((target.z - player.transform.position.z) / .16f);
            for (int i = 0; i < stepsZ; i++) { ProcessControls(.04f, new EchoInputFrame { Move = Vector2.up }); yield return null; }
            AimThroughControls(consoles[side].Position);
            ProcessControls(.05f, new EchoInputFrame { Interact = true });
            if (!consoles[side].Powered) throw new InvalidOperationException("Control route failed to activate console " + side);
            if (combat)
                foreach (EchoEnemy enemy in enemies)
                {
                    for (int shot = 0; shot < 3; shot++)
                    {
                        AimThroughControls(enemy.transform.position + Vector3.up * .95f);
                        ProcessControls(.05f, new EchoInputFrame { Fire = true });
                        ControlSteps(5, .05f, new EchoInputFrame()); yield return null;
                    }
                    if (enemy.Alive) throw new InvalidOperationException("Control route did not defeat sentry " + enemy.Id);
                }
            ProcessControls(.05f, new EchoInputFrame { Record = true });
        }

        private IEnumerator ExitThroughControls()
        {
            // Stay out of the recorded firing line until the Echo's burst finishes.
            float routeEnd = 0; foreach (var tape in recordings) routeEnd = Mathf.Max(routeEnd, tape.Duration);
            while (clock < routeEnd + .1f) { ProcessControls(.05f, new EchoInputFrame()); yield return null; }
            for (int i = 0; i < 140 && screen == EchoScreen.Playing; i++)
            {
                ProcessControls(.05f, new EchoInputFrame { Move = Vector2.up }); yield return null;
            }
        }

        private IEnumerator RunDetailedChecks()
        {
            EchoInputFrame walk = new EchoInputFrame { Move = Vector2.up };
            EchoInputFrame confirm = new EchoInputFrame { Confirm = true };
            EchoInputFrame pauseKey = new EchoInputFrame { Pause = true };
            EchoInputFrame recordKey = new EchoInputFrame { Record = true };
            EchoInputFrame idle = new EchoInputFrame();

            ReturnToMenu(); Vector3 startPosition = player.transform.position;
            ProcessControls(.1f, new EchoInputFrame { Move = Vector2.up, Fire = true, Interact = true, Jump = true });
            Verify(clock == 0 && current.ActionCount == 0 && player.transform.position == startPosition,
                "menu blocks gameplay controls");
            ProcessControls(.05f, confirm);
            Verify(screen == EchoScreen.Playing && clock == 0, "Enter starts without consuming a gameplay frame");
            player.LookSensitivity = 1; player.InvertLook = false;

            // These tests use the same input path and CharacterController as Update, without teleporting a route.
            ControlSteps(20, .05f, walk);
            Verify(Mathf.Abs(player.transform.position.z - spawn.Position.z - 4) < .03f && current.FrameCount > 10,
                "WASD input moves four metres and records real controller poses");
            NewLoop(); ControlSteps(20, .05f, new EchoInputFrame { Move = Vector2.one });
            Vector3 diagonal = player.transform.position - spawn.Position; diagonal.y = 0;
            Verify(Mathf.Abs(diagonal.magnitude - 4) < .03f, "diagonal input does not increase movement speed");
            NewLoop(); ProcessControls(.01f, new EchoInputFrame { Look = new Vector2(90, 200) });
            Verify(Mathf.Abs(Mathf.DeltaAngle(player.transform.eulerAngles.y, 90)) < .01f && player.Pose.Pitch == -80,
                "mouse yaw and pitch clamp use gameplay input");
            ProcessControls(.01f, new EchoInputFrame { Look = new Vector2(0, -400) });
            Verify(player.Pose.Pitch == 80, "opposite pitch clamp prevents camera inversion");
            NewLoop(); player.LookSensitivity = 2; player.InvertLook = true;
            ProcessControls(.01f, new EchoInputFrame { Look = new Vector2(10, 5) });
            Verify(Mathf.Abs(player.transform.eulerAngles.y - 20) < .01f && Mathf.Abs(player.Pose.Pitch - 10) < .01f,
                "sensitivity and inverted mouse alter actual camera controls");
            ApplyPreferences(); NewLoop(); ControlSteps(3, .05f, idle);
            float groundY = player.transform.position.y;
            ProcessControls(.02f, new EchoInputFrame { Jump = true });
            float highest = player.transform.position.y;
            for (int i = 0; i < 35; i++)
            {
                ProcessControls(.02f, idle); highest = Mathf.Max(highest, player.transform.position.y);
                yield return null;
            }
            Verify(highest > groundY + .5f && player.Controller.isGrounded &&
                Mathf.Abs(player.transform.position.y - groundY) <= player.Controller.skinWidth + .01f,
                "Space jumps with gravity and lands on the real floor");
            ProcessControls(.02f, new EchoInputFrame { Jump = true });
            float airPeak = player.transform.position.y;
            for (int i = 0; i < 40; i++)
            {
                ProcessControls(.02f, new EchoInputFrame { Jump = i < 12 });
                airPeak = Mathf.Max(airPeak, player.transform.position.y);
            }
            Verify(player.Controller.isGrounded && airPeak < groundY + .85f && player.transform.position.y < groundY + .1f,
                "repeated jump requests in air do not create a double jump");
            NewLoop(); ControlSteps(40, .1f, new EchoInputFrame { Move = Vector2.left });
            Verify(player.transform.position.x > -7.6f && player.transform.position.x < -7,
                "outer wall blocks real controller movement");
            NewLoop(); ControlSteps(45, .1f, walk);
            Verify(player.transform.position.z > 4 && player.transform.position.z < 4.6f && !won,
                "closed gate physically blocks WASD and exit");

            NewLoop(); AimAt(consoles[0].Position);
            ProcessControls(.05f, new EchoInputFrame { Interact = true });
            Verify(!power && current.ActionCount == 0, "E cannot activate a console beyond interaction range");
            EchoPose platePose = spawn; platePose.Position = consoles[0].Plate;
            player.SetPose(platePose); ProcessControls(.05f, new EchoInputFrame { Look = new Vector2(180, 0), Interact = true });
            Verify(!power && current.ActionCount == 0, "E requires looking at the console");
            AimAt(consoles[0].Position); ProcessControls(.05f, new EchoInputFrame { Interact = true });
            Verify(power && current.ActionCount == 1 && doorOpen, "E control records a real console interaction");
            NewLoop(); ControlSteps(4, .05f, idle); ProcessControls(.05f, recordKey);
            Verify(recordings.Count == 0 && Mathf.Abs(clock - .2f) < .001f, "R rejects recordings below quarter second");
            ProcessControls(.05f, idle); ProcessControls(.05f, recordKey);
            Verify(recordings.Count == 1 && recordings[0].Frozen && clock == 0, "R commits exactly at quarter-second boundary");
            ProcessControls(.05f, new EchoInputFrame { Clear = true, Record = true, Fire = true });
            Verify(recordings.Count == 0 && clock == 0 && current.ActionCount == 0, "X takes priority and clears without an accidental shot");
            ControlSteps(8, .05f, idle); ProcessControls(.05f, recordKey);
            ProcessControls(.05f, pauseKey); Vector3 pausedPosition = player.transform.position;
            float pausedClock = clock, pausedElapsed = elapsedTotal;
            ProcessControls(.1f, new EchoInputFrame { Move = Vector2.up, Jump = true, Fire = true, Interact = true, Record = true, Clear = true });
            Verify(screen == EchoScreen.Paused && clock == pausedClock && elapsedTotal == pausedElapsed &&
                player.transform.position == pausedPosition && recordings.Count == 1 && current.ActionCount == 0,
                "Escape pause suppresses movement fire interaction reset and elapsed time");
            ProcessControls(.05f, pauseKey);
            Verify(screen == EchoScreen.Playing && clock == pausedClock, "Escape resume does not add a hidden frame");
            HandleFocus(false);
            Verify(screen == EchoScreen.Paused, "focus loss pauses gameplay");
            ProcessControls(.05f, pauseKey); ProcessControls(.1f, walk, false);
            Verify(clock == pausedClock && player.transform.position == pausedPosition, "unlocked cursor cannot advance gameplay");

            SetScreen(EchoScreen.Paused); OpenSettings();
            ProcessControls(1, walk);
            Verify(screen == EchoScreen.Settings && paused && clock == pausedClock && recordings.Count == 1,
                "settings opened from pause freeze and preserve current run");
            Checkpoint("settings"); yield return new WaitForSeconds(.25f);
            ProcessControls(.05f, pauseKey);
            Verify(screen == EchoScreen.Paused && recordings.Count == 1 && player.transform.position == pausedPosition,
                "closing pause settings restores pause without rebuilding the sector");
            ReturnToMenu(); OpenSettings(); CloseSettings();
            Verify(screen == EchoScreen.MainMenu, "main menu settings return to main menu");
            bool uiFits = true;
            foreach (Vector2 size in new[] { new Vector2(800, 600), new Vector2(1280, 720), new Vector2(1920, 1080), new Vector2(2560, 1080) })
            {
                Matrix4x4 matrix = UiMatrix(size.x, size.y);
                Vector3 origin = matrix.MultiplyPoint3x4(Vector3.zero), end = matrix.MultiplyPoint3x4(new Vector3(1280, 720, 0));
                uiFits &= origin.x >= 0 && origin.y >= 0 && end.x <= size.x + .01f && end.y <= size.y + .01f &&
                    Mathf.Abs(matrix.m00 - matrix.m11) < .0001f;
            }
            Verify(uiFits, "UI preserves proportions and stays visible at four aspect ratios");

            // Persist test keys only: a user's real options and sector checkpoint are never overwritten by F6.
            const string testPrefix = "EchoProtocol.Verification.";
            string[] keys = { "Sensitivity", "Volume", "FieldOfView", "InvertLook", "Completed", "ResumeLevel", "UnlockedLevel", "ArchiveMask", "SampleMemory", "V2Initialized", "BestSeconds0", "BestSeconds1", "BestSeconds2", "BestLoops0", "BestLoops1", "BestLoops2" };
            var saved = new EchoPreferences(testPrefix);
            try
            {
                saved.Sensitivity = 1.7f; saved.Volume = .35f; saved.FieldOfView = 92;
                saved.InvertLook = true; saved.ResumeLevel = 2; saved.Completed = true; saved.Save();
                var loaded = new EchoPreferences(testPrefix); loaded.Load();
                Verify(Mathf.Abs(loaded.Sensitivity - 1.7f) < .001f && Mathf.Abs(loaded.Volume - .35f) < .001f &&
                    loaded.FieldOfView == 92 && loaded.InvertLook && loaded.ResumeLevel == 2 && loaded.Completed,
                    "settings and checkpoint survive a real PlayerPrefs roundtrip");
                PlayerPrefs.SetFloat(testPrefix + "Sensitivity", float.NaN);
                PlayerPrefs.SetFloat(testPrefix + "Volume", 7);
                PlayerPrefs.SetFloat(testPrefix + "FieldOfView", -4);
                PlayerPrefs.SetInt(testPrefix + "ResumeLevel", 99); loaded.Load();
                Verify(loaded.Sensitivity == 1 && loaded.Volume == 1 && loaded.FieldOfView == 60 && loaded.ResumeLevel == -1,
                    "invalid saved settings and checkpoint are sanitized");
                loaded.ResetSettings();
                Verify(loaded.Sensitivity == 1 && loaded.Volume == .7f && loaded.FieldOfView == 75 && !loaded.InvertLook,
                    "default settings restore valid camera and audio options");
            }
            finally { foreach (string key in keys) PlayerPrefs.DeleteKey(testPrefix + key); PlayerPrefs.Save(); }
            int actualCheckpoint = preferences.ResumeLevel;
            SaveCheckpoint(2);
            Verify(preferences.ResumeLevel == actualCheckpoint, "automatic tests cannot overwrite the user's checkpoint");
            int beforeResume = preferences.ResumeLevel;
            preferences.ResumeLevel = 2; ContinueSavedCampaign();
            Verify(levelIndex == 2 && screen == EchoScreen.Playing && clock == 0 && ghosts.Count == 0 && playerHealth == 100,
                "continue checkpoint starts the selected sector with a fresh world");
            preferences.ResumeLevel = beforeResume;

            LoadLevel(1); SetScreen(EchoScreen.Playing);
            EchoPose visible = spawn; visible.Position = new Vector3(-1, 0, -4); player.SetPose(visible);
            decoyPositions.Clear(); decoyPositions.Add(new Vector3(2.6f, 0, -1.2f));
            for (int i = 0; i < 20; i++) enemies[0].Tick(.1f, player, decoyPositions, DamagePlayer, (a, b) => {});
            Verify(enemies[0].Attacks > 0 && !enemies[0].LastTargetWasEcho && playerHealth < 100,
                "occluded nearby Echo does not hide a visible player from the sentry");
            LoadLevel(1); SetScreen(EchoScreen.Playing);
            EchoPose covered = spawn; covered.Position = new Vector3(4.94f, 0, -1.475f); player.SetPose(covered);
            decoyPositions.Clear(); decoyPositions.Add(new Vector3(-4, 0, -3));
            for (int i = 0; i < 20; i++) enemies[0].Tick(.1f, player, decoyPositions, DamagePlayer, (a, b) => {});
            Verify(enemies[0].Attacks > 0 && enemies[0].LastTargetWasEcho && playerHealth == 100,
                "sentry selects a visible Echo when player is behind cover");
            LoadLevel(1); SetScreen(EchoScreen.Playing);
            EchoPose far = spawn; far.Position = new Vector3(-1, 0, -20); player.SetPose(far);
            ControlEnemySeconds(3);
            Verify(playerHealth == 100 && enemies[0].Attacks == 0, "sentry cannot shoot beyond fourteen metres");
            enemies[0].TakeDamage(-25); DamagePlayer(-25);
            Verify(enemies[0].Health == 60 && playerHealth == 100, "nonpositive damage cannot heal or inflate health");
            enemies[0].TakeDamage(100); player.SetPose(visible); ControlEnemySeconds(3);
            Verify(playerHealth == 100 && enemies[0].Attacks == 0, "dead sentry cannot move or attack");
            NewLoop(); player.SetPose(visible); ControlEnemySeconds(1);
            Verify(playerHealth == 100 && enemies[0].Attacks == 0, "rewind restores the initial sentry attack grace period");
            SetScreen(EchoScreen.Paused); DamagePlayer(12);
            Verify(playerHealth == 100 && !TryFirePlayer(), "pause rejects damage and player shooting");
            SetScreen(EchoScreen.Playing); DamagePlayer(999);
            float deathClock = clock; ProcessControls(.1f, new EchoInputFrame { Move = Vector2.up, Fire = true, Record = true });
            FinishExpiredLoop();
            Verify(screen == EchoScreen.GameLost && playerHealth == 0 && clock == deathClock && ghosts.Count == 0,
                "death freezes gameplay and cannot commit an automatic loop");
            ProcessControls(.05f, confirm);
            Verify(screen == EchoScreen.Playing && playerHealth == 100 && clock == 0 && LivingEnemies() == 1,
                "Enter retries the same sector after death");

            ControlSteps(5, .05f, idle);
            AimAt(enemies[0].transform.position + Vector3.up * .95f);
            ProcessControls(.05f, new EchoInputFrame { Fire = true });
            Verify(enemies[0].Health == 35 && current.ActionCount == 1, "held fire input invokes the real hitscan and recorder");
            Verify(transientBeams.Count > 0, "shots create visible trace objects");
            CommitLoop();
            Verify(transientBeams.Count == 0 && !muzzle.activeSelf && recoil == 0 && hitTimer == 0,
                "rewind clears traces muzzle recoil and hit feedback");
            for (int round = 0; round < 3; round++)
            {
                NewLoop(); SetScreen(EchoScreen.Playing); StepWorld(.15f); StepWorld(.15f);
                Verify(echoShots == 1 && enemies[0].Health == 35, "cursor restarts exactly once on rewind " + (round + 1));
            }
            LoadLevel(2); SetScreen(EchoScreen.Playing);
            for (int i = 0; i < 2; i++)
            {
                Vector3 origin = player.View.transform.position;
                var simultaneous = new ReplayTimeline<EchoPose, EchoAction>();
                simultaneous.AddFrame(0, spawn); simultaneous.AddFrame(.5f, spawn);
                simultaneous.AddAction(0, EchoAction.Fire(origin, (enemies[i].Spawn + Vector3.up * .95f - origin).normalized));
                simultaneous.Freeze(); recordings.Add(simultaneous);
            }
            NewLoop(); StepWorld(.05f);
            Verify(echoShots == 2 && enemies[0].Health == 35 && enemies[1].Health == 35 && current.ActionCount == 0,
                "two Echoes dispatch simultaneous independent shots without recording them again");
            StepWorld(.2f);
            Verify(echoShots == 2 && enemies[0].Health == 35 && enemies[1].Health == 35,
                "simultaneous Echo shots are not duplicated in later frames");
            LoadLevel(0); SetScreen(EchoScreen.Playing);
            platePose.Position = consoles[0].Plate; player.SetPose(platePose); AimAt(consoles[0].Position);
            Interact(); StepWorld(.6f);
            Verify(doorOpen && !doorCollider.enabled, "gate fully opens while a real player holds the powered plate");
            player.SetPose(spawn); StepWorld(.6f);
            Verify(!doorOpen && doorCollider.enabled && doorFraction == 0, "leaving a plate recloses the physical gate");
            LoadLevel(1); SetScreen(EchoScreen.Playing);
            EchoPose exitPose = spawn; exitPose.Position = new Vector3(0, 0, 10);
            player.SetPose(exitPose); doorOpen = true; CheckExit();
            Verify(screen == EchoScreen.Playing && LivingEnemies() == 1, "living sentry prevents premature sector completion");
            enemies[0].TakeDamage(100); CheckExit();
            Verify(screen == EchoScreen.LevelComplete, "cleared sector permits completion with an open gate");
            clock = LoopLength; FinishExpiredLoop();
            Verify(screen == EchoScreen.LevelComplete && recordings.Count == 0,
                "victory at loop deadline cannot reset or erase completion");

            // Entire first-sector solution via input commands and real physics, including both walking routes.
            BeginCampaign();
            ControlSteps(25, .04f, new EchoInputFrame { Move = Vector2.left });
            ControlSteps(31, .04f, walk);
            AimThroughControls(consoles[0].Position); ProcessControls(.05f, new EchoInputFrame { Interact = true });
            ProcessControls(.05f, recordKey);
            Verify(ghosts.Count == 1 && recordings[0].ActionCount == 1, "input-only route records the first-sector console solution");
            for (int i = 0; i < 105 && screen == EchoScreen.Playing; i++)
            {
                ProcessControls(.05f, walk); yield return null;
            }
            Verify(screen == EchoScreen.LevelComplete && player.transform.position.z > 9 && echoActivations == 1,
                "input-only first-sector play reaches the exit while Echo holds the plate");
            ProcessControls(.05f, confirm);
            Verify(levelIndex == 1 && screen == EchoScreen.Playing && ghosts.Count == 0,
                "Enter advances campaign with no previous-sector Echoes");
            IEnumerator route = RecordThroughControls(0, true);
            while (route.MoveNext()) yield return route.Current;
            Verify(ghosts.Count == 1 && recordings[0].ActionCount == 4,
                "input-only second-sector route records console and lethal combat burst");
            route = ExitThroughControls();
            while (route.MoveNext()) yield return route.Current;
            Verify(screen == EchoScreen.LevelComplete && playerHealth > 0 && LivingEnemies() == 0,
                "input-only second sector succeeds with real movement fire and Echo replay");
            ProcessControls(.05f, confirm);
            Verify(levelIndex == 2 && screen == EchoScreen.Playing && ghosts.Count == 0,
                "input-only campaign advances to a clean final sector");
            route = RecordThroughControls(0, true);
            while (route.MoveNext()) yield return route.Current;
            Verify(ghosts.Count == 1 && recordings[0].ActionCount == 7 && !doorOpen,
                "input-only final left route saves both sentry kills and the first console");
            route = RecordThroughControls(1, false);
            while (route.MoveNext()) yield return route.Current;
            Verify(ghosts.Count == 2 && recordings[1].ActionCount == 1 && clock == 0,
                "input-only final right route saves the second console and preserves first Echo");
            route = ExitThroughControls();
            while (route.MoveNext()) yield return route.Current;
            Verify(screen == EchoScreen.GameWon && playerHealth > 0 && LivingEnemies() == 0 && echoActivations == 2,
                "entire input-only campaign reaches victory using two Echoes and physical exits");

            // Repeated destruction/recreation must leave one world, fixed shared assets, and at most two Echoes.
            for (int cycle = 0; cycle < 45; cycle++)
            {
                LoadLevel(cycle % 3); SetScreen(EchoScreen.Playing);
                for (int tape = 0; tape < 4; tape++) { StepWorld(.3f); CommitLoop(); }
                yield return null; yield return null;
                if (ghosts.Count != 2 || recordings.Count != 2 || transform.childCount != 1 ||
                    worldRoot.GetComponentsInChildren<EchoEnemy>(true).Length != Level.Enemies.Length)
                    throw new InvalidOperationException("World/recording objects leaked in reset cycle " + cycle);
            }
            Verify(FindSound(audioBindings.weaponSound, "Weapon/sfx_weapon_pulse_fire").clip != null && ghosts.Count == 2 && transform.childCount == 1,
                "45 sector transitions and 180 loop commits keep active worlds and Echo counts bounded");
            ClearRun(); yield return null; yield return null;
            Verify(transientBeams.Count == 0 && ghosts.Count == 0 && current.FrameCount == 1,
                "soak cleanup leaves no traces or discarded Echo objects");
            LoadLevel(2); SetScreen(EchoScreen.Playing);
            for (int i = 0; i < 2; i++) { StepWorld(.3f); CommitLoop(); }
            float[] frameTimes = new float[120];
            for (int i = 0; i < frameTimes.Length; i++)
            {
                StepWorld(Mathf.Min(Time.deltaTime, .1f));
                yield return null;
                frameTimes[i] = Time.unscaledDeltaTime * 1000;
            }
            Array.Sort(frameTimes);
            PerformanceSummary = "ECHO BENCHMARK (120 frames, final sector, 2 Echoes): median=" + frameTimes[60].ToString("0.0") +
                "ms p95=" + frameTimes[114].ToString("0.0") + "ms Unity allocated=" +
                (UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024)) + "MiB";
            Debug.Log(PerformanceSummary);
        }

        private void ControlEnemySeconds(float seconds)
        {
            decoyPositions.Clear();
            for (int i = 0; i < Mathf.RoundToInt(seconds * 10); i++)
                enemies[0].Tick(.1f, player, decoyPositions, DamagePlayer, (a, b) => {});
        }
    }
}
