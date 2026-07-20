# 内容生产管线（Content Pipeline）v1.0

> 状态：强制（与代码微切片并行）  
> 目的：避免「程序 VS 到了，内容还是散文 Markdown」  
> 服从：`AGENTS.md` §5、`creatures/template.md`、`taxonomy.md`、`slice-workflow.md`

---

## 1. 问题

微切片工作流（§10B）约束的是**程序增量**。  
精灵、区域兴趣点、文案、遭遇表若无独立管线，会在 VS1+ 变成阻塞或劣质填表。

---

## 2. 双轨模型

```text
轨 A 程序微切片：VS0→VS6（slice-workflow）
轨 B 内容条目：提案→门禁→设计完整→数据化→挂接→验收
```

- 两轨可并行。  
- **内容未挂接到可运行数据 ≠ 程序切片失败**；但该内容不得宣称「已入库可玩」。  
- 程序切片的内容依赖必须在设计包 B9 写明「需要哪些 content ID 已到哪一阶段」。

---

## 3. 精灵 / 物种条目阶段

| 阶段 | 代号 | 定义 | 可被谁消费 |
|---|---|---|---|
| 概念 | `Concept` | 一句话 + 区域/澄相/定位 | 世界观讨论 |
| 设计完整 | `DesignComplete` | 通过 `template.md` 强制字段 + 生态位审查 | 文案/关卡规划 |
| 数据草稿 | `DataDraft` | `data/creatures/*.json` 字段齐全但未校验绿 | 程序联调 |
| 已校验 | `Validated` | `tools/validate_data` 通过 | 构建可加载 |
| 可玩挂接 | `Playable` | 遭遇/任务/场景至少一处引用且手测可见 | 切片验收 |
| 旗舰锁定 | `Flagship` | 额外：识别点、图志双文案、结契差异、制作备注 | 宣发/垂直切片门面 |

**禁止**用「完成」统称 Concept 与 Flagship。

---

## 4. 入库门禁（DesignComplete 前）

必须能回答（AGENTS §5.3）：

1. 食物链 / 能量链位置？  
2. 为何出现在该区域？  
3. 怕什么、依赖什么、与谁竞争？  
4. 玩家带它的非纯数值理由？  

任一答不上 = 不得进入 `DesignComplete`。

---

## 5. 数据化规则

1. **设计稿**：`docs/creatures/roster/*.md`  
2. **运行时权威**：`data/creatures/**`（及 skills/encounters 等）  
3. 字段以 `data/schema/` 与 VS 设计包锁定的 schema 为准。  
4. 玩家可见文案使用 **localization key**，不在逻辑里堆死字符串（中文可先写在 `data/l10n/zh-Hans.json`）。  
5. 新增物种理想路径：**不改 Domain 代码**，只加 data + 必要 view 占位。

### 最小校验（tools）

```text
tools/validate_data
  - JSON 可解析
  - 必填字段存在
  - id 唯一
  - 引用的 skill_id / region_id / l10n key 可解析（随 schema 加严）
```

VS1 设计门：必须出现**第一条真实生物 JSON + 校验脚本可运行**（可对草稿加严分级）。

---

## 6. 区域 / 兴趣点 / 文案

| 类型 | 设计位置 | 数据/挂接 |
|---|---|---|
| 区域卡片 | `docs/world/regions/` | 场景、遭遇表、天气配置 |
| 环境叙事物 | 区域叙事清单 | 交互物 ID + 对话 flag |
| 主线 Beat 文案 | `docs/narrative/` | dialogue 资产 / flag |
| 遭遇 | 名录 + 生态 | `data/encounters/` |

R01 切片：兴趣点密度见 `docs/world/regions/R01-environmental-narrative.md`。

---

## 7. 与切片的依赖契约

| 程序切片 | 内容最低就绪 |
|---|---|
| VS0 | 1 条示例 def（可为 dummy）+ schema v0 |
| VS1 | C001–C005 至少 DataDraft；补种常见种 Concept→DataDraft；遭遇表草稿 |
| VS2 | C001 结契步骤与失败码 DesignComplete |
| VS3 | 技能子集 + 相性表子集 Validated |
| VS4 | C003 进化条件 + 演出占位清单 |
| VS5 | Beat0–7 文案预算稿 + 棘影/鹿钩子脚本 |
| VS6 | 挂接齐全 + 文案音频识别点过试玩 |

---

## 8. 变更流程

```text
改设定 → 改 docs roster/区域 →（若已数据化）改 data → validate → 若影响规则则改系统文档/PRD → 再改代码
```

禁止只改游戏内硬编码却不回写 data/docs。

---

## 9. 验收清单（内容条目）

- [ ] 阶段标签正确（README 名录表）  
- [ ] DesignComplete 字段与生态位审查通过  
- [ ] 数据 id 稳定、可检索  
- [ ] 至少一种系统挂接路径写明  
- [ ] 图志/民俗文案非说明书堆砌（旗舰强制）  

---

## 10. 版本

| 版本 | 日期 | 说明 |
|---|---|---|
| v1.0 | 2026-07-20 | 双轨模型、阶段标签、门禁、与 VS 依赖契约 |
