using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RD_Hiding
{
    public class ShepherdResetter : Resetter
    {
        GameObject shepGO;
        Redirector redirector;

        float requiredRotateSteerAngle = 0;//steering angle，rotate the physical plane and avatar together

        float requiredRotateAngle = 0;//normal rotation angle, only rotate avatar

        float rotateDir;//rotation direction, positive if rotate clockwise

        public float circleDiameter;
        public float resetRingDiameter;        
        public float ShepherdSpeed = 2;
        public bool showShepherdTarget = false;

        private new void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            circleDiameter = redirectionManager.globalConfiguration.squareWidth;
            resetRingDiameter = circleDiameter * 4;
            redirector = redirectionManager.redirector;

            /*
            // draw resetRing
            foreach (var point in redirectionManager.globalConfiguration.trackingSpacePoints)
                SingletonFoEveryton.Instance.instantiateSphere(point.normalized * resetRingDiameter, true);
            */

            if (SingletonFoEveryton.Instance.drawTrueTrackingBoundaries)
            {
                // draw trackingSpaceBoundaries
                var trackingBoundary = TrackingSpaceGenerator.GetTrackingSpaceBoundaries();
                if (trackingBoundary.Count == 0)
                {
                    // draw 5x5 m tracking boundary if not headset is used
                    TrackingSpaceGenerator.GenerateRectangleTrackingSpace(0, out trackingBoundary, out _, out _, 5f, 5f);
                    SingletonFoEveryton.Instance.DrawLine(trackingBoundary[0], trackingBoundary[1]);
                    SingletonFoEveryton.Instance.DrawLine(trackingBoundary[1], trackingBoundary[2]);
                    SingletonFoEveryton.Instance.DrawLine(trackingBoundary[2], trackingBoundary[3]);
                    SingletonFoEveryton.Instance.DrawLine(trackingBoundary[3], trackingBoundary[0]);
                }
                else
                {
                    foreach (var point in trackingBoundary)
                        SingletonFoEveryton.Instance.instantiateSphere(point, true);
                }
            }
        }

        public override bool IsResetRequired()
        {
            return IfCollisionHappens();
        }

        public override void InitializeReset()
        {
            Debug.Log("Initalize Reset.");
            spawnShepherd();
        }

        public override void InjectResetting()
        {
            redirector.InjectRedirection();
            if (redirectionManager.currPosReal.magnitude < circleDiameter / 2 - 0.1)
            {
                redirectionManager.OnResetEnd();
            }
        }

        public override void EndReset()
        {
            Debug.Log("End Reset.");
            DestroyHUD();
            Destroy(shepGO);
        }

        public override void SimulatedWalkerUpdate()
        {
            // Act is if there's some dummy target a meter away from you requiring you to rotate        
            var rotateAngle = redirectionManager.GetDeltaTime() * redirectionManager.globalConfiguration.rotationSpeed;
            //finish specified rotation
            if (rotateAngle >= requiredRotateAngle)
            {
                rotateAngle = requiredRotateAngle;
                //Avoid accuracy error
                requiredRotateAngle = 0;
            }
            else
            {
                requiredRotateAngle -= rotateAngle;
            }
            redirectionManager.simulatedWalker.RotateInPlace(rotateAngle * rotateDir);
        }

        private void spawnShepherd()
        {
            Vector3 shepPos = redirectionManager.currPosReal.normalized * resetRingDiameter;
            shepGO = Instantiate(SingletonFoEveryton.Instance.dronePreFab);
            shepGO.transform.SetParent(SingletonFoEveryton.Instance.shepherdTarget.transform);
            shepPos.y = -3;
            shepGO.transform.localPosition = shepPos;

            /* --- material or shader switch unfortantely are not instant
            SingletonFoEveryton.Instance.planeRenderer.material = SingletonFoEveryton.Instance.groundplaneDissolveMat;
            StartCoroutine(waitForShepherdAscend());
            */
        }

        /*
        IEnumerator waitForShepherdAscend()
        {
            while(shepGO != null && shepGO.transform.position.y < 1.5f)
            {
                yield return null;
            }

            SingletonFoEveryton.Instance.planeRenderer.material = SingletonFoEveryton.Instance.groundPlaneMat;
        }
        */
    }
}
