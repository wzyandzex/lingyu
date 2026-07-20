using Aetherion.Application.Modes;
using Aetherion.Application.Ports;
using Aetherion.Application.Sessions;
using Aetherion.Domain.Creatures;
using Aetherion.Infrastructure.Bootstrap;
using Aetherion.Infrastructure.DataLoading;
using Aetherion.Infrastructure.Localization;
using Aetherion.Infrastructure.Save;
using Aetherion.Presentation.Player;
using Aetherion.Presentation.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Aetherion.Presentation.Bootstrap
{
    /// <summary>
    /// Composition root. Auto-created on Play if missing.
    /// Compatible with Unity 2022.3 LTS (no Input System hard dependency).
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }
        public static AppSession Session { get; private set; }

        [SerializeField] private string r01SceneName = "R01_Main";
        [SerializeField] private bool autoStartNewGame = true;

        private JsonDataCatalog _catalog;
        private JsonLocalization _l10n;
        private FileSaveService _save;
        private GameModeRouter _router;
        private HudController _hud;
        private string _statusMessage = string.Empty;
        private bool _shellBuilt;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _catalog = new JsonDataCatalog();
            _catalog.LoadFromDirectory(DataPathResolver.GetCreaturesDirectory());

            _l10n = new JsonLocalization();
            _l10n.LoadFromSimpleJsonObject(DataPathResolver.GetL10nFilePath());

            _save = new FileSaveService(DataPathResolver.GetSaveDirectory());
            _router = new GameModeRouter();

            Session = new AppSession(_catalog, _save, _l10n, _router);

            _router.Register(new ExplorationMode(
                onEnter: () => Debug.Log("[Mode] Exploration enter"),
                onExit: () => Debug.Log("[Mode] Exploration exit")));
            _router.Register(new StubMode(GameModeId.Bonding, id => Debug.Log($"[Mode] Stub enter {id}")));
            _router.Register(new StubMode(GameModeId.Battle, id => Debug.Log($"[Mode] Stub enter {id}")));
            _router.Register(new StubMode(GameModeId.Dialogue, id => Debug.Log($"[Mode] Stub enter {id}")));
            _router.Register(new StubMode(GameModeId.Menu, id => Debug.Log($"[Mode] Stub enter {id}")));

            var c001 = CreatureDefId.Parse("C001");
            if (_catalog.TryGetCreature(c001, out var def))
            {
                var name = _l10n.Get(def.NameKey, def.NameKey);
                Debug.Log($"[Boot] Sample creature loaded: {def.Id} -> {name}");
                _statusMessage = _l10n.Get("ui.prompt.data_loaded", "图志数据已载入") + $" · {name}";
            }
            else
            {
                Debug.LogWarning("[Boot] C001 not found in catalog.");
                _statusMessage = "C001 missing — check data/creatures";
            }
        }

        private void Start()
        {
            if (autoStartNewGame)
                StartNewGame();
        }

        private void Update()
        {
            _router?.Tick(Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.F5))
            {
                PersistPlayerFromScene();
                Session.Save(0);
                PushPrompt(_l10n.Get("ui.prompt.save_done", "已保存"));
            }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                if (Session.TryContinue(0))
                {
                    EnsureWorldShell();
                    ApplyWorldToScene();
                    PushPrompt(_l10n.Get("ui.prompt.load_done", "已读取"));
                }
                else
                {
                    PushPrompt("无存档");
                }
            }
        }

        public void StartNewGame()
        {
            Session.StartNewGame("R01");
            _router.SwitchTo(GameModeId.Exploration);
            EnsureWorldShell();
            ApplyWorldToScene();
            PushPrompt(_l10n.Get("ui.prompt.new_game", "新的旅程开始"));
            if (!string.IsNullOrEmpty(_statusMessage))
                PushPrompt(_statusMessage, 4f);
        }

        public void RegisterHud(HudController hud)
        {
            _hud = hud;
            if (_hud != null)
            {
                _hud.SetAreaName(_l10n.Get("ui.hud.area.R01", "翠语林海"));
                _hud.SetInteractHint(string.Empty);
            }
        }

        public void PushPrompt(string text, float seconds = 2.5f)
        {
            if (_hud != null)
                _hud.ShowPrompt(text, seconds);
            else
                Debug.Log($"[HUD] {text}");
        }

        public void SetInteractHint(bool visible)
        {
            if (_hud == null) return;
            _hud.SetInteractHint(visible
                ? _l10n.Get("ui.hud.interact", "交互 [E]")
                : string.Empty);
        }

        public string Localize(string key, string fallback = null) =>
            _l10n != null ? _l10n.Get(key, fallback) : (fallback ?? key);

        public void ShowInteractionText(string locKey)
        {
            var text = Localize(locKey, locKey);
            PushPrompt(text, 6f);
            if (Session?.World != null)
                Session.World.Flags["interacted_stone"] = true;
        }

        private void EnsureWorldShell()
        {
            // Always prefer runtime shell in VS0 so an empty scene still has content.
            if (GameObject.Find("R01_RuntimeRoot") != null)
            {
                _shellBuilt = true;
                return;
            }

            if (Application.CanStreamedLevelBeLoaded(r01SceneName) &&
                SceneManager.GetActiveScene().name != r01SceneName)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.LoadScene(r01SceneName, LoadSceneMode.Single);
                return;
            }

            Debug.Log("[Boot] Building runtime R01 shell (greybox).");
            RuntimeWorldBuilder.BuildR01Shell();
            _shellBuilt = true;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (GameObject.Find("R01_RuntimeRoot") == null)
            {
                RuntimeWorldBuilder.BuildR01Shell();
                _shellBuilt = true;
            }

            ApplyWorldToScene();
        }

        private void PersistPlayerFromScene()
        {
            var player = FindPlayerMotor();
            if (player == null || Session?.World == null) return;
            var t = player.transform;
            Session.World.Player.X = t.position.x;
            Session.World.Player.Y = t.position.y;
            Session.World.Player.Z = t.position.z;
            Session.World.Player.Yaw = t.eulerAngles.y;
        }

        private void ApplyWorldToScene()
        {
            var player = FindPlayerMotor();
            if (player == null || Session?.World == null) return;
            var p = Session.World.Player;
            // Default spawn if never moved
            var pos = new Vector3(p.X, p.Y <= 0.01f ? 1f : p.Y, p.Z);
            player.Teleport(pos, p.Yaw);
        }

        private static PlayerMotor FindPlayerMotor()
        {
            return Object.FindObjectOfType<PlayerMotor>();
        }
    }
}
