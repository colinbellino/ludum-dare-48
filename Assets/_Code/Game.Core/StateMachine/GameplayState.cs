﻿using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using System.Collections;
using static Game.Core.Utils;
using Cinemachine;
using System;

namespace Game.Core
{
	public partial class GameplayState : BaseGameState
	{
		private bool _confirmWasPressedThisFrame;
		private bool _cancelWasPressedThisFrame;
		private LevelScene _level;

		public GameplayState(GameFSM machine, Game game) : base(machine, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			await _ui.FadeIn(Color.black);

			_ui.SetDebugText("State: Gameplay\n\n[DEBUG MENU]\n- F1: Jump to next level\n- F2: Trigger game over");

			if (IsDevBuild() == false)
			{
				GameObject.Destroy(GameObject.Find("Player Cursor"));
				GameObject.Destroy(GameObject.Find("Dig Cursor"));
			}

			_level = await LoadLevel(_state.CurrentLevel);

			_state.TileHits.Clear();

			_state.Player = SpawnPlayer(_config.PlayerPrefab, _game, _level.PlayerStartPosition);
			_state.Player.Controller.onTriggerEnterEvent += OnPlayerTriggerEnter;
			_state.Player.Controller.onTriggerExitEvent += OnPlayerTriggerExit;

			_camera.VirtualCamera.Follow = _state.Player.transform;
			_camera.Confiner.m_BoundingShape2D = _level.CameraConfiner;
			await UniTask.Delay(300); // Small delay so the initial camera follow is not visible

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
					_ = Victory();
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
					var digPosition = entityPosition + entity.DigDirection;

					if (IsDevBuild())
					{
						GameObject.Find("Player Cursor").transform.position = entityPosition;
						GameObject.Find("Dig Cursor").transform.position = digPosition;
					}

					if (entity.Controller.isGrounded)
					{
						entity.Velocity.y = 0;
					}

					if (Time.time >= entity.DigAnimationEndTimestamp)
					{
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

							entity.DigDirection = new Vector3Int(1, 0, 0);
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

							entity.DigDirection = new Vector3Int(-1, 0, 0);
						}
						else
						{
							entity.NormalizedHorizontalSpeed = 0;

							if (entity.Controller.isGrounded)
							{
								entity.Animator?.Play(Animator.StringToHash("Idle"));
							}

							if (entity.transform.localScale.x > 0)
							{
								entity.DigDirection = new Vector3Int(1, 0, 0);
							}
							else
							{
								entity.DigDirection = new Vector3Int(-1, 0, 0);
							}
						}

						if (moveInput.y < 0f)
						{
							entity.DigDirection = new Vector3Int(0, -1, 0);
						}
						else if (moveInput.y > 0f)
						{
							entity.DigDirection = new Vector3Int(0, 1, 0);
						}

						// JUMP
						if (_confirmWasPressedThisFrame && entity.Controller.isGrounded)
						{
							entity.Velocity.y = Mathf.Sqrt(2f * entity.JumpHeight * -entity.Gravity);
							entity.Animator?.Play(Animator.StringToHash("Jump"));
						}

						// DIG
						if (_cancelWasPressedThisFrame && entity.Controller.isGrounded)
						{
							var tile = _level.PlatformTilemap.GetTile(digPosition);
							var tileData = GetTileData(_config.Tiles, tile);

							if (tileData != null)
							{
								entity.Animator?.Play(Animator.StringToHash("Dig Down"));
								entity.StartDiggingTimestamp = Time.time + 0.2f;
								entity.DigAnimationEndTimestamp = Time.time + 0.3f;
							}
						}

						// apply horizontal speed smoothing it. dont really do this with Lerp. Use SmoothDamp or something that provides more control
						var smoothedMovementFactor = entity.Controller.isGrounded ? entity.GroundDamping : entity.InAirDamping; // how fast do we change direction?
						entity.Velocity.x = Mathf.Lerp(entity.Velocity.x, entity.NormalizedHorizontalSpeed * entity.RunSpeed, Time.deltaTime * smoothedMovementFactor);
					}
					else
					{
						entity.Velocity.x = 0;
					}

					if (Time.time >= entity.StartDiggingTimestamp && entity.StartDiggingTimestamp > 0)
					{
						entity.StartDiggingTimestamp = 0;
						DigTile(entity);
					}

					// apply gravity before moving
					entity.Velocity.y += entity.Gravity * Time.deltaTime;

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

			GameObject.Destroy(_state.Player.gameObject);

			if (_state.WallOfDeath)
			{
				GameObject.Destroy(_state.WallOfDeath.gameObject);
			}

			_controls.Gameplay.Disable();
			_controls.Gameplay.Confirm.started -= ConfirmStarted;
			_controls.Gameplay.Cancel.started -= CancelStarted;
		}

		private async UniTask Shake(float gain, float duration)
		{
			var perlin = _camera.VirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
			perlin.m_AmplitudeGain = gain;
			await UniTask.Delay(TimeSpan.FromMilliseconds(duration));
			perlin.m_AmplitudeGain = 0f;
		}

		private void DigTile(EntityComponent entity)
		{
			var entityPosition = _level.PlatformTilemap.WorldToCell(entity.transform.position);
			var digPosition = entityPosition + entity.DigDirection;

			var tile = _level.PlatformTilemap.GetTile(digPosition);
			var tileData = GetTileData(_config.Tiles, tile);

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
					SpawnEffect(_config.TileBreakEffectPrefab, digPosition + new Vector3(0.5f, 0.5f, 0f));
				}

				_audioPlayer.PlayRandomSoundEffect(_config.DigClips);
				_ = Shake(1f, 0.1f);
			}
			else
			{
				if (tile != null)
				{
					_audioPlayer.PlayRandomSoundEffect(_config.ClingClips);
					_ = Shake(1f, 0.1f);
				}
			}
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
			// Make sure we enable overlay layer (we disable it sometimes during level design)
			var grid = GameObject.Find("Grid");
			var overlay = grid.transform.Find("Overlay");
			overlay.gameObject.SetActive(true);
			level.OverlayTilemap = overlay.GetComponent<Tilemap>();

			return level;
		}

		private async UniTask UnloadLevel(Level level)
		{
			await SceneManager.UnloadSceneAsync(level.SceneName);
		}
	}
}

