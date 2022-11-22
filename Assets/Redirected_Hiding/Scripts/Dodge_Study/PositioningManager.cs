using System.Collections.Generic;
using UnityEngine;

namespace Dodge_Study
{
    public class PositioningManager : MonoBehaviour
    {
        public List<Vector2> boundaryPoints;
        public Vector2 boundaryCenter;
        public float centerMargin;
        public Transform centerTrans;

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

        private void Start()
        {
            boundaryPoints = TrackingSpaceGenerator.GetTrackingSpace(out boundaryCenter, out centerMargin);
            centerTrans.localPosition = new Vector3(boundaryCenter.x, 0, boundaryCenter.y);
            Debug.Log("Distance from center to tracking space is " + centerMargin);
        }

        public void showCenter(bool showCenter)
        {
            centerTrans.gameObject.SetActive(showCenter);
        }
    }
}