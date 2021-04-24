using UnityEngine;

namespace Game.Core
{
	public partial class GameplayState
	{
		private class Level
		{
			public Vector3 PlayerStartPosition;
			public Vector3 WallOfDeathStartPosition;
			public Collider2D CameraConfiner;
		}
	}
}

