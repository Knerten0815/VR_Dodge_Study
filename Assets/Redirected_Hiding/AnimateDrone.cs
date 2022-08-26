using System.Collections;
using UnityEditor;
using UnityEngine;

public class AnimateDrone : MonoBehaviour
{
    [SerializeField] float minRot = 5f;
    [SerializeField] float maxRot = 15f;
    [SerializeField] float minPause = 3f;
    [SerializeField] float maxPause = 5f;
    [SerializeField] float LerpSpeed = 10;
    float movementSpeed = 1;
    [SerializeField] float frontMoveDistance = -0.1f;
    [SerializeField] float relSpeedFactor = 20;
    [SerializeField] GameObject[] parts;
    [SerializeField] Vector3[] partDirections;
    Vector3[] partPositions;
    Vector3[] targetPositions;
    [SerializeField] AudioSource audio;
    [SerializeField] AudioClip rotClip;
    [SerializeField] Light spotLight;

    float rotFactor, currentRotFactor;

    private void Awake()
    {
        partPositions = new Vector3[parts.Length];
        targetPositions = new Vector3[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            partPositions[i] = parts[i].transform.localPosition;
        }
    }

    void Start()
    {
        StartCoroutine(setAnimationKeyValues());
    }

    void Update()
    {
        currentRotFactor = Mathf.Lerp(currentRotFactor, rotFactor, Time.deltaTime * LerpSpeed);
        transform.Rotate(Vector3.up * currentRotFactor);

        for (int i = 0; i < parts.Length; i++)
        {
            parts[i].transform.localPosition = Vector3.Lerp(parts[i].transform.localPosition, targetPositions[i], Time.deltaTime * movementSpeed);
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void SetHalo(bool turnOn)
    {
        Component halo = spotLight.GetComponent("Halo");

        if (turnOn)
        {
            halo.GetType().GetProperty("enabled").SetValue(halo, true, null);
        }
        else
        {
            halo.GetType().GetProperty("enabled").SetValue(halo, false, null);
        }

    }

    IEnumerator setAnimationKeyValues()
    {
        while (true)
        {
            // activate spotLight halo
            spotLight.color = Random.ColorHSV(0, 1, 1, 1, 1, 1);
            SetHalo(true);

            // play sound
            audio.pitch = Random.Range(0.6f, 1.3f);
            audio.PlayOneShot(rotClip);

            // set move variables relative to pitch
            float relativeSpeed = audio.pitch - 1;
            if (relativeSpeed > 0)
                movementSpeed = relativeSpeed * relSpeedFactor;
            else
                movementSpeed = relativeSpeed * -10;

            // move front
            targetPositions[0] = partDirections[0] * 0.012f;

            // move other parts
            for (int i = 1; i < targetPositions.Length; i++)
            {
                if(Random.value >= 0.5)
                    targetPositions[i] = partDirections[i] * frontMoveDistance;
            }

            // rotate
            int rndPositive = Random.Range(0, 2) * 2 - 1;
            rotFactor = Random.Range(minRot, maxRot) * rndPositive;

            // wait for rotationClip
            yield return new WaitForSeconds(0.35f);

            // activate spotLight halo
            SetHalo(false);

            // pause
            rotFactor = 0;

            for (int i = 0; i < targetPositions.Length; i++)
            {
                targetPositions[i] = partPositions[i];
            }

            float wait = Random.Range(minPause, maxPause);

            yield return new WaitForSeconds(wait);
        }
    }
}
