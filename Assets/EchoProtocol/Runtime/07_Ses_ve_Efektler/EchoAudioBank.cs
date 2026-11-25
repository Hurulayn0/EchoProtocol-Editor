using System.Collections.Generic;
using UnityEngine;

namespace EchoProtocol
{
    // Playback for the clips assigned in the scene under Assets/Sesler.
    public sealed partial class EchoPrototypeGame
    {
        private readonly Dictionary<AudioSource, Vector2> soundDefaults = new Dictionary<AudioSource, Vector2>();
        private float footstepTimer, lastWarningSecond = -1f;
        private bool wasGrounded = true, wasAiming, wasDoorOpen;
        private readonly HashSet<int> enemyAlertedIds = new HashSet<int>();
        private readonly HashSet<int> enemyChargingIds = new HashSet<int>();
        private float menuMusicVolume, gameMusicVolume;
        private float menuMusicGain, gameMusicGain, gameMusicPitch;

        private void CreateAudioBank()
        {
            foreach (WorldSoundLink link in audioBindings.worldSoundLinks)
                worldSound.Add((link.Owner, link.Cue), link.Source);
            menuMusicGain = audioBindings.menuMusicSource.volume;
            gameMusicGain = audioBindings.gameMusicSource.volume;
            gameMusicPitch = audioBindings.gameMusicSource.pitch;
            if (!AudioSilent)
            {
                audioBindings.menuMusicSource.volume = 0;
                audioBindings.gameMusicSource.volume = 0;
                if (!audioBindings.menuMusicSource.isPlaying) audioBindings.menuMusicSource.Play();
                if (!audioBindings.gameMusicSource.isPlaying) audioBindings.gameMusicSource.Play();
            }
        }

        private bool AudioSilent { get { return Application.isBatchMode; } }

        private void PlayUiSound(AudioSource source, float volume = .7f, float pitch = 1f)
        {
            if (!AudioSilent) PlaySource(source, volume, pitch, 0);
        }

        private void PlayWorldSound(AudioSource source, Vector3 origin, float volume, float pitch)
        {
            if (AudioSilent) return;
            Vector3 offset = origin - player.View.transform.position;
            float pan = Vector3.Dot(player.View.transform.right, offset.normalized) * .65f;
            PlaySource(source, volume / (1f + offset.magnitude * .04f), pitch, pan);
        }
        internal enum WorldCue { SentryAlert, SentryCharge, SentryDestroy, SentryFire, SentryStun, DoorOpen, ResonanceActivate, ResonancePulse, ConsoleInteract, RelayPower, EchoAction }

        [System.Serializable]
        internal struct WorldSoundLink
        {
            public Transform Owner;
            public WorldCue Cue;
            public AudioSource Source;
        }

        private readonly Dictionary<(Transform, WorldCue), AudioSource> worldSound =
            new Dictionary<(Transform, WorldCue), AudioSource>();

        private void PlayAttachedSound(WorldCue cue, Vector3 origin, float volume = .75f, float pitch = 1f, Transform owner = null)
        {
            if (AudioSilent) return;
            AudioSource source;
            if (cue == WorldCue.EchoAction && owner == null && replayingEcho >= 0 && replayingEcho < ghosts.Count)
                source = ghosts[replayingEcho].Body.GetComponent<EchoReplayActor>().ActionCue;
            else if (owner == null || !worldSound.TryGetValue((owner, cue), out source))
                return;
            PlayWorldSound(source, origin, volume, pitch);
        }


        private static AudioSource FindSound(Transform group, string key)
        {
            if (group == null) return null;
            string name;
            if (key.StartsWith("Weapon/"))
                name = key.EndsWith("pulse_fire") ? "Fire Audio" : key.EndsWith("ads_in") ? "ADS Audio" :
                    System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(key.Substring("Weapon/sfx_weapon_".Length)) + " Audio";
            else
                name = System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(
                    key.Substring(key.IndexOf('/') + 5).Replace('_', ' ')) + " Audio";
            Transform node = group.Find(name);
            return node != null ? node.GetComponent<AudioSource>() : null;
        }

        private void PlaySource(AudioSource source, float volume, float pitch, float pan)
        {
            if (source == null || source.clip == null) return;
            if (!soundDefaults.TryGetValue(source, out Vector2 settings))
            {
                settings = new Vector2(source.pitch, source.panStereo);
                soundDefaults.Add(source, settings);
            }
            bool detached = !source.gameObject.activeInHierarchy;
            if (detached) source = Instantiate(source, source.transform.position, source.transform.rotation);
            source.pitch = settings.x * pitch;
            source.panStereo = Mathf.Clamp(settings.y + pan, -1, 1);
            if (source.loop && !detached)
            {
                if (!source.isPlaying) source.Play();
            }
            else
            {
                source.PlayOneShot(source.clip, volume * preferences.Volume);
                if (detached) Destroy(source.gameObject, source.clip.length / Mathf.Max(.01f, Mathf.Abs(source.pitch)));
            }
        }

        private void ResetAudioTracking()
        {
            footstepTimer = 0f;
            lastWarningSecond = -1f;
            wasGrounded = true;
            wasAiming = false;
            wasDoorOpen = false;
            enemyAlertedIds.Clear();
            enemyChargingIds.Clear();
        }

        private void TickAudioMusic(float dt)
        {
            if (AudioSilent || audioBindings.menuMusicSource == null || audioBindings.gameMusicSource == null) return;
            bool inMenu = screen == EchoScreen.MainMenu || screen == EchoScreen.Controls || screen == EchoScreen.Settings || screen == EchoScreen.Archives;
            bool inPlay = screen == EchoScreen.Playing && !planning;
            float master = preferences.Volume * preferences.MusicVolume;
            float targetMenu = inMenu ? master * 0.55f : 0f;
            float targetGame = inPlay ? master * 0.45f : (screen == EchoScreen.Paused || planning ? master * 0.18f : 0f);
            float rate = dt > 0f ? 1f - Mathf.Exp(-3.5f * dt) : 1f;
            menuMusicVolume = Mathf.Lerp(menuMusicVolume, targetMenu, rate);
            gameMusicVolume = Mathf.Lerp(gameMusicVolume, targetGame, rate);
            audioBindings.menuMusicSource.volume = menuMusicGain * menuMusicVolume;
            audioBindings.gameMusicSource.volume = gameMusicGain * gameMusicVolume;
            // Subtle tempo lift on harder levels or final 4 seconds of the loop
            float tensionPitch = levelIndex == 2 ? 1.06f : (sampleActive ? 1.03f : (levelIndex == 1 ? 1.02f : 1.0f));
            if (inPlay && ActiveLoopLength - clock <= 4f) tensionPitch += 0.04f;
            audioBindings.gameMusicSource.pitch = gameMusicPitch * tensionPitch;
        }

        private void TickGameplayAudio(float dt)
        {
            if (AudioSilent || screen != EchoScreen.Playing || planning || player == null) return;

            // Movement footsteps & jump/landing
            bool grounded = player.Controller != null && player.Controller.isGrounded;
            Vector3 horizVel = player.Controller != null ? new Vector3(player.Controller.velocity.x, 0, player.Controller.velocity.z) : Vector3.zero;
            float speed = horizVel.magnitude;

            if (!wasGrounded && grounded) PlayUiSound(player.playerLandAudio, 0.52f, 0.95f);
            else if (wasGrounded && !grounded && player.Controller.velocity.y > 1.5f) PlayUiSound(player.playerJumpAudio, 0.45f, 1.02f);
            wasGrounded = grounded;

            if (grounded && speed > 0.8f)
            {
                footstepTimer -= dt * (speed > 4.8f ? 1.45f : 1.0f);
                if (footstepTimer <= 0f)
                {
                    bool alt = ((int)(elapsedTotal * 3f)) % 2 == 0;
                    PlayUiSound(alt ? player.footstepOneAudio : player.footstepTwoAudio, speed > 4.8f ? 0.42f : 0.28f, alt ? 1.0f : 0.94f);
                    footstepTimer = 0.44f;
                }
            }
            else footstepTimer = 0.08f;

            // ADS optic zoom click
            if (player.IsAiming && !wasAiming) PlayUiSound(weaponSettings.weaponAdsAudio, 0.42f, 1.0f);
            wasAiming = player.IsAiming;

            // Door opening transition sound
            if (doorOpen && !wasDoorOpen)
            {
                Vector3 gatePos = sampleActive ? SampleGatePosition : (door != null ? door.position : new Vector3(0, 2, 5));
                PlayAttachedSound(WorldCue.DoorOpen, gatePos, 0.72f, 1.0f, door);
            }
            wasDoorOpen = doorOpen;

            // Sentry alert & charging telegraph audio
            foreach (EchoEnemy enemy in enemies)
            {
                if (!enemy.Alive) { enemyAlertedIds.Remove(enemy.Id); enemyChargingIds.Remove(enemy.Id); continue; }
                if (enemy.HasTarget && !enemyAlertedIds.Contains(enemy.Id))
                {
                    enemyAlertedIds.Add(enemy.Id);
                    PlayAttachedSound(WorldCue.SentryAlert, enemy.transform.position, 0.6f, 1.0f, owner: enemy.transform);
                }
                else if (!enemy.HasTarget) enemyAlertedIds.Remove(enemy.Id);

                if (enemy.Charging && !enemyChargingIds.Contains(enemy.Id))
                {
                    enemyChargingIds.Add(enemy.Id);
                    PlayAttachedSound(WorldCue.SentryCharge, enemy.transform.position, 0.55f, 1.0f, owner: enemy.transform);
                }
                else if (!enemy.Charging) enemyChargingIds.Remove(enemy.Id);
            }

            // Final 3-second countdown tick
            float rem = ActiveLoopLength - clock;
            if (rem > 0f && rem <= 3.05f)
            {
                int sec = Mathf.CeilToInt(rem);
                if (sec != Mathf.RoundToInt(lastWarningSecond))
                {
                    lastWarningSecond = sec;
                    PlayUiSound(audioBindings.loopWarningAudio, 0.55f, 1.0f + (3 - sec) * 0.12f);
                }
            }
            else lastWarningSecond = -1f;
        }
    }
}
