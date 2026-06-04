using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KToolkit
{
    public static partial class KDebugLogger
    {
        public static void Example_DebugLog(params object[] args)
        {
            if (!debuggerConfig.ContainsKey("Example") || !debuggerConfig["Example"])
            {
                return;
            }

            string res = "<color=yellow>Example Log:</color> " + DebuggerConcatArgs(args);
            Debug.Log(res);
        }
        
        public static void Cortex_DebugLog(params object[] args)
        {
            if (!debuggerConfig.ContainsKey("Cortex") || !debuggerConfig["Cortex"])
            {
                return;
            }

            string res = "<color=yellow>Cortex Log:</color> " + DebuggerConcatArgs(args);
            Debug.Log(res);
        }
        
        public static void PlayerBallControl_DebugLog(params object[] args)
        {
            if (!debuggerConfig.ContainsKey("PlayerBallControl") || !debuggerConfig["PlayerBallControl"])
            {
                return;
            }

            string res = "<color=blue>PlayerBallControl Log:</color> " + DebuggerConcatArgs(args);
            Debug.Log(res);
        }
        
        public static void Water_DebugLog(params object[] args)
        {
            if (!debuggerConfig.ContainsKey("Water") || !debuggerConfig["Water"])
            {
                return;
            }

            string res = "<color=cyan>Water Log:</color> " + DebuggerConcatArgs(args);
            Debug.Log(res);
        }
    }
}
