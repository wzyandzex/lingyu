# ADR-008 总体运行时架构锁定

- 状态：**Accepted**
- 日期：2026-07-20
- 决策者：世界级架构规划；开码前强制
- 依据：`docs/production/architecture.md`

## 背景

技术栈（Unity/URP/C# 等）已锁定，但仅有分层名词与目录骨架，不足以约束：

- 探索/结契/战斗/对话如何切换  
- 谁拥有进度状态  
- 定义数据与存档状态如何分离  
- 表现与规则如何解耦  

在无总体架构下开 VS0，极易形成上帝场景脚本，导致后续战斗/结契/图鉴无法测试与扩展。

## 决策

采纳 `docs/production/architecture.md` v1.0 为**总体架构法**，并写入 `AGENTS.md` §7A 强制约束。核心锁定：

1. Domain / Application / Infrastructure / Presentation 分层  
2. AppSession + WorldSession 会话模型  
3. 互斥 **GameMode**（Exploration/Bonding/Battle/Dialogue/Menu/Cutscene/Title）  
4. 有界上下文：Party、Bonding、Battle、Codex、Affinity、Evolution、Exploration、World、Narrative、Inventory  
5. Definition / Instance / Progress / Ephemeral 四态分离  
6. Intent → Application → Domain → Events → Presentation 控制流  
7. 战斗产出 `BattleEvent[]` 供表现队列播放，表现不回写不同结算  
8. Domain 禁止 Unity API；内容靠 `data/**` 扩展  

## 后果

- 正：可测、可扩、可演、可存、可协作  
- 负：VS0 需多搭薄骨架，不能一个 Player.cs 打天下  
- 缓解：VS0 只实现空 Mode 路由 + 探索，其他 Mode 先占位  

## 否决

- 单一 GameManager 拥有全部状态与规则  
- 场景 `LoadScene` 代替 Mode 语义且状态散落  
- UI/动画回调内写核心规则  
