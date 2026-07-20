# 仓库工程骨架方案（Engineering Skeleton）v0.2

> 状态：按世界超一流复评修订 — **Unity 为主结构**；Godot 为附录变体  
> 依据：`tech-stack-worldclass-review.md` / ADR-006（修订）  
> 原则：Domain 纯逻辑、数据驱动、切片优先、可扩主机与编制

---

## 1. 目标

一次建好服务 VS0–VS6 **且** 不阻碍 36 个月产品化的骨架：

1. PC 可运行场景壳  
2. 输入 / 镜头 / 交互  
3. UI 壳（HUD、对话、图志、队伍）  
4. 存档版本化接口  
5. 精灵/技能/区域数据管线 + 校验  
6. 可单测的战斗/结契 Domain  
7. 与 `docs/` ID 对齐的内容目录  

---

## 2. 主结构（Unity 推荐）

```text
lingyu/  (local folder may still be cardplayzcode)
  AGENTS.md
  README.md
  docs/                              # 设计单源真相
  data/                              # 权威 JSON（与 Unity 分离）
  tools/                             # validate_data 等
  game/                              # Unity 项目根
    ProjectSettings/
    Packages/manifest.json
    Assets/
      _Project/
        Art/
          Characters/
          Creatures/
          Environments/
          VFX/
          UI/
          Placeholders/
        Audio/
          BGM/
          SFX/
          Mixers/
        Data/                        # 导入后的运行数据（SO/生成物）
          Creatures/
          Skills/
          Encounters/
          Items/
          Quests/
          Regions/
          Balance/
        DataSource/                  # Git 友好源数据（可选放仓库根 data/）
        Prefabs/
          Creatures/
          World/
          UI/
          Battle/
        Scenes/
          Boot/
            Boot.unity
          App/
            Main.unity
          World/
            R01_VerdantEcho/
              R01_Main.unity
          Battle/
            BattleSandbox.unity
          UI/
          Dev/
            BondingSandbox.unity
            BattleLogicSandbox.unity
        Scripts/
          Domain/                    # 纯 C#，禁止 using UnityEngine
            Battle/
            Bonding/
            Codex/
            Affinity/
            Evolution/
            Party/
            Common/
          Application/               # 用例服务，可少量无状态协调
          Infrastructure/
            Save/
            DataLoading/
            Localization/
          Presentation/              # MonoBehaviour / View / UI
            Player/
            Camera/
            Exploration/
            Bonding/
            Battle/
            Codex/
            Narrative/
            World/
            UI/
            Debug/
          Editor/                    # 导入器、校验、预览窗
        Settings/
          Input/
          URP/
          Game/
        Tests/
          EditMode/
            Domain/
          PlayMode/
    UserSettings/                    # gitignore
  data/                              # （推荐）仓库级源数据，CI 校验
    creatures/
    skills/
    encounters/
    items/
    quests/
    dialogue/
    regions/
    balance/
    localization/
      zh-CN/
    schema/
  tools/
    validate_data.py
    import_to_unity.md
    sim_battle/                      # 后置：平衡模拟
  .gitignore
  .gitattributes
  .editorconfig
  README.md
```

### 程序集拆分（世界级硬要求）

| Assembly | 引用 | 职责 |
|---|---|---|
| `Aetherion.Domain` | 无 Unity | 战斗、结契、图鉴、进化规则 |
| `Aetherion.Application` | Domain | 用例：开始结契、结算回合、解锁图志 |
| `Aetherion.Infrastructure` | Domain + Unity | 存档、读表、平台 |
| `Aetherion.Presentation` | 全部 | 场景、动画、UI |
| `Aetherion.Editor` | 编辑器 only | 导入与校验 |
| `Aetherion.Tests` | Domain 等 | EditMode 单测 |

---

## 3. 模块边界

```text
UI / Input → Intent
Application Service → Domain 决策
Presentation → 播动画/VFX/镜头
Repository → 只读定义数据
Save → 只存运行时状态（非配置）
```

| 域 | 负责 | 不负责 |
|---|---|---|
| Bonding | 理解度、试炼、契约状态机 | 3C 移动 |
| Battle | 回合、相性、状态、回放指令 | 世界刷新 |
| Codex | 分层解锁 | 改基础数值定义 |
| Affinity | 羁绊阶与修饰 | 存档格式 |
| Exploration | 迹象、兴趣点、遭遇请求 | 战斗公式 |
| Narrative | 旗标与任务 | 精灵基础数值 |
| World | 天气昼夜区域事件 | UI 动画 |

通信：显式服务 + 领域事件；禁止全局随意单例意大利面。

---

## 4. 数据契约

### 4.1 源数据示例 `data/creatures/C001_wuxian.json`

```json
{
  "id": "C001",
  "name_key": "creature.C001.name",
  "aspect_primary": "verdant",
  "aspect_secondary": null,
  "size_class": "S1",
  "discovery_tier": "C",
  "regions": ["R01"],
  "roles_battle": ["support", "control"],
  "roles_field": ["fog_nav", "alert"],
  "base_stats": {
    "hp": 42, "atk": 10, "def": 12,
    "spa": 14, "spd": 11, "spe": 13
  },
  "skills": ["sk_mist_veil", "sk_calm_chirp"],
  "bonding": {
    "template": "quiet_follow",
    "prefs": ["morning", "fog", "silence"],
    "aversions": ["loud_noise", "forced_approach"]
  },
  "evolution": [
    {
      "to": "C001B",
      "conditions": { "affinity_min": 2, "flags": ["R01_heartwood_visited"] }
    }
  ],
  "codex": {
    "science_key": "creature.C001.codex.science",
    "poem_key": "creature.C001.codex.poem"
  },
  "prefab_guid_or_path": "Creatures/C001_Wuxian"
}
```

### 4.2 权威关系
- 人读设定：`docs/creatures/roster/*.md`  
- 机读权威：`data/**`  
- ID 必须一致（C001、R01、sk_*）  
- Unity `Assets/_Project/Data` 为导入生成物（可提交或 CI 生成，团队锁一种）

### 4.3 校验门禁
`tools/validate_data.py`：
- ID 唯一、引用完整、必填字段、本地化 key、命名规范  

---

## 5. 场景流（VS0）

```text
Boot.unity
  → 初始化 Settings / Save / DataCatalog
  → Main.unity 或直进 R01_Main.unity
```

VS0 出口：
- WASD/手柄移动  
- Cinemachine 跟随  
- 1 个 Interactable  
- HUD 区域名与提示  
- 存读档玩家 Transform + 版本号  

---

## 6. VS 里程碑占用

| 里程碑 | 重点 |
|---|---|
| VS0 | Boot、Player、Camera、Interact、HUD、Save、R01 壳 |
| VS1 | DataCatalog、Codex 目击、生物生成 |
| VS2 | Bonding Domain + 雾衔模板 + Sandbox |
| VS3 | Battle Domain + 教学战 + 表现适配 |
| VS4 | Affinity + Evolution Timeline 演出 |
| VS5 | Narrative 事件、棘影、发现链 |
| VS6 | 音频、平衡、性能、DoD |

---

## 7. Unity 规范摘要

- 场景/预制体：`PascalCase`  
- C# 类型：`PascalCase`；私有字段 `_camel`  
- 禁止 `Presentation` 写战斗公式  
- 玩家可见字符串禁止硬编码逻辑层  
- 一个功能一个服务；新增物种不改 Domain 核心  

---

## 8. 测试

| 层 | 内容 | 起始 |
|---|---|---|
| 数据校验 | schema/引用 | VS0 |
| EditMode | 相性、伤害、结契状态机、图鉴解锁 | VS2 |
| Sandbox 场景 | 手感 | VS2/3 |
| 手测 | 60 分钟脚本 | VS5 |
| 性能 | 雾林与实体 | VS6 |

---

## 9. Git 卫生（Unity）

```gitignore
game/[Ll]ibrary/
game/[Tt]emp/
game/[Oo]bj/
game/[Bb]uild/
game/[Bb]uilds/
game/[Ll]ogs/
game/[Uu]ser[Ss]ettings/
game/MemoryCaptures/
game/ServerData/
*.csproj
*.unityproj
*.sln
*.suo
*.tmp
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db
.DS_Store
Thumbs.db
.idea/
.vscode/
exports/
build/
```

大资源启用 Git LFS（fbx/psd/wav/png 阈值策略另定）。  
`*.meta` **必须提交**。

---

## 10. VS0 执行清单（确认 Unity 后）

1. 创建 Unity 项目于 `game/`（URP 模板）  
2. 建程序集定义与空 Domain  
3. Boot → R01 场景流  
4. Player + Cinemachine  
5. Interactable + 石碑  
6. HUD  
7. SaveService（版本字段）  
8. `data/` 示例 C001 + validate 脚本  
9. EditMode 空测试通过  
10. README 写启动步骤  
11. ADR-006 标 Accepted  

---

## 11. 附录：Godot 变体（仅精品路线时启用）

若确认 Godot，目录回到 v0.1 设想：

```text
game/           # project.godot
  assets/
  data/         # 也可继续用仓库根 data/
  src/          # bonding/battle/codex/...
  scenes/
  tests/
  tools/
```

规则不变：Domain 逻辑可测、数据驱动、切片优先。  
主机与外包扩展风险自负（见复评文档）。

---

## 12. 附录：UE5 变体（仅视听史诗路线）

```text
game/           # .uproject
  Content/
  Source/
    Aetherion/
      Domain/
      Presentation/
  Config/
data/           # 仍建议仓库级源数据
```

需额外编制：TA、更重 CI、Sequencer 规范。不作默认。

---

## 13. 验收（骨架完成）

- [ ] 新机器按 README 打开即进 R01  
- [ ] Domain 程序集无引擎 API  
- [ ] 示例精灵数据可加载  
- [ ] 存读档含版本号  
- [ ] validate_data 可运行  
- [ ] 无上帝对象 Player 包办全部逻辑  

---

## 14. 状态

- 引擎：**Unity 已确认**（ADR-006 Accepted）  
- 技术栈：**已锁定**（ADR-007 / `tech-stack.md` / `AGENTS.md` §7）  
- 总体架构：**已锁定**（ADR-008 / `architecture.md` / `AGENTS.md` §7A）  
- 下一步：创建真实 `game/` Unity 工程与 VS0 代码壳（Boot + Mode 路由 + Exploration）
