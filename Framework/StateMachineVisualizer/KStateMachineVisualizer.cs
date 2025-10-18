using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace KToolkit
{
    public class KStateMachineVisualizer : EditorWindow
    {
        // ===============================
        // 数据结构 / Data Structures
        // ===============================
        private class StateTransition
        {
            public string TargetState;   // 目标状态类名 / Target state class name
            public string SourceFile;    // 源文件路径 / Source file path
            public int LineNumber;       // 行号 / Line number
        }

        private class StateClassInfo
        {
            public string ClassName;             // 状态类名 / State class name
            public string OwnerType;             // 宿主类型 / Owner type
            public string FilePath;              // 文件路径 / File path
            public List<StateTransition> Transitions = new(); // 状态转移列表 / Transition list
        }

        // ===============================
        // 字段 / Fields
        // ===============================
        private Dictionary<string, List<StateClassInfo>> _stateByOwner = new();
        private Vector2 _scrollPos;
        private string _highlightedState = null;
        private string _searchQuery = "";

        // 折叠记忆 / Foldout memory
        private Dictionary<string, bool> _ownerFoldouts = new();
        private Dictionary<string, bool> _classFoldouts = new();

        // 统计数据 / Statistics
        private int _totalOwners;
        private int _totalStates;
        private int _totalTransitions;

        // 图标缓存 / Icon cache
        private Texture2D _scriptIcon;

        // ===============================
        // 菜单入口 / Menu Entry
        // ===============================
        [MenuItem("KToolkit/State Machine Visualizer")]
        public static void ShowWindow()
        {
            GetWindow<KStateMachineVisualizer>("State Machine Visualizer");
        }

        // ===============================
        // 初始化 / Initialization
        // ===============================
        private void OnEnable()
        {
            _scriptIcon = EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D; // 加载脚本图标 / Load built-in script icon
        }

        // ===============================
        // 绘制主界面 / Draw main window
        // ===============================
        private void OnGUI()
        {
            DrawToolbar();
            DrawStatisticsPanel();

            if (_stateByOwner.Count == 0)
            {
                EditorGUILayout.HelpBox("No KIBaseState classes found. Click 'Refresh States' to scan your project.", MessageType.Info);
                return;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            bool hasResult = false;

            foreach (var ownerGroup in _stateByOwner)
            {
                // 过滤宿主类型和状态名 / Filter owners & states
                bool ownerMatch = string.IsNullOrEmpty(_searchQuery) ||
                                  ownerGroup.Key.ToLower().Contains(_searchQuery.ToLower()) ||
                                  ownerGroup.Value.Any(s => s.ClassName.ToLower().Contains(_searchQuery.ToLower()));

                if (!ownerMatch) continue;
                hasResult = true;

                // 宿主折叠 / Owner foldout
                if (!_ownerFoldouts.ContainsKey(ownerGroup.Key))
                    _ownerFoldouts[ownerGroup.Key] = EditorPrefs.GetBool(GetOwnerKey(ownerGroup.Key), true);

                bool newOwnerState = EditorGUILayout.Foldout(
                    _ownerFoldouts[ownerGroup.Key],
                    $"[Owner Type] {ownerGroup.Key}",
                    true,
                    EditorStyles.foldoutHeader
                );

                if (newOwnerState != _ownerFoldouts[ownerGroup.Key])
                {
                    _ownerFoldouts[ownerGroup.Key] = newOwnerState;
                    EditorPrefs.SetBool(GetOwnerKey(ownerGroup.Key), newOwnerState);
                }

                if (_ownerFoldouts[ownerGroup.Key])
                {
                    GUILayout.Space(4);
                    EditorGUI.indentLevel++;

                    foreach (var state in ownerGroup.Value)
                    {
                        if (!string.IsNullOrEmpty(_searchQuery) &&
                            !state.ClassName.ToLower().Contains(_searchQuery.ToLower()) &&
                            !ownerGroup.Key.ToLower().Contains(_searchQuery.ToLower()))
                            continue;

                        DrawStateEntry(state);
                    }

                    EditorGUI.indentLevel--;
                }

                GUILayout.Space(15);
            }

            if (!hasResult)
            {
                GUILayout.Space(10);
                EditorGUILayout.HelpBox("No matching results found.", MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }

        // ===============================
        // 工具栏 / Toolbar
        // ===============================
        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Refresh States", EditorStyles.toolbarButton, GUILayout.Width(120)))
            {
                RefreshStateInfo();
            }

            if (GUILayout.Button("Export Markdown", EditorStyles.toolbarButton, GUILayout.Width(130)))
            {
                ExportMarkdownReport();
            }

            GUILayout.Space(10);
            GUILayout.Label("Search:", GUILayout.Width(50));

            // 搜索栏扩大尺寸 / Widen search bar
            string newQuery = GUILayout.TextField(_searchQuery, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));
            if (newQuery != _searchQuery)
            {
                _searchQuery = newQuery;
            }

            if (GUILayout.Button("✖", EditorStyles.toolbarButton, GUILayout.Width(25)))
            {
                _searchQuery = "";
                GUI.FocusControl(null);
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }

        // ===============================
        // 统计信息面板 / Statistics panel
        // ===============================
        private void DrawStatisticsPanel()
        {
            if (_totalStates == 0 && _totalOwners == 0) return;

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("📊 Statistics / 状态统计信息", EditorStyles.boldLabel);
            GUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button($"Owners: {_totalOwners}", EditorStyles.miniButton, GUILayout.Width(110)))
            {
                ExpandAllStates(false);
                ExpandAllOwners();
            }

            if (GUILayout.Button($"States: {_totalStates}", EditorStyles.miniButton, GUILayout.Width(110)))
            {
                ExpandAllOwners();
                ExpandAllStates();
            }

            if (GUILayout.Button($"Transitions: {_totalTransitions}", EditorStyles.miniButton, GUILayout.Width(130)))
            {
                ExpandAllOwners();
                ExpandAllStates(false);
                foreach (var kvp in _stateByOwner)
                {
                    foreach (var cls in kvp.Value)
                    {
                        if (cls.Transitions.Count > 0)
                        {
                            _classFoldouts[cls.ClassName] = true;
                            EditorPrefs.SetBool(GetClassKey(cls.ClassName), true);
                        }
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            GUILayout.Space(10);
        }

        private void ExpandAllOwners(bool isExpand = true)
        {
            foreach (var key in _ownerFoldouts.Keys.ToList())
            {
                _ownerFoldouts[key] = isExpand;
                EditorPrefs.SetBool(GetOwnerKey(key), isExpand);
            }
        }

        private void ExpandAllStates(bool isExpand = true)
        {
            foreach (var key in _classFoldouts.Keys.ToList())
            {
                _classFoldouts[key] = isExpand;
                EditorPrefs.SetBool(GetClassKey(key), isExpand);
            }
        }

        // ===============================
        // 绘制状态类 / Draw state class
        // ===============================
        private void DrawStateEntry(StateClassInfo state)
        {
            if (!_classFoldouts.ContainsKey(state.ClassName))
                _classFoldouts[state.ClassName] = EditorPrefs.GetBool(GetClassKey(state.ClassName), false);

            GUIStyle foldoutStyle = new(EditorStyles.foldout)
            {
                normal = { textColor = Color.cyan },
                onNormal = { textColor = Color.cyan },
                hover = { textColor = Color.cyan },
                onHover = { textColor = Color.cyan },
                fontStyle = state.ClassName == _highlightedState ? FontStyle.Bold : FontStyle.Normal
            };

            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel++;

            // 状态类名 + 打开脚本按钮（带图标和文字） / State name + "Open Script" button
            EditorGUILayout.BeginHorizontal();
            bool newFoldout = EditorGUILayout.Foldout(
                _classFoldouts[state.ClassName],
                state.ClassName,
                true,
                foldoutStyle
            );

            EditorGUILayout.EndHorizontal();

            if (newFoldout != _classFoldouts[state.ClassName])
            {
                _classFoldouts[state.ClassName] = newFoldout;
                EditorPrefs.SetBool(GetClassKey(state.ClassName), newFoldout);
            }

            if (_classFoldouts[state.ClassName])
            {
                EditorGUI.indentLevel++;
                if (state.Transitions.Count == 0)
                {
                    GUILayout.Space(2);
                    EditorGUILayout.LabelField("    (No transitions / 无状态转移)", EditorStyles.miniLabel);
                }
                else
                {
                    foreach (var trans in state.Transitions)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(45);
                        EditorGUILayout.LabelField("→", GUILayout.Width(15));

                        GUIStyle targetStyle = new(EditorStyles.label)
                        {
                            normal = { textColor = Color.green }
                        };

                        if (GUILayout.Button(trans.TargetState, targetStyle))
                        {
                            _highlightedState = trans.TargetState;
                            OpenScriptAtLine(trans.SourceFile, trans.LineNumber);
                        }

                        if (GUILayout.Button($"(line {trans.LineNumber})", EditorStyles.miniLabel, GUILayout.Width(80)))
                        {
                            OpenScriptAtLine(trans.SourceFile, trans.LineNumber);
                        }
                        
                        GUIContent iconContent = new GUIContent(_scriptIcon, "Open Script File / 打开脚本文件");
                        if (GUILayout.Button(new GUIContent(iconContent) { text = $"Open Script at line {trans.LineNumber}" }, EditorStyles.miniButton, GUILayout.Width(210)))
                        {
                            _highlightedState = state.ClassName;
                            OpenScriptAtLine(state.FilePath, trans.LineNumber);
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        // ===============================
        // 导出报告 / Export Markdown report
        // ===============================
        private void ExportMarkdownReport()
        {
            string savePath = Path.Combine(Application.dataPath, "KStateMachineReport.md");
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("# 🎮 KStateMachine Visualizer Report");
            sb.AppendLine();
            sb.AppendLine($"Generated on: **{System.DateTime.Now}**");
            sb.AppendLine();
            sb.AppendLine($"Total Owners: **{_totalOwners}**");
            sb.AppendLine($"Total States: **{_totalStates}**");
            sb.AppendLine($"Total Transitions: **{_totalTransitions}**");
            sb.AppendLine("\n---\n");

            foreach (var ownerGroup in _stateByOwner)
            {
                sb.AppendLine($"## 🧩 Owner Type: {ownerGroup.Key}");
                sb.AppendLine();

                foreach (var state in ownerGroup.Value)
                {
                    sb.AppendLine($"### 🔹 {state.ClassName}");
                    sb.AppendLine($"*File:* `{Path.GetFileName(state.FilePath)}`");

                    if (state.Transitions.Count == 0)
                    {
                        sb.AppendLine("- (No transitions)");
                    }
                    else
                    {
                        foreach (var t in state.Transitions)
                        {
                            sb.AppendLine($"- → **{t.TargetState}** _(line {t.LineNumber})_");
                        }
                    }

                    sb.AppendLine();
                }

                sb.AppendLine();
            }

            File.WriteAllText(savePath, sb.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Export Complete ✅", $"Markdown report saved to:\n{savePath}", "OK");
        }

        // ===============================
        // 刷新状态信息 / Refresh state data
        // ===============================
        private void RefreshStateInfo()
        {
            _stateByOwner.Clear();
            _totalOwners = 0;
            _totalStates = 0;
            _totalTransitions = 0;

            string[] files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            Regex statePattern = new(@"class\s+(\w+)\s*:[A-Za-z ., \s]*KIBaseState\s*<\s*([\w\d_]+)\s*>", RegexOptions.Compiled);
            Regex transitPattern = new(@"TransitState<\s*([\w\d_]+)\s*>", RegexOptions.Compiled);

            foreach (var file in files)
            {
                string[] lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    var stateMatch = statePattern.Match(lines[i]);
                    if (stateMatch.Success)
                    {
                        string className = stateMatch.Groups[1].Value;
                        string ownerType = stateMatch.Groups[2].Value;

                        StateClassInfo stateInfo = new()
                        {
                            ClassName = className,
                            OwnerType = ownerType,
                            FilePath = file
                        };

                        for (int j = i; j < lines.Length; j++)
                        {
                            var transitMatch = transitPattern.Match(lines[j]);
                            if (transitMatch.Success)
                            {
                                stateInfo.Transitions.Add(new StateTransition
                                {
                                    TargetState = transitMatch.Groups[1].Value,
                                    SourceFile = file,
                                    LineNumber = j + 1
                                });
                            }

                            if (j != i && lines[j].Contains("class "))
                                break;
                        }

                        if (!_stateByOwner.ContainsKey(ownerType))
                            _stateByOwner[ownerType] = new List<StateClassInfo>();

                        _stateByOwner[ownerType].Add(stateInfo);
                    }
                }
            }

            _totalOwners = _stateByOwner.Count;
            _totalStates = _stateByOwner.Sum(o => o.Value.Count);
            _totalTransitions = _stateByOwner.Sum(o => o.Value.Sum(s => s.Transitions.Count));

            Debug.Log($"[KStateMachineVisualizer] Found {_totalStates} state classes in {_totalOwners} owners with {_totalTransitions} transitions.");
        }

        // ===============================
        // 打开脚本到指定行 / Open script at line
        // ===============================
        private void OpenScriptAtLine(string filePath, int line)
        {
            string assetPath = "Assets" + filePath.Replace(Application.dataPath, "");
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (obj != null)
                AssetDatabase.OpenAsset(obj, line);
            else
                Debug.LogWarning($"Could not open file: {filePath}");
        }

        // ===============================
        // EditorPrefs键生成 / EditorPrefs key helpers
        // ===============================
        private string GetOwnerKey(string ownerType) => $"KStateMachineVisualizer.Owner.{ownerType}";
        private string GetClassKey(string className) => $"KStateMachineVisualizer.Class.{className}";
    }
}
