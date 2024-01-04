using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectShaker : MonoBehaviour
{
	[SerializeField] private float _trembleIntensity = 0.1f;
	[SerializeField] private float _trembleSpeed = 5f;
	[SerializeField] private float _lerpSpeed = 5f;
	[SerializeField] private float _disableTrembleTimer = 0f;
	private Coroutine _disableTrembleTimerCoroutine = null;

	private Vector3 _initialPosition;

	public bool ShouldTremble { get; set; } = false;

	void Start()
	{
		_initialPosition = transform.position;
	}

	void Update()
	{
		if (!ShouldTremble)
		{
			// Lerp back
			transform.position = Vector3.Lerp(transform.position, _initialPosition, Time.deltaTime * _lerpSpeed);
			return;
		}
		float trembleX = Mathf.PerlinNoise(Time.time * _trembleSpeed, 0) * 2 - 1;
		float trembleY = Mathf.PerlinNoise(0, Time.time * _trembleSpeed) * 2 - 1;

		Vector3 trembleOffset = new Vector3(trembleX, trembleY, 0) * _trembleIntensity;
		transform.position = _initialPosition + trembleOffset;
		if (_disableTrembleTimer > 0.0f && _disableTrembleTimerCoroutine == null)
		{
			_disableTrembleTimerCoroutine = StartCoroutine(DisableTrembleTimerCoroutine());
		}
	}

	private IEnumerator DisableTrembleTimerCoroutine()
	{
		yield return new WaitForSeconds(_disableTrembleTimer);
		ShouldTremble = false;
		_disableTrembleTimerCoroutine = null;
	}
}
