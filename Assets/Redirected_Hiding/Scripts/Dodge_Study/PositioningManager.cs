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
        public Transform centerTrans, backPosTrans;
        public RedirectionManager rm;
        [HideInInspector]
        public InputDevice device;

        [SerializeField] Camera cam;
        [SerializeField] GameObject LookAtTarget, Crosshair;
        [SerializeField] MeshRenderer targetRndr;
        [SerializeField] MeshRenderer[] bootRndrs;
        [SerializeField] Material hitMat, noHitMat;
        [SerializeField] Image loadCircle;
        [SerializeField] TMP_Text countdown, command;
        [SerializeField] float secondsToStareAtTarget;
        [SerializeField] Light positioningLight;
        [SerializeField] Color incorrect, correct;
        [SerializeField] float positioningTolerance = 0.1f;
        [SerializeField] GameObject trialBoots, nonTrialBoots;
        [SerializeField] GameObject WarningUI;
        [SerializeField] TMP_Text warningUIMarginText, debugUIMarginText;

        private Transform userTrans, lookDirTrans;
        public bool checkPositioning, userIsReady;
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

        private void Start()
        {
            SetTrackingSpaceAndCenter();

            lookDirTrans = new GameObject().transform;
            userTrans = cam.transform;
            command.text = "Stell dich ins Licht und schau auf die Zielscheibe.";

            var hmdDevices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, hmdDevices);
            foreach (var d in hmdDevices)
                device = d;
        }

        private void Update()
        {
            LookAtTarget.SetActive(checkPositioning);
            positioningLight.gameObject.SetActive(checkPositioning);
            Crosshair.SetActive(checkPositioning);
            nonTrialBoots.SetActive(checkPositioning);
            trialBoots.SetActive(!checkPositioning);

            if (checkPositioning)
            {
                Vector3 commandPosition = LookAtTarget.transform.position;
                commandPosition.y = command.transform.position.y;
                command.transform.position = commandPosition;
                command.transform.rotation = LookAtTarget.transform.rotation;
                command.text = "Stell dich ins Licht und schau auf die Zielscheibe.";

                // boiler plate for getting the lookAt-ray. Redirected walking is a mess.
                Quaternion deviceRot;
                device.TryGetFeatureValue(CommonUsages.deviceRotation, out deviceRot);
                lookDirTrans.position = userTrans.position;
                // X from device, bc OpenRDW sets it to zero somehow. Y from OpenRDW because its altered by OpenRDW. Z is the same in both.
                lookDirTrans.rotation = Quaternion.Euler(deviceRot.eulerAngles.x, userTrans.eulerAngles.y, userTrans.eulerAngles.z);
                Ray r = new Ray(userTrans.position, lookDirTrans.TransformDirection(Vector3.forward));

                RaycastHit hitInfo;
                if (Physics.Raycast(r, out hitInfo) && hitInfo.collider.tag == "Target")
                {
                    isDirected = true;
                    targetRndr.material = hitMat;
                }
                else
                {
                    isDirected = false;
                    targetRndr.material = noHitMat;
                }

                float distance = Utilities.FlattenedPos2D(userTrans.position - centerTrans.position).magnitude;
                if (Mathf.Abs(distance) < positioningTolerance)
                {
                    positioningLight.color = correct;
                    foreach(MeshRenderer rndr in bootRndrs)
                        rndr.material = hitMat;
                    isPositioned = true;
                }
                else
                {
                    foreach (MeshRenderer rndr in bootRndrs)
                        rndr.material = noHitMat;
                    positioningLight.color = incorrect;
                    isPositioned = false;
                }

                if (isDirected && isPositioned)
                {
                    if(loadRoutine == null)
                        loadRoutine = StartCoroutine(stayDirected());
                }
                else
                {
                    StopAllCoroutines();
                    loadRoutine = null;

                    loadCircle.fillAmount = 0;
                    loadCircle.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Called on Start and by DevToolUI button
        /// </summary>
        public void SetTrackingSpaceAndCenter()
        {
            boundaryPoints = TrackingSpaceGenerator.GetTrackingSpace(out boundaryCenter, out centerMargin);
            centerTrans.localPosition = new Vector3(boundaryCenter.x, 0, boundaryCenter.y);
            Debug.Log("Boundary Center is at " + boundaryCenter.x + ", " + boundaryCenter.y);
            
            if (centerMargin < 1)
                WarningUI.SetActive(true);
            else
                WarningUI.SetActive(false);

            warningUIMarginText.text = centerMargin.ToString();
        }

        public void StartExperimentByButtonPress()
        {
            checkPositioning = true;
        }

        IEnumerator stayDirected()
        {
            loadCircle.fillAmount = 0;
            loadCircle.gameObject.SetActive(true);

            float thirdOfLoadTime = secondsToStareAtTarget / 3;
            float timer = 0;
            int countDown = 3;

            while (isDirected && isPositioned &&  countDown > 0)
            {
                loadCircle.fillAmount += Time.deltaTime / secondsToStareAtTarget;
                timer += Time.deltaTime;

                if (timer >= thirdOfLoadTime)
                {
                    timer = 0;
                    countDown--;
                }

                countdown.text = countDown.ToString();
                yield return new WaitForEndOfFrame();
            }

            loadCircle.fillAmount = 0;
            loadCircle.gameObject.SetActive(false);
            checkPositioning = false;
            ExperimentManager.Instance.StartTrial();
            command.text = "Weich aus!";
            yield return null;
        }

        public void showCenter(bool showCenter)
        {
            centerTrans.gameObject.SetActive(showCenter);
        }
    }
}