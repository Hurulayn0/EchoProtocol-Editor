using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EchoProtocol
{
    public sealed partial class EchoPrototypeGame
    {
        private readonly Color uiText = new Color(.86f, .9f, .95f);
        private readonly Color uiMuted = new Color(.53f, .63f, .74f);

        [SerializeField] private GameObject combatHud;
        [SerializeField] private GameObject idleReticle, hitReticle, teamHitReticle, headshotReticle;
        [SerializeField] private CanvasGroup damageScreen, damageFront, damageBack, damageRight, damageLeft;
        [SerializeField] private GameObject crouchIndicator;
        [SerializeField] private CanvasGroup empBanner, chapterBanner, chapterBannerUnderEmp;
        [SerializeField] private TMP_Text empBannerLabel, chapterTitle, chapterSubtitle, chapterTitleUnderEmp, chapterSubtitleUnderEmp;
        [SerializeField] private Image chapterAccent, chapterAccentUnderEmp;
        [SerializeField] private Color chapterEasyColor, chapterMediumColor, chapterReactorColor, chapterMaintenanceColor;

        private int displayedChapterDifficulty = -1;

        [SerializeField] private GameObject statusHud, campaignStatus, maintenanceStatus, singleConsoleClock, dualConsoleClock;
        [SerializeField] private TMP_Text sectorName, sectorDifficulty, echoAndGuardCount, playerHealthLabel;
        [SerializeField] private TMP_Text singleLoopClockLabel, dualLoopClockLabel, singlePowerLabel, dualPowerLabel;
        [SerializeField] private TMP_Text consoleAStatus, consoleBStatus, campaignGuidance;
        [SerializeField] private TMP_Text maintenanceDifficulty, maintenanceStage, maintenanceHealth, maintenanceClock, maintenanceCheckpoint;
        [SerializeField] private Image sectorAccent, playerHealthBar, singleLoopBar, dualLoopBar;
        [SerializeField] private Color criticalHealthTint, lateClockTint, completedConsoleTint;

        private void RefreshStatusHud()
        {
            bool visible = screen != EchoScreen.MainMenu && screen != EchoScreen.Controls &&
                           screen != EchoScreen.Settings && screen != EchoScreen.Archives;
            statusHud.SetActive(visible);
            if (!visible) return;
            campaignStatus.SetActive(!sampleActive);
            maintenanceStatus.SetActive(sampleActive);
            float duration = ActiveLoopLength;
            bool late = clock >= duration - 5;
            if (sampleActive)
            {
                maintenanceDifficulty.text = CurrentDifficultyLabel();
                maintenanceStage.text = SampleStageNames[sampleStage];
                maintenanceHealth.text = $"CAN {playerHealth} / {MaxPlayerHealth}  ·  ECHO {ghosts.Count} / {MaxEchoes}";
                maintenanceClock.text = $"DÖNGÜ {clock:00.0} / {duration:0.#}s";
                maintenanceClock.color = clock > duration - 5 ? Color.yellow : uiText;
                maintenanceCheckpoint.text = $"KONTROL NOKTASI {sampleStage + 1} / 3";
                maintenanceCheckpoint.color = Level.Accent;
                return;
            }

            sectorAccent.color = sectorName.color = Level.Accent;
            sectorName.text = Level.Name;
            sectorDifficulty.text = CurrentDifficultyLabel();
            sectorDifficulty.color = CurrentDifficultyColor();
            echoAndGuardCount.text = $"ECHO {ghosts.Count} / {MaxEchoes}    MUHAFIZ {LivingEnemies()}";
            playerHealthLabel.text = $"CAN {playerHealth} / {MaxPlayerHealth}";
            playerHealthBar.fillAmount = (float)playerHealth / MaxPlayerHealth;
            playerHealthBar.color = playerHealth > 30 ? Level.Accent : criticalHealthTint;
            campaignGuidance.text = Guidance();

            bool dual = consoles.Count == 2;
            singleConsoleClock.SetActive(!dual);
            dualConsoleClock.SetActive(dual);
            TMP_Text timer = dual ? dualLoopClockLabel : singleLoopClockLabel;
            timer.text = $"DÖNGÜ {clock:00.0} / {Mathf.RoundToInt(duration)}s";
            timer.color = late ? lateClockTint : uiText;
            (dual ? dualPowerLabel : singlePowerLabel).text = PowerPlateText(power, plateHeld);
            Image loopBar = dual ? dualLoopBar : singleLoopBar;
            loopBar.fillAmount = 1 - clock / duration;
            loopBar.color = Level.Accent;
            if (!dual) return;
            RefreshConsoleLabel(consoleAStatus, consoles[0], "A");
            RefreshConsoleLabel(consoleBStatus, consoles[1], "B");
        }

        private void RefreshConsoleLabel(TMP_Text label, ConsoleState console, string name)
        {
            label.text = name + " / " + PowerPlateText(console.Powered, console.Held);
            label.color = console.Powered && console.Held ? completedConsoleTint : uiMuted;
        }

        private static string PowerPlateText(bool powered, bool held)
        {
            return "GÜÇ " + (powered ? "AÇIK" : "KAPALI") + " / PLAKA " + (held ? "TUTULU" : "BOŞ");
        }

        [SerializeField] private GameObject campaignWeaponPanel, maintenanceWeaponPanel;
        [SerializeField] private TMP_Text campaignWeaponTitle, campaignEnergyLabel, maintenanceEnergyLabel;
        [SerializeField] private Image[] campaignPips, campaignBonusPips, maintenancePips, maintenanceBonusPips;
        [SerializeField] private Color emptyPipColor, lowEnergyColor, hotPipColor, dimHotPipColor, overchargeStart, overchargeEnd, hotWeaponColor;
        [SerializeField] private GameObject[] campaignEchoCards, maintenanceEchoCards;
        [SerializeField] private TMP_Text[] campaignEchoTitles, campaignEchoTasks, maintenanceEchoTasks;
        [SerializeField] private Image[] campaignEchoProgress, maintenanceEchoProgress;

        private void RefreshWeaponAndEchoCards()
        {
            bool visible = statusHud.activeSelf;
            campaignWeaponPanel.SetActive(visible && !sampleActive);
            maintenanceWeaponPanel.SetActive(visible && sampleActive && !MaintenanceResultVisible);
            for (int i = 0; i < campaignEchoCards.Length; i++)
            {
                bool present = visible && i < ghosts.Count;
                campaignEchoCards[i].SetActive(present && !sampleActive && screen == EchoScreen.Playing);
                maintenanceEchoCards[i].SetActive(present && sampleActive);
                if (!present) continue;
                Ghost ghost = ghosts[i];
                if (sampleActive)
                {
                    maintenanceEchoTasks[i].text = SampleCurrentTask(i) + "\n" + SampleTapeEvents(i, 1);
                    maintenanceEchoProgress[i].fillAmount = clock / Mathf.Max(.1f, ghost.Tape.Duration);
                }
                else
                {
                    string state = clock < ghost.Tape.Duration ? "ROTAYI OYNUYOR" : "SON KONUMDA BEKLİYOR";
                    campaignEchoTitles[i].text = $"ECHO {i + 1} / {state}";
                    string assignment = "Plakada bitmeyen kayıt";
                    foreach (ConsoleState item in consoles)
                        if (OnPlate(ghost.FinalPose.Position, item.PlateArea))
                            assignment = "Plaka " + (item.Id == 0 ? "A" : "B") +
                                ((ghost.ConsoleMask & (1 << item.Id)) != 0 ? " + konsol kaydı hazır" : " / konsol etkileşimi eksik");
                    campaignEchoTasks[i].text = assignment;
                    campaignEchoProgress[i].fillAmount = clock / Mathf.Max(.01f, ghost.Tape.Duration);
                }
            }
            if (!visible) return;

            int cap = Mathf.RoundToInt(EffectiveMaxWeaponEnergy);
            int energy = Mathf.FloorToInt(weaponEnergy + .01f);
            if (sampleActive)
            {
                maintenanceEnergyLabel.text = weaponOverheated ? "PULSE CARBINE · AŞIRI ISINDI" :
                    $"PULSE CARBINE · ENERJİ {energy}/{cap} · C:EĞİL";
                maintenanceEnergyLabel.color = weaponOverheated ? hotWeaponColor : Level.Accent;
            }
            else
            {
                campaignWeaponTitle.color = weaponOverheated ? hotWeaponColor : Level.Accent;
                campaignEnergyLabel.text = weaponOverheated ? "AŞIRI ISINDI · SOĞUYOR" : $"ENERJİ {energy} / {cap}  ·  C: EĞİL";
            }

            int filled = IsWeaponOvercharged ? cap : energy;
            Color color = IsWeaponOvercharged ?
                Color.Lerp(overchargeStart, overchargeEnd, Mathf.Sin(Time.unscaledTime * 16f) * .5f + .5f) :
                weaponOverheated ? (Mathf.Sin(Time.unscaledTime * 18f) > 0 ? hotPipColor : dimHotPipColor) :
                filled <= 2 ? lowEnergyColor : Level.Accent;
            UpdateEnergyPips(sampleActive ? maintenancePips : campaignPips, cap == 8, filled, color);
            UpdateEnergyPips(sampleActive ? maintenanceBonusPips : campaignBonusPips, cap == 10, filled, color);
        }

        private void UpdateEnergyPips(Image[] pips, bool visible, int filled, Color color)
        {
            pips[0].transform.parent.gameObject.SetActive(visible);
            if (!visible) return;
            for (int i = 0; i < pips.Length; i++) pips[i].color = i < filled ? color : emptyPipColor;
        }

        [SerializeField] private TMP_Text lockedVaultPrompt, openedVaultPrompt, offlineConsolePrompt, poweredConsolePrompt;

        private void RefreshInteractionPrompt()
        {
            bool vault = false;
            bool console = false;
            bool powered = false;
            if (!sampleActive && screen == EchoScreen.Playing && player != null)
            {
                Transform view = player.View.transform;
                if (Physics.Raycast(view.position, view.forward, out RaycastHit hit, 2.6f, ~(1 << 2), QueryTriggerInteraction.Ignore))
                {
                    vault = hit.transform == archiveTerminal;
                    foreach (ConsoleState item in consoles)
                    {
                        if (hit.transform != item.Visual) continue;
                        console = true;
                        powered = item.Powered;
                        break;
                    }
                }
            }

            lockedVaultPrompt.transform.parent.gameObject.SetActive(vault && !IsVaultHackedThisRun);
            openedVaultPrompt.transform.parent.gameObject.SetActive(vault && IsVaultHackedThisRun);
            offlineConsolePrompt.transform.parent.gameObject.SetActive(console && !powered);
            poweredConsolePrompt.transform.parent.gameObject.SetActive(console && powered);
            if (vault) lockedVaultPrompt.color = openedVaultPrompt.color = Level.Accent;
        }

        [SerializeField] private GameObject sampleDetails, campaignTactics, sampleTactics, testProgressPanel;
        [SerializeField] private TMP_Text sampleObjective, sampleGateHint, sampleDeviceState, sampleArchiveInfo, sampleRecordHint, sampleBottomHint;
        [SerializeField] private TMP_Text campaignInstructions, campaignTacticsTip, resonanceTitle, resonanceBonus, resonanceHint, testProgressLabel;
        [SerializeField] private Image resonanceAccent, campaignRewind, sampleRewind;
        [SerializeField] private GameObject campaignHeadshot, sampleHeadshot, campaignKill, sampleTeamHit;
        [SerializeField] private TMP_Text campaignRewindLabel, sampleTargetThreat, sampleDevicePrompt, sampleEchoHit;
        [SerializeField] private TMP_Text[] upcomingShotLabels, upcomingDeviceLabels;
        [SerializeField] private RectTransform campaignMarkerTemplate, sampleMarkerTemplate, echoPlaybackTemplate, campaignEnemyTemplate, sampleEnemyTemplate;
        [SerializeField] private Color gateReadyTint, vaultBonusTint, targetPlayerTint;
        private readonly Dictionary<RectTransform, List<RectTransform>> tacticalPools = new Dictionary<RectTransform, List<RectTransform>>();
        private readonly Dictionary<RectTransform, int> tacticalCounts = new Dictionary<RectTransform, int>();

        private bool MaintenanceResultVisible => sampleActive && (screen == EchoScreen.GameWon || screen == EchoScreen.GameLost);

        private void RefreshTacticalCanvas()
        {
            bool active = screen == EchoScreen.Playing && player != null;
            campaignTactics.SetActive(active && !sampleActive);
            sampleTactics.SetActive(active && sampleActive);
            sampleDetails.SetActive(statusHud.activeSelf && sampleActive && !MaintenanceResultVisible);
            if (sampleDetails.activeSelf) UpdateSampleDetails();
            tacticalCounts.Clear();
            if (active && sampleActive)
            {
                sampleTargetThreat.gameObject.SetActive(false);
                sampleDevicePrompt.transform.parent.gameObject.SetActive(false);
                sampleEchoHit.gameObject.SetActive(false);
                UpdateSampleTactics();
            }
            else if (active)
            {
                UpdateCampaignTactics();
                UpdateResonancePanel();
            }
            foreach (var entry in tacticalPools)
            {
                tacticalCounts.TryGetValue(entry.Key, out int count);
                for (int i = count; i < entry.Value.Count; i++) entry.Value[i].gameObject.SetActive(false);
            }
            testProgressPanel.SetActive(testing);
            if (testing)
            {
                testProgressLabel.text = $"OTOMATIK TEST / {passedChecks.Count} KONTROL TAMAMLANDI";
                testProgressLabel.color = Level.Accent;
            }
        }

        private RectTransform GetTacticalWidget(RectTransform template, Vector2 point)
        {
            if (!tacticalPools.TryGetValue(template, out List<RectTransform> pool))
            {
                pool = new List<RectTransform>();
                tacticalPools.Add(template, pool);
            }
            tacticalCounts.TryGetValue(template, out int index);
            if (index == pool.Count) pool.Add(Instantiate(template, template.parent));
            tacticalCounts[template] = index + 1;
            RectTransform widget = pool[index];
            widget.anchoredPosition = new Vector2(point.x, -point.y);
            widget.gameObject.SetActive(true);
            return widget;
        }

        private static void SetWidgetLabel(RectTransform widget, string text, Color color)
        {
            var label = widget.Find("Label").GetComponent<TMP_Text>();
            label.text = text;
            label.color = color;
        }

        private static void SetRewindImage(Image image, float amount, Color color)
        {
            image.gameObject.SetActive(amount > 0);
            if (amount <= 0) return;
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, amount);
            image.color = color;
        }

        private void LateUpdate()
        {
            RefreshStatusHud();
            RefreshTacticalCanvas();
            RefreshMinimap();
            RefreshPlanningPanels();
            RefreshInteractionPrompt();
            RefreshNarrativePanels();
            RefreshArchiveAndMaintenanceResult();
            RefreshWeaponAndEchoCards();
            bool playing = screen == EchoScreen.Playing;
            combatHud.SetActive(playing);
            if (!playing) return;

            bool headshot = headshotTimer > 0;
            bool hit = hitTimer > 0;
            bool teamHit = teamHitTimer > .8f && hit;
            idleReticle.SetActive(!headshot && !hit);
            hitReticle.SetActive(!headshot && hit && !teamHit);
            teamHitReticle.SetActive(!headshot && teamHit);
            headshotReticle.SetActive(headshot);
            SetHudOpacity(damageScreen, damageFlash);

            bool showWorldHud = !planning && !storyBriefingActive;
            crouchIndicator.SetActive(showWorldHud && player != null && player.IsCrouching);
            UpdateDamageDirection(showWorldHud);

            bool showEmp = showWorldHud && empCalloutTimer > 0;
            SetHudOpacity(empBanner, showEmp ? empCalloutTimer * 1.2f : 0);
            if (showEmp) empBannerLabel.text = empCalloutText;

            bool showChapter = showWorldHud && chapterBannerTimer > 0 && !string.IsNullOrEmpty(chapterBannerTitle);
            float fade = chapterBannerTimer > 2.8f ? (3.2f - chapterBannerTimer) / .4f :
                         chapterBannerTimer < .7f ? chapterBannerTimer / .7f : 1f;
            SetHudOpacity(chapterBanner, showChapter && !showEmp ? fade : 0);
            SetHudOpacity(chapterBannerUnderEmp, showChapter && showEmp ? fade : 0);
            if (!showChapter) return;

            int chapterDifficulty = sampleActive ? 3 : levelIndex;
            if (displayedChapterDifficulty != chapterDifficulty)
            {
                Color accent = chapterDifficulty == 3 ? chapterMaintenanceColor :
                    chapterDifficulty == 0 ? chapterEasyColor :
                    chapterDifficulty == 1 ? chapterMediumColor : chapterReactorColor;
                chapterAccent.color = chapterAccentUnderEmp.color = accent;
                chapterTitle.color = chapterTitleUnderEmp.color = accent;
                displayedChapterDifficulty = chapterDifficulty;
            }

            (showEmp ? chapterTitleUnderEmp : chapterTitle).text = chapterBannerTitle;
            (showEmp ? chapterSubtitleUnderEmp : chapterSubtitle).text = chapterBannerSub;
        }

        private void UpdateDamageDirection(bool visible)
        {
            float angle = 0;
            float opacity = 0;
            if (visible && damageDirTimer > 0 && player != null)
            {
                Vector3 direction = lastDamageSourcePos - player.transform.position;
                direction.y = 0;
                if (direction.sqrMagnitude >= .01f)
                {
                    angle = Vector3.SignedAngle(player.transform.forward, direction, Vector3.up);
                    opacity = damageDirTimer * 1.4f;
                }
            }

            SetHudOpacity(damageFront, Mathf.Abs(angle) <= 45f ? opacity : 0);
            SetHudOpacity(damageBack, Mathf.Abs(angle) >= 135f ? opacity : 0);
            SetHudOpacity(damageRight, angle > 45f && angle < 135f ? opacity : 0);
            SetHudOpacity(damageLeft, angle < -45f && angle > -135f ? opacity : 0);
        }

        private static void SetHudOpacity(CanvasGroup group, float opacity)
        {
            group.alpha = Mathf.Clamp01(opacity);
            group.gameObject.SetActive(group.alpha > 0);
        }

        private static Matrix4x4 UiMatrix(float width, float height)
        {
            float scale = Mathf.Min(width / 1280f, height / 720f);
            return Matrix4x4.TRS(new Vector3((width - 1280 * scale) * .5f, (height - 720 * scale) * .5f, 0),
                Quaternion.identity, new Vector3(scale, scale, 1));
        }

        private string Guidance()
        {
            if (screen != EchoScreen.Playing) return message;
            string hint;
            if (UnassignedConsole() == null && !doorOpen) hint = "Echo’ların konsol ve plaka görevini tamamlamasını bekle. Yanlış kaydı Q ile geri alabilirsin.";
            else if (!power) hint = "Turuncu konsola nişan al ve E'ye bas. Her konsol bu döngüde yeniden açılmalı.";
            else if (!plateHeld) hint = "Parlayan plakada kalıp R'ye bas. Echo son konumunda bekleyerek plakayı tutar.";
            else if (LivingEnemies() > 0) hint = "Kapı hazır. Kalan muhafızları temizle; Echo ateşini de tekrarlar.";
            else hint = "Geçit açık. Plakaları Echo'larına bırak ve yeşil çıkış alanına ulaş.";
            if (clock >= ActiveLoopLength - 5) hint = "SON " + Mathf.CeilToInt(ActiveLoopLength - clock) + (recordings.Count >= MaxEchoes ? " SANİYE: Echo’lar korunarak yeni döngü başlayacak. " : " SANİYE: otomatik kayıt yaklaşıyor. ") + hint;
            return notificationRemaining > 0 ? message : hint;
        }

        private readonly List<Rect> markerRects = new List<Rect>();
        private string SampleRecordPreview()
        {
            if (replacementEchoSlot >= 0) return "R / Echo " + (replacementEchoSlot + 1) + " kaydını değiştir · Diğer Echo korunur";
            return recordings.Count < 2 ? "R / Rotayı Echo " + (recordings.Count + 1) + " olarak kaydet · T / planı koru, döngüyü başlat" : "R / Değiştirilecek Echo’yu seç · T / kayıtları koru, döngüyü başlat";
        }
        private string SampleArchiveHint()
        {
            if (IsLiveDifficulty)
            {
                if (sampleStageMemoryHacked) return "GÖZLEM BELLEĞİ (ZORUNLU) / İNDİRİLDİ ✓";
                return "ZORUNLU KİLİT / GÖZLEM BELLEĞİ\nSol üst terminalde [E] ile indir.\n(EMP vur, muhafızı durdur veya yem yap)";
            }
            if (preferences.SampleMemory) return "GÖZLEM BELLEĞİ / BULUNDU";
            if (sampleDecoyObserved) return "GÖZLEM ALGILANDI / Arşiv terminalindeki kayıt değişti.";
            return "İSTEĞE BAĞLI / ARŞİV\nKayıt henüz kilitli.\nTesis verilerini çevreden izle.";
        }
        private string SampleTapeEvents(int index, int maxEvents = 3)
        {
            var text = new StringBuilder(); int printed = 0;
            if (ghosts[index].SensorStart >= 0)
            { text.Append(ghosts[index].SensorStart.ToString("0.0")).Append("–").Append(ghosts[index].SensorEnd.ToString("0.0")).Append("s  LENSİ TUT\n"); printed++; }
            foreach (EchoSavedAction action in savedTapes[index].Actions)
            {
                if (printed >= maxEvents) { if (maxEvents >= 3) text.Append("… diğer olaylar kayıtta"); break; }
                string name = action.Action.Kind == EchoActionKind.Fire ? "ATIŞ" : sampleDevices[action.Action.TargetId].Name + " / E";
                text.Append(action.Time.ToString("0.0")).Append("s  ").Append(name).Append(action.Time <= clock ? " ✓" : " →").Append('\n'); printed++;
            }
            return printed == 0 ? "E veya atış olayı yok; yalnızca hareket kaydı." : text.ToString();
        }
        private string SampleCurrentTask(int index)
        {
            Ghost ghost = ghosts[index];
            EchoSavedAction upcoming;
            if (UpcomingEchoShot(index, out upcoming)) return "ATIŞ / " + (upcoming.Time - clock).ToString("0.0") + "s SONRA";
            EchoSavedAction dev;
            if (UpcomingEchoDevice(index, out dev))
            {
                string devName = sampleDevices != null && dev.Action.TargetId >= 0 && dev.Action.TargetId < sampleDevices.Count ? sampleDevices[dev.Action.TargetId].Name : "PANEL";
                return devName + " / " + (dev.Time - clock).ToString("0.0") + "s";
            }
            if (sampleStage == 1 && OnPlate(ghost.Body.transform.position, sampleSensorArea)) return "LENSİ TUTUYOR";
            foreach (EchoSavedAction item in savedTapes[index].Actions)
                if (item.Time > clock) return item.Action.Kind == EchoActionKind.Fire ? "ATIŞA HAZIRLANIYOR" : sampleDevices[item.Action.TargetId].Name + " GÖREVİ";
            return clock < ghost.Tape.Duration ? "ROTAYI TAMAMLIYOR" : "SON KONUMDA";
        }
        private void UpdateSampleDetails()
        {
            sampleObjective.text = SampleGoal();
            sampleGateHint.text = SampleGateReason();
            sampleGateHint.color = doorOpen ? gateReadyTint : uiText;
            string sentryLine = enemies.Count > 0 ? "\nMUHAFIZ / " + enemies[0].Health + "/" + enemies[0].EffectiveMaxHealth + " · " + GuardIntent(enemies[0]) : "";
            string state = sampleStage == 0 ? "SERVİS PENCERESİ / " + Mathf.Max(0, sampleServiceUntil - clock).ToString("0.0") + "s" + sentryLine :
                "ENERJİ / " + ResonanceRemaining.ToString("0.0") + "s\n" + (sampleStage == 1 ? "LENS / " + (SampleSensorHeld() ? "AÇIK" : "KAPALI") + "\nÜST / " + (sampleDevices[0].Powered ? "ETKİN" : "KAPALI") + "  ALT / " + (sampleDevices[1].Powered ? "ETKİN" : "KAPALI") + sentryLine :
                "MUHAFIZ / " + enemies[0].Health + "/" + enemies[0].EffectiveMaxHealth + "\n" + GuardIntent(enemies[0]));
            sampleDeviceState.text = state;
            sampleArchiveInfo.transform.parent.gameObject.SetActive(sampleStage == 2);
            if (sampleStage == 2)
            {
                sampleArchiveInfo.text = SampleArchiveHint();
            }
            sampleRecordHint.text = SampleRecordPreview();
            sampleRecordHint.color = Level.Accent;
            string bottomText = notificationRemaining > 0 ? message : sampleStage == 0 && sampleServiceUntil > clock ?
                "GEÇİT AÇIK / 1,4 sn içinde asansöre koş!" : sampleStage == 0 && recordings.Count == 0 && current.ActionCount > 0 ?
                "SERVİS PANELİ KULLANILDI / Bu işlemi kaydetmek için R'ye bas. Sonraki döngüde Echo paneli açarken sen kapıya koşacaksın." : sampleStage == 0 && recordings.Count == 0 ?
                "SEN KAYDEDİLİYORSUN / Servis paneline git ve E'ye bas. Hareketin ve E etkileşimlerin bu döngüye kaydolur." : sampleStage == 2 && recordings.Count == 0 ?
                "SEN KAYDEDİLİYORSUN / Hareketin ve atışların kaydolur. Muhafızın ve çevrenin davranışlarını gözle." : "SEN KAYDEDİLİYORSUN / Echo planları oynuyor. " + SampleGateReason();
            sampleBottomHint.text = bottomText;
        }
        private void UpdateSampleTactics()
        {
            markerRects.Clear();
            UpdateEchoTactics();
            for (int i = 0; i < ghosts.Count; i++) UpdateEchoPlaybackLabel(i);
            foreach (SampleDevice device in sampleDevices)
            {
                if (device.Id == 2 && (IsLiveDifficulty ? sampleStageMemoryHacked : preferences.SampleMemory)) continue;
                if (sampleStage == 2 && device.Id == 1 && !sampleDevices[0].Powered && Vector3.Distance(player.transform.position, device.Position) > 4) continue;
                if (!IsLiveDifficulty && sampleStage == 2 && device.Id == 2 && (!sampleDecoyObserved || ResonanceRemaining <= 0) && Vector3.Distance(player.transform.position, device.Position) > 3) continue;
                if (!device.Powered) ShowSampleMarker(device.Position, device.Name + (IsLiveDifficulty && device.Id == 2 ? " / [E] ZORUNLU" : " / E"), device.Accent);
            }
            if (sampleStage == 1 && !SampleSensorHeld()) ShowSampleMarker(sampleSensor.position + Vector3.up * .18f, "LENS SENSÖRÜ", Color.cyan);
            if (resonanceNode != null && !resonanceSpent && (sampleStage != 2 || (enemies.Count > 0 && enemies[0].Alive) || (IsLiveDifficulty && !sampleStageMemoryHacked) || (sampleDecoyObserved && !preferences.SampleMemory))) ShowSampleMarker(ResonancePosition, "EMP JENERATÖRÜ / VUR", Level.Accent);
            if (doorOpen) ShowSampleMarker(SampleGatePosition + Vector3.down, "YOL AÇIK", Color.green);
            else if (sampleStage == 0) ShowSampleMarker(SampleGatePosition + Vector3.down, "YÜK ASANSÖRÜ", uiMuted);
            sampleHeadshot.SetActive(headshotTimer > 0);
            SetRewindImage(sampleRewind, rewindFlash > 0 ? 3 + rewindFlash * 10 : 0, Level.Accent);
            RaycastHit hit;
            if (Physics.Raycast(player.View.transform.position, player.View.transform.forward, out hit, 2.6f, ~(1 << 2), QueryTriggerInteraction.Ignore))
                foreach (SampleDevice device in sampleDevices) if (hit.transform == device.Visual)
                { sampleDevicePrompt.transform.parent.gameObject.SetActive(true); sampleDevicePrompt.text = "E / " + device.Name; }
            foreach (EchoEnemy enemy in enemies)
                if (enemy.Charging)
                {
                    sampleTargetThreat.gameObject.SetActive(true);
                    sampleTargetThreat.text = enemy.TargetIsEcho ? "ECHO DİKKATİ ÇEKTİ / Başka açıdan ilerle" : "SEN HEDEFTESİN / Siper al";
                    sampleTargetThreat.color = enemy.TargetIsEcho ? Color.cyan : targetPlayerTint;
                }
        }
        private void ShowSampleMarker(Vector3 position, string label, Color color)
        {
            Vector3 pixel = player.View.WorldToScreenPoint(position);
            Vector3 ui = UiMatrix(Screen.width, Screen.height).inverse.MultiplyPoint3x4(new Vector3(pixel.x, Screen.height - pixel.y, 0));
            string arrow = "";
            if (pixel.z <= 0) { ui.x = player.View.transform.InverseTransformPoint(position).x < 0 ? 354 : 922; ui.y = 478; arrow = "ARKADA / "; }
            else if (ui.x < 354) arrow = "← "; else if (ui.x > 922) arrow = "→ ";
            ui.x = Mathf.Clamp(ui.x, 445, 835); ui.y = Mathf.Clamp(ui.y, 234, 485);
            var widget = GetTacticalWidget(sampleMarkerTemplate, ui);
            var caption = (RectTransform)widget.Find("Caption box");
            var captionTemplate = (RectTransform)sampleMarkerTemplate.Find("Caption box");
            var captionOffset = captionTemplate.anchoredPosition;
            var boxRect = new Rect(ui.x + captionOffset.x, ui.y - captionOffset.y,
                captionTemplate.rect.width, captionTemplate.rect.height);
            const float top = 190;
            const float bottom = 510;
            boxRect.y = Mathf.Clamp(boxRect.y, top, bottom - boxRect.height);
            float preferredY = boxRect.y;
            for (int attempt = 0; attempt < markerRects.Count && markerRects.Exists(other => other.Overlaps(boxRect)); attempt++)
            {
                float step = (boxRect.height + 4) * (attempt + 1);
                boxRect.y = Mathf.Clamp(preferredY + step, top, bottom - boxRect.height);
                if (markerRects.Exists(other => other.Overlaps(boxRect)))
                    boxRect.y = Mathf.Clamp(preferredY - step, top, bottom - boxRect.height);
            }
            markerRects.Add(boxRect);

            caption.anchoredPosition = new Vector2(boxRect.x - ui.x, ui.y - boxRect.y);
            widget.Find("Dot").GetComponent<Image>().color = color;
            SetWidgetLabel(caption, arrow + label + " / " + Vector3.Distance(player.transform.position, position).ToString("0") + "m", color);
        }

        private bool EchoAssigned(ConsoleState item)
        {
            foreach (Ghost ghost in ghosts)
                if ((ghost.ConsoleMask & (1 << item.Id)) != 0 && OnPlate(ghost.FinalPose.Position, item.PlateArea)) return true;
            return false;
        }
        private ConsoleState UnassignedConsole()
        {
            foreach (ConsoleState item in consoles) if (!EchoAssigned(item)) return item;
            return null;
        }
        private void UpdateCampaignTactics()
        {
            ConsoleState goal = UnassignedConsole();
            string instruction = goal != null ? "GÖREV / " + (goal.Id == 0 ? "A" : "B") + " KONSOLUNU ECHO’YA DEVRET"
                : LivingEnemies() > 0 ? "GÖREV / MUHAFIZLARI TEMİZLE" : doorOpen ? "GÖREV / ÇIKIŞA ULAŞ" : "GÖREV / ECHO’LARIN KONUMA ULAŞMASINI BEKLE";
            campaignInstructions.text = instruction;
            campaignInstructions.color = Level.Accent;
            campaignTacticsTip.text = goal != null ? "Konsol E → plakada bekle → R ile kaydet" : "Kırmızı lazer: siper al. Echo’nun ateş hattını açık tut.";
            if (goal != null)
                ShowWorldMarker(goal.Powered ? goal.Plate + Vector3.up * .35f : goal.Position,
                    goal.Powered ? "PLAKA / R" : "KONSOL / E", Level.Accent);
            else if (doorOpen && LivingEnemies() == 0) ShowWorldMarker(new Vector3(0, .4f, 11), "ÇIKIŞ", new Color(.25f, .9f, .55f));
            foreach (EchoEnemy enemy in enemies)
            {
                if (!enemy.Alive) continue;
                Vector3 aim = enemy.transform.position + Vector3.up * 1.4f;
                RaycastHit hit;
                if (Physics.Raycast(player.View.transform.position, aim - player.View.transform.position, out hit,
                    Vector3.Distance(player.View.transform.position, aim), ~(1 << 2)) && hit.transform != enemy.transform) continue;
                Vector3 pixel = player.View.WorldToScreenPoint(enemy.transform.position + Vector3.up * 2.05f);
                if (pixel.z <= 0) continue;
                Vector3 ui = UiMatrix(Screen.width, Screen.height).inverse.MultiplyPoint3x4(new Vector3(pixel.x, Screen.height - pixel.y, 0));
                if (ui.x < 370 || ui.x > 900 || ui.y < 120 || ui.y > 550) continue;
                var widget = GetTacticalWidget(campaignEnemyTemplate, ui);
                widget.Find("Fill").GetComponent<Image>().fillAmount = (float)enemy.Health / Mathf.Max(1, enemy.EffectiveMaxHealth);
                widget.Find("Label").gameObject.SetActive(enemy.Charging);
            }
            campaignKill.SetActive(killTimer > 0);
        }
        private void ShowWorldMarker(Vector3 position, string label, Color tint)
        {
            Vector3 pixel = player.View.WorldToScreenPoint(position); if (pixel.z <= 0) return;
            Vector3 ui = UiMatrix(Screen.width, Screen.height).inverse.MultiplyPoint3x4(new Vector3(pixel.x, Screen.height - pixel.y, 0));
            if (ui.x < 350 || ui.x > 900 || ui.y < 130 || ui.y > 570) return;
            var widget = GetTacticalWidget(campaignMarkerTemplate, ui);
            widget.Find("Dot").GetComponent<Image>().color = tint;
            SetWidgetLabel(widget, label + " / " + Vector3.Distance(player.transform.position, position).ToString("0") + "m", tint);
        }

        private void UpdateEchoPlaybackLabel(int index)
        {
            Vector3 pixel = player.View.WorldToScreenPoint(ghosts[index].Body.transform.position + Vector3.up * 2.1f);
            if (pixel.z < .5f) return;
            Vector3 ui = UiMatrix(Screen.width, Screen.height).inverse.MultiplyPoint3x4(new Vector3(pixel.x, Screen.height - pixel.y, 0));
            if (ui.x < 354 || ui.x > 922 || ui.y < 150 || ui.y > 500) return;
            float x = Mathf.Clamp(ui.x - 86 + index * 30, 330, 752);
            SetWidgetLabel(GetTacticalWidget(echoPlaybackTemplate, new Vector2(x, ui.y)), "ECHO " + (index + 1) + " / KAYDI OYNUYOR\n" + SampleCurrentTask(index), index == 0 ? Color.cyan : new Color(.8f, .5f, 1));
        }

        private void UpdateResonancePanel()
        {
            if (screen != EchoScreen.Playing) return;
            bool vaultOpenNow = !IsVaultHackedThisRun && IsVaultUnlockedNow();
            resonanceAccent.color = resonanceTitle.color = vaultOpenNow ? Color.green : Level.Accent;
            resonanceTitle.text = "EMP JENERATÖRÜ & VERİ KASASI (" + ArchiveCount + "/3)";
            resonanceBonus.text = ActiveVaultBonusText();
            resonanceBonus.color = ArchiveCount > 0 ? vaultBonusTint : uiMuted;
            string hint = IsVaultHackedThisRun
                ? "KASA HACKLENDİ! EMP Küresini istediğin zaman vur: Tüm muhafızlara -20 Can & 3.5 sn elektrik şoku +25 Can +4 sn Aşırı Yükleme!"
                : vaultOpenNow
                    ? "KASA KİLİDİ AÇIK (" + (ResonanceRemaining > 0 ? ResonanceRemaining.ToString("0.0") + "s" : "AKTİF") + ")! HEMEN KASAYA GİT VE [E]'YE BAS → 100 CAN + KALICI SİLAH GÜÇLENMESİ!"
                    : "1) Parlayan halkalı EMP Küresini vur: Tüm odaya elektrik şoku (-20 Can & 3.5sn felç) +25 Can +4sn Aşırı Yükleme!\n2) Küreyi vurunca Veri Kasası 10sn açılır → Kasada [E]'ye bas!";
            resonanceHint.text = hint;
            resonanceHint.color = vaultOpenNow ? Color.green : uiText;
            if (!IsVaultHackedThisRun) ShowWorldMarker(ArchivePosition, vaultOpenNow ? "KASA AÇIK / [E] HACKLE" : "VERİ KASASI (ÖNCE EMP VUR)", vaultOpenNow ? Color.green : new Color(1, .8f, .3f));
            if (!resonanceSpent) ShowWorldMarker(ResonancePosition, "EMP KÜRESİ / ATEŞ ET", Color.cyan);
            campaignHeadshot.SetActive(headshotTimer > 0);
            bool rewinding = rewindFlash > 0 && !planning;
            campaignRewindLabel.gameObject.SetActive(rewinding);
            SetRewindImage(campaignRewind, rewinding ? 3 + rewindFlash * 12 : 0, Level.Accent);
            if (rewinding) campaignRewindLabel.color = Level.Accent;
        }

        private void UpdateEchoTactics()
        {
            sampleTeamHit.SetActive(teamHitTimer > 0);
            for (int i = 0; i < upcomingShotLabels.Length; i++)
            {
                upcomingShotLabels[i].gameObject.SetActive(false);
                upcomingDeviceLabels[i].gameObject.SetActive(false);
            }
            for (int i = 0; i < ghosts.Count; i++)
            {
                EchoSavedAction shot;
                if (UpcomingEchoShot(i, out shot))
                {
                    upcomingShotLabels[i].gameObject.SetActive(true);
                    upcomingShotLabels[i].text = "ECHO " + (i + 1) + " / ATIŞ " + (shot.Time - clock).ToString("0.0") + "s · Renkli çizgi kayıtlı atış hattı";
                }
                EchoSavedAction dev;
                if (UpcomingEchoDevice(i, out dev))
                {
                    string devName = sampleDevices != null && dev.Action.TargetId >= 0 && dev.Action.TargetId < sampleDevices.Count ? sampleDevices[dev.Action.TargetId].Name : "PANEL";
                    upcomingDeviceLabels[i].gameObject.SetActive(true);
                    upcomingDeviceLabels[i].text = "ECHO " + (i + 1) + " / " + devName + " ETKİLEŞİMİ " + (dev.Time - clock).ToString("0.0") + "s";
                }
            }
            if (sampleStage != 2) return;
            foreach (EchoEnemy enemy in enemies)
            {
                if (!enemy.Alive) continue;
                float remaining = EchoEnemy.TeamHitWindow - (clock - enemy.LastHitTime);
                if (enemy.LastHitActor > 0 && remaining > 0 && teamHitTimer <= 0)
                {
                    sampleEchoHit.gameObject.SetActive(true);
                    sampleEchoHit.text = "ECHO İSABETİ / Eşlik et: " + remaining.ToString("0.0") + "s";
                }
                Vector3 pos = enemy.transform.position + Vector3.up * 2.25f;
                Vector3 pixel = player.View.WorldToScreenPoint(pos);
                if (pixel.z <= .5f) continue;
                Vector3 ui = UiMatrix(Screen.width, Screen.height).inverse.MultiplyPoint3x4(new Vector3(pixel.x, Screen.height - pixel.y, 0));
                if (ui.x < 380 || ui.x > 900 || ui.y < 234 || ui.y > 480) continue;
                RaycastHit obstruction;
                if (Physics.Linecast(player.View.transform.position, pos, out obstruction, ~(1 << 2), QueryTriggerInteraction.Ignore) && obstruction.collider.GetComponent<EchoEnemy>() != enemy) continue;
                SetWidgetLabel(GetTacticalWidget(sampleEnemyTemplate, ui), "MUHAFIZ " + enemy.Health + "/60\n" + GuardIntent(enemy), enemy.TargetIsEcho ? Color.cyan : uiText);
            }
        }
    }
}
