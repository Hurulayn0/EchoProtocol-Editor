using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EchoProtocol
{
    public sealed partial class EchoPrototypeGame
    {
        private readonly List<EchoSavedTape> savedTapes = new List<EchoSavedTape>();
        private readonly List<EchoSavedFrame> capturedFrames = new List<EchoSavedFrame>();
        private readonly List<EchoSavedAction> capturedActions = new List<EchoSavedAction>();
        private EchoSaveStore saveStore;
        private EchoSessionSave resumeSave;
        private float sectorElapsed;
        private int sectorLoops;
        private bool planning, resumeRecovered;
        private bool CanPersist { get { return !Application.isBatchMode; } }

        private void InitializeSession()
        {
            saveStore = new EchoSaveStore(Path.Combine(Application.persistentDataPath, "echo-session-v2.json"));
            if (CanPersist && !preferences.V2Initialized)
            {
                var legacy = new EchoSaveStore(Path.Combine(Application.persistentDataPath, "echo-session-v1.json"));
                try
                {
                    saveStore.ImportLegacyIfMissing(legacy);
                    preferences.V2Initialized = true; preferences.Save();
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is InvalidDataException)
                { Debug.LogWarning("ECHO MIGRATION: " + ex.Message); }
            }
            if (CanPersist && saveStore.TryRead(out resumeSave, out resumeRecovered)) { preferences.ResumeLevel = resumeSave.Level; preferences.UnlockedLevel = Mathf.Max(preferences.UnlockedLevel, resumeSave.Level); }
        }
        private void RecordPose(float time, EchoPose pose)
        {
            current.AddFrame(time, pose);
            var value = new EchoSavedFrame { Time = time, Pose = pose };
            if (capturedFrames.Count > 0 && capturedFrames[capturedFrames.Count - 1].Time == time) capturedFrames[capturedFrames.Count - 1] = value;
            else capturedFrames.Add(value);
        }
        private void RecordAction(float time, EchoAction action)
        {
            current.AddAction(time, action); capturedActions.Add(new EchoSavedAction { Time = time, Action = action });
        }
        private void SaveSession()
        {
            if (!CanPersist || saveStore == null || screen == EchoScreen.MainMenu || screen == EchoScreen.GameWon || screen == EchoScreen.LevelComplete) return;
            WriteCheckpoint(CaptureSession());
        }

        private void SaveSynchronizationCheckpoint()
        {
            if (CanPersist && saveStore != null) WriteCheckpoint(SynchronizationCheckpoint());
        }

        private EchoSessionSave SynchronizationCheckpoint() => new EchoSessionSave
        {
            Version = 2,
            Layout = 1,
            Level = MaintenanceScene,
            Memory = preferences.SampleMemory,
            Tapes = Array.Empty<EchoSavedTape>()
        };

        private void WriteCheckpoint(EchoSessionSave data)
        {
            try
            {
                saveStore.Write(data);
                resumeSave = data;
                preferences.ResumeLevel = data.Level;
                preferences.Save();
            }
            catch (Exception ex) when (ex is InvalidDataException || ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
            { Notify("Kayıt dosyasına yazılamadı. Klasör erişimini kontrol et; oyuna devam edebilirsin."); Debug.LogWarning("ECHO SAVE: " + ex.Message); }
        }
        private EchoSessionSave CaptureSession()
        {
            return new EchoSessionSave { Version = sampleActive ? 2 : 1, Layout = sampleActive ? 1 : 0,
                Stage = sampleActive ? sampleStage : 0, CompletedMask = sampleActive ? sampleCompleted : 0,
                Memory = sampleActive && preferences.SampleMemory, Deaths = sampleActive ? sampleDeaths : 0,
                FinalEvents = sampleActive && sampleCompleted == 7 ? sampleFinalEvents.ToArray() : new string[0],
                Level = levelIndex, Loops = savedLoops, SectorLoops = sectorLoops,
                Elapsed = elapsedTotal, SectorElapsed = sectorElapsed, Tapes = savedTapes.ToArray() };
        }
        private void RestoreSession(EchoSessionSave data)
        {
            data.Validate();
            if (data.Layout == 1)
            {
                sampleCompleted = data.CompletedMask; sampleDeaths = data.Deaths;
                preferences.SampleMemory |= data.Memory; LoadSampleStage(data.Stage);
            }
            else LoadLevel(data.Level);
            recordings.Clear(); savedTapes.Clear();
            for (int i = Mathf.Max(0, data.Tapes.Length - MaxEchoes); i < data.Tapes.Length; i++)
            {
                EchoSavedTape tape = data.Tapes[i];
                recordings.Add(tape.Restore(data.Level, data.Layout == 1, data.Stage));
                savedTapes.Add(tape);
            }
            elapsedTotal = data.Elapsed; savedLoops = data.Loops; sectorElapsed = data.SectorElapsed; sectorLoops = data.SectorLoops;
            NewLoop(); SetScreen(sampleActive && sampleCompleted == 7 ? EchoScreen.GameWon : EchoScreen.Playing);
            if (sampleActive && sampleCompleted == 7 && data.FinalEvents != null) sampleFinalEvents.AddRange(data.FinalEvents);
            Notify("Echo kayıtların geri yüklendi. Yeni döngü hazır." + (resumeRecovered ? " Yedek kayıt kullanıldı." : ""));
        }
        private void BeginSector(int index)
        {
            if (index < 0 || index >= EchoLevelDefinition.Campaign.Length || index > preferences.UnlockedLevel) return;
            elapsedTotal = 0; savedLoops = 0;
            if (ChangeScene(CampaignScene(index), saveOnArrival: true)) return;
            LoadLevel(index); SetScreen(EchoScreen.Playing); SaveCheckpoint(index); SaveSession();
        }
        private void RestartSector()
        {
            if (sampleActive) { LoadSampleStage(sampleStage); SetScreen(EchoScreen.Playing); SaveSession(); Notify("Bu kontrol noktasının planı temizlendi. Önceki başarılar korundu."); return; }
            LoadLevel(levelIndex); SetScreen(EchoScreen.Playing); SaveSession(); Notify("Sektör ve Echo kayıtları temizlendi.");
        }
        private void TogglePlanning()
        {
            planning = !planning;
            pendingEchoRecord = false;
            if (planning) { planPreviewTime = clock; SaveSession(); }
            SetCursor(!planning);
        }
        private void CompleteSectorProfile()
        {
            preferences.RecordClear(levelIndex, sectorElapsed, sectorLoops);
            if (CanPersist) preferences.Save();
        }
    }
}
