using System.Collections;
using UnityEngine;

namespace RD_Hiding
{
    public class ShepherdController : MonoBehaviour
    {
        [SerializeField] float minRotTime = 0.1f;
        [SerializeField] float maxRotTime = 1f;
        [SerializeField] private float minRotSpeed = 0.1f;
        [SerializeField] private float maxRotSpeed = 1f;
        [SerializeField] private float minHeight = 1.5f, maxHeight = 2f;

        RedirectionManager rdManager;
        ShepherdResetter resetter;

        float circleDiameter;
        float resetRingDiameter;
        float maxPlayerRingDepth = 0;
        float rotationSpeed = 0.5f;

        GameObject target;
        Vector3 targetPosition;
        Quaternion targetRotation;

        private void Awake()
        {
            rdManager = FindObjectOfType<RedirectionManager>();
            resetter = (ShepherdResetter)rdManager.resetter;

            circleDiameter = resetter.circleDiameter;
            resetRingDiameter = resetter.resetRingDiameter;

            targetPosition = calculateTargetPosition();

            StartCoroutine(calculateAnimationKeys());
        }

        private void OnDestroy()
        {
            Destroy(target);
            StopAllCoroutines();
        }

        // Update is called once per frame
        void Update()
        {
            if (resetter.showShepherdTarget)
                target.transform.localPosition = calculateTargetPosition();

            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * resetter.ShepherdSpeed);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        private Vector3 calculateTargetPosition()
        {
            float playerRingDepth = rdManager.currPosReal.magnitude - circleDiameter / 2;
            float targetVectorMagnitude;

            if (playerRingDepth * 3 < resetRingDiameter)
            {
                if(maxPlayerRingDepth < playerRingDepth)
                    maxPlayerRingDepth = playerRingDepth;

                targetVectorMagnitude = resetRingDiameter - maxPlayerRingDepth;
            }
            else
            {
                targetVectorMagnitude = playerRingDepth + 3f;
            }

            Vector3 targetPos = rdManager.currPosReal.normalized * targetVectorMagnitude;// + targetOffset;
            targetPos.y = Random.Range(minHeight, maxHeight);

            return targetPos;
        }

        IEnumerator calculateAnimationKeys()
        {
            while (true)
            {
                targetPosition = calculateTargetPosition() + Random.insideUnitSphere;
                targetRotation = Quaternion.LookRotation(targetPosition - transform.localPosition);

                float wait = Random.Range(0.7f, 3f);
                yield return new WaitForSeconds(wait);
            }
        }
    }
}
