using System;
using UnityEngine;
using UnityEditor;
using System.IO;


namespace KToolkit
{
    public class StateMachineEditorWindow : EditorWindow
    {
        private KStateMachineData data;
        private Vector2 scrollPos;
        
        [MenuItem("KToolkit/State Machine Generator")]
        public static void ShowWindow()
        {
            GetWindow<StateMachineEditorWindow>("State Machine Generator");
        }

        private void OnGUI()
        {
            GUILayout.Space(5);
            data = (KStateMachineData)EditorGUILayout.ObjectField("State Machine Data", data, typeof(KStateMachineData), false);

            if (data is null)
            {
                EditorGUILayout.HelpBox("请先选择或创建一个 StateMachineData 资源", MessageType.Info);
                return;
            }

            GUILayout.Space(10);

            if (GUILayout.Button("添加状态"))
            {
                AddState();
            }

            if (GUILayout.Button("生成全部状态类"))
            {
                GenerateAllStates();
            }

            GUILayout.Space(10);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            DrawStateList();

            EditorGUILayout.EndScrollView();
        }

        private void AddState()
        {
            data.states.Add(new KStateMachineData.StateNode
            {
                stateName = $"NewState_{data.states.Count}",
                ownerTypeName = "MonoBehaviour",
                position = new Vector2(100, 100 + data.states.Count * 60)
            });
            EditorUtility.SetDirty(data);
        }

        private void DrawStateList()
        {
            for (int i = 0; i < data.states.Count; i++)
            {
                var node = data.states[i];
                GUILayout.BeginVertical("box");
                node.stateName = EditorGUILayout.TextField("state name", node.stateName);
                node.ownerTypeName = EditorGUILayout.TextField("owner type", node.ownerTypeName);
                
                GUILayout.Space(10);

                EditorGUILayout.LabelField("generated directory", EditorStyles.boldLabel);
                node.generateLocation = EditorGUILayout.TextField(node.generateLocation);
                
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("directory", GUILayout.Width(70)))
                {
                    string absPath = EditorUtility.OpenFolderPanel("select your code path", Application.dataPath, "");
                    if (!string.IsNullOrEmpty(absPath))
                    {
                        if (absPath.StartsWith(Application.dataPath))
                        {
                            node.generateLocation = absPath.Substring(Application.dataPath.Length + 1); // 转为相对路径
                            node.generateLocation = "Assets/" + node.generateLocation;
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("ERROR", "Please select the path in Assets directory！", "OK");
                        }
                    }
                }
                if (GUILayout.Button("generate", GUILayout.Width(70)))
                {
                    GenerateState(node);
                }
                if (GUILayout.Button("delete", GUILayout.Width(60)))
                {
                    data.states.RemoveAt(i);
                    EditorUtility.SetDirty(data);
                    break;
                }
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }
        }

        private void GenerateAllStates()
        {
            foreach (var node in data.states)
            {
                GenerateState(node);
            }
            AssetDatabase.Refresh();
        }

        private void GenerateState(KStateMachineData.StateNode node)
        {
            if (!Directory.Exists(node.generateLocation))
                Directory.CreateDirectory(node.generateLocation);

            string className = node.stateName;
            string ownerType = node.ownerTypeName;
            string filePath = Path.Combine(node.generateLocation, $"{className}.cs");

            if (File.Exists(filePath))
            {
                Debug.LogWarning($"⚠️ {className}.cs 已于Assets/{node.generateLocation}存在，跳过生成。");
                return;
            }

            string code = $@"// KToolkit generated in {DateTime.Now}
using UnityEngine;
using KToolkit;

public class {className} : KIBaseState<{ownerType}>
{{
    public void EnterState({ownerType} owner, params object[] args)
    {{
        // 状态进入逻辑
    }}

    public void HandleFixedUpdate({ownerType} owner)
    {{
        // 物理帧逻辑
    }}

    public void HandleUpdate({ownerType} owner)
    {{
        // 普通帧逻辑
    }}

    public void ExitState({ownerType} owner)
    {{
        // 状态退出逻辑
    }}

    public void HandleCollide2D({ownerType} owner, Collision2D collision)
    {{
        // 碰撞响应
    }}

    public void HandleTrigger2D({ownerType} owner, Collider2D collider)
    {{
        // 触发响应
    }}
}}";

            File.WriteAllText(filePath, code);
            Debug.Log($"✅ 已生成状态类: {filePath}");
        }
    }

}
