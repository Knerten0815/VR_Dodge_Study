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
        }

        public override bool IsResetRequired()
        {
            return IfCollisionHappens();
        }

        public override void InitializeReset()
        {
            Debug.Log("Initalize Reset.");
            //spawnShepherd();
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

        /* ---- no more shepherds needed...
        private void spawnShepherd()
        {
            Vector3 shepPos = redirectionManager.currPosReal.normalized * resetRingDiameter;
            shepGO = Instantiate(SingletonFoEveryton.Instance.dronePreFab);
            shepGO.transform.SetParent(SingletonFoEveryton.Instance.shepherdTarget.transform);
            shepPos.y = -3;
            shepGO.transform.localPosition = shepPos;
        }
        */
    }
}
