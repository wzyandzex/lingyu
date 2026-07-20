# 数据根目录（data/）

本目录是**运行时权威游戏数据**的单源真相（JSON 等）。

- 设计散文在 `docs/`；入库后以这里为准。  
- 规范：`docs/production/content-pipeline.md`  
- 校验：`tools/validate_data/`（脚本随 VS0/VS1 落地）  
- **当前阶段**：仅 schema/占位，无完整内容生产（实现未开始）

```text
data/
  schema/           JSON Schema 或字段说明
  creatures/        物种定义
  skills/           技能定义
  encounters/       遭遇表
  regions/          区域配置
  l10n/             本地化字符串
  fixtures/         测试用夹具（可选）
```

在 VS0 设计包批准前，不承诺具体文件名最终形态；仅锁定「数据在仓内、与 Unity 工程分离」。
