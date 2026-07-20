# Unity 工程（game/）— VS0 世界壳

> 产品：澄界 / Aetherion  
> 本机已检测到：**Unity 2022.3.62f3c1**（`ProjectVersion.txt`）  
> 包版本已按 **2022.3 LTS** 对齐（勿用 Unity 6 专用 URP 17）

---

## 若进入 Safe Mode / 看不到场景（按这个恢复）

1. **关闭 Unity**  
2. 确认仓库已更新到最新（含本次修复）  
3. 删除本机缓存（可选但推荐，包解析坏了时必做）：
   - 删掉 `game/Library/` 整个文件夹（会再导入，几分钟）  
   - 删掉 `game/Packages/packages-lock.json`（若存在）  
4. 用 **Unity 2022.3.x** 通过 Hub 打开 `game/`  
5. 若仍提示 Safe Mode：点 **Ignore** 或修编译错误后 **Exit Safe Mode**  
6. 菜单 **Aetherion → VS0 → Open Boot Scene**  
   或手动打开：`Assets/_Project/Scenes/Boot/Boot.unity`  
7. 按 **Play**  
   - `AutoBootstrap` 会创建 `GameBootstrap`  
   - 运行时生成 R01 灰盒（地面 / 玩家 / 石碑 / HUD）  
   - Hierarchy 里应出现 `R01_RuntimeRoot`  

**原因说明（已修）**
- 原先 `manifest` 写了 **URP 17**（仅 Unity 6），在 2022.3 上会解析失败 → Safe Mode  
- 硬引用 **Input System** 程序集，包失败时脚本全红  
- `FindFirstObjectByType` 在 2022.3 不可用  
- 没有可打开的 `Boot.unity`，空场景看起来像“没有场景”

---

## 操作

| 操作 | 键 |
|---|---|
| 移动 | WASD |
| 环视 | 按住 **鼠标右键** + 移动 |
| 交互 | **E**（靠近石碑） |
| 保存 | **F5** |
| 读档 | **F9** |

---

## 数据

- 权威：`../data/`（Editor 下优先）  
- 构建副本：`Assets/StreamingAssets/data/`  
- 校验（仓库根）：`python tools/validate_data.py`

---

## 架构

Domain / Application / Infrastructure / Presentation 程序集见 `Assets/_Project/Scripts/`。  
Domain：`noEngineReferences: true`。
