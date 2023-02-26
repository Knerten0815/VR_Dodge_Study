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
        private Vector3 direction;

        void Update()
        {
            transform.localPosition += direction * speed * Time.deltaTime;
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

        public void Init(float speed, bool isCar)
        {
            this.speed = speed / 3.6f; // convert from km/h to m/s

            direction = transform.forward;
            if (isCar)
            {
                Vector3 carPos = transform.position;
                carPos.y = 0;
                transform.position = carPos;
            }
        }
    }
}

