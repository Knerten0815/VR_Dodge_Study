using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spaces.Demo
{
    public class HoverUpAndDown : MonoBehaviour
    {
        [SerializeField] private float frequency = 1f, amplitude = 1f;
        private float baseHight;

        private void OnEnable()
        {
            baseHight = gameObject.transform.localPosition.y;
        }

        void Update()
        {
            gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, baseHight + amplitude * Mathf.Sin(frequency * Time.time), gameObject.transform.localPosition.z);
        }
    }
}