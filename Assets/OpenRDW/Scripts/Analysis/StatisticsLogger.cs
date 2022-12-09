using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
public class StatisticsLogger : MonoBehaviour {
    private static readonly int MaxResetCount = 1000;//reset exceeds this value will make data invalid
    
    private GlobalConfiguration globalConfiguration;
    [Tooltip("If log sample variables")]
    public bool logSampleVariables = false;

    public float pathStartAlpha = 0.3f;

    public class ResultOfTrial {
        public int endState;
        public List<Dictionary<string, string>> result;
        public ResultOfTrial(int endState, List<Dictionary<string, string>> result) {
            this.endState = endState;
            this.result = result;
        }
        public string EndStateToString() {
            var experimentState = "";
            switch (endState)
            {
                case -1:
                    experimentState = "Invalid";
                    break;
                case 0:
                    experimentState = "Normal";
                    break;
                case 1:
                    experimentState = "Manually";
                    break;
                case 2:
                    experimentState = "Collision";
                    break;
                default:
                    experimentState = "Undefined";
                    break;
            }
            return experimentState;
        }
    }
    [HideInInspector]
    public List<ResultOfTrial> experimentResults = new List<ResultOfTrial> () ;//outside to inside: every trial->every avatar

    [Tooltip("How often we will gather data we have and log it in hertz, average 1 / samplingFrequency second records to one sample")]
    public float samplingFrequency = 10;

    [Header("Image")]
    [Tooltip("Side resolution of a square image(resolution * resolution)")]
    public int imageResolution;
    [Tooltip("Side length represent the real world meters")]
    public int realSideLength;
    [Tooltip("Side length represent the real world meters")]
    public int virtualSideLength;
    [Tooltip("Border thickness drawn in the image")]
    public int borderThickness;
    [Tooltip("Path thickness drawn in the image")]
    public int pathThickness;

    [Header("Video")]
    // Public Properties
    [Tooltip("Maximum number of frames you want to record in one video")]
    public int maxFrames;
    [Tooltip("Number of frames to capture per second")]
    public int frameRate = 30;

    [Header("Screenshot")]
    [Tooltip("Factor by which to increase resolution")]
    [Range(1,10)]
    public int superSize;

    [SerializeField] private Color backgroundColor;
    [SerializeField] private Color realPathColor;
    [SerializeField] private Color trackingSpaceColor;
    [SerializeField] private Color boundaryColor;
    [SerializeField] private Color[] virtualPathColors;

    private Color obstacleColor;
    // The way this works is that we wait 1 / samplingFrequency time to transpire before we attempt to clean buffers and gather samples
    // And since we always get a buffer value right before collecting samples, we'll have at least 1 buffer value to get an average from
    // The only problem with this is that overall we'll be gathering less than the expected frequency since the "lateness" of sampling will accumulate

    // THE FOLLOWING PARAMETERS MUST BE SENSITIVE TO TIME SCALE

    // TEMPORARILY SETTING ALL TO PUBLIC FOR TESTING
    //avatar statistics
    public class AvatarStatistics {
        // Redirection Single Parameters
        public float sumOfInjectedTranslation = 0; // Overall amount of displacement (IN METERS) of redirection reference due to translation gain (always positive)
        public float sumOfInjectedRotationFromRotationGain = 0; // Overall amount of rotation (IN degrees) (around user) of redirection reference due to rotation gain (always positive)
        public float sumOfInjectedRotationFromCurvatureGain = 0; // Overall amount of rotation (IN degrees) (around user) of redirection reference due to curvature gain (always positive)
        public float maxTranslationGain = float.MinValue;
        public float minTranslationGain = float.MaxValue;
        public float maxRotationGain = float.MinValue;
        public float minRotationGain = float.MaxValue;
        public float maxCurvatureGain = float.MinValue;
        public float minCurvatureGain = float.MaxValue;

        // -------------------------------------------------------------------------------------------- VR-Dodge-Study parameters -----------------------------------------------------------------------------------
        public float maxYawVirtual;                                                                             // maximum virtual yaw rotation in degrees, that was achieved during a trial
        public float maxYawReal;                                                                                // maximum real yaw rotation in degrees, that was achieved during a trial
        public float maxDistanceToCenter;                                                                       // maximum distance moved away rom center during one trial
        public Vector3 realPosAtMaxDistanceToCenter, virtPosAtMaxDistanceToCenter;                              // actual positions, when greatest distance was achieved
        public Vector3 realRotAtMaxRealYaw, virtRotAtMaxRealYaw, realRotAtMaxVirtYaw, virtRotAtMaxVirtYaw;      // actual head rotations, when maximum yaw rotations were achieved
        public Vector3 realPosAtMaxRealYaw, virtPosAtMaxRealYaw, realPosAtMaxVirtYaw, virtPosAtMaxVirtYaw;      // actual head positions, when maximum yaw rotations were achieved
        public bool collisionDetected;                                                                          // did the test person successfuly dodge
        public List<float> addVirtRotAccumulationSamples = new List<float>();                            // the values grow, as long as the test person rotates in one direction. Can be used to read rotation in degrees of every movement
        public List<float> realRotAccumulationSamples = new List<float>();                                // see above. But for real rotation, instead of virtual rotation
        public List<Vector3> realRotSamples = new List<Vector3>();                                        // logging the actual rotations
        public List<Vector3> virtRotSamples = new List<Vector3>();                                        // logging the virtual rotations
        // --------------------------- Buffers ----------------------
        public List<float> addVirtRotAccumulationSamplesBuffer = new List<float>();
        public List<float> realRotAccumulationSamplesBuffer = new List<float>();
        public List<Vector3> realRotSamplesBuffer = new List<Vector3>();
        public List<Vector3> virtRotSamplesBuffer = new List<Vector3>();
        // also sumOfInjectedRotationFromRotationGain //-done
        // => Rotation der virtuellen Welt nach Repositionierung der Versuchsperson, relativ zur Rotation der virtuellen Welt zu Beginn eines Ausweichmanövers
        // -------------------------------------------------------------------------------------------- VR-Dodge-Study parameters ------------------------------------------------------------------------------------

        // Reset Single Parameters
        public float resetCount = 0;
        public float sumOfVirtualDistanceTravelled = 0; // Based on user movement controller plus redirection movement
        public float sumOfRealDistanceTravelled = 0; // Based on user movement controller
        public float experimentBeginningTime = 0;//calculated according to getDeltaTime
        public float experimentEndingTime = 0;
        public float virtualWayDistance = 0;//virtual way length of waypoints
        public long executeBeginningTime = 0;//the real world time when experiment trial starts
        public long executeEndingTime = 0;////the real world time when experiment trial ends

        //For passive haptics
        public float positionError;//distance between the final physical avatar position and the  physical target position
        public float angleError;//angle between the final physical avatar direction and the  physical target direction

        // Reset Sample Parameters
        public List<float> virtualDistancesTravelledBetweenResets = new List<float>(); // this will be measured also from beginning to first reset, and from last reset to end (?)
        public float virtualDistanceTravelledSinceLastReset;
        public List<float> timeElapsedBetweenResets = new List<float>(); // this will be measured also from beginning to first reset, and from last reset to end (?)
        public float timeOfLastReset = 0;

        // Sampling Paramers: These parameters are first read per frame/value update and stored in their buffer, and then 1/samplingFrequency time goes by, the values in the buffer will be averaged and logged to the list
        // The buffer variables for gains will be multiplied by time and at sampling time divided by time since last sample to get a proper average (since the functions aren't guaranteed to be called every frame)
        // Actually we can do this for all parameters just for true weighted average!
        public List<Vector2> userRealPositionSamples = new List<Vector2>();
        public List<Vector2> userRealPositionSamplesBuffer = new List<Vector2>();
        public List<Vector2> userVirtualPositionSamples = new List<Vector2>();
        public List<Vector2> userVirtualPositionSamplesBuffer = new List<Vector2>();
        public List<float> translationGainSamples = new List<float>();
        public List<float> translationGainSamplesBuffer = new List<float>();
        public List<float> injectedTranslationSamples = new List<float>();
        public List<float> injectedTranslationSamplesBuffer = new List<float>();
        public List<float> rotationGainSamples = new List<float>();
        public List<float> rotationGainSamplesBuffer = new List<float>();
        // NOTE: IN THE FUTURE, WE MIGHT WANT TO LOG THE INJECTED VALUES DIVIDED BY TIME, SO IT'S MORE CONSISTENT AND NO DEPENDENT ON THE FRAMERATE
        public List<float> injectedRotationFromRotationGainSamples = new List<float>();
        public List<float> injectedRotationFromRotationGainSamplesBuffer = new List<float>();
        public List<float> curvatureGainSamples = new List<float>();
        public List<float> curvatureGainSamplesBuffer = new List<float>();
        public List<float> injectedRotationFromCurvatureGainSamples = new List<float>();
        public List<float> injectedRotationFromCurvatureGainSamplesBuffer = new List<float>();
        public List<float> injectedRotationSamples = new List<float>();
        public List<float> injectedRotationSamplesBuffer = new List<float>();
        public List<float> distanceToNearestBoundarySamples = new List<float>();
        public List<float> distanceToNearestBoundarySamplesBuffer = new List<float>();
        public List<float> distanceToCenterSamples = new List<float>();
        public List<float> distanceToCenterSamplesBuffer = new List<float>();        
        public AvatarStatistics(float currentTime) {
            Initialize(currentTime);
        }
        public void Initialize(float currentTime) {
            sumOfInjectedTranslation = 0;
            sumOfInjectedRotationFromRotationGain = 0;
            sumOfInjectedRotationFromCurvatureGain = 0;
            maxTranslationGain = float.MinValue;
            minTranslationGain = float.MaxValue;
            maxRotationGain = float.MinValue;
            minRotationGain = float.MaxValue;
            maxCurvatureGain = float.MinValue;
            minCurvatureGain = float.MaxValue;            

            resetCount = 0;
            sumOfVirtualDistanceTravelled = 0;
            sumOfRealDistanceTravelled = 0;
            experimentBeginningTime = currentTime;
            executeBeginningTime = DateTime.Now.Ticks;

            positionError = 0;
            angleError = 0;

            virtualDistancesTravelledBetweenResets = new List<float>();
            virtualDistanceTravelledSinceLastReset = 0;
            timeElapsedBetweenResets = new List<float>();
            timeOfLastReset = currentTime; // Technically a reset didn't happen here but we want to remember this time point                                                        

            userRealPositionSamples = new List<Vector2>();
            userRealPositionSamplesBuffer = new List<Vector2>();
            userVirtualPositionSamples = new List<Vector2>();
            userVirtualPositionSamplesBuffer = new List<Vector2>();
            translationGainSamples = new List<float>();
            translationGainSamplesBuffer = new List<float>();
            injectedTranslationSamples = new List<float>();
            injectedTranslationSamplesBuffer = new List<float>();
            rotationGainSamples = new List<float>();
            rotationGainSamplesBuffer = new List<float>();
            injectedRotationFromRotationGainSamples = new List<float>();
            injectedRotationFromRotationGainSamplesBuffer = new List<float>();
            curvatureGainSamples = new List<float>();
            curvatureGainSamplesBuffer = new List<float>();
            injectedRotationFromCurvatureGainSamples = new List<float>();
            injectedRotationFromCurvatureGainSamplesBuffer = new List<float>();
            injectedRotationSamples = new List<float>();
            injectedRotationSamplesBuffer = new List<float>();
            distanceToNearestBoundarySamples = new List<float>();
            distanceToNearestBoundarySamplesBuffer = new List<float>();
            distanceToCenterSamples = new List<float>();
            distanceToCenterSamplesBuffer = new List<float>();

            // --------------------------- VR-Dodge-Study ----------------------------------
            maxYawVirtual = 0;
            maxYawReal = 0;
            maxDistanceToCenter = 0; 
            realPosAtMaxDistanceToCenter = Vector3.zero;
            virtPosAtMaxDistanceToCenter = Vector3.zero;
            realRotAtMaxRealYaw = Vector3.zero;
            realRotAtMaxVirtYaw = Vector3.zero;
            virtRotAtMaxRealYaw = Vector3.zero;
            virtRotAtMaxVirtYaw = Vector3.zero;
            realPosAtMaxRealYaw = Vector3.zero;
            realPosAtMaxVirtYaw = Vector3.zero;
            virtPosAtMaxRealYaw = Vector3.zero;
            virtPosAtMaxVirtYaw = Vector3.zero;
            collisionDetected = false;
            addVirtRotAccumulationSamples = new List<float>();
            realRotAccumulationSamples = new List<float>();
            realRotSamples = new List<Vector3>();
            virtRotSamples = new List<Vector3>();
            addVirtRotAccumulationSamplesBuffer = new List<float>();
            realRotAccumulationSamplesBuffer = new List<float>();
            realRotSamplesBuffer = new List<Vector3>();
            virtRotSamplesBuffer = new List<Vector3>();
        // --------------------------- VR-Dodge-Study ----------------------------------
    }
    }
    //store statistic data of every avatar
    public List<AvatarStatistics> avatarStatistics;

    List<float> samplingIntervals = new List<float>();
    float lastSamplingTime = 0;

    // ---------------------- VR-Dodge Study ---------------------------------
    private float positiveYawAccumulation = 0;
    private float negativeYawAccumulation = 0;
    private float currentYawAccumulation = 0;
    private float virtualRightRotation = 0;
    private float virtualLeftRotation = 0;
    // ---------------------- VR-Dodge Study ---------------------------------

    //the logging state
    enum LoggingState { not_started, logging, paused, complete };
    LoggingState state = LoggingState.not_started;

    public void InitializeAllValues()
    {        
        avatarStatistics = new List<AvatarStatistics>();
        for (int i = 0; i < globalConfiguration.redirectedAvatars.Count; i++) {
            avatarStatistics.Add(new AvatarStatistics(globalConfiguration.GetTime()));
        }
        samplingIntervals = new List<float>();
        lastSamplingTime = globalConfiguration.GetTime();
    }

    //initialize experiment Results
    public void InitializeExperimentResults()
    {
        experimentResults = new List<ResultOfTrial>();
    }

    // IMPORTANT! The gathering of values has to be in LateUpdate to make sure the "Time.deltaTime" that's used by the gain sampling functions is the same ones that are considered when dividing by time elapsed 
    // that we do when gatherin the samples from buffers. Otherwise it can be that we get the buffers from a deltaTime, then the same deltaTime is used later to calculate a buffer value for a gain, and then 
    // later on the division won't be fair!
    public void UpdateStats()
    {
        if (state == LoggingState.logging)
        {
            // Average and Log Sampled Values If It's Time To
            UpdateFrameBasedValues();
            //Debug.Log("generalManager.GetTime():" + generalManager.GetTime() + ";" + "lastSamplingTime:" + lastSamplingTime);
            if (globalConfiguration.GetTime() - lastSamplingTime > (1 / samplingFrequency))
            {
                GenerateSamplesFromBufferValuesAndClearBuffers();
                samplingIntervals.Add(globalConfiguration.GetTime() - lastSamplingTime);
                lastSamplingTime = globalConfiguration.GetTime();
            }
        }
    }

    public void BeginLogging()
    {
        if (state == LoggingState.not_started || state == LoggingState.complete)
        {
            //Debug.Log("BeginLogging");
            state = LoggingState.logging;
            InitializeAllValues();
        }
    }

    // IF YOU PAUSE, YOU HAVE TO BE CAREFUL ABOUT TIME ELAPSED BETWEEN PAUSES!
    public void PauseLogging()
    {
        if (state == LoggingState.logging)
        {
            state = LoggingState.paused;
        }
    }

    public void ResumeLogging()
    {
        if (state == LoggingState.paused)
        {
            state = LoggingState.logging;
        }
    }

    // Experiment Descriptors are given and 
    public void EndLogging()
    {
        if (state == LoggingState.logging)
        {
            Event_Experiment_Ended();
            state = LoggingState.complete;
        }
    }

    // Experiment Descriptors are given and we add the logged data as a full experiment result bundle
    // data of every avatar
    public ResultOfTrial GetExperimentResultForSummaryStatistics(int endState, List<Dictionary<string, string>> experimentDescriptors)
    {
        var experimentResults = new List<Dictionary<string, string>>();
        for (int i = 0; i < globalConfiguration.redirectedAvatars.Count; i++) {
            var experimentDescriptor = experimentDescriptors[i];
            var er = new Dictionary<string, string>(experimentDescriptor);
            experimentResults.Add(er);

            var us = avatarStatistics[i];
            
            er["reset_count"] = us.resetCount.ToString();
            er["virtual_way_distance"] = us.virtualWayDistance.ToString();

            er["virtual_distance_between_resets_average"] = GetAverage(us.virtualDistancesTravelledBetweenResets).ToString();
            er["time_elapsed_between_resets_average"] = GetAverage(us.timeElapsedBetweenResets).ToString();            

            er["sum_injected_translation(IN METERS)"] = us.sumOfInjectedTranslation.ToString();
            er["sum_injected_rotation_g_r(IN DEGREES)"] = us.sumOfInjectedRotationFromRotationGain.ToString();
            er["sum_injected_rotation_g_c(IN DEGREES)"] = us.sumOfInjectedRotationFromCurvatureGain.ToString();
            er["sum_real_distance_travelled(IN METERS)"] = us.sumOfRealDistanceTravelled.ToString();
            er["sum_virtual_distance_travelled(IN METERS)"] = us.sumOfVirtualDistanceTravelled.ToString();
            er["min_g_t"] = us.minTranslationGain < float.MaxValue ? us.minTranslationGain.ToString() : "N/A";
            er["max_g_t"] = us.maxTranslationGain > float.MinValue ? us.maxTranslationGain.ToString() : "N/A";
            er["min_g_r"] = us.minRotationGain < float.MaxValue ? us.minRotationGain.ToString() : "N/A";
            er["max_g_r"] = us.maxRotationGain > float.MinValue ? us.maxRotationGain.ToString() : "N/A";
            er["min_g_c"] = us.minCurvatureGain < float.MaxValue ? us.minCurvatureGain.ToString() : "N/A";
            er["max_g_c"] = us.maxCurvatureGain > float.MinValue ? us.maxCurvatureGain.ToString() : "N/A";
            er["g_t_average"] = GetAverageOfAbsoluteValues(us.translationGainSamples).ToString();
            er["injected_translation_average"] = GetAverage(us.injectedTranslationSamples).ToString();
            er["g_r_average"] = GetAverageOfAbsoluteValues(us.rotationGainSamples).ToString();
            er["injected_rotation_from_rotation_gain_average"] = GetAverage(us.injectedRotationFromRotationGainSamples).ToString();
            er["g_c_average"] = GetAverageOfAbsoluteValues(us.curvatureGainSamples).ToString();
            er["injected_rotation_from_curvature_gain_average"] = GetAverage(us.injectedRotationFromCurvatureGainSamples).ToString();
            er["injected_rotation_average"] = GetAverage(us.injectedRotationSamples).ToString();

            er["real_position_average"] = GetAverage(us.userRealPositionSamples).ToString();
            er["virtual_position_average"] = GetAverage(us.userVirtualPositionSamples).ToString();
            er["distance_to_boundary_average"] = GetAverage(us.distanceToNearestBoundarySamples).ToString();
            er["distance_to_center_average"] = GetAverage(us.distanceToCenterSamples).ToString();

            er["experiment_duration"] = (us.experimentEndingTime - us.experimentBeginningTime).ToString();
            er["execute_duration"] = (((double)us.executeEndingTime - us.executeBeginningTime)/ 1e7).ToString();
            er["average_sampling_interval"] = GetAverage(samplingIntervals).ToString();

            // -------------------------- VR- Dodge-Study -------------------------------------------
            er["max_yaw_real"] = us.maxYawReal.ToString();
            er["max_yaw_virt"] = us.maxYawVirtual.ToString();
            er["max_distance_to_center"] = us.maxDistanceToCenter.ToString();
            er["real_pos_at_max_dist"] = us.realPosAtMaxDistanceToCenter.ToString();
            er["virt_pos_at_max_dist"] = us.virtPosAtMaxDistanceToCenter.ToString();
            er["real_rot_at_real_yaw"] = us.realRotAtMaxRealYaw.ToString();
            er["virt_rot_at_real_yaw"] = us.virtRotAtMaxRealYaw.ToString();
            er["real_rot_at_virt_yaw"] = us.realRotAtMaxVirtYaw.ToString();
            er["virt_rot_at_virt_yaw"] = us.virtRotAtMaxVirtYaw.ToString();
            er["real_pos_at_real_yaw"] = us.realPosAtMaxRealYaw.ToString();
            er["virt_pos_at_real_yaw"] = us.virtPosAtMaxRealYaw.ToString();
            er["real_pos_at_virt_yaw"] = us.realPosAtMaxVirtYaw.ToString();
            er["virt_pos_at_virt_yaw"] = us.virtPosAtMaxVirtYaw.ToString();
            // --------------------------------------------------------------------------------------

            //if passive haptics mode
            if (globalConfiguration.passiveHaptics) {
                er["positionError"] = us.positionError.ToString();
                er["angleError"] = us.angleError.ToString();
            }
        }
        return new ResultOfTrial(endState, experimentResults);
    }

    public void GetExperimentResultsForSampledVariables(out List<Dictionary<string, List<float>>> oneDimensionalSamples, out List<Dictionary<string, List<Vector2>>> twoDimensionalSamples, out List<Dictionary<string, List<Vector3>>> threeDimensionalSamples)
    {
        //store every avatar's info
        oneDimensionalSamples = new List<Dictionary<string, List<float>>>();
        twoDimensionalSamples = new List<Dictionary<string, List<Vector2>>>();
        threeDimensionalSamples = new List<Dictionary<string, List<Vector3>>>();
        for (int i = 0; i < avatarStatistics.Count; i++) {
            var us = avatarStatistics[i];
            var oneDimensionalSample = new Dictionary<string, List<float>>();
            var twoDimensionalSample = new Dictionary<string, List<Vector2>>();
            var threeDimensionalSample = new Dictionary<string, List<Vector3>>();

            oneDimensionalSample.Add("distances_to_boundary", us.distanceToNearestBoundarySamples);
            //oneDimensionalSample.Add("normalized_distances_to_boundary", GetTrackingAreaNormalizedList(us.distanceToNearestBoundarySamples));
            oneDimensionalSample.Add("distances_to_center", us.distanceToCenterSamples);
            //oneDimensionalSample.Add("normalized_distances_to_center", GetTrackingAreaNormalizedList(distanceToCenterSamples));
            //oneDimensionalSample.Add("g_t", us.translationGainSamples);
           // oneDimensionalSample.Add("injected_translations", us.injectedTranslationSamples);
            oneDimensionalSample.Add("g_r", us.rotationGainSamples);
            oneDimensionalSample.Add("injected_rotations_from_rotation_gain", us.injectedRotationFromRotationGainSamples);
            //oneDimensionalSample.Add("g_c", us.curvatureGainSamples);
            //oneDimensionalSample.Add("injected_rotations_from_curvature_gain", us.injectedRotationFromCurvatureGainSamples);
            oneDimensionalSample.Add("injected_rotations", us.injectedRotationSamples);
            oneDimensionalSample.Add("virtual_distances_between_resets", us.virtualDistancesTravelledBetweenResets);
            oneDimensionalSample.Add("time_elapsed_between_resets", us.timeElapsedBetweenResets);
            oneDimensionalSample.Add("sampling_intervals", samplingIntervals);

            twoDimensionalSample.Add("user_real_positions", us.userRealPositionSamples);
            twoDimensionalSample.Add("user_virtual_positions", us.userVirtualPositionSamples);

            //---------------- VR-Dodge-Study ----------------------------------
            oneDimensionalSample.Add("real_rotation_accumulation", us.realRotAccumulationSamples);
            oneDimensionalSample.Add("additional_virtual_rotation_accumulation", us.addVirtRotAccumulationSamples);

            threeDimensionalSample.Add("real_rotation", us.realRotSamples);
            threeDimensionalSample.Add("virtual_rotation", us.virtRotSamples);

            threeDimensionalSamples.Add(threeDimensionalSample);
            //--------------- VR Dodge-Study ----------------------------------

            oneDimensionalSamples.Add(oneDimensionalSample);
            twoDimensionalSamples.Add(twoDimensionalSample);
        }
    }

    public void Event_User_Translated(AvatarStatistics us, Vector3 deltaPosition2D)
    {
        if (state == LoggingState.logging)
        {
            us.sumOfVirtualDistanceTravelled += deltaPosition2D.magnitude;
            us.sumOfRealDistanceTravelled += deltaPosition2D.magnitude;
            us.virtualDistanceTravelledSinceLastReset += deltaPosition2D.magnitude;
        }
    }

    // this is called after Event_Rotation_Gain, which is kinda unintuitive
    public void Event_User_Rotated(AvatarStatistics us, float realRotDelta, Vector3 currDirReal, Vector3 currDirVirt, Vector3 currPosReal, Vector3 currPosVirt)    // ------------------------------------- changed. This method was previously empty. -----------------------------------------------------
    {
        if (state == LoggingState.logging)
        {
            //Debug.Log("Real Rotaion delta: " + realRotDelta);
            if(realRotDelta > 0)
            {
                //if(realRotDelta / Time.deltaTime >= 1.5f)
                    negativeYawAccumulation = 0; //reset negative rotation accumulation

                positiveYawAccumulation += realRotDelta;
                currentYawAccumulation = positiveYawAccumulation;
            }
            else
            {
                //if (Mathf.Abs(realRotDelta) / Time.deltaTime >= 1.5f)
                    positiveYawAccumulation = 0; // reset positive rot accumulation

                negativeYawAccumulation += realRotDelta;
                currentYawAccumulation = negativeYawAccumulation;
                
            }

            us.realRotAccumulationSamplesBuffer.Add(currentYawAccumulation);

            if (Mathf.Abs(currentYawAccumulation) > Mathf.Abs(us.maxYawReal))
            {
                us.maxYawReal = currentYawAccumulation;
                us.realPosAtMaxRealYaw = currPosReal;
                us.virtPosAtMaxRealYaw = currPosVirt;
                us.realRotAtMaxRealYaw = currDirReal;
                us.virtRotAtMaxRealYaw = currDirVirt;
            }
        }
    }

    public void Event_Translation_Gain(int userId, float g_t, Vector3 translationApplied)
    {
        if (state == LoggingState.logging)
        {
            var us = avatarStatistics[userId];
            us.sumOfInjectedTranslation += translationApplied.magnitude;
            us.maxTranslationGain = Mathf.Max(us.maxTranslationGain, g_t);
            us.minTranslationGain = Mathf.Min(us.minTranslationGain, g_t);
            us.sumOfVirtualDistanceTravelled += Mathf.Sign(g_t) * translationApplied.magnitude; // if gain is positive, redirection reference moves with the user, thus increasing the virtual displacement, and if negative, decreases
            us.virtualDistanceTravelledSinceLastReset += Mathf.Sign(g_t) * translationApplied.magnitude;
            //translationGainSamplesBuffer.Add(Mathf.Abs(g_t) * redirectionManager.userMovementManager.lastDeltaTime);
            // The proper way is using redirectionManager.userMovementManager.lastDeltaTime which is the true time the gain was applied for, but this causes problems when we have a long frame and then a short frame
            // But we'll artificially use this current delta time instead!
            //translationGainSamplesBuffer.Add(g_t * redirectionManager.userMovementManager.lastDeltaTime);
            //print("Translation Gain: " + g_t + "\tInterval: " + redirectionManager.getDeltaTime());
            us.translationGainSamplesBuffer.Add(g_t * globalConfiguration.GetDeltaTime());
            //injectedTranslationSamplesBuffer.Add(translationApplied.magnitude * redirectionManager.userMovementManager.lastDeltaTime);
            us.injectedTranslationSamplesBuffer.Add(translationApplied.magnitude * globalConfiguration.GetDeltaTime());
        }
    }

    public void Event_Translation_Gain_Reorientation(float g_t, Vector3 translationApplied)
    {
        if (state == LoggingState.logging)
        {
            throw new System.NotImplementedException();

            //sumOfInjectedTranslation += translationApplied.magnitude;
            //maxTranslationGain = Mathf.Max(maxTranslationGain, g_t);
            //minTranslationGain = Mathf.Min(minTranslationGain, g_t);
            //sumOfVirtualDistanceTravelled += Mathf.Sign(g_t) * translationApplied.magnitude; // if gain is positive, redirection reference moves with the user, thus increasing the virtual displacement, and if negative, decreases
            //virtualDistanceTravelledSinceLastReset += Mathf.Sign(g_t) * translationApplied.magnitude;
            ////translationGainSamplesBuffer.Add(Mathf.Abs(g_t) * redirectionManager.userMovementManager.lastDeltaTime);
            //// The proper way is using redirectionManager.userMovementManager.lastDeltaTime which is the true time the gain was applied for, but this causes problems when we have a long frame and then a short frame
            //// But we'll artificially use this current delta time instead!
            ////translationGainSamplesBuffer.Add(g_t * redirectionManager.userMovementManager.lastDeltaTime);
            ////print("Translation Gain: " + g_t + "\tInterval: " + redirectionManager.getDeltaTime());
            //translationGainSamplesBuffer.Add(g_t * redirectionManager.getDeltaTime());
            ////injectedTranslationSamplesBuffer.Add(translationApplied.magnitude * redirectionManager.userMovementManager.lastDeltaTime);
            //injectedTranslationSamplesBuffer.Add(translationApplied.magnitude * redirectionManager.getDeltaTime());
        }
    }

    public void Event_Rotation_Gain(int userId, float rotationGainFactor, float rotationApplied, Vector3 currDirReal, Vector3 currDirVirt, Vector3 currPosReal, Vector3 currPosVirt)
    {
        if (state == LoggingState.logging)
        {
            var us = avatarStatistics[userId];
            us.sumOfInjectedRotationFromRotationGain += Mathf.Abs(rotationApplied);
            us.maxRotationGain = Mathf.Max(us.maxRotationGain, rotationGainFactor);
            us.minRotationGain = Mathf.Min(us.minRotationGain, rotationGainFactor);
            //rotationGainSamplesBuffer.Add(Mathf.Abs(g_r) * redirectionManager.userMovementManager.lastDeltaTime);
            // The proper way is using redirectionManager.userMovementManager.lastDeltaTime which is the true time the gain was applied for, but this causes problems when we have a long frame and then a short frame
            // But we'll artificially use this current delta time instead!
            //rotationGainSamplesBuffer.Add(g_r * redirectionManager.userMovementManager.lastDeltaTime);
            us.rotationGainSamplesBuffer.Add(rotationGainFactor * globalConfiguration.GetDeltaTime());
            //injectedRotationFromRotationGainSamplesBuffer.Add(Mathf.Abs(rotationApplied) * redirectionManager.userMovementManager.lastDeltaTime);
            us.injectedRotationFromRotationGainSamplesBuffer.Add(rotationApplied * globalConfiguration.GetDeltaTime());                     // ---------- removed Mathf.Abs to show direction change. Since curvature gain is disabled, the absolute rotation gain will be shown in injectedRotationSamplesBuffer anyway
            //injectedRotationSamplesBuffer.Add(Mathf.Abs(rotationApplied) * redirectionManager.userMovementManager.lastDeltaTime);
            us.injectedRotationSamplesBuffer.Add(Mathf.Abs(rotationApplied) * globalConfiguration.GetDeltaTime());


            // ------------------------------- VR-Dodge-Study -----------------------------------------------------
            float virtYawAccumulation;
            if(currentYawAccumulation > 0)  // if user is rotating right
            {
                virtualLeftRotation = 0;    // reset left rotation accumulation
                virtualRightRotation += rotationApplied;
                us.addVirtRotAccumulationSamplesBuffer.Add(virtualRightRotation);

                virtYawAccumulation = currentYawAccumulation + virtualRightRotation;
            }
            else
            {
                virtualRightRotation = 0;
                virtualLeftRotation += rotationApplied;
                us.addVirtRotAccumulationSamplesBuffer.Add(virtualLeftRotation);

                virtYawAccumulation = currentYawAccumulation + virtualLeftRotation;
            }


            if (Mathf.Abs(virtYawAccumulation) > Mathf.Abs(us.maxYawVirtual)) // if this is the maximum virtual rotation
            {
                us.maxYawVirtual = virtYawAccumulation;
                us.realPosAtMaxVirtYaw = currPosReal;
                us.virtPosAtMaxVirtYaw = currPosVirt;
                us.realRotAtMaxVirtYaw = currDirReal;
                us.virtRotAtMaxVirtYaw = currDirVirt;
            }
            // ------------------------------- VR-Dodge-Study -----------------------------------------------------
        }
    }

    public void Event_Curvature_Gain(int userId, float g_c, float rotationApplied)
    {
        if (state == LoggingState.logging)
        {
            var us = avatarStatistics[userId];
            us.sumOfInjectedRotationFromCurvatureGain += Mathf.Abs(rotationApplied);
            //if (float.IsNaN(rotationApplied))
            //    Debug.LogError("rotationApplied nan!");
            us.maxCurvatureGain = Mathf.Max(us.maxCurvatureGain, g_c * Mathf.Deg2Rad);
            us.minCurvatureGain = Mathf.Min(us.minCurvatureGain, g_c * Mathf.Deg2Rad);

            us.curvatureGainSamplesBuffer.Add(g_c * globalConfiguration.GetDeltaTime());

            us.injectedRotationFromCurvatureGainSamplesBuffer.Add(Mathf.Abs(rotationApplied) * globalConfiguration.GetDeltaTime());

            us.injectedRotationSamplesBuffer.Add(Mathf.Abs(rotationApplied) * globalConfiguration.GetDeltaTime());
        }
    }

    public void Event_Reset_Triggered(int userId)
    {
        if (state == LoggingState.logging)
        {
            var us = avatarStatistics[userId];
            us.resetCount++;
            us.virtualDistancesTravelledBetweenResets.Add(us.virtualDistanceTravelledSinceLastReset);
            us.virtualDistanceTravelledSinceLastReset = 0;
            us.timeElapsedBetweenResets.Add(globalConfiguration.GetTime() - us.timeOfLastReset);
            us.timeOfLastReset = globalConfiguration.GetTime(); // Technically a reset didn't happen here but we want to remember this time point
        }
    }

    public void Event_Update_PassiveHaptics_Results(int userId, float positionError, float angleError) {
        if (state == LoggingState.logging)
        {
            var us = avatarStatistics[userId];
            us.positionError=positionError;
            us.angleError = angleError;
        }
    }

    //record real and virtual paths for logging
    void UpdateFrameBasedValues()
    {
        for (int i = 0; i < avatarStatistics.Count; i++) {
            var userGameobject = globalConfiguration.redirectedAvatars[i];
            var rm = userGameobject.GetComponent<RedirectionManager>();
            var us = avatarStatistics[i];
            // Now we are letting the developer determine the movement manually in update, and we pull the info from redirector
            Event_User_Rotated(us, rm.deltaDir, rm.currDirReal, rm.currDir, rm.currPosReal, rm.currPos);
            Event_User_Translated(us, Utilities.FlattenedPos2D(rm.deltaPos));

            us.userRealPositionSamplesBuffer.Add(Utilities.FlattenedPos2D(rm.currPosReal));
            us.userVirtualPositionSamplesBuffer.Add(Utilities.FlattenedPos2D(rm.currPos));
            us.distanceToNearestBoundarySamplesBuffer.Add(Utilities.GetNearestDistToObstacleAndTrackingSpace(globalConfiguration.obstaclePolygons, globalConfiguration.trackingSpacePoints, rm.currPosReal));
            us.distanceToCenterSamplesBuffer.Add(rm.currPosReal.magnitude);

            // ------------------ VR-Dodge-Study ------------------------------------
            if(us.maxDistanceToCenter < rm.currPosReal.magnitude)
            {
                us.maxDistanceToCenter = rm.currPosReal.magnitude;
                us.realPosAtMaxDistanceToCenter = rm.currPosReal;
                us.virtPosAtMaxDistanceToCenter = rm.currPos;
            }
            
            us.realRotSamplesBuffer.Add(rm.currDirReal);
            us.virtRotSamplesBuffer.Add(rm.currDir);
            // ----------------------------------------------------------------------
        }
    }

    //get sample from buffer and clear buffer
    void GenerateSamplesFromBufferValuesAndClearBuffers()
    {
        //Debug.Log("GenerateSamplesFromBufferValuesAndClearBuffers");
        foreach (var us in avatarStatistics) {
            GetSampleFromBuffer(ref us.userRealPositionSamples, ref us.userRealPositionSamplesBuffer);
            GetSampleFromBuffer(ref us.userVirtualPositionSamples, ref us.userVirtualPositionSamplesBuffer);
            GetSampleFromBuffer(ref us.translationGainSamples, ref us.translationGainSamplesBuffer);
            GetSampleFromBuffer(ref us.injectedTranslationSamples, ref us.injectedTranslationSamplesBuffer);
            GetSampleFromBuffer(ref us.rotationGainSamples, ref us.rotationGainSamplesBuffer);
            GetSampleFromBuffer(ref us.injectedRotationFromRotationGainSamples, ref us.injectedRotationFromRotationGainSamplesBuffer);
            GetSampleFromBuffer(ref us.curvatureGainSamples, ref us.curvatureGainSamplesBuffer);
            GetSampleFromBuffer(ref us.injectedRotationFromCurvatureGainSamples, ref us.injectedRotationFromCurvatureGainSamplesBuffer);
            GetSampleFromBuffer(ref us.injectedRotationSamples, ref us.injectedRotationSamplesBuffer);
            GetSampleFromBuffer(ref us.distanceToNearestBoundarySamples, ref us.distanceToNearestBoundarySamplesBuffer);
            GetSampleFromBuffer(ref us.distanceToCenterSamples, ref us.distanceToCenterSamplesBuffer);

            // ------------------- VR-Dodge-Study ------------------------------------
            GetSampleFromBuffer(ref us.realRotAccumulationSamples, ref us.realRotAccumulationSamplesBuffer);
            GetSampleFromBuffer(ref us.addVirtRotAccumulationSamples, ref us.addVirtRotAccumulationSamplesBuffer);
            GetSampleFromBuffer(ref us.realRotSamples, ref us.realRotSamplesBuffer);
            GetSampleFromBuffer(ref us.virtRotSamples, ref us.virtRotSamplesBuffer);
            // -----------------------------------------------------------------------
        }
    }

    void GetSampleFromBuffer(ref List<float> samples, ref List<float> buffer, bool verbose = false)
    {
        float sampleValue = 0;
        foreach (float bufferValue in buffer)
        {
            sampleValue += bufferValue;
        }
        //samples.Add(sampleValue / (redirectionManager.GetTime() - lastSamplingTime));
        // OPTIONALLY WE CAN NOT LOG ANYTHING AT ALL IN THIS CASE!
        samples.Add(buffer.Count != 0 ? sampleValue / buffer.Count : 0);
        if (verbose)
        {
            print("sampleValue: " + sampleValue);
            print("samplingInterval: " + (globalConfiguration.GetTime() - lastSamplingTime));
        }
        buffer.Clear();
    }

    void GetSampleFromBuffer(ref List<Vector2> samples, ref List<Vector2> buffer)
    {
        Vector2 sampleValue = Vector2.zero;
        foreach (Vector2 bufferValue in buffer)
        {
            sampleValue += bufferValue;
        }
        //samples.Add(sampleValue / (redirectionManager.GetTime() - lastSamplingTime));
        samples.Add(sampleValue / buffer.Count);
        buffer.Clear();
    }

    void GetSampleFromBuffer(ref List<Vector3> samples, ref List<Vector3> buffer)               // ------------------------------------------ added --------------------------
    {
        Vector3 sampleValue = Vector3.zero;
        foreach (Vector3 bufferValue in buffer)
        {
            sampleValue += bufferValue;
        }
        //samples.Add(sampleValue / (redirectionManager.GetTime() - lastSamplingTime));
        samples.Add(sampleValue / buffer.Count);
        buffer.Clear();
    }

    void Event_Experiment_Ended()
    {
        for (int i = 0; i < avatarStatistics.Count; i++) {
            var us = avatarStatistics[i];
            us.virtualDistancesTravelledBetweenResets.Add(us.virtualDistanceTravelledSinceLastReset);
            us.timeElapsedBetweenResets.Add(globalConfiguration.GetTime() - us.timeOfLastReset);
            us.experimentEndingTime = globalConfiguration.GetTime();
            us.executeEndingTime = DateTime.Now.Ticks;

            us.virtualWayDistance = 0;
            var waypoints = globalConfiguration.redirectedAvatars[i].GetComponent<MovementManager>().waypoints;
            for (int j = 0; j < waypoints.Count - 1; j++)
                us.virtualWayDistance += (waypoints[j + 1] - waypoints[j]).magnitude;
        }
        //Debug.Log("********redirectionManager.GetTime():"+ redirectionManager.GetTime());
    }

    Vector2 GetTimeWeightedSampleAverage(List<Vector2> sampleArray, List<float> sampleDurationArray)
    {
        Vector2 valueSum = Vector2.zero;
        float timeSum = 0;
        for (int i = 0; i < sampleArray.Count; i++)
        {
            valueSum += sampleArray[i] * sampleDurationArray[i];
            timeSum += sampleDurationArray[i];
        }
        return sampleArray.Count != 0 ? valueSum / timeSum : Vector2.zero;
    }

    float GetAverage(List<float> array)
    {
        float sum = 0;
        foreach (float value in array)
        {
            sum += value;
        }
        return array.Count != 0 ? sum / array.Count : 0;
    }

    float GetAverageOfAbsoluteValues(List<float> array)
    {
        float sum = 0;
        foreach (float value in array)
        {
            sum += Mathf.Abs(value);
        }
        return array.Count != 0 ? sum / array.Count : 0;
    }

    Vector2 GetAverage(List<Vector2> array)
    {
        Vector2 sum = Vector2.zero;
        foreach (Vector2 value in array)
        {
            sum += value;
        }
        return sum / array.Count;
    }

    // We're not providing a time-based version of this at this time
    float GetMedian(List<float> array)
    {
        if (array.Count == 0)
        {
            Debug.LogError("Empty Array");
            return 0;
        }
        List<float> sortedArray = array.OrderBy(item => item).ToList<float>();
        if (sortedArray.Count % 2 == 1)
            return sortedArray[(int)(0.5f * sortedArray.Count)];
        else
            return 0.5f * (sortedArray[(int)(0.5f * sortedArray.Count)] + sortedArray[(int)(0.5f * sortedArray.Count) - 1]);
    }

    ////////////// LOGGING TO FILE
    [HideInInspector]
    public string RESULT_DIRECTORY;
    [HideInInspector]
    public string RESULT_WITH_TIME_DIRECTORY;//save results with time mark
    [HideInInspector]
    public string SUMMARY_STATISTICS_DIRECTORY;
    [HideInInspector]
    public string SAMPLED_METRICS_DIRECTORY;
    [HideInInspector]
    public string GRAPH_DERECTORY;
    [HideInInspector]
    public string TMP_DERECTORY;
    [HideInInspector]
    public string VIDEO_DERECTORY;
    [HideInInspector]
    public string SCREENSHOTS_DERECTORY;
    

    XmlWriter xmlWriter;
    [HideInInspector]
    public string SUMMARY_STATISTICS_XML_FILENAME = "SimulationResults";
    const string XML_ROOT = "Experiments";
    const string XML_ELEMENT = "Experiment";

    StreamWriter csvWriter;
    
    public Texture2D texRealPathGraph;//tex for simulation real path logging
    public Texture2D texVirtualPathGraph;//tex for simulation virtual path logging

    void Awake()
    {
        RESULT_DIRECTORY = "/ExperimentResults/";
        RESULT_WITH_TIME_DIRECTORY = Utilities.GetTimeStringForFileName() + "/";
        SUMMARY_STATISTICS_DIRECTORY = "Summary Statistics/";
        SAMPLED_METRICS_DIRECTORY = "Sampled Metrics/";
        GRAPH_DERECTORY = "Graphs/";
        TMP_DERECTORY = "Tmp/";
        VIDEO_DERECTORY = "Videos/";
        SCREENSHOTS_DERECTORY = "Screenshots/";

        texRealPathGraph = new Texture2D(imageResolution, imageResolution);
        texVirtualPathGraph = new Texture2D(imageResolution, imageResolution);

        globalConfiguration = GetComponent<GlobalConfiguration>();
        RESULT_DIRECTORY = Utilities.GetProjectPath() + RESULT_DIRECTORY;
        RESULT_WITH_TIME_DIRECTORY = RESULT_DIRECTORY + RESULT_WITH_TIME_DIRECTORY;
        SUMMARY_STATISTICS_DIRECTORY = RESULT_WITH_TIME_DIRECTORY + SUMMARY_STATISTICS_DIRECTORY;
        SAMPLED_METRICS_DIRECTORY = RESULT_WITH_TIME_DIRECTORY + SAMPLED_METRICS_DIRECTORY;
        GRAPH_DERECTORY = RESULT_WITH_TIME_DIRECTORY + GRAPH_DERECTORY;
        TMP_DERECTORY = RESULT_WITH_TIME_DIRECTORY + TMP_DERECTORY;
        VIDEO_DERECTORY = RESULT_WITH_TIME_DIRECTORY + VIDEO_DERECTORY;
        SCREENSHOTS_DERECTORY = RESULT_WITH_TIME_DIRECTORY + SCREENSHOTS_DERECTORY;

        //create relative directories        
        Utilities.CreateDirectoryIfNeeded(RESULT_DIRECTORY);
        Utilities.CreateDirectoryIfNeeded(RESULT_WITH_TIME_DIRECTORY);
        Utilities.CreateDirectoryIfNeeded(SUMMARY_STATISTICS_DIRECTORY);
        Utilities.CreateDirectoryIfNeeded(SAMPLED_METRICS_DIRECTORY);
        Utilities.CreateDirectoryIfNeeded(GRAPH_DERECTORY);
        Utilities.CreateDirectoryIfNeeded(TMP_DERECTORY);
        Utilities.CreateDirectoryIfNeeded(VIDEO_DERECTORY);
        Utilities.CreateDirectoryIfNeeded(SCREENSHOTS_DERECTORY);        

        obstacleColor = globalConfiguration.obstacleColor;
    }
    public string Get_TMP_DERECTORY() {
        return TMP_DERECTORY;
    }
    public string Get_VIDEO_DERECTORY() {
        return VIDEO_DERECTORY;
    }
    // Writes all summary statistics for a batch of experiments
    public void LogExperimentSummaryStatisticsResults(List<Dictionary<string, string>> experimentResults)
    {
        // Settings
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;
        settings.IndentChars = ("\t");
        settings.CloseOutput = true;

        // Create XML File
        xmlWriter = XmlWriter.Create(SUMMARY_STATISTICS_DIRECTORY + SUMMARY_STATISTICS_XML_FILENAME + ".xml", settings);
        xmlWriter.Settings.Indent = true;
        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement(XML_ROOT);

        // HACK: If there's only one element, Excel won't show the lablels, so we're duplicating for now
        if (experimentResults.Count == 1)
        {
            experimentResults.Add(experimentResults[0]);
        }

        foreach (Dictionary<string, string> experimentResult in experimentResults)
        {
            xmlWriter.WriteStartElement(XML_ELEMENT);
            foreach (KeyValuePair<string, string> entry in experimentResult)
            {
                xmlWriter.WriteElementString(entry.Key, entry.Value);
            }
            xmlWriter.WriteEndElement();
        }

        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();
        xmlWriter.Flush();
        xmlWriter.Close();
    }

    //export results of every trial to excels
    public void LogExperimentSummaryStatisticsResultsSCSV(string resultDir, string resultFileName)
    {
        Debug.Log("Writing summary statistics");
        if (!Directory.Exists(resultDir))
            Directory.CreateDirectory(resultDir);
        csvWriter = new StreamWriter(resultDir + resultFileName + ".csv");        
        csvWriter.WriteLine("sep=;");

        // Set up the headers
        csvWriter.Write("TrialIteration;");
        csvWriter.Write("EndState;");
        csvWriter.Write("experiment_start_time;");
        foreach (string header in experimentResults[0].result[0].Keys)
        {
            csvWriter.Write(header + ";");
        }
        csvWriter.WriteLine();

        for (int experimentResultID = 0; experimentResultID < experimentResults.Count; experimentResultID++) {
            var experimentDescriptor = experimentResults[experimentResultID].result;
            var endState = experimentResults[experimentResultID].EndStateToString();

            if (experimentDescriptor.Count > 0)
            {
                // Write Values            
                foreach (var experimentResultPerUser in experimentDescriptor)
                {
                    csvWriter.Write(experimentResultID + ";");
                    csvWriter.Write(endState + ";");
                    csvWriter.Write(globalConfiguration.startTimeOfProgram + ";");
                    foreach (string value in experimentResultPerUser.Values)
                    {
                        csvWriter.Write(value + ";");
                    }
                    csvWriter.WriteLine();
                }
            }
        }
        csvWriter.Flush();
        csvWriter.Close();
    }

    public void LogExperimentRealPathPictures(int experimentIteration, Color graphColor, ExperimentSetup experimentSetup, AvatarStatistics avatarStatistics)
    {
        //set background to white
        Utilities.SetTextureToSingleColor(texRealPathGraph, Color.clear);

        var realPosList = Dodge_Study.ExperimentManager.Instance.offSetGraphPoints(avatarStatistics.userRealPositionSamples);
        var deltaWeight = (1 - pathStartAlpha) / realPosList.Count;

        for (int i = 0; i < realPosList.Count - 1; i++)
        {
            var w = (pathStartAlpha + deltaWeight * i);
            var startColor = graphColor;
            startColor.a = w;
            var endColor = graphColor;
            endColor.a = (w + deltaWeight);
            Utilities.DrawLine(texRealPathGraph, realPosList[i], realPosList[i + 1], realSideLength, pathThickness, startColor, endColor);
        }

        texRealPathGraph.Apply();

        string rdirString = Dodge_Study.ExperimentManager.Instance.useRedirection ? "rdir" : "noRdir";
        string path = GRAPH_DERECTORY + "Real/" + string.Format("Iteration{0}_ID{1}_real_{2}.png", experimentIteration, experimentSetup.trialData.TrialID, rdirString);
        //Export as png file
        Utilities.ExportTexture2dToPng(path, texRealPathGraph);
    }

    //save path, boundaries, obstacles as a image
    public void LogExperimentVirtualPathPictures(int experimentIteration, Color graphColor, ExperimentSetup experimentSetup, AvatarStatistics avatarStatistics)
    {
        //set background to transparent
        Utilities.SetTextureToSingleColor(texVirtualPathGraph, Color.clear);

        var virtualPosList = Dodge_Study.ExperimentManager.Instance.offSetGraphPoints(avatarStatistics.userVirtualPositionSamples);
        var deltaWeight = (1 - pathStartAlpha) / virtualPosList.Count;

        for (int i = 0; i < virtualPosList.Count - 1; i++)
        {
            var w = (pathStartAlpha + deltaWeight * i);
            var startColor = graphColor;
            startColor.a = w;
            var endColor = graphColor;
            endColor.a = (w + deltaWeight);
            Utilities.DrawLine(texVirtualPathGraph, virtualPosList[i], virtualPosList[i + 1], realSideLength, pathThickness, startColor, endColor);
        }

        texVirtualPathGraph.Apply();

        string rdirString = Dodge_Study.ExperimentManager.Instance.useRedirection ? "rdir" : "noRdir";
        string path = GRAPH_DERECTORY + "Virtual/" + string.Format("Iteration{0}_ID{1}_virt_{2}.png", experimentIteration, experimentSetup.trialData.TrialID, rdirString);
        //Export as png file
        Utilities.ExportTexture2dToPng(path, texVirtualPathGraph);
    }

    #region old log methods
    //save path, boundaries, obstacles as a image
    public void LogExperimentRealPathPictures(int experimentSetupId) {
        var experimentSetups = globalConfiguration.experimentSetups;

        var experimentSetup = experimentSetups[experimentSetupId];

        //set background to white
        Utilities.SetTextureToSingleColor(texRealPathGraph, backgroundColor);
        
        var trackingSpacePoints = experimentSetup.trackingSpacePoints;
        var obstaclePolygons = experimentSetup.obstaclePolygons;
        var trackingBoundary = Dodge_Study.PositioningManager.Instance.boundaryPoints;                                                                                 // ---------------- added -------------- //
        if (trackingBoundary.Count == 0)
            TrackingSpaceGenerator.GenerateRectangleTrackingSpace(0,out trackingBoundary, out obstaclePolygons, out _, 5f, 5f);
        for (int i = 0; i < trackingBoundary.Count; i++)
            Utilities.DrawLine(texRealPathGraph, trackingBoundary[i], trackingBoundary[(i + 1) % trackingBoundary.Count], realSideLength, borderThickness, boundaryColor);                   // ---------------- added -------------- //
        for (int i = 0; i < trackingSpacePoints.Count; i++)
            Utilities.DrawLine(texRealPathGraph, trackingSpacePoints[i], trackingSpacePoints[(i + 1) % trackingSpacePoints.Count], realSideLength, borderThickness, trackingSpaceColor);        
        foreach (var obstaclePolygon in obstaclePolygons)
            Utilities.DrawPolygon(texRealPathGraph, obstaclePolygon, realSideLength, borderThickness, obstacleColor);
        //for (int i = 0; i < obstaclePolygon.Count; i++)
        //    Utilities.DrawLine(tex, obstaclePolygon[i], obstaclePolygon[(i + 1) % obstaclePolygon.Count], sideLength, borderThickness, obstacleColor);
        Debug.Log(avatarStatistics.Count + "avatarstatistics");
        for (int uId = 0; uId < avatarStatistics.Count; uId++) {
            //var virtualPathColor = globalConfiguration.avatarColors[uId];
            var realPosList = avatarStatistics[uId].userRealPositionSamples;
            var deltaWeight = (1 - pathStartAlpha) / realPosList.Count;
            
            
            for (int i = 0; i < realPosList.Count - 1; i++) {
                var w = (pathStartAlpha + deltaWeight * i);
                //Debug.Log("realPosList[i]:" + realPosList[i].ToString("f3"));
                Utilities.DrawLine(texRealPathGraph, realPosList[i], realPosList[i + 1], realSideLength, pathThickness, w * realPathColor + (1 - w) * backgroundColor, (w + deltaWeight) * realPathColor + (1 - w - deltaWeight) * backgroundColor);
            }
        }
        
        texRealPathGraph.Apply();
        
        //Export as png file
        Utilities.ExportTexture2dToPng(GRAPH_DERECTORY + string.Format("Iteration{0}_ID{1}_realPath.png", experimentSetupId, experimentSetup.trialData.TrialID), texRealPathGraph);

    }

    //save path, boundaries, obstacles as a image
    public void LogExperimentVirtualPathPictures(int experimentSetupId)
    {
        var experimentSetups = globalConfiguration.experimentSetups;

        var experimentSetup = experimentSetups[experimentSetupId];

        //set background to transparent
        Utilities.SetTextureToSingleColor(texVirtualPathGraph, Color.clear);
        
        var trackingSpacePoints = experimentSetup.trackingSpacePoints;
        var obstaclePolygons = experimentSetup.obstaclePolygons;
        var trackingBoundary = Dodge_Study.PositioningManager.Instance.boundaryPoints;                                                                                        // ---------------- added -------------- //
        if (trackingBoundary.Count == 0)
            TrackingSpaceGenerator.GenerateRectangleTrackingSpace(0, out trackingBoundary, out obstaclePolygons, out _, 5f, 5f);
        for (int i = 0; i < trackingBoundary.Count; i++)
            Utilities.DrawLine(texVirtualPathGraph, trackingBoundary[i], trackingBoundary[(i + 1) % trackingBoundary.Count], virtualSideLength, borderThickness, Color.black);                   // ---------------- added -------------- //
        /*for (int i = 0; i < trackingSpacePoints.Count; i++)
            Utilities.DrawLine(texVirtualPathGraph, trackingSpacePoints[i], trackingSpacePoints[(i + 1) % trackingSpacePoints.Count], realSideLength, borderThickness, trackingSpaceColor);
        foreach (var obstaclePolygon in obstaclePolygons)
            Utilities.DrawPolygon(texVirtualPathGraph, obstaclePolygon, realSideLength, borderThickness, obstacleColor);*/
        //for (int i = 0; i < obstaclePolygon.Count; i++)
        //    Utilities.DrawLine(tex, obstaclePolygon[i], obstaclePolygon[(i + 1) % obstaclePolygon.Count], sideLength, borderThickness, obstacleColor);
        for (int uId = 0; uId < avatarStatistics.Count; uId++)
        {
            int colorIndex = PlayerPrefs.GetInt("virtualPathColorIndex", -1);

            if(colorIndex < 0 || colorIndex >= virtualPathColors.Length - 1)
            {
                colorIndex = 0;
                PlayerPrefs.SetInt("virtualPathColorIndex", 0);
            }  
            else
            {
                colorIndex++;
                PlayerPrefs.SetInt("virtualPathColorIndex", colorIndex);
            }

            var color = virtualPathColors[colorIndex];
            var virtualPosList = avatarStatistics[uId].userVirtualPositionSamples;
            var deltaWeight = (1 - pathStartAlpha) / virtualPosList.Count;

            
            for (int i = 0; i < virtualPosList.Count - 1; i++)
            {
                var w = (pathStartAlpha + deltaWeight * i);
                var startColor = color;
                startColor.a = w;
                var endColor = color;
                endColor.a = (w + deltaWeight);
                //Debug.Log("realPosList[i]:" + realPosList[i].ToString("f3"));
                Utilities.DrawLine(texVirtualPathGraph, virtualPosList[i], virtualPosList[i + 1], virtualSideLength, pathThickness, startColor, endColor);//w * color + (1 - w) * backgroundColor, (w + deltaWeight) * color + (1 - w - deltaWeight) * backgroundColor);
            }
        }

        texVirtualPathGraph.Apply();

        //Export as png file
        Utilities.ExportTexture2dToPng(GRAPH_DERECTORY + string.Format("Iteration{0}_ID{1}_virtualPath.png", experimentSetupId, experimentSetup.trialData.TrialID), texVirtualPathGraph);
    }

    #endregion

    public void LogOneDimensionalExperimentSamples(string experimentSamplesDirectory, string measuredMetric, List<float> values)
    {        
        Utilities.CreateDirectoryIfNeeded(experimentSamplesDirectory);
        csvWriter = new StreamWriter(experimentSamplesDirectory + measuredMetric + ".csv");
        foreach (float value in values)
        {
            csvWriter.WriteLine(value.ToString().Replace(",", "."));
        }
        csvWriter.Flush();
        csvWriter.Close();
    }

    public void LogTwoDimensionalExperimentSamples(string experimentSamplesDirectory, string measuredMetric, List<Vector2> values)
    {                
        Utilities.CreateDirectoryIfNeeded(experimentSamplesDirectory);
        csvWriter = new StreamWriter(experimentSamplesDirectory + measuredMetric + ".csv");
        foreach (Vector2 value in values)
        {
            csvWriter.WriteLine(value.x.ToString().Replace(",", ".") + ", " + value.y.ToString().Replace(",", "."));
        }
        csvWriter.Flush();
        csvWriter.Close();
    }

    //save results to local
    public void LogAllExperimentSamples(string experimentDecriptorString, List<Dictionary<string, List<float>>> oneDimensionalSamplesMaps, List<Dictionary<string, List<Vector2>>> twoDimensionalSamplesMaps)
    {
        globalConfiguration.GetResultDirAndFileName(SAMPLED_METRICS_DIRECTORY, out string resultDir, out string fileName);
        Utilities.CreateDirectoryIfNeeded(resultDir);
        resultDir += fileName + "/";
        Utilities.CreateDirectoryIfNeeded(resultDir);
        string experimentSamplesDirectory = resultDir + experimentDecriptorString + "/";
        Utilities.CreateDirectoryIfNeeded(experimentSamplesDirectory);
        Debug.Log("experimentSamplesDirectory: " + experimentSamplesDirectory);

        for (var i = 0; i < oneDimensionalSamplesMaps.Count; i++)
        {
            var oneDimensionalSamplesMap = oneDimensionalSamplesMaps[i];
            foreach (KeyValuePair<string, List<float>> oneDimensionalSamples in oneDimensionalSamplesMap)
            {
                LogOneDimensionalExperimentSamples(experimentSamplesDirectory + "userId_" + i + "/", oneDimensionalSamples.Key, oneDimensionalSamples.Value);
            }
        }

        for (var i = 0; i < twoDimensionalSamplesMaps.Count; i++)
        {
            var twoDimensionalSamplesMap = twoDimensionalSamplesMaps[i];
            foreach (KeyValuePair<string, List<Vector2>> twoDimensionalSamples in twoDimensionalSamplesMap)
            {
                LogTwoDimensionalExperimentSamples(experimentSamplesDirectory + "userId_" + i + "/", twoDimensionalSamples.Key , twoDimensionalSamples.Value);
            }
        }
    }

    public bool IfResetCountExceedLimit(int id) {
        return avatarStatistics[id].resetCount > MaxResetCount;
    }
}
