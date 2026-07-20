# 结契与羁绊规格（Bonding）v0.2

> 状态：VS2 设计前基线；实现以 VS2 设计包 + 单测为准  
> 支柱：Pillar A 结契与共鸣、Pillar B 养成与羁绊  
> ADR：ADR-004

---

## 1. 玩家体验一句话

**接近一只野生生命时，你在学它的语言，而不是砸一个成功率条。**

设计目的：失败给信息；成功有仪式感与关系起点。  
风险：做成 QuTE 小游戏或隐藏百分比黑盒。

---

## 2. 概念

| 术语 | 定义 |
|---|---|
| 结契（Bonding） | 从野生到同伴的仪式/过程，产出 Party 成员与图志深度 |
| 戒备（Guard） | 野生个体对威胁的防卫程度，高则易逃逸/敌对 |
| 理解（Attune） | 玩家行为与该物种「可读信号」对齐的程度 |
| 共鸣瞬间 | 理解足够且戒备受控时的确认窗口 |
| 羁绊（Affinity） | 结契后长期关系值，影响技能表现/形态稳定/事件 |

---

## 3. 通用状态机（形式化）

```text
States:
  IdleInWorld
  Encountered          # 已目击/进入结契场
  Reading              # 观察阶段
  Approaching          # 靠近/同步
  Testing              # 物种模板试炼（静随/节奏/等）
  ResonanceWindow      # 可确认结契
  Success
  Failed_Rebuff        # 被拒，信息保留
  Failed_Flee          # 逃离
  EscalateBattle       # 转战斗（可选）
  Abort

合法转移（摘要）:
  Encountered → Reading
  Reading → Approaching | Failed_Flee | Abort
  Approaching → Testing | Failed_Rebuff | Failed_Flee
  Testing → ResonanceWindow | Failed_Rebuff | Failed_Flee | EscalateBattle
  ResonanceWindow → Success | Failed_Rebuff | Failed_Flee
```

### 3.1 内部变量（逻辑）

```text
guard: 0..100
attune: 0..100
patience: 0..100          # 窗口时间压力
template_progress: 0..1   # 物种模板
rng_seed: int             # 可复现
```

### 3.2 事件输入（玩家 Intent 示例）

```text
Observe, Wait, MoveCloser, MoveAway, OfferItem,
SyncBreath, MatchStep, HoldStill, Cancel, ConfirmResonance
```

物种模板决定哪些 Intent 有效；无效 Intent → 小幅 `guard++` 或提示「它不吃这一套」。

---

## 4. 判定口径（可单测伪代码）

```text
function on_intent(state, intent, species_template, ctx):
  if intent not in species_template.allowed[state]:
     guard += species_template.mismatch_penalty
     emit FailInfo(code=MISMATCH, hint=species_template.hint)
     if guard >= FLEE_GUARD: return Failed_Flee
     return state

  apply species_template.effect(intent, ctx)  # 改 guard/attune/progress

  if weather_ok(ctx) and species_template.weather_bonus:
     attune += bonus

  if guard >= HOSTILE_GUARD: return EscalateBattle or Failed_Rebuff
  if guard >= FLEE_GUARD: return Failed_Flee
  if template_progress >= 1 and attune >= ATTUNE_OK and guard <= GUARD_OK:
     return ResonanceWindow
  return next_state
```

**切片默认阈值（可调表，勿写死魔法数散落）：**

| 常量 | 建议初值 |
|---|---|
| ATTUNE_OK | 70 |
| GUARD_OK | 35 |
| FLEE_GUARD | 85 |
| HOSTILE_GUARD | 95 |

**禁止**向玩家展示精确百分比；UI 用行为/表情/雾气/音乐层表达。

---

## 5. 模板：雾衔静随（C001）— 切片主模板

| 阶段 | 正确行为 | 错误行为 | 失败信息码 |
|---|---|---|---|
| Reading | 观察呼吸/雾向 | 冲刺贴近 | `TOO_FAST` |
| Approaching | 侧向缓步、停顿 | 对视压迫、大声动作 | `PRESSURE` |
| Testing | 同步停-走节奏 | 乱节奏、频繁菜单 | `DESYNC` |
| Resonance | 确认共鸣（按键） | 超时、攻击性动作 | `WINDOW_MISS` |

雨天：`DESYNC` 惩罚降低或 `attune` 被动缓增（呼应生态）。

---

## 6. 失败必须给信息

| code | 玩家应理解 |
|---|---|
| TOO_FAST | 太急 |
| PRESSURE | 压迫感 |
| DESYNC | 未跟上它的节律 |
| MISMATCH | 方法不对种 |
| WINDOW_MISS | 时机断了 |
| FLEE | 它离开了（可再遇） |

失败保留图志笔记碎片（VS1+）。

---

## 7. 羁绊（结契后）

```text
affinity: 0..100
sources: 同行、战斗协同、喂食、事件选择、旅居
sinks: 忽视、反复强制战斗风格冲突（后期）
```

切片（VS4）：
- 达到阈值 + 持有/条件 → 露滴蛹进化  
- 羁绊影响 1 个可见战斗表现（如技能附加语气/特效），避免纯隐藏数值  

---

## 8. 数据接口

```text
BondingTemplateDef: id, species_ids[], states, intents, thresholds_ref
BondingSessionState: instance_id, state, guard, attune, seed
AffinityState: creature_instance_id, value, flags
```

---

## 9. 验收

- [ ] 状态机单测：合法转移与失败码  
- [ ] 静随主路径可成功  
- [ ] 至少 3 种失败可区分文案/表现  
- [ ] 无成功率百分比 UI  

---

## 10. 版本

| 版本 | 日期 | 说明 |
|---|---|---|
| v0.1 | 2026-07-20 | 初稿方向 |
| v0.2 | 2026-07-20 | 形式化状态机、伪代码、阈值表、失败码 |
