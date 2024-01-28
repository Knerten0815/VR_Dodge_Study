using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class AnimateDrone : MonoBehaviour
{
    [SerializeField] bool isShepherd;
    [SerializeField] float hoverAmplitude = 1f, hoverFrequency = 1f;
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
    [SerializeField] AudioSource rotationAudio, loopAudio;
    [SerializeField] AudioClip rotClip;
    [SerializeField] float volume = 0.3f, fadeSpeed = 1f;
    [SerializeField] Light spotLight;

    float rotFactor, currentRotFactor;

    public bool isActive;

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
        isActive = true;
        loopAudio.volume = 0;
        StartCoroutine(setAnimationKeyValues());
        StartCoroutine(fadeInAudio());
    }

    void Update()
    {
        // hover up and down
        transform.localPosition = Vector3.zero + new Vector3(0, hoverAmplitude * Mathf.Sin(hoverFrequency * Time.time), 0);

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
            Type haloType = halo.GetType();
            haloType.GetProperty("enabled").SetValue(halo, true, null);
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
            //spotLight.color = UnityEngine.Random.ColorHSV(0, 1, 1, 1, 1, 1);
            if(isShepherd)
                SetHalo(true);

            // play sound
            rotationAudio.pitch = UnityEngine.Random.Range(0.6f, 1.3f);
            rotationAudio.PlayOneShot(rotClip);

            // set move variables relative to pitch
            float relativeSpeed = rotationAudio.pitch - 1;
            if (relativeSpeed > 0)
                movementSpeed = relativeSpeed * relSpeedFactor;
            else
                movementSpeed = relativeSpeed * -10;

            // move front
            targetPositions[0] = partDirections[0] * 0.012f;

            // move other parts
            for (int i = 1; i < targetPositions.Length; i++)
            {
                if(UnityEngine.Random.value >= 0.5)
                    targetPositions[i] = partDirections[i] * frontMoveDistance;
            }

            // rotate
            int rndPositive = UnityEngine.Random.Range(0, 2) * 2 - 1;
            rotFactor = UnityEngine.Random.Range(minRot, maxRot) * rndPositive;

            // wait for rotationClip
            yield return new WaitForSeconds(0.35f);

            // activate spotLight halo
            if(isShepherd)
                SetHalo(false);

            // pause
            rotFactor = 0;

            for (int i = 0; i < targetPositions.Length; i++)
            {
                targetPositions[i] = partPositions[i];
            }

            float wait = UnityEngine.Random.Range(minPause, maxPause);

            yield return new WaitForSeconds(wait);
        }
    }

    IEnumerator fadeInAudio()
    {
        while(loopAudio.volume < volume && isActive)
        {
            loopAudio.volume += Time.deltaTime * fadeSpeed;
            yield return null;
        }
    }
}
