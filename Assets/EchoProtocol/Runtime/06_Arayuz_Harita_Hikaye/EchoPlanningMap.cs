using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

namespace EchoProtocol
{
    public sealed partial class EchoPrototypeGame
    {
        private static Vector2 MapPoint(Rect map, Vector3 point)
        {
            return new Vector2(map.x + (point.x + 8) / 16 * map.width, map.yMax - (point.z + 9) / 24 * map.height);
        }

        private static Matrix4x4 MapLineMatrix(Matrix4x4 canvas, Vector2 a, Vector2 b)
        {
            return canvas * Matrix4x4.TRS(new Vector3(a.x, a.y, 0),
                Quaternion.Euler(0, 0, Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg), Vector3.one);
        }



        [SerializeField] private GameObject minimapPanel;
        [SerializeField] private RectTransform minimapArea, minimapSweep, minimapSweepGlow;
        [SerializeField] private CanvasRenderer minimapRenderer;
        [SerializeField] private TMP_Text minimapLabelTemplate, minimapHeading, minimapNorth;
        [SerializeField] private Image minimapAccent, minimapOutline, minimapTelemetry;
        [SerializeField] private Image[] minimapAccentDetails;
        [SerializeField] private RawImage minimapScanlines;

        private readonly List<Vector3> minimapVertices = new List<Vector3>();
        private readonly List<int> minimapIndices = new List<int>();
        private readonly List<Color32> minimapColors = new List<Color32>();
        private readonly Dictionary<TMP_Text, List<TMP_Text>> markerPools = new Dictionary<TMP_Text, List<TMP_Text>>();
        private TMP_Text activeMarkerTemplate;
        private List<TMP_Text> minimapLabels;
        private Mesh minimapMesh;
        private int minimapLabelCount;
        private readonly List<(Vector2 Position, string Caption, Color Color)> pendingMapLabels =
            new List<(Vector2, string, Color)>();
        private readonly List<Rect> occupiedMapAreas = new List<Rect>();


        private void RefreshMinimap()
        {
            bool visible = screen == EchoScreen.Playing && !planning && player != null;
            minimapPanel.SetActive(visible);
            if (!visible) return;

            BeginMapGeometry(minimapLabelTemplate);
            float time = Time.unscaledTime;
            Color accent = Level.Accent;
            minimapAccent.color = minimapHeading.color = accent;
            ApplyMinimapAccent(minimapOutline, accent);
            foreach (Image detail in minimapAccentDetails) ApplyMinimapAccent(detail, accent);
            Color north = minimapNorth.color;
            minimapNorth.color = new Color(accent.r, accent.g, accent.b, north.a);
            Color telemetry = minimapTelemetry.color;
            telemetry.a = .68f + .32f * Mathf.Sin(time * 4.2f);
            minimapTelemetry.color = telemetry;
            Rect area = minimapArea.rect;
            float sweep = time * 36f % area.height;
            minimapSweep.anchoredPosition = new Vector2(minimapSweep.anchoredPosition.x, -sweep);
            minimapSweepGlow.anchoredPosition = new Vector2(minimapSweepGlow.anchoredPosition.x, -Mathf.Max(0, sweep - minimapSweepGlow.rect.height));
            Rect uv = minimapScanlines.uvRect;
            uv.y = time * .1f;
            minimapScanlines.uvRect = uv;

            RenderMinimapGeometry(new Rect(0, 0, area.width, area.height), accent);
            FinishMapGeometry(minimapRenderer);
        }

        private void BeginMapGeometry(TMP_Text template)
        {
            minimapVertices.Clear();
            minimapIndices.Clear();
            minimapColors.Clear();
            minimapLabelCount = 0;
            pendingMapLabels.Clear();
            occupiedMapAreas.Clear();
            activeMarkerTemplate = template;
            if (!markerPools.TryGetValue(template, out minimapLabels))
            {
                minimapLabels = new List<TMP_Text>();
                markerPools.Add(template, minimapLabels);
            }
        }

        private void FinishMapGeometry(CanvasRenderer renderer)
        {
            PlaceMapLabels();
            for (int i = minimapLabelCount; i < minimapLabels.Count; i++) minimapLabels[i].gameObject.SetActive(false);

            if (minimapMesh == null)
            {
                minimapMesh = new Mesh { name = "Live minimap" };
                minimapMesh.MarkDynamic();
            }
            minimapMesh.Clear();
            minimapMesh.SetVertices(minimapVertices);
            minimapMesh.SetColors(minimapColors);
            minimapMesh.SetTriangles(minimapIndices, 0);
            renderer.SetMaterial(Graphic.defaultGraphicMaterial, Texture2D.whiteTexture);
            renderer.SetMesh(minimapMesh);
        }

        private static void ApplyMinimapAccent(Image image, Color accent)
        {
            image.color = new Color(accent.r, accent.g, accent.b, image.color.a);
        }

        private void AddMinimapQuad(Rect rect, Color color)
        {
            AddMinimapGeometry(new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMin, rect.yMax),
                new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMax, rect.yMin), color);
        }

        private void AddMinimapLine(Vector2 a, Vector2 b, Color color, float width = 2)
        {
            Vector2 delta = b - a;
            Vector2 perpendicular = new Vector2(-delta.y, delta.x).normalized * (width * .5f);
            AddMinimapGeometry(a - perpendicular, a + perpendicular, b + perpendicular, b - perpendicular, color);
        }

        private void AddMinimapGeometry(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color)
        {
            int start = minimapVertices.Count;
            minimapVertices.Add(new Vector3(a.x, -a.y));
            minimapVertices.Add(new Vector3(b.x, -b.y));
            minimapVertices.Add(new Vector3(c.x, -c.y));
            minimapVertices.Add(new Vector3(d.x, -d.y));
            for (int i = 0; i < 4; i++) minimapColors.Add(color);
            minimapIndices.Add(start);
            minimapIndices.Add(start + 1);
            minimapIndices.Add(start + 2);
            minimapIndices.Add(start);
            minimapIndices.Add(start + 2);
            minimapIndices.Add(start + 3);
        }

        private void AddMinimapLabel(Vector2 point, string caption, Color color)
        {
            pendingMapLabels.Add((point, caption, color));
        }

        private void PlaceMapLabels()
        {
            var area = (RectTransform)activeMarkerTemplate.transform.parent;
            const float padding = 2;
            float maxWidth = Mathf.Min(activeMarkerTemplate.rectTransform.rect.width, area.rect.width - padding * 2);
            foreach (var item in pendingMapLabels)
            {
                if (minimapLabelCount == minimapLabels.Count)
                    minimapLabels.Add(Instantiate(activeMarkerTemplate, area));

                TMP_Text label = minimapLabels[minimapLabelCount++];
                label.text = item.Caption;
                label.color = item.Color;
                Vector4 margin = label.margin;
                Vector2 preferred = label.GetPreferredValues(item.Caption, maxWidth - margin.x - margin.z, 0);
                float width = Mathf.Min(maxWidth, Mathf.Ceil(preferred.x + margin.x + margin.z) + 1);
                float height = Mathf.Ceil(preferred.y + margin.y + margin.w) + 1;
                var bounds = new Rect(item.Position, new Vector2(width, height));
                Vector2 offset = activeMarkerTemplate.rectTransform.anchoredPosition;
                bounds.position += new Vector2(offset.x, -offset.y);
                var limit = new Vector2(area.rect.width - width - padding, area.rect.height - height - padding);
                bounds.position = FindFreeMapLabelPosition(bounds, limit, padding);

                label.rectTransform.anchoredPosition = new Vector2(bounds.x, -bounds.y);
                label.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);
                label.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                label.gameObject.SetActive(true);
                occupiedMapAreas.Add(bounds);
            }
        }

        private Vector2 FindFreeMapLabelPosition(Rect bounds, Vector2 limit, float padding)
        {
            Vector2 preferred = bounds.position;
            Vector2 nearest = new Vector2(Mathf.Clamp(preferred.x, padding, limit.x),
                Mathf.Clamp(preferred.y, padding, limit.y));
            float bestDistance = float.PositiveInfinity;
            int steps = Mathf.CeilToInt(limit.y / Mathf.Max(1, bounds.height));
            for (int column = -1; column <= 1; column++)
            {
                bounds.x = Mathf.Clamp(preferred.x + column * (bounds.width + 12), padding, limit.x);
                for (int row = -steps; row <= steps; row++)
                {
                    bounds.y = Mathf.Clamp(preferred.y + row * (bounds.height + padding), padding, limit.y);
                    float distance = (bounds.position - preferred).sqrMagnitude;
                    if (distance >= bestDistance) continue;
                    bool free = true;
                    foreach (Rect occupied in occupiedMapAreas)
                        if (occupied.Overlaps(bounds))
                        {
                            free = false;
                            break;
                        }
                    if (!free) continue;
                    nearest = bounds.position;
                    bestDistance = distance;
                }
            }
            return nearest;
        }

        private void RenderMinimapGeometry(Rect map, Color accent)
        {
            // Room structure, conduits & gate
            if (sampleActive)
            {
                float wallZ = sampleStage == 0 ? 3.5f : 7f;
                AddMinimapLine(MapPoint(map, new Vector3(-8, 0, wallZ)), MapPoint(map, new Vector3(8, 0, wallZ)), new Color(.45f, .58f, .72f, .85f), 2.5f);
                Vector3 gatePos = SampleGatePosition;
                Color gateCol = doorOpen ? new Color(.2f, .98f, .55f) : new Color(1f, .46f, .18f);
                AddMinimapLine(MapPoint(map, new Vector3(gatePos.x - 1.7f, 0, gatePos.z)),
                        MapPoint(map, new Vector3(gatePos.x + 1.7f, 0, gatePos.z)), gateCol, 5f);
                if (sampleStage == 2)
                {
                    Vector2 bar = MapPoint(map, new Vector3(0, 0, -3.5f));
                    AddMinimapCover(bar, 32, 8);
                }
                if (sampleStage == 1)
                {
                    AddMinimapMarker(map, SampleSensorPosition, SampleSensorHeld() ? Color.green : Color.cyan, 7, "L", SampleSensorHeld());
                    if (IsLiveDifficulty && sampleDevices.Count >= 2)
                    {
                        Color condCol = ResonanceRemaining > 0 ? new Color(.2f, .95f, .55f, .75f) : new Color(accent.r, accent.g, accent.b, .35f);
                        AddMinimapLine(MapPoint(map, ResonancePosition), MapPoint(map, sampleDevices[0].Position), condCol, 1.5f);
                        AddMinimapLine(MapPoint(map, ResonancePosition), MapPoint(map, sampleDevices[1].Position), condCol, 1.5f);
                    }
                }
                foreach (SampleDevice dev in sampleDevices)
                {
                    bool memDone = dev.Id == 2 && (IsLiveDifficulty ? sampleStageMemoryHacked : preferences.SampleMemory);
                    if (memDone) continue;
                    AddMinimapMarker(map, dev.Position, dev.Powered ? Color.green : dev.Accent, 7, dev.Id == 0 ? "1" : dev.Id == 1 ? "2" : "G", !dev.Powered);
                }
                if (resonanceNode != null && !resonanceSpent)
                    AddMinimapMarker(map, ResonancePosition, Color.cyan, 7, "P", true);
                AddMinimapMarker(map, gatePos + Vector3.forward * 2.2f, doorOpen ? Color.green : uiMuted, 7, "Ç", doorOpen);
            }
            else
            {
                AddMinimapLine(MapPoint(map, new Vector3(-8, 0, 5)), MapPoint(map, new Vector3(8, 0, 5)), new Color(.45f, .58f, .72f, .85f), 2.5f);
                Color gateCol = doorOpen ? new Color(.2f, .98f, .55f) : new Color(1f, .46f, .18f);
                AddMinimapLine(MapPoint(map, new Vector3(-2.1f, 0, 5)), MapPoint(map, new Vector3(2.1f, 0, 5)), gateCol, 5f);
                foreach (Vector3 cover in Level.Covers)
                    AddMinimapCover(MapPoint(map, cover), 20, 10);

                // EMP -> Vault power conduit line
                Color condCol = (IsVaultHackedThisRun || ResonanceRemaining > 0) ? new Color(.22f, .95f, .55f, .75f) : new Color(1f, .55f, .2f, .38f);
                AddMinimapLine(MapPoint(map, ResonancePosition), MapPoint(map, ArchivePosition), condCol, 1.6f);

                foreach (ConsoleState item in consoles)
                    AddMinimapMarker(map, item.Plate, item.Held && item.Powered ? Color.green : new Color(1f, .72f, .2f), 7, item.Id == 0 ? "A" : "B", !(item.Held && item.Powered));
                AddMinimapMarker(map, ResonancePosition, resonanceSpent ? uiMuted : Color.cyan, 6, "P", !resonanceSpent);
                bool vaultDone = IsVaultHackedThisRun;
                AddMinimapMarker(map, ArchivePosition, vaultDone ? Color.green : (IsVaultUnlockedNow() ? Color.green : new Color(1f, .78f, .25f)), 6, "V", !vaultDone && IsVaultUnlockedNow());
                AddMinimapMarker(map, new Vector3(0, 0, 12), doorOpen ? Color.green : new Color(.22f, .72f, .45f), 7, "Ç", doorOpen);
            }

            // Saved Echo routes & live Ghost positions
            for (int i = 0; i < recordings.Count; i++)
            {
                var tape = recordings[i];
                Color trailCol = i == 0 ? new Color(.12f, .9f, 1f, .55f) : new Color(.82f, .45f, 1f, .55f);
                Color dotCol = i == 0 ? Color.cyan : new Color(.86f, .5f, 1f);
                Vector2 prev = MapPoint(map, tape.Sample(0, EchoPose.Lerp).Position);
                for (float tm = .4f; tm < tape.Duration + .4f; tm += .4f)
                {
                    Vector2 next = MapPoint(map, tape.Sample(Mathf.Min(tm, tape.Duration), EchoPose.Lerp).Position);
                    AddMinimapLine(prev, next, trailCol, 1.8f);
                    prev = next;
                }
                if (i < ghosts.Count && ghosts[i].Body != null)
                    AddMinimapMarker(map, ghosts[i].Body.transform.position, dotCol, 7, "E" + (i + 1), false);
            }

            // Live Sentry enemies with threat pulse ring
            foreach (EchoEnemy enemy in enemies)
            {
                if (!enemy.Alive) continue;
                bool stunned = enemy.StunRemaining > 0;
                Color eCol = stunned ? new Color(.35f, .78f, 1f) :
                             enemy.Charging ? ((Mathf.Sin(clock * 24f) > 0) ? Color.yellow : new Color(1f, .18f, .18f)) : new Color(1f, .25f, .22f);
                AddMinimapMarker(map, enemy.transform.position, eCol, 8, "M", !stunned);
            }

            // Live Player marker + translucent tactical FOV vision cone
            Vector2 pPos = MapPoint(map, player.transform.position);
            pPos.x = Mathf.Clamp(pPos.x, map.x + 6, map.xMax - 6);
            pPos.y = Mathf.Clamp(pPos.y, map.y + 6, map.yMax - 6);
            Vector3 fwd = player.transform.forward;
            Vector2 pDir = new Vector2(fwd.x, -fwd.z).normalized;
            float baseAngle = Mathf.Atan2(pDir.y, pDir.x);
            for (int r = -2; r <= 2; r++)
            {
                float a = baseAngle + r * .24f;
                Vector2 rayDir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                float alpha = r == 0 ? .55f : (Mathf.Abs(r) == 1 ? .32f : .18f);
                AddMinimapLine(pPos, pPos + rayDir * 20f, new Color(.65f, .96f, 1f, alpha), r == 0 ? 2f : 1.3f);
            }
            AddMinimapQuad(new Rect(pPos.x - 5, pPos.y - 5, 10, 10), new Color(.2f, .92f, 1f, .38f));
            AddMinimapQuad(new Rect(pPos.x - 3.5f, pPos.y - 3.5f, 7, 7), Color.white);

        }

        private void AddMinimapCover(Vector2 center, float w, float h)
        {
            AddMinimapQuad(new Rect(center.x - w * .5f, center.y - h * .5f, w, h), new Color(.22f, .3f, .4f, .95f));
            AddMinimapQuad(new Rect(center.x - w * .5f, center.y - h * .5f, w, 2), new Color(.52f, .68f, .84f, .85f));
        }

        private void AddMinimapMarker(Rect map, Vector3 worldPos, Color color, float size, string shortTag, bool pulseRing = false)
        {
            Vector2 p = MapPoint(map, worldPos);
            p.x = Mathf.Clamp(p.x, map.x + 5, map.xMax - 5);
            p.y = Mathf.Clamp(p.y, map.y + 5, map.yMax - 5);
            if (pulseRing)
            {
                float ring = size + 4f + Mathf.Sin(Time.unscaledTime * 6f) * 2.5f;
                AddMinimapQuad(new Rect(p.x - ring * .5f, p.y - ring * .5f, ring, ring), new Color(color.r, color.g, color.b, .26f));
            }
            var dot = new Rect(p.x - size * .5f, p.y - size * .5f, size, size);
            AddMinimapQuad(dot, color);
            occupiedMapAreas.Add(dot);
            if (!string.IsNullOrEmpty(shortTag))
                AddMinimapLabel(new Vector2(p.x + size * .5f + 2, p.y - 8), shortTag, color);
        }
        private bool echoTeaching, pendingEchoRecord;
        private int replacementEchoSlot = -1;
        private float planPreviewTime;

        private void FinishEchoTeaching()
        {
            echoTeaching = false; planning = false; SetCursor(!testing);
            Notify("Hareketin ve E etkileşimlerin kaydediliyor. R ile bu kaydı Echo olarak oynat.");
        }
        private void OpenEchoRecordReview()
        {
            TogglePlanning(); pendingEchoRecord = true;
        }
        private void ChooseEchoReplacement(int index)
        {
            if (!planning || index < 0 || index >= recordings.Count) return;
            if (pendingEchoRecord) { CommitLoop(index); return; }
            TogglePlanning(); replacementEchoSlot = index;
            Notify("Sonraki R bu denemeyi Echo " + (index + 1) + " yerine kaydeder. Diğer Echo korunur. T kayıtları koruyup döngüyü başlatır.");
        }
        private void CommitReviewedNewEcho()
        {
            if (sampleActive && planning && pendingEchoRecord && recordings.Count < MaxEchoes) CommitLoop();
        }
        private void CancelEchoRecordReview()
        {
            if (pendingEchoRecord && clock >= ActiveLoopLength) RetryLevel();
            else TogglePlanning();
        }

        [SerializeField] private GameObject campaignPlan, maintenancePlan, echoTeachingPanel;
        [SerializeField] private RectTransform campaignPlanArea, maintenancePlanArea;
        [SerializeField] private CanvasRenderer campaignPlanRenderer, maintenancePlanRenderer;
        [SerializeField] private TMP_Text campaignPlanMarker, maintenancePlanMarker;
        [SerializeField] private TMP_Text campaignPlanHeading, campaignPlanGoal, maintenancePlanHeading, maintenancePlanHelp;
        [SerializeField] private TMP_Text maintenancePlanGoal, maintenancePlanGate, maintenancePlanPreview;
        [SerializeField] private TMP_Text[] campaignPlanCards, campaignPlanActions, maintenancePlanCards, maintenancePlanActions;
        [SerializeField] private TMP_Text maintenanceCloseAction;
        [SerializeField] private Button[] campaignPlanButtons, maintenancePlanButtons;
        [SerializeField] private Slider maintenancePlanSlider;
        [SerializeField] private CanvasGroup campaignPlanInput, maintenancePlanInput, teachingPlanInput;

        private void RefreshPlanningPanels()
        {
            bool visible = screen == EchoScreen.Playing && planning;
            bool campaign = visible && !sampleActive;
            bool maintenance = visible && sampleActive && !echoTeaching;
            campaignPlan.SetActive(campaign);
            maintenancePlan.SetActive(maintenance);
            echoTeachingPanel.SetActive(visible && sampleActive && echoTeaching);
            teachingPlanInput.interactable = !testing;
            if (!campaign && !maintenance) return;

            if (campaign)
            {
                campaignPlanInput.interactable = !testing;
                campaignPlanHeading.color = Level.Accent;
                campaignPlanGoal.text = Level.Objective;
                for (int i = 0; i < campaignPlanCards.Length; i++)
                {
                    bool recorded = i < recordings.Count;
                    bool selected = replacementEchoSlot == i;
                    campaignPlanCards[i].text = recorded ? $"ECHO {i + 1} / {recordings[i].Duration:0.0}s\n" +
                        (selected ? "SEÇİLDİ (SONRAKİ R)" : "KORUNUYOR") : $"ECHO {i + 1} / BOŞ";
                    campaignPlanButtons[i].gameObject.SetActive(recorded);
                    campaignPlanActions[i].text = selected ? "SEÇİLDİ" : "R İLE DEĞİŞTİR";
                }
                BeginMapGeometry(campaignPlanMarker);
                RenderCampaignPlan(new Rect(Vector2.zero, campaignPlanArea.rect.size));
                FinishMapGeometry(campaignPlanRenderer);
                return;
            }

            maintenancePlanInput.interactable = !testing;
            bool expired = clock >= ActiveLoopLength;
            maintenancePlanHeading.text = pendingEchoRecord ?
                recordings.Count == MaxEchoes ? "YENİ KAYIT HAZIR / HANGİ ECHO DEĞİŞSİN?" : "20 SANİYE DOLDU / BU KAYDI SAKLA?" :
                "ECHO PLANI / ZAMAN DURDU";
            maintenancePlanHelp.text = pendingEchoRecord ?
                expired ? "Kaydı saklayabilir veya bu denemeyi bırakabilirsin. Mevcut Echo’lar sen seçmeden değişmez." :
                    "Seçmeden hiçbir Echo silinmez. Tab ile iptal edip mevcut denemene devam edebilirsin." :
                "Kaydedilmiş hareketi incele. Echo otomatik oynar; bu harita ona yeni komut vermez.";
            maintenancePlanGoal.text = SampleGoal();
            maintenancePlanGate.text = SampleGateReason();
            maintenancePlanGate.color = Level.Accent;
            maintenancePlanPreview.text = $"ROTA ÖNİZLEMESİ {planPreviewTime:0.0} / 20s · Yalnızca haritadaki kayıt konumları değişir; dünya çalışmaz.";
            maintenancePlanSlider.maxValue = ActiveLoopLength;
            maintenancePlanSlider.SetValueWithoutNotify(planPreviewTime);
            maintenanceCloseAction.text = pendingEchoRecord ?
                expired ? "BU DENEMEYİ SAKLAMA / TAB" : "KAYDI DEĞİŞTİRME / TAB" : "DEVAM ET / TAB";
            for (int i = 0; i < maintenancePlanCards.Length; i++)
            {
                bool recorded = i < recordings.Count;
                bool create = pendingEchoRecord && i == recordings.Count;
                maintenancePlanCards[i].text = recorded ? $"ECHO {i + 1} / OTOMATİK TEKRAR\n" + SampleTapeEvents(i) :
                    $"ECHO {i + 1} / BOŞ\nKendi rotanı yap, sonra R ile kaydet.";
                maintenancePlanButtons[i].gameObject.SetActive(recorded || create);
                maintenancePlanActions[i].text = create ? "YENİ ECHO OLUŞTUR" :
                    pendingEchoRecord ? "BUNUN YERİNE KAYDET" : "SONRAKİ R / BUNU DEĞİŞTİR";
            }
            BeginMapGeometry(maintenancePlanMarker);
            RenderMaintenancePlan(new Rect(Vector2.zero, maintenancePlanArea.rect.size));
            FinishMapGeometry(maintenancePlanRenderer);
        }

        public void ActivatePlanningButton(int action)
        {
            if (testing || screen != EchoScreen.Playing || !planning) return;
            PlayUiSound(uiClickAudio, .55f);
            switch (action)
            {
                case 0:
                case 1:
                    if (sampleActive)
                    {
                        if (action < recordings.Count) ChooseEchoReplacement(action);
                        else if (pendingEchoRecord && action == recordings.Count) CommitReviewedNewEcho();
                    }
                    else if (action < recordings.Count)
                    {
                        replacementEchoSlot = action;
                        Notify($"Sonraki R Echo {action + 1} kaydını değiştirir.");
                    }
                    break;
                case 2:
                    if (sampleActive) CancelEchoRecordReview();
                    else TogglePlanning();
                    break;
                case 3: RetryLevel(); break;
                case 4 when echoTeaching: FinishEchoTeaching(); break;
            }
        }

        public void SetPlanningPreview(float time)
        {
            if (!testing && screen == EchoScreen.Playing && planning && sampleActive && !echoTeaching)
                planPreviewTime = time;
        }

        private void AddPlanMarker(Rect map, Vector3 point, Color color, string label, float labelOffsetY = -9)
        {
            Vector2 p = MapPoint(map, point);
            var dot = new Rect(p.x - 5, p.y - 5, 10, 10);
            AddMinimapQuad(dot, color);
            occupiedMapAreas.Add(dot);
            AddMinimapLabel(new Vector2(p.x + 8, p.y + labelOffsetY), label, color);
        }


        private void RenderCampaignPlan(Rect map)
        {
            AddMinimapLine(MapPoint(map, new Vector3(-8, 0, 5)), MapPoint(map, new Vector3(8, 0, 5)), uiMuted, 4);
            AddMinimapLine(MapPoint(map, new Vector3(-2, 0, 5)), MapPoint(map, new Vector3(2, 0, 5)), doorOpen ? Color.green : new Color(1, .5f, .2f), 6);
            foreach (Vector3 cover in Level.Covers)
            {
                Vector2 p = MapPoint(map, cover); AddMinimapQuad(new Rect(p.x - 20, p.y - 10, 40, 20), uiMuted);
            }
            for (int i = 0; i < recordings.Count; i++)
            {
                var tape = recordings[i]; Color color = i == 0 ? Color.cyan : new Color(.8f, .4f, 1);
                Vector2 previous = MapPoint(map, tape.Sample(0, EchoPose.Lerp).Position);
                for (float t = .2f; t < tape.Duration + .2f; t += .2f)
                {
                    Vector2 next = MapPoint(map, tape.Sample(Mathf.Min(t, tape.Duration), EchoPose.Lerp).Position);
                    AddMinimapLine(previous, next, color); previous = next;
                }
                AddPlanMarker(map, tape.Sample(tape.Duration, EchoPose.Lerp).Position, color, "E" + (i + 1), -28);
                if (i < savedTapes.Count && savedTapes[i].Actions != null) foreach (EchoSavedAction action in savedTapes[i].Actions)
                {
                    Vector3 actionPos = tape.Sample(action.Time, EchoPose.Lerp).Position;
                    string actionLabel = action.Action.Kind == EchoActionKind.Fire ? "ATEŞ " + action.Time.ToString("0.0") + "s" :
                        action.Action.Kind == EchoActionKind.Device ? "KONSOL " + action.Time.ToString("0.0") + "s" :
                        "ARŞİV " + action.Time.ToString("0.0") + "s";
                    AddPlanMarker(map, actionPos, color, actionLabel, 10);
                }
            }
            foreach (ConsoleState item in consoles) AddPlanMarker(map, item.Plate, item.Held ? Color.green : new Color(1, .7f, .2f), item.Id == 0 ? "A" : "B");
            foreach (EchoEnemy enemy in enemies) if (enemy.Alive) AddPlanMarker(map, enemy.transform.position, new Color(1, .3f, .25f), "M");
            AddPlanMarker(map, ResonancePosition, Level.Accent, "P");
            AddPlanMarker(map, ArchivePosition, ArchiveFound ? Color.green : new Color(1, .75f, .2f), "V");
            AddPlanMarker(map, player.transform.position, Color.white, "SEN");
            AddPlanMarker(map, new Vector3(0, 0, 12), Color.green, "ÇIKIŞ");
        }

        private void RenderMaintenancePlan(Rect map)
        {
            AddMinimapLine(MapPoint(map, new Vector3(-8, 0, SampleGatePosition.z)), MapPoint(map, new Vector3(8, 0, SampleGatePosition.z)), uiMuted, 4);
            float wallY = MapPoint(map, SampleGatePosition).y;
            occupiedMapAreas.Add(new Rect(0, wallY - 4, map.width, 8));
            AddMinimapLine(MapPoint(map, SampleGatePosition + Vector3.left), MapPoint(map, SampleGatePosition + Vector3.right), doorOpen ? Color.green : Color.yellow, 6);
            for (int i = 0; i < recordings.Count; i++)
            {
                Color color = i == 0 ? Color.cyan : new Color(.8f, .5f, 1);
                AddEchoPreviewRoute(map, recordings[i], color);
                AddPlanMarker(map, recordings[i].Sample(planPreviewTime, EchoPose.Lerp).Position, color, "E" + (i + 1), i == 0 ? -26 : 8);
            }
            if (current != null)
            {
                AddEchoPreviewRoute(map, current, Color.white);
                AddPlanMarker(map, current.Sample(Mathf.Min(planPreviewTime, clock), EchoPose.Lerp).Position, Color.white, "SEN", -8);
            }
            foreach (SampleDevice device in sampleDevices) AddPlanMarker(map, device.Position, device.Accent, device.Name);
            if (sampleStage == 1) AddPlanMarker(map, SampleSensorPosition, Color.cyan, "LENS");
            if (resonanceNode != null) AddPlanMarker(map, ResonancePosition, Level.Accent, "P");
            foreach (EchoEnemy enemy in enemies) if (enemy.Alive) AddPlanMarker(map, enemy.transform.position, Color.red, "M");
        }

        private void AddEchoPreviewRoute(Rect map, ReplayTimeline<EchoPose, EchoAction> tape, Color color)
        {
            Vector2 a = MapPoint(map, tape.Sample(0, EchoPose.Lerp).Position);
            for (float t = .2f; t <= tape.Duration + .2f; t += .2f)
            { Vector2 b = MapPoint(map, tape.Sample(Mathf.Min(t, tape.Duration), EchoPose.Lerp).Position); AddMinimapLine(a, b, color); a = b; }
        }
    }
}
