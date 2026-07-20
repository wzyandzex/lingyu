# Git 与仓库卫生（Repo Hygiene）v1.0

> 状态：强制基线（工程落地前）  
> 远程：`https://github.com/wzyandzex/lingyu.git`  
> 产品代号：**澄界 / Aetherion**（仓库名 `lingyu` 为对外 Git 标识，不改变产品名）

---

## 1. 目的

没有可审计版本史，ADR/PRD/切片验收都不可回放。  
本文件锁定：**分支、提交、同步义务、LFS、Unity meta** 的最低纪律。

---

## 2. 远程与分支

| 项 | 约定 |
|---|---|
| 远程名 | `origin` |
| 默认分支 | `main` |
| 保护 | 理想情况：`main` 禁止 force-push（在 GitHub 设置中开启） |
| 功能分支 | `vs/{n}-short-name`（如 `vs/0-world-shell`）；文档-only 可用 `docs/...` |
| 热修 | 预制作期尽量不用；有则 `fix/...` |

**规则**
- 文档与规范变更可直接上 `main`（预制作期），但必须可复述提交说明。  
- 业务实现（VS 批准后）优先分支 + PR，或至少本地可回滚的清晰提交序列。

---

## 3. 提交信息

```text
<area>: <imperative summary>

可选正文：为什么 / 影响面 / 关联 PRD 或切片
```

`area` 示例：`docs` / `agents` / `data` / `tools` / `game` / `workflow` / `adr`

示例：
- `docs: fix engine status drift in roadmap`
- `adr: rename ADR-006 to engine-unity`
- `workflow: add content pipeline and playtest protocol`

禁止：无意义 `update`、混装无关大改却无说明。

---

## 4. 同步义务（防文档漂移）

下列变更必须在**同一次提交或紧随提交**中同步相关索引：

| 变更类型 | 必须同步 |
|---|---|
| 引擎/栈/架构 ADR | `README.md`、`roadmap.md`、`tech-stack.md`、相关 ADR 文件名与交叉引用 |
| 切片阶段变化 | `docs/production/slices/STATUS.md` |
| 新强制工作流 | `AGENTS.md` 摘要 + `docs/README.md` 索引 |
| 精灵完成度 | `creatures/roster/README.md` |
| 目录骨架增减 | `engineering-skeleton.md`、根 `README.md` |

**漂移 = 缺陷。** 审查时发现「文档互相打架」按 P0 修，不按文案润色处理。

---

## 5. Unity 工程进仓后（VS0 实现期激活）

1. **永远提交 `.meta`**，且与资源同进同出。  
2. `ProjectSettings/`、`Packages/manifest.json`、`Packages/packages-lock.json` 纳入版本控制。  
3. **忽略** `Library/` `Temp/` `Obj/` `Logs/` `UserSettings/`（见根 `.gitignore`）。  
4. 大资源（贴图/音频/FBX）启用 **Git LFS** 后再批量导入；启用步骤写入本文件修订版。  
5. 线尾：文本 `lf`（见 `.gitattributes`）；勿在 Windows 上无规范地混用 CRLF 提交。  
6. 禁止把本地绝对路径、机器用户名写进共享资源。

### LFS 建议跟踪（资产进入时）

```text
*.png *.jpg *.jpeg *.tga *.psd *.exr
*.wav *.mp3 *.ogg
*.fbx *.blend
*.ttf *.otf
```

---

## 6. 目录所有权（仓库根）

```text
/
  AGENTS.md          宪法
  README.md          人类入口
  docs/              设计与生产单源真相
  data/              权威游戏数据（JSON 等）；无 Unity 引用
  tools/             校验/导入/CI 脚本
  game/              Unity 工程（批准 VS0 实现后创建）
```

- `docs/**` 可独立演进；实现不得与 Accepted ADR / PRD 冲突。  
- `data/**` 是内容 SSOT；Markdown roster 是设计稿，**入库后以 data 为准**（见 content-pipeline）。  
- `game/**` 不得成为唯一内容真相。

---

## 7. 密钥与隐私

- 禁止提交账号、token、证书、本地绝对密钥路径。  
- `.env` / `secrets/` 已忽略。  
- 若误提交：立刻轮换密钥，并视作事故记录到 `docs/production/risks.md`。

---

## 8. 验收（仓库卫生 DoD）

- [ ] `git status` 干净策略可理解  
- [ ] `origin` 指向 `wzyandzex/lingyu`  
- [ ] `.gitignore` / `.gitattributes` / `.editorconfig` 存在  
- [ ] 无 `Library/` 等 Unity 垃圾进仓  
- [ ] README 与 roadmap 引擎状态一致  

---

## 9. 版本

| 版本 | 日期 | 说明 |
|---|---|---|
| v1.0 | 2026-07-20 | 首版：远程、分支、同步义务、Unity/LFS 预约束 |
