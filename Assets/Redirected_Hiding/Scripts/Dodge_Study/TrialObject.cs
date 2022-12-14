using UnityEngine;

namespace Dodge_Study
{
    /// <summary>
    /// A TrialObject is spawned by the TrialObjectSpawner at a position in real space.
    /// Then it moves through virtual space towards the users initial position.
    /// </summary>
    public class TrialObject : MonoBehaviour
    {
        public bool collisionDetected= false;

        private float speed = 0;
        //private TrialObjectSpawner tos = null;
        private Vector3 direction;
        //private float timer = 0;
        //private float arrivalInSeconds = 0;

        // Update is called once per frame
        void Update()
        {
            transform.localPosition += direction * speed * Time.deltaTime;
            /*float calculatedSpeed = cos.distanceToUser / timer;
            float calculatedKMSpeed = calculatedSpeed * 3.6f;
            Debug.Log("Travelled " + cos.distanceToUser + "m in " + timer + " seconds. Which is " + calculatedKMSpeed + " km/h or " + calculatedSpeed + " m/s. ");
            if (Mathf.Abs(arrivalInSeconds - timer) > 0.01f)
                Debug.LogError("Detected incorrect speed! Travelled for " + timer + " seconds instead of " + arrivalInSeconds +"!");
            */
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.tag == "Goal")
            {
                ExperimentManager.Instance.EndTrial();
                Destroy(gameObject);
            }
            else
            {
                Debug.Log(" - BÄM! - Collided with " + other.name);
                ExperimentManager.Instance.currentCondition.CollisionDetected = true;
            }            
        }

        public void Init(float speed, bool isCar)//, float angle, TrialObjectSpawner tos)
        {
            this.speed = speed / 3.6f; // convert from km/h to m/s
            //this.tos = tos;

            direction = transform.forward;//(PositioningManager.Instance.centerTrans.position - transform.position).normalized;
            if (isCar)
            {
                Vector3 carPos = transform.position;
                carPos.y = 0;
                transform.position = carPos;
            }
            //direction.y = 0;
            //transform.Rotate(Vector3.up * (angle + 180));   // face the user
            //timer = 0;
            //arrivalInSeconds = tos.distanceToUser / this.speed;

            //Debug.Log("Travelling with " + speed + " km/h or " + this.speed + " m/s. Should arrive in " + arrivalInSeconds + " seconds.");
        }
    }
}

