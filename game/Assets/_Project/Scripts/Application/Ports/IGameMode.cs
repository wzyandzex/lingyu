namespace Aetherion.Application.Ports
{
    public enum GameModeId
    {
        Title = 0,
        Exploration = 1,
        Bonding = 2,
        Battle = 3,
        Dialogue = 4,
        Menu = 5,
        Cutscene = 6
    }

    public interface IGameMode
    {
        GameModeId Id { get; }
        void Enter();
        void Exit();
        void Tick(float deltaTime);
    }
}
