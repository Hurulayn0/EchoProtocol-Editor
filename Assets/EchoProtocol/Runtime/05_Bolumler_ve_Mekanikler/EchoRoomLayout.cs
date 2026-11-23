using System;
using UnityEngine;

namespace EchoProtocol
{
    public sealed class EchoRoomLayout : MonoBehaviour
    {
        [Serializable]
        public struct ConsoleLink
        {
            public Transform Switch;
            public Transform Plate;
            public Vector3 PlateOffset;
            public BoxCollider PlateArea;
        }

        [Serializable]
        public struct DeviceLink
        {
            public Transform Visual;
            public string Label;
        }

        public Transform Door;
        public Vector3 DoorOpenOffset = Vector3.up * 4.5f;
        public Transform PlayerSpawn;
        public Transform VistaCore;
        public AnimationClip VistaSpin;
        public Transform ResonanceNode;
        public Transform ArchiveTerminal;
        public Transform OuterRing;
        public Transform InnerRing;
        public Transform Conduit;
        public Transform Shutter;
        public Transform Sensor;
        public BoxCollider SensorArea;
        public ConsoleLink[] Consoles;
        public DeviceLink[] Devices;
        public EchoEnemy[] Sentries;
        public Color Ambient;
        public Color GameplayAmbient;
        public Color FogColor;
        public bool Fog;
        public FogMode FogMode;
        public float FogDensity;
    }
}
