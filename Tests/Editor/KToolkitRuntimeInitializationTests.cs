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
        }

        [Test]
        public void SubsystemRegistrationClearsStaticRuntimeStateAndDiscoveredSingletons()
        {
            _ = new TestObserver();
            KUIManager.UI_INFO_MAP[typeof(TestPage)] =
                new KUI_Info("test_page", "TestPage", KUIRenderMode.Screen);
            KUIManager.KUI_CELL_INFO_MAP[typeof(TestCell)] =
                new KUI_Cell_Info("test_cell", "TestCell");
            KDebugLogger.debuggerConfig.Clear();
            KDebugLogger.debuggerConfig["Example"] = false;

            _ = KTimerManager.instance;
            _ = KFrameworkManager.instance;
            _ = KAudioManager.instance;
            _ = DiscoveredNoMonoSingleton.instance;
            _ = DiscoveredMonoSingleton.instance;

            Assert.That(KEventManager.DebugGetKObserverCount(), Is.GreaterThan(0));
            Assert.That(KUIManager.UI_INFO_MAP, Is.Not.Empty);
            Assert.That(KUIManager.KUI_CELL_INFO_MAP, Is.Not.Empty);
            Assert.That(KDebugLogger.debuggerConfig["Example"], Is.False);
            Assert.That(GetSingletonNoMonoInstance(typeof(KTimerManager)), Is.Not.Null);
            Assert.That(GetSingletonInstance(typeof(KFrameworkManager)), Is.Not.Null);
            Assert.That(GetSingletonNoMonoInstance(typeof(KAudioManager)), Is.Not.Null);
            Assert.That(GetSingletonNoMonoInstance(typeof(DiscoveredNoMonoSingleton)), Is.Not.Null);
            Assert.That(GetSingletonInstance(typeof(DiscoveredMonoSingleton)), Is.Not.Null);

            InvokeSubsystemRegistrationInitializer();

            Assert.That(KEventManager.DebugGetKObserverCount(), Is.Zero);
            Assert.That(KUIManager.UI_INFO_MAP, Is.Empty);
            Assert.That(KUIManager.KUI_CELL_INFO_MAP, Is.Empty);
            Assert.That(KDebugLogger.debuggerConfig["Example"], Is.True);
            Assert.That(KDebugLogger.debuggerConfig["Cortex"], Is.True);
            Assert.That(GetSingletonNoMonoInstance(typeof(KTimerManager)), Is.Null);
            Assert.That(GetSingletonInstance(typeof(KFrameworkManager)), Is.Null);
            Assert.That(GetSingletonNoMonoInstance(typeof(KAudioManager)), Is.Null);
            Assert.That(GetSingletonNoMonoInstance(typeof(DiscoveredNoMonoSingleton)), Is.Null);
            Assert.That(GetSingletonInstance(typeof(DiscoveredMonoSingleton)), Is.Null);
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

        private static void DestroyObjectIfExists(string objectName)
        {
            var obj = GameObject.Find(objectName);
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
            }
        }

        private sealed class TestObserver : KObserverNoMono
        {
            public TestObserver()
            {
                AddEventListener(KEventName.TestEvent, Noop);
            }

            private static void Noop(object[] args)
            {
            }
        }

        private sealed class TestPage : KUIBase
        {
        }

        private sealed class TestCell : KUICell
        {
            public override void OnCreate(params object[] args)
            {
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
