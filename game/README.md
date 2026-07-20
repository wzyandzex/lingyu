# game/

**Unity 工程根目录（占位）。**

- 引擎：Unity + URP（见 `docs/decisions/ADR-006-engine-unity.md`）  
- 在 **VS0 设计包获用户批准实现之前**，不在此创建完整 Unity 工程与业务代码。  
- 批准后按 `docs/production/engineering-skeleton.md` 与 VS0 设计包生成工程。  

## 预期（批准后）

```text
game/
  Assets/
  Packages/
  ProjectSettings/
  ...
```

Domain 程序集不得引用 UnityEngine（见 `docs/production/architecture.md`）。
