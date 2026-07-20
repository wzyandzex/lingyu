# 澄界总体架构（Architecture Bible）v1.0

> 状态：**已锁定，开码前强制**  
> 角色视角：世界级游戏架构师 / 技术制作人  
> 变更：必须 ADR，并同步 `AGENTS.md` §7A  
> 配套：`tech-stack.md`（用什么）· 本文（怎么拆、怎么流、谁拥有状态）· `engineering-skeleton.md`（目录落点）

---

## 0. 一句话架构

**以纯逻辑 Domain 为规则内核，以 Application 编排用例，以 Presentation 负责手感与演出，以 Data/Save 分离定义与状态；用明确的 GameMode 切换探索 / 结契 / 战斗 / 对话，而不是一个上帝场景脚本。**

---

## 1. 架构目标与非目标

### 1.1 目标
1. **可测**：战斗、结契、图鉴、进化不依赖进 Play 才能验证  
2. **可扩**：300–600 精灵靠数据扩展，不靠改核心  
3. **可演**：结契/进化/传说有仪式级 Timeline，但不污染规则  
4. **可存**：版本化存档、可迁移、定义与状态分离  
5. **可回放预留**：关键对抗与流程可指令序列化（异步幽灵/观战后置）  
6. **可协作**：边界清晰，Agent 与人类不易互相踩踏  

### 1.2 非目标（架构级）
- 实时 MMO / 帧同步  
- 首日大世界无缝流送全家桶  
- 全面 DOTS/ECS  
- 逻辑写在动画事件或 UI 回调里  
- 单一 `GameManager` 上帝对象  

---

## 2. 逻辑分层（强制）

```text
┌─────────────────────────────────────────────┐
│ Presentation  输入·镜头·动画·VFX·UI·音频触发 │
└─────────────────┬───────────────────────────┘
                  │ Intent / ViewModel / Events
┌─────────────────▼───────────────────────────┐
│ Application     用例编排 · Mode 切换 · 流程  │
└─────────────────┬───────────────────────────┘
                  │ 纯调用
┌─────────────────▼───────────────────────────┐
│ Domain          规则 · 状态机 · 结算 · 判定  │
└─────────────────┬───────────────────────────┘
                  │ 读写抽象
┌─────────────────▼───────────────────────────┐
│ Infrastructure  读表·存档·本地化·时间·平台   │
└─────────────────────────────────────────────┘
         ▲ data/**(defs)     ▲ save(state)
```

| 层 | 可以依赖 | 禁止 |
|---|---|---|
| Domain | 无引擎 | `UnityEngine`、场景、UI、IO 细节 |
| Application | Domain、抽象端口 | 直接找场景节点、写公式 |
| Infrastructure | Domain + Unity API | 业务规则 |
| Presentation | Application 外观 / 事件 | 数值公式、结契成功率私算 |

---

## 3. 运行时骨架：App Session 与 GameMode

### 3.1 启动流

```text
Boot
 → 加载 Settings / DataCatalog / SaveService
 → 创建 AppSession（进程级）
 → Title 或 Continue
 → 进入 WorldSession（一局游戏）
 → 默认 ExplorationMode（R01）
```

### 3.2 两个会话对象

| 对象 | 生命周期 | 拥有 |
|---|---|---|
| **AppSession** | 进程 | 设置、设备输入映射、音频总线、数据目录、全局服务定位 |
| **WorldSession** | 一档存档从加载到退出 | 玩家进度、队伍、图鉴、旗标、世界时钟、当前 Mode |

禁止用 20 个静态单例代替 WorldSession。  
允许一个受控的 `ServiceRegistry`（或组合根 Composition Root）在 Boot 注入，但**游戏状态**必须挂在 WorldSession。

### 3.3 GameMode（同一时刻仅一个主模式）

| Mode | 职责 | 典型输入 |
|---|---|---|
| `Exploration` | 移动、兴趣点、遭遇请求、世界交互 | Move/Interact/Menu |
| `Bonding` | 结契状态机推进与仪式表现协调 | 观察/试探/确认 |
| `Battle` | 回合流程、指令收集、结算、表现队列 | 技能/道具/换位 |
| `Dialogue` | 对话图、选项、任务推进 | 选项/继续 |
| `Menu` | 图志/队伍/设置（可叠在探索上） | UI |
| `Cutscene` | Timeline 演出锁输入 | Skip（若允许） |
| `Title` | 开始/继续 | UI |

**规则**
- Mode 切换只允许通过 `IGameModeRouter` / Application 用例，禁止各处私自 `LoadScene` 冒充模式切换  
- 战斗可以是：**叠加场景**或**同场景战斗舞台**；VS 期推荐独立 Battle 场景降低耦合，但 Domain 不感知场景  
- 切 Mode 时定义：谁暂停、时间是否流逝、输入图切换、UI 文档切换  

```text
Exploration --encounter--> Battle --win/lose--> Exploration
Exploration --start bond--> Bonding --success/fail--> Exploration
Exploration --talk--> Dialogue --> Exploration
Any --flag--> Cutscene --> previous/next
```

---

## 4. 领域边界地图（Bounded Contexts）

> 每个上下文有：**默认拥有的状态**、**对外发布的领域事件**、**禁止越权的事**。

### 4.1 上下文一览

| 上下文 | 拥有状态 | 核心服务 | 发布事件（例） |
|---|---|---|---|
| **Party** | 队伍槽、个体实例 | `PartyService` | `PartyMemberAdded` |
| **Bonding** | 进行中的结契会话 | `BondingSession` / 判定 | `BondSucceeded` `BondFailed` |
| **Battle** | 战场快照、回合、指令队列 | `BattleSimulator` | `BattleEnded` `TurnResolved` |
| **Codex** | 分层解锁进度 | `CodexService` | `CodexLayerUnlocked` |
| **Affinity** | 个体羁绊阶与修饰 | `AffinityService` | `AffinityRankUp` |
| **Evolution** | 待触发/可进化评估 | `EvolutionService` | `EvolutionAvailable` `EvolutionCompleted` |
| **Exploration** | 当前区域实例、兴趣点冷却 | `EncounterDirector` | `CreatureSighted` `EncounterRequested` |
| **World** | 昼夜、天气、季节、区域事件 | `WorldClock` `WeatherService` | `WeatherChanged` `PeriodChanged` |
| **Narrative** | 任务、旗标、对话指针 | `QuestService` `FlagBook` | `FlagSet` `QuestAdvanced` |
| **Inventory** | 道具与数量 | `InventoryService` | `ItemGained` |
| **Save** | 序列化边界（非玩法） | `SaveService` | `GameSaved` |

### 4.2 硬边界

- **Battle 不算遭遇表**：只吃 `BattleRequest`（敌方组合、场地、天气、规则修饰）  
- **Bonding 不算移动**：只吃观察/试探 Intent 与生态上下文快照  
- **Codex 不改基础数值定义**：只改解锁与笔记状态  
- **Presentation 不拥有进度真相**：进度真相在 WorldSession 各域  

### 4.3 跨域编排（Application 的工作）

示例：野生遭遇

```text
Exploration 发现遭遇
 → Application.StartWildBattle
 → 组装 BattleRequest（含 World 天气场地、Party 出战、Enemy 生成）
 → 切 BattleMode
 → BattleSimulator 跑逻辑
 → 结束：写 Codex 战斗层、发奖励、回 Exploration、可能触发 Narrative 旗标
```

示例：结契成功

```text
Bonding 成功
 → 生成 CreatureInstance
 → Party 或牧场收纳
 → Codex 解锁
 → Affinity 置初值
 → Narrative 旗标
 → 表现层播仪式
```

---

## 5. 关键数据模型

### 5.1 定义 vs 实例（必须分离）

| 类型 | 例 | 存哪 |
|---|---|---|
| **Definition（定义）** | `CreatureDef C001`、技能、遭遇表 | `data/**` → DataCatalog |
| **Instance（实例）** | 我的雾衔 #个体，IV/性格/羁绊/昵称 | Save / WorldSession |
| **Progress（进度）** | 图鉴解锁、任务旗标、世界事件 | Save |
| **Ephemeral（临时）** | 当前结契会话、本场战斗 | 内存，可重开不保留或可重连策略另定 |

### 5.2 稳定 ID 契约

```text
CreatureDefId   C001
RegionId        R01
SkillId         sk_mist_veil
QuestId         q_r01_silent_heart
FlagId          flag_r01_tutor_done
ItemId          item_mist_bait
EncounterTable  enc_r01_forest_morning
```

ID 一经进入存档/公开数据，**禁止改名重用**；弃用只能标记 deprecated。

### 5.3 CreatureInstance 最小字段（架构级）

```text
InstanceId
DefId
Nickname?
Level / Exp
Aspects (from def + 可能临时修饰)
StatsRuntime (可重算缓存)
SkillsUnlocked[]
AffinityRank / AffinityPoints
PersonalityAxes
Flags/Traits[]
Origin (region, time, method)
```

数值以 **可从 def + 成长公式重算** 为优先，存档避免冗余权威副本；若缓存必须可校验。

---

## 6. 控制流模式

### 6.1 Intent → Application → Domain → Events → Presentation

```text
Player Input / UI
  → Intent (Move, Interact, ChooseSkill, BondProbe...)
  → Application Handler
  → Domain mutate + result
  → DomainEvent / ApplicationEvent
  → Presentation Adapter (anim, sfx, UI refresh)
```

### 6.2 事件总线策略

- **领域事件**：Domain 内定义，Application 翻译  
- **表现事件**：仅 Presentation 关心（如 `PlayEvolutionTimeline`）  
- 允许进程内 `IEventBus`  
- **禁止**：事件处理器里再偷偷写另一套业务真相  
- **禁止**：深度事件连环超过可追踪边界（编排放 Application）  

### 6.3 战斗表现解耦

```text
BattleSimulator.ResolveTurn()
  → 产出 BattleEvent[]（Damage, StatChange, Faint, Weather...）
  → BattlePresenter 顺序播放
  → 播放期间可锁输入
```

逻辑瞬时结算或逐步结算皆可，但**表现队列不得回写不同的结算结果**。

---

## 7. 场景与资源架构

### 7.1 场景职责

| 场景 | 职责 |
|---|---|
| `Boot` | 组合根、服务注册、加载目录 |
| `Title` | 存档选择 |
| `R01_Main` 等 | 探索关卡几何与兴趣点 |
| `Battle` | 战斗舞台与摄像机架 | 
| `Dev/*Sandbox` | 单系统调试 |

### 7.2 资源引用

- VS 前期：直接引用 / Resources 慎用  
- 生物 Prefab 路径或 Address 写在 def 的 presentation 映射表（可单独 `view_registry`）  
- **Domain 只认 DefId，不认 Prefab 路径**  

### 7.3 视图注册表

```text
ViewRegistry:
  C001 → prefab, animator profile, cry, portrait
```

规则与皮肤分离，避免 def JSON 被美术路径污染到无法校验。

---

## 8. 存档架构

### 8.1 槽位内容（逻辑分组）

```text
SaveHeader (version, playtime, timestamp, area label)
PlayerProfile
Party + Box/Ranch
Inventory
CodexProgress
Affinity/EvolutionProgress (若未内嵌实例)
NarrativeFlags + Quests
WorldClock + WeatherSeed + RegionStates
Settings (可分本地)
```

### 8.2 规则

1. 格式版本号 + 迁移脚本链  
2. 原子写入（临时文件 → replace）  
3. 定义数据不进存档当权威  
4. 读档后：重建 WorldSession → 校验 def 仍存在 → 失败可隔离损坏实例  
5. VS0 可先存 Transform + header，但接口按完整分组设计  

---

## 9. 时间、随机与确定性

| 主题 | 策略 |
|---|---|
| 世界时间 | `WorldClock` 由 World 上下文推进；战斗内可暂停世界时间 |
| 随机 | `IRandom` 注入；战斗/结契可分离种子，利于测试与回放预留 |
| 帧依赖 | Domain **禁止**依赖 `Time.deltaTime` 做规则；表现层用时间 |

---

## 10. UI 架构

```text
UI Toolkit Documents
  → UI Controllers (Presentation)
  → 读 ViewModel / 发 Intent
  → 不直打 Domain 内部状态机
```

- HUD / Codex / Party / BondingPrompt / BattleMenu 分文档  
- 复杂图鉴用虚拟化列表思维（数据可大）  
- UI 文案全部 localization key  

---

## 11. 内容生产架构

```text
docs/ (人读设定)
  ↓ 人工/工具对齐 ID
data/** (机读权威 JSON)
  ↓ validate_data (CI/本地)
Editor Importer
  ↓
Assets/_Project/Data (SO/二进制缓存)
  ↓
DataCatalog 运行时只读
```

**Definition of Content Done**
- schema 过检  
- 本地化 key 齐  
- view registry 有占位  
- 不改 Domain 即可进游戏  

---

## 12. 垂直切片的架构落地顺序

| VS | 架构增量 |
|---|---|
| VS0 | Boot、AppSession、SceneFlow、Player 表现、Interact、HUD、Save 接口、DataCatalog 空壳 |
| VS1 | CreatureDef 加载、Sighted、Codex 外观层、世界生成实体 |
| VS2 | Bonding Domain + BondingMode + 雾衔模板 |
| VS3 | Battle Domain + BattleMode + 事件表现队列 |
| VS4 | Affinity + Evolution 评估 + Timeline 演出端口 |
| VS5 | Narrative flags、世界事件、棘影流程 |
| VS6 | 打磨、性能、校验加固、遥测端口 |

---

## 13. 质量门禁（架构验收）

任何 PR / Agent 交付若违反以下任一条，不合格：

1. Domain 引用 UnityEngine  
2. 在 UI/动画事件里写结契成功率或伤害公式  
3. 新增物种必须改核心 switch/case 才能出现  
4. 存档把整份 def 当唯一权威  
5. 无 Mode 概念的跨系统 `if (inBattle && bonding && dialogue)` 意大利面  
6. 关键规则无测试且不可测  
7. 双轨 UI 或双引擎代码进主仓  

---

## 14. 推荐目录映射（与工程骨架一致）

```text
Scripts/Domain/{Battle,Bonding,Codex,Affinity,Evolution,Party,Common}
Scripts/Application/{Modes,UseCases,Sessions}
Scripts/Infrastructure/{Save,DataLoading,Localization,Random}
Scripts/Presentation/{Player,Camera,Exploration,Bonding,Battle,UI,...}
data/** , Assets/_Project/Data/**
```

---

## 15. 术语

| 术语 | 含义 |
|---|---|
| Def | 静态定义 |
| Instance | 运行个体 |
| Intent | 玩家/系统意图 |
| GameMode | 互斥主交互模式 |
| WorldSession | 单档游戏状态根 |
| DataCatalog | 只读定义库 |
| ViewRegistry | 表现资源映射 |
| BattleEvent | 可播放的战斗结果事件 |

---

## 16. 锁定声明

本架构为澄界开码默认法。  
实现细节可迭代，**分层、Mode、定义/状态分离、Domain 纯净、数据驱动** 四条不可破。
