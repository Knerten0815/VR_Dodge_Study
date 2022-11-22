using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private string[][] AllSampleCSVLines;
        private string sampleDirectory;

        private static ExperimentManager _instance;
        public static ExperimentManager Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
                Destroy(this);
            else
                _instance = this;
            
            setupAllConditions();
            
            sampleDirectory = Utilities.GetProjectPath() + "/ExperimentResults/" + Utilities.GetTimeStringForFileName() + "/" + "Sampled Metrics/";
            Utilities.CreateDirectoryIfNeeded(sampleDirectory);

            AllSampleCSVLines = new string[untestedConditions.Count][];

            //trackingSpaceBoundaries = TrackingSpaceGenerator.GetTrackingSpace(out trackingSpaceCenter);  <---------- this will completly freeze Unity
        }
        private void Start()
        {
            config.statisticsLogger.InitializeAllValues();
        }

        [ContextMenu("Start Trial")]
        public void StartTrial()
        {
            if (!trialIsRunning)
            {
                config.PlayerIsReadyMenue();
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
        
        //save results to local
        public void LogAllSamples(int trialIteration, List<Dictionary<string, List<float>>> oneDimensionalSamplesMaps, List<Dictionary<string, List<Vector2>>> twoDimensionalSamplesMaps)
        {
            StreamWriter csvWriter = new StreamWriter(sampleDirectory + "trialIteration_" + trialIteration + "_samples.csv");
            int lineCount = oneDimensionalSamplesMaps[0].First().Value.Count;

            // Set up the headers
            foreach (string header in twoDimensionalSamplesMaps[0].Keys)
            {
                csvWriter.Write(header + ";");
            }
            foreach (string header in oneDimensionalSamplesMaps[0].Keys)
            {
                csvWriter.Write( header + ";");
            }

            // Write values
            for (int i = 1; i < lineCount; i++)
            {
                csvWriter.WriteLine();
                foreach (KeyValuePair<string, List<Vector2>> keyListPair in twoDimensionalSamplesMaps[0])
                {
                    if (i < keyListPair.Value.Count)
                        csvWriter.Write(keyListPair.Value[i].x.ToString().Replace(",", ".") + ", " + keyListPair.Value[i].y.ToString().Replace(",", ".") + ";");
                    else
                        csvWriter.Write(";");
                }

                foreach (KeyValuePair<string, List<float>> keyListPair in oneDimensionalSamplesMaps[0])
                {
                    if (i < keyListPair.Value.Count)
                        csvWriter.Write(keyListPair.Value[i] + ";");
                    else
                        csvWriter.Write(";");
                }
            }
        }

        public void SaveSamplesForLogging(int trialIteration, List<Dictionary<string, List<float>>> oneDimensionalSamplesMaps, List<Dictionary<string, List<Vector2>>> twoDimensionalSamplesMaps)
        {
            int lineCount = oneDimensionalSamplesMaps[0].First().Value.Count + 1;
            AllSampleCSVLines[trialIteration] = new string[lineCount];

            // Set up the headers
            foreach (string header in twoDimensionalSamplesMaps[0].Keys)
            {
                AllSampleCSVLines[trialIteration][0] = header + ";";
            }
            foreach (string header in oneDimensionalSamplesMaps[0].Keys)
            {
                AllSampleCSVLines[trialIteration][0] += header + ";";
            }

            for (int i = 1; i < lineCount; i++)
            {
                foreach (KeyValuePair<string, List<Vector2>> keyListPair in twoDimensionalSamplesMaps[0])
                {
                    if (i < keyListPair.Value.Count)
                        AllSampleCSVLines[trialIteration][i] = keyListPair.Value[i].x.ToString().Replace(",", ".") + ", " + keyListPair.Value[i].y.ToString().Replace(",", ".") + ";";
                    else
                        AllSampleCSVLines[trialIteration][i] = ";";
                }

                foreach (KeyValuePair<string, List<float>> keyListPair in oneDimensionalSamplesMaps[0])
                {
                    if (i < keyListPair.Value.Count)
                        AllSampleCSVLines[trialIteration][i] += keyListPair.Value[i] + ";";
                    else
                        AllSampleCSVLines[trialIteration][i] += ";";
                }
            }
        }


        public void LogAllSavedData()
        {
            StreamWriter csvWriter;

            for (int i = 0; i < AllSampleCSVLines.Length; i++)
            {
                csvWriter = new StreamWriter(sampleDirectory + "trialIteration_" + i + "_samples.csv");
                for(int j = 0; j < AllSampleCSVLines[i].Length; j++)
                {
                    csvWriter.WriteLine(AllSampleCSVLines[i][j]);
                }
            }
        }
    }
}