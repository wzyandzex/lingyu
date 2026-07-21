namespace Aetherion.Domain.Battle
{
    public enum Effectiveness
    {
        Neutral = 0,
        Super = 1,
        Resist = 2
    }

    public enum BattlePhase
    {
        AwaitingPlayer = 0,
        Resolving = 1,
        Ended = 2
    }

    public enum BattleResult
    {
        Ongoing = 0,
        PlayerWin = 1,
        PlayerLose = 2,
        Fled = 3
    }

    public enum BattleActionType
    {
        Skill = 0,
        Guard = 1,
        Flee = 2
    }
}
