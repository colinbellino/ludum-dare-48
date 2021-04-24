using UnityEngine;

namespace Game.Core
{
	public class GameState
	{
		public EntityComponent Player;
		public EntityComponent WallOfDeath;
		public string CurrentLevel;

		public bool Running;
	}
}
