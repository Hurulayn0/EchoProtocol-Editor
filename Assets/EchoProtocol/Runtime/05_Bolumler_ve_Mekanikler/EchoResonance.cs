using UnityEngine;

namespace EchoProtocol
{
    public sealed partial class EchoPrototypeGame
    {
        private Transform resonanceNode, archiveTerminal;
        private float resonanceUntil, headshotTimer, rewindFlash, introRemaining;
        private bool resonanceSpent, sectorVaultHacked;
        private Vector3 ResonancePosition { get { return resonanceNode != null ? resonanceNode.position : sampleActive ? SamplePulsePosition : levelIndex == 0 ? new Vector3(4.4f, 1.35f, -1) : levelIndex == 1 ? new Vector3(-4.4f, 1.35f, 1.6f) : new Vector3(5.7f, 1.35f, -.6f); } }
        private Vector3 ArchivePosition { get { return archiveTerminal != null ? archiveTerminal.position : levelIndex == 1 ? new Vector3(-5.4f, 1.2f, -5.8f) : new Vector3(6, 1.2f, -5.8f); } }
        private bool ArchiveFound { get { return (preferences.ArchiveMask & (1 << levelIndex)) != 0; } }
        private bool IsVaultHackedThisRun { get { return IsLiveDifficulty ? sectorVaultHacked : ArchiveFound; } }
        private float ResonanceRemaining { get { return Mathf.Max(0, resonanceUntil - clock); } }
        private int ArchiveCount { get { int n = 0; for (int i = 0; i < 3; i++) if ((preferences.ArchiveMask & (1 << i)) != 0) n++; return n; } }
        private static readonly string[] ArchiveTitles = { "01 / İLK İZ", "02 / KAYIP EKİP", "03 / ECHO PROTOKOLÜ" };
        private static readonly string[] ArchiveTexts = {
            "Kayıt 01: Echo bir hayalet değil. Yapacağın hareketi önceden yapmış bir takım arkadaşı. Sistem, aynı insanın farklı denemelerini tek bir ana bağlayabiliyor.",
            "Kayıt 02: Muhafızlar tesisi korumuyordu. Bizi burada tutuyorlardı. Önceki ekibin izleri silindi; yalnızca rezonans kasalarında birkaç kayıt kaldı.",
            "Kayıt 03: Çıkış tek kişiyle açılmasın diye iki anahtar kondu. Kendi geçmişimizle birlikte çalışarak deneyi aşabildik. Geçmişini terk etme; yanında taşı." };

        private void ResetResonance()
        {
            resonanceSpent = false; resonanceUntil = 0; headshotTimer = 0; rewindFlash = .45f;
            if (resonanceNode != null) resonanceNode.GetComponent<Renderer>().sharedMaterial = IdleMaterial(resonanceNode);
            if (archiveTerminal != null) archiveTerminal.GetComponent<Renderer>().sharedMaterial = IsVaultHackedThisRun ? powerOn : IdleMaterial(archiveTerminal);
            if (empConduitLine != null) empConduitLine.GetComponent<Renderer>().sharedMaterial = IsVaultHackedThisRun ? powerOn : IdleMaterial(empConduitLine);
            if (cameraFinish != null) cameraFinish.SetLook(Level.Accent, rewindFlash, elapsedTotal);
        }
        private bool PulseResonance(bool fromEcho)
        {
            if (resonanceSpent) return false;
            if (IsLiveDifficulty)
            {
                resonanceSpent = true;
                resonanceUntil = Mathf.Min(ActiveLoopLength, clock + 10f);
                if (resonanceNode != null) resonanceNode.GetComponent<Renderer>().sharedMaterial = powerOn;
                if (empConduitLine != null) empConduitLine.GetComponent<Renderer>().sharedMaterial = powerOn;
                foreach (EchoEnemy enemy in enemies)
                {
                    if (!enemy.Alive) continue;
                    ShowEmpArc(ResonancePosition, enemy.transform.position + Vector3.up * 1.2f, ghostCyan);
                    enemy.TakeDamage(20);
                    if (enemy.Alive) { enemy.Stagger(3.5f); PlayAttachedSound(WorldCue.SentryStun, enemy.transform.position, 0.78f, owner: enemy.transform); }
                    else { PlayAttachedSound(WorldCue.SentryDestroy, enemy.transform.position, 0.85f, owner: enemy.transform); SpawnSentryWreckage(enemy.transform.position, enemy.transform.rotation); }
                }
                if (sampleActive)
                {
                    foreach (SampleDevice dev in sampleDevices)
                        ShowEmpArc(ResonancePosition, dev.Position, powerOn);
                }
                else if (ArchivePosition != Vector3.zero)
                {
                    ShowEmpArc(ResonancePosition, ArchivePosition, powerOn);
                }
                if (playerHealth > 0) playerHealth = Mathf.Min(MaxPlayerHealth, playerHealth + 25);
                EmitImpact(ResonancePosition, Level.Accent, 42, 2.2f);
                SpawnResonanceShockwave(ResonancePosition, accents[levelIndex]);
                RefillWeaponEnergyFromEmp(4f, sampleActive && sampleStage == 1
                    ? "EMP AKTARIMI! RÖLELER 10 SN ENERJİLENDİ · MUHAFIZ -20 CAN & 3.5 SN DONDU · +25 CAN!"
                    : "EMP ZİNCİR ŞOKU! TÜM MUHAFIZLAR -20 CAN & 3.5 SN DONDU · +25 CAN · 4 SN AŞIRI YÜKLEME!");
                AddCameraShake(.48f);
                if (sampleActive)
                {
                    SampleEvent("EMP Rezonansı / 10 sn enerji", fromEcho);
                    Notify(sampleStage == 1
                        ? "EMP AKTARIMI! Üst & Alt röleler 10 sn açıldı · Köprü muhafızı -20 Can & 3.5 sn dondu · +25 Can!"
                        : "EMP ZİNCİR ŞOKU! Muhafız -20 Can & 3.5 sn dondu · +25 Can · 4 sn Aşırı Yükleme!");
                }
                else if (!fromEcho)
                {
                    Notify(IsVaultHackedThisRun
                        ? "EMP ZİNCİR ŞOKU! Tüm muhafızlar -20 Can & 3.5 sn dondu · +25 Can · 4 sn Aşırı Yükleme!"
                        : "EMP ZİNCİR ŞOKU! Muhafızlar dondu (-20 Can) · +25 Can · VERİ KASASI KİLİDİ 10 SN AÇILDI (Kasada E'ye bas)!");
                }
                PlayAttachedSound(WorldCue.ResonancePulse, ResonancePosition, 0.82f, owner: resonanceNode);
                PlayAttachedSound(WorldCue.ResonanceActivate, ResonancePosition, 0.72f, owner: resonanceNode);
                return true;
            }
            resonanceSpent = true; resonanceUntil = Mathf.Min(LoopLength, clock + 6);
            resonanceNode.GetComponent<Renderer>().sharedMaterial = darkMaterial;
            foreach (EchoEnemy enemy in enemies)
                if (Vector3.Distance(enemy.transform.position, ResonancePosition) < 6) { enemy.Stagger(2); PlayAttachedSound(WorldCue.SentryStun, enemy.transform.position, 0.72f, owner: enemy.transform); }
            if (Vector3.Distance(player.transform.position, ResonancePosition) < 6 && playerHealth > 0) playerHealth = Mathf.Min(MaxPlayerHealth, playerHealth + 20);
            EmitImpact(ResonancePosition, Level.Accent, 35, 1.8f);
            SpawnResonanceShockwave(ResonancePosition, accents[levelIndex]);
            RefillWeaponEnergyFromEmp();
            AddCameraShake(.42f);
            if (sampleActive) { SampleEvent("EMP Rezonansı / 6 sn enerji", fromEcho); Notify("EMP ŞOKU! Muhafız 2 sn dondu · +20 Can · Silah enerjisi doldu!"); }
            else if (!fromEcho) Notify("EMP ŞOKU! Muhafızlar 2 sn dondu · +20 Can · Silah doldu · Veri Kasası 6 sn açık!");
            PlayAttachedSound(WorldCue.ResonancePulse, ResonancePosition, 0.75f, owner: resonanceNode);
            PlayAttachedSound(WorldCue.ResonanceActivate, ResonancePosition, 0.65f, owner: resonanceNode);
            return true;
        }
        private bool EchoHoldsArchiveLock()
        {
            foreach (ConsoleState console in consoles)
            {
                bool held = false;
                foreach (Ghost ghost in ghosts) held |= OnPlate(ghost.Body.transform.position, console.PlateArea);
                if (!console.Powered || !held) return false;
            }
            return true;
        }
        private bool IsVaultUnlockedNow()
        {
            if (IsLiveDifficulty)
                return ResonanceRemaining > 0f || EchoHoldsArchiveLock() || consoles.Exists(c => c.Powered);
            return EchoHoldsArchiveLock() && ResonanceRemaining > 0f;
        }
        private bool TryArchive(Vector3 position, bool fromEcho)
        {
            if (Vector3.Distance(position + Vector3.up * 1.2f, ArchivePosition) > (IsLiveDifficulty ? 3.1f : 2.6f)) return false;
            if (IsVaultHackedThisRun) { if (!fromEcho) Notify(ArchiveTitles[levelIndex] + " / " + ArchiveTexts[levelIndex]); return false; }
            if (!IsVaultUnlockedNow())
            {
                if (IsLiveDifficulty)
                {
                    ShowEmpArc(ArchivePosition, ResonancePosition, powerOff);
                    if (!fromEcho) Notify("KASA KİLİTLİ! Yandaki parlayan EMP Küresine ateş et → Kasa kilidi 10 sn açılır, sonra E'ye bas!");
                }
                else if (!fromEcho)
                {
                    Notify("Kasa kilitli. Plakaları Echo’ya devret; rezonatöre ateş et ve süre bitmeden E’ye bas.");
                }
                return false;
            }
            sectorVaultHacked = true;
            preferences.ArchiveMask |= 1 << levelIndex;
            archiveTerminal.GetComponent<Renderer>().sharedMaterial = powerOn;
            if (empConduitLine != null) empConduitLine.GetComponent<Renderer>().sharedMaterial = powerOn;
            if (CanPersist) preferences.Save();
            if (IsLiveDifficulty)
            {
                playerHealth = MaxPlayerHealth;
                foreach (EchoEnemy enemy in enemies)
                {
                    if (!enemy.Alive) continue;
                    ShowEmpArc(ArchivePosition, enemy.transform.position + Vector3.up * 1.2f, powerOn);
                    enemy.TakeDamage(25);
                    if (enemy.Alive) { enemy.Stagger(4f); PlayAttachedSound(WorldCue.SentryStun, enemy.transform.position, 0.8f, owner: enemy.transform); }
                    else { PlayAttachedSound(WorldCue.SentryDestroy, enemy.transform.position, 0.85f, owner: enemy.transform); SpawnSentryWreckage(enemy.transform.position, enemy.transform.rotation); }
                }
                SpawnResonanceShockwave(ArchivePosition, powerOn);
                RefillWeaponEnergyFromEmp(6f, "VERİ KASASI HACKLENDİ (" + ArchiveCount + "/3)! 100 CAN · MUHAFIZLAR FELÇ · " + ActiveVaultBonusText());
                AddCameraShake(.45f);
                if (!fromEcho) { Notify("VERİ KASASI HACKLENDİ (" + ArchiveCount + "/3)! " + ActiveVaultBonusText() + " · 100 Can & 6 sn Aşırı Yükleme!"); notificationRemaining = 8; }
            }
            else if (!fromEcho)
            {
                Notify(ArchiveTitles[levelIndex] + " / " + ArchiveTexts[levelIndex]);
                notificationRemaining = 8;
            } return true;
        }
        private void TickResonance(float dt)
        {
            headshotTimer = Mathf.Max(0, headshotTimer - dt); rewindFlash = Mathf.Max(0, rewindFlash - dt); introRemaining = Mathf.Max(0, introRemaining - dt);
            bool activePulse = ResonanceRemaining > 0f;
            if (empRingOuter != null) empRingOuter.localRotation = outerRingRest * Quaternion.Euler(35f, clock * (activePulse ? 320f : 95f), 20f);
            if (empRingInner != null) empRingInner.localRotation = innerRingRest * Quaternion.Euler(-40f, -clock * (activePulse ? 380f : 125f), -15f);
            if (IsLiveDifficulty && resonanceSpent && ResonanceRemaining <= 0f)
            {
                resonanceSpent = false;
                if (resonanceNode != null) resonanceNode.GetComponent<Renderer>().sharedMaterial = IdleMaterial(resonanceNode);
                if (empConduitLine != null) empConduitLine.GetComponent<Renderer>().sharedMaterial = IsVaultHackedThisRun ? powerOn : IdleMaterial(empConduitLine);
            }
            if (resonanceNode != null)
            {
                float pulseScale = activePulse ? (.64f + Mathf.Sin(clock * 14f) * .06f) : (!resonanceSpent ? (.58f + Mathf.Sin(clock * 4f) * .04f) : .52f);
                resonanceNode.localScale = resonanceRestScale * (pulseScale / (sampleActive ? .52f : .58f));
            }
            if (archiveTerminal != null) archiveTerminal.GetComponent<Renderer>().sharedMaterial = IsVaultHackedThisRun || IsVaultUnlockedNow() ? powerOn : IdleMaterial(archiveTerminal);
            if (cameraFinish != null) cameraFinish.SetLook(Level.Accent, rewindFlash, elapsedTotal);
        }

    }
}
