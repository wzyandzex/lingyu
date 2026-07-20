# VS2 设计包 — 雾衔静随结契（First Resonance）

> **状态**：**Approved for Implement**（多角色审查修订后用户批准 2026-07-21）  
> **剖面**：**Full**  
> **切片**：VS2  
> **依赖**：VS0、VS1 已通过基线  
> **实现许可**：**已授予**（按本修订包 + 实现计划执行）  
> **规格**：`docs/systems/bonding.md` v0.2 QuietFollow 子集  

---

## A. 宣告

**目标**：对 **C001 雾衔** 完成静随结契——学节奏、失败有原因、成功入队可存档。

| 项 | 内容 |
|---|---|
| PRD | Beat1 系统原子；BND-01/02/05/06；出口「雾衔闭环」 |
| **非目标（本片）** | E1 全路径 12 分钟计时（→ VS5/6 标准路径）；BND-03/04 其它模板 |
| 试玩 | T0 必做；建议隔日自测或 1 次外人 10min（T1 轻） |

### 产品定位备注（全切片序列）

VS0–VS6 **串联** = 垂直切片「可代表最终气质的完整核心循环」，**不是** 300 灵 + 10 区商业 1.0 的体量终局。  
品质条与世界一流对齐；**体量与系统完备度**在切片通过后的扩展阶段继续。详见文末 §产品诚实边界。

---

## B1 范围

### In
1. **BondingMode** 真切换；进出恢复探索输入（图志 C 结契中禁用）  
2. **C001 单实体**：可目击（Codex）+ 交互「尝试共鸣」开结契（禁止双模型叠怪）  
3. **QuietFollow** 全链路：Reading → Approaching → Testing → ResonanceWindow → Success  
4. **公开节拍（Testing）**：雾衔 Walk/Hold 二态 + UI 显示期望「跟着走｜停下」+ Domain `phase`  
5. **失败 ≥3 码可区分**：TOO_FAST、PRESSURE、DESYNC（推荐再加 WINDOW_MISS）+ **人话** l10n  
6. **成功**：`CreatureInstance` → Party；短 prompt（禁用「捕获/收服」用语）；Codex 外观层补登记  
7. **队伍 UI（V）** 可见雾衔名  
8. **Save v2** 含 party；兼容 v0/v1  
9. **Domain 单测**：成功路径；≥3 失败码路径；零随机  

### Out
第二模板、战斗、百分比 UI、大 Timeline、天气修正（**明确 N/A**）、QTE 纯按键序列、黑盒重 roll。

### 防膨胀
- Testing 必须绑移动节奏，禁止「连按 F 赢」  
- 阈值可教学向，但 **必须仍能失败**  

---

## B2 流程

### 成功
Play → 见可辨雾衔 → 可先目击 → **E 尝试共鸣** → BondingMode → 停步观察 → 缓近 → 跟走/停 2–3 轮 → **F** 窗口确认 → 入队 → V 可见 → F5/F9 保持。

### 失败（须均可手测）
| 操作 | 码 | 人话方向 |
|---|---|---|
| 猛冲贴脸 / 过速逼近 | TOO_FAST | 太急，雾惊了 |
| 过近压迫、不停逼 | PRESSURE | 靠太近了 |
| 走停反了 | DESYNC | 没跟上它的步 |
| 共鸣窗不按 | WINDOW_MISS | 时机断了 |

失败后：**会话结束，实体仍可交互立刻重试**（不强制长消失）。

### 时长
- 单次成功仪式 1–3 min；学会含失败 ≤5 min  
- **E1 标准路径计时不作为 VS2 硬门**  

---

## B3 架构

| 模块 | 层 | 说明 |
|---|---|---|
| BondingState/Intent/Phase | Domain | Phase: Walk \| Hold（Testing 用） |
| BondingThresholds | Domain | 常量集中；注明 VS2 tutorial 可 ATTUNE_OK=50 |
| BondingSession | Domain | ApplyIntent → state, failCode?, progress |
| QuietFollowRules | Domain | 表驱动意图效果 |
| PartyState + CreatureInstance | Domain/App | |
| WorldSession | App | **持有** Party；**持有** 进行中 BondingSession 或 null |
| BondingMode | App | 真 Mode |
| WuxianView（单实体） | Pres | 目击碰撞 + 交互开结契 + 表现 Walk/Hold |
| BondingHud | Pres | 阶段提示、期望态、失败/成功；无百分比 |
| PartyScreen | Pres | V 开闭 |

### Mode 输入契约
| | Exploration | Bonding |
|---|---|---|
| WASD | 自由 | **可用**（服务静随） |
| E | 交互 | 不用于确认 |
| F | — | ConfirmResonance |
| C 图志 | 开 | **禁用** |
| V 队伍 | 开 | 建议禁用 |
| Esc | — | CancelBonding |

### 失败判据（可测，P0）
```text
SoftSpeed = 3.5 m/s   // 水平速度近似
RushSeconds = 0.45
NearRadius = 1.2 m    // 过近
ApproachOkMin = 2.0 m // 缓近舒适带大致

TOO_FAST: 在 Reading/Approaching，speed > SoftSpeed 持续 >= RushSeconds
          或 Δdistance 向目标关闭速率过快（实现选一，单测用 speed 规则）
PRESSURE: Approaching/Testing 中 distance < NearRadius 且未 HoldStill
DESYNC:   Testing 中 phase=Walk 时速度≈0 过久，或 phase=Hold 时 speed > SoftSpeed
WINDOW_MISS: Resonance 窗口超时（默认 5s）→ 回 Testing，非整段重开
```

### Testing 节拍（P0）
```text
Domain: metronome phase Walk(1.2s) / Hold(0.9s) 循环（可调）
Presentation: 雾衔位移或明显 idle/walk 动画；UI 大字/图标同步期望
Player: Walk 期望 → 需要移动；Hold 期望 → 需要基本静止
连续正确 N=3 次 phase → TemplateProgress 满 → ResonanceWindow
```

---

## B4 选型
沿用 2022.3 分层；IMGUI；**零随机**；QuietFollow 逻辑 C# 内聚常量（后可 JSON）。

---

## B5 数据
- Party save v2: `party: [{ instanceId, defId }]`  
- l10n：步骤、4 失败码人话、成功句、队伍标题  
- C001 剪影：小四足感、雾白青；**非**扁绿苔团、**非**玩家胶囊同款  

---

## B6 成功副作用
```text
party.Add(Instance C001)
codex.RegisterSighting(C001)
session.Bonding = null
router → Exploration
prompt 非捕获用语
```

### 单测
1. 教学阈值下合法意图序列 → Success  
2. TOO_FAST / PRESSURE / DESYNC 各至少 1  
3. WINDOW_MISS → 回 Testing  
4. 无随机  

---

## B7 接口
`TryStartBonding` / `ApplyBondingIntent` / `CancelBonding` / `GetBondingViewModel`  
`Party` 查询 for UI  

---

## B8 UX
- 主提示 1 行 + 失败覆盖 ~2s  
- 期望态「跟着走｜停下」在 Testing 常驻  
- 常驻显示确认键 F  
- 否决：跳过节奏直接 F 成功；失败无文案；成功无 Party  
- 不否决：PH 丑  

---

## B9 内容
雾衔单实体 + 2–3 路径点；文案表；队伍列表。

---

## B10 风险
假 QTE、调参过难、Mode 卡死、双 C001 模型 — 用 P0 节拍/判据/单实体/Esc 缓解。

---

## B11 DoD
- [ ] 静随成功入队 + V 可见  
- [ ] **≥3** 失败码可手测区分 + 人话  
- [ ] Testing 有公开 Walk/Hold 信号（表现或 UI）  
- [ ] 无百分比；不可跳过节奏秒胜  
- [ ] F5/F9 Party 保持（save v2）  
- [ ] Domain 单测绿  
- [ ] Mode 往返正常；结契中 C 禁用、结束后恢复  
- [ ] 单实体 C001；无战斗/第二模板  

---

## B12 任务序
1 Domain 状态机+阈值+测  
2 Party + WorldSession 会话字段  
3 Save v2  
4 BondingMode + 输入契约  
5 Wuxian 单实体 + 节拍表现  
6 HUD  
7 成功入队 + Party UI  
8 手测报告  

## B13 已锁定默认
F 确认；ATTUNE 教学可降但必可失败；失败立刻可重试；结契禁用 C；天气 N/A；零随机。

---

## 设计门自检
- [x] 审查 P0 已写入  
- [x] bonding.md 失败数对齐  
- [x] 用户批准实现  

---

## §产品诚实边界（回应「做完 VS6 是否即世界级成品」）

| 层级 | VS6 通过时 | 仍不是 |
|---|---|---|
| 体验气质 | 可对陌生人证明「共鸣/生态/图志」差异 | 全平台发行包装终稿 |
| 核心循环 | 探索→目击→结契→战斗→羁绊/进化→事件可玩通 | 300–600 灵、8–12 大区内容量 |
| 品质 | 切片 DoD + 试玩门；一流**标准**已落地 | 全系统深度（经济/牧场/竞技/异步） |
| 工程 | 可扩展数据管线与架构 | 运营/本地化/主机认证全家桶 |

**一句话：** VS0–VS6 交付的是**世界级垂直切片（可发行品质的证明体）**，不是「小玩具 demo」，也**尚未**等于完整商业 1.0 大作体量。切片通过后进入扩展阶段，才把「一流证明」做成「一流产品全量」。

本项目 AGENTS：**垂直切片必须代表最终气质**；**未过切片 DoD 不开全图**。二者同时成立。
