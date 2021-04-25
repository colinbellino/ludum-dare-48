using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Tilemaps;

namespace Game.Core
{
	[CreateAssetMenu(menuName = "Game Jam/Game Config")]
	public class GameConfig : ScriptableObject
	{
		public EntityComponent PlayerPrefab;
		public EntityComponent WallOfDeathPrefab;

		public Level[] Levels;
		public TileData[] Tiles;
		public TileBase[] DamageOverlays;

		[Header("Audio")]
		public AudioMixer AudioMixer;
		public AudioMixerGroup MusicAudioMixerGroup;
		public AudioMixerGroup SoundsAudioMixerGroup;
		public AudioMixerSnapshot DefaultAudioSnapshot;
		public AudioMixerSnapshot PauseAudioSnapshot;
		[Range(0f, 1f)] public float MusicVolume = 1f;
		[Range(0f, 1f)] public float SoundVolume = 1f;
		public AudioClip MainMusic;
		public AudioClip MenuTextAppearClip;
		public AudioClip MenuConfirmClip;
		public AudioClip[] DigClips;
		public AudioClip[] ClingClips;
	}

	[Serializable]
	public class Level
	{
		public string SceneName;
		public AudioClip Music;
		public Vector3 WallOfDeathStartPosition = new Vector3(0, 12f, 0);
		public bool Safe;
	}

	[Serializable]
	public class TileData
	{
		public TileBase Tile;
		public bool Breakable;
		public int HitsToBreak = 3;
	}
}
