using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Game.Core.Utils;

namespace Game.Core
{
	public class BootstrapState : BaseGameState
	{
		public BootstrapState(GameFSM machine, Game game) : base(machine, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			_audioPlayer.SetMusicVolume(_config.MusicVolume);
			_audioPlayer.SetSoundVolume(_config.SoundVolume);

			_state.CurrentLevel = _config.Levels[0];

			if (IsDevBuild())
			{
				_ui.ShowDebug();

				for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
				{
					if (sceneIndex > 0)
					{
						await SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(sceneIndex).name);
					}
				}
			}

			Time.timeScale = 1f;

			_machine.Fire(GameFSM.Triggers.Done);
		}
	}
}
