using System;
using System.Collections.Generic;

namespace Aetherion.Domain.Battle
{
    /// <summary>Deterministic 1v1 teaching battle. Player always acts first.</summary>
    public sealed class BattleSimulator
    {
        public const string PlayerId = "player";
        public const string EnemyId = "enemy";
        public const string WeatherFog = "fog";

        private readonly Dictionary<string, SkillDef> _skills;

        public BattlerState Player { get; }
        public BattlerState Enemy { get; }
        public BattlePhase Phase { get; private set; }
        public BattleResult Result { get; private set; }
        public string WeatherId { get; }
        public IReadOnlyList<BattleEvent> LastEvents => _lastEvents;

        private readonly List<BattleEvent> _lastEvents = new List<BattleEvent>();

        public BattleSimulator(
            BattlerState player,
            BattlerState enemy,
            IEnumerable<SkillDef> skills,
            string weatherId = WeatherFog)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Enemy = enemy ?? throw new ArgumentNullException(nameof(enemy));
            WeatherId = weatherId ?? WeatherFog;
            _skills = new Dictionary<string, SkillDef>(StringComparer.Ordinal);
            if (skills != null)
            {
                foreach (var s in skills)
                {
                    if (s != null && !string.IsNullOrEmpty(s.Id))
                        _skills[s.Id] = s;
                }
            }
            Phase = BattlePhase.AwaitingPlayer;
            Result = BattleResult.Ongoing;
        }

        public IReadOnlyList<BattleEvent> Begin()
        {
            _lastEvents.Clear();
            _lastEvents.Add(new BattleStartedEvent());
            _lastEvents.Add(new WeatherAnnouncedEvent(WeatherId));
            _lastEvents.Add(new MessageEvent("battle.msg.intro"));
            _lastEvents.Add(new TurnStartedEvent(PlayerId));
            Phase = BattlePhase.AwaitingPlayer;
            return _lastEvents;
        }

        public IReadOnlyList<BattleEvent> SubmitPlayerAction(BattleAction action)
        {
            _lastEvents.Clear();
            if (Phase != BattlePhase.AwaitingPlayer || Result != BattleResult.Ongoing)
                return _lastEvents;

            Phase = BattlePhase.Resolving;
            Player.Guarding = false;
            Enemy.Guarding = false;

            if (action == null)
                action = new BattleAction { Type = BattleActionType.Guard };

            if (action.Type == BattleActionType.Flee)
            {
                _lastEvents.Add(new ActionSelectedEvent(PlayerId, "flee"));
                _lastEvents.Add(new MessageEvent("battle.msg.fled"));
                Result = BattleResult.Fled;
                Phase = BattlePhase.Ended;
                _lastEvents.Add(new BattleEndedEvent(Result));
                return _lastEvents;
            }

            if (action.Type == BattleActionType.Guard)
            {
                Player.Guarding = true;
                _lastEvents.Add(new ActionSelectedEvent(PlayerId, "guard"));
                _lastEvents.Add(new MessageEvent("battle.msg.guard"));
            }
            else
            {
                ResolveSkill(Player, Enemy, action.SkillId);
                if (Enemy.IsFainted)
                {
                    _lastEvents.Add(new FaintedEvent(EnemyId));
                    _lastEvents.Add(new MessageEvent("battle.msg.win"));
                    Result = BattleResult.PlayerWin;
                    Phase = BattlePhase.Ended;
                    _lastEvents.Add(new BattleEndedEvent(Result));
                    return _lastEvents;
                }
            }

            // Enemy turn — always first skill
            _lastEvents.Add(new TurnStartedEvent(EnemyId));
            var enemySkill = Enemy.SkillIds != null && Enemy.SkillIds.Count > 0
                ? Enemy.SkillIds[0]
                : null;
            ResolveSkill(Enemy, Player, enemySkill);
            if (Player.IsFainted)
            {
                _lastEvents.Add(new FaintedEvent(PlayerId));
                _lastEvents.Add(new MessageEvent("battle.msg.lose"));
                Result = BattleResult.PlayerLose;
                Phase = BattlePhase.Ended;
                _lastEvents.Add(new BattleEndedEvent(Result));
                return _lastEvents;
            }

            _lastEvents.Add(new TurnStartedEvent(PlayerId));
            Phase = BattlePhase.AwaitingPlayer;
            return _lastEvents;
        }

        public static int ComputeDamage(
            int power,
            int atk,
            int def,
            string skillElement,
            string targetElement,
            string weatherId,
            bool targetGuarding)
        {
            var raw = power * (float)atk / Math.Max(1, def);
            var mult = ElementMatrix.GetMultiplier(skillElement, targetElement);
            if (string.Equals(weatherId, WeatherFog, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(skillElement, ElementMatrix.Tidal, StringComparison.OrdinalIgnoreCase))
            {
                mult *= 1.1f;
            }
            raw *= mult;
            if (targetGuarding)
                raw *= 0.5f;
            return Math.Max(1, (int)Math.Floor(raw));
        }

        public float PreviewMultiplier(string skillId)
        {
            if (string.IsNullOrEmpty(skillId) || !_skills.TryGetValue(skillId, out var skill))
                return 1f;
            var mult = ElementMatrix.GetMultiplier(skill.Element, Enemy.PrimaryElement);
            if (string.Equals(WeatherId, WeatherFog, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(skill.Element, ElementMatrix.Tidal, StringComparison.OrdinalIgnoreCase))
                mult *= 1.1f;
            return mult;
        }

        private void ResolveSkill(BattlerState user, BattlerState target, string skillId)
        {
            if (string.IsNullOrEmpty(skillId) || !_skills.TryGetValue(skillId, out var skill))
            {
                _lastEvents.Add(new ActionSelectedEvent(user.Id, "struggle"));
                ApplyDamage(user, target, 5, Effectiveness.Neutral, 1f);
                return;
            }

            _lastEvents.Add(new ActionSelectedEvent(user.Id, skill.Id));
            var mult = ElementMatrix.GetMultiplier(skill.Element, target.PrimaryElement);
            if (string.Equals(WeatherId, WeatherFog, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(skill.Element, ElementMatrix.Tidal, StringComparison.OrdinalIgnoreCase))
                mult *= 1.1f;
            var dmg = ComputeDamage(skill.Power, user.Atk, target.Def, skill.Element, target.PrimaryElement,
                WeatherId, target.Guarding);
            var eff = ElementMatrix.Classify(ElementMatrix.GetMultiplier(skill.Element, target.PrimaryElement));
            // Classify on type chart without fog so "super" still shows for tidal vs pyric
            ApplyDamage(user, target, dmg, eff, mult);
            target.Guarding = false;
        }

        private void ApplyDamage(BattlerState user, BattlerState target, int dmg, Effectiveness eff, float mult)
        {
            target.Hp = Math.Max(0, target.Hp - dmg);
            _lastEvents.Add(new DamageAppliedEvent(target.Id, dmg, eff, mult));
        }
    }
}
