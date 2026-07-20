# 制作路线图（Roadmap）

> 更新：2026-07-21  
> 引擎状态：**已锁定 Unity**（本机 VS0 验收：**2022.3 LTS**）  
> 实现状态：**VS0 已通过基线**；下一焦点 VS1 设计门

---

## 阶段总览

| 阶段 | 名称 | 状态 | 出口标准 |
|---|---|---|---|
| A | 产品宪法 + 支柱 + 非目标 | **Done** | 文档可指导取舍 |
| B | 世界观骨架 + R01 名片 | **Done（骨架）** | 主题可玩映射；R02–10 仅索引 |
| C | 精灵框架 + 样例 | **Partial** | 框架 Done；旗舰深度仅 C001 完整，C006–10 为 Concept |
| D | 核心循环 + 系统规格 | **Partial** | core/bonding/battle/exploration/economy 有 v0.x；公式待 VS 设计包升格 |
| E | 垂直切片规格 + 微切片协议 | **Done（规格）** | VS PRD + §10B + Slim/内容/试玩协议 |
| F | 仓库卫生 + 数据目录占位 | **Done** | Git 远程、ignore、data/tools/game 占位 |
| G | VS0→VS6 实现 | **进行中（VS0 Done）** | 每片：设计门→批准→实现→验收 |
| H | 内容上量与多区 | 后置 | 切片品质通过后 |

---

## 阶段 A — 产品（Done）

- [x] 一句话卖点与灵魂  
- [x] 5 支柱 + non-goals  
- [x] 平台默认：PC 优先单机叙事根基（ADR-001/002）  
- [x] Master PRD  

---

## 阶段 B — 世界（骨架 Done）

- [x] 澄界法则与当前冲突  
- [x] 10 区名片级  
- [x] 势力 overview  
- [x] R01 区域卡 + **环境叙事物/POI 密度**  
- [ ] R02–R10 深写（**有意不做**，除非切片需要）  

---

## 阶段 C — 精灵（Partial）

- [x] 澄相 / 分类 / 模板 / 入库门禁  
- [x] C001 旗舰级设计  
- [x] C002–C005 有条目（完成度见 roster README）  
- [x] C006–C010 概念锚点  
- [x] 切片补种 C011–C017 **占位**  
- [ ] C002–C004 DesignComplete 补全  
- [ ] C001 + 补种 DataDraft 进 `data/creatures`  

---

## 阶段 D — 系统规格（Partial）

- [x] core-loop  
- [x] bonding v0.2（状态机+伪代码）  
- [x] battle v0.2（公式+相性子集+Event）  
- [x] exploration v0.1  
- [x] economy v0.1（切片白名单）  
- [ ] VS2/VS3 设计包级可单测表（进切片时升格）  
- [ ] onboarding 对话字数预算稿  

---

## 阶段 E — 切片与工作流（规格 Done）

- [x] vertical-slice 体验定义  
- [x] PRD-vertical-slice  
- [x] slice-workflow + STATUS  
- [x] design-package-profiles（Slim/Full）  
- [x] content-pipeline  
- [x] playtest-protocol  
- [x] art-audio-placeholder  
- [x] preflight-checklist  

---

## 阶段 F — 仓库与数据壳（Done）

- [x] Git 远程 `wzyandzex/lingyu`  
- [x] `.gitignore` / `.gitattributes` / `.editorconfig`  
- [x] `data/schema` + dummy creature + l10n 样例  
- [x] `tools/` `game/` README 占位  
- [ ] `tools/validate_data` 可执行脚本（VS0/VS1）  
- [ ] 完整 Unity 工程（**VS0 批准后**）  

---

## 阶段 G — 实现微切片（Not started）

| 切片 | 状态 | 备注 |
|---|---|---|
| VS0 世界壳 | **已通过基线** | 用户验收 2026-07-21；灰盒可 Play |
| VS1 图志目击 | 可宣告 | 设计门未开；依赖内容 DataDraft |
| VS2 结契 | 锁定等待 | Full；bonding 单测 |
| VS3 战斗 | 锁定等待 | Full；battle 单测 |
| VS4 进化 | 锁定等待 | 演出 PH + 试玩 T1 |
| VS5 叙事 | 锁定等待 | Beat 文案 |
| VS6 打磨 | 锁定等待 | T2 + E1–E7 |

实时状态以 `docs/production/slices/STATUS.md` 为准。

---

## 明确不做（现阶段）

- 无 VS0 批准的业务玩法代码  
- R02–R10 长文扩写  
- 引擎复评 / 栈复评  
- 用「再写一份总论」代替 VS0 设计包  

---

## 版本

| 日期 | 说明 |
|---|---|
| 2026-07-20 | 去除「待确认引擎」；对齐 ADR-010 与诚实完成度 |
