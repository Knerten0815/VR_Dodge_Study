using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace Dodge_Study
{
    public class CallMethodWithButton : MonoBehaviour
    {
        [SerializeField] private InputActionReference button = null;
        [SerializeField] UnityEvent method = null;

        private bool isPressed = false;
        private bool isPressedLastFrame = false;

        private void OnEnable()
        {
            button.action.started += enable1;
            button.action.canceled += enable1;
        }

        private void OnDisable()
        {
            button.action.started -= enable1;
            button.action.canceled -= enable1;
        }

        private void enable1(InputAction.CallbackContext context)
        {
            isPressed = context.control.IsPressed();
        }

        void LateUpdate()
        {
            ApplyStatus();
        }

        private void ApplyStatus()
        {
            if (isPressed != isPressedLastFrame)
            {
                isPressedLastFrame = isPressed;

                if(isPressed)
                    method.Invoke();
            }
        }
    }


}
