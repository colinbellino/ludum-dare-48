using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static Game.Core.Utils;

namespace Game.Core
{
	public class GameplayState : BaseGameState
	{
		public GameplayState(GameFSM machine, Game game) : base(machine, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			_ui.SetDebugText(@"State: Gameplay\n
			- F1: Trigger win condition\n
			- F2: Trigger defeat condition\n");
			_state.Player = SpawnPlayer(_config.PlayerPrefab, _game, Vector3.zero);

			await _ui.FadeOut();

			_controls.Gameplay.Enable();
		}

		public override void Tick()
		{
			base.Tick();

			if (IsDevBuild())
			{
				if (Keyboard.current.f1Key.wasPressedThisFrame)
				{
					Victory();
				}
				if (Keyboard.current.f2Key.wasPressedThisFrame)
				{
					Defeat();
				}
			}
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			_ui.HideGameplay();

			_controls.Gameplay.Disable();
		}

		private async void Victory()
		{
			await _ui.FadeIn(Color.white);
			_machine.Fire(GameFSM.Triggers.Victory);
		}

		private async void Defeat()
		{
			await _ui.FadeIn(Color.black);
			_machine.Fire(GameFSM.Triggers.Defeat);
		}
	}
}

