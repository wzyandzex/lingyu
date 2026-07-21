using System;
using Aetherion.Application.Ports;

namespace Aetherion.Application.Modes
{
    public sealed class BattleMode : IGameMode
    {
        private readonly Action _onEnter;
        private readonly Action _onExit;
        private readonly Action<float> _onTick;

        public BattleMode(Action onEnter = null, Action onExit = null, Action<float> onTick = null)
        {
            _onEnter = onEnter;
            _onExit = onExit;
            _onTick = onTick;
        }

        public GameModeId Id => GameModeId.Battle;
        public void Enter() => _onEnter?.Invoke();
        public void Exit() => _onExit?.Invoke();
        public void Tick(float deltaTime) => _onTick?.Invoke(deltaTime);
    }
}
