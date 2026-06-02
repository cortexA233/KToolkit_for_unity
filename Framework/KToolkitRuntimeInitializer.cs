using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KToolkit
{
    internal static class KToolkitRuntimeInitializer
    {
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
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetTypesSafely);
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
