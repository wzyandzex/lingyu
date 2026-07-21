using System.Collections.Generic;
using Aetherion.Application.Modes;
using Aetherion.Application.Ports;
using Aetherion.Application.Sessions;
using Aetherion.Domain.Bonding;
using Aetherion.Domain.Battle;
using Aetherion.Domain.Codex;
using Aetherion.Domain.Creatures;
using Aetherion.Domain.Party;
using Aetherion.Infrastructure.Bootstrap;
using Aetherion.Infrastructure.DataLoading;
using Aetherion.Infrastructure.Localization;
using Aetherion.Infrastructure.Save;
using Aetherion.Presentation.Bonding;
using Aetherion.Presentation.Interaction;
using Aetherion.Presentation.Player;
using Aetherion.Presentation.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Aetherion.Presentation.Bootstrap
{
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
        private CodexScreen _codex;
        private PartyScreen _party;
        private BondingHud _bondingHud;
        private BattleHud _battleHud;
        private BattleSimulator _activeBattle;
        private string _statusMessage = string.Empty;
        private Vector3 _lastPlayerPos;
        private float _playerSpeed;

        public BattleSimulator ActiveBattle => _activeBattle;

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
            _catalog.LoadCreatures(DataPathResolver.GetCreaturesDirectory());
            _catalog.LoadEncounters(DataPathResolver.GetEncountersDirectory());
            _catalog.LoadSkills(DataPathResolver.GetSkillsDirectory());
            _catalog.LoadEnemies(DataPathResolver.GetEnemiesDirectory());

            _l10n = new JsonLocalization();
            _l10n.LoadFromSimpleJsonObject(DataPathResolver.GetL10nFilePath());

            _save = new FileSaveService(DataPathResolver.GetSaveDirectory());
            _router = new GameModeRouter();

            Session = new AppSession(_catalog, _save, _l10n, _router);

            _router.Register(new ExplorationMode(
                onEnter: () => Debug.Log("[Mode] Exploration enter"),
                onExit: () => Debug.Log("[Mode] Exploration exit")));
            _router.Register(new BondingMode(
                onEnter: () => Debug.Log("[Mode] Bonding enter"),
                onExit: () => Debug.Log("[Mode] Bonding exit"),
                onTick: TickBondingMode));
            _router.Register(new BattleMode(
                onEnter: () => Debug.Log("[Mode] Battle enter"),
                onExit: () => Debug.Log("[Mode] Battle exit")));
            _router.Register(new StubMode(GameModeId.Dialogue, id => Debug.Log($"[Mode] Stub enter {id}")));
            _router.Register(new StubMode(GameModeId.Menu, id => Debug.Log($"[Mode] Stub enter {id}")));

            if (_catalog.TryGetCreature(CreatureDefId.Parse("C001"), out var c001))
                Debug.Log($"[Boot] C001 ready: {_l10n.Get(c001.NameKey, c001.Id.Value)}");
            if (_catalog.TryGetCreature(CreatureDefId.Parse("C002"), out var c002))
                _statusMessage = _l10n.Get("ui.prompt.data_loaded", "图志数据已载入") +
                                 $" · {_l10n.Get(c002.NameKey, c002.Id.Value)}";
        }

        private void Start()
        {
            if (autoStartNewGame)
                StartNewGame();
        }

        private void Update()
        {
            SamplePlayerSpeed();
            _router?.Tick(Time.deltaTime);

            var mode = _router?.CurrentId;

            if (mode == GameModeId.Exploration)
            {
                if (Input.GetKeyDown(KeyCode.C))
                {
                    EnsureCodex();
                    if (_party != null && _party.IsOpen) _party.Close();
                    _codex.Toggle();
                }

                if (Input.GetKeyDown(KeyCode.V))
                {
                    EnsureParty();
                    if (_codex != null && _codex.IsOpen) _codex.Close();
                    _party.Toggle();
                }
            }

            if (mode == GameModeId.Battle && _activeBattle != null &&
                _activeBattle.Phase == BattlePhase.AwaitingPlayer &&
                _activeBattle.Result == BattleResult.Ongoing)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                    SubmitBattleSkillIndex(0);
                if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                    SubmitBattleSkillIndex(1);
                if (Input.GetKeyDown(KeyCode.G))
                    SubmitBattleGuard();
                if (Input.GetKeyDown(KeyCode.R))
                    SubmitBattleFlee();
            }

            if (mode == GameModeId.Bonding)
            {
                if (Input.GetKeyDown(KeyCode.F))
                    ApplyBondingIntent(BondingIntent.ConfirmResonance);
                if (Input.GetKeyDown(KeyCode.Escape))
                    CancelBonding();
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                if (mode == GameModeId.Battle)
                {
                    PushPrompt(Localize("battle.ui.no_save", "试炼中无法存档"));
                }
                else
                {
                    PersistPlayerFromScene();
                    Session.Save(0);
                    PushPrompt(_l10n.Get("ui.prompt.save_done", "已保存"));
                }
            }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                if (Session.TryContinue(0))
                {
                    if (Session.World?.ActiveBonding != null)
                        CancelBonding();
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

        public bool TryRegisterSighting(CreatureDefId defId)
        {
            if (Session?.World == null) return false;
            if (_router != null && _router.CurrentId == GameModeId.Bonding) return false;
            var changed = Session.World.Codex.RegisterSighting(defId);
            if (!changed) return false;

            var name = defId.Value;
            if (Session.DataCatalog != null && Session.DataCatalog.TryGetCreature(defId, out var def))
                name = Localize(def.NameKey, defId.Value);

            var template = Localize("ui.prompt.codex_updated", "图志记下一笔：{name}");
            PushPrompt(template.Replace("{name}", name), 3.5f);
            return true;
        }

        public void HandleInteract(string locKey)
        {
            if (locKey == WuxianBondable.InteractKey)
            {
                TryStartBonding(CreatureDefId.Parse("C001"));
                return;
            }

            if (locKey == "interact.battle.tutorial")
            {
                TryStartTutorialBattle();
                return;
            }

            ShowInteractionText(locKey);
        }

        public const string BattleEntranceKey = "interact.battle.tutorial";

        public bool TryStartTutorialBattle()
        {
            if (Session?.World == null || _router == null) return false;
            if (_router.CurrentId != GameModeId.Exploration) return false;
            if (_activeBattle != null) return false;

            if (_codex != null && _codex.IsOpen) _codex.Close();
            if (_party != null && _party.IsOpen) _party.Close();

            EnsurePlayerBattler();
            if (Session.World.Party.Members.Count == 0)
            {
                PushPrompt(Localize("battle.msg.no_partner", "需要旅伴才能应战"));
                return false;
            }

            var playerMember = Session.World.Party.Members[0];
            if (!Session.DataCatalog.TryGetCreature(playerMember.DefId, out var playerDef))
            {
                PushPrompt("缺少伙伴数据");
                return false;
            }
            if (!Session.DataCatalog.TryGetEnemy("E001", out var enemyDef))
            {
                PushPrompt("缺少敌方数据 E001");
                return false;
            }

            var player = ToBattler(BattleSimulator.PlayerId, playerDef);
            var enemy = ToBattler(BattleSimulator.EnemyId, enemyDef);
            var skills = new List<SkillDef>();
            foreach (var sk in Session.DataCatalog.AllSkills)
                skills.Add(sk);

            _activeBattle = new BattleSimulator(player, enemy, skills, BattleSimulator.WeatherFog);
            var events = _activeBattle.Begin();
            EnsureBattleHud();
            _battleHud.ClearLog();
            _battleHud.AppendEvents(events, this);
            LockPlayerForBattle(true);
            _router.SwitchTo(GameModeId.Battle);
            return true;
        }

        public void SubmitBattleSkill(string skillId)
        {
            if (_activeBattle == null || _activeBattle.Phase != BattlePhase.AwaitingPlayer) return;
            var events = _activeBattle.SubmitPlayerAction(new BattleAction
            {
                Type = BattleActionType.Skill,
                SkillId = skillId
            });
            _battleHud?.AppendEvents(events, this);
            if (_activeBattle.Result == BattleResult.PlayerWin)
                OnBattleWon();
        }

        public void SubmitBattleSkillIndex(int index)
        {
            if (_activeBattle?.Player?.SkillIds == null) return;
            if (index < 0 || index >= _activeBattle.Player.SkillIds.Count) return;
            SubmitBattleSkill(_activeBattle.Player.SkillIds[index]);
        }

        public void SubmitBattleGuard()
        {
            if (_activeBattle == null || _activeBattle.Phase != BattlePhase.AwaitingPlayer) return;
            var events = _activeBattle.SubmitPlayerAction(new BattleAction { Type = BattleActionType.Guard });
            _battleHud?.AppendEvents(events, this);
            if (_activeBattle.Result == BattleResult.PlayerLose)
                { /* wait continue */ }
        }

        public void SubmitBattleFlee()
        {
            if (_activeBattle == null || _activeBattle.Phase != BattlePhase.AwaitingPlayer) return;
            var events = _activeBattle.SubmitPlayerAction(new BattleAction { Type = BattleActionType.Flee });
            _battleHud?.AppendEvents(events, this);
        }

        public void EndBattleReturnToExplore()
        {
            _activeBattle = null;
            if (_battleHud != null)
                _battleHud.ClearLog();
            LockPlayerForBattle(false);
            if (_router != null && _router.CurrentId == GameModeId.Battle)
                _router.SwitchTo(GameModeId.Exploration);
        }

        private void OnBattleWon()
        {
            // First win: unlock C001 battle codex layer
            if (Session?.World != null)
            {
                var c001 = CreatureDefId.Parse("C001");
                // RegisterSighting only does Appearance; use progress API for battle layer via raw load
                EnsureBattleCodexLayer(c001);
                Session.World.Flags["tutorial_battle_won"] = true;
            }
        }

        private void EnsureBattleCodexLayer(CreatureDefId id)
        {
            // Appearance first, then mark battle via flag-backed note if layer API limited
            Session.World.Codex.RegisterSighting(id);
            // Extend: use internal unlock by simulating entry — CodexProgress needs Unlock method
            Session.World.Codex.UnlockLayer(id, CodexLayer.Battle);
        }

        private void EnsurePlayerBattler()
        {
            if (Session.World.Party.Members.Count > 0) return;
            var trial = new CreatureInstance(CreatureDefId.Parse("C001"));
            Session.World.Party.TryAdd(trial);
            Session.World.Codex.RegisterSighting(CreatureDefId.Parse("C001"));
            PushPrompt(Localize("battle.msg.trial_partner", "暂以旅伴雾衔应战"), 3f);
        }

        private static BattlerState ToBattler(string id, CreatureDef def)
        {
            return new BattlerState
            {
                Id = id,
                DefId = def.Id.Value,
                NameKey = def.NameKey,
                PrimaryElement = def.AspectPrimary,
                MaxHp = def.BaseHp,
                Hp = def.BaseHp,
                Atk = def.BaseAtk,
                Def = def.BaseDef,
                SkillIds = def.SkillIds ?? System.Array.Empty<string>()
            };
        }

        private void LockPlayerForBattle(bool locked)
        {
            var motor = Object.FindObjectOfType<PlayerMotor>();
            if (motor != null) motor.SetMovementLocked(locked);
        }

        public void RegisterBattleHud(BattleHud hud) => _battleHud = hud;

        private void EnsureBattleHud()
        {
            if (_battleHud != null) return;
            var go = new GameObject("BattleHud");
            DontDestroyOnLoad(go);
            _battleHud = go.AddComponent<BattleHud>();
        }

        public bool TryStartBonding(CreatureDefId defId)
        {
            if (Session?.World == null || _router == null) return false;
            if (_router.CurrentId == GameModeId.Bonding) return false;
            if (Session.World.Flags.TryGetValue("party_has_" + defId.Value, out var has) && has)
            {
                PushPrompt(Localize("ui.bonding.already", "它已与你同行"));
                return false;
            }

            // Close overlays
            if (_codex != null && _codex.IsOpen) _codex.Close();
            if (_party != null && _party.IsOpen) _party.Close();

            Session.World.ActiveBonding = new BondingSession(defId);
            Session.World.Codex.RegisterSighting(defId);
            EnsureBondingHud();
            _router.SwitchTo(GameModeId.Bonding);
            PushPrompt(Localize("ui.bonding.start", "静一静 — 它在听你"));
            return true;
        }

        public void CancelBonding()
        {
            if (Session?.World?.ActiveBonding == null) return;
            Session.World.ActiveBonding.ApplyIntent(BondingIntent.Cancel);
            Session.World.ActiveBonding = null;
            if (_router != null && _router.CurrentId == GameModeId.Bonding)
                _router.SwitchTo(GameModeId.Exploration);
            PushPrompt(Localize("ui.bonding.cancel", "共鸣中止 — 还可再试"));
        }

        private void ApplyBondingIntent(BondingIntent intent)
        {
            var bond = Session?.World?.ActiveBonding;
            if (bond == null || bond.IsTerminal) return;
            bond.ApplyIntent(intent);
            if (bond.State == BondingState.Success)
                CompleteBondingSuccess();
        }

        private void TickBondingMode(float dt)
        {
            var world = Session?.World;
            var bond = world?.ActiveBonding;
            if (bond == null) return;

            if (bond.IsTerminal)
            {
                if (bond.State == BondingState.Success)
                    CompleteBondingSuccess();
                else if (bond.State == BondingState.Failed)
                    HandleBondingFailed(bond.LastFailCode);
                else if (bond.State == BondingState.Aborted)
                {
                    world.ActiveBonding = null;
                    _router.SwitchTo(GameModeId.Exploration);
                }
                return;
            }

            // Reading: reward stillness gently via intent sampling
            if (bond.State == BondingState.Reading && _playerSpeed <= BondingThresholds.StillSpeedMax)
                bond.ApplyIntent(BondingIntent.HoldStill);

            var dist = DistanceToWuxian();
            var still = _playerSpeed <= BondingThresholds.StillSpeedMax;
            bond.Tick(dt, _playerSpeed, dist, still);

            if (bond.State == BondingState.Success)
                CompleteBondingSuccess();
            else if (bond.State == BondingState.Failed)
                HandleBondingFailed(bond.LastFailCode);
            else if (bond.LastFailCode == BondingFailCode.WindowMiss && bond.State == BondingState.Testing)
            {
                _bondingHud?.ShowFail(Localize("ui.bonding.fail.WINDOW_MISS", "时机断了 — 再跟上它的步"));
            }
        }

        private void CompleteBondingSuccess()
        {
            var world = Session.World;
            var defId = world.ActiveBonding != null
                ? world.ActiveBonding.TargetDefId
                : CreatureDefId.Parse("C001");

            var instance = new CreatureInstance(defId);
            world.Party.TryAdd(instance);
            world.Codex.RegisterSighting(defId);
            world.Flags["party_has_" + defId.Value] = true;
            world.ActiveBonding = null;

            var wuxian = Object.FindObjectOfType<WuxianBondable>();
            if (wuxian != null)
            {
                wuxian.SetBondedVisual();
                var interact = wuxian.GetComponent<Interactable>();
                if (interact != null) interact.enabled = false;
            }

            if (_router.CurrentId == GameModeId.Bonding)
                _router.SwitchTo(GameModeId.Exploration);

            var name = Localize("creature.C001.name", "雾衔");
            PushPrompt(Localize("ui.bonding.success", "雾衔愿与你同行").Replace("{name}", name), 4f);
        }

        private void HandleBondingFailed(BondingFailCode code)
        {
            var key = "ui.bonding.fail." + code.ToString().ToUpperInvariant();
            // Map enum names to design codes
            string human;
            switch (code)
            {
                case BondingFailCode.TooFast:
                    human = Localize("ui.bonding.fail.TOO_FAST", "太急了，雾都惊散了");
                    break;
                case BondingFailCode.Pressure:
                    human = Localize("ui.bonding.fail.PRESSURE", "靠太近了 — 它感到压迫");
                    break;
                case BondingFailCode.Desync:
                    human = Localize("ui.bonding.fail.DESYNC", "没跟上它的步");
                    break;
                default:
                    human = Localize(key, "共鸣失败 — 再试一次");
                    break;
            }

            _bondingHud?.ShowFail(human);
            PushPrompt(human, 2.5f);
            Session.World.ActiveBonding = null;
            if (_router.CurrentId == GameModeId.Bonding)
                _router.SwitchTo(GameModeId.Exploration);
        }

        private float DistanceToWuxian()
        {
            var w = Object.FindObjectOfType<WuxianBondable>();
            var p = Object.FindObjectOfType<PlayerMotor>();
            if (w == null || p == null) return 99f;
            return Vector3.Distance(p.transform.position, w.transform.position);
        }

        private void SamplePlayerSpeed()
        {
            var p = Object.FindObjectOfType<PlayerMotor>();
            if (p == null) return;
            var pos = p.transform.position;
            var delta = pos - _lastPlayerPos;
            delta.y = 0f;
            _playerSpeed = delta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
            _lastPlayerPos = pos;
        }

        public void RegisterHud(HudController hud)
        {
            _hud = hud;
            if (_hud != null)
            {
                _hud.SetAreaName(_l10n.Get("ui.hud.area.R01", "翠语林海"));
                _hud.SetInteractHint(string.Empty);
                _hud.SetCodexHint(_l10n.Get("ui.hud.hints", "C 图志 · V 队伍"));
            }
        }

        public void RegisterCodex(CodexScreen codex) => _codex = codex;
        public void RegisterParty(PartyScreen party) => _party = party;
        public void RegisterBondingHud(BondingHud hud) => _bondingHud = hud;

        public void PushPrompt(string text, float seconds = 2.5f)
        {
            if (_hud != null) _hud.ShowPrompt(text, seconds);
            else Debug.Log($"[HUD] {text}");
        }

        public void SetInteractHint(bool visible)
        {
            if (_hud == null) return;
            if (_router != null && _router.CurrentId == GameModeId.Bonding)
            {
                _hud.SetInteractHint(string.Empty);
                return;
            }
            _hud.SetInteractHint(visible
                ? _l10n.Get("ui.hud.interact", "交互 [E]")
                : string.Empty);
        }

        public string Localize(string key, string fallback = null) =>
            _l10n != null ? _l10n.Get(key, fallback) : (fallback ?? key);

        public void ShowInteractionText(string locKey)
        {
            PushPrompt(Localize(locKey, locKey), 6f);
            if (Session?.World != null)
                Session.World.Flags["interacted_stone"] = true;
        }

        private void EnsureCodex()
        {
            if (_codex != null) return;
            var go = new GameObject("CodexScreen");
            DontDestroyOnLoad(go);
            _codex = go.AddComponent<CodexScreen>();
        }

        private void EnsureParty()
        {
            if (_party != null) return;
            var go = new GameObject("PartyScreen");
            DontDestroyOnLoad(go);
            _party = go.AddComponent<PartyScreen>();
        }

        private void EnsureBondingHud()
        {
            if (_bondingHud != null) return;
            var go = new GameObject("BondingHud");
            DontDestroyOnLoad(go);
            _bondingHud = go.AddComponent<BondingHud>();
        }

        private void EnsureWorldShell()
        {
            if (GameObject.Find("R01_RuntimeRoot") != null)
                return;

            if (UnityEngine.Application.CanStreamedLevelBeLoaded(r01SceneName) &&
                SceneManager.GetActiveScene().name != r01SceneName)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.LoadScene(r01SceneName, LoadSceneMode.Single);
                return;
            }

            RuntimeWorldBuilder.BuildR01Shell();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (GameObject.Find("R01_RuntimeRoot") == null)
                RuntimeWorldBuilder.BuildR01Shell();
            ApplyWorldToScene();
        }

        private void PersistPlayerFromScene()
        {
            var player = Object.FindObjectOfType<PlayerMotor>();
            if (player == null || Session?.World == null) return;
            var t = player.transform;
            Session.World.Player.X = t.position.x;
            Session.World.Player.Y = t.position.y;
            Session.World.Player.Z = t.position.z;
            Session.World.Player.Yaw = t.eulerAngles.y;
        }

        private void ApplyWorldToScene()
        {
            var player = Object.FindObjectOfType<PlayerMotor>();
            if (player == null || Session?.World == null) return;
            var p = Session.World.Player;
            player.Teleport(new Vector3(p.X, p.Y <= 0.01f ? 1f : p.Y, p.Z), p.Yaw);
            _lastPlayerPos = player.transform.position;
        }
    }
}
