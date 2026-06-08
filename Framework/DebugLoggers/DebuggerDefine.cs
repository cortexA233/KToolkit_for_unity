using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KToolkit
{
    public static partial class KDebugLogger
    {
        public static void LevelFlow_DebugLog(params object[] args)
        {
            DebugLogIfEnabled("LevelFlow", "green", "关卡流程", args);
        }
        
        public static void Charge_DebugLog(params object[] args)
        {
            DebugLogIfEnabled("Charge", "magenta", "电荷", args);
        }
        
        public static void ChargeVerbose_DebugLog(params object[] args)
        {
            DebugLogIfEnabled("ChargeVerbose", "magenta", "电荷详细", args);
        }
        
        public static void Water_DebugLog(params object[] args)
        {
            DebugLogIfEnabled("Water", "cyan", "水体", args);
        }

        public static void WaterVerbose_DebugLog(params object[] args)
        {
            DebugLogIfEnabled("WaterVerbose", "cyan", "水体详细", args);
        }

        public static void PlayerEvent_DebugLog(params object[] args)
        {
            DebugLogIfEnabled("PlayerEvent", "blue", "玩家事件", args);
        }

        public static void PlayerBallControl_DebugLog(params object[] args)
        {
            DebugLogIfEnabled("PlayerBallControl", "blue", "玩家小球控制", args);
        }

        private static void DebugLogIfEnabled(string key, string color, string label, params object[] args)
        {
            if (!IsDebuggerEnabled(key))
            {
                return;
            }

            string res = $"<color={color}>{label}日志：</color> " + DebuggerConcatArgs(args);
            Debug.Log(res);
        }
    }
}
