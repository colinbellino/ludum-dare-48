using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static Game.Core.Utils;

namespace Game.Core
{
	public class GameplayState : BaseGameState
	{
		private bool _confirmWasPressedThisFrame;

		public GameplayState(GameFSM machine, Game game) : base(machine, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			await _ui.FadeIn(Color.black);

			_ui.SetDebugText(@"State: Gameplay
- F1: Trigger win condition
- F2: Trigger defeat condition");
			_state.Player = SpawnPlayer(_config.PlayerPrefab, _game, new Vector3(-12f, 12f, 0));
			_controls.Gameplay.Enable();
			_controls.Gameplay.Confirm.started += ConfirmStarted;

			_ = _audioPlayer.PlayMusic(_config.MainMusic);

			await _ui.FadeOut();
		}

		private void ConfirmStarted(InputAction.CallbackContext context) => _confirmWasPressedThisFrame = true;

		public override void Tick()
		{
			base.Tick();

			var moveInput = _controls.Gameplay.Move.ReadValue<Vector2>();
			var confirmInput = _controls.Gameplay.Confirm.ReadValue<float>();

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
				var player = _state.Player;
				if (player.Controller.isGrounded)
				{
					player.Velocity.y = 0;
				}

				if (moveInput.x > 0f)
				{
					player.NormalizedHorizontalSpeed = 1;
					if (player.transform.localScale.x < 0f)
					{
						player.transform.localScale = new Vector3(-player.transform.localScale.x, player.transform.localScale.y, player.transform.localScale.z);
					}

					if (player.Controller.isGrounded)
					{
						player.Animator.Play(Animator.StringToHash("Run"));
					}
				}
				else if (moveInput.x < 0f)
				{
					player.NormalizedHorizontalSpeed = -1;
					if (player.transform.localScale.x > 0f)
					{
						player.transform.localScale = new Vector3(-player.transform.localScale.x, player.transform.localScale.y, player.transform.localScale.z);
					}

					if (player.Controller.isGrounded)
					{
						player.Animator.Play(Animator.StringToHash("Run"));
					}
				}
				else
				{
					player.NormalizedHorizontalSpeed = 0;

					if (player.Controller.isGrounded)
					{
						player.Animator.Play(Animator.StringToHash("Idle"));
					}
				}


				// we can only jump whilst grounded
				if (player.Controller.isGrounded && _confirmWasPressedThisFrame)
				{
					player.Velocity.y = Mathf.Sqrt(2f * player.JumpHeight * -player.Gravity);
					player.Animator.Play(Animator.StringToHash("Jump"));
				}

				// apply horizontal speed smoothing it. dont really do this with Lerp. Use SmoothDamp or something that provides more control
				var smoothedMovementFactor = player.Controller.isGrounded ? player.GroundDamping : player.InAirDamping; // how fast do we change direction?
				player.Velocity.x = Mathf.Lerp(player.Velocity.x, player.NormalizedHorizontalSpeed * player.RunSpeed, Time.deltaTime * smoothedMovementFactor);

				// apply gravity before moving
				player.Velocity.y += player.Gravity * Time.deltaTime;

				// // if holding down bump up our movement amount and turn off one way platform detection for a frame.
				// // this lets us jump down through one way platforms
				// if (player.Controller.isGrounded && moveInput.y < 0f)
				// {
				// 	player.Velocity.y *= 3f;
				// 	player.Controller.ignoreOneWayPlatformsThisFrame = true;
				// }

				player.Controller.move(player.Velocity * Time.deltaTime);

				// grab our current player.Velocity to use as a base for all calculations
				player.Velocity = player.Controller.velocity;
			}

			_confirmWasPressedThisFrame = false;
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			_ui.HideGameplay();
			GameObject.Destroy(_state.Player.gameObject);

			_controls.Gameplay.Disable();
			_controls.Gameplay.Confirm.started -= ConfirmStarted;
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

