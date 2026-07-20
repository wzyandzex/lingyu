# 切片状态板（Slice Status）

> 权威工作流：`slice-workflow.md`  
> 设计包剖面：`design-package-profiles.md`  
> 更新人：每完成一阶段的 Agent / 协作者  

| 切片 | 阶段 | 设计包 | 用户批准实现 | 实现 | 验收 | 备注 |
|---|---|---|---|---|---|---|
| VS0 世界壳 | **待用户确认** | [VS0-design.md](./VS0-design.md)（**Slim**） | 未批准 | 未开始 | 未通过 | 设计门已交审；规范基线 ADR-010 已补 |
| VS1 图志目击 | 锁定等待 | — | — | — | — | 依赖 VS0；内容 DataDraft |
| VS2 结契 | 锁定等待 | — | — | — | — | Full；bonding v0.2 + 试玩 T1 建议 |
| VS3 战斗 | 锁定等待 | — | — | — | — | Full；battle v0.2 |
| VS4 羁绊进化 | 锁定等待 | — | — | — | — | 试玩 T1 建议 |
| VS5 叙事事件 | 锁定等待 | — | — | — | — | Delta；R01 POI 清单 |
| VS6 打磨验收 | 锁定等待 | — | — | — | — | T2 + E1–E7 |

**阶段枚举**：待宣告 → 设计中 → 待用户确认 → 实现中 → 待验收 → **已通过基线** / 打回

**当前焦点**：VS0  
**当前阶段**：设计门完成 → **等待你批准实现**  
**全局实现许可**：无  
**开工前审查**：`docs/production/preflight-checklist.md`  
**仓库卫生**：远程 `lingyu`、ignore、data/tools/game 占位已建（仍无业务代码）  
**负债**：CreatureDef schema 以 VS0 设计包为准收敛 `data/schema`；补种 C011–C017 仍为 Concept
