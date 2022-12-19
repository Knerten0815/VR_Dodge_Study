using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace Dodge_Study
{
    public class CallMethodWith2Buttons : MonoBehaviour
    {
        [SerializeField] private InputActionReference button1 = null;
        [SerializeField] private InputActionReference button2 = null;
        [SerializeField] UnityEvent method = null;

        private bool isPressed1 = false;
        private bool isPressedLastFrame1 = false;
        private bool isPressed2 = false;
        private bool isPressedLastFrame2 = false;

        private void OnEnable()
        {
            button1.action.started += enable1;
            button1.action.canceled += enable1;
            button2.action.started += enable2;
            button2.action.canceled += enable2;
        }

        private void OnDisable()
        {
            button1.action.started -= enable1;
            button1.action.canceled -= enable1;
            button2.action.started -= enable2;
            button2.action.canceled -= enable2;
        }

        private void enable1(InputAction.CallbackContext context)
        {
            isPressed1 = context.control.IsPressed();
        }

        private void enable2(InputAction.CallbackContext context)
        {
            isPressed2 = context.control.IsPressed();
        }

        void LateUpdate()
        {
            ApplyStatus();
        }

        private void ApplyStatus()
        {
            if (isPressed1 != isPressedLastFrame1)
            {
                isPressedLastFrame1 = isPressed1;
            }

            if (isPressed2 != isPressedLastFrame2)
            {
                isPressedLastFrame2 = isPressed2;
            }

            if (isPressed1 && isPressed2)
                method.Invoke();
        }
    }


}
