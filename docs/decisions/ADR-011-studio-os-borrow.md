# ADR-011 借鉴 Game Studios 工作室操作系统（非整包替换）

- 状态：**Accepted**
- 日期：2026-07-20
- 决策者：用户要求理解并借鉴流程以完善 AGENTS
- 依据：`docs/production/studio-principles.md`
- 说明：原误编号为 ADR-010；与「开工前工作流补强」冲突后改编号为 **ADR-011**

## 背景
Donchitos/Claude-Code-Game-Studios 提供成熟的「协作非自治、分层委派、路径标准、评审强度、验证优先」思想。  
澄界已有 PRD、架构、§10B 切片门；缺的是**工作室如何在多角色下决策与互审**的显式协议。

## 决策
1. **不**将 Game Studios 整仓作为第二宪法或默认目录。  
2. **是**将其理念吸收进 `AGENTS.md`：  
   - 协作协议 Ask→Options→Decide→Draft→Approve  
   - 导演/Lead/专家分层与冲突升级  
   - 评审强度 full/lean/solo  
   - 路径级工程标准  
   - 设计透镜（MDA/SDT/Flow/Bartle）与验证优先  
   - 会话缺口检查与原型隔离  
3. 实现原子仍以 **VS0–VS6 + §10B** 为准；Epic/Story 仅可选细化。  
4. 与 **ADR-010**（开工前工作流补强：内容管线/试玩/Slim/仓库卫生）互补，不互相覆盖。

## 后果
- 正：决策路径清晰，减少 agent 越权与范围膨胀  
- 负：文档略增；需习惯声明评审强度  
- 缓解：默认 VS 期 `lean`，关键高光片 `full`

## 否决
- 49 agents 强制并行导致流程瘫痪  
- 绕过用户批准的“工作室自动落盘”  
- 用外部 CLAUDE.md 覆盖本仓库 AGENTS.md  
