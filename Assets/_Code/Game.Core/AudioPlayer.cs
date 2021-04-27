using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Game.Core
{
	public class AudioPlayer
	{
		private readonly GameConfig _config;
		private readonly AudioSource _musicSource;

		private readonly Dictionary<string, float> _musicTimes = new Dictionary<string, float>();

		public AudioPlayer(GameConfig config, AudioSource musicSource)
		{
			_config = config;
			_musicSource = musicSource;
		}

		// public void Tick()
		// {
		// 	if (_musicSource.clip)
		// 	{
		// 		_musicTimes[_musicSource.clip.name] = _musicSource.time;
		// 	}
		// }

		public UniTask PlayRandomSoundEffect(AudioClip[] clips, Vector3 position, float volume = 1f)
		{
			var clip = clips[UnityEngine.Random.Range(0, clips.Length)];
			return PlaySoundEffect(clip, position, volume);
		}

		public UniTask PlaySoundEffect(AudioClip clip, float volume = 1f)
		{
			// Default to the center of the screen
			return PlaySoundEffect(clip, Vector3.zero, volume);
		}

		public UniTask PlaySoundEffect(AudioClip clip, Vector3 position, float volume = 1f)
		{
			return PlaySoundClipAtPoint(clip, position, volume);
		}

		public async UniTask PlayMusic(AudioClip clip, bool fromStart = true, float fadeDuration = 0f, float volume = 1f)
		{
			if (fromStart)
			{
				_musicSource.time = 0f;
			}
			else
			{
				_musicTimes.TryGetValue(clip.name, out var time);
				_musicSource.time = time;
			}

			_musicSource.clip = clip;
			_musicSource.Play();

			if (fadeDuration > 0f)
			{
				await _musicSource.DOFade(volume, fadeDuration);
			}
			else
			{
				_musicSource.volume = volume;
			}
		}

		public void PauseMusic()
		{
			_musicSource.Pause();
		}

		public void ResumeMusic()
		{
			_musicSource.UnPause();
		}

		public async UniTask StopMusic(float fadeDuration = 0.5f)
		{
			await _musicSource.DOFade(0f, fadeDuration);
			_musicSource.Stop();
		}

		public void SetMusicVolume(float volume)
		{
			_config.AudioMixer.SetFloat("MusicVolume", ConvertToMixerVolume(volume));
		}

		public void SetSoundVolume(float volume)
		{
			_config.AudioMixer.SetFloat("SoundVolume", ConvertToMixerVolume(volume));
		}

		public bool IsMusicPlaying() => _musicSource.isPlaying;

		public bool IsCurrentMusic(AudioClip clip) => _musicSource.clip == clip;

		/**
			Convert to the volume range the unity audio mixer is expecting.
			<br />0   => -80
			<br />0.5 => -40
			<br />1   => 0
		*/
		private static float ConvertToMixerVolume(float volume)
		{
			return (volume - 1f) * 80f;
		}

		// TODO: Use polling instead of creating game object each time
		private UniTask PlaySoundClipAtPoint(AudioClip clip, Vector3 position, float volume)
		{
			var gameObject = new GameObject("One shot audio");
			gameObject.transform.position = position;
			var audioSource = (AudioSource)gameObject.AddComponent(typeof(AudioSource));
			audioSource.clip = clip;
			audioSource.outputAudioMixerGroup = _config.SoundsAudioMixerGroup;
			audioSource.volume = volume;
			audioSource.Play();
			UnityEngine.Object.Destroy(gameObject, clip.length * ((double)Time.timeScale < 0.00999999977648258 ? 0.01f : Time.timeScale));
			return UniTask.Delay(TimeSpan.FromSeconds(clip.length));
		}
	}
}
