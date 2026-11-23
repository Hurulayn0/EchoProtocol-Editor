using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace EchoProtocol
{
    public sealed class EchoRobotMotion : MonoBehaviour
    {
        private Animation animationPlayer;
        private string idle, walk, run, look, charge, attack, hit, turnOff;
        private Vector3 previous;
        private bool chargingState, stunnedState, deadState;
        private float attackTimer, hitTimer, interactTimer, idleTimer;
        [SerializeField] private Transform muzzleL, muzzleR;
        [SerializeField] private Renderer muzzleLRenderer, muzzleRRenderer;

        private void Awake()
        {
            animationPlayer = GetComponentInChildren<Animation>();
            previous = transform.parent != null ? transform.parent.position : transform.position;
            if (animationPlayer == null) return;
            foreach (AnimationState state in animationPlayer)
            {
                string lower = state.name.ToLowerInvariant();
                if (lower.Contains("idle")) { idle = state.name; state.wrapMode = WrapMode.Loop; }
                else if (lower.Contains("walk") && !lower.Contains("rl")) { walk = state.name; state.wrapMode = WrapMode.Loop; }
                else if (lower.Contains("run")) { run = state.name; state.wrapMode = WrapMode.Loop; }
                else if (lower.Contains("look")) { look = state.name; state.wrapMode = WrapMode.Loop; }
                else if (lower.Contains("charge")) { charge = state.name; state.wrapMode = WrapMode.Loop; }
                else if (lower.Contains("attack")) { attack = state.name; state.wrapMode = WrapMode.Once; state.speed = 1.6f; }
                else if (lower.Contains("hit") && hit == null) { hit = state.name; state.wrapMode = WrapMode.Once; state.speed = 1.35f; }
                else if (lower.Contains("turnoff")) { turnOff = state.name; state.wrapMode = WrapMode.ClampForever; }
            }
            if (walk == null)
                foreach (AnimationState state in animationPlayer)
                    if (state.name.ToLowerInvariant().Contains("walk")) { walk = state.name; state.wrapMode = WrapMode.Loop; break; }
        }

        public void SetCombatState(bool isCharging, bool isStunned, bool isDead = false)
        {
            chargingState = isCharging;
            stunnedState = isStunned;
            deadState = isDead;
        }

        public void TriggerAttack()
        {
            attackTimer = .34f;
            if (animationPlayer != null && attack != null)
            {
                animationPlayer.Stop(attack);
                animationPlayer.Play(attack);
            }
        }

        public void TriggerHit()
        {
            hitTimer = .24f;
            if (animationPlayer != null && hit != null && attackTimer <= 0f)
            {
                animationPlayer.Stop(hit);
                animationPlayer.Play(hit);
            }
        }

        public void TriggerInteract()
        {
            interactTimer = .35f;
        }

        public void TriggerCollapse()
        {
            deadState = true;
            chargingState = false;
            attackTimer = 0f;
            if (muzzleL != null) muzzleL.gameObject.SetActive(false);
            if (muzzleR != null) muzzleR.gameObject.SetActive(false);
            if (animationPlayer != null && turnOff != null)
            {
                animationPlayer.Stop();
                animationPlayer.Play(turnOff);
            }
        }

        private void LateUpdate()
        {
            float dt = Mathf.Min(Time.deltaTime, .1f);
            attackTimer = Mathf.Max(0f, attackTimer - dt);
            hitTimer = Mathf.Max(0f, hitTimer - dt);
            interactTimer = Mathf.Max(0f, interactTimer - dt);

            Vector3 position = transform.parent != null ? transform.parent.position : transform.position;
            float speed = dt > .0001f ? Vector3.Distance(position, previous) / dt : 0f;
            previous = position;

            // Update twin gun muzzle flashes
            bool showFlash = !deadState && !stunnedState && (attackTimer > .18f || chargingState);
            if (muzzleL != null && muzzleR != null)
            {
                muzzleL.gameObject.SetActive(showFlash);
                muzzleR.gameObject.SetActive(showFlash);
                if (showFlash)
                {
                    float s = attackTimer > .18f ? .24f + (attackTimer - .18f) * .8f : .09f + Mathf.Abs(Mathf.Sin(Time.time * 28f)) * .07f;
                    muzzleL.localScale = Vector3.one * s;
                    muzzleR.localScale = Vector3.one * s;
                }
            }

            if (animationPlayer == null) return;
            string clip = null;
            if (deadState) clip = turnOff ?? hit ?? idle;
            else if (stunnedState) clip = hit ?? turnOff ?? idle;
            else if (attackTimer > 0f && attack != null) clip = attack;
            else if (hitTimer > 0f && hit != null) clip = hit;
            else if (interactTimer > 0f && (look != null || charge != null)) clip = look ?? charge;
            else if (chargingState && charge != null) clip = charge;
            else if (speed > 4.4f && run != null) clip = run;
            else if (speed > .12f && walk != null) clip = walk;
            else
            {
                idleTimer += dt;
                clip = (look != null && (int)(idleTimer / 4f) % 3 == 2) ? look : idle;
            }
            if (clip != null && !animationPlayer.IsPlaying(clip))
                animationPlayer.CrossFade(clip, clip == attack || clip == hit ? .05f : .12f);
        }
    }
}
