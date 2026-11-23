using UnityEngine;

namespace EchoProtocol
{
    public sealed partial class EchoPrototypeGame
    {
        [SerializeField] private LineRenderer cyanShotPreview, purpleShotPreview, teamHitRing;
        [SerializeField, Min(.01f)] private float teamHitRingDuration = .65f;

        private float teamHitTimer;
        private int teamHits;
        private const float ShotPreviewWindow = 1;
        private const float InteractionPreviewWindow = 1.2f;

        private bool UpcomingEchoShot(int index, out EchoSavedAction shot)
        {
            shot = default(EchoSavedAction);
            if (!sampleActive || index < 0 || index >= savedTapes.Count) return false;
            foreach (EchoSavedAction item in savedTapes[index].Actions)
                if (item.Action.Kind == EchoActionKind.Fire && item.Time > clock + .0001f)
                { shot = item; return item.Time - clock <= ShotPreviewWindow; }
            return false;
        }

        private bool UpcomingEchoDevice(int index, out EchoSavedAction device)
        {
            device = default(EchoSavedAction);
            if (!sampleActive || index < 0 || index >= savedTapes.Count) return false;
            foreach (EchoSavedAction item in savedTapes[index].Actions)
                if (item.Action.Kind == EchoActionKind.Device && item.Time > clock + .0001f)
                { device = item; return item.Time - clock <= InteractionPreviewWindow; }
            return false;
        }

        private void TickEchoTactics(float dt)
        {
            teamHitTimer = Mathf.Max(0, teamHitTimer - dt);
            if (!sampleActive) return;
            for (int i = 0; i < ghosts.Count; i++)
            {
                Ghost ghost = ghosts[i]; EchoSavedAction shot;
                bool show = UpcomingEchoShot(i, out shot);
                LineRenderer template = i == 0 ? cyanShotPreview : purpleShotPreview;
                if (show && ghost.ShotPreview == null)
                    ghost.ShotPreview = Instantiate(template, ghost.Body.transform);
                if (ghost.ShotPreview == null) continue;
                ghost.ShotPreview.enabled = show;
                if (!show) continue;
                RaycastHit hit;
                Vector3 origin = shot.Action.Origin;
                Vector3 end = origin + shot.Action.Direction * 45;
                if (Physics.Raycast(origin, shot.Action.Direction, out hit, 45, ~(1 << 2), QueryTriggerInteraction.Ignore)) end = hit.point;
                ghost.ShotPreview.widthMultiplier = template.widthMultiplier * (2 - (shot.Time - clock));
                ghost.ShotPreview.SetPosition(0, origin); ghost.ShotPreview.SetPosition(1, end);
            }
        }

        private void ShowTeamHit(EchoEnemy enemy, Vector3 point, bool fromEcho)
        {
            teamHits++; teamHitTimer = 1.1f;
            if (enemy.Alive) enemy.Stagger(.8f);
            EmitImpact(point, new Color(.25f, 1, .9f), 28, .8f);
            LineRenderer ring = Instantiate(teamHitRing, enemy.transform.position, Quaternion.identity, worldRoot);
            transientBeams.Add(ring.gameObject);
            Destroy(ring.gameObject, teamHitRingDuration);
            SampleEvent("EŞZAMANLI VURUŞ / +10 HASAR", fromEcho);
        }

        private string GuardIntent(EchoEnemy enemy)
        {
            if (!enemy.Alive) return "ETKİSİZ";
            if (enemy.StunRemaining > 0) return "SERSEMLEDİ / " + enemy.StunRemaining.ToString("0.0") + "s";
            if (!enemy.HasTarget) return "ARANIYOR";
            return enemy.TargetIsEcho ? "HEDEF: ECHO / FIRSAT" : "HEDEF: SEN / SİPER AL";
        }

    }
}
