using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KToolkit.Tests
{
    public class KToolkitRuntimeInitializationTests
    {
        [TearDown]
        public void TearDown()
        {
            DestroyObjectIfExists("KFrameworkManager");
            DestroyObjectIfExists("KTickManager");
            DestroyObjectIfExists("AudioManager");
            DestroyObjectIfExists(nameof(DiscoveredMonoSingleton));
            DestroyObjectIfExists("KCanvas");
            DestroyObjectIfExists("EventSystem");
            ResetSingletonInstance(typeof(DiscoveredNoMonoSingleton), typeof(KSingletonNoMono<>));
            ResetSingletonInstance(typeof(DiscoveredMonoSingleton), typeof(KSingleton<>));
        }

        [Test]
        public void SubsystemRegistrationClearsOnlyWhitelistedAssemblySingletons()
        {
            _ = KTimerManager.instance;
            _ = KFrameworkManager.instance;
            _ = KAudioManager.instance;
            _ = DiscoveredNoMonoSingleton.instance;
            _ = DiscoveredMonoSingleton.instance;

            Assert.That(GetSingletonNoMonoInstance(typeof(KTimerManager)), Is.Not.Null);
            Assert.That(GetSingletonInstance(typeof(KFrameworkManager)), Is.Not.Null);
            Assert.That(GetSingletonNoMonoInstance(typeof(KAudioManager)), Is.Not.Null);
            Assert.That(GetSingletonNoMonoInstance(typeof(DiscoveredNoMonoSingleton)), Is.Not.Null);
            Assert.That(GetSingletonInstance(typeof(DiscoveredMonoSingleton)), Is.Not.Null);

            InvokeSubsystemRegistrationInitializer();

            Assert.That(GetSingletonNoMonoInstance(typeof(KTimerManager)), Is.Null);
            Assert.That(GetSingletonInstance(typeof(KFrameworkManager)), Is.Null);
            Assert.That(GetSingletonNoMonoInstance(typeof(KAudioManager)), Is.Null);
            Assert.That(GetSingletonNoMonoInstance(typeof(DiscoveredNoMonoSingleton)), Is.Not.Null);
            Assert.That(GetSingletonInstance(typeof(DiscoveredMonoSingleton)), Is.Not.Null);
        }

        [Test]
        public void AssemblyWhitelistIncludesKToolkitAndUnityDefaultScriptAssemblies()
        {
            Type initializerType = Type.GetType("KToolkit.KToolkitRuntimeInitializer, KToolkit");
            Assert.That(initializerType, Is.Not.Null);

            MethodInfo shouldScanAssemblyName = initializerType.GetMethod(
                "ShouldScanAssemblyName",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(shouldScanAssemblyName, Is.Not.Null);

            Assert.That(InvokeShouldScanAssemblyName(shouldScanAssemblyName, "KToolkit"), Is.True);
            Assert.That(InvokeShouldScanAssemblyName(shouldScanAssemblyName, "KToolkit.Editor"), Is.True);
            Assert.That(InvokeShouldScanAssemblyName(shouldScanAssemblyName, "Assembly-CSharp"), Is.True);
            Assert.That(InvokeShouldScanAssemblyName(shouldScanAssemblyName, "Assembly-CSharp-firstpass"), Is.True);
            Assert.That(InvokeShouldScanAssemblyName(shouldScanAssemblyName, "Assembly-CSharp-Editor"), Is.True);
            Assert.That(InvokeShouldScanAssemblyName(shouldScanAssemblyName, "Assembly-CSharp-Editor-firstpass"), Is.True);
            Assert.That(InvokeShouldScanAssemblyName(shouldScanAssemblyName, "KToolkit.Tests.Editor"), Is.False);
            Assert.That(InvokeShouldScanAssemblyName(shouldScanAssemblyName, "UnityEngine.CoreModule"), Is.False);
        }

        private static bool InvokeShouldScanAssemblyName(MethodInfo method, string assemblyName)
        {
            return (bool)method.Invoke(null, new object[] { assemblyName });
        }

        private static void InvokeSubsystemRegistrationInitializer()
        {
            var initializerType = Type.GetType("KToolkit.KToolkitRuntimeInitializer, KToolkit");
            Assert.That(initializerType, Is.Not.Null);

            var methods = initializerType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(HasSubsystemRegistrationAttribute)
                .ToArray();

            Assert.That(methods, Is.Not.Empty);

            foreach (var method in methods)
            {
                method.Invoke(null, null);
            }
        }

        private static bool HasSubsystemRegistrationAttribute(MethodInfo method)
        {
            return method.GetCustomAttributes<RuntimeInitializeOnLoadMethodAttribute>()
                .Any(attribute => attribute.loadType == RuntimeInitializeLoadType.SubsystemRegistration);
        }

        private static object GetSingletonInstance(Type singletonType)
        {
            return typeof(KSingleton<>)
                .MakeGenericType(singletonType)
                .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)
                ?.GetValue(null);
        }

        private static object GetSingletonNoMonoInstance(Type singletonType)
        {
            return typeof(KSingletonNoMono<>)
                .MakeGenericType(singletonType)
                .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)
                ?.GetValue(null);
        }

        private static void ResetSingletonInstance(Type singletonType, Type openSingletonType)
        {
            Type closedSingletonType = openSingletonType.MakeGenericType(singletonType);
            MethodInfo resetMethod = closedSingletonType.GetMethod(
                "ResetInstance",
                BindingFlags.Static | BindingFlags.NonPublic);
            resetMethod?.Invoke(null, null);
        }

        private static void DestroyObjectIfExists(string objectName)
        {
            var obj = GameObject.Find(objectName);
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
            }
        }

        public sealed class DiscoveredNoMonoSingleton : KSingletonNoMono<DiscoveredNoMonoSingleton>
        {
        }

        public sealed class DiscoveredMonoSingleton : KSingleton<DiscoveredMonoSingleton>
        {
        }
    }
}
