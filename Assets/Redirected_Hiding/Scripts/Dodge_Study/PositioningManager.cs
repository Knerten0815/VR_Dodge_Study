using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Dodge_Study
{
    public class PositioningManager : MonoBehaviour
    {
        public List<Vector2> boundaryPoints;
        public Vector2 boundaryCenter;
        public float centerMargin;
        public Transform centerTrans;

        [SerializeField] Camera cam;
        [SerializeField] GameObject lookDirection;

        private Transform userTrans, lookDirTrans;
        private InputDevice device;
        private bool checkPositioning;
        private bool isPositioned, isDirected;

        private static PositioningManager _instance;
        public static PositioningManager Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
                Destroy(this);
            else
                _instance = this;

#if !UNITY_EDITOR
            if (OVRManager.instance == null)
            {
                Debug.Log("Adding OVR Manager!");
                OVRManager ovrInstance = gameObject.AddComponent<OVRManager>();
                ovrInstance.trackingOriginType = OVRManager.TrackingOrigin.FloorLevel;
            }
            else
                OVRManager.instance.trackingOriginType = OVRManager.TrackingOrigin.FloorLevel;
#endif
        }

        private void Update()
        {
            // this is all just for getting the look at ray
            Quaternion deviceRot;
            device.TryGetFeatureValue(CommonUsages.deviceRotation, out deviceRot);
            lookDirTrans.rotation = deviceRot;
            Ray r = new Ray(userTrans.position, lookDirTrans.TransformDirection(Vector3.forward));

            /*// test ray
            Debug.DrawRay(r.origin, r.direction);
            lookDirection.transform.position = r.origin + r.direction;*/


            if (checkPositioning)
            {
                float distance = Utilities.FlattenedPos2D(userTrans.position - centerTrans.position).magnitude;
                if (Mathf.Abs(distance) < 0.1f)
                    isPositioned = true;
                else
                    isPositioned = false;


                if (Physics.Raycast(userTrans.position, userTrans.TransformDirection(Vector3.forward)))
                    isDirected = true;
                else
                    isDirected = false;
            }
        }

        private void Start()
        {
            boundaryPoints = TrackingSpaceGenerator.GetTrackingSpace(out boundaryCenter, out centerMargin);
            centerTrans.localPosition = new Vector3(boundaryCenter.x, 0, boundaryCenter.y);
            Debug.Log("Boundary Center is at " + boundaryCenter.x + ", " + boundaryCenter.y);
            
            if (lookDirection == null)
                lookDirection = new GameObject();
            lookDirTrans = lookDirection.transform;
            userTrans = cam.transform;

            var hmdDevices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, hmdDevices);

            foreach (var d in hmdDevices)
            {
                device = d;
                Debug.Log("Found device.");
            }
        }

        public void showCenter(bool showCenter)
        {
            centerTrans.gameObject.SetActive(showCenter);
        }
    }
}