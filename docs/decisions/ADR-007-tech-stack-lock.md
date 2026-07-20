# ADR-007 技术栈锁定（Unity 生态内明细）

- 状态：**Accepted**
- 日期：2026-07-20
- 决策者：用户确认引擎 Unity；制作组锁定明细栈
- 依据：`docs/production/tech-stack-worldclass-review.md`、`docs/production/tech-stack.md`

## 背景

引擎已确认为 Unity。若不定明细栈，实现期将在 UI、音频、数据、异步、资源管理上反复摇摆，直接伤害垂直切片速度与长期架构。

## 决策

采纳 `docs/production/tech-stack.md` v1.0 为**强制技术栈**：

- Unity 6 / LTS + **URP** + C#
- Input System、Cinemachine、Timeline
- **UI Toolkit**（唯一业务 UI 方案）
- UniTask 作为异步标准（引入后）
- 仓库 `data/**` JSON 为内容源权威；Domain 纯逻辑程序集
- VS 期 Unity Audio；产品化再评估 FMOD
- Addressables 不进 VS 前期
- 新第三方包走门禁 + ADR

并要求写入根目录 `AGENTS.md` 技术约束，使所有 Agent/协作者默认遵守。

## 后果

- 正：减少选型噪声；架构可测；内容可工业扩展  
- 负：个别偏好方案（如 UGUI、HDRP）被限制  
- 缓解：确有世界级收益时可 ADR 升级，不允许静默双轨  

## 替代方案（否决）

- UI UGUI 与 UI Toolkit 双轨  
- 逻辑写在 MonoBehaviour  
- 多引擎实验并行  
