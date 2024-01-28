using UnityEngine;
public class BillboardText : MonoBehaviour
{
    [Tooltip("Refernce the camera here, if its not tagged MainCamera.")]
    public Transform camTransform;

    private void Awake()
    {
        if (camTransform == null)
            camTransform = Camera.main.transform;
    }

    void Update()
    {
        transform.rotation = camTransform.rotation;
    }
}