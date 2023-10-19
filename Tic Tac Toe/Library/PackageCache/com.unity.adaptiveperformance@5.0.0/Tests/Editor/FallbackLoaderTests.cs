using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AdaptivePerformance;
using UnityEngine.AdaptivePerformance.Provider;
using UnityEngine.AdaptivePerformance.Tests.Standalone;
using UnityEngine.TestTools;

namespace UnityEditor.AdaptivePerformance.Editor.Tests
{
    public class FallbackLoaderTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            FailToLoadSubsystem.RegisterDescriptor();

            AdaptivePerformanceSubsystemDescriptor.RegisterDescriptor(new AdaptivePerformanceSubsystemDescriptor.Cinfo
            {
                id = "Standalone Subsystem",
                providerType = typeof(StandaloneSubsystem.StandaloneProvider),
                subsystemTypeOverride = typeof(StandaloneSubsystem)
            });
        }

        AdaptivePerformanceManagerSettings m_TestManager;
        FailToLoadLoader m_FailToLoadLoader;
        StandaloneLoader m_StandaloneLoader;

        [SetUp]
        public void SetUp()
        {
            m_TestManager = ScriptableObject.CreateInstance<AdaptivePerformanceManagerSettings>();

            m_FailToLoadLoader = ScriptableObject.CreateInstance<FailToLoadLoader>();
            m_TestManager.loaders.Add(m_FailToLoadLoader);

            m_StandaloneLoader = ScriptableObject.CreateInstance<StandaloneLoader>();
            m_TestManager.loaders.Add(m_StandaloneLoader);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(m_StandaloneLoader);
            m_StandaloneLoader = null;
        }

        [Test]
        public void FallbackToStandaloneTest()
        {
            m_TestManager.InitializeLoaderSync();

            Assert.AreEqual(2, m_TestManager.loaders.Count);
            Assert.IsTrue(m_TestManager.loaders[0] is FailToLoadLoader);
            Assert.IsNotNull(m_TestManager.activeLoader);
            Assert.IsTrue(m_TestManager.activeLoader is StandaloneLoader);
        }

        [UnityTest]
        public IEnumerator FallbackToStandaloneAsyncTest()
        {
            yield return m_TestManager.InitializeLoader();

            Assert.AreEqual(2, m_TestManager.loaders.Count);
            Assert.IsTrue(m_TestManager.loaders[0] is FailToLoadLoader);
            Assert.IsNotNull(m_TestManager.activeLoader);
            Assert.IsTrue(m_TestManager.activeLoader is StandaloneLoader);
        }
    }

    [Serializable]
    public class FailToLoadSettings : IAdaptivePerformanceSettings
    {
        static FailToLoadSettings s_Settings = null;

        public static FailToLoadSettings GetSettings()
        {
            if (s_Settings != null)
                return s_Settings;

            s_Settings = CreateInstance<FailToLoadSettings>();
            return s_Settings;
        }
    }

    public class FailToLoadLoader : AdaptivePerformanceLoaderHelper
    {
        static List<AdaptivePerformanceSubsystemDescriptor> s_FailToLoadSubsystemDescriptors =
            new List<AdaptivePerformanceSubsystemDescriptor>();

        public override bool Initialized => false;
        public override bool Running => false;

        public FailToLoadSubsystem failToLoadSubsystem
        {
            get { return GetLoadedSubsystem<FailToLoadSubsystem>(); }
        }

        void OnEnable()
        {
            name = "Bug Fix Loader";
        }

        public override ISubsystem GetDefaultSubsystem()
        {
            return failToLoadSubsystem;
        }

        public override IAdaptivePerformanceSettings GetSettings()
        {
            return FailToLoadSettings.GetSettings();
        }

        public override bool Initialize()
        {
            CreateSubsystem<AdaptivePerformanceSubsystemDescriptor, FailToLoadSubsystem>(s_FailToLoadSubsystemDescriptors, "FailToLoad");

            // when the TryInitialize method on the Provider returns false, no subsystem is created
            // and the subsystem will be null, resulting a false value returned here
            return failToLoadSubsystem != null;
        }

        public override bool Start()
        {
            StartSubsystem<FailToLoadSubsystem>();
            return true;
        }

        public override bool Stop()
        {
            StopSubsystem<FailToLoadSubsystem>();
            return true;
        }

        public override bool Deinitialize()
        {
            DestroySubsystem<FailToLoadSubsystem>();
            return true;
        }
    }

    public class FailToLoadSubsystem : AdaptivePerformanceSubsystem
    {
        public class FailToLoadProvider : APProvider, IApplicationLifecycle, IDevicePerformanceLevelControl
        {
            PerformanceDataRecord m_Data = new PerformanceDataRecord();

            public override Feature Capabilities { get; set; }
            public override IApplicationLifecycle ApplicationLifecycle => this;
            public override IDevicePerformanceLevelControl PerformanceLevelControl => this;
            public override Version Version => new Version(0, 0, 0);
            public override bool Initialized { get; set; }

            public int MaxCpuPerformanceLevel { get; set; }
            public int MaxGpuPerformanceLevel { get; set; }

            protected override bool TryInitialize()
            {
                // return false to cause this loader to fail initialization and a fallback to the next loader should occur
                return false;
            }

            public override void Start() { }

            public override void Stop() { }

            public override void Destroy() { }

            public override PerformanceDataRecord Update()
            {
                return m_Data;
            }

            public void ApplicationPause() { }

            public void ApplicationResume() { }

            public bool SetPerformanceLevel(ref int cpu, ref int gpu)
            {
                return false;
            }

            public bool EnableCpuBoost()
            {
                return false;
            }

            public bool EnableGpuBoost()
            {
                return false;
            }
        }

        public static AdaptivePerformanceSubsystemDescriptor RegisterDescriptor()
        {
            return AdaptivePerformanceSubsystemDescriptor.RegisterDescriptor(new AdaptivePerformanceSubsystemDescriptor.Cinfo
            {
                id = "FailToLoad",
                providerType = typeof(FailToLoadProvider),
                subsystemTypeOverride = typeof(FailToLoadSubsystem)
            });
        }
    }
}
