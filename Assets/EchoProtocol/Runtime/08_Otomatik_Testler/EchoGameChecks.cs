using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EchoProtocol
{
    public sealed partial class EchoPrototypeGame
    {
        private readonly List<string> passedChecks = new List<string>();
        private string validationError;
        public const int ExpectedCheckCount = 311;
        public string PerformanceSummary { get; private set; }

        public void StartSmokeChecks()
        {
            if (!testing) StartCoroutine(RunSmokeTest());
        }

        private IEnumerator RunSmokeTest()
        {
            EchoSessionSave resumeBeforeTest = resumeSave; resumeSave = null;
            testing = true; SmokeTestFinished = false; SmokeTestPassed = false;
            int checkpointBeforeTest = preferences.ResumeLevel, unlockedBeforeTest = preferences.UnlockedLevel;
            bool completedBeforeTest = preferences.Completed; int archiveBeforeTest = preferences.ArchiveMask;
            bool sampleMemoryBeforeTest = preferences.SampleMemory;
            float[] bestSecondsBeforeTest = (float[])preferences.BestSeconds.Clone();
            int[] bestLoopsBeforeTest = (int[])preferences.BestLoops.Clone();
            PerformanceSummary = null;
            SmokeTestCount = 0; passedChecks.Clear(); SmokeTestDetails = "Running";
            validationError = null;
            Application.logMessageReceived += ObserveValidationError;
            IEnumerator steps = RunGameChecks();
            string failure = null;
            while (true)
            {
                bool advanced = false;
                try { advanced = steps.MoveNext(); }
                catch (Exception ex) { failure = ex.ToString(); }
                if (validationError != null) failure = validationError;
                if (failure != null || !advanced) break;
                yield return steps.Current;
            }
            SmokeTestCount = passedChecks.Count;
            Application.logMessageReceived -= ObserveValidationError;
            if (failure == null && SmokeTestCount != ExpectedCheckCount)
                failure = "Expected " + ExpectedCheckCount + " checks, ran " + SmokeTestCount;
            SmokeTestPassed = failure == null;
            SmokeTestDetails = string.Join("; ", passedChecks.ToArray()) + "\n" + PerformanceSummary + (failure == null ? "" : "\n" + failure);
            SmokeTestSummary = SmokeTestPassed
                ? "PASS: " + SmokeTestCount + "/" + ExpectedCheckCount + " Unity game checks"
                : "FAIL after " + SmokeTestCount + " checks: " + failure;
            preferences.ResumeLevel = checkpointBeforeTest; preferences.UnlockedLevel = unlockedBeforeTest;
            preferences.ArchiveMask = archiveBeforeTest; preferences.Completed = completedBeforeTest; preferences.BestSeconds = bestSecondsBeforeTest; preferences.BestLoops = bestLoopsBeforeTest;
            preferences.SampleMemory = sampleMemoryBeforeTest;
            ApplyPreferences(); ReturnToMenu(); resumeSave = resumeBeforeTest; testing = false; SmokeTestFinished = true;
            testResult = SmokeTestSummary;
            if (SmokeTestPassed) Debug.Log("ECHO SMOKE PASS: " + SmokeTestSummary + "\n" + SmokeTestDetails);
            else Debug.LogError("ECHO SMOKE FAIL: " + SmokeTestDetails);
        }

        private void ObserveValidationError(string text, string trace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                validationError = text + "\n" + trace;
        }

        private void Verify(bool condition, string name)
        {
            if (!condition) throw new InvalidOperationException("FAIL: " + name + " | clock=" + clock + " player=" + player.transform.position + " health=" + playerHealth + " screen=" + screen + " door=" + doorOpen + " living=" + LivingEnemies() + " echoShots=" + echoShots);
            passedChecks.Add(name); testResult = passedChecks.Count + " / " + name;
            Debug.Log("ECHO CHECK PASS: " + name);
        }

        private void Checkpoint(string name)
        {
            if (SmokeCheckpoint != null) SmokeCheckpoint(name);
        }

        private void AimAt(Vector3 target)
        {
            EchoPose pose = player.Pose;
            Vector3 direction = target - player.View.transform.position;
            pose.Rotation = Quaternion.Euler(0, Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg, 0);
            pose.Pitch = -Mathf.Atan2(direction.y, new Vector2(direction.x, direction.z).magnitude) * Mathf.Rad2Deg;
            player.SetPose(pose); Physics.SyncTransforms();
        }

        private IEnumerator MoveToPlate(int index)
        {
            EchoPose initial = player.Pose;
            for (int i = 1; i <= 40; i++)
            {
                EchoPose pose = initial;
                pose.Position = Vector3.Lerp(initial.Position, consoles[index].Plate + Vector3.up * .02f, i / 40f);
                player.SetPose(pose); StepWorld(.05f);
                yield return new WaitForSeconds(.01f);
            }
        }

        private IEnumerator WalkThroughGate(string capture)
        {
            for (int i = 0; i < 100; i++)
            {
                player.Controller.Move(new Vector3(0, -2, 4) * .05f);
                StepWorld(.05f); CheckExit();
                if (won)
                {
                    Checkpoint(capture);
                    yield return new WaitForSeconds(.3f);
                    yield break;
                }
                yield return new WaitForSeconds(.01f);
            }
        }

        private IEnumerator RecordLeftSolution(int index)
        {
            LoadLevel(index); SetScreen(EchoScreen.Playing);
            IEnumerator move = MoveToPlate(0);
            while (move.MoveNext()) yield return move.Current;
            AimAt(consoles[0].Position); Interact();
            foreach (EchoEnemy enemy in enemies)
            {
                for (int shot = 0; shot < 3; shot++)
                {
                    AimAt(enemy.transform.position + Vector3.up * .95f);
                    if (!TryFirePlayer()) throw new InvalidOperationException("Solution shot rejected.");
                    StepWorld(.25f); yield return new WaitForSeconds(.015f);
                }
                if (enemy.Alive) throw new InvalidOperationException("Solution ray did not kill the sentry.");
            }
            for (int i = 0; i < 10; i++) { StepWorld(.05f); yield return new WaitForSeconds(.01f); }
            CommitLoop();
            for (int i = 0; i < 100; i++) { StepWorld(.05f); yield return new WaitForSeconds(.01f); }
        }

        private IEnumerator RunGameChecks()
        {
            Debug.Log("ECHO TIMELINE: " + TimelineSelfChecks.Run());
            // Keep the six original live checks before checking the new campaign.
            BeginCampaign();
            IEnumerator move = MoveToPlate(0);
            while (move.MoveNext()) yield return move.Current;
            AimAt(consoles[0].Position); Interact();
            for (int i = 0; i < 10; i++) { StepWorld(.05f); yield return new WaitForSeconds(.01f); }
            Verify(power && current.ActionCount == 1 && current.FrameCount > 30 && doorOpen, "core record and console raycast");
            CommitLoop();
            ReplayTimeline<EchoPose, EchoAction> firstTape = recordings[0];
            Verify(!power && !doorOpen && doorCollider.enabled && clock == 0 && ghosts.Count == 1 &&
                Vector3.Distance(player.transform.position, spawn.Position) < .01f, "core world rewind");
            for (int i = 0; i < 60; i++) { StepWorld(.05f); yield return new WaitForSeconds(.01f); }
            Verify(power && plateHeld && doorOpen && !doorCollider.enabled && echoActivations == 1, "core replay and final plate hold");
            Checkpoint("level-1-echo-open-gate"); yield return new WaitForSeconds(.25f);
            IEnumerator cross = WalkThroughGate("level-1-complete");
            while (cross.MoveNext()) yield return cross.Current;
            Verify(screen == EchoScreen.LevelComplete && player.transform.position.z > 9, "core physical gate and completion");
            CommitLoop(); StepWorld(.5f); CommitLoop();
            Verify(ghosts.Count == 2 && recordings.Count == 2 && recordings[0] != firstTape && clock == 0 && !power, "core oldest recording removed at cap two");
            ClearRun();
            Verify(ghosts.Count == 0 && recordings.Count == 0 && !power && current.ActionCount == 0, "core clear recordings");

            StepWorld(.5f); CommitLoop(); SetScreen(EchoScreen.LevelComplete); ContinueCampaign();
            Verify(levelIndex == 1 && ghosts.Count == 0 && recordings.Count == 0 && current.FrameCount == 1 && enemies.Count == 1,
                "level transition clears previous recordings");

            AimAt(enemies[0].transform.position + Vector3.up * .95f);
            Vector3 shotOrigin = player.View.transform.position, shotDirection = player.View.transform.forward;
            Verify(TryFirePlayer() && enemies[0].Health == 35, "player hitscan damages real sentry collider");
            Checkpoint("combat"); yield return new WaitForSeconds(.25f);
            Verify(!TryFirePlayer() && enemies[0].Health == 35 && current.ActionCount == 1, "weapon cooldown rejects duplicate shot");
            current.Freeze(); bool exactShot = false;
            current.CreateCursor().Advance(0, (time, action) => exactShot = action.Kind == EchoActionKind.Fire &&
                Vector3.Distance(action.Origin, shotOrigin) < .0001f && Vector3.Angle(action.Direction, shotDirection) < .01f);
            Verify(exactShot, "recorded shot retains original origin and direction");
            recordings.Add(current); playerHealth = 12;
            enemies[0].transform.position += Vector3.right * 2;
            NewLoop(); SetScreen(EchoScreen.Playing);
            Verify(playerHealth == 100 && enemies[0].Health == 60 && Vector3.Distance(enemies[0].transform.position, enemies[0].Spawn) < .01f,
                "rewind restores player health and sentry state");
            StepWorld(.05f);
            Verify(echoShots == 1 && enemies[0].Health == 35, "Echo time-zero shot uses real combat raycast");
            StepWorld(.5f);
            Verify(echoShots == 1 && enemies[0].Health == 35, "Echo shot cursor prevents duplicate damage");
            ClearRun();
            Verify(ghosts.Count == 0 && echoShots == 0 && enemies[0].Health == 60, "clear also resets combat state");
            for (int i = 0; i < 3; i++)
            {
                AimAt(enemies[0].transform.position + Vector3.up * .95f);
                TryFirePlayer(); StepWorld(.25f);
            }
            Verify(LivingEnemies() == 0 && !enemies[0].gameObject.activeSelf && current.ActionCount == 3, "three carbine shots kill sentry");
            CommitLoop();
            Verify(enemies[0].Alive && enemies[0].Health == 60 && enemies[0].gameObject.activeSelf, "dead sentry respawns on loop rewind");
            for (int i = 0; i < 20; i++) { StepWorld(.05f); yield return new WaitForSeconds(.01f); }
            Verify(echoShots == 3 && LivingEnemies() == 0, "Echo replays complete lethal burst");

            LoadLevel(1); SetScreen(EchoScreen.Playing);
            EchoPose visible = spawn; visible.Position = new Vector3(-1, .02f, -4); player.SetPose(visible);
            for (int i = 0; i < 15; i++) StepWorld(.1f);
            Verify(playerHealth < 100 && enemies[0].Attacks > 0, "sentry attacks visible player");
            LoadLevel(1); SetScreen(EchoScreen.Playing);
            EchoPose covered = spawn; covered.Position = new Vector3(4.94f, .02f, -1.475f); player.SetPose(covered);
            for (int i = 0; i < 15; i++) StepWorld(.1f);
            Verify(playerHealth == 100 && enemies[0].Attacks == 0, "cover blocks sentry line of sight");
            AimAt(enemies[0].transform.position + Vector3.up * .95f); TryFirePlayer();
            Verify(enemies[0].Health == 60, "cover blocks player hitscan too");

            LoadLevel(1); SetScreen(EchoScreen.Playing);
            var decoy = new ReplayTimeline<EchoPose, EchoAction>();
            EchoPose closeEcho = spawn; closeEcho.Position = new Vector3(-1, .02f, -.3f);
            decoy.AddFrame(0, closeEcho); decoy.AddFrame(1, closeEcho); decoy.Freeze();
            recordings.Add(decoy); NewLoop(); SetScreen(EchoScreen.Playing);
            for (int i = 0; i < 15; i++) StepWorld(.1f);
            Verify(enemies[0].Attacks > 0 && enemies[0].LastTargetWasEcho && playerHealth == 100, "closer Echo draws sentry fire");
            ClearRun(); player.SetPose(visible);
            for (int i = 0; i < 150 && playerHealth > 0; i++) StepWorld(.1f);
            Verify(playerHealth == 0 && screen == EchoScreen.GameLost && paused && !won, "sentry fire causes loss screen");
            Checkpoint("loss"); yield return new WaitForSeconds(.25f);
            RetryLevel();
            Verify(playerHealth == 100 && LivingEnemies() == 1 && ghosts.Count == 0 && clock == 0 && screen == EchoScreen.Playing,
                "retry resets health enemies recordings and clock");
            SetScreen(EchoScreen.Paused);
            Vector3 pausedEnemy = enemies[0].transform.position;
            AdvancePlayingWorld(2);
            Verify(clock == 0 && enemies[0].transform.position == pausedEnemy && playerHealth == 100, "pause freezes timeline and sentries");
            SetScreen(EchoScreen.Playing); AdvancePlayingWorld(.05f);
            Verify(clock > 0, "resume advances the world again");
            ReturnToMenu(); SetScreen(EchoScreen.Controls);
            Verify(screen == EchoScreen.Controls && paused && weaponRoot != null && levelIndex == 0, "controls and main menu state");
            Checkpoint("controls"); yield return new WaitForSeconds(.25f);

            IEnumerator solve = RecordLeftSolution(1);
            while (solve.MoveNext()) yield return solve.Current;
            Verify(doorOpen && plateHeld && ghosts.Count == 1 && LivingEnemies() == 0 && echoShots == 3 && echoActivations == 1,
                "sector two Echo replays console combat and plate");
            cross = WalkThroughGate("level-2-complete");
            while (cross.MoveNext()) yield return cross.Current;
            Verify(screen == EchoScreen.LevelComplete && levelIndex == 1, "sector two physically completed");
            ContinueCampaign();
            Verify(levelIndex == 2 && ghosts.Count == 0 && LivingEnemies() == 2 && consoles.Count == 2 && playerHealth == 100,
                "final sector starts with clean state and two objectives");
            solve = RecordLeftSolution(2);
            while (solve.MoveNext()) yield return solve.Current;
            Verify(ghosts.Count == 1 && consoles[0].Powered && consoles[0].Held && !doorOpen && LivingEnemies() == 0,
                "one Echo cannot open the final dual-plate gate");
            move = MoveToPlate(1);
            while (move.MoveNext()) yield return move.Current;
            AimAt(consoles[1].Position); Interact();
            for (int i = 0; i < 10; i++) { StepWorld(.05f); yield return new WaitForSeconds(.01f); }
            CommitLoop();
            for (int i = 0; i < 180; i++) { StepWorld(.05f); yield return new WaitForSeconds(.01f); }
            Verify(ghosts.Count == 2 && doorOpen && plateHeld && power && echoActivations == 2 && LivingEnemies() == 0,
                "two independent Echoes power both consoles hold both plates and defeat sentries");
            Checkpoint("level-3-dual-echo"); yield return new WaitForSeconds(.25f);
            cross = WalkThroughGate("win");
            while (cross.MoveNext()) yield return cross.Current;
            Verify(screen == EchoScreen.GameWon && won && playerHealth > 0 && player.transform.position.z > 9,
                "final physical exit reaches campaign victory screen");
            BeginCampaign();
            Verify(levelIndex == 0 && ghosts.Count == 0 && playerHealth == 100 && !power && clock == 0,
                "replay campaign starts clean at sector one");
            StepWorld(LoopLength); FinishExpiredLoop();
            Verify(clock == 0 && ghosts.Count == 1 && recordings.Count == 1, "twenty-second deadline automatically commits a loop");
            IEnumerator detailed = RunDetailedChecks();
            while (detailed.MoveNext()) yield return detailed.Current;
            IEnumerator playability = RunPlayabilityChecks();
            while (playability.MoveNext()) yield return playability.Current;
            IEnumerator session = RunSessionChecks();
            while (session.MoveNext()) yield return session.Current;
            IEnumerator immersion = RunImmersionChecks();
            while (immersion.MoveNext()) yield return immersion.Current;
            IEnumerator sample = RunSampleChecks();
            while (sample.MoveNext()) yield return sample.Current;
            IEnumerator guidance = RunGuidanceChecks();
            while (guidance.MoveNext()) yield return guidance.Current;
            IEnumerator tactics = RunTacticalChecks();
            while (tactics.MoveNext()) yield return tactics.Current;
        }
    }
}
