using UnityEngine;

namespace Trivial
{
    public class TransformMimic : MonoBehaviour
    {
        [Header("Mimic")]
        [SerializeField] private Transform _transformToMimic;
        public Transform Mimic { get { return _transformToMimic; } set { _transformToMimic = value; } }
        [Header("Settings")]
        [SerializeField] private Vector3 _positionOffset = new(0, 0, 0);
        [SerializeField] private bool _mimicPosition = true;
        [SerializeField] private bool _mimicRotation = true;
        [SerializeField] private bool _mimicScale = false;

        void LateUpdate()
        {
            if (_transformToMimic == null) return;

            if (_mimicPosition)
            {
                transform.position = _transformToMimic.position + _positionOffset;
            }
            if (_mimicRotation)
            {
                transform.rotation = _transformToMimic.rotation;
            }
            if (_mimicScale)
            {
                transform.localScale = _transformToMimic.localScale;
            }
        }
    }
}