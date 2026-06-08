public enum KEventName
{
    TestEvent,          // Just for Demo
    
    // Triggered when task status changed,
    // param type:
    //      param1: TaskConfig, to indicate which task is proceeding 
    //      param2: TaskStateEnum, to indicate which state the task is in
    TaskStatusChange,

    // Triggered once when every configured task reaches Completed.
    AllTasksCompleted,
}
