# AGENTS

## 适用范围

- 本文件适用于 `Assets/Scripts/KToolkit_for_unity/` 子模块及其所有子目录。
- `CLAUDE.md` 仅作为兼容入口，实际协作规则以本文件为准。

## 语言约定

- 本文件后续更新统一使用中文。
- 面向项目成员或后续代理的说明优先使用中文，必要的 API 名称、类型名和路径保持原文。

## 修改原则

- KToolkit 是可复用的 Unity 轻量级框架，修改时优先保持通用性，不引入 `Endfield_test` 的具体玩法、场景、资源或业务依赖。
- Runtime 与 Editor 代码保持清晰边界：运行时代码放在 `Framework/`，编辑器扩展放在 `Editor/`，运行时程序集不得依赖 `UnityEditor`。
- 对外 API、特性、枚举和单例初始化流程要谨慎改动；如必须调整，需要同步考虑现有调用方和后续自定义 Unity 编辑器工具的安全复用。
- 新增能力控制规模和实现难度，优先选择能快速落地、低耦合、便于后续迭代的方案。
- 后续所有 ScriptableObject 配置和 MonoBehaviour 里的脚本配置字段，都要加上中文 Header。
- 如非特殊提出需求，实现或迭代功能时，不需要单独编写 Editor 测试代码。

## Unity 与资源约定

- 当前子模块 `.gitignore` 会忽略 `*.meta`，新增文档或脚本时遵循该子模块现有提交约定，不强行提交 `.meta` 文件。
- 不随意移动或重命名已有框架文件；如确实需要，需考虑 `.asmdef`、命名空间、README 和引用关系。
- 不在 KToolkit 子模块内放置项目专用 Prefab、场景、贴图或玩法配置。

## 测试与验证

- 需要验证框架运行逻辑时，可通过 Unity Test Runner 运行 EditMode 或 PlayMode 测试。
- 涉及编辑器工具时，优先手动验证窗口打开、反射扫描、Domain Reload 后初始化状态是否正常。
