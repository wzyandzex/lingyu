using System.Collections.Generic;
using System.Text;
using Aetherion.Domain.Battle;
using Aetherion.Presentation.Bootstrap;
using UnityEngine;

namespace Aetherion.Presentation.UI
{
    /// <summary>IMGUI battle UI — displays Domain events only, never recomputes damage.</summary>
    public sealed class BattleHud : MonoBehaviour
    {
        private readonly List<string> _log = new List<string>();
        private const int MaxLog = 6;

        public void ClearLog() => _log.Clear();

        public void AppendEvents(IReadOnlyList<BattleEvent> events, GameBootstrap boot)
        {
            if (events == null || boot == null) return;
            foreach (var e in events)
            {
                var line = FormatEvent(e, boot);
                if (!string.IsNullOrEmpty(line))
                {
                    _log.Add(line);
                    while (_log.Count > MaxLog)
                        _log.RemoveAt(0);
                }
            }
        }

        private void OnGUI()
        {
            var boot = GameBootstrap.Instance;
            var sim = boot != null ? boot.ActiveBattle : null;
            if (boot == null || sim == null) return;

            var w = Mathf.Min(700, Screen.width - 20);
            var panel = new Rect((Screen.width - w) * 0.5f, 20, w, Screen.height - 40);
            GUI.Box(panel, boot.Localize("battle.ui.title", "共鸣试炼"));

            // Enemy
            var ey = panel.y + 28;
            GUI.Label(new Rect(panel.x + 16, ey, w - 32, 22),
                boot.Localize(sim.Enemy.NameKey, sim.Enemy.DefId) + "  [" + sim.Enemy.PrimaryElement + "]");
            DrawHpBar(new Rect(panel.x + 16, ey + 24, w - 32, 18), sim.Enemy.Hp, sim.Enemy.MaxHp, new Color(0.85f, 0.35f, 0.3f));

            // Log
            var logY = ey + 56;
            var sb = new StringBuilder();
            foreach (var line in _log)
                sb.AppendLine(line);
            GUI.Box(new Rect(panel.x + 16, logY, w - 32, 120), sb.ToString());

            // Player
            var py = logY + 130;
            GUI.Label(new Rect(panel.x + 16, py, w - 32, 22),
                boot.Localize(sim.Player.NameKey, sim.Player.DefId) + "  [" + sim.Player.PrimaryElement + "]");
            DrawHpBar(new Rect(panel.x + 16, py + 24, w - 32, 18), sim.Player.Hp, sim.Player.MaxHp, new Color(0.35f, 0.7f, 0.9f));

            if (sim.Phase != BattlePhase.AwaitingPlayer || sim.Result != BattleResult.Ongoing)
            {
                if (sim.Result != BattleResult.Ongoing)
                {
                    if (GUI.Button(new Rect(panel.x + w * 0.5f - 60, py + 60, 120, 32),
                            boot.Localize("battle.ui.continue", "继续")))
                        boot.EndBattleReturnToExplore();
                }
                return;
            }

            var by = py + 55;
            var skills = sim.Player.SkillIds;
            var bw = (w - 48) / 2f;
            for (var i = 0; i < skills.Count && i < 2; i++)
            {
                var sid = skills[i];
                var label = sid;
                var preview = "";
                if (boot.Session.DataCatalog.TryGetSkill(sid, out var sk))
                {
                    label = boot.Localize(sk.NameKey, sid) + " · " + sk.Element;
                    var mult = sim.PreviewMultiplier(sid);
                    preview = "\n" + EffLabel(ElementMatrix.Classify(ElementMatrix.GetMultiplier(sk.Element, sim.Enemy.PrimaryElement)), boot);
                    if (mult > ElementMatrix.GetMultiplier(sk.Element, sim.Enemy.PrimaryElement) + 0.01f)
                        preview += " · 雾";
                }
                var rect = new Rect(panel.x + 16 + i * (bw + 8), by, bw, 48);
                if (GUI.Button(rect, (i + 1) + ". " + label + preview))
                    boot.SubmitBattleSkill(sid);
            }

            var row2 = by + 56;
            if (GUI.Button(new Rect(panel.x + 16, row2, bw, 32),
                    boot.Localize("battle.ui.guard", "G 防守")))
                boot.SubmitBattleGuard();
            if (GUI.Button(new Rect(panel.x + 24 + bw, row2, bw, 32),
                    boot.Localize("battle.ui.flee", "R 撤离")))
                boot.SubmitBattleFlee();
        }

        private static void DrawHpBar(Rect r, int hp, int maxHp, Color c)
        {
            GUI.Box(r, "");
            var t = maxHp <= 0 ? 0f : Mathf.Clamp01(hp / (float)maxHp);
            var fill = new Rect(r.x + 2, r.y + 2, (r.width - 4) * t, r.height - 4);
            var old = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(fill, Texture2D.whiteTexture);
            GUI.color = old;
            GUI.Label(r, " " + hp + " / " + maxHp);
        }

        private static string FormatEvent(BattleEvent e, GameBootstrap boot)
        {
            if (e is MessageEvent m)
                return boot.Localize(m.TextKey, m.TextKey);
            if (e is WeatherAnnouncedEvent w)
                return boot.Localize("battle.msg.weather_fog", "林雾弥漫…");
            if (e is DamageAppliedEvent d)
            {
                var who = d.TargetId == BattleSimulator.EnemyId
                    ? boot.Localize("battle.ui.enemy", "敌方")
                    : boot.Localize("battle.ui.ally", "我方");
                return who + " -" + d.Amount + "  " + EffLabel(d.Effectiveness, boot);
            }
            if (e is ActionSelectedEvent a)
            {
                if (a.ActionLabel == "guard") return boot.Localize("battle.msg.guard", "摆出守势");
                if (a.ActionLabel == "flee") return boot.Localize("battle.msg.fled", "撤离试炼");
                if (boot.Session.DataCatalog.TryGetSkill(a.ActionLabel, out var sk))
                    return boot.Localize(sk.NameKey, a.ActionLabel);
                return a.ActionLabel;
            }
            if (e is FaintedEvent)
                return "…";
            if (e is BattleEndedEvent)
                return "";
            return null;
        }

        private static string EffLabel(Effectiveness eff, GameBootstrap boot)
        {
            switch (eff)
            {
                case Effectiveness.Super:
                    return boot.Localize("battle.eff.super", "效果绝佳");
                case Effectiveness.Resist:
                    return boot.Localize("battle.eff.resist", "效果不理想");
                default:
                    return boot.Localize("battle.eff.neutral", "效果一般");
            }
        }
    }
}
