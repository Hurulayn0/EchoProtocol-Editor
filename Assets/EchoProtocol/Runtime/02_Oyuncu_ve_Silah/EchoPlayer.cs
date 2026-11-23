using UnityEngine;

namespace EchoProtocol
{
    [System.Serializable] public struct EchoPose
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float Pitch;
        public static EchoPose Lerp(EchoPose a, EchoPose b, float t)
        {
            return new EchoPose { Position = Vector3.Lerp(a.Position, b.Position, t),
                Rotation = Quaternion.Slerp(a.Rotation, b.Rotation, t), Pitch = Mathf.LerpAngle(a.Pitch, b.Pitch, t) };
        }
    }
    public sealed class EchoPlayer : MonoBehaviour
    {
        public CharacterController Controller;
        public Camera View;
        [SerializeField, Min(0)] private float walkSpeed = 4f;
        [SerializeField, Min(0)] private float sprintSpeed = 6f;
        [SerializeField, Min(0)] private float slowSpeed = 2.6f;
        [SerializeField, Min(0)] private float jumpSpeed = 5f;
        [SerializeField, Min(0)] private float gravity = 18f;
        public float LookSensitivity = 1;
        public bool InvertLook;
        public float BaseFieldOfView = 75;
        public bool IsAiming { get; private set; }
        public bool IsCrouching { get; private set; }
        public float EyeHeight { get; private set; } = 1.6f;
        private float pitch, vertical;
        private float standingHeight, standingEyeHeight;
        private Vector3 standingCenter, cameraOffset;
        private Quaternion cameraRestRotation;
        internal Quaternion CameraRotation => cameraRestRotation * Quaternion.Euler(pitch, 0, 0);
        internal Vector3 CameraPosition => cameraOffset + Vector3.up * EyeHeight;

        internal void CaptureSceneSettings()
        {
            cameraRestRotation = View.transform.localRotation;
            standingHeight = Controller.height;
            standingCenter = Controller.center;
            standingEyeHeight = View.transform.localPosition.y;
            cameraOffset = View.transform.localPosition - Vector3.up * standingEyeHeight;
            EyeHeight = standingEyeHeight;
        }
        public EchoPose Pose { get { return new EchoPose { Position = transform.position, Rotation = transform.rotation, Pitch = pitch }; } }
        public void Tick(float dt)
        {
            Tick(dt, EchoInput.Read());
        }
        public void Tick(float dt, EchoInputFrame controls)
        {
            IsAiming = controls.Aim;
            IsCrouching = controls.Crouch;
            float targetHeight = IsCrouching ? Mathf.Max(Controller.radius * 2, standingHeight - .55f) : standingHeight;
            float targetEye = IsCrouching ? standingEyeHeight - .58f : standingEyeHeight;
            float blend = dt > 0f ? 1f - Mathf.Exp(-14f * dt) : 1f;
            Controller.height = Mathf.Lerp(Controller.height, targetHeight, blend);
            Controller.center = standingCenter + Vector3.up * ((Controller.height - standingHeight) * .5f);
            EyeHeight = Mathf.Lerp(EyeHeight, targetEye, blend);
            View.transform.localPosition = CameraPosition;
            View.fieldOfView = Mathf.Lerp(View.fieldOfView, IsAiming ? BaseFieldOfView * .8f : BaseFieldOfView, blend);
            Vector2 look = controls.Look * LookSensitivity * (IsAiming ? .65f : 1);
            pitch = Mathf.Clamp(pitch - look.y * (InvertLook ? -1 : 1), -80, 80);
            transform.Rotate(0, look.x, 0);
            View.transform.localRotation = CameraRotation;
            Vector2 input = Vector2.ClampMagnitude(controls.Move, 1);
            float speed = IsAiming || IsCrouching ? slowSpeed : controls.Sprint ? sprintSpeed : walkSpeed;
            Vector3 move = (transform.forward * input.y + transform.right * input.x) * speed;
            if (Controller.isGrounded && vertical < 0) vertical = -2;
            if (Controller.isGrounded && controls.Jump && !IsCrouching) vertical = jumpSpeed;
            vertical -= gravity * dt;
            move.y = vertical;
            Controller.Move(move * dt);
        }
        public void SetPose(EchoPose pose)
        {
            Controller.enabled = false;
            Controller.height = standingHeight; Controller.center = standingCenter;
            transform.SetPositionAndRotation(pose.Position, pose.Rotation);
            pitch = pose.Pitch; vertical = 0;
            IsAiming = false; IsCrouching = false; EyeHeight = standingEyeHeight;
            View.fieldOfView = BaseFieldOfView;
            View.transform.localPosition = CameraPosition;
            View.transform.localRotation = CameraRotation;
            Controller.enabled = true;
        }
    }
}
