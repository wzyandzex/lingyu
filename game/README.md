# Unity 工程（game/）— VS0 世界壳

> 产品：澄界 / Aetherion  
> 切片：VS0（已实现代码基线，待你本机 Play 验收）  
> 引擎：Unity 6.x（见 `ProjectSettings/ProjectVersion.txt`，可按本机 Hub 版本调整）

---

## 首次打开

1. 安装 **Unity Hub** + **Unity 6**（推荐与 `ProjectVersion.txt` 接近的 6000.x；若版本提示不匹配，用本机已装 6.x 打开并允许升级/生成 lock）  
2. Hub → **Add** → 选择本仓库下的 `game/` 目录  
3. 等待 Package Manager 解析 `Packages/manifest.json`（Input System、URP、Test Framework 等）  
4. 任意场景按 **Play**  
   - `AutoBootstrap` 会创建 `GameBootstrap`  
   - 若无 Build Settings 场景，会调用 `RuntimeWorldBuilder` 生成 R01 灰盒  

> 本机实现环境**未检测到 Unity CLI**，因此场景采用运行时灰盒，避免手写脆弱 `.unity` YAML。你在 Editor 中可另存 `Boot.unity` / `R01_Main.unity` 并加入 Build Settings。

---

## 操作（VS0）

| 操作 | 默认 |
|---|---|
| 移动 | WASD / 方向键 |
| 环视 | 按住 **鼠标右键** 移动鼠标 |
| 交互 | **E**（靠近石碑时 HUD 提示） |
| 保存 | **F5** |
| 读档 | **F9** |

---

## 架构落点

| 程序集 | 路径 |
|---|---|
| `Aetherion.Domain` | `Assets/_Project/Scripts/Domain`（`noEngineReferences`） |
| `Aetherion.Application` | `.../Application` |
| `Aetherion.Infrastructure` | `.../Infrastructure` |
| `Aetherion.Presentation` | `.../Presentation` |
| `Aetherion.Tests` | `Assets/_Project/Tests/EditMode` |

组合根：`GameBootstrap`  
数据：Editor 读仓库根 `data/`；Player 读 `StreamingAssets/data/`  
存档：`Application.persistentDataPath/saves/slot0.json`（含 `version`）

---

## 数据校验（仓库根）

```bash
python tools/validate_data.py
```

应输出 `validate_data: OK`。

---

## EditMode 测试

Unity → Window → General → Test Runner → EditMode → Run All  
期望：`CreatureDefIdTests` 通过。

---

## 手测清单（T0）

对照 `docs/production/slices/VS0-design.md` B11：

- [ ] Play 无报错刷屏  
- [ ] 可移动；镜头跟随  
- [ ] 靠近石碑有交互提示；E 出文案  
- [ ] HUD 显示区域名  
- [ ] F5 保存 / 移动后 F9 回到存档位置  
- [ ] Console 出现 C001 / 雾衔 加载日志  
- [ ] Domain 程序集无 Unity 引用（asmdef `noEngineReferences`）  

完成后在 `docs/production/slices/STATUS.md` 标记验收结果。
