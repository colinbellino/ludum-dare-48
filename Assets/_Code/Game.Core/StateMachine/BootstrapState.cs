using Cysharp.Threading.Tasks;
using UnityEngine;
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
			}

			Time.timeScale = 1f;

			_machine.Fire(GameFSM.Triggers.Done);
		}
	}
}
