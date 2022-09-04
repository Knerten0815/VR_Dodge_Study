using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the Observers spot light angle, rotation and animation.
/// </summary>
public class ObserverController : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] Light observerLight;
    [SerializeField] float spotWidth = 1, spotOffset = 0, intensityFactor = 0.235f;
    [SerializeField] float frequency = 1f, amplitude = 1f;

    private Vector3 basePos;
    private Vector3 lookAtPos;
    private float playerDistance;

    private void Awake()
    {
        basePos = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        // hover up and down
        transform.localPosition = basePos + new Vector3(0, amplitude * Mathf.Sin(frequency * Time.time), 0);

        // point spotLight at Player
        transform.LookAt(lookAtPos);

        // adjust spot light angle
        playerDistance = (transform.position - player.position).magnitude;
        lookAtPos = player.position;
        lookAtPos.z -= spotOffset;
        observerLight.spotAngle = Mathf.Rad2Deg * Mathf.Atan(spotWidth / playerDistance);
    }
}
