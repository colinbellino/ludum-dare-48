using UnityEngine;

namespace Game.Core
{
	public class GameState
	{
		public EntityComponent Player;
		public EntityComponent WallOfDeath;
		public Level CurrentLevel;

		public bool Running;
	}
}
