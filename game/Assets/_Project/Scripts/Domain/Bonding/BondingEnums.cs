namespace Aetherion.Domain.Bonding
{
    public enum BondingState
    {
        Encountered = 0,
        Reading = 1,
        Approaching = 2,
        Testing = 3,
        ResonanceWindow = 4,
        Success = 5,
        Failed = 6,
        Aborted = 7
    }

    public enum BondingIntent
    {
        Observe = 0,
        HoldStill = 1,
        MoveCloser = 2,
        MatchStep = 3,
        ConfirmResonance = 4,
        Cancel = 5
    }

    public enum BondingPhase
    {
        Walk = 0,
        Hold = 1
    }

    public enum BondingFailCode
    {
        None = 0,
        TooFast = 1,
        Pressure = 2,
        Desync = 3,
        WindowMiss = 4
    }
}
