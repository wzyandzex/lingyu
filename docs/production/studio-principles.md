# Game Studios 理念映射（澄界采纳说明）v1.0

> 来源：Donchitos/Claude-Code-Game-Studios（MIT）设计思想  
> 用途：说明我们**借什么、不借什么**；执行约束以 `AGENTS.md` 为准  
> 决策：ADR-011  
> 日期：2026-07-20

---

## 1. 他们的核心思想（充分理解）

### 1.1 要解决的病
单人 + 通用 AI 会话常见失败：

- 没有结构：跳过设计、硬编码、通心粉代码  
- 没有人喊停：不做 QA、不做愿景对齐、不问“这还是不是那个游戏”  
- 没有边界：谁都能改一切，冲突无升级路径  

### 1.2 解药：工作室结构，而不是更强的自动驾驶
- **49 角色分层**：导演守愿景/技术/生产；Lead 守域；专家动手  
- **73 流程入口**：从构思 → 系统 → 架构 → Epic/Story → 开发 → QA → 发布  
- **Hooks / Rules**：机器侧防呆（提交、资产、缺口检测、路径标准）  
- **模板**：把“应该写什么”变成默认动作  

### 1.3 五条协作铁律（官方 How It Works）
1. **纵向委派** Director → Lead → Specialist  
2. **横向咨询** 同级可商量，**不能**做跨域终裁  
3. **冲突上抛** 设计→Creative Director；技术→Technical Director  
4. **跨部门变更走 Producer**  
5. **领域边界** 未经委派不改域外文件  

### 1.4 协作协议（Collaborative, Not Autonomous）
```text
Ask → Present 2–4 options → You decide → Draft → Approve → Write
```
Agent 提供结构与专业，**不提供自治拍板**。

### 1.5 设计哲学工具箱
| 工具 | 含义 | 澄界用法 |
|---|---|---|
| **MDA** | Mechanics→Dynamics→Aesthetics | 机制必须推到可感美学（共鸣、生态、失谐） |
| **SDT** | 自主/胜任/关系 | 结契自主、战斗胜任、羁绊关系 |
| **Flow** | 挑战-技能平衡 | 教学战与探索密度，防劝退 |
| **Bartle** | 玩家类型 | 探索/成就/社交/杀手型诉求的覆盖检查 |
| **VDD** | 先验证再实现 | Domain 测试、设计门、试玩协议 |

### 1.6 评审强度旋钮
`full` / `lean` / `solo`：同一流程可调“门禁有多重”，而不是只有“全做或全不做”。

### 1.7 我们明确不整包搬迁的
- 替换 `AGENTS.md` 为 `CLAUDE.md` 双宪法  
- 强制 49 实体 agent 文件  
- 强制他们的 `src/design/production` 目录取代我们的 `docs/game/data`  
- 默认 DOTS/Addressables 等与 VS 冲突的引擎菜单  
- 无用户批准的自动写盘文化（若某 fork 偏离官方 “approve first”）  

---

## 2. 映射到澄界已有体系

| Game Studios | 澄界已有 | 本次补强 |
|---|---|---|
| 导演/Lead/专家 | §1 多角色表 | 分层、委派、冲突升级、Producer 角色 |
| Ask→Options→Decide→Approve | §10B 设计门/用户门 | 写成全局协作协议 §1.3 |
| review full/lean/solo | 无显式旋钮 | §10B.7 评审强度 |
| path rules | §7 技术约束较散 | §7B 路径级标准 |
| gap detect | preflight 文档 | §11 会话启动缺口检查 |
| prototypes/ 隔离 | 防膨胀条款 | §7B 原型隔离 |
| story pipeline | VS0–VS6 | 保持 VS 为主原子；story 仅作可选细分 |
| MDA/SDT/Flow | 支柱与体验承诺 | §3 设计透镜 |
| VDD | TDD/验收 | 验证优先写死 |

---

## 3. 采纳原则
**借操作系统，不借另一套宪法。**  
冲突时永远：`AGENTS.md` > 外部模板。
