using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace EchoProtocol
{
    // One frame of controls, shared by live input and deterministic gameplay checks.
    public struct EchoInputFrame
    {
        public Vector2 Move, Look;
        public bool Jump, Interact, Record, Clear, Pause, Confirm, Test, Fire, Sprint, Aim, Undo, Plan, Retry, Crouch;
    }

    // Works in old-input and new-input projects without requiring an asset package.
    public static class EchoInput
    {
        public static EchoInputFrame Read()
        {
            return new EchoInputFrame {
                Move = Move(), Look = Look(), Jump = Down(KeyCode.Space),
                Interact = Down(KeyCode.E), Record = Down(KeyCode.R), Clear = Down(KeyCode.X),
                Pause = Down(KeyCode.Escape), Confirm = Down(KeyCode.Return),
                Test = Down(KeyCode.F6), Fire = FireHeld(), Sprint = SprintHeld(),
                Aim = AimHeld(), Undo = Down(KeyCode.Q), Plan = Down(KeyCode.Tab), Retry = Down(KeyCode.T),
                Crouch = CrouchHeld()
            };
        }
        public static bool Down(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            if (k == null) return false;
            switch (key)
            {
                case KeyCode.E: return k.eKey.wasPressedThisFrame;
                case KeyCode.R: return k.rKey.wasPressedThisFrame;
                case KeyCode.T: return k.tKey.wasPressedThisFrame;
                case KeyCode.X: return k.xKey.wasPressedThisFrame;
                case KeyCode.Q: return k.qKey.wasPressedThisFrame;
                case KeyCode.Tab: return k.tabKey.wasPressedThisFrame;
                case KeyCode.Escape: return k.escapeKey.wasPressedThisFrame;
                case KeyCode.Space: return k.spaceKey.wasPressedThisFrame;
                case KeyCode.F6: return k.f6Key.wasPressedThisFrame;
                case KeyCode.Return: return k.enterKey.wasPressedThisFrame;
            }
            return false;
#else
            return Input.GetKeyDown(key);
#endif
        }
        public static Vector2 Move()
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            if (k == null) return Vector2.zero;
            return new Vector2((k.dKey.isPressed ? 1 : 0) - (k.aKey.isPressed ? 1 : 0),
                (k.wKey.isPressed ? 1 : 0) - (k.sKey.isPressed ? 1 : 0));
#else
            return new Vector2((Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0),
                (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0));
#endif
        }
        public static Vector2 Look()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current == null ? Vector2.zero : Mouse.current.delta.ReadValue() * .10f;
#else
            return new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * 1.8f;
#endif
        }

        public static bool FireHeld()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.isPressed;
#else
            return Input.GetMouseButton(0);
#endif
        }

        public static bool SprintHeld()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
#else
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#endif
        }

        public static bool AimHeld()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.rightButton.isPressed;
#else
            return Input.GetMouseButton(1);
#endif
        }

        public static bool CrouchHeld()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && (Keyboard.current.cKey.isPressed || Keyboard.current.leftCtrlKey.isPressed);
#else
            return Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl);
#endif
        }
    }
}
