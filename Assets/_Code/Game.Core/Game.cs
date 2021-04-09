using System.Collections.Generic;
using UnityEngine;
using Game.Inputs;

namespace Game.Core
{
	public class Game
	{
		public GameConfig Config;
		public GameUI UI;
		public Camera Camera;
		public GameControls Controls;
		public GameState State;
		public AudioPlayer AudioPlayer;
		public GameFSM FSM;
	}
}
