using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RD_Hiding
{
    public class ShepherdRedirectorReplay : SteerToRedirector
    {
        // Testing Parameters
        bool dontUseTempTargetInS2C = false;


        private const float S2C_BEARING_ANGLE_THRESHOLD_IN_DEGREE = 160;
        private const float S2C_TEMP_TARGET_DISTANCE = 4;

        public override void PickRedirectionTarget()
        {
            Vector3 userToCenter = -redirectionManager.currPosReal;

            //Compute steering target for S2C
            float bearingToCenter = Vector3.Angle(userToCenter, redirectionManager.currDir);//unsigned angle
            float directionToCenter = Mathf.Sign(Utilities.GetSignedAngle(redirectionManager.currDir, userToCenter));//signed angle
                                                                                                                     //Debug.Log(bearingToCenter);
            if (bearingToCenter >= S2C_BEARING_ANGLE_THRESHOLD_IN_DEGREE && !dontUseTempTargetInS2C)
            {
                //Generate temporary target
                if (noTmpTarget)
                {
                    tmpTarget = new GameObject("S2C Temp Target");
                    tmpTarget.transform.position = redirectionManager.currPos + S2C_TEMP_TARGET_DISTANCE * (Quaternion.Euler(0, directionToCenter * 90, 0) * redirectionManager.currDir);
                    tmpTarget.transform.parent = transform;
                    noTmpTarget = false;
                }
                currentTarget = tmpTarget.transform;
            }
            else
            {
                currentTarget = redirectionManager.trackingSpace;
                if (!noTmpTarget)
                {
                    Destroy(tmpTarget);
                    noTmpTarget = true;
                }
            }
        }
    }
}