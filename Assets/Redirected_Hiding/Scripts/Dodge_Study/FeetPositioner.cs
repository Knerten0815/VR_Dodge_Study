using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Dodge_Study
{
    public class FeetPositioner : MonoBehaviour
    {
        public float height = 0;
        private InputDevice hmd;
        private void Start()
        {
            var hmdDevices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, hmdDevices);
            foreach (var d in hmdDevices)
                hmd = d;
        }

        void Update()
        {
            Vector3 pos;
            hmd.TryGetFeatureValue(CommonUsages.centerEyePosition, out pos);
            pos.y = height;
            transform.position = pos;

            Quaternion rot;
            hmd.TryGetFeatureValue(CommonUsages.centerEyeRotation, out rot);
            transform.rotation = Quaternion.Euler(0, rot.eulerAngles.y, 0);
        }
    }
}