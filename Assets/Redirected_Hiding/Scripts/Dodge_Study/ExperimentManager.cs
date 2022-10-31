using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dodge_Study
{
    public class ExperimentManager : MonoBehaviour
    {
        [SerializeField] GlobalConfiguration config;
        [SerializeField] TrialObjectSpawner spawner;
        public List<TrialData> untestedConditions = new List<TrialData>();
        private List<TrialData> testedConditions = new List<TrialData>();
        public TrialData currentCondition = null;
        public bool trialIsRunning;

        private static ExperimentManager _instance;
        public static ExperimentManager Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
                Destroy(this);
            else
                _instance = this;

            setupAllConditions();
            //trackingSpaceBoundaries = TrackingSpaceGenerator.GetTrackingSpace(out trackingSpaceCenter);  <---------- this will completly freeze Unity
        }

        [ContextMenu("Start Trial")]
        public void StartTrial()
        {
            if (!trialIsRunning)
            {
                trialIsRunning = true;
                pickRandomCondition();
                spawner.spawnObject();
            }
        }

        [ContextMenu("End Trial")]
        public void EndTrial()
        {
            if (trialIsRunning)
            {
                trialIsRunning = false;
                if (currentCondition.CollisionDetected)
                    config.EndExperiment(2);
                else
                    config.EndExperiment(0);

                currentCondition = null;
            }
        }

        private TrialData pickRandomCondition()
        {
            currentCondition = untestedConditions[UnityEngine.Random.Range(0, untestedConditions.Count)];
            untestedConditions.Remove(currentCondition);
            testedConditions.Add(currentCondition);
            return currentCondition;
        }

        [ContextMenu("Setup conditions")]
        private void setupAllConditions()
        {
            testedConditions = new List<TrialData>();

            for (int formIndex = 0; formIndex < Enum.GetNames(typeof(TrialData.Form)).Length; formIndex++)
            {
                for (int sizeIndex = 0; sizeIndex < TrialData.sizes.Length; sizeIndex++)
                {
                    for (int speedIndex = 0; speedIndex < TrialData.speeds.Length; speedIndex++)
                    {
                        for (int angleIndex = 0; angleIndex < TrialData.angles.Length; angleIndex++)
                        {
                            untestedConditions.Add(new TrialData(formIndex, sizeIndex, speedIndex, angleIndex));
                        }
                    }
                }
            }

            Debug.Log("Setup experiment with " + untestedConditions.Count + " conditions.");
        }
    }
}