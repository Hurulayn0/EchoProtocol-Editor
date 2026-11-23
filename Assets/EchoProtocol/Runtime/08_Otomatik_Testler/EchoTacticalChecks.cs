using System.Collections;
using UnityEngine;

namespace EchoProtocol
{
    public sealed partial class EchoPrototypeGame
    {
        private IEnumerator RunTacticalChecks()
        {
            LoadSampleStage(2); sampleCompleted = 3; SetScreen(EchoScreen.Playing);
            Transform freightCollider = worldRoot.Find("Freight barricade collider");
            Verify(sampleDevices.Count == 3 && sampleDevices[0].StandbyMaterial == powerOff && sampleDevices[1].StandbyMaterial == objectiveOff && sampleDevices[2].StandbyMaterial == objectiveOff && sampleDevices[1].StandbyMaterial != ghostCyan && sampleDevices[2].StandbyMaterial != ghostPurple && CountSampleObjects("Art / Prop_Crate3") == 4 && freightCollider != null && freightCollider.GetComponent<Collider>().enabled && freightCollider.localScale == new Vector3(3.3f, 2.4f, .9f),
                "evacuation terminal colors remain distinct from Echo identity and preserve the four-crate tactical collider");
            EchoEnemy guard = enemies[0];
            Verify(!guard.HasTarget && guard.LastHitActor == -1 && teamHits == 0, "fresh tactical loop has no stale target or cooperative hit state");
            EchoPose left = SampleSpawn; left.Position = new Vector3(-3, .02f, -1);
            player.SetPose(left); RecordPose(0, player.Pose);
            WaitSampleUntil(.9f); AimAt(guard.transform.position + Vector3.up * .95f); TryFirePlayer();
            Verify(guard.Health == 35 && teamHits == 0 && guard.LastHitActor == 0, "one player body shot does normal damage without a solo synergy bonus");
            WaitSampleUntil(1.1f); CommitLoop();
            EchoPose right = SampleSpawn; right.Position = new Vector3(3, .02f, -1); player.SetPose(right);
            TickEchoTactics(0); EchoSavedAction upcoming;
            Verify(UpcomingEchoShot(0, out upcoming) && Mathf.Abs(upcoming.Time - .9f) < .001f && ghosts[0].ShotPreview.enabled, "recorded shot has a visible one-second anticipation without changing its timestamp");
            Verify(guard.Health == 60 && current.ActionCount == 0 && echoShots == 0 && clock == 0, "shot anticipation does not fire apply damage or record player input");
            var tape = recordings[0]; int frames = tape.FrameCount, actions = tape.ActionCount;
            WaitSampleUntil(.8f); AimAt(guard.transform.position + Vector3.up * .95f);
            Checkpoint("echo-shot-anticipation"); yield return new WaitForSeconds(.25f);
            TogglePlanning(); float before = clock;
            AdvancePlayingWorld(2);
            Verify(planning && clock == before && guard.Health == 60 && recordings[0] == tape && tape.FrameCount == frames && tape.ActionCount == actions, "planning preserves anticipation timing and immutable replay during pause");
            TogglePlanning(); WaitSampleUntil(.92f);
            Verify(guard.Health == 35 && echoShots == 1 && guard.LastHitActor == 1 && teamHits == 0 && !ghosts[0].ShotPreview.enabled, "real replayed shot opens its actor-specific response window and removes preview after execution");
            Checkpoint("echo-shot-response"); yield return new WaitForSeconds(.25f);
            AimAt(guard.transform.position + Vector3.up * .95f); TryFirePlayer();
            Verify(!guard.Alive && teamHits == 1 && teamHitTimer > 0 && killTimer > 0, "player joins actual recorded Echo body shot to defeat a guard in two cooperative hits");
            Verify(sampleFinalEvents[sampleFinalEvents.Count - 1].Contains("EŞZAMANLI VURUŞ") && current.ActionCount == 1, "cooperative finish has readable event feedback and records only the player's own shot");
            Checkpoint("echo-team-hit"); yield return new WaitForSeconds(.25f);
            RetryLevel(); guard = enemies[0];
            Verify(guard.Health == 60 && guard.LastHitActor == -1 && teamHits == 0 && teamHitTimer == 0 && recordings[0] == tape, "retry clears combat opportunity and effects while retaining the exact Echo plan");
            var save = CloneSave(CaptureSession()); RestoreSession(save); guard = enemies[0];
            Verify(save.Version == 2 && save.Tapes.Length == 1 && guard.LastHitActor == -1 && clock == 0 && recordings[0].ActionCount == 1, "loading keeps existing v2 recording schema and starts fresh cooperative combat state");
            EchoPose intercept = SampleSpawn; intercept.Position = new Vector3(-1.3f, .02f, .5f); player.SetPose(intercept);
            WaitSampleUntil(.92f);
            Verify(guard.Health == 60 && echoShots == 1 && teamHits == 0 && message.Contains("ATIŞ HATTI ENGELLİ") && message.Contains("Kendi rotanı hattın dışına taşı"), "player blocking a recorded combat shot prevents damage and explains how to clear the line");
            AimAt(guard.transform.position + Vector3.up * .95f); TryFirePlayer();
            Verify(guard.Health == 35 && teamHits == 0, "blocked Echo shot cannot seed a false cooperative bonus for a later player hit");
            guard.ResetEnemy(); guard.RegisterTeamHit(1, 1);
            Verify(guard.RegisterTeamHit(0, 2.2f), "response boundary accepts exactly 1.2 seconds despite float clock precision");
            guard.ResetEnemy();
            Verify(!guard.RegisterTeamHit(0, 1) && !guard.RegisterTeamHit(0, 1.2f), "same actor cannot obtain a cooperative bonus by rapid repeat hits");
            guard.ResetEnemy(); guard.RegisterTeamHit(1, 1);
            Verify(!guard.RegisterTeamHit(0, 2.21f), "a player shot outside the real response window gives no synchronization bonus");
            guard.ResetEnemy(); guard.RegisterTeamHit(1, 1);
            Verify(guard.RegisterTeamHit(2, 2.19f), "two distinct Echo slots can cooperate within the same response window");
            Verify(!guard.RegisterTeamHit(0, 2.2f), "third actor cannot repeatedly farm an already awarded response window");
            guard.ResetEnemy();
            Verify(!guard.RegisterTeamHit(-1, 1) && !guard.RegisterTeamHit(3, 1) && !guard.RegisterTeamHit(0, float.NaN) && guard.LastHitActor == -1, "invalid or unknown hit actors and nonfinite timestamps never create a valid opportunity");
            EchoPose hidden = SampleSpawn; hidden.Position = new Vector3(5, .02f, -6); player.SetPose(hidden);
            decoyPositions.Clear(); decoyPositions.Add(new Vector3(-3, .02f, -1));
            guard.Tick(0, player, decoyPositions, DamagePlayer, (a, b) => {});
            Verify(guard.HasTarget && guard.TargetIsEcho && guard.Attacks == 0 && !guard.LastTargetWasEcho && GuardIntent(guard).Contains("ECHO"), "guard reports current Echo focus before firing instead of a stale previous attack");
            Checkpoint("echo-guard-focus"); yield return new WaitForSeconds(.25f);
            guard.Stagger(.8f);
            Verify(!guard.HasTarget && !guard.Charging && GuardIntent(guard).Contains("SERSEMLEDİ"), "stagger immediately clears threat intent and communicates the available opening");
            for (int i = 0; i < 12; i++) { RetryLevel(); TickEchoTactics(0); yield return null; }
            Verify(worldRoot.GetComponentsInChildren<LineRenderer>(true).Length == 2 && teamHits == 0 && recordings.Count == 1 && recordings[0].ActionCount == actions,
                "twelve retries with visible anticipation keep only one ghost preview and one guard laser without changing replay data");
            ClearRun(); player.SetPose(right); WaitSampleUntil(.5f);
            guard = enemies[0]; AimAt(guard.transform.position + Vector3.up * .95f); TryFirePlayer();
            WaitSampleUntil(.8f); AimAt(guard.transform.position + Vector3.up * .95f); TryFirePlayer();
            Verify(guard.Health == 10 && teamHits == 0, "two real same-player shots retain normal fifty damage and the old solo alternative");
            WaitSampleUntil(1.1f); right.Position = new Vector3(2, .02f, 1); player.SetPose(right);
            AimAt(guard.transform.position + Vector3.up * .95f); TryFirePlayer();
            Verify(!guard.Alive && teamHits == 0, "normal three-shot solo combat still works without Echo cooperation");
            LoadSampleStage(0); SetScreen(EchoScreen.Playing);
            Verify(teamHitTimer == 0 && teamHits == 0 && ghosts.Count == 0 && sampleDevices.Count == 1, "checkpoint rebuild never leaks combat bonuses or preview renderers into the next puzzle");
            LoadLevel(0); SetScreen(EchoScreen.MainMenu); yield return null;
        }
    }
}
