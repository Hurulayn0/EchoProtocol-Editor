using UnityEngine;

namespace EchoProtocol
{
    public sealed class EchoReplayActor : MonoBehaviour
    {
        [SerializeField] private Transform aim;
        [SerializeField] private EchoRobotMotion motion;
        public Transform Aim => aim;
        public EchoRobotMotion Motion => motion;
    }
}
