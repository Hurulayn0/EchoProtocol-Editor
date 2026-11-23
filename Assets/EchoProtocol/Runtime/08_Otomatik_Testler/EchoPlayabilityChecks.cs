using System.Collections;
using UnityEngine;

namespace EchoProtocol
{
    public sealed partial class EchoPrototypeGame
    {
        private IEnumerator RunPlayabilityChecks()
        {
            BeginCampaign();
            ControlSteps(20, .05f, new EchoInputFrame { Move = Vector2.up, Sprint = true });
            Verify(Mathf.Abs(player.transform.position.z - spawn.Position.z - 6) < .04f, "sprint travels six metres per second with the real controller");
            NewLoop(); ControlSteps(20, .05f, new EchoInputFrame { Move = Vector2.one, Sprint = true });
            Vector3 delta = player.transform.position - spawn.Position; delta.y = 0;
            Verify(Mathf.Abs(delta.magnitude - 6) < .04f, "diagonal sprint retains the six metre speed limit");
            NewLoop(); ControlSteps(20, .05f, new EchoInputFrame { Move = Vector2.up, Aim = true, Sprint = true });
            Verify(Mathf.Abs(player.transform.position.z - spawn.Position.z - 2.6f) < .04f, "aiming takes priority over sprint and slows movement");
            Verify(Mathf.Abs(player.View.fieldOfView - preferences.FieldOfView * .8f) < .02f, "aim zoom converges relative to the selected field of view");
            float yawBefore = player.transform.eulerAngles.y;
            ProcessControls(.01f, new EchoInputFrame { Aim = true, Look = new Vector2(20, 0) });
            Verify(Mathf.Abs(Mathf.DeltaAngle(yawBefore, player.transform.eulerAngles.y) - 13 * player.LookSensitivity) < .01f, "aiming reduces mouse sensitivity for precise shots");
            ControlSteps(20, .05f, new EchoInputFrame());
            Verify(!player.IsAiming && Mathf.Abs(player.View.fieldOfView - preferences.FieldOfView) < .02f, "releasing aim restores normal view and sensitivity mode");
            Vector3 authoredWeaponPosition = weaponRoot.localPosition;
            ProcessControls(.05f, new EchoInputFrame { Aim = true }); killTimer = .5f; NewLoop();
            Verify(!player.IsAiming && killTimer == 0 && player.View.fieldOfView == preferences.FieldOfView && weaponRoot.localPosition == authoredWeaponPosition && weaponMotion.localPosition == motionRestPosition, "rewind restores aim camera weapon position and kill feedback together");
            ControlSteps(20, .05f, new EchoInputFrame { Move = Vector2.up, Sprint = true }); CommitLoop(); StepWorld(1);
            Verify(Vector3.Distance(ghosts[0].Body.transform.position, recordings[0].Sample(1, EchoPose.Lerp).Position) < .001f && ghosts[0].Body.transform.position.z > -1.1f,
                "Echo replays a sprint route at its recorded speed");
            ClearRun(); StepWorld(.4f); var inProgress = current; float previousClock = clock;
            ProcessControls(.05f, new EchoInputFrame { Undo = true });
            Verify(current == inProgress && clock == previousClock && ghosts.Count == 0, "undo with no Echo preserves the current attempt");
            CommitLoop(); var first = recordings[0]; StepWorld(.5f); CommitLoop();
            ProcessControls(.05f, new EchoInputFrame { Undo = true });
            Verify(recordings.Count == 1 && recordings[0] == first && ghosts.Count == 1 && clock == 0, "undo removes only the newest Echo and replays the older recording");
            Verify(first.Frozen && first.Duration == .4f && first.ActionCount == 0, "undo does not mutate an earlier immutable tape");
            StepWorld(.4f); ProcessControls(.05f, new EchoInputFrame { Undo = true, Record = true });
            Verify(recordings.Count == 0 && clock == 0, "undo has priority over a simultaneous record command");
            StepWorld(.4f); CommitLoop(); StepWorld(.4f); CommitLoop();
            ProcessControls(.05f, new EchoInputFrame { Clear = true, Undo = true });
            Verify(recordings.Count == 0 && ghosts.Count == 0, "clear has priority over a simultaneous undo command");
            StepWorld(.4f); CommitLoop(); SetScreen(EchoScreen.Paused);
            ProcessControls(.05f, new EchoInputFrame { Undo = true });
            Verify(recordings.Count == 1 && screen == EchoScreen.Paused, "pause blocks undo and retains stored recordings");
            SetScreen(EchoScreen.Playing); StepWorld(.4f); CommitLoop();
            first = recordings[0]; var second = recordings[1]; int loopsBefore = savedLoops;
            StepWorld(LoopLength); FinishExpiredLoop();
            Verify(recordings.Count == 2 && recordings[0] == first && recordings[1] == second && savedLoops == loopsBefore && clock == 0,
                "deadline with two Echoes preserves both solutions instead of silently replacing one");
            StepWorld(.4f); CommitLoop();
            Verify(recordings.Count == 2 && recordings[0] == second && recordings[1] != first && savedLoops == loopsBefore + 1,
                "explicit record still replaces the oldest Echo when both slots are full");
            ClearRun(); EchoPose onPlate = spawn; onPlate.Position = consoles[0].Plate + Vector3.up * .02f;
            player.SetPose(onPlate); AimAt(consoles[0].Position); Interact(); StepWorld(.4f); CommitLoop();
            Verify(EchoAssigned(consoles[0]) && UnassignedConsole() == null, "objective guidance recognizes a pending console and final plate assignment");
            ClearRun(); player.SetPose(onPlate); StepWorld(.4f); CommitLoop();
            Verify(!EchoAssigned(consoles[0]) && UnassignedConsole() == consoles[0], "plate-only Echo warns about a missing recorded console interaction");
            LoadLevel(1); SetScreen(EchoScreen.Playing);
            var motion = enemies[0].GetComponentInChildren<EchoRobotMotion>();
            var animationPlayer = enemies[0].GetComponentInChildren<Animation>();
            yield return null; yield return null;
            var robotRenderer = enemies[0].GetComponentInChildren<SkinnedMeshRenderer>();
            Verify(motion != null && animationPlayer != null && animationPlayer.GetClip("Rig|Walk") != null &&
                robotRenderer.sharedMaterial.mainTexture != null && robotRenderer.bounds.size.y > .7f && robotRenderer.bounds.size.y < 2.6f,
                "textured robot and walking animation survive prefab import and runtime instantiation");
            int[] signatures = new int[3];
            for (int sector = 0; sector < 3; sector++)
            {
                LoadLevel(sector); SetScreen(EchoScreen.Playing);
                int meshes = 0; foreach (MeshRenderer r in worldRoot.GetComponentsInChildren<MeshRenderer>()) if (r.name.Contains("Art") || (r.sharedMaterial.HasProperty("_MainTex") && r.sharedMaterial.mainTexture != null)) meshes++;
                signatures[sector] = meshes;
                EchoPose panorama = spawn; panorama.Position = new Vector3(0, 2, -7.2f); panorama.Pitch = 13; player.SetPose(panorama);
                Checkpoint("art-sector-" + (sector + 1)); yield return new WaitForSeconds(.25f);
            }
            Verify(signatures[0] > 40 && signatures[1] > 40 && signatures[2] > 40 && signatures[0] != signatures[1] && signatures[1] != signatures[2],
                "three sectors instantiate distinct textured laboratory maintenance and reactor layouts");
            LoadLevel(1); SetScreen(EchoScreen.Playing);
            EchoPose visible = spawn; visible.Position = new Vector3(-1, 0, -4); player.SetPose(visible);
            ControlEnemySeconds(1);
            Verify(enemies[0].Charging && enemies[0].Attacks == 0 && playerHealth == 100, "red sentry aim warning appears before the first damaging shot");
            ControlEnemySeconds(.3f);
            Verify(!enemies[0].Charging && enemies[0].Attacks == 1 && playerHealth == 88, "telegraphed sentry shot fires once at the original grace deadline");
            NewLoop(); player.SetPose(visible); ControlEnemySeconds(1);
            EchoPose covered = spawn; covered.Position = new Vector3(4.94f, 0, -1.475f); player.SetPose(covered); ControlEnemySeconds(.1f);
            Verify(!enemies[0].Charging && playerHealth == 100, "entering cover cancels the visible sentry charge warning");
            Verify(EchoLevelDefinition.Campaign[0].Plates[0] != EchoLevelDefinition.Campaign[1].Plates[0] && consoles[0].Plate.x > 0,
                "maintenance sector uses a different right-side route from the laboratory");
        }
    }
}
