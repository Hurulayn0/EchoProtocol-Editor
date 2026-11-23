using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoProtocol
{
    // Authored maintenance chapter, using the existing recorder and controller.
    public sealed partial class EchoPrototypeGame
    {
        private bool sampleActive, sampleDecoyObserved, sampleExtraction, sampleCrossingAuthorized, sampleStageMemoryHacked;
        private int sampleStage, sampleCompleted, sampleDeaths;
        private float sampleServiceUntil;
        private Transform sampleShutter, sampleSensor;
        private BoxCollider sampleSensorArea;
        private readonly List<SampleDevice> sampleDevices = new List<SampleDevice>();
        private readonly List<SampleReplayEvent> sampleEvents = new List<SampleReplayEvent>();
        private readonly List<string> sampleFinalEvents = new List<string>();
        private int replayingEcho = -1;
        private static readonly string[] SampleStageNames = { "01 / SERVİS GEÇİDİ", "02 / ATIŞ PENCERESİ", "03 / TAHLİYE" };
        private const string SampleMemoryText = "Gözlem 04: Muhafız canlı insanı değil, hareket izini takip ediyor. Bakım ekibi geçmişlerini yem ve operatör olarak kullanıp tahliye hattını açmış. Echo deneyinden çıkış, geçmişini silmekten değil onunla çalışmaktan geçiyor.";
        private sealed class SampleDevice { public int Id; public string Name; public Transform Visual; public Vector3 Position; public Material StandbyMaterial; public Color Accent; public bool Powered; }
        private sealed class SampleReplayEvent { public Ghost Ghost; public int Index; public float Time; public EchoAction Action; }
        private Vector3 SampleGatePosition { get { return worldRoot.TransformPoint(closedDoorPosition); } }
        private EchoPose SampleSpawn => roomPlayerSpawn != null
            ? new EchoPose { Position = roomPlayerSpawn.position, Rotation = roomPlayerSpawn.rotation, Pitch = spawn.Pitch }
            : spawn;
        private Vector3 SamplePulsePosition { get { return sampleStage == 0 && IsLiveDifficulty ? new Vector3(-1.2f, 1.35f, -3.2f) : new Vector3(0, 1.35f, 4); } }

        private void BeginSample()
        {
            elapsedTotal = 0; savedLoops = 0; sampleCompleted = 0; sampleDeaths = 0; sectorDamageTaken = 0;
            if (ChangeScene(SynchronizationScene, saveOnArrival: true)) return;
            StartSampleStage();
            SaveSession();
        }

        private string ServiceWindowText => activeRoom.ServiceWindowSeconds.ToString("0.##", System.Globalization.CultureInfo.GetCultureInfo("tr-TR"));

        private void StartSampleStage()
        {
            LoadSampleStage(0); SetScreen(EchoScreen.Playing);
            Notify(IsLiveDifficulty ? "Muhafızı aş, servis geçidini Echo ile aç ve yük asansörüne ulaş." : $"Yük asansörüne ulaş. Servis düğmesi geçidi yalnızca {ServiceWindowText} saniye açar.");
            if (!IsLiveDifficulty) { TogglePlanning(); echoTeaching = true; }
        }
        private void LoadSampleStage(int stage)
        {
            foreach (Ghost ghost in ghosts) { ghost.Body.SetActive(false); Destroy(ghost.Body); }
            UnloadAuthoredRoom();
            ghosts.Clear(); recordings.Clear(); savedTapes.Clear(); enemies.Clear(); consoles.Clear(); sampleDevices.Clear();
            sampleActive = true; sampleStage = Mathf.Clamp(stage, 0, 2); levelIndex = 1; sampleStageMemoryHacked = false;
            sectorElapsed = 0; sectorLoops = 0; vistaCore = null; resonanceNode = null; archiveTerminal = null;
            ClearWreckage(); ClearShockwaves();
            LoadAuthoredRoom((IsLiveDifficulty ? maintenanceRooms : validationRooms)[sampleStage]);
            NewLoop(); TriggerChapterBanner(); QueueStoryBriefing(); introRemaining = 0; message = SampleGoal();
        }
        private void ResetSampleLoop()
        {
            sampleServiceUntil = 0; sampleDecoyObserved = false; sampleExtraction = false; sampleCrossingAuthorized = false; sampleFinalEvents.Clear();
            foreach (SampleDevice device in sampleDevices) { device.Powered = false; device.Visual.GetComponent<Renderer>().sharedMaterial = device.StandbyMaterial; }
            player.SetPose(SampleSpawn); door.localPosition = closedDoorPosition;
            if (sampleShutter != null) { sampleShutter.gameObject.SetActive(true); sampleShutter.GetComponent<Collider>().enabled = true; }
            Physics.SyncTransforms();
        }
        private void InteractSample()
        {
            Physics.SyncTransforms(); RaycastHit hit;
            if (!Physics.Raycast(player.View.transform.position, player.View.transform.forward, out hit, 2.6f, ~(1 << 2), QueryTriggerInteraction.Ignore))
            { Notify("E: yakındaki bir paneli hedefle. Etkileşim mesafesi 2,6 m."); return; }
            foreach (SampleDevice device in sampleDevices)
            {
                if (hit.transform != device.Visual) continue;
                // Keep intent even when power is unavailable; replay evaluates the new world.
                RecordPose(clock, player.Pose); RecordAction(clock, EchoAction.Device(device.Id));
                UseSampleDevice(device.Id, player.Pose.Position, false); return;
            }
            Notify("Bu nesne etkileşim paneli değil. E işaretli terminale nişan al.");
        }
        private bool UseSampleDevice(int id, Vector3 actor, bool fromEcho)
        {
            if (id < 0 || id >= sampleDevices.Count) return false;
            SampleDevice device = sampleDevices[id];
            if (Vector3.Distance(actor + Vector3.up * 1.2f, device.Position) > 2.6f) return false;
            string failure = null;
            if (sampleStage == 0) sampleServiceUntil = clock + activeRoom.ServiceWindowSeconds;
            else if (sampleStage == 1 && ResonanceRemaining <= 0) failure = "Rezonans yok. E denemen kaydedildi; enerji penceresinde tekrar oynatılabilir.";
            else if (sampleStage == 2 && id == 1)
            {
                if (!sampleDevices[0].Powered) failure = "Sol servis rölesi kapalı. Operatör E denemen kaydedildi.";
                else if (enemies[0].Alive && enemies[0].StunRemaining <= 0) failure = "Muhafız aktif. Rezonansla durdur veya etkisiz hale getir; E denemen kaydedildi.";
                else { sampleExtraction = true; SampleEvent("Operatör tahliyeyi serbest bıraktı", fromEcho); }
            }
            else if (sampleStage == 2 && id == 2)
            {
                if (IsLiveDifficulty)
                {
                    if (sampleStageMemoryHacked) { if (!fromEcho) Notify(SampleMemoryText); return true; }
                    bool canScan = ResonanceRemaining > 0 || sampleDecoyObserved || (enemies.Count > 0 && !enemies[0].Alive);
                    if (!canScan) failure = "Tarama kilitli: EMP Küresini vur, muhafızı durdur ya da Echo'nu yem yap, sonra [E]'ye bas!";
                    else
                    {
                        sampleStageMemoryHacked = true;
                        preferences.SampleMemory = true; if (CanPersist) preferences.Save();
                        playerHealth = MaxPlayerHealth;
                        SpawnResonanceShockwave(device.Position, powerOn);
                        if (!fromEcho) { Notify($"GÖZLEM BELLEĞİ İNDİRİLDİ! Can {MaxPlayerHealth} · Tahliye kilidi için Operatör + Sol Servis rölesini tamamla!"); notificationRemaining = 9; } PlayUiSound(audioBindings.archiveUnlockAudio, 0.72f); SaveSession();
                    }
                }
                else
                {
                    if (preferences.SampleMemory) { if (!fromEcho) Notify(SampleMemoryText); return true; }
                    if (!sampleDecoyObserved) failure = "Gözlem eksik: muhafızın bir Echo’ya ateş etmesini sağla.";
                    else if (ResonanceRemaining <= 0) failure = "Tarama kilitli: gözlemden sonra rezonans penceresinde terminali oku.";
                    else
                    {
                        preferences.SampleMemory = true; if (CanPersist) preferences.Save();
                        if (!fromEcho) { Notify(SampleMemoryText); notificationRemaining = 9; } PlayUiSound(audioBindings.archiveUnlockAudio, 0.72f); SaveSession();
                    }
                }
            }
            if (failure != null) { if (!fromEcho) { Notify(failure); PlayUiSound(audioBindings.consoleErrorAudio, 0.48f); } else Notify("ECHO " + (replayingEcho + 1) + " / " + failure.Replace("E denemen kaydedildi", "E koşulu sağlanmadı")); return false; }
            if (!device.Powered) SampleEvent(device.Name + " / E", fromEcho);
            device.Powered = true; device.Visual.GetComponent<Renderer>().sharedMaterial = powerOn;
            if (!fromEcho && id != 2) Notify(sampleStage == 0 ? $"Geçit {ServiceWindowText} sn açık. Bu E işlemini R ile Echo’ya kaydet." : device.Name + " etkin. Planın sonraki adımına geç.");
            PlayAttachedSound(fromEcho ? WorldCue.EchoAction : (sampleStage == 1 ? WorldCue.RelayPower : WorldCue.ConsoleInteract), device.Position, 0.72f, owner: fromEcho ? null : device.Visual);
            return true;
        }
        private void SampleEvent(string text, bool fromEcho)
        {
            string line = clock.ToString("0.0") + "s / " + (fromEcho ? "E" + (replayingEcho + 1) : "SEN") + " / " + text;
            if (sampleFinalEvents.Count >= 8) sampleFinalEvents.RemoveAt(0);
            sampleFinalEvents.Add(line);
        }
        private Vector3 SampleSensorPosition => sampleSensor.position - Vector3.up * .12f;

        private bool SampleSensorHeld()
        {
            if (OnPlate(player.transform.position, sampleSensorArea)) return true;
            foreach (Ghost ghost in ghosts) if (OnPlate(ghost.Tape.Sample(clock, EchoPose.Lerp).Position, sampleSensorArea)) return true;
            return false;
        }
        private void UpdateSampleDoor(float dt)
        {
            if (sampleStage == 1 && sampleShutter != null)
            {
                bool held = SampleSensorHeld() || (IsLiveDifficulty && ResonanceRemaining > 0f);
                sampleShutter.GetComponent<Collider>().enabled = !held; sampleShutter.GetComponent<Renderer>().enabled = !held;
                sampleSensor.GetComponent<Renderer>().sharedMaterial = held ? plateOn : IdleMaterial(sampleSensor);
            }
            if (sampleStage == 2 && enemies.Count > 0 && enemies[0].Attacks > 0 && enemies[0].LastTargetWasEcho) sampleDecoyObserved = true;
            bool stage2Unlocked = IsLiveDifficulty ? (sampleExtraction && (sampleDevices[2].Powered || sampleStageMemoryHacked)) : sampleExtraction;
            power = sampleStage == 0 ? clock < sampleServiceUntil : sampleStage == 1 ? sampleDevices[0].Powered && sampleDevices[1].Powered : stage2Unlocked;
            plateHeld = sampleStage != 1 || SampleSensorHeld();
            doorOpen = power && (sampleStage != 1 || ResonanceRemaining > 0);
            // The sliding leaf never traps a controller standing at the threshold.
            Vector3 delta = player.transform.position - SampleGatePosition;
            bool threshold = Mathf.Abs(delta.x) < 1.25f && Mathf.Abs(delta.z) < .85f;
            if (doorOpen && threshold) sampleCrossingAuthorized = true;
            bool physicalOpen = doorOpen || (threshold && doorFraction > .01f);
            doorFraction = Mathf.MoveTowards(doorFraction, physicalOpen ? 1 : 0, dt * 5);
            door.localPosition = closedDoorPosition + doorOpenOffset * doorFraction;
            doorCollider.enabled = closedDoorCollision && !physicalOpen;
            if (vistaCore != null) vistaCore.GetComponent<Renderer>().sharedMaterial = doorOpen ? powerOn : IdleMaterial(vistaCore);
        }
        private void CheckSampleExit()
        {
            if (screen != EchoScreen.Playing || playerHealth <= 0 || (!doorOpen && !sampleCrossingAuthorized)) return;
            float targetZ = SampleGatePosition.z + (sampleStage == 0 ? 1.6f : 2.2f);
            float targetX = SampleGatePosition.x;
            if (player.transform.position.z < targetZ || Mathf.Abs(player.transform.position.x - targetX) > 1.5f) return;
            sampleCompleted |= 1 << sampleStage;
            if (sampleStage < 2)
            {
                LoadSampleStage(sampleStage + 1); SetScreen(EchoScreen.Playing); SaveSession();
                Notify("KONTROL NOKTASI / Önceki sorun çözüldü. Yerel Echo’lar arşivlendi; yeni plan burada başlar.");
            }
            else { RecordCurrentPerformanceRank(3, elapsedTotal, savedLoops); SaveSession(); SetScreen(EchoScreen.GameWon); message = "Geçmişin tahliye hattını açtı."; }
        }
        private void AdvanceSampleEchoes(float targetTime)
        {
            sampleEvents.Clear();
            for (int i = 0; i < ghosts.Count; i++)
            {
                Ghost ghost = ghosts[i]; int index = i;
                ghost.Cursor.Advance(targetTime, (time, action) => sampleEvents.Add(new SampleReplayEvent { Ghost = ghost, Index = index, Time = time, Action = action }));
            }
            sampleEvents.Sort((a, b) => { int order = a.Time.CompareTo(b.Time); return order != 0 ? order : a.Index.CompareTo(b.Index); });
            foreach (SampleReplayEvent item in sampleEvents)
            {
                clock = item.Time; replayingEcho = item.Index; UpdateSampleDoor(0); Physics.SyncTransforms();
                if (item.Action.Kind == EchoActionKind.Fire) { ExecuteShot(item.Action, true); echoShots++; }
                else if (item.Action.Kind == EchoActionKind.Device && UseSampleDevice(item.Action.TargetId, item.Ghost.Tape.Sample(item.Time, EchoPose.Lerp).Position, true))
                { echoActivations++; TriggerGhostInteractAnimation(item.Index, item.Ghost.Tape.Sample(item.Time, EchoPose.Lerp).Position + Vector3.up); }
            }
            replayingEcho = -1; clock = targetTime;
            foreach (Ghost ghost in ghosts) ApplyGhostPose(ghost, ghost.Tape.Sample(clock, EchoPose.Lerp));
        }
        private string SampleGoal()
        {
            if (sampleStage == 0) return "Yük asansörüne ulaş. Servis geçidi kilitli.";
            if (sampleStage == 1) return "Aktarım köprüsü kilitli. Sahadaki sistemlerin tepkisini gözle.";
            return "Tahliye hattı kilitli. Operatör alanına ulaş ve durumu değerlendir.";
        }
        private string SampleGateReason()
        {
            if (doorOpen) return "YOL AÇIK / Planın çalıştı. Karşıya geç.";
            if (sampleStage == 0)
            {
                if (sampleServiceUntil <= 0) return "GEÇİT KAPALI / Servis panelinde E işlemi bekleniyor.";
                return $"GEÇİT KAPALI / Servis penceresi {ServiceWindowText} sn sürdü; geçiş rotanı kısalt.";
            }
            if (sampleStage == 1)
            {
                if (ResonanceRemaining <= 0)
                {
                    if (!resonanceSpent) return "ENERJİ YOK / Rezonatör bu döngüde etkinleşmedi.";
                    string upper = sampleDevices[0].Powered ? "✓" : "—";
                    string lower = sampleDevices[1].Powered ? "✓" : "—";
                    return "PENCERE KAPANDI / Üst " + upper + " · Alt " + lower + ". T aynı zamanlamayı tekrarlar; geciken E kaydını değiştir.";
                }
                return !sampleDevices[0].Powered ? "ÜST RÖLE EKSİK / Enerji: " + ResonanceRemaining.ToString("0.0") + " sn · terminalde E." : "ALT RÖLE EKSİK / Enerji: " + ResonanceRemaining.ToString("0.0") + " sn · terminalde E.";
            }
            if (!sampleDevices[0].Powered) return "SOL RÖLE KAPALI / Sol servis terminali E.";
            if (enemies.Count > 0 && enemies[0].Alive && enemies[0].StunRemaining <= 0) return "MUHAFIZ AKTİF / Rezonansla durdur veya vur.";
            if (!sampleExtraction) return "OPERATÖR BEKLİYOR / Üst sağ terminal E.";
            if (IsLiveDifficulty && !sampleDevices[2].Powered && !sampleStageMemoryHacked) return "GÖZLEM BELLEĞİ EKSİK / Sol üst terminalde E ile indir.";
            return "OPERATÖR BEKLİYOR / Üst sağ terminal E.";
        }
    }
}
