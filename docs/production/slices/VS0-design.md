# VS0 设计包 — 世界壳（World Shell）

> **状态**：Review（待用户批准实现）  
> **剖面**：**Slim**（见 `docs/production/design-package-profiles.md`）  
> **切片**：VS0  
> **日期**：2026-07-20  
> **PRD 映射**：VS-MOV-01..03, VS-INT-01..02, VS-UI-01, VS-SAV-01..04（子集）, VS-DAT-01/03/04（样例级）, VS PRD §11 VS0 出口  
> **依赖**：无前序实现；依赖全局 ADR-001～010、架构圣经、tech-stack、工程骨架、git/content/playtest 协议  
> **设计门**：本文件；**实现许可**：未授予  

---

## A. 切片宣告（固定保留在设计包顶部）

## 当前切片：VS0 — 世界壳

**目标**：玩家能在翠语林海壳场景中移动、环视、与至少一个兴趣点交互、看到 HUD、存读档恢复位置；工程具备可扩展的 Session/Mode/Data 骨架。  

**PRD 映射**：移动/交互/HUD/存档壳/数据样例/Mode 路由（Exploration 实装，其余占位）  

**依赖**：无代码依赖；文档基线已就绪  

**本切片设计点**：
- 架构增量：**有**（首次落地 AppSession/WorldSession/ModeRouter/DataCatalog/Save）  
- 技术选型：**增量决策有**（Unity 精确版本策略、JSON 加载方式、是否首日引入 UniTask 等）；**无全局换栈**  
- 数据模型：**有**（CreatureDef schema v0、Save v0、loc key 规范、input actions）  
- 算法/规则：**极少 / 玩法公式 N/A**（无结契/战斗；交互仅射线/触发距离）  

**规范引用（实现前只读，不重复扩写）**：
- 仓库卫生：`git-and-repo-hygiene.md`  
- 内容阶段：`content-pipeline.md`（VS0 仅 dummy/C001 样例级）  
- PH 资产：`art-audio-placeholder.md`  
- 试玩：`playtest-protocol.md` → VS0 仅 **T0 作者冒烟**  
- R01 兴趣点：`R01-environmental-narrative.md`（VS0 只实装 ≥1 POI，推荐 `POI-R01-02` 青篱旧徽/石碑）  

**当前阶段**：待用户确认  
**建议**：审阅本设计包 → 回复「批准 VS0 实现」或提出修改意见。批准前不创建业务实现代码。

---

## B1 范围

### In
1. 创建 Unity 工程于 `game/`（URP）  
2. 程序集：Domain / Application / Infrastructure / Presentation / Editor / Tests 空壳+最小类型  
3. Boot →（可选极简 Title）→ R01 壳场景  
4. AppSession + WorldSession + GameModeRouter  
5. ExplorationMode 实装；Title/Menu 最小；Bonding/Battle/Dialogue/Cutscene **占位不实现玩法**  
6. 玩家移动（WASD/手柄）+ Cinemachine 跟随  
7. Interactable 接口 + ≥1 测试石碑（弹出对话或日志+UI 提示）  
8. HUD：区域名「翠语林海」、交互提示  
9. Save v0：版本号 + 玩家位置/旋转 + 当前区域 ID + 时间戳；读档恢复  
10. `data/` 示例：`C001` 最小 JSON + schema + `tools/validate_data.py` 可运行  
11. DataCatalog 能加载并 Debug 打印 C001  
12. 本地化 key 结构 + 中文表至少含 HUD/交互字符串  
13. Input System 动作表  
14. Git 基础（仓库卫生文档已建；实现期补 Unity 运行说明与 `ProjectVersion.txt` 钉死）  
15. EditMode 烟测：Domain 程序集无 Unity 引用的编译级约束（AsmDef）；可选 1 个占位测试  
16. Placeholder 路径服从 `art-audio-placeholder.md`（`PH_` / `Placeholders/`）  

### Out
- 结契、战斗、图志完整 UI、进化、天气玩法、主线任务图  
- 真·雾林美术终稿、完整导航网格、Addressables  
- 多存档槽 UI 美化（可先单槽 `slot0`）  
- UniTask 全面异步化（见选型：可延后到有真异步 IO 时）  
- 完整 Creature 运行时实体 AI  

### 防膨胀（明确不做）
- 不做“顺便把雾衔结契做了”  
- 不做战斗沙盒可玩（Dev 场景可空着）  
- 不引入 FMOD/DOTS/HDRP/UGUI 业务  
- 不铺第二区域  

---

## B2 玩家流程

### 标准路径（3–8 分钟烟测）
1. 启动游戏 → Boot 加载服务与数据  
2. 自动或一点击进入 R01 壳（Title 可极简：New Game）  
3. 出现在出生点，HUD 显示区域名  
4. WASD 移动，鼠标/右摇杆环视（按 Input 表）  
5. 靠近石碑 → HUD 显示「按 E / 南键 交互」  
6. 交互 → 短文本（民谣残句或测试文案）  
7. 按存档键或菜单「保存」→ 提示已保存  
8. 移动到他处 → 读档 → 回到存档位置  

### 失败与重试
- 无存档时 Continue 禁用或提示  
- 交互距离外按下：无事发生或轻提示一次  

### 时长预算
- 开发验收路径 ≤10 分钟  
- 非内容体验时长目标（内容在后续 VS）  

---

## B3 架构增量

### 新增/修改模块（首次落地）

| 模块 | 层 | VS0 职责 |
|---|---|---|
| `AppSession` | Application | 进程级：Settings、DataCatalog 句柄、服务 |
| `WorldSession` | Application | 单档：PlayerState、Mode、Flags 空壳 |
| `GameModeRouter` | Application | 切换 Mode；VS0 仅 Exploration+Title/Menu |
| `IGameMode` | Application | Enter/Exit/Tick 契约 |
| `ExplorationMode` | Application+Presentation 协作 | 启用玩家输入与世界 |
| `DataCatalog` | Infrastructure | 读 `data/**` JSON |
| `SaveService` | Infrastructure | 原子写读 slot0 |
| `PlayerController` | Presentation | 移动 |
| `CameraRig` | Presentation | Cinemachine |
| `Interactable` / `InteractionService` | Presentation(+App) | 检测与触发 |
| `HudController` | Presentation | UI Toolkit HUD |
| `SceneFlow` | Infrastructure/App | Boot→R01 加载 |
| Domain 空壳 | Domain | `CreatureDefId` 等值对象；**无战斗结契逻辑** |

### Mode / 上下文 / 事件（VS0）

```text
Title (optional) → Exploration
Menu 可叠加（暂停输入可选）
Bonding/Battle/Dialogue/Cutscene = StubMode（Enter 打日志，不可玩）
```

领域事件 VS0 最少：
- `GameSaved` / `GameLoaded`（Application 级即可）  
- `InteractionStarted`（可选）  

### 与 §7A 对齐
- 进度真相在 WorldSession（位置等）  
- Domain 不引用 Unity  
- 无上帝 Player 写存档格式细节（SaveService 负责）  
- 定义走 DataCatalog，不进存档权威  

### 场景流方案对比

| 方案 | 优点 | 代价 | 结论 |
|---|---|---|---|
| A. Boot 附加场景 + 加载 R01 | 清晰组合根 | 多一场景 | **推荐** |
| B. 单场景含所有 | 快 | 后期拆痛 | 否 |
| C. 纯加性加载大世界 | 前瞻 | VS0 过重 | 否 |

**推荐 A**：`Boot.unity` 初始化 → `R01_Main.unity` 单场景探索（VS 期不用流送）。

### 组合根方案

| 方案 | 结论 |
|---|---|
| 纯手动 ServiceRegistry 在 Boot | **推荐**（透明、无第三方 DI 成本） |
| Zenject/VContainer | 否（VS0 引入过重；他日 ADR） |

---

## B4 技术选型（仅增量）

### 沿用全局栈？
**是**：Unity + URP + C# + Input System + Cinemachine + UI Toolkit + 分层架构。

### 增量决策

| 议题 | 选项 | 推荐 | 理由 |
|---|---|---|---|
| Unity 版本 | 6.0/6.1 具体 LTS 号 | **实现时本机安装的最新稳定 Unity 6.x，并写入 `ProjectVersion.txt` 与 README 钉死** | 文档无法预知你本机安装；钉死在仓库 |
| JSON 库 | `System.Text.Json` / Newtonsoft / Unity JsonUtility | **System.Text.Json**（Domain/Infra 可用） | 无第三方；JsonUtility 弱 |
| 源数据位置 | 仅 `data/` 根 / 仅 Assets | **仓库根 `data/` + 开发期 StreamingAssets 拷贝或 Editor 加载绝对/相对路径** | Git 友好；运行时策略见 B5 |
| 运行时读 data | Editor 直读仓库路径；Player 读 StreamingAssets 副本 | **推荐双模式** | 开发爽、构建可复制 |
| UniTask | VS0 引入 / 延后 | **延后**到真有异步加载需求 | 减少首包依赖 |
| 输入 | Input System only | **是** | 已锁 |
| UI | UI Toolkit only | **是** | 已锁 |
| 物理移动 | CharacterController / Rigidbody / 变形位移 | **CharacterController 或简单 Transform+碰撞** | VS0 壳够用；推荐 CharacterController |
| 对话 | 完整对话图 / 单 Text 弹层 | **单 Text 弹层或 UI Label** | 防膨胀 |
| 测试 | UTF EditMode | **是，至少程序集+1 烟测** | 架构门 |

### 新第三方依赖
**默认零新增**（除 Unity 官方包：Input System、Cinemachine、URP、UI Toolkit、Test Framework）。

---

## B5 数据模型

### Definition 变更 — CreatureDef schema v0

```json
{
  "$schema": "creature_def_v0",
  "id": "C001",
  "name_key": "creature.C001.name",
  "aspect_primary": "verdant",
  "aspect_secondary": null,
  "size_class": "S1",
  "discovery_tier": "C",
  "regions": ["R01"],
  "base_stats": { "hp": 42, "atk": 10, "def": 12, "spa": 14, "spd": 11, "spe": 13 },
  "skills": [],
  "bonding": { "template": "quiet_follow" },
  "codex": {
    "science_key": "creature.C001.codex.science",
    "poem_key": "creature.C001.codex.poem"
  },
  "view_key": "view.C001"
}
```

VS0 **不要求** skills 非空；校验：必填 id/name_key/aspect_primary/regions。

### Runtime / Save v0

```json
{
  "version": 0,
  "savedAtUtc": "ISO-8601",
  "areaId": "R01",
  "player": {
    "position": { "x": 0, "y": 0, "z": 0 },
    "yaw": 0
  },
  "flags": {},
  "party": [],
  "codex": {}
}
```

说明：
- `party`/`codex`/`flags` 空结构预留，避免 VS1 改破版本哲学  
- VS0 读档主要恢复 `player` + `areaId`  
- 版本迁移：`version < 当前` 时走迁移函数链（VS0 仅拒绝未来版本）  

### 本地化 key 规范

```text
ui.hud.area.R01
ui.hud.interact
ui.prompt.save_done
ui.prompt.load_done
creature.C001.name
creature.C001.codex.science
creature.C001.codex.poem
interact.stone.intro
```

规则：`域.路径.名称`，小写点分；禁止逻辑硬编码中文。

### Input Actions（名）

| Action | 键鼠默认 | 手柄默认 |
|---|---|---|
| Move | WASD | 左摇杆 |
| Look | 鼠标 | 右摇杆 |
| Interact | E | South (A) |
| OpenMenu | Esc | Start |
| Save | F5 | 可选+菜单 |
| Load | F9 | 可选+菜单 |

### Schema / 校验
`tools/validate_data.py`：
- 扫描 `data/creatures/*.json`  
- 检查必填字段、id 唯一、regions 非空  
- exit code ≠0 表示失败  

---

## B6 算法与规则

### 交互检测（Presentation）
- 方案推荐：玩家前方 SphereCast/Overlap 半径 R=1.5m，夹角限制或简单距离最近 Interactable  
- 无复杂算法；可测性低，手测即可  

### 存档原子写
```text
write temp file → flush → replace target
```

### Domain
VS0 无战斗/结契算法。可选：`CreatureDefId` 值对象相等性单测。

---

## B7 接口契约

### Use cases（Application）
- `StartNewGame()`  
- `LoadGame(slot=0)`  
- `SaveGame(slot=0)`  
- `EnterExploration(areaId)`  
- `TryInteract()`  

### Domain API（最小）
- `CreatureDefId`  
- 预留 namespace：`Aetherion.Domain.Codex` 等空文件夹可接受  

### Infrastructure
- `IDataCatalog.GetCreature(CreatureDefId)`  
- `ISaveService.Save(WorldSessionSnapshot)` / `Load`  
- `ILocalization.Get(key)`  

### Presentation
- `IPlayerMotor`  
- `IInteractable.Interact(ctx)`  
- `HudView.SetArea / SetPrompt`  

### Events
- `GameSaved` `GameLoaded`  

---

## B8 UI/UX

### 界面
- HUD（UI Toolkit）：左上区域名；中下交互提示（有目标才显示）  
- 交互结果：简单 modal 或顶部 toast 文本  
- Menu：可选最小面板（继续/保存/读档/退出）— **P0 可用热键代替完整菜单**  

### 反馈链
移动 → 视觉位移；靠近石碑 → 提示出现；交互 → 文本；保存 → toast  

### 教学
无长教程；靠提示键  

### Placeholder 视觉规范
- 服从 `docs/production/art-audio-placeholder.md`  
- 玩家：胶囊体/简单 mesh，主色可辨  
- 地面：灰绿平面 + 简单雾参数（URP）  
- 石碑：方柱 + 自发光边，避免与地面融死（建议映射 POI-R01-02）  
- 禁止把临时材质路径写死进 Domain  

---

## B9 内容清单

| 类型 | 项 | 占位？ |
|---|---|---|
| 场景 | Boot, R01_Main | 是 |
| 实体 | Player, InteractStone（≥1 POI） | 是 |
| 数据 | C001 json + schema v0；C000 仅管线夹具可选 | 是（文案真） |
| 文案 | HUD/石碑/生物名（l10n key） | 中文真，可改 |
| 音频 | 可选足音/UI 嘀 | 可选 P1（PH 规范） |
| 精灵模型 | 无（仅数据加载） | VS1 |
| 试玩 | T0 作者冒烟清单 = B11 | 是 |

---

## B10 风险

| 风险 | 缓解 |
|---|---|
| 做成上帝 Boot 脚本 | 强制拆 Session/Router/Services |
| data 路径在 Build 失效 | 双模式加载写进 README 与代码注释 |
| 过早做结契 | Out 清单 + 验收不包含 |
| Unity 版本不一致 | ProjectVersion 钉死 |
| Windows 路径/中文用户路径问题 | 相对仓库根解析；文档说明 |
| UI Toolkit 学习成本 | HUD 极简；不进复杂绑定 |

---

## B11 本切片验收 DoD

- [ ] 新 clone + 按 README 用钉死 Unity 版本打开 `game/` 可进 Play  
- [ ] Boot 后进入 R01，无报错刷屏  
- [ ] WASD/手柄可移动，镜头跟随可用  
- [ ] 至少 1 个 Interactable 有提示并可交互出文本  
- [ ] HUD 显示区域名  
- [ ] F5/F9 或菜单存读档恢复位置（误差可接受）  
- [ ] Save 文件含 `version` 字段  
- [ ] `data/creatures` 含 C001；`validate_data.py` 通过  
- [ ] DataCatalog 启动时可解析 C001（日志或 Debug 面板）  
- [ ] 程序集存在且 Domain 无 `UnityEngine` 引用  
- [ ] 无结契/战斗可玩内容误入（防膨胀）  
- [ ] `slices/STATUS.md` 可更新为「待用户验收」  
- [ ] README 写明运行步骤  

---

## B12 实现任务序（批准后）

1. 确认 Git 远程/ignore 已就绪（预制作已建）；补 Unity LFS 启用说明若开始进大资源  
2. 创建 Unity URP 项目于 `game/`，钉版本到 `ProjectVersion.txt` + README  
3. 目录与 AsmDef（Domain→Tests 等引用关系）  
4. 最小 Domain 类型 + 1 烟测  
5. 收敛 `data/`：C001 + schema v0 + `tools/validate_data.py`  
6. Infrastructure：DataCatalog、Localization、SaveService  
7. Application：AppSession、WorldSession、ModeRouter、Modes  
8. Boot 场景组合根  
9. R01 场景：地面、出生点、石碑、灯光雾  
10. Player + Cinemachine + Input  
11. Interaction  
12. HUD UI Toolkit  
13. Save/Load 热键接好  
14. README 运行说明  
15. 对照 B11 自检报告 + T0 冒烟  

---

## B13 开放问题（需用户拍板 / 默认可批）

| ID | 问题 | 默认推荐 | 你可改 |
|---|---|---|---|
| Q1 | VS0 Title 场景要不要 | **New Game 直接进 R01**，Title 可后补 | 可要求完整 Title |
| Q2 | 存读档键位 F5/F9 是否可接受 | **接受** | 可改只走菜单 |
| Q3 | 本机 Unity 精确版本 | 实现时钉死你机器上的 Unity 6.x | 你可指定版本号 |
| Q4 | Git 远程 | **已指向 `wzyandzex/lingyu`** | 仅当要换远程 |
| Q5 | 交互用键盘 E 是否可接受 | **是** | 可改 F |

无阻塞级开放问题：均可按默认推进。

---

## 设计门自检

- [x] 映射 PRD（VS0 出口与相关 ID）  
- [x] 无架构否决项（分层、Mode、Save 版本、Data 权威）  
- [x] 验收可观察  
- [x] 关键选型有对比与推荐  
- [ ] 等待用户批准实现  

---

## 请求实现许可

设计包路径：`docs/production/slices/VS0-design.md`

**推荐方案摘要**：Boot 组合根 + 手动 ServiceRegistry；R01 单场景壳；Input System + Cinemachine + UI Toolkit HUD；Save v0 原子写；根目录 `data/` + System.Text.Json；Domain 空壳可测；不做结契/战斗。

请回复其一：
- **「批准 VS0 实现」** 或 **「按方案开工」**  
- 或提出修改意见（我将先改设计包再谈实现）
