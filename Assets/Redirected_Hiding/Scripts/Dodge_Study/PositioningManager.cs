using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dodge_Study
{
    public class PositioningManager : MonoBehaviour
    {
        public List<Vector2> boundaryPoints;
        public Vector2 boundaryCenter;
        float centerMargin;

        private static PositioningManager _instance;
        public static PositioningManager Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
                Destroy(this);
            else
                _instance = this;

            boundaryPoints = TrackingSpaceGenerator.GetTrackingSpace(out boundaryCenter, out centerMargin);
            Debug.Log("Distance from center to tracking space is " + centerMargin);
        }
    }
}