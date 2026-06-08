# AGENTS

## 适用范围

- 本文件适用于 `Assets/Scripts/KToolkit_for_unity/` 子模块及其所有子目录。
- `CLAUDE.md` 仅作为兼容入口，实际协作规则以本文件为准。

## 语言约定

- 本文件后续更新统一使用中文。
- 面向项目成员或后续代理的说明优先使用中文，必要的 API 名称、类型名和路径保持原文。

## 修改原则

- KToolkit 是可复用的 Unity 轻量级框架，修改时优先保持通用性，不引入宿主项目或父仓库的具体玩法、场景、资源或业务依赖。
- Runtime 与 Editor 代码保持清晰边界：运行时代码放在 `Framework/`，编辑器扩展放在 `Editor/`，运行时程序集不得依赖 `UnityEditor`。
- 对外 API、特性、枚举和单例初始化流程要谨慎改动；如必须调整，需要同步考虑现有调用方和后续自定义 Unity 编辑器工具的安全复用。
- 新增能力控制规模和实现难度，优先选择能快速落地、低耦合、便于后续迭代的方案。
- 后续所有 ScriptableObject 配置和 MonoBehaviour 里的脚本配置字段，都要加上中文 Header。
- 如非特殊提出需求，实现或迭代功能时，不需要单独编写 Editor 测试代码。

## 框架架构概览

入口：通过 `KFrameworkManager.instance.InitKFramework()` 初始化 UI 框架，并创建或复用持久化的 `pool_transform_parent` GameObject 作为对象池父节点。

`KFrameworkManager.Update()` 负责驱动 UI、Timer、Tick 的运行时更新循环。

`KToolkitRuntimeInitializer` 使用 `[RuntimeInitializeOnLoadMethod]` + 反射扫描程序集，在 Editor Domain Reload 后自动重置所有单例状态，确保 Play Mode 进入时的一致性。

**核心子系统**：

| 子系统 | 路径 | 说明 |
|--------|------|------|
| 单例基类 | `Framework/Singleton.cs` | `KSingleton<T>`（MonoBehaviour）支持懒加载并在自动创建实例时使用 `DontDestroyOnLoad`；`KSingletonNoMono<T>`（纯 C#）提供懒加载实例，不涉及场景对象持久化 |
| 事件系统 | `Framework/EventSystem/` | `KEventManager` 静态派发，`KObserver`/`KObserverNoMono` 为监听者基类，事件名通过 `KEventName` 枚举类型安全地定义 |
| 状态机 | `Framework/StateMachine/KStateMachineLib.cs` | `KStateMachine<TOwner>` 泛型实现，状态实现 `KIBaseState<TOwner>`，通过 `TransitState<TState>()` 切换，支持 2D 碰撞/触发回调 |
| UI 框架 | `Framework/UI/` | `KUIManager`（非 Mono 单例）管理 `KUIPage`/`KUICell` 层级；通过 `[KUI_Info]` 特性注册预制体路径；Canvas 固定 1920×1080 ScreenSpace-Overlay |
| 定时器 | `Framework/KTimerManager.cs` | Guid 追踪的延迟/循环回调，支持非缩放时间和对象池复用 |
| Tick 系统 | `Framework/TickSystem/KTickManager.cs` | 固定频率 tick（可配置），实现 `IKTickable` 接口订阅；中途增删使用 pending 列表防止迭代破坏 |
| 对象池 | `Framework/ObjectsPool.cs` | 抽象基类，Queue 复用，支持最大数量限制和父节点管理 |

## 程序集定义

- `KToolkit.asmdef`：框架运行时。
- `Editor/KToolkit.Editor.asmdef`：编辑器扩展。

## Unity 与资源约定

- 当前子模块 `.gitignore` 会忽略 `*.meta`，新增文档或脚本时遵循该子模块现有提交约定，不强行提交 `.meta` 文件。
- 不随意移动或重命名已有框架文件；如确实需要，需考虑 `.asmdef`、命名空间、README 和引用关系。
- 不在 KToolkit 子模块内放置项目专用 Prefab、场景、贴图或玩法配置。

## 测试与验证

- 需要验证框架运行逻辑时，可通过 Unity Test Runner 运行 EditMode 或 PlayMode 测试。
- 涉及编辑器工具时，优先手动验证窗口打开、反射扫描、Domain Reload 后初始化状态是否正常。
