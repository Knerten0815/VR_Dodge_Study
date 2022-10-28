namespace Dodge_Study
{
    public class ExperimentCondition
    {
        // Variables and their possible values. Adjust them here and only here.
        public enum Form { Sphere, MorningStar, Particles, Car }
        public static float[] sizes = new float[] { 0.1f, 1.2f };
        public static float[] speeds = new float[] { 3, 10, 30 };
        public static float[] angles = new float[] { 51, 34, 17, 0, -17, -34, -51 };          

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
        /// Has this condition caused a collision?
        /// </summary>
        public bool Collided { get { return collided; } set { collided = value; } }

        private int formIndex;
        private int sizeIndex;
        private int speedIndex;
        private int angleIndex;
        private bool collided = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="formIndex">0 = sphere; 1 = morning star; 2 = particle sphere; 3 = car</param>
        /// <param name="sizeIndex">0 = small; 1 = big;</param>
        /// <param name="speedIndex">0 = 3 km/h; 1 = 10 km/h; 2 = 30 km/h</param>
        /// <param name="angleIndex">0 = +51°; 1 = +34°; 2 = +17°; 3 = 0°; 4 = -17°; 5 = -34°; 6 = -51°</param>
        public ExperimentCondition(int formIndex, int sizeIndex, int speedIndex, int angleIndex)
        {
            this.formIndex = formIndex;
            this.sizeIndex = sizeIndex;
            this.speedIndex = speedIndex;
            this.angleIndex = angleIndex;
        }

        public Form GetForm() { return (Form)formIndex; }
        public float GetSize() { return sizes[sizeIndex]; }
        public float GetSpeed() { return speeds[speedIndex]; }
        public float GetAngle() { return angles[angleIndex]; }

        /* probably not used
         * 
        
        
        public enum Variable { Form, Size, Speed, Angle }

        public float GetFloatVariable(Variable variable)
        {
            switch (variable)
            {
                case Variable.Size:
                    return sizes[sizeIndex];

                case Variable.Speed:
                    return speeds[speedIndex];

                case Variable.Angle:
                    return angles[angleIndex];

                default:
                    return -1;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="indices">Variable indices: 0 = form; 1 = size; 2 = speed; 3 = angle </param>
        public ExperimentCondition(int[] indices)
        {
            if(indices.Length != 4)
                throw new System.Exception("An experiment condition needs 4 variables. You are trying to construct it with the wrong array length.");

            formIndex = indices[0];
            sizeIndex = indices[1];
            speedIndex = indices[2];
            angleIndex = indices[3];
        }
         */
    }
}
