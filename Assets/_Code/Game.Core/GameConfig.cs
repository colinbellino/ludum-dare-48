using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Core
{
	[CreateAssetMenu(menuName = "Game Jam/Game Config")]
	public class GameConfig : ScriptableObject
	{
		public EntityComponent PlayerPrefab;
		public EntityComponent WallOfDeathPrefab;

		public Level[] Levels;

		[Header("Audio")]
		public AudioMixer AudioMixer;
		public AudioMixerGroup MusicAudioMixerGroup;
		public AudioMixerGroup SoundsAudioMixerGroup;
		public AudioMixerSnapshot DefaultAudioSnapshot;
		public AudioMixerSnapshot PauseAudioSnapshot;
		public AudioClip MainMusic;
		public AudioClip MenuTextAppearClip;
		public AudioClip MenuConfirmClip;
		[Range(0f, 1f)] public float MusicVolume = 1f;
		[Range(0f, 1f)] public float SoundVolume = 1f;
	}

	[Serializable]
	public class Level
	{
		public string SceneName;
		public AudioClip Music;
		// public Vector3 PlayerStartPosition = new Vector3(-12f, 8f, 0);
		public Vector3 WallOfDeathStartPosition = new Vector3(0, 12f, 0);
		public bool Safe;
	}
}
