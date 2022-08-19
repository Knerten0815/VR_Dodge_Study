using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RD_Hiding
{
    public class ShepherdResetter : Resetter
    {
        float requiredRotateSteerAngle = 0;//steering angle，rotate the physical plane and avatar together

        float requiredRotateAngle = 0;//normal rotation angle, only rotate avatar

        float rotateDir;//rotation direction, positive if rotate clockwise

        float circleDiameter;
        float resetRingDiameter;
        float maxPlayerRingDepth = 0;
        GameObject shepGO;

        Redirector redirector;

        private new void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            circleDiameter = redirectionManager.globalConfiguration.squareWidth;
            resetRingDiameter = circleDiameter * 4;
            redirector = redirectionManager.redirector;

            Debug.Log("ResetRingDiamter: " + resetRingDiameter);

            // draw resetRing
            foreach (var point in redirectionManager.globalConfiguration.trackingSpacePoints)
                SingletonFoEveryton.Instance.instantiateSphere(point.normalized * resetRingDiameter, true);
        }

        public override bool IsResetRequired()
        {
            return IfCollisionHappens();
        }

        public override void InitializeReset()
        {
<<<<<<< HEAD
            Debug.Log("Initalize Reset.");
            maxPlayerRingDepth = 0;
            spawnShepherd();
=======
            redirector = redirectionManager.redirector;
>>>>>>> dd2ae00ca1f06ebd8e35c46881658d99f8f4c86c
        }

        public override void InjectResetting()
        {
            redirector.InjectRedirection();
            calculateShepPosition();
            if(redirectionManager.currPosReal.magnitude < circleDiameter / 2 - 0.1)
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
            shepGO = SingletonFoEveryton.Instance.instantiateSphere(shepPos, true, Vector3.one, Color.red);
            shepGO.transform.localPosition = shepPos;
            shepGO.name = "Shepherd";
        }

        private Vector2 calculateShepPosition()
        {
            float playerRingDepth = redirectionManager.currPosReal.magnitude - circleDiameter / 2;

            if (playerRingDepth > maxPlayerRingDepth)
                maxPlayerRingDepth = playerRingDepth;

            Vector3 shepPos = redirectionManager.currPosReal.normalized * (resetRingDiameter - maxPlayerRingDepth);
            shepPos.y = 1;
            shepGO.transform.localPosition = shepGO.transform.InverseTransformPoint(shepPos);

            return Vector2.zero;
        }
    }
}
