using UnityEngine;

namespace Aetherion.Presentation.UI
{
    /// <summary>
    /// VS0 uses IMGUI for zero-asset HUD so the project plays without UI Toolkit assets.
    /// VS1+ can replace with UI Toolkit documents without touching Domain.
    /// </summary>
    public sealed class HudController : MonoBehaviour
    {
        private string _areaName = "翠语林海";
        private string _interactHint = string.Empty;
        private string _codexHint = "C 图志";
        private string _prompt = string.Empty;
        private float _promptUntil;

        public void SetAreaName(string name) => _areaName = name ?? string.Empty;

        public void SetInteractHint(string hint) => _interactHint = hint ?? string.Empty;

        public void SetCodexHint(string hint) => _codexHint = hint ?? string.Empty;

        public void ShowPrompt(string text, float seconds = 2.5f)
        {
            _prompt = text ?? string.Empty;
            _promptUntil = Time.unscaledTime + seconds;
        }

        private void OnGUI()
        {
            const int pad = 16;
            var areaStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.96f, 0.9f) }
            };
            var hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                normal = { textColor = new Color(1f, 0.92f, 0.55f) }
            };
            var promptStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 16,
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            GUI.Label(new Rect(pad, pad, 480, 32), _areaName, areaStyle);
            if (!string.IsNullOrEmpty(_codexHint))
                GUI.Label(new Rect(pad, pad + 28, 240, 24), _codexHint, hintStyle);

            if (!string.IsNullOrEmpty(_interactHint))
            {
                var w = 220;
                GUI.Label(new Rect((Screen.width - w) * 0.5f, Screen.height * 0.62f, w, 28), _interactHint, hintStyle);
            }

            if (!string.IsNullOrEmpty(_prompt) && Time.unscaledTime <= _promptUntil)
            {
                var width = Mathf.Min(640, Screen.width - 40);
                var height = 88f;
                var rect = new Rect((Screen.width - width) * 0.5f, Screen.height * 0.78f, width, height);
                GUI.Box(rect, _prompt, promptStyle);
            }
        }
    }
}
