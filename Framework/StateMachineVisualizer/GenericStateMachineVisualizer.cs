#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

public class GenericStateMachineVisualizer : EditorWindow
{
    private MonoBehaviour targetComponent;
    private BaseFSM selectedStateMachine;
    private Texture2D nodeTexture;
    private GUIStyle nodeStyle;
    private GUIStyle currentNodeStyle;
    private Vector2 scrollPosition;
    private Dictionary<string, Vector2> nodePositions = new Dictionary<string, Vector2>();
    private Dictionary<string, StateInfo> discoveredStates = new Dictionary<string, StateInfo>();
    private bool isAnalyzed = false;
    private string currentStateName = "";
    
    // Node appearance settings
    private const float NODE_WIDTH = 140f;
    private const float NODE_HEIGHT = 60f;
    private const float GRID_SIZE = 20f;
    
    // Visualization options
    private bool showFieldInfo = true;
    private bool showMethodInfo = true;
    private bool autoArrange = true;
    private VisualizationLayout layoutType = VisualizationLayout.Circular;
    
    private enum VisualizationLayout
    {
        Circular,
        Grid,
        Hierarchical,
        Force
    }
    
    private class StateInfo
    {
        public string name;
        public Type stateType;
        public List<TransitionInfo> transitions = new List<TransitionInfo>();
        public FieldInfo fieldReference; // If state is stored in a field
        public bool isNestedClass;
    }
    
    private class TransitionInfo
    {
        public string targetState;
        public string condition;
        public string sourceMethod;
    }
    
    [MenuItem("Tools/Generic State Machine Visualizer")]
    public static void ShowWindow()
    {
        GetWindow<GenericStateMachineVisualizer>("State Machine Visualizer");
    }
    
    private void OnEnable()
    {
        CreateStyles();
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
    
    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }
    
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.EnteredEditMode)
        {
            // Re-analyze when play mode changes
            if (targetComponent != null)
            {
                AnalyzeStateMachine();
            }
        }
    }
    
    private void CreateStyles()
    {
        // Create textures for node backgrounds
        nodeTexture = new Texture2D(1, 1);
        nodeTexture.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.4f, 0.9f));
        nodeTexture.Apply();
        
        // Regular node style
        nodeStyle = new GUIStyle();
        nodeStyle.normal.background = nodeTexture;
        nodeStyle.normal.textColor = Color.white;
        nodeStyle.border = new RectOffset(12, 12, 12, 12);
        nodeStyle.padding = new RectOffset(8, 8, 8, 8);
        nodeStyle.alignment = TextAnchor.MiddleCenter;
        nodeStyle.wordWrap = true;
        nodeStyle.fontSize = 11;
        
        // Current state node style (highlighted)
        Texture2D currentNodeTexture = new Texture2D(1, 1);
        currentNodeTexture.SetPixel(0, 0, new Color(0.4f, 0.8f, 0.6f, 0.9f));
        currentNodeTexture.Apply();
        
        currentNodeStyle = new GUIStyle(nodeStyle);
        currentNodeStyle.normal.background = currentNodeTexture;
    }
    
    private void OnGUI()
    {
        // Create a horizontal split layout
        EditorGUILayout.BeginHorizontal();
        
        // Left panel with fixed width
        EditorGUILayout.BeginVertical(GUILayout.Width(300));
        DrawControlPanel();
        EditorGUILayout.EndVertical();
        
        // Add a visual separator
        GUILayout.Box("", GUILayout.Width(2), GUILayout.ExpandHeight(true));
        
        // Right panel for visualization (takes remaining space)
        EditorGUILayout.BeginVertical();
        if (targetComponent != null && selectedStateMachine != null && isAnalyzed)
        {
            DrawStateMachineVisualization();
        }
        else
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox("Select a MonoBehaviour component that contains a state machine.", MessageType.Info);
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawControlPanel()
    {
        // Add scrollview for the control panel if content is too long
        GUILayout.BeginScrollView(Vector2.zero, false, true, GUILayout.ExpandHeight(true));
        
        EditorGUILayout.LabelField("State Machine Visualizer", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Component selection
        EditorGUILayout.LabelField("Target Component", EditorStyles.boldLabel);
        targetComponent = EditorGUILayout.ObjectField("Component", targetComponent, typeof(MonoBehaviour), true) as MonoBehaviour;
        
        if (targetComponent != null)
        {
            // Find all BaseFSM fields and properties
            List<BaseFSM> stateMachines = FindStateMachinesInComponent(targetComponent);
            
            if (stateMachines.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Found State Machines:", EditorStyles.boldLabel);
                
                foreach (var sm in stateMachines)
                {
                    string name = GetStateMachineName(sm, targetComponent);
                    if (GUILayout.Button(name))
                    {
                        selectedStateMachine = sm;
                        AnalyzeStateMachine();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No state machines found in this component.", MessageType.Warning);
            }
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Visualization Options", EditorStyles.boldLabel);
        
        showFieldInfo = EditorGUILayout.Toggle("Show Field Info", showFieldInfo);
        showMethodInfo = EditorGUILayout.Toggle("Show Method Info", showMethodInfo);
        autoArrange = EditorGUILayout.Toggle("Auto Arrange", autoArrange);
        layoutType = (VisualizationLayout)EditorGUILayout.EnumPopup("Layout Type", layoutType);
        
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh"))
        {
            AnalyzeStateMachine();
        }
        
        if (GUILayout.Button("Reset Layout"))
        {
            ArrangeNodes();
        }
        EditorGUILayout.EndHorizontal();
        
        // Current state display
        if (Application.isPlaying && selectedStateMachine != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Current State:", EditorStyles.boldLabel);
            
            if (selectedStateMachine.currentState != null)
            {
                currentStateName = selectedStateMachine.currentState.GetType().Name;
                EditorGUILayout.LabelField(currentStateName);
            }
            else
            {
                EditorGUILayout.LabelField("None");
            }
        }
        
        // State information
        if (isAnalyzed)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Discovered States:", EditorStyles.boldLabel);
            
            foreach (var state in discoveredStates.Values)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(state.name, EditorStyles.miniBoldLabel);
                
                if (state.transitions.Count > 0)
                {
                    EditorGUILayout.LabelField("Transitions:");
                    foreach (var transition in state.transitions)
                    {
                        string info = $"→ {transition.targetState}";
                        if (!string.IsNullOrEmpty(transition.condition))
                        {
                            info += $" (if {transition.condition})";
                        }
                        EditorGUILayout.LabelField(info, EditorStyles.miniLabel);
                    }
                }
                
                EditorGUILayout.EndVertical();
            }
        }
        
        GUILayout.EndScrollView();
    }
    
    private List<BaseFSM> FindStateMachinesInComponent(MonoBehaviour component)
    {
        List<BaseFSM> stateMachines = new List<BaseFSM>();
        Type componentType = component.GetType();
        
        // Check fields
        FieldInfo[] fields = componentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            if (typeof(BaseFSM).IsAssignableFrom(field.FieldType))
            {
                BaseFSM sm = field.GetValue(component) as BaseFSM;
                if (sm != null)
                {
                    stateMachines.Add(sm);
                }
            }
        }
        
        // Check properties
        PropertyInfo[] properties = componentType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            if (typeof(BaseFSM).IsAssignableFrom(prop.PropertyType) && prop.CanRead)
            {
                BaseFSM sm = prop.GetValue(component) as BaseFSM;
                if (sm != null)
                {
                    stateMachines.Add(sm);
                }
            }
        }
        
        // Check if the component itself inherits from BaseFSM (though this is unlikely since BaseFSM doesn't inherit from MonoBehaviour)
        if (typeof(BaseFSM).IsAssignableFrom(component.GetType()))
        {
            // This case is actually impossible since BaseFSM inherits from ObserverNoMono, not MonoBehaviour
            // But keeping it for completeness
        }
        
        return stateMachines;
    }
    
    private string GetStateMachineName(BaseFSM stateMachine, MonoBehaviour component)
    {
        Type componentType = component.GetType();
        
        // Find field name
        foreach (var field in componentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (field.GetValue(component) == stateMachine)
            {
                return $"{field.Name} ({stateMachine.GetType().Name})";
            }
        }
        
        // Find property name
        foreach (var prop in componentType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (prop.CanRead && prop.GetValue(component) == stateMachine)
            {
                return $"{prop.Name} ({stateMachine.GetType().Name})";
            }
        }
        
        return stateMachine.GetType().Name;
    }
    
    private void AnalyzeStateMachine()
    {
        if (selectedStateMachine == null) return;
        
        discoveredStates.Clear();
        Type stateMachineType = selectedStateMachine.GetType();
        
        // Method 1: Find states stored as fields/properties in the state machine
        FieldInfo[] fields = stateMachineType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            if (typeof(BaseState).IsAssignableFrom(field.FieldType))
            {
                StateInfo stateInfo = new StateInfo
                {
                    name = field.FieldType.Name,
                    stateType = field.FieldType,
                    fieldReference = field,
                    isNestedClass = false
                };
                
                AnalyzeStateTransitions(stateInfo);
                discoveredStates[stateInfo.name] = stateInfo;
            }
        }
        
        // Method 2: Find nested state classes
        Type[] nestedTypes = stateMachineType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var nestedType in nestedTypes)
        {
            if (typeof(BaseState).IsAssignableFrom(nestedType))
            {
                StateInfo stateInfo = new StateInfo
                {
                    name = nestedType.Name,
                    stateType = nestedType,
                    isNestedClass = true
                };
                
                AnalyzeStateTransitions(stateInfo);
                discoveredStates[stateInfo.name] = stateInfo;
            }
        }
        
        // Method 3: Find states in the same namespace (for separate files)
        Assembly assembly = stateMachineType.Assembly;
        Type[] allTypes = assembly.GetTypes();
        foreach (var type in allTypes)
        {
            if (typeof(BaseState).IsAssignableFrom(type) && 
                !type.IsAbstract && 
                type != typeof(BaseState) &&
                !discoveredStates.ContainsKey(type.Name))
            {
                StateInfo stateInfo = new StateInfo
                {
                    name = type.Name,
                    stateType = type,
                    isNestedClass = false
                };
                
                AnalyzeStateTransitions(stateInfo);
                discoveredStates[stateInfo.name] = stateInfo;
            }
        }
        
        isAnalyzed = true;
        if (autoArrange)
        {
            ArrangeNodes();
        }
    }
    
    private void AnalyzeStateTransitions(StateInfo stateInfo)
    {
        // Analyze HandleUpdate method for transitions
        MethodInfo handleUpdate = stateInfo.stateType.GetMethod("HandleUpdate", BindingFlags.Public | BindingFlags.Instance);
        if (handleUpdate != null)
        {
            // This is a simplified analysis - for more accuracy, you'd need to parse the IL
            // or use Roslyn for code analysis
            AnalyzeMethodForTransitions(stateInfo, handleUpdate, "HandleUpdate");
        }
        
        // Analyze HandleFixedUpdate
        MethodInfo handleFixedUpdate = stateInfo.stateType.GetMethod("HandleFixedUpdate", BindingFlags.Public | BindingFlags.Instance);
        if (handleFixedUpdate != null)
        {
            AnalyzeMethodForTransitions(stateInfo, handleFixedUpdate, "HandleFixedUpdate");
        }
    }
    
    private void AnalyzeMethodForTransitions(StateInfo stateInfo, MethodInfo method, string methodName)
    {
        // Use the StateMachineAnalyzer for more accurate transition detection
        var analysisResult = StateMachineAnalyzer.AnalyzeState(stateInfo.stateType);
        
        foreach (var transition in analysisResult.Transitions)
        {
            if (transition.SourceMethod == methodName)
            {
                // Only add if we haven't already found this transition
                if (!stateInfo.transitions.Any(t => t.targetState == transition.TargetState && t.sourceMethod == methodName))
                {
                    stateInfo.transitions.Add(new TransitionInfo
                    {
                        targetState = transition.TargetState,
                        sourceMethod = transition.SourceMethod,
                        condition = transition.Condition
                    });
                }
            }
        }
    }
    
    private void ArrangeNodes()
    {
        nodePositions.Clear();
        
        switch (layoutType)
        {
            case VisualizationLayout.Circular:
                ArrangeCircular();
                break;
            case VisualizationLayout.Grid:
                ArrangeGrid();
                break;
            case VisualizationLayout.Hierarchical:
                ArrangeHierarchical();
                break;
            case VisualizationLayout.Force:
                ArrangeForceDirected();
                break;
        }
    }
    
    private void ArrangeCircular()
    {
        float radius = 200f;
        int index = 0;
        int totalStates = discoveredStates.Count;
        
        foreach (var state in discoveredStates.Keys)
        {
            float angle = (index * 2 * Mathf.PI) / totalStates;
            float x = Mathf.Cos(angle) * radius + 400f;
            float y = Mathf.Sin(angle) * radius + 300f;
            
            nodePositions[state] = new Vector2(x, y);
            index++;
        }
    }
    
    private void ArrangeGrid()
    {
        int columns = Mathf.CeilToInt(Mathf.Sqrt(discoveredStates.Count));
        int index = 0;
        float spacing = 200f;
        float startX = 200f;
        float startY = 100f;
        
        foreach (var state in discoveredStates.Keys)
        {
            int row = index / columns;
            int col = index % columns;
            
            float x = startX + col * spacing;
            float y = startY + row * spacing;
            
            nodePositions[state] = new Vector2(x, y);
            index++;
        }
    }
    
    private void ArrangeHierarchical()
    {
        // Simple hierarchical layout based on transition count
        var statesByTransitionCount = discoveredStates.OrderByDescending(s => s.Value.transitions.Count).ToList();
        
        float ySpacing = 150f;
        float xSpacing = 200f;
        float startY = 100f;
        
        int level = 0;
        int nodesInLevel = 0;
        int maxNodesPerLevel = 3;
        
        foreach (var state in statesByTransitionCount)
        {
            float x = 400f + (nodesInLevel - maxNodesPerLevel / 2) * xSpacing;
            float y = startY + level * ySpacing;
            
            nodePositions[state.Key] = new Vector2(x, y);
            
            nodesInLevel++;
            if (nodesInLevel >= maxNodesPerLevel)
            {
                nodesInLevel = 0;
                level++;
            }
        }
    }
    
    private void ArrangeForceDirected()
    {
        // Simple force-directed layout simulation
        if (discoveredStates.Count == 0) return;
        
        // Initialize random positions
        System.Random random = new System.Random();
        foreach (var state in discoveredStates.Keys)
        {
            nodePositions[state] = new Vector2(
                random.Next(200, 600),
                random.Next(100, 500)
            );
        }
        
        // Simulate forces
        for (int iteration = 0; iteration < 50; iteration++)
        {
            Dictionary<string, Vector2> forces = new Dictionary<string, Vector2>();
            
            foreach (var state in discoveredStates.Keys)
            {
                forces[state] = Vector2.zero;
                
                // Repulsion between all nodes
                foreach (var otherState in discoveredStates.Keys)
                {
                    if (state != otherState)
                    {
                        Vector2 diff = nodePositions[state] - nodePositions[otherState];
                        float distance = diff.magnitude;
                        if (distance < 0.1f) distance = 0.1f;
                        
                        forces[state] += diff.normalized * (10000f / (distance * distance));
                    }
                }
                
                // Attraction along transitions
                foreach (var transition in discoveredStates[state].transitions)
                {
                    if (nodePositions.ContainsKey(transition.targetState))
                    {
                        Vector2 diff = nodePositions[transition.targetState] - nodePositions[state];
                        forces[state] += diff * 0.1f;
                    }
                }
                
                // Center attraction
                Vector2 toCenter = new Vector2(400f, 300f) - nodePositions[state];
                forces[state] += toCenter * 0.01f;
            }
            
            // Apply forces
            foreach (var state in discoveredStates.Keys)
            {
                nodePositions[state] += forces[state] * 0.1f;
            }
        }
    }
    
    private void DrawStateMachineVisualization()
    {
        // Calculate visualization area (excluding the control panel)
        float panelWidth = 302; // 300 + 2 for separator
        Rect visualizationArea = new Rect(panelWidth, 0, position.width - panelWidth, position.height);
        
        // Begin scroll view for the visualization area only
        scrollPosition = GUI.BeginScrollView(
            visualizationArea, 
            scrollPosition, 
            new Rect(0, 0, 1000, 800),
            false, // Don't show horizontal scrollbar
            false  // Don't show vertical scrollbar
        );
        
        // Draw grid background
        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);
        
        // Draw connections first
        DrawConnections();
        
        // Draw nodes
        DrawNodes();
        
        // Handle node dragging
        ProcessNodeEvents(Event.current);
        
        GUI.EndScrollView();
        
        // Draw scrollbars manually for better control
        GUI.BeginGroup(visualizationArea);
        
        // Horizontal scrollbar
        if (1000 > visualizationArea.width)
        {
            Rect hScrollRect = new Rect(0, visualizationArea.height - 15, visualizationArea.width - 15, 15);
            scrollPosition.x = GUI.HorizontalScrollbar(hScrollRect, scrollPosition.x, visualizationArea.width, 0, 1000);
        }
        
        // Vertical scrollbar
        if (800 > visualizationArea.height)
        {
            Rect vScrollRect = new Rect(visualizationArea.width - 15, 0, 15, visualizationArea.height - 15);
            scrollPosition.y = GUI.VerticalScrollbar(vScrollRect, scrollPosition.y, visualizationArea.height, 0, 800);
        }
        
        GUI.EndGroup();
        
        // Repaint in play mode
        if (Application.isPlaying)
        {
            Repaint();
        }
    }
    
    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(1000 / gridSpacing);
        int heightDivs = Mathf.CeilToInt(800 / gridSpacing);
        
        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);
        
        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, 0, 0), new Vector3(gridSpacing * i, 800, 0));
        }
        
        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(0, gridSpacing * j, 0), new Vector3(1000, gridSpacing * j, 0));
        }
        
        Handles.color = Color.white;
        Handles.EndGUI();
    }
    
    private void DrawNodes()
    {
        foreach (var state in discoveredStates)
        {
            if (!nodePositions.ContainsKey(state.Key)) continue;
            
            Vector2 position = nodePositions[state.Key];
            Rect nodeRect = new Rect(position.x - NODE_WIDTH / 2, position.y - NODE_HEIGHT / 2, NODE_WIDTH, NODE_HEIGHT);
            
            // Determine node style
            bool isCurrentState = Application.isPlaying && selectedStateMachine != null && 
                selectedStateMachine.currentState != null && 
                selectedStateMachine.currentState.GetType().Name == state.Key;
            
            GUIStyle style = isCurrentState ? currentNodeStyle : nodeStyle;
            
            // Draw node
            GUI.Box(nodeRect, "", style);
            
            // Draw state name
            string displayName = state.Key.Replace("State", "");
            if (displayName.Length > 15)
            {
                displayName = displayName.Substring(0, 12) + "...";
            }
            
            Rect labelRect = new Rect(nodeRect.x, nodeRect.y + 5, nodeRect.width, 20);
            GUI.Label(labelRect, displayName, style);
            
            // Draw additional info
            if (showFieldInfo && state.Value.fieldReference != null)
            {
                Rect fieldRect = new Rect(nodeRect.x, nodeRect.y + 25, nodeRect.width, 20);
                GUI.Label(fieldRect, $"Field: {state.Value.fieldReference.Name}", EditorStyles.miniLabel);
            }
            
            if (showMethodInfo && state.Value.transitions.Count > 0)
            {
                Rect transitionRect = new Rect(nodeRect.x, nodeRect.y + 40, nodeRect.width, 20);
                GUI.Label(transitionRect, $"Transitions: {state.Value.transitions.Count}", EditorStyles.miniLabel);
            }
        }
    }
    
    private void DrawConnections()
    {
        foreach (var state in discoveredStates)
        {
            if (!nodePositions.ContainsKey(state.Key)) continue;
            
            Vector2 startPos = nodePositions[state.Key];
            
            foreach (var transition in state.Value.transitions)
            {
                if (!nodePositions.ContainsKey(transition.targetState)) continue;
                
                Vector2 endPos = nodePositions[transition.targetState];
                
                // Check if this is a self-transition
                bool isSelfTransition = state.Key == transition.targetState;
                
                // Draw the arrow
                DrawArrow(startPos, endPos, isSelfTransition, transition);
            }
        }
    }
    
    private void DrawArrow(Vector2 start, Vector2 end, bool isSelfTransition, TransitionInfo transition)
    {
        Color arrowColor = Color.white;
        
        if (isSelfTransition)
        {
            // Draw a curved arrow for self-transitions
            Vector2 controlPoint1 = start + new Vector2(60, -60);
            Vector2 controlPoint2 = start + new Vector2(60, 60);
            
            Handles.DrawBezier(start, start, controlPoint1, controlPoint2, arrowColor, null, 2f);
            
            // Draw arrowhead
            Vector2 arrowDir = (start - controlPoint2).normalized;
            DrawArrowHead(start, arrowDir, arrowColor);
        }
        else
        {
            // Calculate edge points
            Vector2 dir = (end - start).normalized;
            Vector2 startEdge = start + dir * (NODE_WIDTH / 2);
            Vector2 endEdge = end - dir * (NODE_WIDTH / 2);
            
            // Draw line
            Handles.color = arrowColor;
            Handles.DrawLine(startEdge, endEdge);
            
            // Draw arrowhead
            DrawArrowHead(endEdge, -dir, arrowColor);
            
            // Draw transition info if available
            if (!string.IsNullOrEmpty(transition.condition))
            {
                Vector2 midPoint = (startEdge + endEdge) / 2;
                Handles.Label(midPoint, transition.condition, EditorStyles.miniLabel);
            }
        }
    }
    
    private void DrawArrowHead(Vector2 position, Vector2 direction, Color color)
    {
        Handles.color = color;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);
        Vector2 arrowPoint1 = position - direction * 10 + perpendicular * 5;
        Vector2 arrowPoint2 = position - direction * 10 - perpendicular * 5;
        
        Handles.DrawLine(position, arrowPoint1);
        Handles.DrawLine(position, arrowPoint2);
        Handles.DrawLine(arrowPoint1, arrowPoint2);
    }
    
    private void ProcessNodeEvents(Event e)
    {
        if (e.type == EventType.MouseDrag && e.button == 0)
        {
            foreach (var state in nodePositions.Keys.ToList())
            {
                Vector2 position = nodePositions[state];
                Rect nodeRect = new Rect(position.x - NODE_WIDTH / 2, position.y - NODE_HEIGHT / 2, NODE_WIDTH, NODE_HEIGHT);
                
                if (nodeRect.Contains(e.mousePosition))
                {
                    nodePositions[state] += e.delta;
                    e.Use();
                    break;
                }
            }
        }
    }
}
#endif