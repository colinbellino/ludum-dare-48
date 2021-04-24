using Game.Inputs;

namespace Game.Core
{
	public class Game
	{
		public GameConfig Config;
		public GameUI UI;
		public CameraRig Camera;
		public GameControls Controls;
		public GameState State;
		public AudioPlayer AudioPlayer;
		public GameFSM FSM;
	}
}
