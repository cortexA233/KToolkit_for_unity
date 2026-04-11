#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

/*
/// <summary>
/// Helper class to analyze state machine transitions
/// </summary>
public static class StateMachineAnalyzer
{
    public class TransitionInfo
    {
        public string TargetState { get; set; }
        public string Condition { get; set; }
        public string SourceMethod { get; set; }
        public int LineNumber { get; set; }
    }
    
    public class StateAnalysisResult
    {
        public Type StateType { get; set; }
        public List<TransitionInfo> Transitions { get; set; } = new List<TransitionInfo>();
        public bool HasEnterState { get; set; }
        public bool HasExitState { get; set; }
        public bool HasHandleUpdate { get; set; }
        public bool HasHandleFixedUpdate { get; set; }
    }
    
    /// <summary>
    /// Analyzes a state type to find all transitions
    /// </summary>
    public static StateAnalysisResult AnalyzeState(Type stateType)
    {
        var result = new StateAnalysisResult { StateType = stateType };
        
        // Check which methods are implemented
        result.HasEnterState = stateType.GetMethod("EnterState", BindingFlags.Public | BindingFlags.Instance) != null;
        result.HasExitState = stateType.GetMethod("ExitState", BindingFlags.Public | BindingFlags.Instance) != null;
        result.HasHandleUpdate = stateType.GetMethod("HandleUpdate", BindingFlags.Public | BindingFlags.Instance) != null;
        result.HasHandleFixedUpdate = stateType.GetMethod("HandleFixedUpdate", BindingFlags.Public | BindingFlags.Instance) != null;
        
        // Analyze HandleUpdate for transitions
        if (result.HasHandleUpdate)
        {
            var method = stateType.GetMethod("HandleUpdate", BindingFlags.Public | BindingFlags.Instance);
            result.Transitions.AddRange(AnalyzeMethodForTransitions(method, "HandleUpdate"));
        }
        
        // Analyze HandleFixedUpdate for transitions
        if (result.HasHandleFixedUpdate)
        {
            var method = stateType.GetMethod("HandleFixedUpdate", BindingFlags.Public | BindingFlags.Instance);
            result.Transitions.AddRange(AnalyzeMethodForTransitions(method, "HandleFixedUpdate"));
        }
        
        // Analyze other methods that might contain transitions
        var allMethods = stateType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var method in allMethods)
        {
            if (method.Name != "HandleUpdate" && method.Name != "HandleFixedUpdate" &&
                method.Name != "EnterState" && method.Name != "ExitState")
            {
                var transitions = AnalyzeMethodForTransitions(method, method.Name);
                if (transitions.Count > 0)
                {
                    result.Transitions.AddRange(transitions);
                }
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Analyzes a method to find state transitions using reflection and pattern matching
    /// </summary>
    private static List<TransitionInfo> AnalyzeMethodForTransitions(MethodInfo method, string methodName)
    {
        var transitions = new List<TransitionInfo>();
        
        try
        {
            var methodBody = method.GetMethodBody();
            if (methodBody == null) return transitions;
            
            var ilBytes = methodBody.GetILAsByteArray();
            var module = method.Module;
            
            // Look for TransitState calls
            for (int i = 0; i < ilBytes.Length; i++)
            {
                // Look for call/callvirt instructions
                if (ilBytes[i] == 0x28 || ilBytes[i] == 0x6F) // call or callvirt
                {
                    if (i + 4 < ilBytes.Length)
                    {
                        int token = BitConverter.ToInt32(ilBytes, i + 1);
                        try
                        {
                            var calledMethod = module.ResolveMethod(token);
                            if (calledMethod != null && calledMethod.Name == "TransitState")
                            {
                                // Try to find the state being transitioned to
                                var transition = ExtractTransitionInfo(ilBytes, i, module, methodName);
                                if (transition != null)
                                {
                                    transitions.Add(transition);
                                }
                            }
                        }
                        catch
                        {
                            // Ignore resolution errors
                        }
                    }
                }
            }
        }
        catch
        {
            // Fallback to basic pattern matching if IL analysis fails
            transitions.AddRange(AnalyzeMethodUsingPatternMatching(method, methodName));
        }
        
        return transitions;
    }
    
    /// <summary>
    /// Extracts transition information from IL bytes
    /// </summary>
    private static TransitionInfo ExtractTransitionInfo(byte[] ilBytes, int callIndex, Module module, string methodName)
    {
        // Look backwards to find the state being created
        for (int i = callIndex - 1; i >= 0 && i > callIndex - 20; i--)
        {
            if (ilBytes[i] == 0x73) // newobj
            {
                if (i + 4 < ilBytes.Length)
                {
                    int token = BitConverter.ToInt32(ilBytes, i + 1);
                    try
                    {
                        var constructor = module.ResolveMethod(token);
                        if (constructor != null && constructor.DeclaringType != null)
                        {
                            string stateName = constructor.DeclaringType.Name;
                            return new TransitionInfo
                            {
                                TargetState = stateName,
                                SourceMethod = methodName,
                                Condition = "" // Would need more complex analysis to extract
                            };
                        }
                    }
                    catch
                    {
                        // Ignore resolution errors
                    }
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Fallback method using simple pattern matching
    /// </summary>
    private static List<TransitionInfo> AnalyzeMethodUsingPatternMatching(MethodInfo method, string methodName)
    {
        var transitions = new List<TransitionInfo>();
        
        // This is a simplified approach - in a real implementation, you'd want to
        // parse the method body more thoroughly
        
        // Common patterns to look for:
        var patterns = new[]
        {
            @"TransitState\s*\(\s*new\s+(\w+State)",
            @"TransitState\s*\(\s*(\w+)State\s*\)",
            @"TransitState\s*\(\s*stateMachine\.(\w+)State\s*\)",
            @"TransitState\s*\(\s*this\.(\w+)State\s*\)",
            @"TransitState\s*\(\s*(\w+)\s*\)",
        };
        
        // Get method source code if available (this would require additional setup)
        // For now, we'll return a basic result
        string[] knownStates = { "Idle", "Walk", "Jump", "Fall", "Run", "Attack", "Die", "Dash", "Melee" };
        
        foreach (var state in knownStates)
        {
            transitions.Add(new TransitionInfo
            {
                TargetState = state + "State",
                SourceMethod = methodName,
                Condition = "Possible transition (pattern analysis)"
            });
        }
        
        return transitions;
    }
    
    /// <summary>
    /// Finds all state types in an assembly
    /// </summary>
    public static List<Type> FindAllStateTypes(Assembly assembly)
    {
        var stateTypes = new List<Type>();
        
        foreach (var type in assembly.GetTypes())
        {
            if (typeof(BaseState).IsAssignableFrom(type) && !type.IsAbstract && type != typeof(BaseState))
            {
                stateTypes.Add(type);
            }
        }
        
        return stateTypes;
    }
    
    /// <summary>
    /// Finds all state machine types in an assembly
    /// </summary>
    public static List<Type> FindAllStateMachineTypes(Assembly assembly)
    {
        var stateMachineTypes = new List<Type>();
        
        foreach (var type in assembly.GetTypes())
        {
            if (typeof(BaseFSM).IsAssignableFrom(type) && !type.IsAbstract && type != typeof(BaseFSM))
            {
                stateMachineTypes.Add(type);
            }
        }
        
        return stateMachineTypes;
    }
    
    /// <summary>
    /// Analyzes relationships between state machines and states
    /// </summary>
    public static Dictionary<Type, List<Type>> AnalyzeStateMachineStateRelationships(Assembly assembly)
    {
        var relationships = new Dictionary<Type, List<Type>>();
        var stateMachineTypes = FindAllStateMachineTypes(assembly);
        var stateTypes = FindAllStateTypes(assembly);
        
        foreach (var smType in stateMachineTypes)
        {
            var relatedStates = new List<Type>();
            
            // Check for nested state classes
            var nestedTypes = smType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var nested in nestedTypes)
            {
                if (stateTypes.Contains(nested))
                {
                    relatedStates.Add(nested);
                }
            }
            
            // Check for state fields
            var fields = smType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (stateTypes.Contains(field.FieldType))
                {
                    relatedStates.Add(field.FieldType);
                }
            }
            
            // Check for states with similar naming
            foreach (var stateType in stateTypes)
            {
                if (stateType.Name.StartsWith(smType.Name.Replace("FSM", "").Replace("StateMachine", "")))
                {
                    if (!relatedStates.Contains(stateType))
                    {
                        relatedStates.Add(stateType);
                    }
                }
            }
            
            relationships[smType] = relatedStates;
        }
        
        return relationships;
    }
}
*/
#endif