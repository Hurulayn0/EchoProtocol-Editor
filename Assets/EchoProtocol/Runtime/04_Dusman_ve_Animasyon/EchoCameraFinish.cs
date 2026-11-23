using UnityEngine;

namespace EchoProtocol
{
    [RequireComponent(typeof(Camera))]
    public sealed class EchoCameraFinish : MonoBehaviour
    {
        private Material finish;
        public bool Ready { get { return finish != null; } }
        private void Awake()
        {
            Shader shader = Resources.Load<Shader>("EchoShaders/Atmosphere");
            if (shader != null && shader.isSupported) finish = new Material(shader);
        }
        public void SetLook(Color accent, float rewind, float time)
        {
            if (finish == null) return;
            finish.SetColor("_Accent", accent); finish.SetFloat("_Rewind", rewind); finish.SetFloat("_Clock", time);
        }
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (finish == null) Graphics.Blit(source, destination); else Graphics.Blit(source, destination, finish);
        }
        private void OnDestroy() { if (finish != null) Destroy(finish); }
    }
}
