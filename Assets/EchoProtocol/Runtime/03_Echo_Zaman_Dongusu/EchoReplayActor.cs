using UnityEngine;

namespace EchoProtocol
{
    public sealed class EchoReplayActor : MonoBehaviour
    {
        [SerializeField] private Transform aim;
        [SerializeField] private EchoRobotMotion motion;
        [SerializeField] private AudioSource actionCue;
        public AudioSource ActionCue => actionCue;
        public Transform Aim => aim;
        public EchoRobotMotion Motion => motion;
    }
}
