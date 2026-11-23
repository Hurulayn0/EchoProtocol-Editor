using System;
using System.Collections;
using UnityEngine;

namespace EchoProtocol
{
    public sealed partial class EchoPrototypeGame
    {
        private IEnumerator RunImmersionChecks()
        {
            BeginCampaign();
            Verify(cameraFinish != null && cameraFinish.Ready && effectsMaterial.shader.isSupported, "atmosphere and transparent effects shaders survive runtime loading");
            Verify(audioBindings.menuMusicSource.clip != null && audioBindings.gameMusicSource.clip != null && FindSound(audioBindings.weaponSound, "Weapon/sfx_weapon_pulse_fire").clip != null && FindSound(audioBindings.playerSound, "Movement/sfx_footstep_1").clip != null,
                "original music and sound effects are assigned to their scene emitters");
            Verify(worldRoot.GetComponentsInChildren<ParticleSystem>().Length >= 2 && resonanceNode.GetComponent<Collider>() != null && archiveTerminal.GetComponent<Collider>() != null,
                "vent ambience resonance target and interactive archive are instantiated");
            LoadLevel(1); SetScreen(EchoScreen.Playing);
            EchoPose visible = spawn; visible.Position = new Vector3(-1, .02f, -4); player.SetPose(visible);
            AimAt(enemies[0].transform.position + Vector3.up * 1.55f); TryFirePlayer();
            Verify(enemies[0].Health == 10 && enemies[0].StunRemaining > .4f && headshotTimer > 0, "weak point hits deal fifty damage and interrupt the sentry");
            StepWorld(.25f);
            Verify(playerHealth == 100 && enemies[0].Attacks == 0 && !enemies[0].Charging, "stagger stops charging and damage while the player lines up the next shot");
            AimAt(enemies[0].transform.position + Vector3.up * 1.55f); TryFirePlayer(); StepWorld(.05f); CommitLoop();
            Verify(!enemies[0].Charging && enemies[0].Health == 60 && enemies[0].StunRemaining == 0 && headshotTimer == 0,
                "rewind clears weak point feedback and stagger with enemy health");
            EchoSessionSave shotSave = CloneSave(CaptureSession()); RestoreSession(shotSave);
            StepWorld(.05f); StepWorld(.3f);
            Verify(echoShots == 2 && LivingEnemies() == 0 && current.ActionCount == 0, "saved Echo weak point rays replay both fifty-damage hits without duplicating player actions");
            Verify(impactObjects.Count > 0, "hits and deaths emit bounded visible spark effects");
            NewLoop(); Verify(impactObjects.Count == 0 && weaponTravel == 0, "rewind cleans every transient effect and weapon motion phase");

            LoadLevel(1); SetScreen(EchoScreen.Playing);
            visible.Position = new Vector3(-3, .02f, -2); player.SetPose(visible); playerHealth = 50;
            StepWorld(1); AimAt(ResonancePosition); TryFirePlayer();
            Verify(resonanceSpent && ResonanceRemaining > 5.9f && enemies[0].StunRemaining == 2 && playerHealth == 70,
                "real raycast on resonance capacitor stuns nearby enemy heals twenty and opens six-second window");
            int attacks = enemies[0].Attacks; StepWorld(.3f);
            Verify(enemies[0].Attacks == attacks && !enemies[0].Charging && playerHealth == 70, "EMP pulse cancels the enemy warning and suppresses its next attack");
            AimAt(ResonancePosition); TryFirePlayer();
            Verify(playerHealth == 70 && enemies[0].StunRemaining < 2 && ResonanceRemaining < 6, "a spent capacitor cannot stack healing stun or extend the window");
            TogglePlanning(); float remaining = ResonanceRemaining; AdvancePlayingWorld(2);
            Verify(ResonanceRemaining == remaining && enemies[0].StunRemaining > 1, "planning also freezes resonance countdown and enemy stagger");
            TogglePlanning(); StepWorld(6.1f);
            Verify(ResonanceRemaining == 0 && resonanceSpent, "resonance access expires without silently rearming its capacitor");
            CommitLoop();
            Verify(!resonanceSpent && ResonanceRemaining == 0 && enemies[0].StunRemaining == 0 && playerHealth == 100, "new loop rearms the optional puzzle and resets stun and healing");
            var pulseSave = CloneSave(CaptureSession()); RestoreSession(pulseSave); StepWorld(1.1f);
            Verify(resonanceSpent && ResonanceRemaining > 5.9f && echoShots == 1 && enemies[0].StunRemaining > 0,
                "disk-restored Echo can fire the resonance capacitor to create a new tactical window");
            StepWorld(.3f); Verify(echoShots == 2 && ResonanceRemaining < 6, "Echo replay cannot reactivate an already spent capacitor");

            preferences.ArchiveMask = 0;
            for (int sector = 0; sector < 3; sector++)
            {
                elapsedTotal = 0; savedLoops = 0; LoadLevel(sector); SetScreen(EchoScreen.Playing);
                EchoPose nearArchive = spawn; nearArchive.Position = ArchivePosition + new Vector3(sector == 1 ? 1.2f : -1.2f, -1.18f, 0); player.SetPose(nearArchive);
                AimAt(ArchivePosition); Interact();
                Verify(!ArchiveFound && current.ActionCount == 0, "archive sector " + sector + " is locked before Echo and resonance requirements");
                NewLoop(); IEnumerator route = RecordThroughControls(0, sector > 0);
                while (route.MoveNext()) yield return route.Current;
                if (sector == 2) { route = RecordThroughControls(1, false); while (route.MoveNext()) yield return route.Current; }
                for (int i = 0; i < 110; i++) ProcessControls(.05f, new EchoInputFrame());
                Verify(EchoHoldsArchiveLock(), "archive sector " + sector + " recognizes the actual Echo-held powered plates");
                player.SetPose(nearArchive); AimAt(ArchivePosition); Interact();
                Verify(!ArchiveFound && current.ActionCount == 0, "archive sector " + sector + " requires a charged resonance window even with correct Echoes");
                player.SetPose(spawn); AimThroughControls(ResonancePosition);
                ProcessControls(.01f, new EchoInputFrame { Fire = true });
                Verify(resonanceSpent, "archive sector " + sector + " capacitor is reachable by an unobstructed real shot");
                for (int step = 0; step < 60; step++)
                {
                    Vector3 direction = nearArchive.Position - player.transform.position; direction.y = 0;
                    if (direction.magnitude < .15f) break;
                    direction.Normalize();
                    ProcessControls(.04f, new EchoInputFrame { Sprint = true, Move = new Vector2(
                        Vector3.Dot(direction, player.transform.right), Vector3.Dot(direction, player.transform.forward)) });
                    yield return null;
                }
                AimThroughControls(ArchivePosition); ProcessControls(.01f, new EchoInputFrame { Interact = true });
                Verify(ArchiveFound && current.ActionCount == 2 && ArchiveCount == sector + 1 && ResonanceRemaining > 0,
                    "archive sector " + sector + " real controller reaches terminal within six seconds and records memory interaction");
                int actions = current.ActionCount; Interact();
                Verify(current.ActionCount == actions, "archive sector " + sector + " rereading does not duplicate collection or events");
                var extraTape = new EchoSavedTape { Frames = capturedFrames.ToArray(), Actions = capturedActions.ToArray() };
                extraTape.Restore(sector);
                Verify(extraTape.Actions[1].Action.Kind == EchoActionKind.Archive, "archive sector " + sector + " event passes persistent tape schema validation");
                headshotTimer = 0; rewindFlash = 0; introRemaining = 0;
                Checkpoint("archive-sector-" + (sector + 1)); yield return new WaitForSeconds(.3f);
                EchoPose panorama = spawn; panorama.Position = new Vector3(0, 2, -7.2f); panorama.Pitch = 12; player.SetPose(panorama);
                Checkpoint("immersion-sector-" + (sector + 1)); yield return new WaitForSeconds(.3f);
                TogglePlanning(); Checkpoint("resonance-map-" + (sector + 1)); yield return new WaitForSeconds(.3f); TogglePlanning();
                NewLoop(); Verify(ArchiveFound, "archive sector " + sector + " memory survives world rewind");
            }
            Verify(ArchiveCount == 3, "all three optional memories form the complete connected story");
            SetScreen(EchoScreen.Archives); Checkpoint("archives"); yield return new WaitForSeconds(.3f);
            ProcessControls(.05f, new EchoInputFrame { Pause = true });
            Verify(screen == EchoScreen.MainMenu && !planning, "archive menu returns to main menu through Escape");
            const string prefix = "EchoProtocol.ImmersionVerification.";
            try
            {
                var profile = new EchoPreferences(prefix); profile.ArchiveMask = 5; profile.Save();
                var loaded = new EchoPreferences(prefix); loaded.Load();
                Verify(loaded.ArchiveMask == 5, "collected memory flags persist through real isolated PlayerPrefs reload");
                PlayerPrefs.SetInt(prefix + "ArchiveMask", 999); loaded.Load();
                Verify(loaded.ArchiveMask == (999 & 7), "invalid archive flags cannot address a nonexistent sector");
            }
            finally { foreach (string key in new[] { "Sensitivity", "Volume", "FieldOfView", "InvertLook", "Completed", "ResumeLevel", "UnlockedLevel", "ArchiveMask", "SampleMemory", "V2Initialized", "BestSeconds0", "BestSeconds1", "BestSeconds2", "BestLoops0", "BestLoops1", "BestLoops2" }) PlayerPrefs.DeleteKey(prefix + key); PlayerPrefs.Save(); }
        }
    }
}
