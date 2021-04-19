﻿using Cysharp.Threading.Tasks;
using UnityEngine;
using Game.Inputs;

namespace Game.Core
{
	public static class Utils
	{
		public static EntityComponent SpawnPlayer(EntityComponent prefab, Game game, Vector3 position)
		{
			var entity = GameObject.Instantiate(prefab, position, Quaternion.identity);
			entity.transform.name = "Player";
			entity.CurrentHealth = entity.BaseHealth;
			// entity.StateMachine = new UnitStateMachine(false, game, entity);
			// await entity.StateMachine.Start();
			return entity;
		}

		public static bool IsDevBuild()
		{
			return true;
			// #if UNITY_EDITOR
			// 			return true;
			// #endif

			// #pragma warning disable 162
			// 			return false;
			// #pragma warning restore 162
		}

		public static Vector3 GetMouseWorldPosition(GameControls controls, Camera camera)
		{
			var mousePosition = controls.Gameplay.MousePosition.ReadValue<Vector2>();
			var mouseWorldPosition = camera.ScreenToWorldPoint(mousePosition);
			mouseWorldPosition.z = 0f;
			return mouseWorldPosition;
		}
	}
}