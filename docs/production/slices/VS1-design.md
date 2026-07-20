# VS1 设计包 — 图志目击（See & Record）

> **状态**：Review（待用户批准实现）  
> **剖面**：**Full（轻）** — 有新 Domain 规则（目击解锁）与数据模型；无新全局栈  
> **切片**：VS1  
> **日期**：2026-07-21  
> **PRD 映射**：VS-CDX-01..06（子集）、VS-WLD-04（遭遇表雏形）、VS-DAT 扩展、用户故事「图志目击」；出口见 PRD §11 VS1  
> **依赖**：VS0 **已通过基线**  
> **设计门**：本文件；**实现许可**：未授予  

---

## A. 切片宣告

## 当前切片：VS1 — 图志目击

**目标（玩家体验一句话）**  
在翠语林海灰盒里**看见**至少一种野生精灵占位体，靠近或进入视野后**自动记入图志外观层**，并能打开图志看到「已发现 / 未发现」差异。

**PRD 映射**

| 组 | 本片范围 |
|---|---|
| CDX | 目击 → 外观层；列表+详情最小 UI；未解锁遮蔽 |
| 世界/遭遇 | 遭遇表**数据雏形** + 区域生成至少 1 种生物（**不进战斗**） |
| DAT | C001 保持；新增 **C002 苔行** DataDraft；`enc_r01_demo` 表 |
| SAV | 图志进度写入存档（扩展 Save v0→v1 字段，兼容读旧档） |

**依赖**：VS0 已通过？**是**

**本切片设计点**

| 类 | 有无 | 要点 |
|---|---|---|
| 架构增量 | **有** | Codex 进度服务；野生实体表现；目击检测；图志 UI |
| 技术选型 | **无全局换栈** | 沿用 Unity 2022.3 + 分层；图志 UI 继续 IMGUI 或简单 uGUI（见 B4） |
| 数据模型 | **有** | Codex 层枚举、EncounterTable、Save.codex |
| 算法/规则 | **有（可单测）** | 首次目击解锁外观层；幂等 |

**当前阶段**：设计中 → **待你确认**  
**建议**：审阅本设计包 → 回复「批准 VS1 实现」或修改意见。批准前不写 VS1 业务实现。

---

## B1 范围

### In

1. **野生精灵占位生成**：R01 灰盒内按遭遇表或固定刷点生成 ≥1 种（优先 **C002 苔行**；C001 可作可选第二只或静止展示）  
2. **目击（Sight）**：玩家进入检测范围（或视线锥简化为距离球）→ 记录该 `CreatureDefId` 的**外观层**已解锁  
3. **图志 Codex UI（最小）**  
   - 打开/关闭（默认键 **C** 或 **Tab**，见 B8）  
   - 列表：已发现条目显示名称；未发现显示「？？？」或剪影占位  
   - 详情：外观层文案（科普/诗意至少各一处展示位，数据有则显示）  
4. **Codex 进度进存档**：F5/F9 后目击状态保持  
5. **数据**  
   - `data/creatures/C002_*.json`  
   - `data/encounters/enc_r01_demo.json`（最小表）  
   - l10n 键补全 C002 + UI  
   - `validate_data` 扩展校验 encounters（或先校验 creatures 仍绿 + encounter 文件存在）  
6. **Domain 单测**：`CodexProgress.RegisterSighting` 首次解锁 / 重复幂等  
7. Console 或 HUD 短暂提示：「图志更新：xxx」  

### Out（禁止塞进 VS1）

- 结契 BondingMode 可玩闭环（VS2）  
- 战斗 / BattleRequest 实战（VS3）  
- 完整图志 3 层全部解锁玩法（生态/战斗层可**数据结构预留**，解锁条件可后置；VS1 只强制**外观层**）  
- NPC「授予图志簿」完整对话（可用 HUD 提示代替；完整授予放 VS5/叙事）  
- 隐藏栖息链、天气玩法（VS-WLD-01/02/05 不本片）  
- 终局美术、动画状态机、导航网格  
- 地址系统 Addressables  

### 防膨胀

- 不做「顺便雾衔结契」  
- 不做图鉴筛选/排序/搜索高级功能  
- 不做多区域遭遇  
- 野生怪**不可被攻击**、不触发战斗  

---

## B2 玩家流程

### 标准路径（5–12 分钟烟测）

1. Play → VS0 灰盒世界（`R01_RuntimeRoot`）  
2. 地面上可见 ≥1 个精灵占位（颜色/高度可辨，非纯玩家胶囊同款）  
3. 走向该生物 → 进入目击半径 → 提示「图志更新」  
4. 按 **C**（或约定键）打开图志 → 列表中该条目由「？？？」变为名称（如「苔行」）  
5. 点选详情 → 见外观层描述（双文案位有则显示）  
6. **F5** 存档 → 重进 Play 或 **F9** → 图志仍显示已发现  
7. （可选）第二次靠近同一生物 → 无重复刷屏骚扰（至多静默幂等）  

### 失败与重试

| 情况 | 处理 |
|---|---|
| 数据缺 C002 | Console 警告；不生成该种；DoD 失败 |
| 未目击就开图志 | 列表可为空或全「？？？」；不崩溃 |
| 旧存档无 codex 字段 | 读档时默认空进度，不炸 |

### 时长预算

- 开发验收路径 ≤15 分钟  
- 非内容时长目标（完整 60 分钟脚本仍属后续 VS）  

---

## B3 架构增量

### 新增/修改模块

| 模块 | 层 | VS1 职责 |
|---|---|---|
| `CodexLayer` 枚举 | Domain | Appearance / Ecology / Battle / Folklore |
| `CodexProgress` | Domain | 每 DefId 已解锁层集合；`RegisterSighting` |
| `ICodexService` / 实现 | Application | 查询、登记目击、对外事件 |
| `EncounterTable` def | Domain/Data | 表 id + entries{defId, weight} |
| `JsonDataCatalog` 扩展 | Infrastructure | 加载 encounters；GetEncounterTable |
| `WorldSession` / Save | Application/Infra | `codex` 进度序列化（Save version **1**） |
| `WildCreatureView` | Presentation | 占位 mesh + defId + 可选浮动名 |
| `SightingSensor` | Presentation | 距离检测 → 发 Intent/调用 Application |
| `CreatureSpawnDirector` | Presentation/App | 读表或固定点生成野生视图 |
| `CodexScreen` | Presentation | 图志 UI（开/关、列表、详情） |
| Domain 测试 | Tests | 目击解锁单测 |

### Mode

- **仍主 Mode = Exploration**  
- 图志作为 **Exploration 上叠加的 UI 状态**（非独立 GameMode），避免 VS1 引入 MenuMode 复杂栈  
- Bonding/Battle 保持 Stub  

### 与 §7A 对齐

- 进度真相：`WorldSession` / CodexProgress，不在 View 里私藏唯一真相  
- Domain 无 Unity；目击判定的**规则结果**在 Domain（「登记外观层」），**距离检测**在 Presentation  
- 定义仍 `data/**`；存档只存解锁进度  

### 生成方案对比

| 方案 | 优点 | 代价 | 结论 |
|---|---|---|---|
| A. 固定刷点 + 表声明 defId | 简单稳定、易验收 | 欠动态 | **推荐 VS1** |
| B. 纯随机权重刷怪 | 更像成品 | 调参、空刷、难复现 | VS3+ 再强化 |
| C. 场景手工摆 Prefab | 美术友好 | 无 Editor 流程易碎 | 后置 |

**推荐 A**：`RuntimeWorldBuilder` 或独立 `Vs1WorldBootstrap` 在 R01 生成 2–4 个固定点；点位配置可读 encounter 表的 def 列表（权重可先忽略，只取 entries）。

### 目击检测方案对比

| 方案 | 优点 | 代价 | 结论 |
|---|---|---|---|
| A. 玩家 Overlap 球 / 距离 | 实现快、稳 | 无遮挡 | **推荐** |
| B. 真视线 Raycast | 真实 | 灰盒遮挡坑 | 否 |
| C. 仅触发器进入区域 | 简单 | 多 collider | 可选补充 |

**推荐 A**：`SightingSensor` 在玩家上，每 0.2s 扫 `WildCreatureView`，距离 ≤ `sightRadius`（默认 8m）触发一次登记。

---

## B4 技术选型（仅增量）

### 沿用全局？

**是**：Unity **2022.3 LTS**（以本机验收为准）、Domain/App/Infra/Pres 分层、仓库 `data/`、F5/F9 存档、运行时灰盒。

### 增量决策

| 议题 | 推荐 | 理由 |
|---|---|---|
| 图志 UI | **IMGUI 面板**（与 VS0 HUD 一致） | 零 UXML 资产、少踩坑；VS6 前可换 UI Toolkit |
| 打开键 | **C**（Codex）；冲突则 **Tab** | 文档写死；手柄后置 |
| 野生外观 | Primitive 变体（球/扁胶囊/色块）+ 可选 TextMesh 名 | 可辨即可 |
| JSON | 继续 `JsonUtility` + DTO | 与 VS0 一致 |
| Save 版本 | **0 → 1**：增加 codex 映射；读 0 当空 codex | 防丢档哲学 |
| 遭遇表校验 | validate_data 增加 encounters 可选扫描 | 渐进 |

### 新第三方

**无。**

---

## B5 数据模型

### Codex（Domain）

```text
enum CodexLayer { Appearance = 0, Ecology = 1, Battle = 2, Folklore = 3 }

class CodexProgress {
  // defId -> set of layers
  RegisterSighting(defId) -> bool changed  // unlocks Appearance; idempotent
  IsUnlocked(defId, layer) -> bool
  GetUnlockedDefIds() -> list
}
```

### CreatureDef

沿用 VS0 schema；C002 必填 id/name_key/aspect_primary/regions。

### EncounterTable（新文件）

```json
{
  "id": "enc_r01_demo",
  "region": "R01",
  "entries": [
    { "def_id": "C002", "weight": 1.0 },
    { "def_id": "C001", "weight": 0.3 }
  ]
}
```

VS1 生成器可：只取 weight>0 的 def 做固定点分配，不必实现加权随机。

### Save v1

```json
{
  "version": 1,
  "savedAtUtc": "...",
  "areaId": "R01",
  "player": { "x", "y", "z", "yaw" },
  "flags": [],
  "party": [],
  "codex": [
    { "defId": "C002", "layers": ["Appearance"] }
  ]
}
```

读档：`version==0` → codex 空；未知未来 version → 拒绝或警告（与 VS0 策略一致）。

### 本地化（增）

```text
creature.C002.name
creature.C002.codex.science
creature.C002.codex.poem
ui.codex.title
ui.codex.unknown
ui.codex.layer.appearance
ui.codex.opened_hint
ui.prompt.codex_updated
```

---

## B6 算法与规则

### 目击登记（可单测）

```text
function RegisterSighting(progress, defId):
  if defId empty: return false
  if progress has Appearance for defId: return false
  progress add Appearance for defId
  return true
```

Presentation：

```text
if distance(player, wild) <= sightRadius:
  if app.Codex.RegisterSighting(wild.DefId):
    show prompt + optional SFX later
```

### 生成

```text
points = predefined world positions (3)
defs = encounter.entries.map(e => e.def_id)  // 至少 C002
for i, point in points:
  def = defs[i % defs.length]
  spawn WildCreatureView(def, point)
```

### 复杂度

O(n) 野生数量；n≤16 足够。

---

## B7 接口契约

### Application 用例

- `RegisterCreatureSighting(CreatureDefId) -> bool`  
- `IsCodexUnlocked(CreatureDefId, CodexLayer) -> bool`  
- `GetCodexEntries() -> read models for UI`  
- （保留）`SaveGame` / `LoadGame` 含 codex  

### Domain

- `CodexProgress`  
- `CodexLayer`  
- `CreatureDefId`（已有）  

### Infrastructure

- Catalog: `TryGetEncounterTable(id)`  
- Save: 序列化 codex  

### Presentation

- `WildCreatureView.DefId`  
- `SightingSensor`  
- `CodexScreen.Open/Close/Toggle`  

### Events（可选薄）

- `CodexAppearanceUnlocked(defId)` — 可用 C# event / 回调，避免重型总线  

---

## B8 UI/UX

### 信息架构

```text
HUD（已有）
  + 键位提示可含「C 图志」（可选一行）

Codex 面板（模态或半透明）
  左：列表（已发现名 / 未发现 ???）
  右：详情
     - 名称
     - 层页：VS1 只亮「外观」
     - 科普文案 / 诗意文案（有 key 则显示）
  关闭：C 再按 / Esc
```

### 反馈链

目击成功 → 立即 prompt「图志更新：{名}」→ 列表可验证。  

### 教学

无 NPC 长教程；首次打开图志可用一行 hint。  

### 输入

| Action | 键 |
|---|---|
| ToggleCodex | C（备选 Tab） |
| CloseCodex | C / Esc |
| 其余 | 沿用 VS0 |

图志打开时：**可选择暂停玩家移动**（推荐暂停，防边走边点）。  

---

## B9 内容清单

| 类型 | 项 | 占位？ |
|---|---|---|
| 数据 | C002 苔行 JSON | 文案真、模型 PH |
| 数据 | enc_r01_demo | 是 |
| 表现 | 野生 PH mesh（C002 主色偏绿褐） | 是 |
| UI | 图志面板 | IMGUI |
| 文案 | C002 双文案 + UI 键 | 中文 |
| 音频 | 可选发现音 | P1，可不做 |
| 结契/战斗 | 无 | — |

内容管线：C002 目标 **DataDraft → Validated → Playable（目击）**。  

---

## B10 风险

| 风险 | 缓解 |
|---|---|
| 命名空间再遮蔽 Unity 类型 | 禁止 `namespace ...Camera/Application`；API 写全限定 |
| 灰盒看不见怪 | 放大 scale、高对比色、刷在出生点附近 |
| 图志与存档不同步 | 单测 + 手测 F5/F9 |
| 范围滑向结契 | Out 清单；验收不含结契 |
| validate 过严阻塞 | encounters 校验可 warn 起步 |
| IMGUI 丑 | 接受 VS1；不挡规则正确 |

---

## B11 本切片验收 DoD

- [ ] Play 后出生点附近可见 ≥1 野生占位体（可与玩家区分）  
- [ ] 进入目击范围后，出现图志更新提示（或等价可观察反馈）  
- [ ] 打开图志可见该 def **已解锁外观层名称**  
- [ ] 未目击物种显示未知态（非崩溃、非空白误导成已发现）  
- [ ] 详情能显示至少 1 段外观相关文案（若配置了 science/poem）  
- [ ] F5 → 移动/重开流程 → F9（或重进）后目击进度仍在  
- [ ] Save `version >= 1` 且含 codex 结构  
- [ ] `data/creatures` 含 **C002**；`python tools/validate_data.py` 通过  
- [ ] Domain 单测：首次目击 true、再次 false、外观层已解锁  
- [ ] 无结契成功入队、无战斗结算  
- [ ] 架构：Codex 进度不在纯 View 静态里作为唯一真相  
- [ ] STATUS 可标为待验收 / 通过  

---

## B12 实现任务序（批准后）

1. Domain：`CodexLayer` + `CodexProgress` + 单测  
2. Application：Codex 用例接入 `AppSession` / `WorldSession`  
3. Save v1：读写 codex；兼容 v0  
4. 数据：C002 + enc_r01_demo + l10n；validate 保持绿  
5. Catalog 加载 encounter（可最小）  
6. `WildCreatureView` + 固定点生成  
7. `SightingSensor` → RegisterSighting + prompt  
8. `CodexScreen` 开闭/列表/详情  
9. 键位 C / Esc；打开时可选锁移动  
10. 手测 B11 + 更新 STATUS / 实现报告  

---

## B13 开放问题（默认可批）

| ID | 问题 | 默认推荐 | 你可改 |
|---|---|---|---|
| Q1 | 图志键 | **C** | Tab / J |
| Q2 | 图志打开时是否锁移动 | **锁** | 不锁 |
| Q3 | 野生是否含 C001 | **以 C002 为主**；可选再刷 1 只 C001 | 只要 C002 |
| Q4 | 图志 UI 技术 | **IMGUI** | 坚持本片上 UI Toolkit |
| Q5 | 生态层是否可在 VS1 用「观察交互」解锁 | **不做**（只预留层枚举） | 加最简观察交互 |

无阻塞级分叉：均可按默认推进。

---

## 设计门自检

- [x] 映射 PRD VS1 出口与 CDX/WLD 子集  
- [x] 不破坏 Domain 纯净与 VS0 Mode 骨架  
- [x] 存档影响写清（v1 + codex）  
- [x] 验收可观察  
- [x] 无未声明的全局栈变更  
- [ ] **等待用户批准实现**  

---

## 请求实现许可

设计包：`docs/production/slices/VS1-design.md`

**推荐一句话**：固定点生成 C002 占位体 + 距离目击解锁外观层 + IMGUI 图志 + 存档 codex；不做结契/战斗。

请回复：

- **「批准 VS1 实现」** / **「按方案开工」**，或  
- 修改意见（先改设计包再实现）  
