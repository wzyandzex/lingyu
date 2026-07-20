# 开工前就绪审查（Preflight Checklist）v1.1

> 目的：在写第一行业务代码前，暴露会在实现期爆雷的缺口。  
> 结论日期：2026-07-20（v1.1 同步 ADR-010 规范补强）  
> 配套：`slice-workflow.md` / `PRD-vertical-slice.md` / `architecture.md` / `content-pipeline.md` / `playtest-protocol.md`

---

## 0. 总判读

| 层 | 状态 | 说明 |
|---|---|---|
| 产品灵魂 / 支柱 / 非目标 | ✅ 足够 | 可指导取舍 |
| Master PRD + VS PRD | ✅ 足够 | 全游戏 1.0 仍会加厚 |
| 总架构 / 技术栈 / 切片工作流 | ✅ 已锁 | Unity；§10B + Slim/内容/试玩 |
| 世界 / 旗舰设定 | ✅ 方向够 | R01 POI 密度已有 v0.1；旗舰完成度已诚实化 |
| 系统可测基线 | ✅ 部分 | bonding/battle v0.2；exploration/economy v0.1 |
| 仓库卫生 / data 占位 | ✅ 已建 | Git 远程、ignore、data/tools/game 占位；**无业务代码** |
| **VS0 设计包** | ✅ 已写待批 | `slices/VS0-design.md` **待你批准实现** |
| **Unity 工程业务落地** | ❌ 未开始 | **硬阻塞 = 你的实现许可** |
| 数值终表 / 全技能表 | ⏳ 有意后置 | 不阻塞 VS0 |
| 美术风格锚终稿 | ⏳ PH 规范已有 | 终稿 Art Bible 后置 |
| 法律/商标/音频版权流程 | ⏳ 未建 | 发行前必补 |

**一句话**：战略、纪律、VS0 战术设计与仓库规范已够；**只差你对 VS0 的实现批准**（或改设计）。  

---

## 1. 你已想到且正确的点（保持）

1. 总架构先于代码  
2. 详细 PRD 先于代码  
3. 逐微切片、先设计后实现  
4. 写入 `AGENTS.md` 强制约束  
5. 你可随时提意见  
6. **实现 0 时把规范补全，而不是用“没代码”否定准备工作**

---

## 2. 缺口表（更新后）

### P0 — 实现许可前

| # | 缺口 | 状态 | 说明 |
|---|---|---|---|
| 1 | VS0-design.md | ✅ 待批 | 已交审 |
| 2 | CreatureDef schema v0 | ✅ 草案 | `data/schema` + VS0 B5；实现时以设计包钉死并出 C001 |
| 3 | 本地化 key 规范 | ✅ 草案 | VS0 B5 |
| 4 | Input 动作表 | ✅ 草案 | VS0 B5 |
| 5 | Boot/R01/Mode 职责 | ✅ 草案 | VS0 B3 |
| 6 | 存档 v0 字段 | ✅ 草案 | VS0 B5 |
| 7 | Placeholder 规范 | ✅ | `art-audio-placeholder.md` |
| 8 | Unity 精确版本号 | ⏳ | **实现首日钉死**（设计包 Q3） |
| 9 | Git 策略 | ✅ | `git-and-repo-hygiene.md` + 远程 |
| 10 | 验收签字习惯 | ✅ 流程 | 用户批准 / 验收门 |

### P1 — VS1–VS4 前

| # | 缺口 | 状态 | 阻塞 |
|---|---|---|---|
| 11 | 结契状态机/失败码 | ✅ v0.2 文档 | VS2 设计包再表化 |
| 12 | 战斗公式/相性子集/Event | ✅ v0.2 文档 | VS3 |
| 13 | 进化条件 DSL + 演出端口 | ⏳ | VS4 |
| 14 | R01 兴趣点密度 | ✅ v0.1 | VS5 体验 |
| 15 | 补种名单 C011–C017 | ✅ 占位 Concept | VS1 DataDraft |
| 16 | 对话 flag 命名表 | ⏳ | VS5 |
| 17 | 随机种子策略 | ⏳ | VS3 |
| 18 | UI 面板导航图 | ⏳ | 各片增量 |

### P2 — 后置

性能预算、无障碍、商标检索、FMOD 商务、CI 强化、GM 菜单等——见 v1.0 列表，仍有效。

---

## 3. 规范补强清单（ADR-010，已落地）

- [x] content-pipeline（阶段标签 / 双轨）  
- [x] playtest-protocol  
- [x] design-package-profiles（Slim）  
- [x] git-and-repo-hygiene  
- [x] art-audio-placeholder  
- [x] exploration.md / economy.md  
- [x] R01-environmental-narrative  
- [x] roster 完成度诚实化  
- [x] bonding/battle 升 v0.2  

---

## 4. 现在唯一硬阻塞实现的事项

1. **你对 `VS0-design.md` 的明确批准**（或修改意见）  
2. （批准后）本机 Unity 版本钉死 + 创建 `game/` 工程  

没有第 1 项，不写业务实现代码。

---

## 5. 版本

| 版本 | 日期 | 说明 |
|---|---|---|
| v1.0 | 2026-07-20 | 首版缺口表 |
| v1.1 | 2026-07-20 | 同步规范落地与 VS0 待批状态 |
