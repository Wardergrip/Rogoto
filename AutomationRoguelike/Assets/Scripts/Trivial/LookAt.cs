using UnityEngine;

namespace Trivial
{
	public class LookAt : MonoBehaviour
	{
		[SerializeField] private GameObject[] _objectsToRotate;
		public GameObject[] ObjectsToRotate { get => _objectsToRotate; }
		[SerializeField] private GameObject _objectToLookAt;
		public GameObject ObjectToLookAt { get => _objectToLookAt; set => _objectToLookAt = value; }
		[SerializeField] private float _angleEventTreshold = 0.9f;
		[Tooltip("If it's negative it's disabled")] [SerializeField] private float _lerpSpeed = -1.0f;
		[SerializeField] private bool _lockXRotation = true;
		[SerializeField] private bool _lockYrotation = false;
		[SerializeField] private bool _lockZRotation = false;
		[SerializeField] private bool _flipAroundY = false;

		public bool IsLookingAtObject { get; private set; }

		private Vector3 _lookPos = new();

		void LateUpdate()
		{
			if (_objectToLookAt == null) return;

			foreach (var obj in _objectsToRotate)
			{
				UpdateRotation(obj.transform);
			}
		}

		private void UpdateRotation(Transform transform)
		{
			_lookPos.x = _objectToLookAt.transform.position.x;
			_lookPos.y = _objectToLookAt.transform.position.y;
			_lookPos.z = _objectToLookAt.transform.position.z;
			if (_lockXRotation)
			{
				_lookPos.x = transform.position.x;
			}
			if (_lockYrotation)
			{
				_lookPos.y = transform.position.y;
			}
			if (_lockZRotation)
			{
				_lookPos.z = transform.position.z;
			}

			if (_lerpSpeed < 0.0f)
			{
				transform.LookAt(_lookPos);
			}
			else
			{
				Quaternion toRotation = Quaternion.LookRotation(_lookPos - transform.position);
				transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, _lerpSpeed * Time.deltaTime);
			}
			if (Mathf.Abs(Vector3.SignedAngle(_lookPos - transform.position, transform.forward, Vector3.up)) < _angleEventTreshold)
			{
				IsLookingAtObject = true;
			}
			else
			{
				IsLookingAtObject = false;
			}
			if (_flipAroundY) transform.Rotate(Vector3.up, 180.0f);
		}
	}
}