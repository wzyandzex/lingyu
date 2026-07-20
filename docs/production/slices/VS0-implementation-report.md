# VS0 实现后报告（世界壳）

> 日期：2026-07-20  
> 设计包：`VS0-design.md`（用户已批准实现）  
> 阶段：**待验收**（实现环境无 Unity Editor，Play 须你本机确认）

---

## 变更摘要

### 做了什么
1. Unity 工程骨架：`game/Packages`、`ProjectSettings`、程序集划分  
2. Domain：`CreatureDefId` / `CreatureDef` / `RegionId`（无 Unity 引用）  
3. Application：`AppSession`、`WorldSession`+Save 快照、`GameModeRouter`、`ExplorationMode`、Stub modes  
4. Infrastructure：`JsonDataCatalog`、`FileSaveService`（原子写）、`JsonLocalization`、`DataPathResolver`（Editor 读仓库 `data/`，Player 读 StreamingAssets）  
5. Presentation：`GameBootstrap` 组合根、`AutoBootstrap`、`RuntimeWorldBuilder` 灰盒 R01、移动/镜头/交互/HUD、F5/F9 存读档  
6. 数据：`data/creatures/C001_wuxian.json`、l10n 扩展、StreamingAssets 副本  
7. `tools/validate_data.py`（已跑通 OK）  
8. EditMode 测试：`CreatureDefIdTests`  
9. `game/README.md` 运行说明  

### 没做什么（符合 Out）
- 结契 / 战斗 / 图志完整 UI / 进化  
- 手写完整 `.unity` 场景资产（改用运行时灰盒，避免无 Editor 时损坏场景）  
- Cinemachine 资产绑定（第三人称跟随用 `SimpleFollowCamera`；可在后续换成 Cinemachine）  
- UniTask  

---

## 验收对照（B11）

| ID | 结果 | 说明 |
|---|---|---|
| 新 clone 可开 `game/` | ⏳ 待你机 | 实现机无 Unity；工程结构已齐 |
| Boot 后进 R01 壳 | ✅ 代码路径 | `RuntimeWorldBuilder` / 场景名加载 |
| WASD 移动 + 镜头 | ✅ 代码 | `PlayerMotor` + `SimpleFollowCamera` |
| ≥1 Interactable | ✅ 代码 | 石碑 + E |
| HUD 区域名 | ✅ 代码 | IMGUI HUD（VS0 零 UXML 依赖） |
| F5/F9 存读档 + version | ✅ 代码 | `FileSaveService` |
| C001 + validate_data | ✅ | 脚本 OK；Catalog 加载 C001 |
| Domain 无 UnityEngine | ✅ | asmdef `noEngineReferences` |
| 无结契/战斗误入 | ✅ | Stub Mode 仅日志 |
| README 运行步骤 | ✅ | `game/README.md` |

---

## 测试

| 类型 | 结果 |
|---|---|
| `python tools/validate_data.py` | **OK**（2 creature files） |
| EditMode `CreatureDefIdTests` | 代码已交付；**未在本机跑 Unity Test Runner** |
| Play 手测 T0 | **未执行**（无 Editor） |

---

## 已知问题

| 级 | 项 |
|---|---|
| 说明 | 首次用本机 Unity 版本打开可能改写 `ProjectVersion.txt` / 生成 `packages-lock.json` — 属正常 |
| 轻微 | HUD 为 IMGUI，非 UI Toolkit 文档；VS1 可换 UXML 而不改 Domain |
| 轻微 | `packages-lock.json` 未提交，由首次解析生成 |
| 轻微 | 无 Build Settings 场景时始终 runtime shell；你可另存场景后加入 Build |
| 中 | **Play 验收尚未由人类签字** |

---

## 设计偏差

| 设计 | 实际 | 原因 |
|---|---|---|
| Cinemachine 跟随 | SimpleFollowCamera | 无 Editor 导入 Cinemachine 预制；架构可替换 |
| UI Toolkit HUD | IMGUI HudController | VS0 零资产可玩；表现层可替换 |
| Boot.unity / R01_Main.unity 资产 | Runtime + AutoBootstrap | 无 Unity CLI；避免坏 YAML |
| System.Text.Json | Unity JsonUtility + DTO | 与设计「System.Text.Json」略偏；Infra 内可日后换而不影响 Domain |

---

## 架构合规自检

- [x] 无上帝 Player 写规则公式  
- [x] Domain 无 Unity  
- [x] Mode 路由存在  
- [x] Save 含 version，定义不进存档权威  
- [x] 数据驱动样例 C001  

---

## 是否建议进入 VS1

**否** — 等你完成 `game/README.md` 手测清单并回复「VS0 验收通过」后，再宣告 VS1。

---

## 请你验收

打开 `game/` → Play → 按手测清单勾选 → 回复：

- **「VS0 验收通过」** 或  
- 问题列表（将返工）  
