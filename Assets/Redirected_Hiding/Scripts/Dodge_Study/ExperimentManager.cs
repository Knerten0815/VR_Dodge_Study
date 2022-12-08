using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;
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

        [HideInInspector] public List<AvatarStatistics> savedStats = new List<AvatarStatistics>();

        [SerializeField] private Color boundaryColor, radiusColor;
        [SerializeField] private Color[] graphColors;

        //private string[][] AllSampleCSVLines = new string[untestedConditions.Count][];
        private string sampleDirectory, graphDirectory, timeString;
        private System.Random rnd;

        private static ExperimentManager _instance;

        public static ExperimentManager Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
                Destroy(this);
            else
                _instance = this;
            
            setupAllConditions();

            timeString = Utilities.GetTimeStringForFileName();
            sampleDirectory = Utilities.GetProjectPath() + "/ExperimentResults/" + timeString + "/" + "Sampled Metrics/";
            graphDirectory = Utilities.GetProjectPath() + "/ExperimentResults/" + timeString + "/" + "Graphs/";
            Utilities.CreateDirectoryIfNeeded(sampleDirectory);
            Utilities.CreateDirectoryIfNeeded(graphDirectory);
            Utilities.CreateDirectoryIfNeeded(graphDirectory + "Real/");
            Utilities.CreateDirectoryIfNeeded(graphDirectory + "Virtual/");

            rnd = new System.Random();
        }
        private void Start()
        {
            config.statisticsLogger.InitializeAllValues();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!trialIsRunning)
                {
                    StartTrial();
                }
            }
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
                
                config.experimentSetups[config.experimentIterator].trialData = currentCondition;
                
                if (currentCondition.CollisionDetected)
                    config.EndExperiment(2);
                else
                    config.EndExperiment(0);

                currentCondition = null;
            }
        }

        private TrialData pickRandomCondition()
        {
            currentCondition = untestedConditions[rnd.Next(0, untestedConditions.Count)];
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
        public void LogAllSamples(int trialIteration, List<Dictionary<string, List<float>>> oneDimensionalSamplesMaps, List<Dictionary<string, List<Vector2>>> twoDimensionalSamplesMaps, List<Dictionary<string, List<Vector3>>> threeDimensionalSamplesMaps)
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
            foreach (string header in threeDimensionalSamplesMaps[0].Keys)
            {
                csvWriter.Write(header + ";");
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

                foreach (KeyValuePair<string, List<Vector3>> keyListPair in threeDimensionalSamplesMaps[0])
                {
                    if (i < keyListPair.Value.Count)
                        csvWriter.Write(keyListPair.Value[i].x.ToString().Replace(",", ".") + ", " + keyListPair.Value[i].y.ToString().Replace(",", ".") + ", " + keyListPair.Value[i].z.ToString().Replace(",", ".") + ";");
                    else
                        csvWriter.Write(";");
                }
            }
        }        

        public void SaveSamplesForGraphExport(AvatarStatistics stats)
        {
            savedStats.Add(stats);
        }

        public Color pickGraphColors(out Color virtualPathColor)
        {
            int colorIndex = PlayerPrefs.GetInt("graphColorIndex", -1);

            if (colorIndex < 0 || colorIndex >= graphColors.Length - 1)
            {
                colorIndex = 0;
                PlayerPrefs.SetInt("graphColorIndex", 0);
            }
            else
            {
                colorIndex++;
                PlayerPrefs.SetInt("graphColorIndex", colorIndex);
            }

            int virtualID = graphColors.Length - 1 - colorIndex;

            virtualPathColor = graphColors[virtualID];
            return graphColors[colorIndex];
        }

        public List<Vector2> offSetGraphPoints(List<Vector2> graphPoints)
        {
            for (int i = 0; i < graphPoints.Count; i++)
                graphPoints[i] -= PositioningManager.Instance.boundaryCenter;

            return graphPoints;
        }

        public void LogBoundaryPicture()
        {
            int resolution = config.statisticsLogger.imageResolution;
            int sideLength = config.statisticsLogger.realSideLength;
            int borderThickness = config.statisticsLogger.borderThickness;
            Texture2D texBoundary = new Texture2D(resolution, resolution);

            //set background to white
            Utilities.SetTextureToSingleColor(texBoundary, Color.clear);

            // draw boundary
            List<Vector2> boundaryPoints = new List<Vector2>(offSetGraphPoints(PositioningManager.Instance.boundaryPoints));

            if (boundaryPoints.Count == 0)
                TrackingSpaceGenerator.GenerateRectangleTrackingSpace(0, out boundaryPoints, out _, out _, 5f, 5f);

            for (int i = 0; i < boundaryPoints.Count; i++)
            {
                Utilities.DrawLine(texBoundary, boundaryPoints[i], boundaryPoints[(i + 1) % boundaryPoints.Count], sideLength, borderThickness, boundaryColor);
            }

            // draw circle with 1m radius around visual center of boundary
            List<Vector2> circlePoints = TrackingSpaceGenerator.GeneratePolygonTrackingSpacePoints(100, 1);
            for (int i = 0; i < circlePoints.Count; i++)
            {                
                Utilities.DrawLine(texBoundary, circlePoints[i], circlePoints[(i + 1) % circlePoints.Count], sideLength, borderThickness, radiusColor);
            }                

            texBoundary.Apply();

            string filePath = graphDirectory + "Tracking_Space_" + timeString + ".png";
            //Export as png file
            Utilities.ExportTexture2dToPng(filePath, texBoundary);
        }

        #region unused sample saving code
        /*
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
                for (int j = 0; j < AllSampleCSVLines[i].Length; j++)
                {
                    csvWriter.WriteLine(AllSampleCSVLines[i][j]);
                }
            }
        }
        */
        #endregion#
    }
}