using Cysharp.Threading.Tasks;
using UnityEngine;
using Game.Inputs;
using UnityEngine.Tilemaps;

namespace Game.Core
{
	public static class Utils
	{
		public static EntityComponent SpawnPlayer(EntityComponent prefab, Game game, Vector3 position)
		{
			var entity = GameObject.Instantiate(prefab, position, Quaternion.identity);
			entity.transform.name = "Player";
			// entity.StateMachine = new UnitStateMachine(false, game, entity);
			// await entity.StateMachine.Start();
			return entity;
		}

		public static EntityComponent SpawnSaw(EntityComponent prefab, int index, Vector3 position)
		{
			var entity = GameObject.Instantiate(prefab, position, Quaternion.identity);
			entity.transform.name = "Saw " + index;
			entity.DigDirection = new Vector3Int(0, -2, 0);
			// entity.StateMachine = new UnitStateMachine(false, game, entity);
			// await entity.StateMachine.Start();
			return entity;
		}

		public static ParticleSystem SpawnEffect(ParticleSystem effectPrefab, Vector3 position)
		{
			return GameObject.Instantiate(effectPrefab, position, Quaternion.identity);
		}

		public static TileData GetTileData(TileData[] tiles, TileBase tile)
		{
			for (int tileIndex = 0; tileIndex < tiles.Length; tileIndex++)
			{
				if (tiles[tileIndex].Tile == tile)
				{
					return tiles[tileIndex];
				}
			}

			return null;
		}

		public static bool IsDevBuild()
		{
#if UNITY_EDITOR
			return true;
#endif

#pragma warning disable 162
			return false;
#pragma warning restore 162
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
