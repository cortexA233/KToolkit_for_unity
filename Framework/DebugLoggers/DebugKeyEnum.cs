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
        }
        
        private static void InitDebuggerConfig()
        {
            debuggerConfig["Example"] = true;
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