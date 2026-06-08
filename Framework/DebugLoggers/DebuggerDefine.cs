using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KToolkit
{
    public static partial class KDebugLogger
    {
        public static void LevelFlow_DebugLog(params object[] args)
        {
            DebugLogIfEnabled("LevelFlow", "green", "LevelFlow", args);
        }
        
        public static void Charge_DebugLog(params object[] args)
        {
            DebugLogIfEnabled("Charge", "magenta", "Charge", args);
        }
        
        public static void ChargeVerbose_DebugLog(params object[] args)
        {
            DebugLogIfEnabled("ChargeVerbose", "magenta", "ChargeVerbose", args);
        }
        
        public static void Water_DebugLog(params object[] args)
        {
            DebugLogIfEnabled("Water", "cyan", "Water", args);
        }

        public static void WaterVerbose_DebugLog(params object[] args)
        {
            DebugLogIfEnabled("WaterVerbose", "cyan", "WaterVerbose", args);
        }

        public static void PlayerEvent_DebugLog(params object[] args)
        {
            DebugLogIfEnabled("PlayerEvent", "blue", "PlayerEvent", args);
        }

        public static void PlayerBallControl_DebugLog(params object[] args)
        {
            DebugLogIfEnabled("PlayerBallControl", "blue", "PlayerBallControl", args);
        }

        private static void DebugLogIfEnabled(string key, string color, string label, params object[] args)
        {
            if (!IsDebuggerEnabled(key))
            {
                return;
            }

            string res = $"<color={color}>{label} Log:</color> " + DebuggerConcatArgs(args);
            Debug.Log(res);
        }
    }
}
