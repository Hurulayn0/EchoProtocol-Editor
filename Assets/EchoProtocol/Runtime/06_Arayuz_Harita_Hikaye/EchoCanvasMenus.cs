using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EchoProtocol
{
    public sealed partial class EchoPrototypeGame
    {


        private void RefreshGamePanels()
        {
            bool showSettings = screen == EchoScreen.Settings;
            canvasBindings.settingsPanel.gameObject.SetActive(showSettings);
            if (showSettings)
            {
                canvasBindings.settingsInput.interactable = !testing;
                canvasBindings.settingSliders[0].SetValueWithoutNotify(preferences.Sensitivity);
                canvasBindings.settingSliders[1].SetValueWithoutNotify(preferences.Volume);
                canvasBindings.settingSliders[2].SetValueWithoutNotify(preferences.MusicVolume);
                canvasBindings.settingSliders[3].SetValueWithoutNotify(preferences.FieldOfView);
                canvasBindings.settingLabels[0].text = "Fare hassasiyeti: " + preferences.Sensitivity.ToString("0.00");
                canvasBindings.settingLabels[1].text = "Genel ses seviyesi: %" + Mathf.RoundToInt(preferences.Volume * 100);
                canvasBindings.settingLabels[2].text = "Müzik seviyesi: %" + Mathf.RoundToInt(preferences.MusicVolume * 100);
                canvasBindings.settingLabels[3].text = "Görüş açısı: " + Mathf.RoundToInt(preferences.FieldOfView) + "°";
                canvasBindings.invertSetting.text = "DİKEY FARE: " + (preferences.InvertLook ? "TERS" : "NORMAL");
            }
            bool pausedPanel = screen == EchoScreen.Paused;
            bool lost = screen == EchoScreen.GameLost && !sampleActive;
            bool complete = screen == EchoScreen.LevelComplete;
            canvasBindings.statePanel.gameObject.SetActive(pausedPanel || lost || complete);
            if (!canvasBindings.statePanel.gameObject.activeSelf) return;
            canvasBindings.stateInput.interactable = !testing;
            canvasBindings.pausedOptions.SetActive(pausedPanel);
            canvasBindings.resultOptions.SetActive(!pausedPanel);
            canvasBindings.stateTitle.color = lost ? new Color(1, .3f, .25f) : Level.Accent;
            if (pausedPanel)
            {
                canvasBindings.stateTitle.text = "PROTOKOL DURAKLATILDI";
                canvasBindings.statePrimary.text = "DEVAM ET";
                canvasBindings.stateDescription.text = (sampleActive ? SampleGoal() : Level.Objective) + "\nZaman ve muhafızlar sen devam edene kadar bekler.";
                canvasBindings.restartLabel.text = sampleActive ? "BU PLANI BAŞTAN AL" : "SEKTÖRÜ BAŞTAN AL";
            }
            else if (lost)
            {
                canvasBindings.stateTitle.text = "BAĞLANTI KESİLDİ";
                canvasBindings.statePrimary.text = "YENİDEN DENE / ENTER";
                canvasBindings.stateDescription.text = "Muhafız ateşi seni durdurdu. Echo kayıtların korundu.\nEcho'yu yem olarak kullanabilir, sipere geçebilir veya muhafızı önce vurabilirsin.";
            }
            else
            {
                canvasBindings.stateTitle.text = "BÖLÜM TAMAMLANDI  ·  NOT: " + EchoPreferences.RankLetter(lastEarnedRank);
                canvasBindings.statePrimary.text = levelIndex == 0 ? "SONRAKİ: BÖLÜM 2 (ORTA) / ENTER" : "SONRAKİ: BÖLÜM 3 (ZOR) / ENTER";
                canvasBindings.stateDescription.text = Level.Name + " (" + CurrentDifficultyLabel() + ")\nSüre: " + sectorElapsed.ToString("0.0") + "s · Kayıt: " + sectorLoops + " · Alınan Hasar: " + sectorDamageTaken + "\nKayıtların görevini tamamladı. Bir sonraki zorlu protokole geçebilirsin.";
            }
        }

        public void ApplySettingsSliders()
        {
            if (testing || screen != EchoScreen.Settings) return;
            preferences.Sensitivity = canvasBindings.settingSliders[0].value;
            preferences.Volume = canvasBindings.settingSliders[1].value;
            preferences.MusicVolume = canvasBindings.settingSliders[2].value;
            preferences.FieldOfView = canvasBindings.settingSliders[3].value;
            ApplyPreferences();
        }

        public void ActivateGamePanelButton(int choice)
        {
            if (testing) return;
            PlayUiSound(audioBindings.uiClickAudio, .55f);
            switch (choice)
            {
                case 0: preferences.InvertLook = !preferences.InvertLook; ApplyPreferences(); break;
                case 1: preferences.ResetSettings(); ApplyPreferences(); break;
                case 2: CloseSettings(); break;
                case 3:
                    if (screen == EchoScreen.GameLost) RetryLevel();
                    else if (screen == EchoScreen.LevelComplete) ContinueCampaign();
                    else SetScreen(EchoScreen.Playing);
                    break;
                case 4: RestartSector(); break;
                case 5: ReturnToMenu(); break;
                case 6: OpenSettings(); break;
            }
            RefreshCanvasMenus();
        }

        private void RefreshCanvasMenus()
        {
            RefreshGamePanels();
            bool main = screen == EchoScreen.MainMenu;
            canvasBindings.mainMenuPanel.gameObject.SetActive(main);
            canvasBindings.controlsPanel.gameObject.SetActive(screen == EchoScreen.Controls);
            if (!main) return;

            canvasBindings.menuInput.interactable = !testing && !creditsOverlayActive;
            canvasBindings.menuInput.blocksRaycasts = !creditsOverlayActive;
            bool canResume = preferences.ResumeLevel >= 0;
            canvasBindings.continueLabel.transform.parent.gameObject.SetActive(canResume);
            canvasBindings.unavailableLabel.gameObject.SetActive(!canResume);
            canvasBindings.continueLabel.text = resumeSave != null && resumeSave.Layout == 1
                ? "DEVAM / EŞZAMANLILIK " + (resumeSave.Stage + 1)
                : "DEVAM / BÖLÜM " + (preferences.ResumeLevel + 1);
            canvasBindings.unavailableLabel.text = preferences.Completed ? "TÜM BÖLÜMLER TAMAMLANDI" : "KOLAY → ORTA → ZOR → FİNAL";
            canvasBindings.archiveLabel.text = "BELLEK ARŞİVİ / " + ArchiveCount + " / 3";
            canvasBindings.validationLabel.text = testResult;
            canvasBindings.validationLabel.color = SmokeTestPassed ? new Color(.25f, .85f, .55f) : uiMuted;
            for (int i = 0; i < canvasBindings.sectorButtons.Length; i++)
            {
                bool unlocked = i == 0 || i == 2 || (i == 1 && preferences.UnlockedLevel >= 1)
                    || (i == 3 && preferences.UnlockedLevel >= 2);
                int sector = i == 3 ? 2 : i == 2 ? -1 : i;
                int rank = preferences.BestRanks[i == 2 ? 3 : sector];
                string rankTag = rank > 0 ? "  ·  DERECE: " + EchoPreferences.RankLetter(rank) : "";
                canvasBindings.sectorButtons[i].gameObject.SetActive(unlocked);
                canvasBindings.sectorRecords[i].text = sector >= 0 && preferences.BestSeconds[sector] > 0
                    ? "EN İYİ: " + preferences.BestSeconds[sector].ToString("0.0") + "s / " + preferences.BestLoops[sector] + " KAYIT" + rankTag
                    : unlocked ? "OYNAMAYA HAZIR" + rankTag : "ÖNCE ÖNCEKİ BÖLÜMÜ TAMAMLA";
                canvasBindings.sectorRecords[i].color = rank >= 4 ? RankColor(4) : uiMuted;
            }
        }

        public void ActivateMenuButton(int choice)
        {
            if (testing || (screen != EchoScreen.MainMenu && screen != EchoScreen.Controls)) return;
            PlayUiSound(audioBindings.uiClickAudio, .55f);
            switch (choice)
            {
                case 0: BeginCampaign(); break;
                case 1: ContinueSavedCampaign(); break;
                case 2: SetScreen(EchoScreen.Controls); break;
                case 3: OpenSettings(); break;
                case 4: OpenValidation(); break;
                case 5: creditsOverlayActive = true; break;
                case 6: QuitGame(); break;
                case 7: SetScreen(EchoScreen.Archives); break;
                case 8: BeginSample(); break;
                case 9: BeginSector(0); break;
                case 10: BeginSector(1); break;
                case 11: BeginSector(2); break;
                case 12: SetScreen(EchoScreen.MainMenu); break;
            }
            RefreshCanvasMenus();
        }

        private float menuOrbitTime;

        private void TickMenuShowcase(float dt)
        {
            if (testing || automatedRun || player == null || player.View == null) return;
            bool inMenu = screen == EchoScreen.MainMenu || screen == EchoScreen.Controls || screen == EchoScreen.Settings || screen == EchoScreen.Archives;
            if (!inMenu) return;
            menuOrbitTime += dt;
            float angle = Mathf.Sin(menuOrbitTime * 0.28f) * 0.35f;
            Vector3 camPos = new Vector3(Mathf.Sin(angle) * 4.8f, 2.55f + Mathf.Sin(menuOrbitTime * 0.5f) * 0.12f, -6.8f);
            Vector3 lookTarget = new Vector3(Mathf.Sin(menuOrbitTime * 0.18f) * 1.4f, 1.35f, 3.2f);
            player.View.transform.position = camPos;
            player.View.transform.LookAt(lookTarget);
            if (weaponRoot != null) weaponRoot.gameObject.SetActive(false);
        }

        private void RestorePlayerViewFromShowcase()
        {
            if (player == null || player.View == null) return;
            player.View.transform.localPosition = player.CameraPosition;
            player.View.transform.localRotation = player.CameraRotation;
            if (weaponRoot != null) weaponRoot.gameObject.SetActive(true);
        }

        private string CurrentDifficultyLabel()
        {
            if (sampleActive)
                return sampleStage == 0 ? "ZORLUK: ZOR (AŞAMA 1/3)" :
                       sampleStage == 1 ? "ZORLUK: ZOR (AŞAMA 2/3)" : "ZORLUK: ZOR (AŞAMA 3/3)";
            return levelIndex == 0 ? "ZORLUK: KOLAY" :
                   levelIndex == 1 ? "ZORLUK: ORTA" : "ZORLUK: ÇOK ZOR / FİNAL";
        }

        private Color CurrentDifficultyColor()
        {
            if (sampleActive) return new Color(1f, .46f, .22f);
            return levelIndex == 0 ? new Color(.22f, .9f, .55f) :
                   levelIndex == 1 ? new Color(1f, .72f, .2f) : new Color(.95f, .32f, .88f);
        }

        private void QuitGame()
        {
            SetCursor(false);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private bool storyBriefingActive;
        private bool pendingStoryBriefing;
        private bool creditsOverlayActive;
        private float storyOpenedAt;
        private int storyNodeIndex;

        private void QueueStoryBriefing()
        {
            if (IsLiveDifficulty) pendingStoryBriefing = true;
        }

        private void OpenStoryBriefing()
        {
            pendingStoryBriefing = false;
            if (!IsLiveDifficulty) return;

            storyOpenedAt = Time.unscaledTime;
            storyNodeIndex = sampleActive ? sampleStage + 2 : levelIndex == 2 ? 5 : levelIndex;
            storyBriefingActive = true;
            planning = false;
            echoTeaching = false;
            SetCursor(false);
        }

        private void DismissStoryBriefing()
        {
            if (!storyBriefingActive) return;
            storyBriefingActive = false;
            SetCursor(screen == EchoScreen.Playing && !testing && !planning);
            PlayUiSound(audioBindings.uiClickAudio, 0.55f);
        }


        private void RefreshArchiveAndMaintenanceResult()
        {
            bool archives = screen == EchoScreen.Archives;
            canvasBindings.archivePanel.SetActive(archives);
            if (archives)
            {
                bool compact = preferences.SampleMemory;
                canvasBindings.normalArchiveRows.SetActive(!compact);
                canvasBindings.compactArchiveRows.SetActive(compact);
                canvasBindings.archiveMemory.SetActive(compact);
                canvasBindings.archiveCountLabel.text = "VERİ KASASI ARŞİVİ / " + ArchiveCount + " / 3";
                canvasBindings.archiveMemoryCountLabel.text = canvasBindings.archiveCountLabel.text + "  + BAKIM KAYDI";
                canvasBindings.archiveCountLabel.gameObject.SetActive(!compact);
                canvasBindings.archiveMemoryCountLabel.gameObject.SetActive(compact);
                canvasBindings.archiveBonusSummary.text = ActiveVaultBonusText();
                canvasBindings.archiveBonusSummary.color = ArchiveCount > 0 ? canvasBindings.archiveBonusActive : uiMuted;
                canvasBindings.archiveControls.interactable = !testing;
                GameObject[] locked = compact ? canvasBindings.compactArchiveLocked : canvasBindings.archiveLocked;
                GameObject[] unlocked = compact ? canvasBindings.compactArchiveUnlocked : canvasBindings.archiveUnlocked;
                for (int i = 0; i < locked.Length; i++)
                {
                    bool found = (preferences.ArchiveMask & (1 << i)) != 0;
                    locked[i].SetActive(!found);
                    unlocked[i].SetActive(found);
                }
            }

            bool won = sampleActive && screen == EchoScreen.GameWon;
            bool lost = sampleActive && screen == EchoScreen.GameLost;
            canvasBindings.maintenanceWonPanel.SetActive(won);
            canvasBindings.maintenanceLostPanel.SetActive(lost);
            if (!won && !lost) return;

            string stats = $"Oyun zamanı: {elapsedTotal:0.0}s  /  Kayıt: {savedLoops}  /  Ölüm: {sampleDeaths}\nGözlem belleği: " +
                (preferences.SampleMemory ? "BULUNDU" : "henüz bulunmadı");
            if (lost)
            {
                canvasBindings.maintenanceLossStats.text = stats;
                canvasBindings.maintenanceLossControls.interactable = !testing;
                return;
            }
            canvasBindings.maintenanceWinControls.interactable = !testing;
            canvasBindings.maintenanceWinTitle.text = "TAHLİYE AÇILDI  ·  NOT: " + EchoPreferences.RankLetter(lastEarnedRank);
            canvasBindings.maintenanceWinTitle.color = RankColor(lastEarnedRank);
            canvasBindings.maintenanceWinStats.text = stats;
            for (int i = 0; i < canvasBindings.maintenanceResultEvents.Length; i++)
            {
                bool available = i < sampleFinalEvents.Count;
                canvasBindings.maintenanceResultEvents[i].gameObject.SetActive(available);
                if (available) canvasBindings.maintenanceResultEvents[i].text = sampleFinalEvents[i];
            }
        }

        public void ActivateArchiveAndResultButton(int action)
        {
            if (testing) return;
            if (screen == EchoScreen.Archives && action == 0)
            {
                PlayUiSound(audioBindings.uiClickAudio, .55f);
                SetScreen(EchoScreen.MainMenu);
                return;
            }
            if (!sampleActive || (screen != EchoScreen.GameLost && screen != EchoScreen.GameWon)) return;
            PlayUiSound(audioBindings.uiClickAudio, .55f);
            switch (action)
            {
                case 1 when screen == EchoScreen.GameLost: RetryLevel(); break;
                case 2: ReturnToMenu(); break;
                case 3 when screen == EchoScreen.GameWon: ContinueToReactor(); break;
                case 4 when screen == EchoScreen.GameWon: BeginSample(); break;
            }
        }


        private void UpdateNarrativeEffects(int briefing, bool final, bool credits)
        {
            bool visible = briefing >= 0 || final || credits;
            canvasBindings.narrativeBackground.transform.parent.gameObject.SetActive(visible);
            if (!visible) return;

            float time = Time.unscaledTime;
            if (briefing >= 0)
                canvasBindings.briefingSignal[briefing].alpha = canvasBindings.briefingGlow[briefing].alpha = .72f + .28f * Mathf.Sin(time * 3.5f);
            if (final) canvasBindings.finalGlow.alpha = .74f + .26f * Mathf.Sin(time * 3f);

            Color background = canvasBindings.narrativeBackground.color;
            background.a = briefing >= 0 ? .86f : .88f;
            canvasBindings.narrativeBackground.color = background;

            Color accent = briefing < 0 ? new Color(.18f, .86f, .96f) :
                briefing == 5 ? new Color(.9f, .35f, .95f) : CurrentDifficultyColor();
            Rect area = canvasBindings.narrativeBackground.rectTransform.rect;
            for (int i = 0; i < canvasBindings.narrativeParticles.Length; i++)
            {
                float seed = i * 37.13f;
                Image particle = canvasBindings.narrativeParticles[i];
                float x = seed * 53.7f % (area.width - 40) + 20 + Mathf.Sin(time * (.4f + (i % 5) * .12f) + seed) * 18;
                float y = area.height - ((time * (14 + (i % 4) * 7) + seed * 19) % (area.height + 20));
                particle.rectTransform.anchoredPosition = new Vector2(x, -y);
                Color tint = i % 3 == 0 ? accent : particle.color;
                tint.a = (.18f + .22f * Mathf.Sin(time * 2.2f + seed)) * (i % 3 == 0 ? 1 : .85f);
                particle.color = tint;
            }
        }

        private void RefreshNarrativePanels()
        {
            int briefingIndex = storyBriefingActive && screen == EchoScreen.Playing ? storyNodeIndex : -1;
            for (int i = 0; i < canvasBindings.briefingPanels.Length; i++) canvasBindings.briefingPanels[i].SetActive(i == briefingIndex);
            if (briefingIndex >= 0)
                canvasBindings.briefingDescriptions[briefingIndex].maxVisibleCharacters = Mathf.FloorToInt((Time.unscaledTime - storyOpenedAt) * 155f) + 24;

            bool final = screen == EchoScreen.GameWon && !sampleActive;
            bool credits = screen == EchoScreen.MainMenu && creditsOverlayActive;
            canvasBindings.finalPanel.SetActive(final);
            canvasBindings.creditsPanel.SetActive(credits);
            UpdateNarrativeEffects(briefingIndex, final, credits);
            if (final)
            {
                Color rank = RankColor(lastEarnedRank);
                canvasBindings.finalRankBackground.color = new Color(rank.r * .18f, rank.g * .18f, rank.b * .18f, canvasBindings.finalRankBackground.color.a);
                foreach (Graphic accent in canvasBindings.finalRankAccents) accent.color = rank;
                canvasBindings.finalRank.text = "  " + EchoPreferences.RankLetter(lastEarnedRank) + " RANK";
                canvasBindings.finalStats[0].text = Mathf.RoundToInt(elapsedTotal) + " sn";
                canvasBindings.finalStats[1].text = savedLoops + " DÖNGÜ";
                canvasBindings.finalStats[2].text = sectorDamageTaken + " HP";
                canvasBindings.finalStats[3].text = ArchiveCount + " / 3";
                RefreshCreditHighlight(canvasBindings.finalCreditHighlights);
            }
            if (credits) RefreshCreditHighlight(canvasBindings.menuCreditHighlights);
        }

        private static void RefreshCreditHighlight(GameObject[] highlights)
        {
            int selected = Mathf.FloorToInt(Time.unscaledTime * 1.15f) % highlights.Length;
            for (int i = 0; i < highlights.Length; i++) highlights[i].SetActive(i == selected);
        }

        public void ActivateNarrativeButton(int action)
        {
            if (testing) return;
            if (action == 0 && storyBriefingActive && screen == EchoScreen.Playing)
            {
                DismissStoryBriefing();
                return;
            }
            if (action == 1 && screen == EchoScreen.MainMenu && creditsOverlayActive)
            {
                PlayUiSound(audioBindings.uiClickAudio, .55f);
                creditsOverlayActive = false;
            }
            if (screen != EchoScreen.GameWon || sampleActive) return;
            if (action != 2 && action != 3) return;
            PlayUiSound(audioBindings.uiClickAudio, .55f);
            if (action == 2) BeginCampaign();
            else ReturnToMenu();
        }

    }
}
