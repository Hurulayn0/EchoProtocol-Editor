using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoProtocol
{
    // One enemy type: a patrolling hitscan sentry. Echoes can draw its fire.
    public sealed class EchoEnemy : MonoBehaviour
    {
        public const int MaxHealth = 60;
        [SerializeField, Min(1)] private int baseHealth = MaxHealth;
        [SerializeField, Min(1)] private int baseDamage = 12;
        [SerializeField, Min(0)] private float baseDetectionRange = 14f;
        [SerializeField, Min(0)] private float basePatrolSpeed = .8f;
        [SerializeField, Min(.01f)] private float baseFireInterval = 1.1f;
        [SerializeField] private Transform patrolLeft, patrolRight;

        public int EffectiveMaxHealth { get { return baseHealth + (!liveDifficulty ? 0 : difficultyTier >= 3 ? 20 : difficultyTier == 2 ? 15 : 0); } }
        public int Health { get; private set; }
        public int Id { get; private set; }
        public Vector3 Spawn { get; private set; }
        public bool Alive { get { return Health > 0; } }
        public bool LastTargetWasEcho { get; private set; }
        public int Attacks { get; private set; }
        private CharacterController controller;
        private Quaternion spawnRotation;
        private EchoRobotMotion robotMotion;
        private float age, nextAttack;
        private int difficultyTier;
        private bool liveDifficulty;
        public void SetDifficultyTier(int tier, bool livePlay = false) { difficultyTier = Mathf.Clamp(tier, 0, 3); liveDifficulty = livePlay; }
        public float StunRemaining { get; private set; }
        private LineRenderer chargeLine;
        public bool Charging { get; private set; }
        public bool HasTarget { get; private set; }
        public bool TargetIsEcho { get; private set; }
        public Vector3 TargetPoint { get; private set; }
        public const float TeamHitWindow = 1.2f;
        public int LastHitActor { get; private set; }
        public float LastHitTime { get; private set; }
        private float nextTeamHit;

        // Actor 0 is the player; 1 and 2 are actual replay slots. Unknown actors cannot pair.
        public bool RegisterTeamHit(int actor, float time)
        {
            if (!Alive || actor < 0 || actor > 2 || float.IsNaN(time) || float.IsInfinity(time) || time < 0 || time < LastHitTime) return false;
            bool paired = LastHitActor >= 0 && LastHitActor != actor && time >= LastHitTime &&
                time - LastHitTime <= TeamHitWindow + .0001f && time >= nextTeamHit;
            LastHitActor = actor; LastHitTime = time;
            if (paired) nextTeamHit = time + TeamHitWindow;
            return paired;
        }

        internal void BindAuthored(int id)
        {
            Id = id;
            Spawn = transform.position;
            spawnRotation = transform.rotation;
            controller = GetComponent<CharacterController>();
            robotMotion = GetComponentInChildren<EchoRobotMotion>();
            chargeLine = GetComponent<LineRenderer>();
        }

        public void ResetEnemy()
        {
            gameObject.SetActive(true); controller.enabled = false;
            transform.SetPositionAndRotation(Spawn, spawnRotation);
            controller.enabled = true;
            Health = EffectiveMaxHealth; age = 0;
            nextAttack = !liveDifficulty ? 1.25f : difficultyTier >= 3 ? .82f : difficultyTier == 2 ? .90f : 1.0f;
            StunRemaining = 0;
            Attacks = 0; LastTargetWasEcho = false; Charging = false; if (chargeLine != null) { chargeLine.enabled = false; chargeLine.startWidth = .016f; chargeLine.endWidth = .025f; }
            HasTarget = false; TargetIsEcho = false; TargetPoint = Spawn;
            LastHitActor = -1; LastHitTime = -1; nextTeamHit = 0;
            if (robotMotion != null) robotMotion.SetCombatState(false, false, false);
        }

        public void Stagger(float seconds) { if (!Alive || seconds <= 0) return; StunRemaining = Mathf.Max(StunRemaining, seconds); nextAttack = Mathf.Max(nextAttack, age + seconds + .35f); HasTarget = false; Charging = false; chargeLine.enabled = false; if (robotMotion != null) { robotMotion.SetCombatState(false, true, false); robotMotion.TriggerHit(); } }

        public void TakeDamage(int damage)
        {
            if (!Alive || damage <= 0) return;
            Health = Mathf.Max(0, Health - damage);
            if (Alive) { if (robotMotion != null) robotMotion.TriggerHit(); }
            else { HasTarget = false; Charging = false; chargeLine.enabled = false; gameObject.SetActive(false); }
        }

        public void Tick(float dt, EchoPlayer player, IList<Vector3> echoes,
            Action<int> damagePlayer, Action<Vector3, Vector3> showBeam)
        {
            if (!Alive) return;
            age += dt;
            if (StunRemaining > 0)
            {
                StunRemaining = Mathf.Max(0, StunRemaining - dt);
                HasTarget = false; Charging = false; chargeLine.enabled = false;
                if (robotMotion != null) robotMotion.SetCombatState(false, StunRemaining > 0, false);
                transform.localRotation = Quaternion.Euler(-14f * Mathf.Sin(StunRemaining * 12f), transform.localEulerAngles.y, 6f * Mathf.Cos(StunRemaining * 10f));
                return;
            }
            float patrolSpeed = basePatrolSpeed + (!liveDifficulty ? 0 : difficultyTier >= 3 ? .45f : difficultyTier == 2 ? .35f : .2f);
            Vector3 patrol = Spawn + (Vector3.Lerp(patrolLeft.position, patrolRight.position,
                (1 + Mathf.Sin(age * patrolSpeed)) * .5f) - patrolLeft.parent.position);
            Vector3 movement = patrol - transform.position; movement.y = -2 * dt;
            controller.Move(movement);
            Vector3 origin = transform.position + Vector3.up * 1.4f;
            Vector3 target = player.transform.position + Vector3.up * 1.4f;
            bool decoy = false;
            float distance = Vector3.Distance(origin, target);
            float visionRange = baseDetectionRange + (!liveDifficulty ? 0 : difficultyTier >= 3 ? 3.5f : difficultyTier == 2 ? 2.5f : 1.5f);
            Physics.SyncTransforms();
            bool visible = CanSee(origin, target, player, false, visionRange);
            foreach (Vector3 echo in echoes)
            {
                Vector3 candidate = echo + Vector3.up * 1.4f;
                float candidateDistance = Vector3.Distance(origin, candidate);
                if ((!visible || candidateDistance < distance) && CanSee(origin, candidate, player, true, visionRange))
                { target = candidate; distance = candidateDistance; decoy = true; visible = true; }
            }
            Vector3 horizontal = target - origin; horizontal.y = 0;
            HasTarget = visible && distance <= visionRange; TargetIsEcho = HasTarget && decoy; TargetPoint = target;
            if (horizontal.sqrMagnitude > .01f) transform.rotation = Quaternion.LookRotation(horizontal);
            Charging = visible && distance <= visionRange && age >= nextAttack - .35f && age < nextAttack;
            chargeLine.enabled = Charging;
            if (robotMotion != null) robotMotion.SetCombatState(Charging, false, false);
            if (Charging)
            {
                chargeLine.startWidth = decoy ? .012f + Mathf.Sin(age * 36f) * .005f : .025f;
                chargeLine.endWidth = decoy ? .018f : .035f;
                chargeLine.SetPosition(0, origin);
                chargeLine.SetPosition(1, target);
            }
            if (!visible || distance > visionRange || age < nextAttack) return;
            Charging = false; chargeLine.enabled = false;
            float fireCooldown = Mathf.Max(.01f, baseFireInterval - (!liveDifficulty ? (difficultyTier >= 2 ? .1f : 0) : difficultyTier >= 3 ? .32f : difficultyTier == 2 ? .25f : .2f));
            int fireDamage = baseDamage + (!liveDifficulty ? (difficultyTier >= 2 ? 2 : 0) : difficultyTier >= 3 ? 8 : difficultyTier == 2 ? 6 : 4);
            nextAttack = age + fireCooldown; Attacks++; LastTargetWasEcho = decoy;
            if (robotMotion != null) { robotMotion.SetCombatState(false, false, false); robotMotion.TriggerAttack(); }
            showBeam(origin, target);
            if (!decoy) damagePlayer(fireDamage);
        }

        private static bool CanSee(Vector3 origin, Vector3 target, EchoPlayer player, bool echo, float maxRange)
        {
            float distance = Vector3.Distance(origin, target);
            if (distance > maxRange || distance < .01f) return false;
            RaycastHit hit;
            if (!Physics.Raycast(origin, (target - origin).normalized, out hit, distance,
                ~(1 << 2), QueryTriggerInteraction.Ignore)) return true;
            // Echo bodies are non-solid. A player standing in front of an Echo occludes it.
            return !echo && hit.transform.IsChildOf(player.transform);
        }
    }
}
