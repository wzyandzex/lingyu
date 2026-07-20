using Aetherion.Application.Ports;

namespace Aetherion.Application.Modes
{
    /// <summary>Placeholder modes for later slices. Enter only logs via optional callback.</summary>
    public sealed class StubMode : IGameMode
    {
        private readonly GameModeId _id;
        private readonly System.Action<GameModeId> _onEnter;

        public StubMode(GameModeId id, System.Action<GameModeId> onEnter = null)
        {
            _id = id;
            _onEnter = onEnter;
        }

        public GameModeId Id => _id;

        public void Enter() => _onEnter?.Invoke(_id);

        public void Exit()
        {
        }

        public void Tick(float deltaTime)
        {
        }
    }
}
