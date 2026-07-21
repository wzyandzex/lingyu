# VS3 设计包 — 可读教学战（First Battle）v1.1

> **状态**：**Approved for Implement → Implemented（待验收）**（用户批准 2026-07-21）  
> **剖面**：**Full**  
> **切片**：VS3  
> **日期**：2026-07-21  
> **依赖**：VS0–VS2 已通过基线  
> **规格对齐**：`docs/systems/battle.md` v0.2 + 本包锁定的教学子集  
> **实现许可**：未授予  

---

## 0. 制作组结论（先读）

### 推荐方案（默认执行）

**一场可复述的 1v1 教学战**：用已结契（或试用）的 **雾衔 C001**，在林缘 **失谐余烬裂口** 对战 **E001 烬屑螨**；Domain 结算伤害与澄相；UI 只播报。  
**本场只教一件事：澄相相生/相斥（有效 · 一般 · 受阻）。**

| 锁定项 | 值 |
|---|---|
| 敌主澄相 | **焰序 pyric**（失谐灼燃的林屑螨，非 C004 棘影） |
| 我方主战技 | **潮汐 sk_mist_veil** → 对焰序 **×1.5 有效** |
| 对照技 | **蔓息 sk_vine_whisper** → 对焰序 **×0.5 受阻**（故意教「不是每招都强」） |
| 敌技 | **焰序 sk_ember_nibble** → 对潮汐雾衔 **×0.5**（玩家感到「它打我不疼」的可读反向） |
| 先手 | **永远我方先手**（spe 本片不排序） |
| 随机 | **无**（倍率与伤害确定） |
| 命中 | **必中** |
| 攻防 | **仅 atk/def**（spa 忽略） |
| 防守 | **本回合承伤 ×0.5** |
| 雾场地 | 开战宣告 + **潮汐技能 ×1.1** |
| 逃跑 | **立即成功** 回探索 |
| 战后 HP | **双方逻辑回满**（不写长期战损） |
| 站位 | **不做** |
| 试用雾衔 | **仅 Party 空时**注入 1 只，并提示；已有结契实例则用真伙伴 |

### 世界一流体验一句话

> 玩家赢的时候会说：「我用雾水克它的火屑」，而不是「我平 A 平死了」。

### 关键分叉（仅 1 个仍可由你改）

| 分叉 | 默认 | 备选 |
|---|---|---|
| 敌澄相教学模型 | **A 焰序余烬（推荐）** | B 敌墟响 + 改矩阵（潮/心对墟 1.5）— 改动面大，不推荐本片 |

其余审查项已收束为默认，不再开放讨论。

---

## A. 宣告

**目标**：雾衔打赢一场可读 1v1；相性三档可复述；Event 驱动表现。

| 项 | 内容 |
|---|---|
| PRD | Beat3 系统原子；BTL-01/02/03/04/06/08；BTL-07 最小（见下）；BTL-05 不做 |
| 支柱 | D 主；轻触失谐主题（余烬杂兵） |
| 非本片 | E5 全叙事场、4–7 场铺量、站位、完整天气、棘影伦理战 |

### BTL-07 最小满足（审查锁定）

| 做 | 不做 |
|---|---|
| 胜/负人话 Message | 经验曲线 / 升级 |
| **首胜**解锁 C001 或 E001 相关 **Codex Battle 层**（对 C001：解锁自己战斗笔记；对 E001：若不进图鉴则只 Message） | 任务旗标链 |
| 回 Exploration | 掉落经济 |

**推荐首胜 Codex**：`Register` C001 的 `CodexLayer.Battle`（文案用已有战斗笔记 key 或新增 `creature.C001.codex.battle`）。E001 默认 **不进图鉴列表**（`regions` 不含展示或 id 前缀 E 过滤），避免垃圾图鉴。

---

## B1 范围

### In
1. BattleMode 真切换；与 Bonding/Codex/Party UI 互斥  
2. 入口 Interactable：失谐余烬裂口（出生点可见）  
3. 1v1：Party[0] vs E001；Party 空 → 试用 C001 + 提示  
4. Domain `BattleSimulator` + `ElementMatrix` + `BattleEvent[]`  
5. 技能数据 `data/skills/*`；C001.skills 非空；E001 敌定义  
6. IMGUI 战斗 UI：HP、技能（澄相+预告）、日志、防守、逃离  
7. 雾宣告 + 潮汐 ×1.1  
8. 单测：矩阵边；固定序列击倒  

### Out
站位、多敌、换人、道具、PVP、Timeline 大片、持久战损、第二教学因果（天气/换人）。

---

## B2 流程

1. （建议）已 VS2 结契雾衔；否则自动试用并提示「暂以旅伴雾衔应战」  
2. E 裂口 → 开战 Message：「林雾里蹿出烬屑螨 — 看清澄相」  
3. 玩家回合：点 **雾幕**（预告：有效）→ 结算飘字「效果绝佳」  
4. 敌回合：余烬啮咬 → 「效果不理想」（打潮汐）  
5. 可试 **缠藤低语** → 「效果不理想」（蔓息打焰序）— 对照教学  
6. 再用雾幕收掉 → 「试炼通过 · 失谐余烬散了」→ 探索  
7. 再战可重复；败则「先退一步」回探索，不毁队  

### 时长
单场 2–5 分钟；烟测 ≤10 分钟。

---

## B3 架构

沿用设计 v1.0 分层；补充：

| 项 | 锁定 |
|---|---|
| 先手 | 玩家 → 敌（固定） |
| ActiveBattle | **WorldSession** 或 Bootstrap 单处；战后 null |
| HP | 开战快照；**不写** Save 长期字段 |
| 战斗中 F5 | **禁止**或忽略 ActiveBattle（推荐开战时禁用存档提示） |

### ElementMatrix（VS3 实现常量，与 battle.md 一致）

元素 id：`verdant` | `tidal` | `pyric` | `wane`（及扩展默认 1.0）

```text
攻\防    verdant  tidal  pyric  wane
verdant  1.0      0.5    0.5    1.0
tidal    1.5      1.0    1.5    1.0
pyric    1.5      0.5    1.0    1.0
wane     1.0      1.0    1.0    1.0
```

注：上表 verdant→pyric=0.5、tidal→pyric=1.5 用于 **雾衔双技能对照教学**；与 battle.md 原文「蔓息强焰」在完整表中需统一——**本片以教学对照为准**，完整表在 battle 规格升 v0.3 时与 taxonomy 克制句对齐。  
（制作决策：切片教学可读 > 过早冻结全球表；v0.3 回写 battle.md。）

### 伤害

```text
raw = power * atk / max(1, def)
raw *= matrix(skill.element, target.primary)
if weather==fog && skill.element==tidal: raw *= 1.1
if target.guarding: raw *= 0.5
dmg = max(1, floor(raw))
```

### 敌 AI
每回合同一 `sk_ember_nibble`。

---

## B4 技术

2022.3；IMGUI；JsonUtility；skills+enemies 数据；零随机；无新包。

---

## B5 数据清单（实现必交）

| 文件 | 要点 |
|---|---|
| `skills/sk_mist_veil.json` | tidal, power 40 |
| `skills/sk_vine_whisper.json` | verdant, power 35 |
| `skills/sk_ember_nibble.json` | pyric, power 28 |
| `creatures/C001` | skills 填上两招；stats 已有 |
| `enemies/E001_ember_mite.json` 或 creatures 前缀 E | pyric, hp≈40, atk/def 教学向 |
| l10n | 技能/敌/有效一般受阻/胜负/试用提示/入口 |

validate：skill id 可解析；C001.skills 非空。

---

## B6–B7 接口（摘要）

```text
TryStartTutorialBattle()
SubmitPlayerAction(Skill|Guard|Flee)
GetBattleViewModel() // 只读快照 + 待显示 events
BattleSimulator.Resolve → IReadOnlyList<BattleEvent>
```

Events 最小：BattleStarted, WeatherAnnounced, TurnStarted, ActionSelected, DamageApplied(effectiveness), Fainted, BattleEnded, Message。

---

## B8 UX（世界一流信息）

- 技能按钮：名称 · 澄相 · **预告**（对当前敌）  
- 快捷键 **1 / 2** 对应技能；**G** 防守；**R** 逃离（或按钮）  
- 日志每回合 ≤4 行  
- 文案禁用「消灭」；用「驱散 / 试炼通过」  
- 三档文案：**效果绝佳 / 效果一般 / 效果不理想**（辅色可选，不唯色）  
- **否决**：两招预告与结算全是「一般」且无对照；UI 私改 HP  
- **不否决**：方块敌、无动作捕捉  

---

## B9 内容调性

| 角色 | 调性 |
|---|---|
| 雾衔 | 已是伙伴，出战有陪伴感（一句上场 log） |
| E001 烬屑螨 | 失谐灼过的林屑聚形，杂兵，不抢棘影识别 |
| 裂口 | 青篱曾封过的小裂 — 试炼感非无意义刷怪 |

---

## B10 风险

| 风险 | 缓解 |
|---|---|
| 教不会 | 双技能对照 + 预告 + 结算同词 |
| 公式分叉 | 单测唯一真相 |
| 无伙伴 | 试用注入 |
| 矩阵与全球表争议 | 本片锁定教学表；战后升 battle v0.3 |

---

## B11 DoD

- [ ] E 开战；有/无结契均可（试用规则正确）  
- [ ] 雾幕预告「有效」，结算同文案；缠藤「不理想」可对照  
- [ ] 敌打我可见「不理想」  
- [ ] 可胜可败；败不毁队  
- [ ] Domain 单测矩阵 + 击倒序列  
- [ ] 无 UI 私算；无站位；技能数据驱动  
- [ ] 首胜 C001 Battle 层或等价可观察解锁  
- [ ] 回探索后 C/V/结契入口不坏  

---

## B12 任务序

1. ElementMatrix + 公式 + Simulator + Events + 测  
2. skills + E001 + C001.skills + l10n + validate  
3. Catalog 读 skills/enemies  
4. BattleMode + 用例  
5. 入口 + 试用注入  
6. BattleHud + 1/2/G/R  
7. 胜负/Codex Battle 层  
8. 报告 + STATUS  

---

## B13 已关闭分叉

敌相性、先手、防守、雾、逃跑、HP、站位 — 全部默认如上。  
仅当你明确要求时再改敌为墟响方案 B。

---

## 设计门自检

- [x] 审查 P0 已吸收  
- [x] 教学相性可复述  
- [x] 世界一流验收：赢要说得出澄相  
- [ ] **等待用户批准实现**  

---

## 请求实现许可

**推荐一句话**：焰序烬屑螨教学战 + 潮汐有效/蔓息受阻对照 + Domain Event 结算 + 雾 ×1.1；站位不做。

请回复：

- **「批准 VS3 实现」** / **「按方案开工」**  
- 或点名修改（例如坚持方案 B 敌墟响）  
