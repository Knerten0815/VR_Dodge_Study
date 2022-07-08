using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace RD_Hiding
{
    public class SingletonFoEveryton : MonoBehaviour
    {
        public GameObject openRDWTrackingSpace;
        public GlobalConfiguration config;
        public GameObject camRig;
        public GameObject startUI;

        public XRInputSubsystem inputSystem = null;
        public InputDevice hmd;

        private bool firstStart = true;

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

        private void Update()
        {
            if(Mathf.Abs(Camera.main.transform.position.x) < 0.5f && Mathf.Abs(Camera.main.transform.position.z) < 0.5f && firstStart)
            {                
                firstStart = false;
                startUI.SetActive(true);
            }
        }

        public void RecenterHMD()
        {
            if (hmd.subsystem.TryRecenter())
                Debug.Log("Succesfully recentered!");
            else
                Debug.Log("Failed to recenter!");
        }

        public void instantiateSphere(Vector2 position, bool isPartOfTrackingSpace)
        {
            instantiateSphere(position, (Vector3.one * 0.1f), Color.yellow, isPartOfTrackingSpace);
        }

        public void instantiateSphere(Vector2 position, Vector3 size, Color colour, bool isPartOfTrackingSpace)
        {
            GameObject theSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            theSphere.GetComponent<MeshRenderer>().material.color = colour;

            theSphere.transform.position = new Vector3(position.x, 0, position.y);
            theSphere.transform.localScale = size;

            if (isPartOfTrackingSpace)
                theSphere.transform.SetParent(openRDWTrackingSpace.transform);
        }

        public void SetRelativeCameraPosition()
        {
            Vector2 center = TrackingSpaceGenerator.GetTrackingSpaceCenter();
            camRig.transform.position = new Vector3(-center.x, 0, -center.y);

            instantiateSphere(Vector2.zero, true);
        }
    }
}
