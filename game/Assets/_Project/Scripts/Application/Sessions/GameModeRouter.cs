using System;
using System.Collections.Generic;
using Aetherion.Application.Ports;

namespace Aetherion.Application.Sessions
{
    public sealed class GameModeRouter
    {
        private readonly Dictionary<GameModeId, IGameMode> _modes = new Dictionary<GameModeId, IGameMode>();
        private IGameMode _current;

        public GameModeId? CurrentId => _current?.Id;

        public void Register(IGameMode mode)
        {
            if (mode == null) throw new ArgumentNullException(nameof(mode));
            _modes[mode.Id] = mode;
        }

        public void SwitchTo(GameModeId id)
        {
            if (!_modes.TryGetValue(id, out var next))
                throw new InvalidOperationException($"Mode not registered: {id}");

            if (_current != null && ReferenceEquals(_current, next))
                return;

            _current?.Exit();
            _current = next;
            _current.Enter();
        }

        public void Tick(float deltaTime)
        {
            _current?.Tick(deltaTime);
        }
    }
}
