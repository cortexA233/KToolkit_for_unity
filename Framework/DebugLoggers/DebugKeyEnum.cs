using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KToolkit
{
    public static partial class KDebugLogger
    {
        public static Dictionary<string, bool> debuggerConfig = new Dictionary<string, bool>();

        static KDebugLogger()
        {
            InitDebuggerConfig();
            debuggerConfig["UI"] = true;
        }
        
        private static void InitDebuggerConfig()
        {
            debuggerConfig["UI"] = true;
            debuggerConfig["Battle"] = true;
            debuggerConfig["System"] = true;
            debuggerConfig["Player"] = true;
            debuggerConfig["Level"] = true;
        
            debuggerConfig["Cortex"] = true;
            debuggerConfig["Veyo"] = false;
            debuggerConfig["Shin"] = true;
        }

        private static string DebuggerConcatArgs(params object[] args)
        {
            string res = "";
            foreach (var item in args)
            {
                res += item.ToString();
                res += "  ";
            }
            return res;
        }
    }

}