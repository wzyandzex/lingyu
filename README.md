# 澄界 / Aetherion（lingyu）

> 世界级精灵宠物游戏  
> 一句话：在活的生态世界里，以共鸣与羁绊结伴精灵，而不只是收集与对战。

**GitHub：** <https://github.com/wzyandzex/lingyu>  
**本地产品名：** 澄界 · **仓库名：** `lingyu`

---

## 当前阶段

| 项 | 状态 |
|---|---|
| 产品 / PRD / 架构 / 工作流 | 已锁定 |
| **VS0 世界壳** | **实现已交付，待你本机 Play 验收** |
| VS1+ | 锁定等待（VS0 验收通过后） |

设计包：[`docs/production/slices/VS0-design.md`](docs/production/slices/VS0-design.md)  
状态板：[`docs/production/slices/STATUS.md`](docs/production/slices/STATUS.md)  
实现报告：[`docs/production/slices/VS0-implementation-report.md`](docs/production/slices/VS0-implementation-report.md)

---

## 快速开始（运行 VS0）

1. 用 Unity Hub 打开目录 **`game/`**（Unity 6.x）  
2. 等待包解析完成后按 **Play**  
3. 操作见 [`game/README.md`](game/README.md)

```bash
# 仓库根：数据校验
python tools/validate_data.py
```

---

## 仓库结构

```text
/
  AGENTS.md
  README.md
  docs/          设计与生产单源真相
  data/          权威 JSON 数据
  tools/         validate_data 等
  game/          Unity 工程
```

权威序：`AGENTS.md` → ADR → PRD → 分域 docs → 聊天。

---

## 文档入口

- [`docs/README.md`](docs/README.md)  
- [`docs/product/PRD-vertical-slice.md`](docs/product/PRD-vertical-slice.md)  
- [`docs/production/slice-workflow.md`](docs/production/slice-workflow.md)  
- [`docs/production/preflight-checklist.md`](docs/production/preflight-checklist.md)  

---

## 协作

1. 每 VS：设计门 → 你批准 → 实现 → 验收  
2. 内容阶段标签见 `content-pipeline.md`  
3. 文档漂移按缺陷处理  
