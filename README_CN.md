# KToolkit
一个轻量的 Unity 游戏逻辑框架，最初用于独立游戏 **Exp10sion**。  
<https://store.steampowered.com/app/2618850/Exp10sion/>

## 快速开始
1. **导入框架**
   * 直接使用Release版本（推荐）
      - 在 GitHub Releases 中下载 ''最新的unitypackage文件'' 并导入引擎。
      - 该包内已包含预配置场景与示例脚本，导入后可直接打开示例场景 `KEntrance.unity` 体验。
      - 框架的启动代码已经包含在 `KEntrance.unity` 场景中，需要确保启动/进入游戏时必须以这个场景作为入口，或自行重构框架的启动和初始化方式。
    
   * 从源码开始手动配置框架
      - 直接将 `Framework/` 与 `Framework_Editor/` 复制到 Unity工程 `Assets/` 下的任意目录。
      - **需要准备默认管理器使用的场景，包含以下GameObject和对应的组件**：
         - `KCanvas`（UI 根节点，需要包含UGUI的Canvas组件）
         - `Main Camera` （场景的默认主相机）
         - `EventSystem` （需要包含UGUI交互需要使用的EvnetSystem组件）
         - `pool_transform_parent`（空物体，对象池挂载点）
      - 以上准备完成后，在自己的脚本中调用 `KFrameworkManager.instance.InitKFramework()`。即可启动并初始化本框架的功能。

## 项目结构
- `Framework/` 运行时系统：事件、UI、状态机、定时器、Tick、对象池、音频等。
- `Framework_Editor/` 编辑器工具。
- `Logo/` 渲染仓库LOGO所使用的2D SDF shader。

## 事件系统
### 主要文件
- `Framework/EventSystem/KEventSystem.cs`：`KEventManager` 静态分发器。
- `Framework/EventSystem/KObserver.cs`：`KObserver` / `KObserverNoMono` 基类。
- `Framework/Enums/KEventName.cs`：事件枚举（自行扩展）。

### 用法
- 继承 `KObserver`（`MonoBehaviour`）或 `KObserverNoMono`（纯 C#）。
- 使用 `AddEventListener(eventName, callback)` 注册。
- 使用 `KEventManager.SendNotification(eventName, params object[] args)` 发送。

### 示例
```csharp
using UnityEngine;
using KToolkit;

public class TestMono : MonoBehaviour
{
    private readonly TestObserverNoMono observer = new TestObserverNoMono();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            KEventManager.SendNotification(KEventName.TestEvent, "arg1", 2, 3);
        }
    }
}

public class TestObserverNoMono : KObserverNoMono
{
    public TestObserverNoMono()
    {
        AddEventListener(KEventName.TestEvent, args =>
        {
            Debug.Log("TestEvent Triggered!!");
            Debug.Log((string)args[0]);
            Debug.Log((int)args[1] + (int)args[2]);
        });
    }
}
```

## UI 框架
### 主要文件 / 类
- `Framework/UI/KUIBase.cs`：UI 页面基类。
- `Framework/UI/KUIManager.cs`：创建、销毁与更新。
- `Framework/UI/KUIAttributes.cs`：`KUI_Info` / `KUI_Cell_Info` 注册特性。
- `Framework/Enums/KPageEnum.cs`：`KUIManager` 自动注册逻辑。
- `Framework/UI/KUIPage.cs`：层级页面类型（仍在施工中）。

### 用法
- 新建类继承 `KUIBase`。
- 在 `Resources/UI_prefabs/` 下创建 prefab。
- 使用 `[KUI_Info("prefab_path", "PageName")]` 注册（路径相对 `Resources/UI_prefabs`）。
- 通过 `KUIManager.instance.CreateUI<TPage>(params object[] args)` 创建。
- 使用 `DestroySelf()` 或 `KUIManager.instance.DestroyUI(page)` 销毁。

### 示例
```csharp
using UnityEngine;
using UnityEngine.UI;
using KToolkit;

[KUI_Info("start_page", "StartMenuPage")]
public class StartMenuPage : KUIBase
{
    Button newGameButton;

    public override void InitParams(params object[] args)
    {
        base.InitParams(args);
        newGameButton = transform.Find("root/new_game_button").GetComponent<Button>();
        newGameButton.onClick.AddListener(OnNewGame);
    }

    void OnNewGame()
    {
        DestroySelf();
    }
}
```

## 状态机
### 主要文件
- `Framework/StateMachine/KStateMachineLib.cs`：`KIBaseState<T>` 与 `KStateMachine<T>`。

### 用法
- 为角色实现 `KIBaseState<T>`（如 `PlayerIdle`）。
- 使用 `KStateMachine<T>` 绑定 `MonoBehaviour` 与初始状态。
- 调用 `TransitState<TState>()` 切换状态。

### 示例
```csharp
public class PlayerIdle : KIBaseState<Player>
{
    public void EnterState(Player owner, params object[] args) {}
    public void HandleFixedUpdate(Player owner) {}
    public void HandleUpdate(Player owner) {}
    public void ExitState(Player owner) {}
    public void HandleCollide2D(Player owner, Collision2D collision) {}
    public void HandleTrigger2D(Player owner, Collider2D collider) {}
}
```

## 定时器
- `Framework/KTimerManager.cs`：延迟/循环回调。
- 每帧调用 `KTimerManager.instance.Update()`（`KFrameworkManager.Update` 已包含）。

## Tick 系统
- `Framework/TickSystem/KTickManager.cs`：固定频率 Tick（`IKTickable`）。
- 场景中放置一个 `KTickManager`，或由单例自动创建。

## 对象池
- `Framework/ObjectsPool.cs`：简易 prefab 池。
- 需要场景内存在 `pool_transform_parent` 作为挂载点。

## 音频
- `Framework/Audio/AudioManager.cs`：通过 Resources 路径播放 BGM/音效。
