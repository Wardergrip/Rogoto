using System.Collections.Generic;
using UnityEngine;

public class AnimationPlayer : MonoBehaviour
{
	[SerializeField] private Animator _animator;
	[SerializeField] private bool _playOnAwake = false;
	[SerializeField] private bool _cycle = false;
	private int _cycleIndex = 0;
	[SerializeField] private List<string> _animationNames;

	private void OnEnable()
	{
		if (_animator != null && _playOnAwake)
		{
			Play();
		}
	}

	public void Play(string name)
	{
		_animator.Play(name);
	}

	public void Play()
	{
		if (_cycle)
		{
			_cycleIndex = (_cycleIndex + 1) % _animationNames.Count;
		}

		_animator.Play(_animationNames[_cycleIndex]);
	}
}