# 战斗规格（Battle）v0.2

> 状态：VS3 设计前基线；实现以 VS3 设计包 + Domain 单测为准  
> 支柱：Pillar D 战斗与构筑  
> ADR：ADR-003（回合制）

---

## 1. 玩家体验一句话

**每回合都读得懂「为什么疼 / 为什么该换招」，天气与相性是世界语言，不是隐藏修正表。**

风险：信息过载；或做成纯数值无表达。

---

## 2. 切片范围

### In
- 单边或 1v1 回合制  
- 相性表**子集**  
- 基础伤害公式  
- 2–3 异常/态势之一（可选：湿润、破势）  
- `BattleEvent[]` 表现队列  
- 教学战脚本关  

### Out（切片）
- 完整 PVP 平衡  
- 庞大特性库  
- 多单位位置战完整版（若 P0 做站位，仅最小前排/后排）  
- 在线同步  

**站位**：默认 VS3 **P1 后置**；若 VS3 设计包论证「无站位教学战无表达」可升 P0（须在设计包写清）。

---

## 3. 回合流程

```text
StartBattle
  → TurnStart(side)
  → SelectAction(actor)   # 玩家菜单 / AI 策略
  → ResolveAction         # Domain 纯函数式结算
  → Append BattleEvents
  → CheckEnd
  → TurnStart(next) | EndBattle
```

表现层只消费 Event，不得重算伤害。

---

## 4. 伤害公式（语义 → 可单测）

```text
raw = power_scale(skill.power) * atk_stat(user) / def_stat(target)
raw *= element_multiplier(skill.element, target.elements)  # 从表
raw *= weather_multiplier(skill, weather_id)               # 可 1.0
raw *= random_factor(seed)                                 # 切片可收窄到 0.95–1.05 或先 1.0
dmg = max(1, floor(raw))
```

### 4.1 相性子集（切片示例表）

澄相 id 以 `taxonomy.md` 为准。下表为**教学用子集**，全表后置。

| 攻 \ 防 | 蔓息 | 潮汐 | 烬燎 | 墟响 |
|---|---|---|---|---|
| 蔓息 | 1.0 | 0.5 | 1.5 | 1.0 |
| 潮汐 | 1.5 | 1.0 | 0.5 | 1.0 |
| 烬燎 | 0.5 | 1.5 | 1.0 | 1.0 |
| 墟响 | 1.0 | 1.0 | 1.0 | 1.0 |

UI：**有效 / 一般 / 受阻** 文案 + 可选色；色盲模式后置但不依赖唯色。

---

## 5. BattleEvent 枚举（最小）

```text
BattleStarted
TurnStarted { side }
ActionSelected { actor_id, skill_id }
DamageApplied { target_id, amount, effectiveness }
HealApplied { ... }
StatusApplied { ... }
StatusResisted { ... }
Fainted { actor_id }
WeatherAnnounced { weather_id }
BattleEnded { result }
Message { key }   # 教学用
```

可扩展，但 VS3 实现不得在动画回调里分支改 HP。

---

## 6. AI（教学战）

- 脚本序列优先于聪明 AI  
- 每场教学战只教 **一个因果**（相性 / 天气 / 换人预期 三选一）  
- 见 `narrative/onboarding-60m.md` 场次表升格  

---

## 7. 数据

```text
SkillDef: id, element, power, accuracy, effects[]
CreatureBattleStats: hp, atk, def, spd, ...
ElementMatrix: versioned table
BattleParticipantState: current_hp, statuses, ...
```

---

## 8. 验收

- [ ] 公式单测（含相性边）  
- [ ] Event 序可快进表现  
- [ ] 教学战可复述克制原因  
- [ ] 败北有可读信息与恢复路径  

---

## 9. 版本

| 版本 | 日期 | 说明 |
|---|---|---|
| v0.1 | 2026-07-20 | 方向 |
| v0.2 | 2026-07-20 | 公式、相性子集、Event 枚举、流程 |
