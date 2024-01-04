using UnityEngine;

namespace Trivial
{
    public class UISpaceFollowWorld : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Transform _follow;
        [SerializeField] private Vector3 _worldOffset;
        [SerializeField] private Vector3 _screenOffset;
        [Header("This is optional, if unassigned, will use Camera.main")]
        [SerializeField] private Camera _camera;
        private void Start()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        private void LateUpdate()
        {
            Vector3 pos = _camera.WorldToScreenPoint(_follow.position + _worldOffset);

            transform.position = pos + _screenOffset;
        }
    }
}