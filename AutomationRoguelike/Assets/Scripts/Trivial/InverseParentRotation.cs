using UnityEngine;

namespace Trivial
{
    public class InverseParentRotation : MonoBehaviour
    {
		[SerializeField] private LockAxis _lockAxis;
		[SerializeField] private bool _swapYAndZ = false;

		private void Awake()
		{
			UpdateRotation();
		}

		public void UpdateRotation()
		{
			Vector3 euler = new
				(
					_lockAxis.IsLockedOn(LockAxis.X) ? 0 : transform.rotation.eulerAngles.x,
					_lockAxis.IsLockedOn(LockAxis.Y) ? 0 : transform.rotation.eulerAngles.y,
					_lockAxis.IsLockedOn(LockAxis.Z) ? 0 : transform.rotation.eulerAngles.z
				);
			Quaternion rotation = Quaternion.Euler(
					euler.x,
					_swapYAndZ ? euler.z : euler.y,
					_swapYAndZ ? euler.y : euler.z
				);
			transform.Rotate(-rotation.eulerAngles);
		}
	}
}