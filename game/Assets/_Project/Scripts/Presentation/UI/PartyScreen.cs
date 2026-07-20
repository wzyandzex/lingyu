using Aetherion.Presentation.Bootstrap;
using Aetherion.Presentation.Player;
using UnityEngine;

namespace Aetherion.Presentation.UI
{
    public sealed class PartyScreen : MonoBehaviour
    {
        private bool _open;
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
            if (_motor != null) _motor.SetMovementLocked(true);
        }

        public void Close()
        {
            _open = false;
            if (_motor != null) _motor.SetMovementLocked(false);
        }

        private void Update()
        {
            if (_open && Input.GetKeyDown(KeyCode.Escape))
                Close();
        }

        private void OnGUI()
        {
            if (!_open || GameBootstrap.Instance == null || GameBootstrap.Session?.World == null)
                return;

            var w = 360f;
            var h = 220f;
            var rect = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.Box(rect, GameBootstrap.Instance.Localize("ui.party.title", "队伍"));

            var party = GameBootstrap.Session.World.Party.Members;
            var y = rect.y + 36;
            if (party.Count == 0)
            {
                GUI.Label(new Rect(rect.x + 16, y, w - 32, 40),
                    GameBootstrap.Instance.Localize("ui.party.empty", "尚无同行伙伴"));
            }
            else
            {
                foreach (var m in party)
                {
                    var name = m.DefId.Value;
                    if (GameBootstrap.Session.DataCatalog != null &&
                        GameBootstrap.Session.DataCatalog.TryGetCreature(m.DefId, out var def))
                    {
                        name = GameBootstrap.Instance.Localize(def.NameKey, m.DefId.Value);
                    }
                    GUI.Label(new Rect(rect.x + 16, y, w - 32, 24), "· " + name);
                    y += 28;
                }
            }

            GUI.Label(new Rect(rect.x + 16, rect.yMax - 28, w - 32, 20),
                GameBootstrap.Instance.Localize("ui.party.hint", "V / Esc 关闭"));
        }
    }
}
