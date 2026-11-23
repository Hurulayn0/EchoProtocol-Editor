using UnityEngine;


namespace EchoProtocol
{
    [DisallowMultipleComponent]
    public sealed class EchoAudioBindings : MonoBehaviour
    {
        [SerializeField] internal AudioSource menuMusicSource, gameMusicSource;
        [SerializeField] internal Transform uiSound, playerSound, weaponSound, loopSound;
        [SerializeField] internal AudioSource archiveUnlockAudio, checkpointWinAudio, loopFailAudio, uiClickAudio;
        [SerializeField] internal AudioSource consoleErrorAudio, echoResetAudio, echoRecordAudio, echoSpawnAudio;
        [SerializeField] internal AudioSource loopWarningAudio;
        [SerializeField] internal EchoPrototypeGame.WorldSoundLink[] worldSoundLinks;
    }
}
