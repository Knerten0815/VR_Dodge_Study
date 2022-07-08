using System.Collections.Generic;
using TMPro;
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
        [SerializeField] TMP_Text diagonaleValue;

        public XRInputSubsystem inputSystem = null;
        public InputDevice hmd;

        private bool firstStart = true;

        #region Singleton Setup
        private static SingletonFoEveryton _instance;
        private GameObject longestDiagonal;

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
        }

        public void GetDiagonale(bool draw)
        {
            diagonaleValue.text = GetLongestDistanceInBoundaries(draw).ToString();
        }

        public void DestroyDiagonale()
        {
            GameObject.Destroy(longestDiagonal);
        }

        public float GetLongestDistanceInBoundaries(bool draw)
        {
            List<Vector2> bounds = TrackingSpaceGenerator.GetTrackingSpaceBoundaries();

            float longestDistance = 0;
            Vector3 center = new Vector3(TrackingSpaceGenerator.GetTrackingSpaceCenter().x, 0, TrackingSpaceGenerator.GetTrackingSpaceCenter().y);
            Vector3 start = Vector3.zero;
            Vector3 end = Vector3.zero;

            for (int i = 0; i < bounds.Count; i++)
            {
                if (i + 2 == bounds.Count)
                    break;

                for (int j = i + 2; j < bounds.Count; j++)
                {
                    float magnitude = (bounds[i] - bounds[j]).magnitude;
                    if (Mathf.Abs(magnitude) > longestDistance)
                    {
                        longestDistance = magnitude;
                        start = new Vector3(bounds[i].x, 0.1f, bounds[i].y) - center;
                        end = new Vector3(bounds[j].x, 0.1f, bounds[j].y) - center;
                    }
                }
            }

            if (draw)
                DrawLine(start, end, Color.red);

            return longestDistance;
        }

        void DrawLine(Vector3 start, Vector3 end, Color color)
        {
            longestDiagonal = new GameObject();
            longestDiagonal.transform.position = start;
            longestDiagonal.AddComponent<LineRenderer>();
            LineRenderer lr = longestDiagonal.GetComponent<LineRenderer>();
            lr.SetColors(color, color);
            lr.SetWidth(0.1f, 0.1f);
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }
    }
}
