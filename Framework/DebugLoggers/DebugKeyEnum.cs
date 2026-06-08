using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KToolkit
{
    public static partial class KDebugLogger
    {
        public static Dictionary<string, bool> debuggerConfig = new Dictionary<string, bool>();
        public static IReadOnlyDictionary<string, bool> DebuggerConfig => debuggerConfig;

        static KDebugLogger()
        {
            ResetRuntimeState();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void ResetRuntimeState()
        {
            debuggerConfig.Clear();
            InitDebuggerConfig();
        }
        
        private static void InitDebuggerConfig()
        {
            debuggerConfig["LevelFlow"] = true;
            debuggerConfig["Charge"] = true;
            debuggerConfig["ChargeVerbose"] = false;
            debuggerConfig["Water"] = true;
            debuggerConfig["WaterVerbose"] = false;
            debuggerConfig["PlayerEvent"] = true;
            debuggerConfig["PlayerBallControl"] = false;
        }

        public static List<string> GetDebuggerKeys()
        {
            List<string> keys = new List<string>(debuggerConfig.Keys);
            keys.Sort();
            return keys;
        }

        public static bool IsDebuggerEnabled(string key)
        {
            return !string.IsNullOrEmpty(key)
                && debuggerConfig.TryGetValue(key, out bool enabled)
                && enabled;
        }

        public static void SetDebuggerEnabled(string key, bool enabled)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            debuggerConfig[key] = enabled;
        }

        public static void SetAllDebuggersEnabled(bool enabled)
        {
            List<string> keys = new List<string>(debuggerConfig.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                debuggerConfig[keys[i]] = enabled;
            }
        }

        public static void ResetDebuggerConfigToDefault()
        {
            debuggerConfig.Clear();
            InitDebuggerConfig();
        }

        private static string DebuggerConcatArgs(params object[] args)
        {
            string res = "";
            if (args == null)
            {
                return "null  ";
            }

            foreach (var item in args)
            {
                res += item != null ? item.ToString() : "null";
                res += "  ";
            }
            return res;
        }
    }

}
