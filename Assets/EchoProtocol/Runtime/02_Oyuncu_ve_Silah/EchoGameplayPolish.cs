using System.Collections.Generic;
using UnityEngine;

namespace EchoProtocol
{
    // Gameplay feel & difficulty polish: weapon energy/overheat, screen shake,
    // directional damage indicator, Sentry death collapse wreckage, Echo combat animation anticipation,
    // live loop duration scaling, and S/A/B/C performance ranking.
    public sealed partial class EchoPrototypeGame
    {
        private const float MaxWeaponEnergy = 8f;
        private float weaponEnergy = MaxWeaponEnergy, weaponRechargeDelay;
        private bool weaponOverheated;
        private float cameraShake, damageDirTimer, chapterBannerTimer, empCalloutTimer, overchargeTimer;
        private string empCalloutText = "EMP ŞOKU! MUHAFIZLAR 3.5 SN DONDU · +25 CAN · 4 SN AŞIRI YÜKLEME!";
        private string chapterBannerTitle = "", chapterBannerSub = "";
        private Vector3 lastDamageSourcePos;
        private int sectorDamageTaken, lastEarnedRank = 3;
        private readonly List<GameObject> wreckageObjects = new List<GameObject>();
        private readonly List<ShockwaveRing> activeShockwaves = new List<ShockwaveRing>();
        private Transform empRingOuter, empRingInner, empConduitLine;

        private sealed class ShockwaveRing
        {
            public GameObject Ring;
            public float Age;
        }

        private bool IsLiveDifficulty { get { return !testing && !automatedRun; } }

        // Permanent upgrades unlocked by collecting Sector Data Vaults (Bellek Kasaları) + live Overcharge buff
        private float EffectiveMaxWeaponEnergy { get { return IsLiveDifficulty && ArchiveCount >= 2 ? MaxWeaponEnergy + 2f : MaxWeaponEnergy; } }
        private int EffectiveBonusShotDamage { get { return (IsLiveDifficulty && ArchiveCount >= 3 ? 5 : 0) + (IsLiveDifficulty && overchargeTimer > 0f ? 10 : 0); } }
        private bool IsWeaponOvercharged { get { return IsLiveDifficulty && overchargeTimer > 0f; } }
        private string ActiveVaultBonusText()
        {
            int c = ArchiveCount;
            if (c <= 0) return "BONUS YOK · Kasaları açarak kalıcı güçlenme kazan";
            if (c == 1) return "AKTİF: +%30 Hızlı Silah Soğuma";
            if (c == 2) return "AKTİF: +%30 Soğuma · +2 Silah Enerjisi (10)";
            return "AKTİF: +%30 Soğuma · 10 Enerji · +5 Bonus Hasar";
        }

        private float ActiveLoopLength => IsLiveDifficulty && activeRoom != null ? activeRoom.LoopSeconds : LoopLength;

        private void ResetWeaponEnergy()
        {
            weaponEnergy = EffectiveMaxWeaponEnergy;
            weaponRechargeDelay = 0f;
            weaponOverheated = false;
            overchargeTimer = 0f;
        }

        private void RefillWeaponEnergyFromEmp(float overchargeSeconds = 4f, string bannerText = null)
        {
            weaponEnergy = EffectiveMaxWeaponEnergy;
            weaponRechargeDelay = 0f;
            weaponOverheated = false;
            if (IsLiveDifficulty) overchargeTimer = Mathf.Max(overchargeTimer, overchargeSeconds);
            empCalloutText = bannerText ?? "EMP ŞOKU! MUHAFIZLAR 20 HASAR + 3.5 SN DONDU · +25 CAN · 4 SN SINIRSIZ GÜÇ!";
            empCalloutTimer = 3.0f;
        }

        private bool TryConsumeWeaponEnergy()
        {
            if (!IsLiveDifficulty) return true;
            if (overchargeTimer > 0f)
            {
                weaponEnergy = EffectiveMaxWeaponEnergy;
                weaponOverheated = false;
                return true;
            }
            if (weaponOverheated)
            {
                PlayUiSound(weaponSettings.weaponDryFireAudio, 0.55f);
                Notify("SİLAH AŞIRI ISINDI / Enerji soğuyor... (EMP Küresi anında doldurur)");
                return false;
            }
            weaponEnergy = Mathf.Max(0f, weaponEnergy - 1f);
            weaponRechargeDelay = .45f;
            if (weaponEnergy <= .01f)
            {
                weaponOverheated = true;
                PlayUiSound(weaponSettings.weaponDryFireAudio, 0.55f, 0.85f);
                Notify("ENERJİ TÜKENDİ / Kısa soğuma bekleniyor...");
            }
            return true;
        }

        private void TickWeaponEnergy(float dt)
        {
            if (!IsLiveDifficulty) return;
            overchargeTimer = Mathf.Max(0f, overchargeTimer - dt);
            float maxE = EffectiveMaxWeaponEnergy;
            if (overchargeTimer > 0f) { weaponEnergy = maxE; weaponOverheated = false; return; }
            if (weaponEnergy > maxE) weaponEnergy = maxE;
            if (weaponRechargeDelay > 0f) { weaponRechargeDelay = Mathf.Max(0f, weaponRechargeDelay - dt); return; }
            if (weaponEnergy < maxE)
            {
                float speedMul = ArchiveCount >= 1 ? 1.3f : 1.0f;
                weaponEnergy = Mathf.Min(maxE, weaponEnergy + dt * (weaponOverheated ? 2.6f : 3.2f) * speedMul);
                if (weaponOverheated && weaponEnergy >= 2.5f) weaponOverheated = false;
            }
        }

        private void AddCameraShake(float trauma)
        {
            if (!IsLiveDifficulty) return;
            cameraShake = Mathf.Clamp01(cameraShake + trauma);
        }

        private void RegisterDamageSource(Vector3 sourceWorldPos)
        {
            lastDamageSourcePos = sourceWorldPos;
            damageDirTimer = .95f;
            AddCameraShake(.52f);
        }

        private void ResetCameraShake()
        {
            cameraShake = 0f;
            damageDirTimer = 0f;
            empCalloutTimer = 0f;
            if (player != null && player.View != null && screen == EchoScreen.Playing)
                player.View.transform.localPosition = player.CameraPosition;
        }

        private void TickCameraShake(float dt)
        {
            damageDirTimer = Mathf.Max(0f, damageDirTimer - dt);
            chapterBannerTimer = Mathf.Max(0f, chapterBannerTimer - dt);
            empCalloutTimer = Mathf.Max(0f, empCalloutTimer - dt);
            TickShockwaves(dt);
            if (!IsLiveDifficulty || player == null || player.View == null) return;
            if (screen != EchoScreen.Playing || planning) { cameraShake = 0f; return; }
            if (cameraShake > 0f)
            {
                cameraShake = Mathf.Max(0f, cameraShake - dt * 2.4f);
                float mag = cameraShake * cameraShake * .075f;
                float t = Time.unscaledTime * 48f;
                Vector3 offset = new Vector3(Mathf.Sin(t * 1.13f) * mag, Mathf.Cos(t * 1.37f) * mag, 0f);
                player.View.transform.localPosition = player.CameraPosition + offset;
            }
            else player.View.transform.localPosition = player.CameraPosition;
        }

        private void TriggerChapterBanner()
        {
            if (!IsLiveDifficulty) return;
            chapterBannerTimer = 3.2f;
            if (sampleActive)
            {
                chapterBannerTitle = "BÖLÜM 3 · " + SampleStageNames[sampleStage];
                chapterBannerSub = sampleStage == 0 ? "20 sn Döngü · Servis butonunu Echo'ya kaydet ve kapıdan geç" :
                                   sampleStage == 1 ? "20 sn Döngü · Lens + EMP Küresi + Çift Röle senkronizasyonu" :
                                   "20 sn Döngü · Muhafızı EMP ile dondur veya Echo ile dikkatini dağıt";
            }
            else
            {
                chapterBannerTitle = "BÖLÜM " + (levelIndex == 2 ? 4 : levelIndex + 1) + " · " + Level.Name;
                chapterBannerSub = levelIndex == 0 ? "KOLAY · 20 sn Döngü · 0 Muhafız · Konsol E → Plaka → R ile Echo kaydet" :
                                   levelIndex == 1 ? "ORTA · 18 sn Döngü · 1 Muhafız · C ile eğil, siper al veya EMP Küresini vur" :
                                   "FİNAL / ÇOK ZOR · 16 sn Döngü · 2 Muhafız · Siperleri ve EMP Küresini kullan!";
            }
        }

        private void ShowEmpArc(Vector3 from, Vector3 to, Material mat = null)
        {
            if (!IsLiveDifficulty || worldRoot == null) return;
            LineRenderer line = Instantiate(effectsBindings.arcPrefab, worldRoot);
            line.gameObject.name = "EMP Lightning Arc";
            transientBeams.RemoveAll(item => item == null);
            transientBeams.Add(line.gameObject);
            if (mat != null) line.sharedMaterial = mat;
            for (int i = 0; i < 5; i++)
            {
                float f = i / 4f;
                Vector3 pt = Vector3.Lerp(from, to, f);
                if (i > 0 && i < 4)
                    pt += new Vector3(Mathf.Sin(i * 3.7f + Time.time * 25f) * .25f, .18f * (i % 2 == 1 ? 1f : -.5f), Mathf.Cos(i * 2.9f) * .25f);
                line.SetPosition(i, pt);
            }
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Destroy(line.gameObject, effectsBindings.arcDuration);
        }

        private void SpawnResonanceShockwave(Vector3 origin, Material mat)
        {
            if (!IsLiveDifficulty || worldRoot == null) return;
            MeshRenderer ring = Instantiate(activeRoom.ShockwavePrefab, origin, worldRoot.rotation, worldRoot);
            if (mat != null) ring.sharedMaterial = mat;
            activeShockwaves.Add(new ShockwaveRing { Ring = ring.gameObject });
        }

        private void TickShockwaves(float dt)
        {
            for (int i = activeShockwaves.Count - 1; i >= 0; i--)
            {
                ShockwaveRing sw = activeShockwaves[i];
                if (sw.Ring == null) { activeShockwaves.RemoveAt(i); continue; }
                sw.Age += dt;
                activeRoom.ShockwaveAnimation.SampleAnimation(sw.Ring, Mathf.Min(sw.Age, activeRoom.ShockwaveAnimation.length));
                if (sw.Age >= activeRoom.ShockwaveAnimation.length)
                {
                    sw.Ring.SetActive(false);
                    Destroy(sw.Ring);
                    activeShockwaves.RemoveAt(i);
                }
            }
        }

        private void ClearShockwaves()
        {
            foreach (ShockwaveRing sw in activeShockwaves)
                if (sw.Ring != null) { sw.Ring.SetActive(false); Destroy(sw.Ring); }
            activeShockwaves.Clear();
        }


        private void SpawnSentryWreckage(EchoEnemy enemy)
        {
            if (!IsLiveDifficulty || worldRoot == null) return;
            GameObject wreck = Instantiate(enemy.wreckagePrefab, enemy.transform.position, enemy.transform.rotation, worldRoot);
            wreck.name = "Sentry wreckage";
            EchoRobotMotion motion = wreck.GetComponentInChildren<EchoRobotMotion>();
            if (motion != null) motion.TriggerCollapse();
            wreckageObjects.Add(wreck);
            EmitImpact(enemy.transform.position + Vector3.up * 1.1f, new Color(1f, .62f, .18f), 28, 1.1f);
            AddCameraShake(.38f);
        }

        private void ClearWreckage()
        {
            foreach (GameObject w in wreckageObjects)
                if (w != null) { w.SetActive(false); Destroy(w); }
            wreckageObjects.Clear();
        }

        private void UpdateEchoAnimationStates()
        {
            for (int i = 0; i < ghosts.Count; i++)
            {
                EchoRobotMotion motion = ghosts[i].Motion;
                if (motion == null) continue;
                bool aboutToFire = false;
                if (i < savedTapes.Count && savedTapes[i].Actions != null)
                {
                    foreach (EchoSavedAction act in savedTapes[i].Actions)
                    {
                        if (act.Action.Kind == EchoActionKind.Fire && act.Time > clock && act.Time - clock <= .36f)
                        { aboutToFire = true; break; }
                    }
                }
                motion.SetCombatState(aboutToFire, false, false);
            }
        }

        private void TriggerGhostAttackAnimation(int ghostIndex)
        {
            if (ghostIndex < 0 || ghostIndex >= ghosts.Count) return;
            if (ghosts[ghostIndex].Motion != null) ghosts[ghostIndex].Motion.TriggerAttack();
        }

        private void TriggerGhostInteractAnimation(int ghostIndex, Vector3 worldPos)
        {
            if (ghostIndex >= 0 && ghostIndex < ghosts.Count && ghosts[ghostIndex].Motion != null)
                ghosts[ghostIndex].Motion.TriggerInteract();
            Color col = ghostIndex == 1 ? new Color(.78f, .45f, 1f) : new Color(.15f, .92f, 1f);
            EmitImpact(worldPos, col, 8, .35f);
        }

        private int EvaluatePerformanceRank(int slot, float seconds, int loops, int damageTaken)
        {
            // S = 4, A = 3, B = 2, C = 1
            int score = 100 + (IsLiveDifficulty && ArchiveCount >= 3 ? 15 : 0);
            float parTime = slot == 0 ? 22f : slot == 1 ? 30f : slot == 2 ? 42f : 75f;
            int parLoops = slot == 0 ? 1 : slot == 1 ? 2 : slot == 2 ? 3 : 4;
            if (seconds > parTime) score -= Mathf.Min(30, Mathf.RoundToInt((seconds - parTime) * .9f));
            if (loops > parLoops) score -= Mathf.Min(25, (loops - parLoops) * 10);
            score -= Mathf.Min(35, damageTaken / 2);
            return score >= 88 ? 4 : score >= 70 ? 3 : score >= 50 ? 2 : 1;
        }

        private void RecordCurrentPerformanceRank(int slot, float seconds, int loops)
        {
            lastEarnedRank = EvaluatePerformanceRank(slot, seconds, loops, sectorDamageTaken);
            if (CanPersist) { preferences.RecordRank(slot, lastEarnedRank); preferences.Save(); }
            else preferences.RecordRank(slot, lastEarnedRank);
        }

        private static Color RankColor(int rankValue)
        {
            return rankValue >= 4 ? new Color(1f, .84f, .25f) :
                   rankValue == 3 ? new Color(.18f, .9f, 1f) :
                   rankValue == 2 ? new Color(.32f, .92f, .55f) : new Color(1f, .58f, .28f);
        }



    }
}
