# 风险登记（Risks）

> 更新：2026-07-20

| ID | 风险 | 等级 | 缓解 | 状态 |
|---|---|---|---|---|
| P01 | 独特性只停留在文档，无试玩验证 | 高 | playtest-protocol；VS2/4/6 试玩门槛 | 开放 |
| P02 | 范围膨胀（未做切片先扩 10 区/300 灵） | 高 | roadmap 明确不做；内容管线阶段标签 | 缓解中 |
| P03 | 文档漂移（引擎/完成度/状态板互斥） | 高 | git 同步义务；ADR-010；本次已修一波 | 缓解中 |
| P04 | 无数据权威，Markdown 无法进游戏 | 高 | data/ + content-pipeline + schema v0 | 缓解中 |
| P05 | 文案墙劝退 | 中 | onboarding 字数预算（待写）；POI 环境叙事 | 开放 |
| P06 | 结契做成隐藏百分比或 QTE | 高 | bonding v0.2 状态机+失败码；VS2 单测 | 开放 |
| P07 | 战斗无表达或信息过载 | 中 | battle v0.2；教学战单因果 | 开放 |
| P08 | Placeholder 永久化 | 中 | art-audio-placeholder；PH 清退清单 | 开放 |
| P09 | Unity 版本/meta 腐烂 | 中 | git-and-repo-hygiene；VS0 锁精确版本 | 开放 |
| P10 | 旗舰完成度自欺 | 中 | roster 诚实标签；审查禁止虚高 | 缓解中 |
| P11 | 密钥或大资产误提交 | 中 | gitignore；LFS 策略预写 | 开放 |
| P12 | 一人+Agent 设计包过劳 | 中 | Slim 剖面 + 时间盒 | 缓解中 |

## 已关闭/降级

| ID | 说明 |
|---|---|
| P-ENG-0 | 「引擎未选定」— 已关闭，Unity 锁定 |
| P-REPO-0 | 「完全无 Git」— 已关闭（本轮建立远程基线） |
