using UnityEngine;

namespace RD_Hiding
{
    /// <summary>
    /// taken from: https://forum.unity.com/threads/make-ui-or-image-following-facing-smoothly-to-camera-like-the-unity-vr-splashscreen.835957/
    /// </summary>
    public class SmoothCameraFollowUI : MonoBehaviour
    {
        public Camera Camera2Follow;
        public float CameraDistance = 3.0F;
        public float smoothTime = 0.3F;
        private Vector3 velocity = Vector3.zero;
        private Transform target;
        float height;

        void Awake()
        {
            target = Camera2Follow.transform;
            height = transform.position.y;
        }

        void Update()
        {
            //position
            Transform newTrans = target;
            newTrans.rotation = Quaternion.Euler(0, target.rotation.eulerAngles.y, target.rotation.eulerAngles.z);
            Vector3 targetPosition = newTrans.TransformPoint(new Vector3(0, 0, CameraDistance));
            targetPosition.y = height;

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

            //rotation
            transform.LookAt(transform.position + Camera2Follow.transform.rotation * Vector3.forward, Camera2Follow.transform.rotation * Vector3.up);
            Vector3 rot = transform.rotation.eulerAngles;
            rot.x = 0;
            rot.z = 0;

            transform.rotation = Quaternion.Euler(rot);
        }
    }
}