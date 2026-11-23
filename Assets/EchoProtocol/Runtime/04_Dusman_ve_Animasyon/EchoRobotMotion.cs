using UnityEngine;

namespace EchoProtocol
{
    public sealed class EchoRobotMotion : MonoBehaviour
    {
        private enum Motion { Idle, Walk, Run, Look, Charge, Attack, Hit, Off }
        private static readonly int State = Animator.StringToHash("Motion");
        private static readonly int Attack = Animator.StringToHash("Restart Attack");
        private static readonly int Hit = Animator.StringToHash("Restart Hit");

        [SerializeField] private Animator animator;
        [SerializeField] private Transform muzzleL, muzzleR;
        private Vector3 previous;
        private bool charging, stunned, dead;
        private float attackTimer, hitTimer, interactTimer, idleTimer;

        private void Awake() => previous = Position;
        private Vector3 Position => transform.parent != null ? transform.parent.position : transform.position;

        public void SetCombatState(bool isCharging, bool isStunned, bool isDead = false)
        {
            charging = isCharging;
            stunned = isStunned;
            dead = isDead;
        }

        public void TriggerAttack()
        {
            attackTimer = .34f;
            animator.SetTrigger(Attack);
        }

        public void TriggerHit()
        {
            hitTimer = .24f;
            if (attackTimer <= 0) animator.SetTrigger(Hit);
        }

        public void TriggerInteract() => interactTimer = .35f;

        public void TriggerCollapse()
        {
            dead = true;
            charging = false;
            attackTimer = 0;
            UpdateFlash();
            animator.SetInteger(State, (int)Motion.Off);
        }

        private void LateUpdate()
        {
            float dt = Mathf.Min(Time.deltaTime, .1f);
            attackTimer = Mathf.Max(0, attackTimer - dt);
            hitTimer = Mathf.Max(0, hitTimer - dt);
            interactTimer = Mathf.Max(0, interactTimer - dt);
            Vector3 position = Position;
            float speed = dt > .0001f ? Vector3.Distance(position, previous) / dt : 0;
            previous = position;
            UpdateFlash();
            animator.SetInteger(State, (int)CurrentMotion(speed, dt));
        }

        private Motion CurrentMotion(float speed, float dt)
        {
            if (dead) return Motion.Off;
            if (stunned) return Motion.Hit;
            if (attackTimer > 0) return Motion.Attack;
            if (hitTimer > 0) return Motion.Hit;
            if (interactTimer > 0) return Motion.Look;
            if (charging) return Motion.Charge;
            if (speed > 4.4f) return Motion.Run;
            if (speed > .12f) return Motion.Walk;
            idleTimer += dt;
            return (int)(idleTimer / 4f) % 3 == 2 ? Motion.Look : Motion.Idle;
        }

        private void UpdateFlash()
        {
            if (muzzleL == null || muzzleR == null) return;
            bool show = !dead && !stunned && (attackTimer > .18f || charging);
            muzzleL.gameObject.SetActive(show);
            muzzleR.gameObject.SetActive(show);
            if (!show) return;
            float scale = attackTimer > .18f ? .24f + (attackTimer - .18f) * .8f : .09f + Mathf.Abs(Mathf.Sin(Time.time * 28f)) * .07f;
            muzzleL.localScale = muzzleR.localScale = Vector3.one * scale;
        }
    }
}
