using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObserverController : MonoBehaviour
{
    public Transform player;
    public Light observerLight;
    public float spotWidth = 1;
    public float spotOffset = 0.18f;

    private Vector3 lookAtPos;
    private float playerDistance;

    // Update is called once per frame
    void Update()
    {
        playerDistance = (transform.position - player.position).magnitude;
        lookAtPos = player.position;
        lookAtPos.z -= spotOffset;
        observerLight.spotAngle = Mathf.Rad2Deg * Mathf.Atan(spotWidth / playerDistance);
        transform.LookAt(lookAtPos);
    }
}
