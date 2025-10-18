using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace KToolkit
{
    public class KStateMachineVisualizer : EditorWindow
    {
        private class StateTransition
        {
            public string TargetState;
            public string SourceFile;
            public int LineNumber;
        }

        private class StateClassInfo
        {
            public string ClassName;
            public string OwnerType;
            public string FilePath;
            public List<StateTransition> Transitions = new();
        }

        private Dictionary<string, List<StateClassInfo>> _stateByOwner = new();
        private Vector2 _scrollPos;
        private string _highlightedState = null;

        // 折叠状态
        private Dictionary<string, bool> _ownerFoldouts = new();
        private Dictionary<string, bool> _classFoldouts = new();

        [MenuItem("KToolkit/State Machine Visualizer")]
        public static void ShowWindow()
        {
            GetWindow<KStateMachineVisualizer>("State Machine Visualizer");
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Refresh States"))
            {
                RefreshStateInfo();
            }

            if (_stateByOwner.Count == 0)
            {
                EditorGUILayout.HelpBox("No KIBaseState classes found. Click 'Refresh States' to scan your project.", MessageType.Info);
                return;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            foreach (var ownerGroup in _stateByOwner)
            {
                // 一级：宿主类型折叠
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
                    GUILayout.Space(5);
                    EditorGUI.indentLevel++;
                    foreach (var state in ownerGroup.Value)
                    {
                        DrawStateEntry(state);
                    }
                    EditorGUI.indentLevel--;
                }

                GUILayout.Space(20);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawStateEntry(StateClassInfo state)
        {
            if (!_classFoldouts.ContainsKey(state.ClassName))
                _classFoldouts[state.ClassName] = EditorPrefs.GetBool(GetClassKey(state.ClassName), false);

            GUIStyle foldoutStyle = new(EditorStyles.foldout);
            foldoutStyle.normal.textColor = Color.cyan;
            foldoutStyle.onNormal.textColor = Color.cyan;
            foldoutStyle.hover.textColor = Color.cyan;
            foldoutStyle.onHover.textColor = Color.cyan;
            foldoutStyle.fontStyle = state.ClassName == _highlightedState ? FontStyle.Bold : FontStyle.Normal;

            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel++;

            bool newFoldout = EditorGUILayout.Foldout(
                _classFoldouts[state.ClassName],
                state.ClassName,
                true,
                foldoutStyle
            );

            if (newFoldout != _classFoldouts[state.ClassName])
            {
                _classFoldouts[state.ClassName] = newFoldout;
                EditorPrefs.SetBool(GetClassKey(state.ClassName), newFoldout);
            }

            if (_classFoldouts[state.ClassName])
            {
                EditorGUI.indentLevel++;
                if (GUILayout.Button("📄 Open Script", EditorStyles.miniButton))
                {
                    _highlightedState = state.ClassName;
                    OpenScriptAtLine(state.FilePath, 1);
                }

                if (state.Transitions.Count == 0)
                {
                    GUILayout.Space(2);
                    EditorGUILayout.LabelField("    (No transitions)", EditorStyles.miniLabel);
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

                        // GUILayout.Space(50);
                        if (GUILayout.Button($"(line {trans.LineNumber})", EditorStyles.miniLabel, GUILayout.Width(80)))
                        {
                            OpenScriptAtLine(trans.SourceFile, trans.LineNumber);
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private void RefreshStateInfo()
        {
            _stateByOwner.Clear();

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
                        KDebugLogger.Cortex_DebugLog("匹配状态成功", file, className, ownerType);

                        StateClassInfo stateInfo = new()
                        {
                            ClassName = className,
                            OwnerType = ownerType,
                            FilePath = file
                        };

                        // 扫描 TransitState 调用
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

                            // 检查是否到达下一个类定义
                            if (j != i && lines[j].Contains("class "))
                                break;
                        }

                        if (!_stateByOwner.ContainsKey(ownerType))
                            _stateByOwner[ownerType] = new List<StateClassInfo>();

                        _stateByOwner[ownerType].Add(stateInfo);
                    }
                }
            }

            Debug.Log($"[StateMachineVisualizer] Found {_stateByOwner.Sum(x => x.Value.Count)} state classes.");
        }

        private void OpenScriptAtLine(string filePath, int line)
        {
            string assetPath = "Assets" + filePath.Replace(Application.dataPath, "");
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (obj != null)
            {
                AssetDatabase.OpenAsset(obj, line);
            }
            else
            {
                Debug.LogWarning($"Could not open file: {filePath}");
            }
        }

        // ==== EditorPrefs key helpers ====
        private string GetOwnerKey(string ownerType) => $"KStateMachineVisualizer.Owner.{ownerType}";
        private string GetClassKey(string className) => $"KStateMachineVisualizer.Class.{className}";
    }
}
