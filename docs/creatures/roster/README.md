# 精灵名录（Roster Index）

> 完成度标签服从 `docs/production/content-pipeline.md`  
> **禁止**用「完成」统称 Concept 与 Flagship

## 阶段标签

`Concept` → `DesignComplete` → `DataDraft` → `Validated` → `Playable` →（可选）`Flagship`

## 旗舰与切片相关

| ID | 名称 | 阶段 | 区域倾向 | 澄相 | 切片角色 | 文档 |
|---|---|---|---|---|---|---|
| C001 | 雾衔 | **Flagship** 设计完整，待数据化 | R01 | 潮汐/雾向 | 第一结契伙伴 | [C001-wuxian.md](./C001-wuxian.md) |
| C002 | 苔行 | **Partial**（未满模板） | R01 | 蔓息 | 常见目击/生态课 | [C002-taixing.md](./C002-taixing.md) |
| C003 | 露滴蛹 | **Partial→目标 Flagship** | R01 | 蔓息/潮汐 | 进化高光 | [C003-ludiyong.md](./C003-ludiyong.md) |
| C004 | 棘影 | **Partial→目标 Flagship** | R01 边缘 | 墟响 | 伦理事件 | [C004-jiying.md](./C004-jiying.md) |
| C005 | 回声鹿 | **Concept+**（强概念，制作字段不足） | R01 深层 | 澄心倾向 | 传说钩 | [C005-huishenglu.md](./C005-huishenglu.md) |
| C006–C010 | （见概念集） | **Concept** | 多区 | 各异 | 非切片保底 | [C006-C010-concepts.md](./C006-C010-concepts.md) |

### 阶段诚实说明（2026-07-20）

- 历史表述「旗舰 10 完成」**不准确**：仅 C001 达旗舰设计深度；C002–C004 部分字段；C005 概念强；C006–C010 为概念锚点。  
- 垂直切片 8–12 只：**未满员**，见下方补种。

## 切片补种占位（VS1 前须推进到 DataDraft）

| ID | 暂名 | 阶段 | 生态位一句话 | 澄相 | 功能 |
|---|---|---|---|---|---|
| C011 | 屑光蛾 | Concept | 夜林传粉/食露，教学「夜行目击」 | 蔓息 | 常见、非战斗主 |
| C012 | 溪敲虾 | Concept | 浅溪敲石求偶，潮汐相性教材 | 潮汐 | 教学战可选 |
| C013 | 桩盾鼹 | Concept | 护根土层，防御向对照 | 界骨/蔓息 | 墙盾演示 |
| C014 | 粉尘兔 | Concept | 林缘警戒鸣叫，遇敌易逃 | 澄风倾向 | 失败逃跑信息 |
| C015 | 炭籽鸦 | Concept | 啄食焦种，点状烬燎生态 | 烬燎 | 相性对照 |
| C016 | 雾苔寄生（待名） | Concept | 附雾衔活动区，表现共生 | 潮汐/蔓息 | 生态叙事 |
| C017 | 锈铃虫 | Concept | 啃食废弃测谐仪金属 | 墟响 | 历史钩 |

> 暂名可改；**ID 稳定后不要轻易改号**。补种达 DesignComplete 前不得宣称为旗舰。

## 覆盖缺口（切片内）

| 维度 | 现状 | 动作 |
|---|---|---|
| 常见种数量 | 不足 | C011–C015 优先 |
| 澄相教学 | 偏潮汐/蔓息 | C012/C015 |
| 非战斗生态位 | 弱 | C011/C016 |
| 传说 | C005 钩 | 保持稀缺，不填数值怪 |

## 维护规则

1. 新条目先改本表阶段，再写文件  
2. 入库门禁见 content-pipeline + template  
3. 数据化后 `data/creatures` 为运行时权威  
