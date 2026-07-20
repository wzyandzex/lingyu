using Aetherion.Domain.Bonding;
using Aetherion.Presentation.Bootstrap;
using UnityEngine;

namespace Aetherion.Presentation.UI
{
    /// <summary>Bonding HUD: stage line, Walk/Hold expectation, fail copy. No percentages.</summary>
    public sealed class BondingHud : MonoBehaviour
    {
        private string _fail;
        private float _failUntil;

        public void ShowFail(string humanText, float seconds = 2f)
        {
            _fail = humanText ?? string.Empty;
            _failUntil = Time.unscaledTime + seconds;
        }

        private void OnGUI()
        {
            var session = GameBootstrap.Session?.World?.ActiveBonding;
            if (session == null || session.IsTerminal)
            {
                if (!string.IsNullOrEmpty(_fail) && Time.unscaledTime <= _failUntil)
                    DrawFail();
                return;
            }

            var boot = GameBootstrap.Instance;
            if (boot == null) return;

            var stage = StageLabel(session.State, boot);
            var hint = StageHint(session.State, session.Phase, boot);

            var box = new Rect(Screen.width * 0.5f - 280, 48, 560, session.State == BondingState.Testing ? 88 : 64);
            GUI.Box(box, "");
            GUI.Label(new Rect(box.x + 16, box.y + 10, box.width - 32, 24), stage);
            GUI.Label(new Rect(box.x + 16, box.y + 34, box.width - 32, 24), hint);

            if (session.State == BondingState.Testing)
            {
                var expect = session.Phase == BondingPhase.Walk
                    ? boot.Localize("ui.bonding.expect.walk", "跟着走")
                    : boot.Localize("ui.bonding.expect.hold", "停下");
                GUI.Label(new Rect(box.x + 16, box.y + 56, box.width - 32, 24),
                    ">>> " + expect + " <<<");
            }

            if (session.State == BondingState.ResonanceWindow)
            {
                GUI.Label(new Rect(box.x + 16, box.y + 34, box.width - 32, 24),
                    boot.Localize("ui.bonding.hint.confirm", "按 F 伸出掌心澄气"));
            }

            if (!string.IsNullOrEmpty(_fail) && Time.unscaledTime <= _failUntil)
                DrawFail();
        }

        private void DrawFail()
        {
            var r = new Rect(Screen.width * 0.5f - 240, Screen.height * 0.72f, 480, 40);
            GUI.Box(r, _fail);
        }

        private static string StageLabel(BondingState state, GameBootstrap boot)
        {
            switch (state)
            {
                case BondingState.Reading:
                    return boot.Localize("ui.bonding.step.reading", "观察 · 停步侧耳");
                case BondingState.Approaching:
                    return boot.Localize("ui.bonding.step.approaching", "缓缓靠近");
                case BondingState.Testing:
                    return boot.Localize("ui.bonding.step.testing", "静随 · 跟上节奏");
                case BondingState.ResonanceWindow:
                    return boot.Localize("ui.bonding.step.resonance", "共鸣时刻");
                default:
                    return "结契";
            }
        }

        private static string StageHint(BondingState state, BondingPhase phase, GameBootstrap boot)
        {
            switch (state)
            {
                case BondingState.Reading:
                    return boot.Localize("ui.bonding.hint.reading", "先停住，听它呼吸");
                case BondingState.Approaching:
                    return boot.Localize("ui.bonding.hint.approaching", "慢慢走近，不要冲");
                case BondingState.Testing:
                    return phase == BondingPhase.Walk
                        ? boot.Localize("ui.bonding.hint.walk", "它在走 — 你也轻轻跟上")
                        : boot.Localize("ui.bonding.hint.hold", "它停了 — 你也停下");
                case BondingState.ResonanceWindow:
                    return boot.Localize("ui.bonding.hint.confirm", "按 F 伸出掌心澄气");
                default:
                    return "";
            }
        }
    }
}
