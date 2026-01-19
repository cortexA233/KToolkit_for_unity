# KToolkit
A lightweight Unity gameplay framework originally created for the indie game **Exp10sion**.  
<https://store.steampowered.com/app/2618850/Exp10sion/>

## Quick Start
1. **Import the framework**
   1.1 Use the Release package (recommended)
      - Download the latest `.unitypackage` from GitHub Releases and import it.
      - The package already includes a preconfigured scene and sample scripts. After importing, you can open
        `KEntrance.unity` to try it out.
      - The framework bootstrap code is already included in `KEntrance.unity`. Make sure your game starts from
        this scene, or refactor the startup/initialization on your own.
   1.2 Configure manually from source
      - Copy `Framework/` and `Framework_Editor/` into any folder under your Unity project's `Assets/`.
      - **Prepare the default manager scene objects with the following GameObjects and components**:
         - `KCanvas` (UI root; must have a UGUI Canvas component)
         - `Main Camera` (default main camera)
         - `EventSystem` (must include the UGUI EventSystem component)
         - `pool_transform_parent` (empty object used as the pool mount point)
      - After the setup, call `KFrameworkManager.instance.InitKFramework()` in your own script to boot and
        initialize the framework.

## Project Structure
- `Framework/` runtime systems: event system, UI framework, state machine, timers, tick system, pooling, audio, etc.
- `Framework_Editor/` editor helpers.
- `Logo/` 2D SDF shader used to render the repository logo.

## Event System
### Main Files
- `Framework/EventSystem/KEventSystem.cs`: `KEventManager` static dispatcher.
- `Framework/EventSystem/KObserver.cs`: `KObserver` / `KObserverNoMono` base classes.
- `Framework/Enums/KEventName.cs`: event name enum (extend it for your game).

### Usage
- Inherit from `KObserver` (for `MonoBehaviour`) or `KObserverNoMono` (plain C#).
- Register listeners via `AddEventListener(eventName, callback)`.
- Send events via `KEventManager.SendNotification(eventName, params object[] args)`.

### Example
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

## UI Framework
### Main Files / Classes
- `Framework/UI/KUIBase.cs`: base class for UI pages.
- `Framework/UI/KUIManager.cs`: creation, destruction, and update.
- `Framework/UI/KUIAttributes.cs`: `KUI_Info` / `KUI_Cell_Info` registration attributes.
- `Framework/Enums/KPageEnum.cs`: `KUIManager` auto-registration helpers.
- `Framework/UI/KUIPage.cs`: layered page type (currently TODO).

### Usage
- Create a class inheriting from `KUIBase`.
- Create a prefab under `Resources/UI_prefabs/`.
- Register with `[KUI_Info("prefab_path", "PageName")]` (path is relative to `Resources/UI_prefabs`).
- Call `KUIManager.instance.CreateUI<TPage>(params object[] args)` to show it.
- Use `DestroySelf()` or `KUIManager.instance.DestroyUI(page)` to remove it.

### Example
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

## State Machine
### Main File
- `Framework/StateMachine/KStateMachineLib.cs`: `KIBaseState<T>` and `KStateMachine<T>`.

### Usage
- Implement `KIBaseState<T>` for your actor (e.g., `PlayerStateIdle`).
- Create `KStateMachine<T>` with an owner `MonoBehaviour` and initial state.
- Call `TransitState<TState>()` to switch states.

### Example
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

## Timer System
- `Framework/KTimerManager.cs`: schedule delayed or looping callbacks.
- Call `KTimerManager.instance.Update()` every frame (already invoked by `KFrameworkManager.Update`).

## Tick System
- `Framework/TickSystem/KTickManager.cs`: fixed-rate tick loop (`IKTickable`).
- Place one `KTickManager` in the scene or let the singleton create it.

## Object Pool
- `Framework/ObjectsPool.cs`: simple prefab pool.
- Requires a `pool_transform_parent` object in the scene for pooled instances.

## Audio
- `Framework/Audio/AudioManager.cs`: BGM/effect playback via Resources paths.
