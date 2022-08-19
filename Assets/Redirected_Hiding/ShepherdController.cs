using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RD_Hiding
{
    public class ShepherdController : MonoBehaviour
    {
        [SerializeField] float minRotTime = 0.7f;
        [SerializeField] float maxRotTime = 3f;
        [SerializeField] private float minRotSpeed = 0.1f;
        [SerializeField] private float maxRotSpeed = 1f;
        [SerializeField] AudioClip[] rotationSounds;

        RedirectionManager rdManager;
        ShepherdResetter resetter;
        AudioSource audio;

        float circleDiameter;
        float resetRingDiameter;
        float maxPlayerRingDepth = 0;
        float rotationSpeed = 0.5f;

        GameObject target;
        Quaternion targetRotation;

        private void Awake()
        {
            rdManager = FindObjectOfType<RedirectionManager>();
            resetter = (ShepherdResetter)rdManager.resetter;
            audio = GetComponent<AudioSource>();

            circleDiameter = resetter.circleDiameter;
            resetRingDiameter = resetter.resetRingDiameter;

            if(resetter.showShepherdTarget)
                target = SingletonFoEveryton.Instance.instantiateSphere(calculateTargetPosition(), true);

            StartCoroutine(calculateRotation());
            audio.Play();
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

            transform.localPosition = Vector3.Lerp(transform.localPosition, calculateTargetPosition(), Time.deltaTime * resetter.ShepherdSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        private Vector3 calculateTargetPosition()
        {
            float playerRingDepth = rdManager.currPosReal.magnitude - circleDiameter / 2;
            float targetVectotMagnitude = 1;

            if (playerRingDepth * 3 < resetRingDiameter)
            {
                if(maxPlayerRingDepth < playerRingDepth)
                    maxPlayerRingDepth = playerRingDepth;

                targetVectotMagnitude = resetRingDiameter - maxPlayerRingDepth;
            }
            else
            {
                targetVectotMagnitude = playerRingDepth + 3f;
            }

            Vector3 targetPos = rdManager.currPosReal.normalized * targetVectotMagnitude;
            targetPos.y = 1;

            return targetPos;
        }

        IEnumerator calculateRotation()
        {
            while (true)
            {
                targetRotation = Random.rotation;
                if (targetRotation.eulerAngles.z > 2)
                {
                    Vector3 euler = targetRotation.eulerAngles;
                    euler.z = Random.Range(-30, 0);
                    targetRotation.eulerAngles = euler;
                }

                rotationSpeed = Random.Range(minRotSpeed, maxRotSpeed);
                float wait = Random.Range(minRotTime, maxRotTime);
                //Debug.Log("Set rotation speed to: " + rotationSpeed + ". Next rotation in " + wait + "seconds.");

                audio.PlayOneShot(rotationSounds[Random.Range(0, rotationSounds.Length - 1)]);
                yield return new WaitForSeconds(wait);
            }
        }
    }
}
