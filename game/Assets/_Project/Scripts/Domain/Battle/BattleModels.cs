using System;
using System.Collections.Generic;

namespace Aetherion.Domain.Battle
{
    public sealed class SkillDef
    {
        public string Id { get; set; } = string.Empty;
        public string NameKey { get; set; } = string.Empty;
        public string Element { get; set; } = string.Empty;
        public int Power { get; set; }
    }

    public sealed class BattlerState
    {
        public string Id { get; set; } = string.Empty;
        public string DefId { get; set; } = string.Empty;
        public string NameKey { get; set; } = string.Empty;
        public string PrimaryElement { get; set; } = string.Empty;
        public int MaxHp { get; set; }
        public int Hp { get; set; }
        public int Atk { get; set; }
        public int Def { get; set; }
        public bool Guarding { get; set; }
        public IReadOnlyList<string> SkillIds { get; set; } = Array.Empty<string>();

        public bool IsFainted => Hp <= 0;
    }

    public sealed class BattleAction
    {
        public BattleActionType Type { get; set; }
        public string SkillId { get; set; }
    }

    public abstract class BattleEvent
    {
        public string Kind { get; protected set; }
    }

    public sealed class BattleStartedEvent : BattleEvent
    {
        public BattleStartedEvent() { Kind = "BattleStarted"; }
    }

    public sealed class WeatherAnnouncedEvent : BattleEvent
    {
        public string WeatherId { get; }
        public WeatherAnnouncedEvent(string weatherId)
        {
            Kind = "WeatherAnnounced";
            WeatherId = weatherId;
        }
    }

    public sealed class TurnStartedEvent : BattleEvent
    {
        public string Side { get; }
        public TurnStartedEvent(string side)
        {
            Kind = "TurnStarted";
            Side = side;
        }
    }

    public sealed class ActionSelectedEvent : BattleEvent
    {
        public string ActorId { get; }
        public string ActionLabel { get; }
        public ActionSelectedEvent(string actorId, string actionLabel)
        {
            Kind = "ActionSelected";
            ActorId = actorId;
            ActionLabel = actionLabel;
        }
    }

    public sealed class DamageAppliedEvent : BattleEvent
    {
        public string TargetId { get; }
        public int Amount { get; }
        public Effectiveness Effectiveness { get; }
        public float Multiplier { get; }
        public DamageAppliedEvent(string targetId, int amount, Effectiveness eff, float mult)
        {
            Kind = "DamageApplied";
            TargetId = targetId;
            Amount = amount;
            Effectiveness = eff;
            Multiplier = mult;
        }
    }

    public sealed class FaintedEvent : BattleEvent
    {
        public string ActorId { get; }
        public FaintedEvent(string actorId)
        {
            Kind = "Fainted";
            ActorId = actorId;
        }
    }

    public sealed class BattleEndedEvent : BattleEvent
    {
        public BattleResult Result { get; }
        public BattleEndedEvent(BattleResult result)
        {
            Kind = "BattleEnded";
            Result = result;
        }
    }

    public sealed class MessageEvent : BattleEvent
    {
        public string TextKey { get; }
        public MessageEvent(string textKey)
        {
            Kind = "Message";
            TextKey = textKey;
        }
    }
}
