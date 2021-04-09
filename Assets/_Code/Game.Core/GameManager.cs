using UnityEngine;
using UnityEngine.Assertions;
using Game.Inputs;

namespace Game.Core
{
	public class GameManager : MonoBehaviour
	{
		public Game Game { get; private set; }

		private void Start()
		{
			var musicAudioSource = GameObject.Find("Music Audio Source").GetComponent<AudioSource>();
			var config = Resources.Load<GameConfig>("Game Config");
			var camera = Camera.main;
			var ui = FindObjectOfType<GameUI>();

			Assert.IsNotNull(config);
			Assert.IsNotNull(musicAudioSource);
			Assert.IsNotNull(camera);
			Assert.IsNotNull(ui);

			Game = new Game();
			Game.Config = config;
			Game.Controls = new GameControls();
			Game.Camera = Camera.main;
			Game.UI = ui;
			Game.State = new GameState();
			Game.AudioPlayer = new AudioPlayer(config, musicAudioSource);
			Game.FSM = new GameFSM(false, Game);

			Game.UI.Inject(Game);
			Game.FSM.Start();
		}

		private void Update()
		{
			Game.FSM.Tick();
		}

		private void FixedUpdate()
		{
			Game.FSM.FixedTick();
		}
	}
}
