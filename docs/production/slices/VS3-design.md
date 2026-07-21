# VS3 设计包 — 可读教学战（First Battle）

> **状态**：Review（待用户批准实现）  
> **剖面**：**Full**（BattleMode + Domain 结算 + Event 表现 + 技能数据）  
> **切片**：VS3  
> **日期**：2026-07-21  
> **依赖**：VS0–VS2 **已通过基线**（须有 Party 雾衔；无则 Dev 补发）  
> **规格**：`docs/systems/battle.md` v0.2 子集  
> **实现许可**：未授予  

---

## A. 切片宣告

## 当前切片：VS3 — 可读的战斗

**目标（玩家体验一句话）**  
用 **C001 雾衔** 打赢一场 **1v1 回合教学战**：看懂技能与澄相「有效/受阻」，胜负由 Domain 结算，UI 只播报不私算。

| 项 | 内容 |
|---|---|
| **PRD** | Beat 3 系统原子；VS-BTL-01/02/03/04(子集)/06/07(子集)/08；PTY-03 可简化为自动出战唯一队员 |
| **支柱** | D 战斗与构筑（主）；C 探索（遭遇入口轻） |
| **试玩** | T0 必做；能复述「为什么这一下更疼/更肉」 |

### 本片只教 **一个因果：澄相相性**

- 场地/天气：**最小雾场地**（乘区 1.1 给潮汐/蔓息技能或仅文案+1.0 数值，见 B4 默认）  
- **站位 VS-BTL-05：本片不做**（P1 后置，写死）  
- 道具/多技能复杂分支：最小（1–2 技能 + 防守可选）

---

## B1 范围

### In

1. **BattleMode** 真切换；进战锁探索输入；Esc 仅「尝试逃跑」（非退出 Mode 作弊）  
2. **入口**：灰盒交互点「失谐裂口 / 教学木桩」**E 开战**（不依赖完整遭遇表随机）  
   - 可选：若 Party 空，开战前 **自动给予试用雾衔实例**（Dev 友好，日志注明）  
3. **1v1 回合制**  
   - 我方：Party 第一只（期望 C001）  
   - 敌方：`E_tutorial_mite` 或数据 `C0xx` 教学敌（墟响/中性弱点清晰）  
4. **Domain `BattleSimulator`**  
   - 回合：玩家选技能 → 结算 → 敌方脚本行动 → 检查胜负  
   - 伤害公式 + **相性子集**（至少 蔓息/潮汐/烬燎/墟响 四方表中用到的边）  
   - **零随机**（`random_factor = 1.0`）便于单测与教学  
5. **`BattleEvent[]` 队列**：Presentation 只消费事件改 HUD/飘字，**禁止**在 UI 里重算 HP  
6. **技能数据** `data/skills/*.json` + CreatureDef.skills 引用；validate 可解析  
7. **战斗 UI（IMGUI）**  
   - 双方名/HP 条  
   - 技能按钮（显示澄相与「有效/一般/受阻」预告）  
   - 日志区：本回合事件短句  
   - 可选防守指令  
8. **胜负结算**  
   - 胜：回 Exploration + prompt；可选解锁图志战斗层字段（若实现成本低；否则只 Message）  
   - 负：回 Exploration + 可再战；**不毁 Party**  
9. **Domain 单测**：伤害 × 相性边；战斗打至 Faint 的确定性序列  

### Out

| 项 | 说明 |
|---|---|
| 站位前后排 | P1 后置 |
| 真·天气系统/昼夜 | VS4/WLD；本片仅战斗内 `weatherId` 常量 `fog` 可选 |
| 多敌人、换人深度、道具背包 | 后置 |
| PVP、经验曲线精调 | 后置 |
| Timeline 演出大片 | 后置；本片飘字+闪白即可 |
| 棘影伦理战完整 | VS5 |

### 防膨胀
- 一场教学战跑通即可，不铺 4–7 场  
- 不做「自动战斗」或跳过结算  
- 敌方 AI = **固定脚本**（每回合同一技能或简单循环），不写行为树  

---

## B2 玩家流程

### 标准路径（烟测 ≤10 分钟）

1. Play → 确保 Party 有雾衔（VS2 结契或自动试用）  
2. 走到 **教学遭遇石/裂口** → **E**「进入共鸣试炼」  
3. 进入 BattleMode：见敌我 HP、技能列表  
4. 点 **雾幕**（或主战技能）→ 日志显示伤害 +「效果绝佳/一般/不理想」  
5. 敌方回合自动攻击  
6. 重复至敌方 HP≤0 →「试炼通过」→ 回探索  
7. （可选）再开一战验证可重复  

### 失败
- 我方 HP≤0 →「先退一步，再试」→ 回探索，雾衔 HP **战斗外满血重置**（VS3 简化，不做持久战损）  

### 教学信息架构
- 开战 Message：「看澄相颜色——有的一击更疼」  
- 技能旁标注攻方澄相  
- 结算后一行 effectiveness 文案  

---

## B3 架构增量

| 模块 | 层 | 职责 |
|---|---|---|
| `ElementId` / 相性矩阵 | Domain | 表查询 multiplier |
| `SkillDef` | Domain + data | power, element, name_key |
| `BattleSide` / `BattlerState` | Domain | hp, maxHp, atk, def, defId, skills[] |
| `BattleAction` | Domain | Skill / Guard / Flee |
| `BattleEvent` + 类型 | Domain | 不可变事件记录 |
| `BattleSimulator` | Domain | Start, Choose, ResolveTurn, Events |
| `BattleRequest` | Domain/App | 敌组合、weatherId、seed(unused) |
| `BattleMode` | App | 真 Mode |
| WorldSession | App | `ActiveBattle` 可选；不把 HP 写进长期存档（VS3） |
| `BattleHud` | Pres | 消费 events 显示 |
| `BattleEntrance` Interactable | Pres | E 开战 |

### 控制流（强制）

```text
Input 选技能 → Application.SubmitBattleAction
  → Domain.Simulator.Resolve → BattleEvent[]
  → Presentation 播放/刷新 UI
禁止：按钮 OnClick 里 hp -= 10
```

### Mode

```text
Exploration --E entrance--> Battle --win/lose/flee--> Exploration
结契中不可开战；开战时关图志/队伍 UI
```

### 方案对比

| 议题 | 推荐 | 否决 |
|---|---|---|
| 天气 | **战斗内常量 fog，乘区 1.0 或弱 1.1** 满足 BTL-04 最小可读 | 全图天气系统 |
| 站位 | **不做** | 本片加前后排 |
| 敌数据 | 专用 `enemy_tutorial_skitter` JSON（非图鉴种亦可） | 硬塞 C004 棘影全套 |
| HP 持久 | **战后回满** | 持久伤残系统 |

---

## B4 技术选型

| 项 | 选择 |
|---|---|
| 引擎/分层 | 沿用 2022.3 + asmdef |
| UI | IMGUI 战斗面板 |
| JSON | JsonUtility DTO；skills + enemies |
| 随机 | **1.0 固定** |
| 动画 | HP 条插值可选；无 Timeline |

---

## B5 数据

### skills（例）

```json
// data/skills/sk_mist_veil.json
{ "id": "sk_mist_veil", "name_key": "skill.sk_mist_veil.name",
  "element": "tidal", "power": 40 }
// data/skills/sk_vine_whisper.json
{ "id": "sk_vine_whisper", "name_key": "skill.sk_vine_whisper.name",
  "element": "verdant", "power": 35 }
// data/skills/sk_enemy_nibble.json
{ "id": "sk_enemy_nibble", "name_key": "skill.sk_enemy_nibble.name",
  "element": "wane", "power": 28 }
```

### C001 skills 引用
更新 `C001_wuxian.json`：`skills: ["sk_mist_veil", "sk_vine_whisper"]`  
`base_stats` 已有 hp/atk/def 等。

### 敌定义（可用 creatures 或 enemies/）
```json
{ "id": "E001", "name_key": "enemy.E001.name",
  "aspect_primary": "wane", "base_stats": { "hp": 36, "atk": 11, "def": 9, ... },
  "skills": ["sk_enemy_nibble"] }
```
教学设计：雾衔 **潮汐/蔓息** 打 **墟响** 为 1.0 或表内有利边；另给一技能打「一般」对照——以 taxonomy 子集为准写死矩阵。

### 相性矩阵（VS3 实现常量类 `ElementMatrix`）
与 battle.md 4.1 一致（verdant/tidal/pyric/wane 映射中英 id）。

### validate
- skills 文件 + creature/enemy 的 skill id 可解析（硬校或至少 C001/E001）  

### l10n
技能名、敌名、有效/受阻/一般、开战/胜/负、入口交互。

---

## B6 算法

```text
function Damage(user, target, skill):
  raw = skill.power * user.atk / max(1, target.def)
  mult = ElementMatrix.Get(skill.element, target.primaryElement)
  // optional: if weather==fog && skill.element in (tidal,verdant): mult *= 1.1
  return max(1, floor(raw * mult))

effectiveness:
  mult >= 1.5 → Super
  mult <= 0.5 → Resist
  else → Neutral
```

敌 AI：
```text
每回合若可行动：使用 skills[0]
```

逃跑：玩家 Flee → 50% 固定成功（零随机则 **VS3 逃跑必成** 或 **消耗回合失败一次再成**——推荐 **必成回探索** 减挫败）

---

## B7 接口

```text
App:
  TryStartTutorialBattle()
  SubmitPlayerAction(BattleAction)
  GetBattleViewModel() // hp, skills, last events
  IsInBattle()

Domain:
  BattleSimulator
  ElementMatrix
```

---

## B8 UI/UX

```text
顶部：敌名 + HP
中部：日志
底部：我方名 + HP + [技能1][技能2][防守][逃离]
```

- 技能按钮禁用条件：非我方选择阶段  
- 相性预告：选中/悬停显示（IMGUI 可固定在按钮旁小字）  
- **禁止**显示内部 atk 公式  
- 否决：UI 私改 HP；无相性反馈的纯数字战  
- 不否决：PH 方块敌  

键位：鼠标点按钮为主；可选 1/2 快捷技能。

---

## B9 内容

| 项 | |
|---|---|
| 入口 Interactable | 裂口石，距出生点可见 |
| 敌 E001 | 深灰紫小体 |
| 技能 3 个 | 上表 |
| 雾场地文案 | 开战一句「林雾弥漫」 |

---

## B10 风险

| 风险 | 缓解 |
|---|---|
| 公式在 UI | 代码审查 + 单测唯一真相 |
| 打不过/秒杀 | 调 E001 hp 与 power；教学向 |
| 无雾衔开战 | 自动试用实例 |
| 与 Bonding Mode 冲突 | 互斥 SwitchTo |
| validate 过严 | 先 skills+E001+C001 |

---

## B11 DoD

- [ ] E 入口可进 BattleMode  
- [ ] 1v1 可操作技能并看见 HP 变化  
- [ ] 至少一次结算展示相性文案（有效/一般/受阻三选一可见）  
- [ ] 可胜利回探索；可失败再战  
- [ ] 事件驱动：断点审计无 UI 私算  
- [ ] Domain 单测：相性倍率边 + 一场打完  
- [ ] skills 数据驱动；validate 相关项绿  
- [ ] 无站位/无第二套 AI/无结契回归损坏  
- [ ] F5 不要求存战斗中途（战斗中存档可选禁止）  

---

## B12 任务序

1. Domain ElementMatrix + SkillDef + Battler + Simulator + Events + 测试  
2. data skills + E001 + C001 skills 字段 + validate  
3. Catalog 加载 skills（或开战硬读）  
4. BattleMode + App 用例  
5. 入口 Interactable  
6. BattleHud 消费 events  
7. 胜负回探索 + 自动试用雾衔  
8. 手测报告 + STATUS  

---

## B13 默认可批

| ID | 默认 |
|---|---|
| Q1 站位 | **不做** |
| Q2 雾乘区 | **1.1 弱加成** 或 1.0+纯文案（实现选弱加成） |
| Q3 逃跑 | **必成** |
| Q4 战后 HP | **回满** |
| Q5 敌 id | **E001** 独立敌人，不进图鉴强制 |

---

## 设计门自检

- [x] 映射 PRD VS3 / BTL 子集  
- [x] Domain 可测；表现不结算  
- [x] 单因果教学：相性  
- [ ] **等待用户批准实现**  

---

## 请求实现许可

设计包：`docs/production/slices/VS3-design.md`

**一句话**：BattleMode + Domain 1v1 结算 + 相性可读 + Event/IMGUI；入口 E 开战；站位与完整天气后置。

请回复：

- **「批准 VS3 实现」** / **「按方案开工」**  
- **「先多角色审查」**  
- 或修改意见  
