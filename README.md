# KToolkit
A Simple Unity Game Logic Framework
Originated from the indie game "Exp10sion"
<u>https://store.steampowered.com/app/2618850/Exp10sion/</u>

简体中文文档（施工中）：
<u>[点击这里](https://github.com/cortexA233/KToolkit/blob/main/README_CN.md)</u>

# Event System
## Main Files
* EventSystem.cs: A static class for global management of the event system, with the main external interface being the SendNotification series of methods used for sending event notifications. It supports variable parameters.
* EventEnum.cs: Event enumeration, primarily used for adding event names. Event types may be expanded in the future.
* KObserver.cs: Base class for observers, optional to inherit from MonoBehavior.
## Usage
* If a class wants to be an observer and integrate with the event system, it needs to inherit from KObserver (or KObserverNoMono if it doesn't want to inherit from MonoBehavior).
* Use KObserver.AddEventListener() to register for listening to a specific event. The two parameters are: event name and the callback function for the event when the observer receives the corresponding notification. (You cannot register the same event multiple times; the last registration will overwrite previous callbacks).
* Use EventSystem.SendNotification() to send event notifications. SendNotification() will iterate over all observers, remove any invalid ones, and call the callback for each valid observer corresponding to the event.
## TODO: Example
* Under construction......


# UI Framework
## Main Files/Classes
* KUIBasePage Class: The base class for all UI pages. Each UI page should inherit from this base class and register the custom page type in PageEnum.cs.
* KUIManager Class: The KUIManager.cs file defines external interfaces for various UI operations. The PageEnum.cs file defines an enumeration for all UI pages. Each element of this enum should contain the prefab path (relative to the Resources directory) and the UI name.
## Usage
* When creating a new UI page, first create your page class and inherit from KUIBasePage.
Register the new page type in PageEnum.cs following the example format.
* To create a page, call KUIManager.CreateUI<xxxPage>(xxx parameters). To destroy a page, call KUIManager.DestroyUI<xxxPage>().
* KUIBasePage's onStart, onDestroy, and InitParams functions are virtual methods and should be overridden as needed. InitParams is used to receive data passed to the UI, while onStart and onDestroy are called when the UI is created or destroyed.
## TODO: Example
* Under construction......
## Notes
* KUIBasePage inherits from KObserverNoMono class. In fact, unless otherwise specified, all classes should inherit from KObserver or KObserverNoMono.
Currently, there are not many features. There is no standardized logic for more complex UI interactions (mainly for complex lists that need to be generalized; if future pages require complex data handling, a new member in KUIBasePage should be created to manage the data). Future updates will continue to improve this based on new requirements (like hierarchy divisions, sorting, generic dialogs, etc.).

# State Machine
## Main Files/Classes
* StateMachineLib.cs: Contains the abstract BaseState class (state class) and BaseFSM class (FSM, Finite State Machine).
Usage
* This state machine is tightly coupled with MonoBehavior, meaning the state machine must hold a MonoBehavior as its attached object and can only exist as an attachment to a MonoBehavior.
* To use the state machine, create a new class that inherits from BaseState for a specific MonoBehavior class (e.g., Player), and create a PlayerFSM class inheriting from BaseFSM (naming is flexible, but should be clear and understandable).
* Define the necessary state classes (inheriting from PlayerBaseState). For example, if the Player has standing, running, and jumping states, three state classes can be created, and each state should override methods such as HandleUpdate, HandleFixedUpdate, HandleTrigger, etc., defining the behavior for each state at different times.
* The EnterState and ExitState functions are called when entering or exiting a state. Use BaseFSM.TransitState to switch between different states.
In the MonoBehavior script, create an instance of PlayerFSM and hold it (the state machine and MonoBehavior mutually hold each other). In the corresponding lifecycle methods, call the state's Handle method (e.g., in the Update method, call stateMachine.currentState.HandleUpdate without adding any other logic).
## TODO: Example
* Under construction......
## Notes
* The business logic for MonoBehavior lifecycle methods (such as Update, FixedUpdate, OnTriggerEnter, etc., which are called repeatedly) should be written inside the state machine. While this may result in repetitive logic, using a state machine in a complex system with many states will make the overall logic clearer, more readable, and maintainable.
* To avoid redundant logic, you can encapsulate common logic across all states. For example, if all states need to listen to input (if(input(xxx))), consider adding a HandleInput method in the corresponding MonoBehavior, putting all input logic there, and calling HandleInput in all states to reduce code duplication.
