# VS1 实现后报告 — 图志目击

> 日期：2026-07-21  
> 设计包：`VS1-design.md`（审查修订后已批准）  
> 阶段：**已通过基线**（用户验收 2026-07-21）

## 做了什么
- Domain：`CodexLayer` / `CodexProgress` + `CodexProgressTests`
- WorldSession **持有** Codex；Save **v1** 序列化 codex（兼容 v0 无字段）
- 数据：`C002_taixing.json`、`enc_r01_demo.json`、l10n；validate 含 encounter 引用硬校
- Catalog 加载 encounters；R01 列表源
- 表现：贴地扁球体 C002×3、SightingSensor 5m、IMGUI CodexScreen（C 开闭、锁移动、↑↓）
- Prompt：`图志记下一笔：{name}`
- StreamingAssets 已同步

## 未做（符合 Out）
结契、战斗、CDX-04 观察解锁、NPC 授簿、UI Toolkit

## 验收对照（请你手测）
| 项 | 期望 |
|---|---|
| 可见 C002 | 出生点附近扁绿贴地球 |
| 走近 ~5m | prompt 记录句 |
| C 图志 | 苔行真名；C001 等未目击为 ？？？ |
| F5/F9 | 目击保持 |
| validate | `python tools/validate_data.py` OK |

## 验收结论
| 项 | 结果 |
|---|---|
| 用户验收 | **通过**（2026-07-21） |
| 下一动作 | 宣告 VS2 结契设计门（Full）；未批准前不实现 |
