public enum KEventName
{
    GameStartComplete,              // 游戏开始载入完成，参数1:PlayerSnakeType，表示玩家角色类型，参数2（可空）:PlayerWeaponType，表示玩家武器类型
    GameEnd,                        // 游戏结束，暂时无参数
    GamePause,                      // 游戏暂停，暂时无参数
    GameResume,                     // 游戏继续，暂时无参数
    
    PlayerDie,                      // 玩家生命值归零，暂时无参数
    
    EnemyDie,                       // 敌人死亡，暂时无参数
    BodyRecycle,                    // 身体进入黑洞回收
    
    LevelUp,                        // 玩家升级，暂时无参数
    AddExpNotify,                   // 玩家经验上升通知，参数1:int（上升的经验值）
    LevelUpEnhancementComplete,     // 玩家升级强化完成，参数1:WordEntry（点击的升级卡片对应词条）
    
    SwitchAutoAim,                    // 开/关自动瞄准，暂无参数
    DamageNotify,                     // 受伤通知，参数1:GameObject（受伤的对象），参数2:int（受伤值），可选参数3:Vector2（受击判定点）
    PlayerDamageNotify,               // 玩家受伤通知，一般来说仅限受击，流失体力之类的不算，暂无参数
    
    RoleSelectComplete,               // 角色选择完成，参数1:PlayerSnakeType（选择的玩家角色类型）
    WeaponSelectComplete,              // 武器选择完成，参数1:PlayerWeaponType（选择的武器类型）
    
    // UI，交互相关事件
    StartJumpText,               // 通知进行一次跳字，参数1:string（跳字内容）
}