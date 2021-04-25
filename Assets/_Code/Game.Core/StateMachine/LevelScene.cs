using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Core
{
	public class LevelScene
	{
		public Vector3 PlayerStartPosition;
		public Vector3 WallOfDeathStartPosition;
		public Collider2D CameraConfiner;
		public Tilemap PlatformTilemap;
	}
}
