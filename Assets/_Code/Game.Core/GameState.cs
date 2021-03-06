using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
	public class GameState
	{
		public EntityComponent Player;
		public EntityComponent[] Saws;
		public Level CurrentLevel;
		public Dictionary<Vector3Int, int> TileHits = new Dictionary<Vector3Int, int>();
		public int GauntlerPower = 1;
		public bool Running;
		public bool EnableScreenshake = true;
		public bool EnableAssistMode = false;
		public float InitialMusicVolume;
		public float CurrentMusicVolume;
		public float InitialSoundVolume;
		public float CurrentSoundVolume;
	}
}
