# 澄界 / Aetherion（lingyu）

> 世界级精灵宠物游戏 — **预制作 / 规范基线**阶段  
> 一句话：在活的生态世界里，以共鸣与羁绊结伴精灵，而不只是收集与对战。

**GitHub：** <https://github.com/wzyandzex/lingyu>  
**本地产品名：** 澄界 · **仓库名：** `lingyu`（历史/路径名可能仍见 `cardplayzcode`，不改变产品名）

---

## 当前阶段

| 项 | 状态 |
|---|---|
| 产品宪法 / 支柱 / 非目标 | 已锁定 |
| Master PRD + 垂直切片 PRD | 已锁定（会随切片增量修订） |
| 引擎 / 技术栈 / 总架构 | **已锁定：Unity + URP + C#**（ADR-006/007/008） |
| 切片工作流 + 内容管线 + 试玩协议 | 已锁定（ADR-009/010） |
| 仓库卫生（Git/ignore/目录） | 已建立 |
| 业务实现代码 | **未开始**（须 VS0 设计门 + 批准） |
| 当前焦点切片 | **VS0 世界壳**（待宣告/设计） |

**下一步（唯一正确）：** 撰写并审阅 `docs/production/slices/VS0-design.md`（建议 Slim 剖面）→ 你批准 → 再创建完整 Unity 工程与实现。

---

## 仓库结构

```text
/
  AGENTS.md                 # Agent / 制作宪法（最高优先级）
  README.md                 # 本文件
  docs/                     # 设计与生产单源真相
  data/                     # 运行时权威数据（与引擎分离）
  tools/                    # 校验与管线脚本（实现期充实）
  game/                     # Unity 工程根（现为占位 README）
```

权威阅读序：`AGENTS.md` → ADR → PRD → 分域 docs → 聊天。

---

## 文档入口

- 总索引：[`docs/README.md`](docs/README.md)  
- 垂直切片 PRD：[`docs/product/PRD-vertical-slice.md`](docs/product/PRD-vertical-slice.md)  
- 切片状态：[`docs/production/slices/STATUS.md`](docs/production/slices/STATUS.md)  
- 开工前检查：[`docs/production/preflight-checklist.md`](docs/production/preflight-checklist.md)  
- 仓库卫生：[`docs/production/git-and-repo-hygiene.md`](docs/production/git-and-repo-hygiene.md)  

---

## 协作要点

1. **先设计后实现**：每个 VS 过设计门与用户批准（`slice-workflow.md`）。  
2. **内容双轨**：物种/POI 走 `content-pipeline.md` 阶段标签，禁止虚高「完成」。  
3. **文档漂移 = 缺陷**：改引擎/阶段必须同步 README、roadmap、STATUS。  
4. **不写业务代码**直到你对当前 VS 说「批准实现」。  

---

## 许可与隐私

勿提交密钥、账号、本地绝对密钥路径。见 `.gitignore` 与仓库卫生文档。
