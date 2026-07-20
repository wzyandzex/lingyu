# 技术栈锁定（Tech Stack Lock）v1.0

> 状态：**已锁定（用户确认引擎 Unity，2026-07-20）**  
> 变更须走 ADR，并同步 `AGENTS.md` §7 与本文件。  
> 目标：服务世界超一流可发行产品，同时支撑 VS0–VS6 高速推进。

---

## 1. 总决策

| 层 | 锁定 |
|---|---|
| 引擎 | **Unity 6**（若环境受限可用当前 LTS，但新项目优先 6） |
| 渲染 | **URP**（禁止无 ADR 改 HDRP） |
| 语言 | **C#** |
| 架构 | **Domain / Application / Infrastructure / Presentation** 分层；总体运行时见 `architecture.md`（ADR-008） |
| 平台首发 | **PC**（Windows 优先；键鼠 + 手柄） |
| 联机 | 首发无实时联机；异步社交后置 |

---

## 2. 运行时栈（VS 期即生效）

| 域 | 锁定选择 | 禁止/延后 |
|---|---|---|
| 输入 | **Input System** | 旧 Input Manager 新代码 |
| 镜头 | **Cinemachine** | 手写复杂镜头堆无规范 |
| 过场/仪式演出 | **Timeline** + Cinemachine | 无结构的协程动画长龙 |
| UI | **UI Toolkit** | 同时维护 UGUI 业务双轨 |
| 异步 | **UniTask**（引入后统一） | 裸 `async void`；随意协程滥用业务逻辑 |
| 场景管理 | 自研轻量 `SceneFlow` / 官方 SceneManager 封装 | 过早引入重型场景框架 |
| 物理 | Unity Physics 按需 | 非必要不引入第三方物理 |
| 寻路 | 官方 NavMesh **按需** | VS0–VS2 不做复杂导航 |

---

## 3. 内容与数据栈

| 域 | 锁定 |
|---|---|
| 源数据权威 | 仓库根目录 `data/**`（JSON） |
| 运行时数据 | Unity `Assets/_Project/Data/**`（ScriptableObject 或导入生成物） |
| ID 契约 | 与 `docs/` 共用稳定 ID：`C001` `R01` `sk_*` |
| 校验 | `tools/validate_data.py` + schema；关键引用必须过检 |
| 本地化 | 字符串表（CSV/JSON）+ key；逻辑层禁止玩家可见硬编码中文 |
| 新增内容原则 | 加数据 + 资源 + 本地化，不改 Domain 核心 |

### 数据域目录（锁定）

```text
data/
  creatures/
  skills/
  encounters/
  items/
  quests/
  dialogue/
  regions/
  balance/
  localization/zh-CN/
  schema/
```

---

## 4. 架构栈（硬约束）

### 4.1 程序集

| Assembly | 允许引用 | 职责 |
|---|---|---|
| `Aetherion.Domain` | 无 `UnityEngine` | 战斗/结契/图鉴/羁绊/进化纯规则 |
| `Aetherion.Application` | Domain | 用例编排 |
| `Aetherion.Infrastructure` | Domain + Unity | 存档、读表、平台 |
| `Aetherion.Presentation` | 上层 | MonoBehaviour、UI、动画、VFX |
| `Aetherion.Editor` | Editor only | 导入器、校验窗 |
| `Aetherion.Tests` | Domain 等 | EditMode 单测 |

### 4.2 规则

1. **Domain 内禁止** `UnityEngine` / `MonoBehaviour` / 场景查询  
2. 战斗结算、结契判定、图鉴解锁、进化条件必须可 EditMode 单测  
3. Presentation 只发 Intent / 播表现，不写数值公式  
4. 跨域通信：显式服务 + 领域事件；禁止随意全局静态可变状态  
5. 存档只存 **state**，不存 **definition** 副本当权威  

### 4.3 回放/异步预留

战斗与关键演出输入以**指令序列**可记录为目标（即使 VS 期不实现回放 UI）。

---

## 5. 音频 / 美术工具链

| 域 | VS 期 | 产品化期（可 ADR 升级） |
|---|---|---|
| 音频运行时 | Unity Audio Mixer | 评估 **FMOD** |
| 建模绑定 | Blender 为主 | 外包可 Maya，但导出规范统一 |
| 贴图 | 团队自定；规范贴图尺寸与命名 | Substance 工作流可选 |
| 动画 | 统一 Rig 导出规范 + Animator/Animancer（二选一后锁定） | 同左 |
| 着色 | URP Shader Graph / 自研少量 | 禁止无序第三方shader包爆炸 |

**动画方案锁定策略**：VS0–VS1 可用原生 Animator；若伴随层复杂度上升，评估 **Animancer** 后写 ADR 锁死，禁止 Animator/Animancer 业务双轨长期并存。

---

## 6. 工程与品质栈

| 域 | 锁定 |
|---|---|
| 版本管理 | Git；`*.meta` 必须提交 |
| 大文件 | 需要时 Git LFS（fbx/wav/psd 等策略另表） |
| 依赖管理 | UPM；核心系统禁止资产店黑盒依赖 |
| 测试 | EditMode（Domain）+ 数据校验；PlayMode 关键路径 |
| CI（后置可先本地脚本） | validate_data + 单测 |
| 日志 | 统一 `ILog` 门面；禁止满天 `Debug.Log` 业务真相 |
| 崩溃/遥测 | VS 后置接口预留；不阻塞切片 |
| 代码格式 | `.editorconfig`；C# 可读性优先 |

---

## 7. 平台与发行栈（分期）

| 阶段 | 栈 |
|---|---|
| VS | 本地 PC 运行；无商店 SDK 也可 |
| 产品化 | Steamworks（或当时 PC 发行目标 SDK） |
| 主机 | 官方平台 SDK（另立 ADR） |
| 异步社交 | HTTPS API + 对象存储 + 回放元数据；禁止首发实时 MMO |

---

## 8. 包与第三方引入门禁

允许默认引入：
- Input System、Cinemachine、Timeline、URP、UniTask  
- Unity Test Framework  
- Unity Localization（若启用官方方案）

引入任何新第三方包前必须回答：
1. 解决哪个支柱/工程问题？  
2. 能否 50 行内自研替代？  
3. 许可证是否商业友好？  
4. 是否制造升级风险？  
5. 是否写了 ADR / 更新本文件？  

**禁止**：无评审资产店插件堆叠；禁止核心战斗/结契依赖闭源不可测插件。

---

## 9. 明确不采用（当前周期）

- Godot / UE5 双引擎并行  
- HDRP 首发  
- ECS/DOTS 全面化（除非局部有证明过的性能热点 ADR）  
- 实时联机帧同步  
- 首日 Addressables 全家桶（VS6 后再评估）  
- 逻辑层硬编码中文文案  
- Domain 与 MonoBehaviour 混写  

---

## 10. 版本记录

| 版本 | 日期 | 说明 |
|---|---|---|
| v1.0 | 2026-07-20 | 用户确认 Unity；锁定全栈草案为执行标准 |
