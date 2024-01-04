using System.Collections;
using Trivial;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicPlayer : MonoSingleton<MusicPlayer>
{
	[SerializeField] private AudioSource _audioSourceFocus;
	[SerializeField] private AudioSource _audioSourceFight;
	[SerializeField] private AudioClip[] _focusClips;
	[SerializeField] private AudioClip[] _fightClips;
	[SerializeField] private float _timeBeforeFightStart = 1f;
	[SerializeField] private float _fadeInTime = .5f;
	[SerializeField] private float _fadeOutTime = 1f;
	private float _initialVolume = 100f;

	protected override void Awake()
	{
		base.Awake();

		DontDestroyOnLoad(this.gameObject);
		SceneManager.sceneLoaded += PlayFocusMusic;
		GameSystem.OnRewardClaimed += PlayFocusMusic;
		GameSystem.OnWaveStarted += PlayFightMusic;
		GameSystem.OnNestDamaged += FadeOutFightMusic;
		GameSystem.OnShowGameOverScreen += FadeOutFightMusic;
		_initialVolume = _audioSourceFocus.volume;
		_audioSourceFocus.Stop();
		PlayFocusMusic();
	}

	private void ResetClipAndPlay(AudioSource source, AudioClip[] clips, bool fade = true)
	{
		source.clip = clips.GetRandomValue();
		source.loop = true;
		if (fade)
			StartCoroutine(Fade(source, _fadeInTime, _initialVolume));
		else
			source.volume = _initialVolume;
		source.Play();
	}

	private void PlayFocusMusic(Scene scene, LoadSceneMode mode)
	{
		PlayFocusMusic();
	}

	private void PlayFocusMusic()
	{
		if (!_audioSourceFocus.isPlaying)
		{
			_audioSourceFight.Stop();
			ResetClipAndPlay(_audioSourceFocus, _focusClips);
		}
		else
		{
			_audioSourceFocus.volume = _initialVolume;
		}
	}

	private void PlayFightMusic()
	{
		StartCoroutine(Fade(_audioSourceFocus, _fadeOutTime, 0f));
		StartCoroutine(PlayFightWithDelay());
	}

	private IEnumerator Fade(AudioSource audioSource, float duration, float targetVolume)
	{
		float time = 0f;
		float startVolume = audioSource.volume;
		while (time < duration) 
		{ 
			time += Time.deltaTime;
			audioSource.volume = Mathf.Lerp(startVolume, targetVolume, time/duration);
			yield return null;
		}
		yield break;
	}

	private IEnumerator PlayFightWithDelay()
	{
		yield return new WaitForSeconds(_timeBeforeFightStart);
		ResetClipAndPlay(_audioSourceFight, _fightClips, false);
		//_audioSourceFocus.Stop();
	}

	private void FadeOutFightMusic()
	{
		StartCoroutine(Fade(_audioSourceFight, _fadeOutTime, 0f));
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= PlayFocusMusic;
		GameSystem.OnRewardClaimed -= PlayFocusMusic;
		GameSystem.OnWaveStarted -= PlayFightMusic;
		GameSystem.OnNestDamaged -= FadeOutFightMusic;
		GameSystem.OnShowGameOverScreen -= FadeOutFightMusic;
	}
}

