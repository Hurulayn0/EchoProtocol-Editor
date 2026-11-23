using System.Collections.Generic;
using UnityEngine;

namespace EchoProtocol
{
    public sealed partial class EchoPrototypeGame
    {
        private EchoCameraFinish cameraFinish;
        private Material effectsMaterial;
        private readonly List<GameObject> impactObjects = new List<GameObject>();
        private Transform vistaCore;
        private float weaponTravel;

        [SerializeField] private ParticleSystem impactPrefab;
        [SerializeField] private LineRenderer tracePrefab, arcPrefab;
        [SerializeField, Min(.01f)] private float traceDuration = .09f, arcDuration = .45f;

        private void CreateImmersion()
        {
            cameraFinish = player.View.GetComponent<EchoCameraFinish>();
            effectsMaterial = impactPrefab.GetComponent<ParticleSystemRenderer>().sharedMaterial;
        }

        private void EmitImpact(Vector3 position, Color color, int count = 10, float lifetime = .4f)
        {
            impactObjects.RemoveAll(item => item == null);
            if (impactObjects.Count >= 12) return;
            ParticleSystem particles = Instantiate(impactPrefab, position, Quaternion.identity, worldRoot);
            particles.gameObject.name = "Pulse sparks";
            float duration = particles.main.startLifetime.constant * (lifetime / .4f);
            var emission = new ParticleSystem.EmitParams { startColor = particles.main.startColor.color * color * color, startLifetime = duration };
            particles.Play();
            particles.Emit(emission, count);
            impactObjects.Add(particles.gameObject);
            Destroy(particles.gameObject, duration + .15f);
        }
        private void ClearImpacts()
        {
            foreach (GameObject effect in impactObjects) if (effect != null) { effect.SetActive(false); Destroy(effect); }
            impactObjects.Clear(); weaponTravel = 0;
        }
        private void TickImmersion()
        {
            if (vistaCore != null) vistaCore.localRotation = vistaRest * Quaternion.Euler(0, clock * 12, 0);
            if (screen == EchoScreen.Playing)
            {
                float alert = ActiveLoopLength - clock <= 3f && ActiveLoopLength > 0 ? Mathf.Sin(clock * 16f) * .5f + .5f : 0f;
                Color baseAmbient = roomAmbient;
                if (doorOpen) baseAmbient = Color.Lerp(baseAmbient, new Color(.12f, .32f, .26f), .45f);
                if (ResonanceRemaining > 0) baseAmbient = Color.Lerp(baseAmbient, new Color(.35f, .26f, .12f), Mathf.Min(1, ResonanceRemaining));
                if (alert > 0) baseAmbient = Color.Lerp(baseAmbient, new Color(.38f, .10f, .10f), alert * .55f);
                RenderSettings.ambientLight = baseAmbient;
            }
        }
    }
}
