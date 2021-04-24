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
			_state.Player = SpawnPlayer(_config.PlayerPrefab, _game, new Vector3(-12f, 8f, 0));
			_state.Player.Controller.onTriggerEnterEvent += OnPlayerTriggerEnter;
			_state.Player.Controller.onTriggerExitEvent += OnPlayerTriggerExit;

			_camera.VirtualCamera.Follow = _state.Player.transform;
			_camera.Confiner.m_BoundingShape2D = GetLevelConfiner();

			_state.WallOfDeath = SpawnWallOfDeath(_config.WallOfDeathPrefab, _game, new Vector3(0, 12f, 0));

			_controls.Gameplay.Enable();
			_controls.Gameplay.Confirm.started += ConfirmStarted;

			_ = _audioPlayer.PlayMusic(_config.MainMusic);

			_state.Running = true;

			await _ui.FadeOut();
		}

		public override void Tick()
		{
			base.Tick();

			var moveInput = _controls.Gameplay.Move.ReadValue<Vector2>();

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

			if (_state.Running)
			{
				if (_state.WallOfDeath != null)
				{
					var entity = _state.WallOfDeath;

					entity.Velocity.y = -entity.RunSpeed;
					entity.Controller.move(entity.Velocity * Time.deltaTime);
				}

				if (_state.Player != null)
				{
					var entity = _state.Player;

					if (entity.Controller.isGrounded)
					{
						entity.Velocity.y = 0;
					}

					if (moveInput.x > 0f)
					{
						entity.NormalizedHorizontalSpeed = 1;
						if (entity.transform.localScale.x < 0f)
						{
							entity.transform.localScale = new Vector3(-entity.transform.localScale.x, entity.transform.localScale.y, entity.transform.localScale.z);
						}

						if (entity.Controller.isGrounded)
						{
							entity.Animator?.Play(Animator.StringToHash("Run"));
						}
					}
					else if (moveInput.x < 0f)
					{
						entity.NormalizedHorizontalSpeed = -1;
						if (entity.transform.localScale.x > 0f)
						{
							entity.transform.localScale = new Vector3(-entity.transform.localScale.x, entity.transform.localScale.y, entity.transform.localScale.z);
						}

						if (entity.Controller.isGrounded)
						{
							entity.Animator?.Play(Animator.StringToHash("Run"));
						}
					}
					else
					{
						entity.NormalizedHorizontalSpeed = 0;

						if (entity.Controller.isGrounded)
						{
							entity.Animator?.Play(Animator.StringToHash("Idle"));
						}
					}


					// we can only jump whilst grounded
					if (entity.Controller.isGrounded && _confirmWasPressedThisFrame)
					{
						entity.Velocity.y = Mathf.Sqrt(2f * entity.JumpHeight * -entity.Gravity);
						entity.Animator?.Play(Animator.StringToHash("Jump"));
					}

					// apply horizontal speed smoothing it. dont really do this with Lerp. Use SmoothDamp or something that provides more control
					var smoothedMovementFactor = entity.Controller.isGrounded ? entity.GroundDamping : entity.InAirDamping; // how fast do we change direction?
					entity.Velocity.x = Mathf.Lerp(entity.Velocity.x, entity.NormalizedHorizontalSpeed * entity.RunSpeed, Time.deltaTime * smoothedMovementFactor);

					// apply gravity before moving
					entity.Velocity.y += entity.Gravity * Time.deltaTime;

					// // if holding down bump up our movement amount and turn off one way platform detection for a frame.
					// // this lets us jump down through one way platforms
					// if (entity.Controller.isGrounded && moveInput.y < 0f)
					// {
					// 	entity.Velocity.y *= 3f;
					// 	entity.Controller.ignoreOneWayPlatformsThisFrame = true;
					// }

					entity.Controller.move(entity.Velocity * Time.deltaTime);

					// grab our current entity.Velocity to use as a base for all calculations
					entity.Velocity = entity.Controller.velocity;
				}
			}

			_confirmWasPressedThisFrame = false;
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			_ui.HideGameplay();
			GameObject.Destroy(_state.Player.gameObject);
			GameObject.Destroy(_state.WallOfDeath.gameObject);

			_controls.Gameplay.Disable();
			_controls.Gameplay.Confirm.started -= ConfirmStarted;
		}

		private async void OnPlayerTriggerEnter(Collider2D col)
		{
			Debug.Log("OnPlayerTriggerEnter: " + col.gameObject.name);

			if (col.gameObject.tag == "Killbox")
			{
				await _ui.FadeIn(Color.black);
				_machine.Fire(GameFSM.Triggers.Defeat);
			}
			else if (col.gameObject.tag == "Exit")
			{
				await _ui.FadeIn(Color.white);
				_machine.Fire(GameFSM.Triggers.Victory);
			}

		}

		private void OnPlayerTriggerExit(Collider2D col)
		{
			// Debug.Log("OnPlayerTriggerExit: " + col.gameObject.name);
		}

		private void ConfirmStarted(InputAction.CallbackContext context) => _confirmWasPressedThisFrame = true;

		private Collider2D GetLevelConfiner()
		{
			return GameObject.Find("Camera Confiner").GetComponent<Collider2D>();
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

