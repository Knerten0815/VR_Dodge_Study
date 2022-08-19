using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RD_Hiding
{
    public class SingletonFoEveryton : MonoBehaviour
    {
        public GameObject openRDWTrackingSpace;
        public GlobalConfiguration config;
        public GameObject camRig;
        public GameObject startUI;
        public float distanceToActivateStartUI = 0.1f;
        public RayToggler leftRayToggler, rightRayToggler;
        public GameObject dronePreFab;
        [SerializeField] bool ignoreWarning; 
        [SerializeField] GameObject warningUI;
        [SerializeField] float minArea, maxArea, minDiagonal, maxDiagonal;
        [SerializeField] TMP_Text diagonaleValue, areaValue, resemblingArea, diagonalWarning, areaWarning;
        public bool getLoadFromTxt;
        public GlobalConfiguration.MovementController getMovementController;

        private bool firstStart = true;
        private List<GameObject> debugVisuals = new List<GameObject>();

        #region Singleton Setup
        private static SingletonFoEveryton _instance;

        public static SingletonFoEveryton Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
                Destroy(this);
            else
                _instance = this;

            getLoadFromTxt = config.loadFromTxt;
            getMovementController = config.movementController;
        }
        #endregion

        void Start()
        {
            //check Tracking Space dimensions
            if (!ignoreWarning)
            {
                float area = TrackingSpaceGenerator.GetTrackingSpaceArea();
                float diagonal = TrackingSpaceGenerator.GetLongestDistanceInBoundaries(out _, out _);
                areaWarning.text = area.ToString("0.00");
                diagonalWarning.text = diagonal.ToString("0.00");

                if (area > maxArea || area < minArea || diagonal > maxArea || diagonal < minArea)
                {
                    Debug.Log("TrackingSpace is too small. Opening Warning UI.");
                    warningUI.SetActive(true);
                }                    
            }            
        }

        private void Update()
        {
            if(config.movementController == GlobalConfiguration.MovementController.HMD && firstStart && Mathf.Abs(Camera.main.transform.position.x) < distanceToActivateStartUI && Mathf.Abs(Camera.main.transform.position.z) < distanceToActivateStartUI)
            {
                Debug.Log("Participant entered start location: Opening Start Screen");
                firstStart = false;
                startUI.SetActive(true);
                leftRayToggler.alwaysShowRays = true;
                rightRayToggler.alwaysShowRays = true;
            }
        }

        public void SetRelativeCameraPosition()
        {
            Vector2 center = TrackingSpaceGenerator.GetTrackingSpaceCenter();
            camRig.transform.position = new Vector3(-center.x, 0, -center.y);
        }

        public GameObject instantiateSphere(Vector2 position, bool isPartOfTrackingSpace)
        {
            return instantiateSphere(position, isPartOfTrackingSpace, (Vector3.one * 0.1f), Color.yellow);
        }

        public GameObject instantiateSphere(Vector2 position, bool isPartOfTrackingSpace, Vector3 size, Color colour)
        {
            GameObject theSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            theSphere.GetComponent<MeshRenderer>().material.color = colour;

            theSphere.transform.position = new Vector3(position.x, 0, position.y);
            theSphere.transform.localScale = size;

            if (isPartOfTrackingSpace)
                theSphere.transform.SetParent(openRDWTrackingSpace.transform);

            return theSphere;
        }

        #region debug visuals
        public void DrawDiagonaleAndSetValue()
        {
            diagonaleValue.text = TrackingSpaceGenerator.GetLongestDistanceInBoundaries(out Vector3 start, out Vector3 end).ToString("0.00");
            DrawLine(start, end);
        }

        public void DrawResemblingRectangleAndSetValue()
        {
            resemblingArea.text = TrackingSpaceGenerator.GetQuadResemblingTrackingSpace(out Vector3 a, out Vector3 b, out Vector3 c, out Vector3 d).ToString("0.00");
            DrawLine(a, b);
            DrawLine(b, c);
            DrawLine(c, d);
            DrawLine(d, a);
            DrawLine(b, d);
            debugVisuals.Add(instantiateSphere(Vector2.zero, false));
        }

        public void SetAreaValue()
        {
            areaValue.text = TrackingSpaceGenerator.GetTrackingSpaceArea().ToString("0.00");
        }

        public void DestroyDebugVisuals()
        {
            foreach(GameObject go in debugVisuals)
                GameObject.Destroy(go);
        }

        /// <summary>
        /// taken from https://answers.unity.com/questions/8338/how-to-draw-a-line-using-script.html
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        void DrawLine(Vector3 start, Vector3 end, float width = 0.05f)
        {
            GameObject line = new GameObject();
            line.transform.position = start;
            line.AddComponent<LineRenderer>();
            LineRenderer lr = line.GetComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Standard"));
            lr.startWidth = width;
            lr.endWidth = width;
            lr.startColor = Color.red;
            lr.endColor = Color.red;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);

            debugVisuals.Add(line);
        }
        #endregion
    }
}
