using UnityEngine;

namespace EchoProtocol
{
    [DisallowMultipleComponent]
    public sealed class EchoWeaponSettings : MonoBehaviour
    {
        [SerializeField] internal AudioSource weaponAdsAudio, weaponDryFireAudio, weaponFireAudio, weaponHeadshotAudio, weaponHitmarkerAudio;
        [SerializeField, Min(1)] private int damage = 25;
        [SerializeField, Min(.01f)] private float fireInterval = .24f;
        [SerializeField, Min(1)] private float range = 45f;
        [SerializeField] private Vector3 aimOffset = new Vector3(-.22f, .08f, -.07f);
        [SerializeField, Min(0)] private float recoilRecovery = 7f;
        [SerializeField, Min(0)] private float recoilDistance = .075f;
        [SerializeField] private Vector3 recoilRotation = new Vector3(-6.5f, 0f, 1.5f);
        [SerializeField] private Transform motion;
        [SerializeField] private GameObject muzzle;

        [SerializeField] internal Material shotMaterial;

        public int Damage => damage;
        public float FireInterval => fireInterval;
        public float Range => range;
        public Vector3 AimOffset => aimOffset;
        public float RecoilRecovery => recoilRecovery;
        public float RecoilDistance => recoilDistance;
        public Vector3 RecoilRotation => recoilRotation;
        public Transform Motion => motion;
        public GameObject Muzzle => muzzle;
    }
}
