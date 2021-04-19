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

			await _ui.FadeIn(Color.black);

			_ui.SetDebugText(@"State: Gameplay
- F1: Trigger win condition
- F2: Trigger defeat condition");
			_state.Player = SpawnPlayer(_config.PlayerPrefab, _game, Vector3.zero);
			_state.Background = GameObject.Find("parallax_mockup");
			_state.Background.SetActive(true);
			_controls.Gameplay.Enable();

			_ = _audioPlayer.PlayMusic(_config.MainMusic);

			await _ui.FadeOut();
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

			if (_state.Player != null)
			{
				var speed = 5f;
				var moveInput = _controls.Gameplay.Move.ReadValue<Vector2>();
				var position = _state.Player.transform.position;
				position.x += moveInput.x * Time.deltaTime * speed;
				position.y += moveInput.y * Time.deltaTime * speed;
				_state.Player.transform.position = position;
			}
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			_ui.HideGameplay();
			GameObject.Destroy(_state.Player.gameObject);
			_state.Background.SetActive(false);

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

