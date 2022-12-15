using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dodge_Study
{
    public class TrialObjectSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject[] formPrefabs = new GameObject[4];
        [SerializeField] private GameObject boundaryCenter, objectGoal;
        [SerializeField] private float extraDistance = 2;
        public float distanceToUser = 20;
        public float objectHeight = 1.65f;


        private TrialObject currentCollisionObject = null;
        private Transform[] startPositions;
        private GameObject[] loadInstances;

        private void Awake()
        {
            startPositions = new Transform[TrialData.angles.Length];

            for(int i = 0; i < TrialData.angles.Length; i++)
            {
                startPositions[i] = Instantiate(new GameObject().transform);//formPrefabs[2]).transform;
                startPositions[i].name = "Start Angle " + TrialData.angles[i];

                // set angle (meaning position)
                Vector3 objPos = Quaternion.AngleAxis(TrialData.angles[i], Vector3.up) * Vector3.forward * distanceToUser;
                objPos.y = objectHeight;
                startPositions[i].position = objPos;
                startPositions[i].Rotate(new Vector3(0, TrialData.angles[i] + 180, 0));
                startPositions[i].SetParent(boundaryCenter.transform);
            }
        }

        /// <summary>
        /// load every prefab once, to avoid lag when spawning.
        /// </summary>
        private IEnumerator Start()
        {
            loadInstances = new GameObject[formPrefabs.Length];
            for (int i = 0; i < loadInstances.Length; i++)
            {
                loadInstances[i] = Instantiate(formPrefabs[i]);
            }

            yield return null;

            foreach (GameObject go in loadInstances)
                Destroy(go);
        }

        public void spawnObject()
        {
            TrialData ecpC = ExperimentManager.Instance.currentCondition;

            // set form
            GameObject go = Instantiate(formPrefabs[ecpC.FormIndex]);
            // set angle (meaning position)
            Vector3 objPos = startPositions[ecpC.AngleIndex].position;
            objPos.y = 1.65f;               // --------------------------------------------------------------------------------------------------TODO: set Object height to head height of user
            go.transform.position = objPos;
            go.transform.rotation = startPositions[ecpC.AngleIndex].rotation;
            // set size
            go.transform.localScale *= ecpC.GetSize();

            //go.transform.SetParent(objectGoal.transform);
            go.name = "TrialObject" + ecpC.TrialID;

            // set speed
            currentCollisionObject = go.AddComponent<TrialObject>();
            currentCollisionObject.Init(ecpC.GetSpeed(), ecpC.GetForm() == TrialData.Form.Car);//, ecpC.GetAngle(), this);

            objectGoal.transform.position = PositioningManager.Instance.centerTrans.position + (currentCollisionObject.transform.forward * extraDistance);
            objectGoal.transform.rotation = currentCollisionObject.transform.rotation;
        }
    }
}
