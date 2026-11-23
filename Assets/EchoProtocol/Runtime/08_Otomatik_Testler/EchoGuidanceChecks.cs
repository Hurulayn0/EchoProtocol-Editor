using System.Collections;
using UnityEngine;

namespace EchoProtocol
{
    public sealed partial class EchoPrototypeGame
    {
        private IEnumerator RunGuidanceChecks()
        {
            BeginSample();
            Verify(echoTeaching && planning && clock == 0 && recordings.Count == 0, "sample opens with explicit explanation before any timer or recording starts");
            ProcessControls(1, new EchoInputFrame { Move = Vector2.up, Record = true, Retry = true, Fire = true });
            Verify(echoTeaching && clock == 0 && current.ActionCount == 0 && player.Pose.Position == SampleSpawn.Position, "teaching blocks movement firing record and retry without consuming the first attempt");
            Checkpoint("echo-teaching"); yield return new WaitForSeconds(.25f);
            ProcessControls(0, new EchoInputFrame { Confirm = true });
            Verify(!echoTeaching && !planning && clock == 0 && screen == EchoScreen.Playing, "Enter starts the lesson with immediate control at the first decision");
            ProcessControls(.1f, new EchoInputFrame { Record = true });
            Verify(!planning && recordings.Count == 0 && clock == 0, "empty immediate record is rejected instead of creating a confusing Echo");
            ControlSteps(10, .05f, new EchoInputFrame { Move = Vector2.left });
            ProcessControls(0, new EchoInputFrame { Record = true });
            Verify(recordings.Count == 1 && ghosts.Count == 1 && clock == 0 && SampleRecordPreview().Contains("Echo 2"), "first R creates automatic playback and previews the next free slot");
            ControlSteps(10, .05f, new EchoInputFrame { Move = Vector2.right });
            ProcessControls(0, new EchoInputFrame { Record = true });
            Verify(recordings.Count == 2 && ghosts.Count == 2 && !planning, "second R creates the second Echo without asking for a replacement");
            var first = recordings[0]; var second = recordings[1]; var firstSave = savedTapes[0];
            StepWorld(.7f); var attempt = current; float before = clock; int loopsBefore = savedLoops;
            ProcessControls(0, new EchoInputFrame { Record = true });
            Verify(pendingEchoRecord && planning && current == attempt && !current.Frozen && recordings[0] == first && recordings[1] == second, "full sample R freezes for a choice without silently deleting either Echo");
            Vector3 ghostPosition = ghosts[0].Body.transform.position; planPreviewTime = 19;
            recordings[0].Sample(planPreviewTime, EchoPose.Lerp); AdvancePlayingWorld(4);
            Verify(clock == before && ghosts[0].Body.transform.position == ghostPosition && current == attempt && savedLoops == loopsBefore, "preview sampling never executes events moves real Echoes or modifies the live attempt");
            ChooseEchoReplacement(9);
            Verify(pendingEchoRecord && planning && recordings[0] == first && recordings[1] == second, "invalid replacement selection cannot destroy a recording");
            Checkpoint("echo-record-choice"); yield return new WaitForSeconds(.25f);
            ProcessControls(0, new EchoInputFrame { Plan = true });
            Verify(!pendingEchoRecord && !planning && current == attempt && clock == before && !current.Frozen, "Tab cancels replacement and resumes the same unfinished attempt");
            ProcessControls(0, new EchoInputFrame { Record = true }); ChooseEchoReplacement(1);
            Verify(recordings[0] == first && savedTapes[0] == firstSave && recordings[1] == attempt && ghosts.Count == 2 && clock == 0, "choosing Echo two preserves Echo one and commits only the selected slot");
            Verify(second.Frozen && Mathf.Abs(second.Duration - .5f) < .001f && !pendingEchoRecord && !planning && savedLoops == loopsBefore + 1, "replacement retains immutable old tape data and completes one rewind only");
            StepWorld(.6f); var replacement = current; var retained = recordings[1];
            ProcessControls(0, new EchoInputFrame { Plan = true }); ChooseEchoReplacement(0);
            Verify(!planning && replacementEchoSlot == 0 && recordings[0] == first && recordings[1] == retained && current == replacement, "planner can arm an explicit replacement while preserving both original tapes until R");
            ProcessControls(0, new EchoInputFrame { Record = true });
            Verify(recordings[0] == replacement && recordings[1] == retained && replacementEchoSlot == -1 && clock == 0, "armed R replaces Echo one while preserving Echo two and clears the pending choice");
            var saved = CloneSave(CaptureSession()); RestoreSession(saved);
            Verify(saved.Version == 2 && saved.Layout == 1 && recordings.Count == 2 && !echoTeaching && !pendingEchoRecord && replacementEchoSlot == -1, "new guidance needs no save version change and resume restores two automatic Echoes safely");
            first = recordings[0]; second = recordings[1]; StepWorld(.8f); playerHealth = 24;
            sampleServiceUntil = clock + 1; loopsBefore = savedLoops;
            ProcessControls(.1f, new EchoInputFrame { Retry = true, Record = true, Fire = true });
            Verify(recordings[0] == first && recordings[1] == second && savedLoops == loopsBefore && clock == 0 && playerHealth == 100 && sampleServiceUntil == 0, "T has priority over record and fire and resets health and devices while preserving both plans");
            ProcessControls(0, new EchoInputFrame { Plan = true }); ProcessControls(0, new EchoInputFrame { Retry = true });
            Verify(!planning && clock == 0 && recordings[0] == first && recordings[1] == second, "same-plan retry works from the unlocked planning screen");
            StepWorld(.3f); SetScreen(EchoScreen.Paused); before = clock;
            ProcessControls(.1f, new EchoInputFrame { Retry = true });
            Verify(screen == EchoScreen.Paused && clock == before && recordings[0] == first, "pause blocks retry instead of unexpectedly releasing controls");
            SetScreen(EchoScreen.Playing); ProcessControls(0, new EchoInputFrame { Undo = true });
            Verify(recordings.Count == 1 && recordings[0] == first && clock == 0, "existing Q still removes only the latest slot after targeted replacements");
            ClearRun(); StepWorld(.4f); ProcessControls(0, new EchoInputFrame { Retry = true });
            Verify(recordings.Count == 0 && clock == 0 && current.ActionCount == 0 && !planning, "T also safely discards a first unfinished attempt without creating any Echo");
            StepWorld(.3f); CommitLoop(); StepWorld(.4f); CommitLoop(); StepWorld(.5f);
            ProcessControls(0, new EchoInputFrame { Record = true }); SetScreen(EchoScreen.Paused); ChooseEchoReplacement(0);
            Verify(screen == EchoScreen.Paused && !pendingEchoRecord && recordings.Count == 2 && clock == .5f, "leaving a replacement dialog invalidates stale choices without modifying saved Echoes");
            RestoreSession(CloneSave(CaptureSession()));
            Verify(!planning && !echoTeaching && clock == 0 && recordings.Count == 2 && current.ActionCount == 0, "loading after cancelled replacement starts a clean loop from completed tapes only");
            ClearRun(); StepWorld(LoopLength); FinishExpiredLoop();
            Verify(planning && pendingEchoRecord && clock == LoopLength && recordings.Count == 0 && !current.Frozen, "sample deadline asks before creating an unrequested first Echo");
            Checkpoint("echo-deadline-choice"); yield return new WaitForSeconds(.25f);
            ProcessControls(0, new EchoInputFrame { Plan = true });
            Verify(!planning && !pendingEchoRecord && clock == 0 && recordings.Count == 0, "Tab discards a completed unaccepted attempt instead of trapping the player in a deadline dialog");
            StepWorld(LoopLength); FinishExpiredLoop();
            CommitReviewedNewEcho();
            Verify(!planning && !pendingEchoRecord && clock == 0 && recordings.Count == 1 && recordings[0].Duration == LoopLength, "explicit deadline acceptance creates exactly one complete Echo");
            first = recordings[0]; StepWorld(LoopLength); FinishExpiredLoop(); CommitReviewedNewEcho();
            Verify(recordings.Count == 2 && recordings[0] == first && recordings[1].Duration == LoopLength && clock == 0, "second deadline acceptance preserves the first plan and fills only the empty slot");
            LoadLevel(0); SetScreen(EchoScreen.MainMenu);
            yield return null;
        }
    }
}
