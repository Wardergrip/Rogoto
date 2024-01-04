using System.Collections;
using System.Collections.Generic;
using Trivial;
using UnityEngine;

public class EnvironmentalAmbience : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
	[SerializeField] private AudioClip[] _clips;
	[SerializeField] private float _minWaitTime = 45.0f;
	[SerializeField] private float _maxWaitTime = 150.0f;

	private void Awake()
	{
		Debug.Assert(_clips.Length > 0,$"Clips are empty!");
		StartCoroutine(EnvironmentalSoundsCoroutine());
	}

	private IEnumerator EnvironmentalSoundsCoroutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(Random.Range(_minWaitTime, _maxWaitTime));
			_audioSource.clip = _clips.GetRandomValue();
			_audioSource.Play();
		}
	}
}
