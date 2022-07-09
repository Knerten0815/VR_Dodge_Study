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
        [SerializeField] TMP_Text diagonaleValue, areaValue, diagonalWarning, areaWarning;

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

            List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();
            SubsystemManager.GetInstances(subsystems);
            foreach (var subsystem in subsystems)
                inputSystem = subsystem;


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

        public void DrawDiagonaleAndSetValue()
        {
            Vector3 start, end;
            diagonaleValue.text = TrackingSpaceGenerator.GetLongestDistanceInBoundaries(out start, out end).ToString("0.00");
            DrawLine(start, end, Color.red);
        }

        public void SetAreaValue()
        {
            areaValue.text = TrackingSpaceGenerator.GetTrackingSpaceArea().ToString("0.00");
        }

        public void DestroyDiagonale()
        {
            GameObject.Destroy(longestDiagonal);
        }

        /// <summary>
        /// taken from https://answers.unity.com/questions/8338/how-to-draw-a-line-using-script.html
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="color"></param>
        void DrawLine(Vector3 start, Vector3 end, Color color)
        {
            longestDiagonal = new GameObject();
            longestDiagonal.transform.position = start;
            longestDiagonal.AddComponent<LineRenderer>();
            LineRenderer lr = longestDiagonal.GetComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Standard"));
            lr.SetColors(color, color);
            lr.SetWidth(0.1f, 0.1f);
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }
    }
}
