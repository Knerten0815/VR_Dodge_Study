using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRRayInteractor))]
public class RayToggler : MonoBehaviour
{
    [SerializeField] private InputActionReference activateReference1 = null;
    [SerializeField] private InputActionReference activateReference2 = null;

    private XRRayInteractor rayInteractor = null;
    private bool isEnabled1 = false;
    private bool isEnabled2 = false;
    private bool isEnabledAll;

    void Awake()
    {
        rayInteractor = GetComponent<XRRayInteractor>();
    }

    private void OnEnable()
    {
        activateReference1.action.started += enable1;
        activateReference1.action.canceled += enable1;
        activateReference2.action.started += enable2;
        activateReference2.action.canceled += enable2;
    }

    private void OnDisable()
    {
        activateReference1.action.started -= enable1;
        activateReference1.action.canceled -= enable1;
        activateReference2.action.started -= enable2;
        activateReference2.action.canceled -= enable2;
    }

    private void enable1(InputAction.CallbackContext context)
    {
        isEnabled1 = context.control.IsPressed();
        isEnabledAll = isEnabled1 && isEnabled2 ? true : false;
    }

    private void enable2(InputAction.CallbackContext context)
    {
        isEnabled2 = context.control.IsPressed();
        isEnabledAll = isEnabled1 && isEnabled2 ? true : false;
    }

    void LateUpdate()
    {        
        ApplyStatus();
    }

    private void ApplyStatus()
    {
        if (rayInteractor.enabled != isEnabledAll)
            rayInteractor.enabled = isEnabledAll;
    }
}
