using UnityEngine;

namespace Dodge_Study
{
    public class TrialObjectSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject[] formPrefabs = new GameObject[4];
        public float distanceToUser = 20;
        
        private TrialObject currentCollisionObject = null;
        [SerializeField] private bool userIsPositioned;

        public void spawnObject()
        {
            TrialData ecpC = ExperimentManager.Instance.currentCondition;

            // set form
            GameObject go = Instantiate(formPrefabs[ecpC.FormIndex]);
            // set angle (meaning position)
            Vector3 objPos = Quaternion.AngleAxis(ecpC.GetAngle(), Vector3.up) * Vector3.forward * distanceToUser;
            objPos.y = 1.65f;               // --------------------------------------------------------------------------------------------------TODO: set Object height to head height of user
            go.transform.position = objPos;
            // set size
            go.transform.localScale *= ecpC.GetSize();
            // set speed
            currentCollisionObject = go.AddComponent<TrialObject>();
            currentCollisionObject.Init(ecpC.GetSpeed(), ecpC.GetAngle(), this);
        }
    }
}
