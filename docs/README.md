# 澄界文档索引

> 单源真相入口。聊天记录不覆盖已 Accepted 的 ADR / PRD。

## 权威序

1. `/AGENTS.md` — 宪法  
2. `docs/decisions/ADR-*` — 已接受决策  
3. `docs/product/PRD*.md` — 需求  
4. 分域：`world/` `creatures/` `systems/` `narrative/` `production/`  
5. 对话中的临时意见（须回写文档才算数）

---

## 产品

| 文档 | 说明 |
|---|---|
| [product/product-constitution.md](./product/product-constitution.md) | 产品宪法 |
| [product/pillars.md](./product/pillars.md) | 体验支柱 |
| [product/non-goals.md](./product/non-goals.md) | 非目标 |
| [product/PRD.md](./product/PRD.md) | Master PRD |
| [product/PRD-vertical-slice.md](./product/PRD-vertical-slice.md) | 垂直切片 PRD |

## 世界

| 文档 | 说明 |
|---|---|
| [world/world-bible.md](./world/world-bible.md) | 世界观圣经 |
| [world/timeline.md](./world/timeline.md) | 时间轴 |
| [world/factions/overview.md](./world/factions/overview.md) | 势力 |
| [world/regions/R01-verdant-echo.md](./world/regions/R01-verdant-echo.md) | R01 区域卡 |
| [world/regions/R01-environmental-narrative.md](./world/regions/R01-environmental-narrative.md) | R01 POI/时间盒 |

## 精灵

| 文档 | 说明 |
|---|---|
| [creatures/taxonomy.md](./creatures/taxonomy.md) | 分类与澄相 |
| [creatures/template.md](./creatures/template.md) | 设计模板 |
| [creatures/roster/README.md](./creatures/roster/README.md) | 名录与完成度 |

## 系统

| 文档 | 说明 |
|---|---|
| [systems/core-loop.md](./systems/core-loop.md) | 核心循环 |
| [systems/bonding.md](./systems/bonding.md) | 结契与羁绊 v0.2 |
| [systems/battle.md](./systems/battle.md) | 战斗 v0.2 |
| [systems/exploration.md](./systems/exploration.md) | 探索 v0.1 |
| [systems/economy.md](./systems/economy.md) | 经济（切片薄规格） |

## 叙事

| 文档 | 说明 |
|---|---|
| [narrative/main-story.md](./narrative/main-story.md) | 主线大纲 |
| [narrative/onboarding-60m.md](./narrative/onboarding-60m.md) | 新手 60 分钟 |

## 生产 / 工作流

| 文档 | 说明 |
|---|---|
| [production/slice-workflow.md](./production/slice-workflow.md) | 微切片强制协议 |
| [production/design-package-profiles.md](./production/design-package-profiles.md) | Slim/Full 设计包 |
| [production/content-pipeline.md](./production/content-pipeline.md) | 内容双轨管线 |
| [production/playtest-protocol.md](./production/playtest-protocol.md) | 试玩与否决 |
| [production/git-and-repo-hygiene.md](./production/git-and-repo-hygiene.md) | Git 与仓库卫生 |
| [production/art-audio-placeholder.md](./production/art-audio-placeholder.md) | 美术音频 PH 规范 |
| [production/preflight-checklist.md](./production/preflight-checklist.md) | 开工前检查 |
| [production/studio-principles.md](./production/studio-principles.md) | Game Studios 理念映射（借 OS 不换宪法） |
| [production/slices/STATUS.md](./production/slices/STATUS.md) | 切片状态板 |
| [production/vertical-slice.md](./production/vertical-slice.md) | 体验切片定义 |
| [production/roadmap.md](./production/roadmap.md) | 路线图 |
| [production/architecture.md](./production/architecture.md) | 软件架构 |
| [production/tech-stack.md](./production/tech-stack.md) | 技术栈 |
| [production/engineering-skeleton.md](./production/engineering-skeleton.md) | 工程骨架说明 |
| [production/risks.md](./production/risks.md) | 风险 |

## 决策 ADR

| ADR | 主题 |
|---|---|
| [ADR-001](./decisions/ADR-001-product-shape.md) | 产品形态 |
| [ADR-002](./decisions/ADR-002-platform-pc-first.md) | PC 优先 |
| [ADR-003](./decisions/ADR-003-battle-turn-based.md) | 回合制 |
| [ADR-004](./decisions/ADR-004-bonding-resonance.md) | 共鸣结契 |
| [ADR-005](./decisions/ADR-005-world-aetherion.md) | 世界设定核 |
| [ADR-006](./decisions/ADR-006-engine-unity.md) | **引擎 Unity** |
| [ADR-007](./decisions/ADR-007-tech-stack-lock.md) | 技术栈锁 |
| [ADR-008](./decisions/ADR-008-overall-architecture.md) | 总架构 |
| [ADR-009](./decisions/ADR-009-slice-workflow.md) | 切片工作流 |
| [ADR-010](./decisions/ADR-010-pre-implementation-workflow.md) | 开工前工作流补强 |
| [ADR-011](./decisions/ADR-011-studio-os-borrow.md) | 借鉴 Game Studios OS（非整包） |

---

## 数据与工程目录

| 路径 | 说明 |
|---|---|
| `/data` | 权威 JSON 数据（schema + 占位样例） |
| `/tools` | 校验脚本（待 VS0/VS1 充实） |
| `/game` | Unity 工程占位（批准实现后创建） |
