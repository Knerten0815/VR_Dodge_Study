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

        float speedRatio;

        Redirector redirector;

        public override bool IsResetRequired()
        {
            return IfCollisionHappens();
        }

        public override void InitializeReset()
        {
            redirector = redirectionManager.redirector;
        }

        public override void InjectResetting()
        {
            redirector.InjectRedirection();
        }

        public override void EndReset()
        {
            DestroyHUD();
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
    }
}
