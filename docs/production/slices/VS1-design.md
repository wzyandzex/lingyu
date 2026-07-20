# VS1 设计包 — 图志目击（See & Record）

> **状态**：**Accepted Baseline**（用户验收通过 2026-07-21）  
> **剖面**：**Full（轻）**  
> **切片**：VS1  
> **日期**：2026-07-21  
> **依赖**：VS0 已通过基线  
> **实现许可**：**已授予**（按本修订包执行）

---

## A. 宣告

**目标**：灰盒中看见野生占位体，靠近后写入图志外观层，打开图志可区分已发现/未发现。

### PRD 分期表（审查锁定）

| ID | VS1 | 说明 |
|---|---|---|
| CDX-01 | 做 | 列表 + 详情 |
| CDX-02 | 半 | 层枚举预留；只玩外观 |
| CDX-03 | 做 | 目击解锁外观层 |
| CDX-04 | **不做** | 观察/战斗解锁 → VS2/VS3 |
| CDX-05 | 做 | 未解锁遮蔽，不泄真名 |
| CDX-06 | 做 | 双文案位（有数据则显） |
| WLD-04 | 雏形 | enc 表 = 权威可刷 def + 固定点 |
| WLD-01/02/03/05 | 不做 | |
| Beat2 NPC 授簿 | 不做 | 系统原子 only；叙事 VS5 |

**默认野生：仅 C002。** 目击半径默认 **5m**。图志键 **C**，打开锁移动。UI：**IMGUI**。

---

## B1 范围

### In
1. 固定点生成 C002（enc_r01_demo 权威列表）；出生点 8–12m 内至少 1 只；剪影扁贴地、绿褐、矮于玩家  
2. 距离目击 → `WorldSession.Codex.RegisterSighting`；必须可见占位，禁止空气解锁  
3. 图志 UI：列表 = R01 catalog ∪ 已解锁；未知显示 unknown；已解锁显示名+外观文案；↑↓/鼠标  
4. Save v1 序列化 codex；兼容 v0  
5. 数据 C002 + enc + l10n；validate **硬校** encounter 引用  
6. Domain 单测幂等  
7. Prompt：「图志记下一笔：{name}」

### Out
结契、战斗、CDX-04、天气/隐藏栖息、Addressables、授簿长对话、多区。

---

## B2 流程
Play → 见贴地 C002 → 走近 5m → prompt → C 开图志见真名 → 未发现条为 ??? → F5/F9 保持。

---

## B3 架构
- Domain: `CodexLayer`, `CodexProgress`  
- **WorldSession 持有 CodexProgress**（唯一进度真相）  
- App: RegisterCreatureSighting / 查询  
- Infra: catalog encounters；Save v1  
- Pres: WildCreatureView, SightingSensor, CodexScreen, 固定点 spawn  
- Mode: 仍 Exploration；图志为叠加 UI  

---

## B4 选型
沿用 Unity 2022.3 分层；IMGUI 图志；JsonUtility；无新包。

---

## B5 数据
- `data/creatures/C002_taixing.json`  
- `data/encounters/enc_r01_demo.json`（entries 至少 C002）  
- Save version=1, codex: [{defId, layers:["Appearance"]}]  
- 路径：仓库 data/ + StreamingAssets 副本  

---

## B6 规则
```
RegisterSighting: first Appearance unlock -> true; else false
sightRadius = 5
spawn from encounter.entries defs at fixed points near spawn
```

---

## B7 接口
`RegisterCreatureSighting`, `IsUnlocked`, `GetCodexListRows`（含未知行）, Save/Load。

---

## B8 UX
- HUD 提示「C 图志」  
- 否决：未走近全解锁、未知泄真名、丢存档  
- 不否决：IMGUI 丑、PH 简陋  

---

## B9 内容
C002 DataDraft 真文案；PH 扁体贴地。

---

## B10 风险
命名遮蔽、看不见怪、列表策略漂移 — 已用钉死策略缓解。

---

## B11 DoD
- [ ] 出生点附近可见贴地 C002  
- [ ] 5m 内 prompt 记录句  
- [ ] 图志真名/未知策略正确  
- [ ] 详情文案  
- [ ] 键盘或鼠标导航  
- [ ] F5/F9 codex  
- [ ] validate 含 encounter 全绿  
- [ ] 单测幂等  
- [ ] 无结契战斗  
- [ ] WorldSession 为真相  

---

## B12 任务序
1 Domain+测 2 WorldSession/Save v1 3 数据+validate 4 Catalog enc 5 Spawn+View 6 Sensor 7 Codex UI 8 手测报告  

## B13
Q 均按默认；已批准。
