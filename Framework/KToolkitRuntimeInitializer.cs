using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KToolkit
{
    internal static class KToolkitRuntimeInitializer
    {
        private static readonly HashSet<string> AssemblyNameWhitelist =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "KToolkit",
                "KToolkit.Editor",
                "Assembly-CSharp",
                "Assembly-CSharp-firstpass",
                "Assembly-CSharp-Editor",
                "Assembly-CSharp-Editor-firstpass",
            };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRuntimeState()
        {
            // 新增静态状态必须 either 加 [RuntimeInitializeOnLoadMethod] reset，either 走 KSingleton
#if UNITY_EDITOR
            ResetSingletonInstances(typeof(KSingleton<>));
            ResetSingletonInstances(typeof(KSingletonNoMono<>));
#endif
        }

        private static void ResetSingletonInstances(Type openSingletonType)
        {
            foreach (var type in GetAllTypes())
            {
                if (type == null || type.IsAbstract || type.ContainsGenericParameters)
                {
                    continue;
                }

                Type closedSingletonType = FindClosedGenericBase(type, openSingletonType);
                if (closedSingletonType == null)
                {
                    continue;
                }

                MethodInfo resetMethod = closedSingletonType.GetMethod(
                    "ResetInstance",
                    BindingFlags.Static | BindingFlags.NonPublic);
                resetMethod?.Invoke(null, null);
            }
        }

        private static Type FindClosedGenericBase(Type type, Type openGenericBase)
        {
            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == openGenericBase)
                {
                    return type;
                }

                type = type.BaseType;
            }

            return null;
        }

        private static IEnumerable<Type> GetAllTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(ShouldScanAssembly)
                .SelectMany(GetTypesSafely);
        }

        private static bool ShouldScanAssembly(Assembly assembly)
        {
            return assembly != null && ShouldScanAssemblyName(assembly.GetName().Name);
        }

        private static bool ShouldScanAssemblyName(string assemblyName)
        {
            return assemblyName != null && AssemblyNameWhitelist.Contains(assemblyName);
        }

        private static IEnumerable<Type> GetTypesSafely(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(type => type != null);
            }
        }
    }
}
