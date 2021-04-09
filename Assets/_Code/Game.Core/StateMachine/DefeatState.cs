using Cysharp.Threading.Tasks;

namespace Game.Core
{
	public class DefeatState : BaseGameState
	{
		public DefeatState(GameFSM machine, Game game) : base(machine, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			_ui.SetDebugText("State: Defeat");
			_ = _ui.ShowDefeat();
			_ui.DefeatButton1.onClick.AddListener(Restart);
			_ui.DefeatButton2.onClick.AddListener(Quit);
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			await _ui.HideDefeat();

			_ui.DefeatButton1.onClick.RemoveListener(Restart);
			_ui.DefeatButton2.onClick.RemoveListener(Quit);
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
