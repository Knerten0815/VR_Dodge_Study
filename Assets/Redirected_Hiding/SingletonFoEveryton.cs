using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace RD_Hiding
{
    public class SingletonFoEveryton : MonoBehaviour
    {
        public GameObject testSphere;
        public GameObject openRDWTrackingSpace;
        public GameObject cam;
        public XRInputSubsystem inputSystem = null;
        public InputDevice hmd;

        #region Singleton Setup
        private static SingletonFoEveryton _instance;
        public static SingletonFoEveryton Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
                Destroy(this);
            else
                _instance = this;
        }
        #endregion

        void Start()
        {
            var hmdDevices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, hmdDevices);

            foreach (var device in hmdDevices)
                hmd = device;

            List<XRInputSubsystem> subsystems = new List<UnityEngine.XR.XRInputSubsystem>();
            SubsystemManager.GetInstances(subsystems);
            foreach (var subsystem in subsystems)
            {
                inputSystem = subsystem;
            }
        }

        public void RecenterHMD()
        {
            if (hmd.subsystem.TryRecenter())
                Debug.Log("Succesfully recentered!");
            else
                Debug.Log("Failed to recenter!");
        }

        public void instantiateSphere(Vector3 position, bool isPartOfTrackingSpace)
        {
            GameObject theSphere;
            if (isPartOfTrackingSpace)
                theSphere = Instantiate(testSphere, openRDWTrackingSpace.transform);
            else
                theSphere = Instantiate(testSphere);

            theSphere.transform.position = position;
        }

        public void instantiateSphere(Vector2 position, bool isPartOfTrackingSpace)
        {
            GameObject theSphere;
            if (isPartOfTrackingSpace)
                theSphere = Instantiate(testSphere, openRDWTrackingSpace.transform);
            else
                theSphere = Instantiate(testSphere);

            theSphere.transform.position = new Vector3(position.x, 0, position.y);
        }


        public void SetCameraToCenter()
        {
            Vector2 center = TrackingSpaceGenerator.GetTrackingSpaceCenter();
            cam.transform.position = new Vector3(-center.x, 0, -center.y);
        }
    }
}
