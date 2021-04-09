using Cysharp.Threading.Tasks;

namespace Game.Core
{
	public interface IState
	{
		UniTask Enter();
		UniTask Exit();
		void Tick();
		void FixedTick();
	}
}
