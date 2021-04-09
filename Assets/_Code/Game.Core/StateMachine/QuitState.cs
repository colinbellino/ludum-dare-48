using Cysharp.Threading.Tasks;
using UnityEditor;

namespace Game.Core
{
	public class QuitState : BaseGameState
	{
		public QuitState(GameFSM machine, Game game) : base(machine, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
#else
			UnityEngine.Application.Quit();
#endif
		}
	}
}
