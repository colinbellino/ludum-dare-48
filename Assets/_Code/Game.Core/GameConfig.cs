using UnityEngine;
using UnityEngine.Audio;

namespace Game.Core
{
	[CreateAssetMenu(menuName = "Game Jam/Game Config")]
	public class GameConfig : ScriptableObject
	{
		public EntityComponent PlayerPrefab;
		public EntityComponent WallOfDeathPrefab;

		public string[] Levels;

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
}
