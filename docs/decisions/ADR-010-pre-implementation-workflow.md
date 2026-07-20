# ADR-010 开工前工作流补强（内容管线 / 试玩 / Slim 设计包 / 仓库卫生）

- 状态：**Accepted**
- 日期：2026-07-20
- 决策者：多角色审查后的制作规范补强（用户授权：修漂移 + 规范，不写业务实现）

## 背景

项目在实现量为 0 时已具备较强 PRD/架构/切片协议，但审查发现：

1. 仅有程序微切片流，缺少内容生产阶段标签与数据权威  
2. 试玩与品质否决未操作化  
3. B1–B13 全量设计包对 VS0 过重，缺少官方 Slim  
4. 无 Git/目录卫生，文档存在引擎状态漂移  
5. 探索/经济文档缺失，名录完成度表述虚高  

若在无上述规范下直接开码，将重复「散文设定无法挂接 / 假完成 / 文档互斥」。

## 决策

新增并强制引用：

| 文档 | 作用 |
|---|---|
| `docs/production/git-and-repo-hygiene.md` | 远程、分支、同步义务、Unity meta |
| `docs/production/content-pipeline.md` | 内容双轨与阶段标签 |
| `docs/production/playtest-protocol.md` | 试玩类型与否决 |
| `docs/production/design-package-profiles.md` | Slim/Full/Delta |
| `docs/production/art-audio-placeholder.md` | PH 资产规范 |
| `docs/systems/exploration.md` | 探索域规格 |
| `docs/systems/economy.md` | 切片经济边界 |
| `docs/world/regions/R01-environmental-narrative.md` | R01 POI/时间盒 |

仓库根增加：`.gitignore` / `.gitattributes` / `.editorconfig`，以及 `data/` `tools/` `game/` 占位（无业务代码）。

ADR-006 文件名更正为 `ADR-006-engine-unity.md`。

## 后果

- 正：实现前规范可执行；完成度诚实；切片设计可裁剪但不假精简  
- 负：文档量再增——以「可勾选协议」为限，禁止继续无目标扩世界观长文  
- 不改变：仍须 VS0 设计门 + 用户批准后才写业务实现  

## 否决

- 用更多设定文代替管线与试玩协议  
- 无阶段标签宣称旗舰 10 完成  
- 无试玩记录宣称体验已验证  
