using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static StatisticsLogger;

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
        public bool useRedirection;

        [HideInInspector] public List<ExperimentSetup> setups = new List<ExperimentSetup>();

        [SerializeField] private StatisticsLogger logger;
        [SerializeField] private Color backgroundColor;
        [SerializeField] private Color realPathColor;
        [SerializeField] private Color trackingSpaceColor;
        [SerializeField] private Color boundaryColor;
        [SerializeField] private Color[] virtualPathColors;

        private string[][] AllSampleCSVLines;
        private string sampleDirectory, graphDirectory;

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
            graphDirectory = Utilities.GetProjectPath() + "/ExperimentResults/" + Utilities.GetTimeStringForFileName() + "/" + "Graphs/";
            Utilities.CreateDirectoryIfNeeded(sampleDirectory);

            AllSampleCSVLines = new string[untestedConditions.Count][];
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

        public void AddSetup(ExperimentSetup setup)
        {
            setups.Add(setup);
        }

        public void LogExperimentRealPathPictures(int experimentSetupId)
        {
            var experimentSetup = setups[experimentSetupId];
            //set background to white
            Utilities.SetTextureToSingleColor(logger.texRealPathGraph, backgroundColor);

            var trackingSpacePoints = experimentSetup.trackingSpacePoints;
            var obstaclePolygons = experimentSetup.obstaclePolygons;
            var trackingBoundary = Dodge_Study.PositioningManager.Instance.boundaryPoints;                                                                                 // ---------------- added -------------- //
            if (trackingBoundary.Count == 0)
                TrackingSpaceGenerator.GenerateRectangleTrackingSpace(0, out trackingBoundary, out obstaclePolygons, out _, 5f, 5f);
            for (int i = 0; i < trackingBoundary.Count; i++)
                Utilities.DrawLine(logger.texRealPathGraph, trackingBoundary[i], trackingBoundary[(i + 1) % trackingBoundary.Count], logger.realSideLength, logger.borderThickness, boundaryColor);                   // ---------------- added -------------- //
            for (int i = 0; i < trackingSpacePoints.Count; i++)
                Utilities.DrawLine(logger.texRealPathGraph, trackingSpacePoints[i], trackingSpacePoints[(i + 1) % trackingSpacePoints.Count], logger.realSideLength, logger.borderThickness, trackingSpaceColor); // ---------------- added -------------- //
            foreach (var obstaclePolygon in obstaclePolygons)
                Utilities.DrawPolygon(logger.texRealPathGraph, obstaclePolygon, logger.realSideLength, logger.borderThickness, boundaryColor);
            //for (int i = 0; i < obstaclePolygon.Count; i++)
            //    Utilities.DrawLine(tex, obstaclePolygon[i], obstaclePolygon[(i + 1) % obstaclePolygon.Count], sideLength, borderThickness, obstacleColor);
            for (int uId = 0; uId < logger.avatarStatistics.Count; uId++)
            {
                //var virtualPathColor = globalConfiguration.avatarColors[uId];
                var realPosList = logger.avatarStatistics[uId].userRealPositionSamples;
                var deltaWeight = (1 - logger.pathStartAlpha) / realPosList.Count;


                for (int i = 0; i < realPosList.Count - 1; i++)
                {
                    var w = (logger.pathStartAlpha + deltaWeight * i);
                    //Debug.Log("realPosList[i]:" + realPosList[i].ToString("f3"));
                    Utilities.DrawLine(logger.texRealPathGraph, realPosList[i], realPosList[i + 1], logger.realSideLength, logger.pathThickness, w * realPathColor + (1 - w) * backgroundColor, (w + deltaWeight) * realPathColor + (1 - w - deltaWeight) * backgroundColor);
                }
            }

            logger.texRealPathGraph.Apply();

            //Export as png file
            Utilities.ExportTexture2dToPng(graphDirectory + string.Format("Iteration{0}_ID{1}_realPath.png", experimentSetupId, experimentSetup.trialData.TrialID), logger.texRealPathGraph);

        }
    }
}