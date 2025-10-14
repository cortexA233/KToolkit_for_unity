using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using KToolkit;
using UnityEditor;
using UnityEngine;

public class KIStateMachineVisualizer : EditorWindow
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

    [MenuItem("Tools/KI State Machine Visualizer")]
    public static void ShowWindow()
    {
        GetWindow<KIStateMachineVisualizer>("KI State Visualizer");
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
            EditorGUILayout.LabelField($"[Owner Type] {ownerGroup.Key}", EditorStyles.boldLabel);

            foreach (var state in ownerGroup.Value)
            {
                DrawStateEntry(state);
            }

            EditorGUILayout.Space(10);
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawStateEntry(StateClassInfo state)
    {
        GUIStyle stateStyle = new(EditorStyles.label);
        if (state.ClassName == _highlightedState)
        {
            stateStyle.normal.textColor = Color.cyan;
            stateStyle.fontStyle = FontStyle.Bold;
        }

        // 点击状态名：打开对应脚本文件
        if (GUILayout.Button($"  • {state.ClassName}", stateStyle))
        {
            _highlightedState = state.ClassName;
            OpenScriptAtLine(state.FilePath, 1);
        }

        if (state.Transitions.Count == 0)
        {
            EditorGUILayout.LabelField("      (No transitions)", EditorStyles.miniLabel);
            return;
        }

        foreach (var trans in state.Transitions)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(30);
            EditorGUILayout.LabelField("→", GUILayout.Width(20));

            GUIStyle targetStyle = new(EditorStyles.label);
            if (trans.TargetState == _highlightedState)
            {
                targetStyle.normal.textColor = Color.green;
                targetStyle.fontStyle = FontStyle.Bold;
            }

            if (GUILayout.Button(trans.TargetState, targetStyle))
            {
                _highlightedState = trans.TargetState;
                OpenScriptAtLine(trans.SourceFile, trans.LineNumber);
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private void RefreshStateInfo()
    {
        _stateByOwner.Clear();

        string[] files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
        Regex statePattern = new(@"class\s+(\w+)\s*:[A-Za-z ., \s]*KIBaseState\s*<\s*([\w\d_]+)\s*>", RegexOptions.Compiled);
        Regex transitPattern = new(@"TransitState<\s*([\w\d_]+)\s*>", RegexOptions.Compiled);

        foreach (var file in files)
        {
            Debug.Log(file);
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

        Debug.Log($"[KIStateMachineVisualizer] Found {_stateByOwner.Sum(x => x.Value.Count)} state classes.");
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
}
