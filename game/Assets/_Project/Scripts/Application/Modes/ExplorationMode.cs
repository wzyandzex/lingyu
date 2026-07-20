using System;
using Aetherion.Application.Ports;

namespace Aetherion.Application.Modes
{
    public sealed class ExplorationMode : IGameMode
    {
        private readonly Action _onEnter;
        private readonly Action _onExit;

        public ExplorationMode(Action onEnter = null, Action onExit = null)
        {
            _onEnter = onEnter;
            _onExit = onExit;
        }

        public GameModeId Id => GameModeId.Exploration;

        public void Enter() => _onEnter?.Invoke();

        public void Exit() => _onExit?.Invoke();

        public void Tick(float deltaTime)
        {
        }
    }
}
