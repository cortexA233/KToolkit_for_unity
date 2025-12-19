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
    }
}
