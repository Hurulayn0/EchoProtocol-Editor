using UnityEngine;

namespace EchoProtocol
{
    public enum EchoActionKind { Console, Fire, Archive, Device }

    // A shot stores its original world-space ray, not a precomputed damage result.
    // On replay it hits the current, reset world using the same combat code.
    [System.Serializable] public struct EchoAction
    {
        public EchoActionKind Kind;
        public int TargetId;
        public Vector3 Origin;
        public Vector3 Direction;
        public static EchoAction Device(int id) { return new EchoAction { Kind = EchoActionKind.Device, TargetId = id }; }
        public static EchoAction Archive(int id) { return new EchoAction { Kind = EchoActionKind.Archive, TargetId = id }; }
        public static EchoAction Console(int id)
        {
            return new EchoAction { Kind = EchoActionKind.Console, TargetId = id };
        }
        public static EchoAction Fire(Vector3 origin, Vector3 direction)
        {
            return new EchoAction { Kind = EchoActionKind.Fire, Origin = origin, Direction = direction.normalized };
        }
    }
}
