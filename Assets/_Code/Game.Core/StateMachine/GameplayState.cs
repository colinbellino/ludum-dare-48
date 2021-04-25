﻿using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using static Game.Core.Utils;

namespace Game.Core
{
	public partial class GameplayState : BaseGameState
	{
		private bool _confirmWasPressedThisFrame;
		private bool _cancelWasPressedThisFrame;
		private LevelScene _level;
		private Vector3Int _digDirection = new Vector3Int(0, -1, 0);

		public GameplayState(GameFSM machine, Game game) : base(machine, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			await _ui.FadeIn(Color.black);

			_ui.SetDebugText("State: Gameplay\n\n[DEBUG MENU]\n- F1: Jump to next level\n- F2: Trigger game over");

			if (IsDevBuild() == false)
			{
				Object.Destroy(GameObject.Find("Player Cursor"));
				Object.Destroy(GameObject.Find("Dig Cursor"));
			}

			_level = await LoadLevel(_state.CurrentLevel);

			_state.TileHits.Clear();

			_state.Player = SpawnPlayer(_config.PlayerPrefab, _game, _level.PlayerStartPosition);
			_state.Player.Controller.onTriggerEnterEvent += OnPlayerTriggerEnter;
			_state.Player.Controller.onTriggerExitEvent += OnPlayerTriggerExit;

			_camera.VirtualCamera.Follow = _state.Player.transform;
			_camera.Confiner.m_BoundingShape2D = _level.CameraConfiner;

			if (_state.CurrentLevel.Safe == false)
			{
				_state.WallOfDeath = SpawnWallOfDeath(_config.WallOfDeathPrefab, _game, _level.WallOfDeathStartPosition);
			}

			_controls.Gameplay.Enable();
			_controls.Gameplay.Confirm.started += ConfirmStarted;
			_controls.Gameplay.Cancel.started += CancelStarted;

			_state.Running = true;

			_ = _audioPlayer.PlayMusic(_state.CurrentLevel.Music, true, 1f);
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

					var entityPosition = _level.PlatformTilemap.WorldToCell(entity.transform.position);
					var digPosition = entityPosition + _digDirection;

					if (IsDevBuild())
					{
						GameObject.Find("Player Cursor").transform.position = entityPosition;
						GameObject.Find("Dig Cursor").transform.position = digPosition;
					}

					if (entity.Controller.isGrounded && _cancelWasPressedThisFrame)
					{
						var tile = _level.PlatformTilemap.GetTile(digPosition);
						var tileData = GetTileData(_config.Tiles, tile);

						if (tileData != null)
						{
							if (tileData.Breakable)
							{
								if (_state.TileHits.ContainsKey(digPosition) == false)
								{
									_state.TileHits[digPosition] = tileData.HitsToBreak;
								}

								_state.TileHits[digPosition] -= _state.GauntlerPower;

								var damageTile = _state.TileHits[digPosition] switch
								{
									3 => _config.DamageOverlays[0],
									2 => _config.DamageOverlays[1],
									1 => _config.DamageOverlays[2],
									_ => null
								};
								_level.OverlayTilemap.SetTile(digPosition, damageTile);

								if (_state.TileHits[digPosition] <= 0)
								{
									_level.PlatformTilemap.SetTile(digPosition, null);
								}
							}
							else
							{
								// TODO: play cling sound
							}

							// TODO: play dig sound
							entity.Animator?.Play(Animator.StringToHash("Dig"));
						}
					}

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
			_cancelWasPressedThisFrame = false;
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			_ui.HideGameplay();

			_level = null;

			Object.Destroy(_state.Player.gameObject);

			if (_state.WallOfDeath)
			{
				Object.Destroy(_state.WallOfDeath.gameObject);
			}

			_controls.Gameplay.Disable();
			_controls.Gameplay.Confirm.started -= ConfirmStarted;
			_controls.Gameplay.Cancel.started -= CancelStarted;
		}

		private async void OnPlayerTriggerEnter(Collider2D col)
		{
			// Debug.Log("OnPlayerTriggerEnter: " + col.gameObject.name);

			if (col.gameObject.tag == "Killbox")
			{
				Defeat();
			}
			else if (col.gameObject.tag == "Exit")
			{
				await Victory();
			}
		}

		private void OnPlayerTriggerExit(Collider2D col)
		{
			// Debug.Log("OnPlayerTriggerExit: " + col.gameObject.name);
		}

		private void ConfirmStarted(InputAction.CallbackContext context) => _confirmWasPressedThisFrame = true;

		private void CancelStarted(InputAction.CallbackContext context) => _cancelWasPressedThisFrame = true;

		private async UniTask Victory()
		{
			var index = System.Array.IndexOf(_config.Levels, _state.CurrentLevel);
			if (index < _config.Levels.Length - 1)
			{
				_ = _audioPlayer.StopMusic(1f);
				await _ui.FadeIn(Color.black);
				_ = UnloadLevel(_state.CurrentLevel);

				_machine.Fire(GameFSM.Triggers.NextLevel);

				_state.CurrentLevel = _config.Levels[index + 1];
			}
			else
			{
				_ = _audioPlayer.StopMusic(1f);
				await _ui.FadeIn(Color.white);
				_ = UnloadLevel(_state.CurrentLevel);

				_machine.Fire(GameFSM.Triggers.Victory);
			}
		}

		private async void Defeat()
		{
			_ = _audioPlayer.StopMusic(0.5f);
			await _ui.FadeIn(Color.black);

			_ = UnloadLevel(_state.CurrentLevel);

			_machine.Fire(GameFSM.Triggers.Defeat);
		}

		private static async UniTask<LevelScene> LoadLevel(Level data)
		{
			await SceneManager.LoadSceneAsync(data.SceneName, LoadSceneMode.Additive);

			var level = new LevelScene();
			level.PlayerStartPosition = GameObject.Find("Player Start").transform.position;
			level.WallOfDeathStartPosition = data.WallOfDeathStartPosition;
			level.CameraConfiner = GameObject.Find("Camera Confiner").GetComponent<Collider2D>();
			level.PlatformTilemap = GameObject.Find("Platform").GetComponent<Tilemap>();
			level.OverlayTilemap = GameObject.Find("Overlay").GetComponent<Tilemap>();

			return level;
		}

		private async UniTask UnloadLevel(Level level)
		{
			await SceneManager.UnloadSceneAsync(level.SceneName);
		}
	}
}

