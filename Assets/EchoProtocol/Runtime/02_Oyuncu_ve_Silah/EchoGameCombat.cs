using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace EchoProtocol
{
    public sealed partial class EchoPrototypeGame
    {
        [SerializeField, Min(1)] private int shotDamage = 25;
        [SerializeField, Min(.01f)] private float fireInterval = .24f;
        [SerializeField, Min(1)] private float shotRange = 45f;
        [SerializeField] private Vector3 aimOffset = new Vector3(-.22f, .08f, -.07f);
        private Transform weaponMotion;
        private Vector3 motionRestPosition;
        private Quaternion motionRestRotation;
        private float nextPlayerShot, recoil, muzzleTimer, damageFlash, hitTimer;
        private float killTimer;
        private Transform weaponRoot;
        private GameObject muzzle;
        private readonly List<GameObject> transientBeams = new List<GameObject>();

        private bool TryFirePlayer()
        {
            if (planning || screen != EchoScreen.Playing || clock + .0001f < nextPlayerShot) return false;
            if (!TryConsumeWeaponEnergy()) return false;
            nextPlayerShot = clock + fireInterval;
            EchoAction shot = EchoAction.Fire(player.View.transform.position, player.View.transform.forward);
            RecordPose(clock, player.Pose); RecordAction(clock, shot);
            ExecuteShot(shot, false); recoil = 1; muzzleTimer = .08f;
            AddCameraShake(.12f);
            muzzle.SetActive(true);
            return true;
        }

        private void ExecuteShot(EchoAction action, bool fromEcho)
        {
            if (fromEcho) TriggerGhostAttackAnimation(replayingEcho);
            Physics.SyncTransforms();
            RaycastHit hit;
            Vector3 end = action.Origin + action.Direction * shotRange;
            if (Physics.Raycast(action.Origin, action.Direction, out hit, shotRange, ~(1 << 2), QueryTriggerInteraction.Ignore))
            {
                end = hit.point;
                if (sampleActive && fromEcho && hit.transform.IsChildOf(player.transform))
                    Notify("ATIŞ HATTI ENGELLİ / Sen Echo ile hedef arasındasın. Kendi rotanı hattın dışına taşı.");
                else if (sampleActive && fromEcho && resonanceNode != null && hit.transform != resonanceNode &&
                    Vector3.Dot(action.Direction, (ResonancePosition - action.Origin).normalized) > .97f)
                    Notify("ATIŞ HATTI KAPALI / Echo atış anında rezonatörü göremiyor. Sensör rotasını veya atış zamanını değiştir.");
                EmitImpact(hit.point + hit.normal * .025f, fromEcho ? new Color(.3f, .8f, 1) : new Color(1, .74f, .32f));
                if (hit.transform == resonanceNode) PulseResonance(fromEcho);
                EchoEnemy enemy = hit.collider.GetComponent<EchoEnemy>();
                if (enemy != null && enemy.Alive)
                {
                    bool headshot = hit.point.y - enemy.transform.position.y >= 1.4f;
                    int actor = fromEcho ? (replayingEcho >= 0 && replayingEcho < ghosts.Count ? replayingEcho + 1 : -1) : 0;
                    bool teamHit = sampleActive && sampleStage == 2 && enemy.RegisterTeamHit(actor, clock);
                    enemy.TakeDamage((headshot ? shotDamage * 2 : shotDamage) + EffectiveBonusShotDamage + (teamHit ? 10 : 0));
                    if (teamHit) ShowTeamHit(enemy, hit.point, fromEcho);
                    if (headshot && enemy.Alive) { enemy.Stagger(.45f); PlayAttachedSound(WorldCue.SentryStun, hit.point, 0.65f, owner: enemy.transform); }
                    if (headshot && !fromEcho) { headshotTimer = .6f; PlayUiSound(weaponHeadshotAudio, 0.7f); }
                    else if (!fromEcho) PlayUiSound(weaponHitmarkerAudio, 0.55f);
                    if (!enemy.Alive) { EmitImpact(hit.point, new Color(1, .65f, .2f), 22, .75f); PlayAttachedSound(WorldCue.SentryDestroy, hit.point, 0.8f, owner: enemy.transform); SpawnSentryWreckage(enemy.transform.position, enemy.transform.rotation); }
                    if (!fromEcho) hitTimer = .12f;
                    if (!enemy.Alive) killTimer = .6f;
                }
            }
            ShowBeam(action.Origin + action.Direction * .3f, end, fromEcho ? (replayingEcho == 1 ? ghostPurple : ghostCyan) : shotMaterial);
            PlayWorldSound(weaponFireAudio, action.Origin, fromEcho ? 0.42f : 0.72f, fromEcho ? 1.12f : 1.0f);
        }

        private void ShowBeam(Vector3 from, Vector3 to, Material material)
        {
            LineRenderer line = Instantiate(tracePrefab, worldRoot);
            line.gameObject.name = "Pulse trace";
            transientBeams.RemoveAll(item => item == null);
            transientBeams.Add(line.gameObject);
            line.sharedMaterial = material;
            line.SetPosition(0, from); line.SetPosition(1, to);
            Destroy(line.gameObject, traceDuration);
        }

        private void ClearBeams()
        {
            foreach (GameObject beam in transientBeams)
                if (beam != null) { beam.SetActive(false); Destroy(beam); }
            transientBeams.Clear();
        }

        private void TickWeapon(float dt)
        {
            recoil = Mathf.MoveTowards(recoil, 0, dt * 7);
            muzzleTimer = Mathf.Max(0, muzzleTimer - dt); hitTimer = Mathf.Max(0, hitTimer - dt);
            killTimer = Mathf.Max(0, killTimer - dt);
            Vector3 offset = player.IsAiming ? aimOffset : Vector3.zero;
            offset.z -= recoil * .075f;
            if (dt > 0) { weaponTravel += dt * player.Controller.velocity.magnitude * 1.9f; float walk = player.IsAiming ? .0015f : .009f; offset += new Vector3(Mathf.Sin(weaponTravel) * walk, Mathf.Sin(weaponTravel * 2) * walk, 0); }
            Vector3 target = motionRestPosition + weaponRoot.InverseTransformVector(weaponRoot.parent.TransformVector(offset));
            weaponMotion.localPosition = dt == 0 ? target : Vector3.Lerp(weaponMotion.localPosition, target, 1 - Mathf.Exp(-18 * dt));
            weaponMotion.localRotation = motionRestRotation * Quaternion.Euler(-recoil * 6.5f, 0, recoil * 1.5f);
            muzzle.SetActive(muzzleTimer > 0);
        }

        private void DamagePlayer(int damage)
        {
            if (playerHealth <= 0 || damage <= 0 || screen != EchoScreen.Playing) return;
            playerHealth = Mathf.Max(0, playerHealth - damage); damageFlash = .25f;
            sectorDamageTaken += damage;
            PlayUiSound(playerHurtAudio, 0.65f);
            if (playerHealth == 0)
            {
                if (sampleActive) sampleDeaths++;
                SetScreen(EchoScreen.GameLost);
                message = "Baglanti kesildi. Sektoru yeniden baslat.";
            }
        }

    }
}
