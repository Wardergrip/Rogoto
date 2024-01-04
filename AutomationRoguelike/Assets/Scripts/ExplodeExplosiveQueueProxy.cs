using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExplodeExplosiveQueueProxy : MonoBehaviour
{
	private ExplosiveQueue _explosiveQueue;
	private void Awake()
	{
		StartCoroutine(InitCoroutine());
	}

	public void Execute(InputAction.CallbackContext ctx)
	{
		if (ctx.performed)
			_explosiveQueue.ExplodePath();
	}

	private IEnumerator InitCoroutine()
	{
		while (_explosiveQueue == null)
		{
			_explosiveQueue = FindObjectOfType<ExplosiveQueue>();
			yield return null;
		}
	}
}
