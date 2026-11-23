using System.Collections.Generic;
using UnityEngine;

namespace EchoProtocol
{
    public sealed partial class EchoPrototypeGame
    {
        [SerializeField] private Material darkMaterial, plateOn, powerOn, powerOff, objectiveOff;
        [SerializeField] private Material ghostCyan, ghostPurple, enemyBeam, shotMaterial;
        [SerializeField] private Material[] accents = new Material[3];

        [SerializeField] private EchoRoomLayout[] campaignRooms;
        [SerializeField] private EchoRoomLayout[] maintenanceRooms;
        [SerializeField] private EchoRoomLayout[] validationRooms;
        [SerializeField] private GameObject authoredRoomDisplay;
        [SerializeField] private EchoPlayer authoredPlayer;
        [SerializeField] private Transform authoredWeapon;
        [SerializeField] private GameObject authoredMuzzle;
        private Vector3 closedDoorPosition, doorOpenOffset;
        private bool closedDoorCollision;
        private Vector3 roomDisplayPosition;
        private Transform roomDisplayParent, roomPlayerSpawn;
        private readonly Dictionary<Transform, Material> idleMaterials = new Dictionary<Transform, Material>();
        private Vector3 resonanceRestScale;
        private Quaternion outerRingRest, innerRingRest;
        private Color roomAmbient;

        private void CaptureIdleMaterial(Transform target)
        {
            if (target != null) idleMaterials.Add(target, target.GetComponent<Renderer>().sharedMaterial);
        }

        private Material IdleMaterial(Transform target) => idleMaterials[target];

        private void UnloadAuthoredRoom()
        {
            if (worldRoot == null) return;
            worldRoot.gameObject.SetActive(false);
            door.localPosition = closedDoorPosition;
            doorCollider.enabled = closedDoorCollision;
            foreach (var entry in idleMaterials)
                entry.Key.GetComponent<Renderer>().sharedMaterial = entry.Value;
            idleMaterials.Clear();
            if (resonanceNode != null) resonanceNode.localScale = resonanceRestScale;
            if (empRingOuter != null) empRingOuter.localRotation = outerRingRest;
            if (empRingInner != null) empRingInner.localRotation = innerRingRest;
            if (vistaSpin != null) vistaSpin.SampleAnimation(vistaCore.gameObject, 0);
            foreach (EchoEnemy enemy in enemies) enemy.ResetEnemy();
            worldRoot.SetParent(roomDisplayParent, false);
            worldRoot.localPosition = roomDisplayPosition;
            worldRoot = null;
        }

        private void LoadAuthoredRoom(EchoRoomLayout[] rooms, int index)
        {
            EchoRoomLayout layout = rooms[index];
            roomDisplayParent = layout.transform.parent;
            roomDisplayPosition = layout.transform.localPosition;
            layout.transform.SetParent(transform, false);
            worldRoot = layout.transform;
            worldRoot.localPosition = Vector3.zero;
            worldRoot.gameObject.SetActive(true);
            door = layout.Door;
            doorOpenOffset = layout.DoorOpenOffset;
            doorCollider = door.GetComponent<Collider>();
            closedDoorPosition = door.localPosition;
            closedDoorCollision = doorCollider.enabled;
            vistaCore = layout.VistaCore;
            resonanceNode = layout.ResonanceNode;
            archiveTerminal = layout.ArchiveTerminal;
            empRingOuter = layout.OuterRing;
            empRingInner = layout.InnerRing;
            empConduitLine = layout.Conduit;
            sampleShutter = layout.Shutter;
            sampleSensor = layout.Sensor;
            sampleSensorArea = layout.SensorArea;
            roomPlayerSpawn = layout.PlayerSpawn;
            roomAmbient = layout.GameplayAmbient;
            if (resonanceNode != null) resonanceRestScale = resonanceNode.localScale;
            if (empRingOuter != null) outerRingRest = empRingOuter.localRotation;
            if (empRingInner != null) innerRingRest = empRingInner.localRotation;
            vistaSpin = layout.VistaSpin;
            CaptureIdleMaterial(resonanceNode);
            CaptureIdleMaterial(archiveTerminal);
            CaptureIdleMaterial(empConduitLine);
            CaptureIdleMaterial(sampleSensor);
            CaptureIdleMaterial(vistaCore);
            sectorVaultHacked = false;
            for (int i = 0; i < layout.Consoles.Length; i++)
            {
                EchoRoomLayout.ConsoleLink link = layout.Consoles[i];
                CaptureIdleMaterial(link.Switch);
                CaptureIdleMaterial(link.Plate);
                consoles.Add(new ConsoleState { Id = i, Visual = link.Switch, PlateVisual = link.Plate,
                    Position = link.Switch.position, PlateArea = link.PlateArea, PlateFloorOffset = link.PlateOffset.y });
            }
            for (int i = 0; i < layout.Devices.Length; i++)
            {
                EchoRoomLayout.DeviceLink link = layout.Devices[i];
                CaptureIdleMaterial(link.Visual);
                sampleDevices.Add(new SampleDevice { Id = i, Name = link.Label, Visual = link.Visual,
                    Position = link.Visual.position, StandbyMaterial = IdleMaterial(link.Visual), Accent = IdleMaterial(link.Visual).color });
            }
            for (int i = 0; i < layout.Sentries.Length; i++)
            {
                EchoEnemy enemy = layout.Sentries[i];
                enemy.SetDifficultyTier(sampleActive ? 2 : levelIndex == 2 ? 3 : levelIndex, IsLiveDifficulty);
                enemy.BindAuthored(i);
                enemies.Add(enemy);
            }
            RenderSettings.ambientLight = layout.Ambient;
            RenderSettings.fog = layout.Fog;
            RenderSettings.fogMode = layout.FogMode;
            RenderSettings.fogColor = layout.FogColor;
            RenderSettings.fogDensity = layout.FogDensity;
        }
    }
}
