# KToolkit
一个简单的Unity游戏逻辑框架
起源于独立游戏《Exp10sion》  
<u>https://store.steampowered.com/app/2618850/Exp10sion/</u>

# 启动方法
* 将KToolkit框架的所有代码放到Assets目录下；
* 在游戏初始化时调用一次KFrameworkManager.instance.InitKFramework();用以启动框架代码，即完成启动流程；

# 事件系统

## 主要文件
* KEventSystem.cs：全局管理事件系统的静态类，主要对外接口是SendNotification系列接口，用来发送事件通知。支持可变参数。
* KEventName.cs：事件枚举，主要用来新增事件名称，后续可能会扩展事件类型等。
* KObserver.cs：观察者基类，可选是否继承自MonoBehavior（KObserver类和KObserverNoMono类）。   


## 用法
* 如果一个类想要作为观察者，接入事件系统，则需要继承自KObserver（或者KObserverNoMono，如果不想继承自MonoBehavior的话）。
* 使用KObserver.AddEventListener()接口来注册对单个事件的监听，两个参数为：事件名称；对于该事件，这个观察者收到对应事件通知后调用的回调函数（对同一个事件不可以重复注册，重复注册同一事件以最后一次注册的结果为准，前面传入的回调都会被覆盖掉）。
* 使用KEventManager.SendNotification()接口来发送事件通知，SendNotification()接口会遍历所有观察者，删除其中已经失效的观察者，并调用每个有效的观察者对应事件回调。

## 实例
* 以下是一个使用KObserverNoMono类和一个普通MonoBehavior脚本实现的观察者模式实例；
* 把TestMono脚本挂到场景里，然后按下K键，即可打印出三行对于的内容（见下述代码）。
```csharp
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using KToolkit;


// 事件的发送者，做一个最简单的脚本组件
public class TestMono : MonoBehavior
{
    // 声明并缓存一个观察者（即事件的监听者）
    TestObserverNoMono observer = new TestObserverNoMono();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            int arg2 = 2;
            int arg3 = 3;
            KEventManager.SendNotification(KEventName.TestEventName_1, "arg1", arg2, arg3);
        }
    }
}


// 事件的接收者，继承自KObserver或KObserverNoMono即可
public class TestObserverNoMono : KObserverNoMono
{
    // 在构造函数，或MonoBehavior的Awake，Start函数等适合作逻辑初始化逻辑的位置监听事件
    TestObserverNoMono()
    {
        AddEventListener(KEventName.TestEventName_1, args =>
        {
            Debug.Log("TestEventName_1 Triggered!!");   // 打印字符串：TestEventName_1 Triggered!!
            Debug.Log((string)arg1);    // 打印字符串：arg1
            Debug.Log((int)arg2 + (int)arg3);   // 打印整数：5
        });
    }
}

```


# UI框架

## 主要文件/类
* KUIBase类：继承自KObserver。所有UI页面的基类，每一个单独的UI页面都应继承这个基类，并且将自定义页面的类型注册到PageEnum.cs文件里，或通过KUI_Info进行注册。
* KUIPage类（未完成，仍在施工中）：继承自KUIBase。注册方式与KUIBase一致，区别在于新创建的KUIPage会强制隐藏掉其他已经存在的KUIPage实例，也就是使当前新创建的KUIPage位于其他KUIPage的上方，并在其销毁时依次显示其下方的其他KUIPage。
* KUICell类：功能较轻量，需要依附于KUIBase类存在，用于放置一些简单且功能外观重复的UI元素，如菜单里的一排按钮，道具栏里的一排物品图标等。
* KUIManager类：在KUIManager.cs文件中定义了各类外部接口，在PageEnums.cs文件里定义了所有的UIPage的枚举，这个枚举的元素应该包含UI的prefab路径（相对于Resources目录）和UI的名字。


## 用法
* 如上，当你需要新建一个UI页面时，首先新建一个自己的页面类，并继承KUIBase
* 在PageEnum.cs中按照示例的格式注册新页面类型，或使用KUI_Info进行注册
* 在需要创建页面时，调用KUIManager.CreateUI<xxxPage>(xxx参数)；在需要销毁页面时，调用KUIManager.DestroyUI<xxxPage>()即可
* KUIBasePage的onStart，onDestroy和InitParams函数为虚函数，应根据需要做重载。其中InitParam用于接收UI传入的数据，onStart，onDestroy分别会在UI被创建和销毁时调用
* 可以在任意KUIBase的子类中使用CreateUICell函数来创建KUICell，在该KUIBase销毁时，其上的所有KUICell也会跟着销毁。


## 实例
* 以下是一个基于KUIBase的自定义页面例子，这个页面除了自带的按钮外，也含有使用KUICell动态创建的按钮。
```csharp
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using KToolkit;


// 通过KUI_Info的Attribute进行UI的注册
[KUI_Info("start_page", "StartMenuPage")]
// 定义一个自定义页面类StartMenuPage
public class StartMenuPage : KUIBase
{
    Button newGameButton;
    Button quitButton;
    Button tutorialButton;

    // 页面初始化函数
    public override void InitParams(params object[] args)
    {
        base.InitParams(args);
        
        // 监听外部事件，游戏开始时关闭这个菜单
        AddEventListener(KEventName.GameStartComplete, args =>
        {
            DestroySelf();
        });
        
        // 找到按钮并绑定事件
        newGameButton = transform.Find("root/new_game_button").GetComponent<Button>();
        quitButton = transform.Find("root/quit_button").GetComponent<Button>();
        tutorialButton = transform.Find("root/tutorial_button").GetComponent<Button>();

        newGameButton.onClick.AddListener(EnterNewGameRoleSelectState);
        quitButton.onClick.AddListener(QuitGame);
        tutorialButton.onClick.AddListener(Tutorial);
        
        // 在prefab里的root节点下创建五个按钮Cell
        for (int i = 0; i < 5; ++i)
        {
            cellList.Add(CreateUICell<MenuButtonCell>(transform.Find("root")));
        }
    }

    void EnterNewGameRoleSelectState()
    {
        KUIManager.instance.CreateUI<RoleSelectPage>();
    }

    void Tutorial()
    {
        KUIManager.instance.CreateUI<TutorialPage>();
    }

    void QuitGame()
    {
        Application.Quit();
    }
}


// 示例用按钮Cell，无功能，可自行定义
[KUI_Cell_Info("UI_prefabs/Cell/menu_button", "MenuButtonCell")]
public class MenuButtonCell : KUICell
{
    public override void OnCreate(params object[] args)
    {
        // 初始化按钮，如绑定按钮功能监听等
    }
}

```

## 备注
* KUIBasePage继承自KObserverNoMono类。事实上，如非特别说明，所有的类都应该继承自KObserver或KObserverNoMono类
* 目前功能还没那么多，对于比较复杂的UI逻辑还没有规范化的通用逻辑（UI层面主要是复杂列表需要通用化，如果未来页面的数据处理业务比较复杂，应该单分出来一个KUIBasePage内的新成员来管理数据）。后续有这方面需求或者新需求（如层级划分，排序，通用对话框等）也会继续在里面完善。

# 状态机

## 主要文件/类
* StateMachineLib.cs，内含抽象的BaseState类（状态类），和BaseFSM类（FSM即Finite State Machine，有限状态机）。


## 用法
* 本状态机和MonoBehavior脚本强绑定，即状态机必须持有一个MonoBehavior作为依附对象，只能依附于MonoBehavior存在。
* 要使用状态机时，需要针对指定的MonoBehavior类（如Player），单独新建一个继承自BaseState的PlayerBaseState类，和一个继承自BaseFSM的PlayerFSM类（命名无硬性规则，清晰易懂即可）。
* 定义需要的状态类（继承自PlayerBaseState）。如Player可能有站立，奔跑，跳跃状态，则可以定义三个状态类，并重写每个状态的HandleUpdate，HandleFixedUpdate，HandleTrigger等函数，定义当MonoBehavior对象在对应状态时的不同时期需要执行的行为。
* EnterState和ExitState函数分别会在进入和离开状态时被调用。使用BaseFSM.TransitState即可在不同状态间进行切换。
* 在MonoBehavior脚本中创建一个PlayerFSM实例并持有（即状态机和MonoBehavior互相持有对方）。并在对应的生命周期函数中调用状态的Handle方法（比如在Update函数中只调用stateMachine.currentState.HandleUpdate方法即可，不添加其他逻辑）。


## TODO：实例
* 施工中......


## 备注
* 应当尽量把MonoBehavior生命周期函数（主要是Update，FixedUpdate，OnTriggerEnter等会重复调用的函数）的业务逻辑写在状态机里。当然这样可能会产生很多需要复制粘贴的重复的逻辑，但是在一个多状态的复杂系统中，使用状态机会让整体逻辑更加清晰可读，增强可维护性。
* 封装一些所有状态的通用逻辑可以一定程度上解决重复逻辑冗余问题，比如在所有状态中都需要监听输入if(input(xxx))......那么可以考虑在对应的MonoBehavior中增加一个HandleInput方法，把输入逻辑都放进去，并在所有状态中调用HandleInput减少重复代码量。
