using System.Collections.Generic;
using UnityEngine;

namespace Dodge_Study
{
    public class DebugVisuals : MonoBehaviour
    {
        public GameObject trackingSpaceRoot;

        private List<GameObject> debugVisualsGOList = new List<GameObject>();

        public GameObject instantiateSphere2D(Vector2 position, bool isPartOfTrackingSpace)
        {
            return instantiateSphere2D(position, isPartOfTrackingSpace, (Vector3.one * 0.1f), Color.yellow);
        }

        public GameObject instantiateSphere2D(Vector2 position, bool isPartOfTrackingSpace, Vector3 size, Color colour)
        {
            GameObject theSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            theSphere.GetComponent<MeshRenderer>().material.color = colour;

            theSphere.transform.position = new Vector3(position.x, 0, position.y);
            theSphere.transform.localScale = size;

            if (isPartOfTrackingSpace)
                theSphere.transform.SetParent(trackingSpaceRoot.transform);

            return theSphere;
        }

        public GameObject instantiateSphere3D(Vector3 position, bool isPartOfTrackingSpace)
        {
            return instantiateSphere3D(position, isPartOfTrackingSpace, (Vector3.one * 0.1f), Color.yellow);
        }

        public GameObject instantiateSphere3D(Vector3 position, bool isPartOfTrackingSpace, Vector3 size, Color colour)
        {
            GameObject theSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            
            theSphere.GetComponent<MeshRenderer>().material.color = colour;
            theSphere.transform.position = position;
            theSphere.transform.localScale = size;

            if (isPartOfTrackingSpace)
                theSphere.transform.SetParent(trackingSpaceRoot.transform);

            return theSphere;
        }

        #region debug visuals
        public void drawTrackingBoundaries()
        {
            
            // draw trackingSpaceBoundaries
            if (PositioningManager.Instance.boundaryPoints.Count == 0)
            {
                // draw 5x5 m tracking boundary if not headset is used
                TrackingSpaceGenerator.GenerateRectangleTrackingSpace(0, out PositioningManager.Instance.boundaryPoints, out _, out _, 5f, 5f);
                debugVisualsGOList.Add(DrawLine(PositioningManager.Instance.boundaryPoints[0], PositioningManager.Instance.boundaryPoints[1]));
                debugVisualsGOList.Add(DrawLine(PositioningManager.Instance.boundaryPoints[1], PositioningManager.Instance.boundaryPoints[2]));
                debugVisualsGOList.Add(DrawLine(PositioningManager.Instance.boundaryPoints[2], PositioningManager.Instance.boundaryPoints[3]));
                debugVisualsGOList.Add(DrawLine(PositioningManager.Instance.boundaryPoints[3], PositioningManager.Instance.boundaryPoints[0]));
            }
            else
            {
                foreach (var point in PositioningManager.Instance.boundaryPoints)
                    debugVisualsGOList.Add(instantiateSphere2D(point, true));

                // add center
                debugVisualsGOList.Add(instantiateSphere2D(PositioningManager.Instance.boundaryCenter, true, Vector3.one * 0.2f, Color.red));
            }
        }

        public void DestroyDebugVisuals()
        {
            foreach(GameObject go in debugVisualsGOList)
                GameObject.Destroy(go);
        }

        /// <summary>
        /// taken from https://answers.unity.com/questions/8338/how-to-draw-a-line-using-script.html
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public GameObject DrawLine(Vector3 start, Vector3 end, float width = 0.05f)
        {
            GameObject line = new GameObject();
            line.transform.position = start;
            line.AddComponent<LineRenderer>();
            LineRenderer lr = line.GetComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Standard"));
            lr.startWidth = width;
            lr.endWidth = width;
            lr.startColor = Color.red;
            lr.endColor = Color.red;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);

            debugVisualsGOList.Add(line);
            return line;
        }

        public GameObject DrawLine(Vector2 start, Vector2 end, float width = 0.05f, bool isPartOfTrackingSpace = true)
        {
            Vector3 startVec3 = new Vector3(start.x, 0, start.y);
            Vector3 endVec3 = new Vector3(end.x, 0, end.y);

            GameObject line = new GameObject();
            //line.transform.position = startVec3;
            line.AddComponent<LineRenderer>();
            LineRenderer lr = line.GetComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Standard"));
            lr.material.color = Color.red;
            lr.startWidth = width;
            lr.endWidth = width;
            lr.SetPosition(0, startVec3);
            lr.SetPosition(1, endVec3);

            if (isPartOfTrackingSpace)
                line.transform.parent = trackingSpaceRoot.transform;

            line.GetComponent<LineRenderer>().useWorldSpace = false;

            return line;
        }
            #endregion
    }
}
