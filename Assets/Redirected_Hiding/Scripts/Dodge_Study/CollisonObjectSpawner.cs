using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dodge_Study
{
    public class CollisonObjectSpawner : MonoBehaviour
    {
        public bool testIsRunning;
        [SerializeField] private GameObject[] formPrefabs = new GameObject[4];
        public float distanceToUser = 20;

        private List<ExperimentCondition> untestedConditions = new List<ExperimentCondition>();
        private List<ExperimentCondition> testedConditions = new List<ExperimentCondition>();
        private ExperimentCondition currentCondition = null;
        private CollisionObject currentCollisionObject = null;
        [SerializeField] private bool userIsPositioned;        

        private void Awake()
        {
            setupAllConditions();
        }

        // Update is called once per frame
        private void Update()
        {
            if(userIsPositioned && !testIsRunning)
            {
                spawnObject();
            }
        }

        private void spawnObject()
        {
            currentCondition = pickRandomCondition();

            // set form
            GameObject go = Instantiate(formPrefabs[currentCondition.FormIndex]);
            // set angle (meaning position)
            Vector3 objPos = Quaternion.AngleAxis(currentCondition.GetAngle(), Vector3.up) * Vector3.forward * distanceToUser;
            objPos.y = 1.65f;               // --------------------------------------------------------------------------------------------------TODO: set Object height to head height of user
            go.transform.position = objPos;
            // set size
            go.transform.localScale *= currentCondition.GetSize();
            // set speed
            currentCollisionObject = go.AddComponent<CollisionObject>();
            currentCollisionObject.Init(currentCondition.GetSpeed(), currentCondition.GetAngle(), this);

            testIsRunning = true;
            userIsPositioned = false;
        }

        public void endTest()
        {
            currentCondition.CollisionDetected = currentCollisionObject.collisionDetected;
            testedConditions.Add(currentCondition);

            Destroy(currentCollisionObject.gameObject);
            currentCollisionObject = null;
            testIsRunning = false;
        }

        private ExperimentCondition pickRandomCondition()
        {
            ExperimentCondition ret = untestedConditions[UnityEngine.Random.Range(0, untestedConditions.Count)];
            untestedConditions.Remove(ret);
            return ret;
        }

        [ContextMenu("Setup conditions")]
        private void setupAllConditions()
        {
            testedConditions = new List<ExperimentCondition>();

            for(int formIndex = 0; formIndex < Enum.GetNames(typeof(ExperimentCondition.Form)).Length; formIndex++)
            {
                for(int sizeIndex = 0; sizeIndex < ExperimentCondition.sizes.Length; sizeIndex++)
                {
                    for(int speedIndex = 0; speedIndex < ExperimentCondition.speeds.Length; speedIndex++)
                    {
                        for(int angleIndex = 0; angleIndex < ExperimentCondition.angles.Length; angleIndex++)
                        {
                            untestedConditions.Add(new ExperimentCondition(formIndex, sizeIndex, speedIndex, angleIndex));
                        }
                    }
                }
            }

            Debug.Log("Setup experiment with " + untestedConditions.Count + " conditions.");
        }
    }
}
