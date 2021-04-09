using System.Collections.Generic;
using Stateless;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Core
{
	public class GameFSM
	{
		public enum States { Bootstrap, Gameplay, Victory, Defeat, Quit }
		public enum Triggers { Done, Victory, Defeat, Retry, Quit }

		private readonly bool _debug;
		private readonly Dictionary<States, IState> _states;
		private readonly StateMachine<States, Triggers> _machine;
		private IState _currentState;

		public GameFSM(bool debug, Game game)
		{
			Assert.IsNotNull(game);

			_debug = debug;
			_states = new Dictionary<States, IState>
			{
				{ States.Bootstrap, new BootstrapState(this, game) },
				{ States.Gameplay, new GameplayState(this, game) },
				{ States.Victory, new VictoryState(this, game) },
				{ States.Defeat, new DefeatState(this, game) },
				{ States.Quit, new QuitState(this, game) },
			};

			_machine = new StateMachine<States, Triggers>(States.Bootstrap);
			_machine.OnTransitioned(OnTransitioned);

			_machine.Configure(States.Bootstrap)
				.Permit(Triggers.Done, States.Gameplay);

			_machine.Configure(States.Gameplay)
				.Permit(Triggers.Victory, States.Victory)
				.Permit(Triggers.Defeat, States.Defeat);

			_machine.Configure(States.Victory)
				.Permit(Triggers.Retry, States.Gameplay)
				.Permit(Triggers.Quit, States.Quit);

			_machine.Configure(States.Defeat)
				.Permit(Triggers.Retry, States.Gameplay)
				.Permit(Triggers.Quit, States.Quit);

			_currentState = _states[_machine.State];
		}

		public async void Start()
		{
			await _currentState.Enter();
		}

		public void Tick() => _currentState?.Tick();

		public void FixedTick() => _currentState?.FixedTick();

		public void Fire(Triggers trigger)
		{
			if (_machine.CanFire(trigger))
			{
				_machine.Fire(trigger);
			}
			else
			{
				Debug.LogWarning("Invalid transition " + _currentState + " -> " + trigger);
			}
		}

		private async void OnTransitioned(StateMachine<States, Triggers>.Transition transition)
		{
			if (_currentState != null)
			{
				await _currentState.Exit();
			}

			if (_debug)
			{
				if (_states.ContainsKey(transition.Destination) == false)
				{
					UnityEngine.Debug.LogError("Missing state class for: " + transition.Destination);
				}
			}

			_currentState = _states[transition.Destination];
			if (_debug)
			{
				UnityEngine.Debug.Log($"{transition.Source} -> {transition.Destination}");
			}

			await _currentState.Enter();
		}
	}
}
