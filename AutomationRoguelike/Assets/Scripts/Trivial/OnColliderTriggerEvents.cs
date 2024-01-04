using UnityEngine;
using UnityEngine.Events;

namespace Trivial
{
	public class OnColliderTriggerEvents : MonoBehaviour
	{
		public UnityEvent<Collider> OnTriggerEnterEvent;
		public UnityEvent<Collider> OnTriggerStayEvent;
		public UnityEvent<Collider> OnTriggerExitEvent;

		private void OnTriggerEnter(Collider other)
		{
			OnTriggerEnterEvent?.Invoke(other);
		}

		private void OnTriggerStay(Collider other)
		{
			OnTriggerStayEvent?.Invoke(other);
		}

		private void OnTriggerExit(Collider other)
		{
			OnTriggerExitEvent?.Invoke(other);
		}
	}
}
