using UnityEngine;


namespace EchoProtocol
{
    [DisallowMultipleComponent]
    public sealed class EchoEffectsBindings : MonoBehaviour
    {
        [SerializeField] internal ParticleSystem impactPrefab;
        [SerializeField] internal LineRenderer tracePrefab, arcPrefab;
        [SerializeField, Min(.01f)] internal float traceDuration = .09f, arcDuration = .45f;
        [SerializeField] internal LineRenderer cyanShotPreview, purpleShotPreview, teamHitRing;
        [SerializeField, Min(.01f)] internal float teamHitRingDuration = .65f;
    }
}
