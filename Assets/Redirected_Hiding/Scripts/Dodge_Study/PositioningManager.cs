using System.Collections.Generic;
using UnityEngine;

namespace RD_Hiding
{
    public class PositioningManager : MonoBehaviour
    {
        public List<Vector2> boundary;
        public Vector2 boundaryCenter;
        
        private static PositioningManager _instance;
        public static PositioningManager Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
                Destroy(this);
            else
                _instance = this;

            boundary = TrackingSpaceGenerator.GetTrackingSpace(out boundaryCenter);

            Debug.Log("Fetched " + boundary.Count + " tracking space points. Boundary Center is: " + boundaryCenter);
        }
    }
}