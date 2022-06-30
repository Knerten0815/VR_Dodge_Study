using UnityEngine;

namespace RD_Hiding
{
    [RequireComponent(typeof(GlobalConfiguration))]
    public class GainsSetter : MonoBehaviour
    {
        GlobalConfiguration config;

        private void Awake()
        {

            config = GetComponent<GlobalConfiguration>();
        }

        public void SetMaxTransGain(float maxTrans)
        {
            config.MAX_TRANS_GAIN = maxTrans;
        }

        public void SetMinTransGain(float minTrans)
        {
            config.MIN_TRANS_GAIN = minTrans;
        }

        public void SetMaxRotGain(float maxRot)
        {
            config.MAX_ROT_GAIN = maxRot;
        }

        public void SetMinRotGain(float minRot)
        {
            config.MIN_ROT_GAIN = minRot;
        }

        public void SetCurvatureRadius(float curveRad)
        {
            config.CURVATURE_RADIUS = curveRad;
        }

        public void SetResetBuffer(float resetBuffer)
        {
            config.RESET_TRIGGER_BUFFER = resetBuffer;
        }
    }
}

