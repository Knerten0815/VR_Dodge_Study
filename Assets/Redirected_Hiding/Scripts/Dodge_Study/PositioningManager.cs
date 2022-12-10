using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
using TMPro;

namespace Dodge_Study
{
    public class PositioningManager : MonoBehaviour
    {
        public List<Vector2> boundaryPoints;
        public Vector2 boundaryCenter;
        public float centerMargin;
        public Transform centerTrans;
        public RedirectionManager rm;

        [SerializeField] Camera cam;
        [SerializeField] GameObject LookAtTarget;
        [SerializeField] MeshRenderer targetRndr;
        [SerializeField] Material hitMat, noHitMat;
        [SerializeField] Image loadCircle;
        [SerializeField] TMP_Text txt;
        [SerializeField] float secondsToStareAtTarget;

        private Transform userTrans, lookDirTrans;
        private InputDevice device;
        public bool checkPositioning;
        private bool isPositioned, isDirected;
        private Coroutine loadRoutine;

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
            LookAtTarget.SetActive(checkPositioning);

            if (checkPositioning)
            {
                
                // boiler plate for getting the lookAt-ray. Redirected walking is a mess.
                Quaternion deviceRot;
                device.TryGetFeatureValue(CommonUsages.deviceRotation, out deviceRot);
                // X from device, bc OpenRDW sets it to zero somehow. Y from OpenRDW bc its altered by OpenRDW. Z is the same in both.
                lookDirTrans.position = userTrans.position;
                lookDirTrans.rotation = Quaternion.Euler(deviceRot.eulerAngles.x, userTrans.eulerAngles.y, userTrans.eulerAngles.z);
                Ray r = new Ray(userTrans.position, lookDirTrans.TransformDirection(Vector3.forward));


                RaycastHit hitInfo;
                if (Physics.Raycast(r, out hitInfo) && hitInfo.collider.tag == "Target")
                {
                    isDirected = true;
                    if (loadRoutine == null)
                    {
                        Debug.Log("starting Coroutine");

                        targetRndr.material = hitMat;
                        loadRoutine = StartCoroutine(loadingCircle());
                    }
                }
                else
                {
                    isDirected = false;
                    if (loadRoutine != null)
                    {
                        Debug.Log("stopping Coroutine");

                        targetRndr.material = noHitMat;
                        StopAllCoroutines();
                        loadRoutine = null;

                        loadCircle.fillAmount = 0;
                        txt.gameObject.SetActive(false);
                    }
                }

                /*// test ray
                Debug.Log("Device Rot: " + deviceRot.eulerAngles);
                Debug.Log("User Rot: " + userTrans.eulerAngles);
                Debug.DrawRay(r.origin, r.direction);
                lookDirTrans.position = r.origin + r.direction;
                lookDirTrans.rotation = deviceRot;*/


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
            
            lookDirTrans = new GameObject().transform;
            userTrans = cam.transform;

            var hmdDevices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, hmdDevices);

            foreach (var d in hmdDevices)
            {
                device = d;
                Debug.Log("Found device.");
            }
        }

        IEnumerator loadingCircle()
        {
            loadCircle.fillAmount = 0;
            txt.gameObject.SetActive(true);

            float thirdOfLoadTime = secondsToStareAtTarget / 3;
            float timer = 0;
            int countDown = 3;

            Debug.Log("ThirdOfloadTime = " + thirdOfLoadTime);

            while (isDirected && countDown > 0)
            {
                
                Debug.Log("FillAmount = " + loadCircle.fillAmount);
                Debug.Log("Countdown = " + countDown);
                
                loadCircle.fillAmount += secondsToStareAtTarget / Time.deltaTime;
                timer += Time.deltaTime;
                Debug.Log("Timer = " + timer);


                if (timer >= thirdOfLoadTime)
                {
                    timer = 0;
                    countDown--;
                }

                txt.text = countDown.ToString();
                Debug.Log("isDirected = " + isDirected);
                Debug.Log("while = " + (isDirected && countDown > 0));
                yield return new WaitForEndOfFrame();
            }

            Debug.Log("Countdown = " + countDown);

            loadCircle.fillAmount = 0;
            txt.gameObject.SetActive(false);
            yield return null;
        }

        public void showCenter(bool showCenter)
        {
            centerTrans.gameObject.SetActive(showCenter);
        }
    }
}