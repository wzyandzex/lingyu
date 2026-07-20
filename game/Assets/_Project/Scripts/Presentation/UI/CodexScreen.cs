using System.Collections.Generic;
using System.Linq;
using Aetherion.Domain.Codex;
// Linq used for catalog filtering
using Aetherion.Domain.Creatures;
using Aetherion.Presentation.Bootstrap;
using Aetherion.Presentation.Player;
using UnityEngine;

namespace Aetherion.Presentation.UI
{
    /// <summary>IMGUI codex panel. List = R01 catalog ∪ unlocked; unknown hides real names.</summary>
    public sealed class CodexScreen : MonoBehaviour
    {
        private bool _open;
        private int _selected;
        private Vector2 _scroll;
        private PlayerMotor _motor;

        public bool IsOpen => _open;

        public void Toggle()
        {
            if (_open) Close();
            else Open();
        }

        public void Open()
        {
            _open = true;
            _motor = Object.FindObjectOfType<PlayerMotor>();
            if (_motor != null)
                _motor.SetMovementLocked(true);
        }

        public void Close()
        {
            _open = false;
            if (_motor != null)
                _motor.SetMovementLocked(false);
        }

        private void Update()
        {
            if (!_open) return;
            if (Input.GetKeyDown(KeyCode.Escape))
                Close();
            if (Input.GetKeyDown(KeyCode.UpArrow))
                _selected = Mathf.Max(0, _selected - 1);
            if (Input.GetKeyDown(KeyCode.DownArrow))
                _selected++;
        }

        private void OnGUI()
        {
            if (!_open || GameBootstrap.Instance == null || GameBootstrap.Session == null)
                return;

            var rows = BuildRows();
            if (_selected >= rows.Count)
                _selected = Mathf.Max(0, rows.Count - 1);

            var w = Mathf.Min(720, Screen.width - 40);
            var h = Mathf.Min(420, Screen.height - 80);
            var rect = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.Box(rect, GameBootstrap.Instance.Localize("ui.codex.title", "图志"));

            var listRect = new Rect(rect.x + 12, rect.y + 28, w * 0.38f, h - 48);
            var detailRect = new Rect(listRect.xMax + 12, rect.y + 28, w * 0.52f, h - 48);

            _scroll = GUI.BeginScrollView(listRect, _scroll, new Rect(0, 0, listRect.width - 20, Mathf.Max(rows.Count * 28, listRect.height)));
            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var label = row.Discovered ? row.DisplayName : GameBootstrap.Instance.Localize("ui.codex.unknown", "？？？");
                if (GUI.Button(new Rect(0, i * 28, listRect.width - 24, 26), (i == _selected ? "> " : "  ") + label))
                    _selected = i;
            }
            GUI.EndScrollView();

            GUI.Box(detailRect, "");
            if (rows.Count > 0 && _selected >= 0 && _selected < rows.Count)
            {
                var row = rows[_selected];
                var y = detailRect.y + 8;
                var x = detailRect.x + 10;
                var dw = detailRect.width - 20;
                if (!row.Discovered)
                {
                    GUI.Label(new Rect(x, y, dw, 24), GameBootstrap.Instance.Localize("ui.codex.unknown", "？？？"));
                    GUI.Label(new Rect(x, y + 28, dw, 60), "尚未留下可靠记录。走近它，先看清轮廓。");
                }
                else
                {
                    GUI.Label(new Rect(x, y, dw, 24), row.DisplayName);
                    GUI.Label(new Rect(x, y + 26, dw, 20),
                        GameBootstrap.Instance.Localize("ui.codex.layer.appearance", "外观层"));
                    if (!string.IsNullOrEmpty(row.Science))
                        GUI.Label(new Rect(x, y + 50, dw, 80), row.Science);
                    if (!string.IsNullOrEmpty(row.Poem))
                        GUI.Label(new Rect(x, y + 140, dw, 60), row.Poem);
                }
            }

            GUI.Label(new Rect(rect.x + 12, rect.yMax - 22, w - 24, 20),
                GameBootstrap.Instance.Localize("ui.codex.hint_key", "C / Esc 关闭 · ↑↓ 选择"));
        }

        private List<CodexRow> BuildRows()
        {
            var rows = new List<CodexRow>();
            var session = GameBootstrap.Session;
            var catalog = session.DataCatalog;
            var codex = session.World?.Codex;
            if (catalog == null || codex == null)
                return rows;

            var region = session.World.AreaId ?? "R01";
            var defs = catalog.CreaturesInRegion(region)
                .Where(d => d.Id.Value != "C000")
                .OrderBy(d => d.Id.Value)
                .ToList();

            // Ensure unlocked-out-of-region still appear
            foreach (var id in codex.UnlockedDefIds)
            {
                if (defs.Any(d => d.Id.Value == id)) continue;
                if (catalog.TryGetCreature(CreatureDefId.Parse(id), out var extra) && extra.Id.Value != "C000")
                    defs.Add(extra);
            }

            foreach (var def in defs)
            {
                var discovered = codex.IsUnlocked(def.Id, CodexLayer.Appearance);
                var name = GameBootstrap.Instance.Localize(def.NameKey, def.Id.Value);
                rows.Add(new CodexRow
                {
                    DefId = def.Id.Value,
                    Discovered = discovered,
                    DisplayName = name,
                    Science = discovered ? GameBootstrap.Instance.Localize(def.CodexScienceKey, "") : "",
                    Poem = discovered ? GameBootstrap.Instance.Localize(def.CodexPoemKey, "") : ""
                });
            }

            return rows;
        }

        private sealed class CodexRow
        {
            public string DefId;
            public bool Discovered;
            public string DisplayName;
            public string Science;
            public string Poem;
        }
    }
}
