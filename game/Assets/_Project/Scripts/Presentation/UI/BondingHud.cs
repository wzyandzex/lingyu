using Aetherion.Domain.Bonding;
using Aetherion.Presentation.Bootstrap;
using UnityEngine;

namespace Aetherion.Presentation.UI
{
    /// <summary>Large, readable beat coaching. No percentages of attune/guard.</summary>
    public sealed class BondingHud : MonoBehaviour
    {
        private string _fail;
        private float _failUntil;

        public void ShowFail(string humanText, float seconds = 2.5f)
        {
            _fail = humanText ?? string.Empty;
            _failUntil = Time.unscaledTime + seconds;
        }

        private void OnGUI()
        {
            var session = GameBootstrap.Session?.World?.ActiveBonding;
            var boot = GameBootstrap.Instance;
            if (boot == null) return;

            if (!string.IsNullOrEmpty(_fail) && Time.unscaledTime <= _failUntil)
                DrawCenteredBox(Screen.height * 0.7f, 52, _fail, 18);

            if (session == null || session.IsTerminal)
                return;

            var stage = StageLabel(session.State, boot);
            var hint = StageHint(session.State, session.Phase, boot);

            // Top banner
            DrawCenteredBox(40, 70, stage + "\n" + hint, 16);

            if (session.State == BondingState.Testing)
            {
                var expect = session.Phase == BondingPhase.Walk
                    ? "▶▶  现在：跟着走（按住 W）"
                    : "■   现在：停下（松开 WASD）";
                var colorNote = session.Phase == BondingPhase.Walk
                    ? "雾衔在动 / 身体发亮绿"
                    : "雾衔停住 / 颜色变浅";
                var bar = Mathf.Clamp01(session.PhaseRemaining01);
                DrawCenteredBox(120, 100,
                    expect + "\n" + colorNote + "\n本拍剩余 " + bar.ToString("0.0") + " 秒 · 对了 " +
                    session.TestingCorrect + "/" + BondingThresholds.TestingCorrectPhasesNeeded + " 拍",
                    20);
            }
            else if (session.State == BondingState.ResonanceWindow)
            {
                DrawCenteredBox(120, 64, "✨ 按  F  伸出掌心澄气（别冲）", 22);
            }
            else if (session.State == BondingState.Reading)
            {
                DrawCenteredBox(120, 50, "先完全停下 1～2 秒", 18);
            }
            else if (session.State == BondingState.Approaching)
            {
                DrawCenteredBox(120, 50, "轻轻按住 W 走近，不要冲刺", 18);
            }
        }

        private static void DrawCenteredBox(float y, float h, string text, int fontSize)
        {
            var w = Mathf.Min(640, Screen.width - 24);
            var rect = new Rect((Screen.width - w) * 0.5f, y, w, h);
            var style = new GUIStyle(GUI.skin.box)
            {
                fontSize = fontSize,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                normal = { textColor = Color.white }
            };
            GUI.Box(rect, text, style);
        }

        private static string StageLabel(BondingState state, GameBootstrap boot)
        {
            switch (state)
            {
                case BondingState.Reading:
                    return boot.Localize("ui.bonding.step.reading", "① 观察");
                case BondingState.Approaching:
                    return boot.Localize("ui.bonding.step.approaching", "② 缓近");
                case BondingState.Testing:
                    return boot.Localize("ui.bonding.step.testing", "③ 静随节奏");
                case BondingState.ResonanceWindow:
                    return boot.Localize("ui.bonding.step.resonance", "④ 共鸣");
                default:
                    return "结契";
            }
        }

        private static string StageHint(BondingState state, BondingPhase phase, GameBootstrap boot)
        {
            switch (state)
            {
                case BondingState.Reading:
                    return "松开所有方向键，站着听";
                case BondingState.Approaching:
                    return "慢慢靠近雾衔，保持距离别贴脸";
                case BondingState.Testing:
                    return phase == BondingPhase.Walk
                        ? "看雾衔：动了你就走，停了你就停"
                        : "看雾衔：停住时你也停住";
                case BondingState.ResonanceWindow:
                    return "窗口只有几秒 — 按 F";
                default:
                    return "";
            }
        }
    }
}
