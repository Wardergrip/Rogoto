using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Trivial
{
    /*
   DISCLAIMER: THIS IS A STINKY SCRIPT BY A STINKY ARTIST AND DOESNT REFLECT PROGRAMMER WORK


   */

    public class OffsetInAxis : MonoBehaviour
    {
        [SerializeField] private AnimationCurve offsetCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] private float offsetStrength = 0.1f;
        [SerializeField] private float offsetSpeed = 1.0f;
        [SerializeField] private Vector3 offsetDirection = Vector3.up;
        [SerializeField] private float startingPoint = 0f;

        private Vector3 originalPosition;
        private float curveLength;

        private void Start()
        {
            originalPosition = transform.localPosition;
            curveLength = offsetCurve.keys[offsetCurve.length - 1].time;
        }

        private void Update()
        {
            float offsetTime = (Time.time * offsetSpeed + startingPoint * curveLength) % curveLength;
            float offsetAmount = offsetCurve.Evaluate(offsetTime) * offsetStrength;
            Vector3 worldOffsetDirection = transform.TransformDirection(offsetDirection);
            Vector3 offset = offsetDirection * offsetAmount;
            transform.localPosition = originalPosition + offset;
            //transform.Translate(offset, Space.Self);
        }
    }
}
