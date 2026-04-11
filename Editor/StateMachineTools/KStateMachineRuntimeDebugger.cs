using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;


// todo：增加一个log开关，可选切换状态的时候同时打log
namespace KToolkit
{
    /// <summary>
    /// 🎮 KStateMachine Runtime Debugger
    /// 实时查看播放模式下的 KStateMachine 当前状态与历史（持久化、增量更新）
    /// Real-time debugger for KStateMachine<> with persistent, incremental updates.
    /// </summary>
    public class KStateMachineRuntimeDebugger : EditorWindow
    {
        // ────────────────────────────────────────────────────────────────────────────────
        // 数据结构 / Data Structures
        // ────────────────────────────────────────────────────────────────────────────────
        private class RuntimeStateMachineInfo
        {
            public MonoBehaviour Owner;           // 宿主组件 / Owner MonoBehaviour
            public int OwnerId;                   // 宿主实例ID / Owner instanceID (stable during play session)
            public string GameObjectName;         // GameObject 名称 / GameObject name
            public string OwnerTypeName;          // 宿主类型名 / Script type name
            public string MemberName;             // 成员名（字段/属性）/ Member name (field/property)
            public string MachinePretty;          // 友好机器名 / Pretty generic name (e.g., KStateMachine<Player>)
            public string CurrentState;           // 当前状态名 / Current state type name
            public DateTime LastChange;           // 最近一次状态变更时间 / Last change timestamp
            public readonly List<(DateTime time, string from, string to)> History = new(); // 历史 / History list
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // 状态 & 配置 / State & Configuration
        // ────────────────────────────────────────────────────────────────────────────────
        private Vector2 _scroll;
        private string _search = "";
        private bool _autoRefresh = true;
        private double _lastRepaint;

        // 持久缓存：以 (OwnerId, MemberName) 唯一键保存条目，避免每帧丢历史
        // Persistent cache keyed by (OwnerId, MemberName) to keep history across scans.
        private readonly Dictionary<(int ownerId, string member), RuntimeStateMachineInfo> _machines = new();

        // 历史折叠开关 / Foldout states for histories
        private readonly Dictionary<(int ownerId, string member), bool> _historyFoldouts = new();

        [MenuItem("KToolkit/State Machine Runtime Debugger")]
        public static void ShowWindow()
        {
            GetWindow<KStateMachineRuntimeDebugger>("State Machine Debugger");
        }

        private void OnEnable() => EditorApplication.update += EditorTick;
        private void OnDisable() => EditorApplication.update -= EditorTick;

        private void EditorTick()
        {
            if (!Application.isPlaying) return;
            if (_autoRefresh && EditorApplication.timeSinceStartup - _lastRepaint > 0.5f)
            {
                // 每次自动重绘前进行一次增量扫描 / Incremental scan before repaint
                ScanAndUpdateMachines();
                Repaint();
                _lastRepaint = EditorApplication.timeSinceStartup;
            }
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to debug runtime state machines. / 进入播放模式以调试运行时状态机。", MessageType.Info);
                return;
            }

            // 在手动刷新或第一次绘制时，也执行一次扫描 / also scan on manual refresh or first draw
            if (!_autoRefresh && Event.current.type == EventType.Layout)
                ScanAndUpdateMachines();

            if (_machines.Count == 0)
            {
                EditorGUILayout.HelpBox("No active KStateMachine<> detected. / 未检测到活动的 KStateMachine<>。", MessageType.Info);
                return;
            }

            // 排序展示（先按GO名，再按脚本，再按成员）/ Sort for stable view
            var ordered = _machines.Values
                .OrderBy(m => m.GameObjectName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(m => m.OwnerTypeName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(m => m.MemberName, StringComparer.OrdinalIgnoreCase);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var info in ordered)
            {
                if (!PassFilter(info, _search)) continue;
                DrawMachineCard(info);
            }
            EditorGUILayout.EndScrollView();
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // Toolbar / 工具栏
        // ────────────────────────────────────────────────────────────────────────────────
        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label("Search:", GUILayout.Width(50));
            _search = GUILayout.TextField(_search, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));

            _autoRefresh = GUILayout.Toggle(_autoRefresh, new GUIContent("Auto Refresh / 自动刷新"), EditorStyles.toolbarButton, GUILayout.Width(140));

            if (GUILayout.Button("Manual Refresh / 手动刷新", EditorStyles.toolbarButton, GUILayout.Width(150)))
            {
                ScanAndUpdateMachines();
                Repaint();
            }

            if (GUILayout.Button("Clear All History / 清除所有历史", EditorStyles.toolbarButton, GUILayout.Width(170)))
            {
                foreach (var m in _machines.Values)
                {
                    m.History.Clear();
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(4);
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // Machine card / 单个状态机卡片
        // ────────────────────────────────────────────────────────────────────────────────
        private void DrawMachineCard(RuntimeStateMachineInfo info)
        {
            EditorGUILayout.BeginVertical("box");

            // Header
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"🎯 {info.GameObjectName}", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Select / 选中对象", EditorStyles.miniButton, GUILayout.Width(100)))
            {
                if (info.Owner) { Selection.activeObject = info.Owner.gameObject; EditorGUIUtility.PingObject(info.Owner.gameObject); }
            }
            if (GUILayout.Button("Open Script / 打开脚本", EditorStyles.miniButton, GUILayout.Width(120)))
            {
                if (info.Owner) { var script = MonoScript.FromMonoBehaviour(info.Owner); if (script) AssetDatabase.OpenAsset(script); }
            }
            EditorGUILayout.EndHorizontal();

            // Meta
            EditorGUILayout.LabelField("Owner Script / 宿主脚本:", info.OwnerTypeName, EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField("Member / 成员:", $"{info.MemberName}   ({info.MachinePretty})", EditorStyles.miniLabel);

            // Current state
            var stateStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = string.Equals(info.CurrentState, "null", StringComparison.Ordinal) ? Color.gray : Color.cyan }
            };
            EditorGUILayout.LabelField("Current State / 当前状态:", info.CurrentState, stateStyle);

            // 最近变化高亮（1秒）/ Highlight for 1s after last change
            if ((DateTime.Now - info.LastChange).TotalSeconds < 1.0)
            {
                var rect = GUILayoutUtility.GetLastRect();
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, rect.height), new Color(1, 1, 0, 0.18f));
            }

            // History foldout
            var key = (info.OwnerId, info.MemberName);
            if (!_historyFoldouts.ContainsKey(key)) _historyFoldouts[key] = false;
            _historyFoldouts[key] = EditorGUILayout.Foldout(_historyFoldouts[key], "📜 State History / 状态切换历史", true);

            if (_historyFoldouts[key])
            {
                if (info.History.Count == 0)
                {
                    GUILayout.Label("   (No transitions yet / 尚无状态变化)", EditorStyles.miniLabel);
                }
                else
                {
                    // 最新在上 / newest first
                    foreach (var (time, from, to) in info.History.AsEnumerable().Reverse())
                    {
                        GUILayout.Label($"   [{time:HH:mm:ss}]  {from} → {to}", EditorStyles.miniLabel);
                    }
                }
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }

        private bool PassFilter(RuntimeStateMachineInfo info, string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return true;
            query = query.ToLowerInvariant();
            return info.GameObjectName.ToLowerInvariant().Contains(query)
                || info.OwnerTypeName.ToLowerInvariant().Contains(query)
                || info.MemberName.ToLowerInvariant().Contains(query)
                || info.MachinePretty.ToLowerInvariant().Contains(query)
                || info.CurrentState.ToLowerInvariant().Contains(query);
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // Incremental scan & update / 增量扫描与更新（保持历史）
        // ────────────────────────────────────────────────────────────────────────────────
        private void ScanAndUpdateMachines()
        {
            var seen = new HashSet<(int ownerId, string member)>();
            var behaviours = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true);

            foreach (var mb in behaviours)
            {
                if (!mb) continue;
                int id = mb.GetInstanceID();

                // 1) Fields
                var fields = mb.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var f in fields)
                {
                    // 跳过自动属性生成的 backing field（避免重复）/ skip compiler-generated backing fields
                    if (f.Name.Contains("k__BackingField")) continue;

                    TryExtractStateName(mb, f.FieldType, () => SafeGetFieldValue(f, mb), out string machinePretty, out string stateName);
                    if (machinePretty == null) continue; // not a KStateMachine<>

                    var key = (id, f.Name);
                    seen.Add(key);

                    UpsertMachineEntry(key, mb, f.Name, machinePretty, stateName);
                }

                // 2) Properties
                var props = mb.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var p in props)
                {
                    if (!p.CanRead || p.GetIndexParameters().Length > 0) continue;

                    TryExtractStateName(mb, p.PropertyType, () => SafeGetPropertyValue(p, mb), out string machinePretty, out string stateName);
                    if (machinePretty == null) continue;

                    var key = (id, p.Name);
                    seen.Add(key);

                    UpsertMachineEntry(key, mb, p.Name, machinePretty, stateName);
                }
            }

            // 清理不再存在的条目（宿主销毁或成员丢失）/ remove stale
            var staleKeys = _machines.Keys.Where(k => !seen.Contains(k)).ToList();
            foreach (var k in staleKeys)
            {
                _machines.Remove(k);
                _historyFoldouts.Remove(k);
            }
        }

        private void UpsertMachineEntry((int ownerId, string member) key, MonoBehaviour mb, string memberName, string machinePretty, string stateName)
        {
            if (_machines.TryGetValue(key, out var info))
            {
                // 已存在：仅更新变化并记录历史 / existing: record change if any
                if (!string.Equals(info.CurrentState, stateName, StringComparison.Ordinal))
                {
                    info.History.Add((DateTime.Now, info.CurrentState ?? "null", stateName ?? "null"));
                    info.CurrentState = stateName;
                    info.LastChange = DateTime.Now;
                }
                else
                {
                    // 状态未变：保持信息，其它基础字段也刷新一下以防GO名变更 / refresh meta (rare)
                    info.GameObjectName = mb ? mb.gameObject.name : info.GameObjectName;
                }
            }
            else
            {
                // 新条目：创建并加入字典 / new item
                info = new RuntimeStateMachineInfo
                {
                    Owner = mb,
                    OwnerId = key.ownerId,
                    GameObjectName = mb ? mb.gameObject.name : "(null)",
                    OwnerTypeName = mb ? mb.GetType().Name : "(null)",
                    MemberName = memberName,
                    MachinePretty = machinePretty,
                    CurrentState = stateName ?? "null",
                    LastChange = DateTime.Now
                };
                _machines.Add(key, info);
            }
        }

        // ────────────────────────────────────────────────────────────────────────────────
        // Helpers / 辅助方法
        // ────────────────────────────────────────────────────────────────────────────────
        private object SafeGetFieldValue(FieldInfo f, object o) { try { return f.GetValue(o); } catch { return null; } }
        private object SafeGetPropertyValue(PropertyInfo p, object o) { try { return p.GetValue(o, null); } catch { return null; } }

        /// <summary>
        /// 从成员值中判定是否为 KStateMachine<>，并读取 currentState 的类型名
        /// Detect KStateMachine<> and read the type name of its `currentState` property.
        /// </summary>
        private void TryExtractStateName(MonoBehaviour owner, Type memberType, Func<object> getter,
                                         out string machinePretty, out string currentStateName)
        {
            machinePretty = null;
            currentStateName = null;

            // 成员不是泛型则不可能为 KStateMachine<> / must be generic
            if (!memberType.IsGenericType) return;

            // 要求泛型定义精确等于 KStateMachine<> / require exact KStateMachine<> definition
            var gdef = memberType.GetGenericTypeDefinition();
            if (gdef != typeof(KStateMachine<>)) return;

            var value = getter();
            if (value == null) return;

            // 读取 public currentState 属性 / read public currentState
            try
            {
                var prop = value.GetType().GetProperty("currentState", BindingFlags.Instance | BindingFlags.Public);
                object state = prop?.GetValue(value, null);
                currentStateName = state != null ? state.GetType().Name : "null";
            }
            catch { currentStateName = "null"; }

            // 生成友好泛型名 / pretty generic name
            try
            {
                var arg = memberType.GetGenericArguments().FirstOrDefault();
                var argName = arg != null ? arg.Name : "?";
                machinePretty = $"KStateMachine<{argName}>";
            }
            catch
            {
                machinePretty = "KStateMachine<?>"; // fallback
            }
        }
    }
}
