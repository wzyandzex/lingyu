# VS2 设计包 — 雾衔静随结契（First Resonance）

> **状态**：Review（待用户批准实现）  
> **剖面**：**Full**（新 Mode + 状态机 + 可单测规则 + Party 实例）  
> **切片**：VS2  
> **日期**：2026-07-21  
> **依赖**：VS0、VS1 **已通过基线**  
> **实现许可**：未授予  

---

## A. 切片宣告

## 当前切片：VS2 — 第一份共鸣

**目标（玩家体验一句话）**  
在灰盒林海中对 **C001 雾衔** 完成一次**静随结契**：理解节奏、失败有原因、成功后它成为队伍伙伴（可在简单队伍 UI 中看到）。

| 项 | 内容 |
|---|---|
| **PRD** | Beat 1；VS-BND-01/02/05/06（本片）；出口 PRD §11 VS2 |
| **规格** | `docs/systems/bonding.md` v0.2 QuietFollow 子集 |
| **支柱** | A 结契与共鸣（主）；B 羁绊起点（轻） |
| **试玩** | T0 必做；尽量 T1（静随可读） |

**本片不做**：培育模板（VS4 向）、苔行助径/仪式鹿、战斗、完整 0–5 羁绊系统。

**设计点**

| 类 | 要点 |
|---|---|
| 架构 | 真正 **BondingMode**（替换 Stub）；结契会话；成功 → Party |
| 数据 | Party 存档；C001 结契模板字段；失败码 l10n |
| 算法 | Domain 状态机 + QuietFollow 意图表；**可单测** |
| UI | 结契提示条（步骤/反馈）；**禁止成功率百分比** |

**当前阶段**：待你确认设计 → 批准后实现。

---

## B1 范围

### In

1. **BondingMode** 可进入/退出；结契中探索移动按设计锁定或降权（见 B8）  
2. **C001 雾衔结契体**（与 VS1 目击体区分：可开始结契的野生雾衔占位）  
3. **静随模板 QuietFollow** 可玩闭环：  
   - 观察（Reading）  
   - 侧向缓步接近（Approaching）  
   - 同步停-走（Testing）  
   - 共鸣确认（ResonanceWindow → Success）  
4. **失败可读**：至少 `TOO_FAST` / `PRESSURE` / `DESYNC` 之一在错误操作时出现，可重试  
5. **成功**：生成 `CreatureInstance` 入 **Party**（容量 2–3，VS2 用 1 只即可）；短反馈（prompt + 可选简短停顿，不做 Timeline 大片）  
6. **队伍 UI 最小**：按 **V** 或菜单键查看已有伙伴名（IMGUI）  
7. **存档**：Party 进 Save（version **2** 或扩展 v1 字段，兼容读旧档）  
8. **图志**：结契成功可确保 C001 外观层已解锁（若未目击则补登记）  
9. **Domain 单测**：静随合法成功路径；至少一条失败转移 + 失败码  

### Out

| 项 | 归属 |
|---|---|
| NurtureEnvironment / AssistTrail / RitualLite | 后片 |
| 完整 guard/attune 百分比 UI | 永不（原则） |
| 战斗、进化 | VS3/VS4 |
| 多队员切换出战 | VS3 |
| 电影级结契 Timeline | VS6 可加码 |
| 棘影不可结契逻辑 | 数据标记可预留，玩法 VS5 |

### 防膨胀
- 不做第二模板「顺便」  
- 不做复杂 AI 寻路巡逻（可用路径点伪巡逻）  
- 不做付费/道具秒结契  

---

## B2 玩家流程

### 标准成功路径（目标 ≤12 分钟含熟悉，开发烟测 ≤8 分钟）

1. Play → R01 灰盒（VS0/1 内容仍在：C002、石碑、图志 C）  
2. 发现 **雾衔** 占位（雾白-青绿、比苔行更「立」一点但仍小）  
3. 靠近交互 / 自动进入结契场 → **BondingMode**  
4. **Reading**：停步观察（HoldStill / Wait）→ 提示「侧耳听雾」  
5. **Approaching**：缓步靠近（禁止冲刺键连点；VS2 无冲刺则快速连走触发 TOO_FAST）  
6. **Testing**：按提示 **停—走—停** 同步 2–3 次  
7. **Resonance**：提示出现时按 **F**（或 E）确认掌心澄气  
8. Success → prompt「雾衔愿与你同行」→ Party 有 C001 → 回 Exploration  
9. **V** 打开队伍见「雾衔」  

### 失败路径（须可复现）

| 操作 | 期望 |
|---|---|
| Reading 阶段猛冲贴脸 | 失败码 TOO_FAST / 喷雾遁离可重进 |
| Approaching 连按冲刺式移动 | PRESSURE |
| Testing 乱按节奏 | DESYNC |
| Resonance 超时不按 | WINDOW_MISS，可重开窗口或回 Testing |

失败后：**不毁档**；可再次对同一刷新点/重刷雾衔尝试。

### 时长预算
- 单次成功仪式：1–3 分钟  
- 含失败重试：≤5 分钟可学会  

---

## B3 架构增量

### 模块

| 模块 | 层 | 职责 |
|---|---|---|
| `BondingState` / `BondingIntent` | Domain | 状态与意图枚举 |
| `QuietFollowTemplate` | Domain | 合法意图、效果、阈值 |
| `BondingSession` | Domain | guard/attune/progress；`ApplyIntent` → 新状态+FailCode? |
| `BondingService` / 用例 | Application | StartBonding(defId)、ApplyPlayerIntent、CompleteSuccess |
| `CreatureInstance` + `PartyState` | Domain/App | 实例 id、defId、昵称可选 |
| WorldSession | App | 持有 Party + 可选进行中 BondingSession 引用 |
| `BondingMode` | App | 真 Mode；Enter/Exit 切输入与 UI |
| `BondingPresenter` / UI | Pres | 步骤文案、失败提示、确认键 |
| `WuxianBondableView` | Pres | 雾衔实体 + 触发开始结契 |
| Save | Infra | party[] 序列化 |

### Mode 流

```text
Exploration --start bond--> Bonding --success/fail/abort--> Exploration
```

- 结契中：**不**开图志也可（C 可禁用）；移动仅服务静随意图  
- GameModeRouter：注册真 `BondingMode`，移除纯 Stub 或 Stub 仅占位其它  

### 方案对比：结契触发

| 方案 | 优点 | 代价 | 结论 |
|---|---|---|---|
| A. 交互键对雾衔「尝试共鸣」 | 清晰 | 多一步 | **推荐** |
| B. 进入范围自动进 Mode | 沉浸 | 误触 | 否 |
| C. 仅任务旗标强制 | 叙事 | 灰盒弱 | 后置 |

### 方案对比：Testing 同步

| 方案 | 优点 | 代价 | 结论 |
|---|---|---|---|
| A. 提示「停/走」玩家用移动输入对齐 | 符合静随 | 调参 | **推荐** |
| B. 纯 QT 按键序列 | 简单 | 易成 QTE | 否（违支柱） |
| C. 小游戏进度条 | 直观 | 像砸球 | 否 |

**推荐 A**：雾衔以动画/位移节奏「走/停」；玩家在「走」相应移动，「停」时 HoldStill；错则 DESYNC。

### §7A
- 规则在 Domain；Presentation 只发 Intent  
- Party 进度在 WorldSession  
- 禁止 UI 里算成功率  

---

## B4 技术选型

| 项 | 选择 |
|---|---|
| 引擎/分层 | 沿用 2022.3 + 既有 asmdef |
| 结契 UI | IMGUI 步骤条（与 VS0/1 一致） |
| 雾衔表现 | Primitive + 色（雾白青）；可 bob 动画 |
| 伪巡逻 | 2–3 路径点 Lerp |
| 随机 | Domain 可注入 `IRandom` 或固定 seed 单测；VS2 可确定性模板 |
| Save | version **2**：增加 `party` 实例列表 |

无新第三方包。

---

## B5 数据模型

### CreatureInstance

```text
InstanceId: string (guid)
DefId: C001
Nickname?: string
// VS2 最小；亲和/等级后置默认 1 / 0
```

### PartyState

```text
MaxSize: 3
Members: List<CreatureInstance>  // VS2 成功后 1
```

### BondingSessionState（内存/可测）

```text
TargetDefId
State: BondingState
Guard, Attune, TemplateProgress
LastFailCode?
Seed
```

### Save v2

```json
{
  "version": 2,
  "player": {},
  "codex": [],
  "party": [
    { "instanceId": "...", "defId": "C001" }
  ]
}
```

读 v0/v1：party 空。

### l10n（例）

```text
ui.bonding.step.reading
ui.bonding.step.approaching
ui.bonding.step.testing
ui.bonding.step.resonance
ui.bonding.fail.TOO_FAST
ui.bonding.fail.PRESSURE
ui.bonding.fail.DESYNC
ui.bonding.fail.WINDOW_MISS
ui.bonding.success
ui.party.title
creature.C001.name  // 已有
```

### 数据驱动模板（可选 VS2 内嵌 C# QuietFollow，VS3+ 再 JSON 化）

VS2 **允许** QuietFollow 逻辑在 Domain 代码内写死表，但失败码与阈值常量集中一处 `BondingThresholds`。

---

## B6 算法与规则

### 状态机（VS2 精简实现集）

```text
Encountered → Reading → Approaching → Testing → ResonanceWindow → Success
任何阶段 Cancel → Abort → Exploration
失败：→ Failed_* → 可 Restart 回 Reading 或销毁会话回 Exploration 留重试点
```

### QuietFollow 意图效果（摘要，实现填表）

| State | Intent | 效果 |
|---|---|---|
| Reading | HoldStill / Observe | attune+, progress+ |
| Reading | MoveCloser 过猛 | TOO_FAST, guard+ |
| Approaching | MoveCloser 缓 | progress+ |
| Approaching | 连续高速位移 | PRESSURE |
| Testing | MatchStep（走窗移动/停窗静止） | progress+ |
| Testing | 反节奏 | DESYNC |
| ResonanceWindow | ConfirmResonance | Success if 窗口内 |
| ResonanceWindow | timeout | WINDOW_MISS |

阈值：沿用 bonding.md（ATTUNE_OK 70 等）——VS2 可略降教学（如 ATTUNE_OK 50）并在常量注明 `// VS2 tutorial softened`。

### 成功副作用（Application）

```text
instance = new CreatureInstance(C001)
party.TryAdd(instance)
codex.RegisterSighting(C001)  // 确保外观层
bondingSession = null
router.Switch(Exploration)
emit prompt success
```

### 单测清单

1. Reading 正确意图 → 进入 Approaching（或 progress 增加）  
2. TOO_FAST 路径设 fail code  
3. 完整成功路径 → Success 且可生成 instance 字段  
4. 幂等：Success 后 session 结束  

---

## B7 接口契约

```text
IBondingService / AppSession:
  bool TryStartBonding(CreatureDefId defId)
  BondingViewModel GetBondingView()  // state, hints, canConfirm
  void ApplyBondingIntent(BondingIntent intent)
  void CancelBonding()

WorldSession:
  PartyState Party
  // Codex already

Presentation:
  BondingHud
  WuxianBondable.Interact -> TryStartBonding
  PartyScreen (V)
```

---

## B8 UI/UX

### 结契 HUD
- 当前阶段一句话（无百分比）  
- 正确操作提示（「停步侧耳」「慢慢靠近」「跟着它走/停」「按 F 伸出掌心」）  
- 失败：短 prompt + 失败码对应文案  
- 成功：庆祝句 + 回探索  

### 输入（结契中）

| 意图 | 键鼠 |
|---|---|
| 移动（缓近/同步走） | WASD |
| HoldStill | 不移动 |
| ConfirmResonance | **F**（与 E 交互区分） |
| Cancel | Esc |

### 体验约束
- 失败必须**可理解**（E6 向）  
- 禁止「再随机一次黑盒」作为唯一反馈  
- 灰盒可简陋；**节奏对错**必须可感  

### 品质否决
- 只有进度条砸满无行为  
- 失败无文案  
- 成功无 Party 记录  

不否决：PH 模型丑、无雾 VFX。

---

## B9 内容清单

| 项 | 说明 |
|---|---|
| C001 结契体 | 雾白青占位；路径点巡逻 |
| C002 | 保留目击，不结契 |
| 文案 | 静随各步 + 失败码 + 成功 |
| 队伍 UI | 列表一名 |
| 音频 | 可选成功音；非必须 |

---

## B10 风险

| 风险 | 缓解 |
|---|---|
| 做成 QTE | 禁止纯按键序列模板；Testing 绑定移动节奏 |
| 调参过难/过易 | 教学向降阈值；单测锁转移 |
| Mode 卡死 | Esc 取消；超时 Abort |
| Save 丢伙伴 | v2 party + 手测 |
| 与 VS1 目击体混淆 | 雾衔单独 prefab/名 `Bondable_C001` |

---

## B11 验收 DoD

- [ ] 可对 C001 开始静随结契并成功入队  
- [ ] 故意错误操作至少触发 **1** 种可读失败并可重试成功  
- [ ] 无百分比成功率 UI  
- [ ] Party UI 可见雾衔  
- [ ] F5/F9 后 Party 仍在  
- [ ] Domain 单测绿（成功路径 + 失败码）  
- [ ] 回 Exploration 输入/Mode 正常；图志 C 仍可用  
- [ ] 无战斗、无第二结契模板  
- [ ] 架构：判定不在 MonoBehaviour 私写公式  

---

## B12 实现任务序

1. Domain：BondingState/Intent/Session/QuietFollow + 测试  
2. Party + CreatureInstance + WorldSession  
3. Save v2 party  
4. BondingMode + Router 接线  
5. Wuxian 实体 + 开始结契  
6. Presenter/HUD 步骤与失败  
7. 成功入队 + Party UI  
8. 手测 B11 + 实现报告  

---

## B13 开放问题（默认可批）

| ID | 问题 | 默认 | 可改 |
|---|---|---|---|
| Q1 | 确认键 | **F** | E（若与交互不冲） |
| Q2 | 教学阈值 | **略降** ATTUNE_OK=50 | 维持 70 |
| Q3 | 雾衔是否同时可目击 | **进结契前可目击** | 结契成功才登记 |
| Q4 | 失败后雾衔 | **短消失再刷新点** | 原地可立刻重试 |
| Q5 | 结契中开图志 | **禁用 C** | 允许 |

---

## 设计门自检

- [x] 映射 PRD VS2 / BND-01/02/05/06  
- [x] Full：状态机可单测  
- [x] 不破坏 VS0/1；无换栈  
- [x] 验收可观察  
- [ ] **等待用户批准实现**  

---

## 请求实现许可

设计包：`docs/production/slices/VS2-design.md`

**一句话方案**：BondingMode + Domain 静随状态机 + 雾衔灰盒仪式；失败有码；成功入 Party 可存档；不做第二模板与战斗。

请回复：

- **「批准 VS2 实现」** / **「按方案开工」**，或  
- 修改意见 / **「先多角色审查」**  
