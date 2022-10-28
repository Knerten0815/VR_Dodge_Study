using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dodge_Study
{
    public class SteerToBackRedirector : SteerToRedirector
    {
        // Testing Parameters
        bool useTempTargetInS2B = true;


        private const float S2B_UNSIGNED_ANGLE_THRESHOLD_IN_DEGREE = 160;
        private const float S2B_TEMP_TARGET_DISTANCE = 4;

        public override void PickRedirectionTarget()
        {
            //Vector3 trackingAreaPosition = Utilities.FlattenedPos3D(redirectionManager.trackingSpace.position);
            Vector3 userToTarget = Vector3.back - redirectionManager.currPos;

            //Compute steering target for S2B
            float unsignedAngleToTarget = Vector3.Angle(userToTarget, redirectionManager.currDir);//unsigned angle
            float directionToTarget = Mathf.Sign(Utilities.GetSignedAngle(redirectionManager.currDir, userToTarget));//signed angle
                                                                                                                     //Debug.Log(bearingToCenter);
            if (unsignedAngleToTarget >= S2B_UNSIGNED_ANGLE_THRESHOLD_IN_DEGREE && useTempTargetInS2B)
            {
                //Generate temporary target
                if (noTmpTarget)
                {
                    tmpTarget = new GameObject("S2B Temp Target");
                    tmpTarget.transform.position = redirectionManager.currPos + S2B_TEMP_TARGET_DISTANCE * (Quaternion.Euler(0, directionToTarget * 90, 0) * redirectionManager.currDir);
                    tmpTarget.transform.parent = transform;
                    noTmpTarget = false;
                }
                currentTarget = tmpTarget.transform;
            }
            else
            {
                currentTarget.position = Vector3.back;
                if (!noTmpTarget)
                {
                    Destroy(tmpTarget);
                    noTmpTarget = true;
                }
            }
        }
    }
}
