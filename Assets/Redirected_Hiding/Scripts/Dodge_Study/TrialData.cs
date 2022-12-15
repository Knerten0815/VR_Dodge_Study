using UnityEngine;

namespace Dodge_Study
{
    public class TrialData
    {
        // Variables and their possible values. Adjust them here and only here.
        public enum Form { Sphere, MorningStar, Cylinder, Car }
        public static float[] sizes = new float[] { 0.4f, 1f };
        public static float[] speeds = new float[] { 18, 24, 30 };
        public static float[] angles = new float[] { 40, 27, 13, 0, -13, -27, -40 };

        /// <summary>
        /// Indexed with 0. Values:
        /// 0 = sphere; 1 = morning star; 2 = particle sphere; 3 = car
        /// </summary>
        public int FormIndex { get { return formIndex; } }

        /// <summary>
        /// Indexed with 1. Values:
        /// 0 = small; 1 = big;
        /// </summary>
        public int SizeIndex { get { return sizeIndex; } }

        /// <summary>
        /// Indexed with 2. Values:
        /// 0 = 3 km/h; 1 = 10 km/h; 2 = 30 km/h
        /// </summary>
        public int SpeedIndex { get { return speedIndex; } }

        /// <summary>
        /// Indexed with 3. Values:
        /// 0 = +51°; 1 = +34°; 2 = +17°; 3 = 0°; 4 = -17°; 5 = -34°; 6 = -51°
        /// </summary>
        public int AngleIndex { get { return angleIndex; } }

        /// <summary>
        /// The ID of a specific condition. Is made up of the 4 Variable indices. This only works if all variables have less than 10 states.
        /// </summary>
        public string TrialID { get { return trialID; } }

        /// <summary>
        /// Has this condition caused a collision?
        /// </summary>
        public bool CollisionDetected { get { return collisionDetected; } set { collisionDetected = value; } }

        private int formIndex;
        private int sizeIndex;
        private int speedIndex;
        private int angleIndex;
        private bool collisionDetected = false;
        private string trialID;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="formIndex">0 = sphere; 1 = morning star; 2 = particle sphere; 3 = car</param>
        /// <param name="sizeIndex">0 = small; 1 = big;</param>
        /// <param name="speedIndex">0 = 3 km/h; 1 = 10 km/h; 2 = 30 km/h</param>
        /// <param name="angleIndex">0 = +51°; 1 = +34°; 2 = +17°; 3 = 0°; 4 = -17°; 5 = -34°; 6 = -51°</param>
        public TrialData(int formIndex, int sizeIndex, int speedIndex, int angleIndex)
        {
            this.formIndex = formIndex;
            this.sizeIndex = sizeIndex;
            this.speedIndex = speedIndex;
            this.angleIndex = angleIndex;

            trialID = "" + formIndex + sizeIndex + speedIndex + angleIndex;
        }

        public Form GetForm() { return (Form)formIndex; }
        public float GetSize() { return sizes[sizeIndex]; }
        public float GetSpeed() { return speeds[speedIndex]; }
        public float GetAngle() { return angles[angleIndex]; }
    }
}
