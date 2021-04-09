using Cysharp.Threading.Tasks;

namespace Game.Core
{
	public class VictoryState : BaseGameState
	{
		public VictoryState(GameFSM machine, Game game) : base(machine, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			_ui.SetDebugText("State: Victory");
			await _ui.ShowVictory();

			_ui.VictoryButton1.onClick.AddListener(Restart);
			_ui.VictoryButton2.onClick.AddListener(Quit);

			_ = _audioPlayer.StopMusic(5f);
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			await _ui.HideVictory();

			_ui.VictoryButton1.onClick.RemoveListener(Restart);
			_ui.VictoryButton2.onClick.RemoveListener(Quit);
		}

		private void Restart()
		{
			_machine.Fire(GameFSM.Triggers.Retry);
		}

		private void Quit()
		{
			_machine.Fire(GameFSM.Triggers.Quit);
		}
	}
}
