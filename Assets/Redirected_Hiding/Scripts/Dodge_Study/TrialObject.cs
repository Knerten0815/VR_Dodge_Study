using UnityEngine;

namespace Dodge_Study
{
    public class TrialObject : MonoBehaviour
    {
        public bool collisionDetected= false;

        private float speed = 0;
        private TrialObjectSpawner cos = null;
        private Vector3 direction;
        private float timer = 0;
        private float arrivalInSeconds = 0;

        // Update is called once per frame
        void Update()
        {
            if (transform.position.z > 0)
            {
                transform.position += direction * speed * Time.deltaTime;
                timer += Time.deltaTime;
            }                
            else
            {
                float calculatedSpeed = cos.distanceToUser / timer;
                float calculatedKMSpeed = calculatedSpeed * 3.6f;
                /*Debug.Log("Travelled " + cos.distanceToUser + "m in " + timer + " seconds. Which is " + calculatedKMSpeed + " km/h or " + calculatedSpeed + " m/s. ");
                if (Mathf.Abs(arrivalInSeconds - timer) > 0.01f)
                    Debug.LogError("Detected incorrect speed! Travelled for " + timer + " seconds instead of " + arrivalInSeconds +"!");
                */
                ExperimentManager.Instance.EndTrial();
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            //Debug.Log(" - BÄM! - Collision detected!");
            ExperimentManager.Instance.currentCondition.CollisionDetected = true;
        }

        public void Init(float speed, float angle, TrialObjectSpawner cos)
        {
            this.speed = speed / 3.6f; // convert from km/h to m/s
            this.cos = cos;

            direction = (PositioningManager.Instance.centerTrans.position - transform.position).normalized;
            direction.y = 0;
            transform.Rotate(Vector3.up * (angle + 180));   // face the user
            timer = 0;
            arrivalInSeconds = cos.distanceToUser / this.speed;

            //Debug.Log("Travelling with " + speed + " km/h or " + this.speed + " m/s. Should arrive in " + arrivalInSeconds + " seconds.");
        }
    }
}

