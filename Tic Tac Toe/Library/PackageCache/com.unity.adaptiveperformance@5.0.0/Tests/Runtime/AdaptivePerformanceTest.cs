using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor.AdaptivePerformance.Simulator.Editor;
using UnityEngine;
using UnityEngine.AdaptivePerformance;
using UnityEngine.AdaptivePerformance.Provider;
using UnityEngine.TestTools;

namespace UnityEditor.AdaptivePerformance.Tests
{
    public class AdaptivePerformanceSimulation : ManagementTestSetup
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Setup()
        {
            if (!AdaptivePerformanceGeneralSettings.Instance || !AdaptivePerformanceGeneralSettings.Instance.Manager || !AdaptivePerformanceGeneralSettings.Instance.Manager.isInitializationComplete)
                return;

            IAdaptivePerformanceSettings settings = AdaptivePerformanceGeneralSettings.Instance.Manager.activeLoader.GetSettings();
            if (settings == null)
                return;

            settings.automaticPerformanceMode = false;
            settings.logging = false;
        }

        [OneTimeSetUp]
        public void SetupAdaptivePerformanceManagerTest()
        {
            var ap = Holder.Instance;
            ap.DevelopmentSettings.Logging = false;
            ap.DevicePerformanceControl.AutomaticPerformanceControl = false;
        }

        [OneTimeTearDown]
        public void TeardownAdaptivePerformanceManagerTest()
        {
            base.TearDownTest();
        }

        [SetUp]
        public void Initialization()
        {
            if(!adaptivePerformanceGeneralSettings.IsProviderInitialized)
                Holder.Instance.InitializeAdaptivePerformance();

            if(!adaptivePerformanceGeneralSettings.IsProviderStarted)
                Holder.Instance.StartAdaptivePerformance();
        }

        [UnityTest]
        public IEnumerator Applies_Cpu_Level()
        {
            var subsystem = AdaptivePerformanceGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<SimulatorAdaptivePerformanceSubsystem>();
            var ap = Holder.Instance;

            subsystem.AcceptsPerformanceLevel = true;

            var level = ap.DevicePerformanceControl.MaxCpuPerformanceLevel;

            ap.DevicePerformanceControl.CpuLevel = level;

            yield return null;

            Assert.AreEqual(level, ap.DevicePerformanceControl.CpuLevel);
            Assert.AreEqual(level, ap.PerformanceStatus.PerformanceMetrics.CurrentCpuLevel);
        }

        [UnityTest]
        public IEnumerator Applies_Gpu_Level()
        {
            var subsystem = AdaptivePerformanceGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<SimulatorAdaptivePerformanceSubsystem>();
            var ap = Holder.Instance;

            subsystem.AcceptsPerformanceLevel = true;

            var level = ap.DevicePerformanceControl.MaxGpuPerformanceLevel;

            ap.DevicePerformanceControl.GpuLevel = level;

            yield return null;

            Assert.AreEqual(level, ap.DevicePerformanceControl.GpuLevel);
            Assert.AreEqual(level, ap.PerformanceStatus.PerformanceMetrics.CurrentGpuLevel);
        }

        [UnityTest]
        public IEnumerator Unknown_GpuLevel_In_Throttling_State()
        {
            var subsystem = AdaptivePerformanceGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<SimulatorAdaptivePerformanceSubsystem>();
            var ap = Holder.Instance;

            subsystem.AcceptsPerformanceLevel = false;

            ap.DevicePerformanceControl.GpuLevel = ap.DevicePerformanceControl.MaxCpuPerformanceLevel;

            yield return null;

            Assert.AreEqual(PerformanceControlMode.System, ap.DevicePerformanceControl.PerformanceControlMode);

            Assert.AreEqual(Constants.UnknownPerformanceLevel, ap.PerformanceStatus.PerformanceMetrics.CurrentGpuLevel);
        }

        [UnityTest]
        public IEnumerator Unknown_CpuLevel_In_Throttling_State()
        {
            var subsystem = AdaptivePerformanceGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<SimulatorAdaptivePerformanceSubsystem>();
            var ap = Holder.Instance;

            subsystem.AcceptsPerformanceLevel = false;

            ap.DevicePerformanceControl.CpuLevel = ap.DevicePerformanceControl.MaxCpuPerformanceLevel;

            yield return null;

            Assert.AreEqual(PerformanceControlMode.System, ap.DevicePerformanceControl.PerformanceControlMode);
            Assert.AreEqual(Constants.UnknownPerformanceLevel, ap.PerformanceStatus.PerformanceMetrics.CurrentCpuLevel);
        }

        [UnityTest]
        public IEnumerator Ignores_Invalid_Cpu_Level()
        {
            var subsystem = AdaptivePerformanceGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<SimulatorAdaptivePerformanceSubsystem>();
            var ap = Holder.Instance;

            subsystem.AcceptsPerformanceLevel = true;
            subsystem.WarningLevel = WarningLevel.NoWarning;

            ap.DevicePerformanceControl.CpuLevel = 100;

            yield return null;

            Assert.AreEqual(Constants.UnknownPerformanceLevel, ap.PerformanceStatus.PerformanceMetrics.CurrentCpuLevel);
        }

        [UnityTest]
        public IEnumerator Ignores_Invalid_Gpu_Level()
        {
            var subsystem = AdaptivePerformanceGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<SimulatorAdaptivePerformanceSubsystem>();
            var ap = Holder.Instance;

            subsystem.AcceptsPerformanceLevel = true;
            subsystem.WarningLevel = WarningLevel.NoWarning;

            ap.DevicePerformanceControl.GpuLevel = -2;

            yield return null;

            Assert.AreEqual(Constants.UnknownPerformanceLevel, ap.PerformanceStatus.PerformanceMetrics.CurrentGpuLevel);
        }

        [UnityTest]
        public IEnumerator TemperatureChangeEvent_Values_Are_Applied()
        {
            var subsystem = AdaptivePerformanceGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<SimulatorAdaptivePerformanceSubsystem>();
            var ap = Holder.Instance;

            subsystem.TemperatureLevel = 0.0f;
            subsystem.TemperatureTrend = 1.0f;

            yield return null;

            Assert.AreEqual(0.0f, ap.ThermalStatus.ThermalMetrics.TemperatureLevel);
            Assert.AreEqual(1.0f, ap.ThermalStatus.ThermalMetrics.TemperatureTrend);
        }

        [UnityTest]
        public IEnumerator WarningLevel_Is_Applied()
        {
            var subsystem = AdaptivePerformanceGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<SimulatorAdaptivePerformanceSubsystem>();
            var ap = Holder.Instance;

            subsystem.WarningLevel = WarningLevel.ThrottlingImminent;

            yield return null;

            Assert.AreEqual(WarningLevel.ThrottlingImminent, ap.ThermalStatus.ThermalMetrics.WarningLevel);

            subsystem.WarningLevel = WarningLevel.Throttling;

            yield return null;

            Assert.AreEqual(WarningLevel.Throttling, ap.ThermalStatus.ThermalMetrics.WarningLevel);

            subsystem.WarningLevel = WarningLevel.NoWarning;

            yield return null;

            Assert.AreEqual(WarningLevel.NoWarning, ap.ThermalStatus.ThermalMetrics.WarningLevel);
        }

        [UnityTest]
        public IEnumerator Provider_FrameTimes_Work()
        {
            var subsystem = AdaptivePerformanceGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<SimulatorAdaptivePerformanceSubsystem>();
            var ap = Holder.Instance;

            subsystem.NextGpuFrameTime = 0.033f;
            subsystem.NextCpuFrameTime = 0.015f;
            subsystem.NextOverallFrameTime = 0.042f;

            yield return null;

            var ft = ap.PerformanceStatus.FrameTiming;

            Assert.IsTrue(Mathf.Abs(ft.CurrentFrameTime - subsystem.NextOverallFrameTime) < 0.001f);
            Assert.IsTrue(Mathf.Abs(ft.CurrentCpuFrameTime - subsystem.NextCpuFrameTime) < 0.001f);
            Assert.IsTrue(Mathf.Abs(ft.CurrentGpuFrameTime - subsystem.NextGpuFrameTime) < 0.001f);
        }

        [UnityTest]
        public IEnumerator GpuBound_When_GpuTime_Is_High()
        {
            var subsystem = AdaptivePerformanceGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<SimulatorAdaptivePerformanceSubsystem>();
            var ap = Holder.Instance;

            for (int i = 0; i < Constants.DefaultAverageFrameCount; ++i)
            {
                subsystem.NextGpuFrameTime = 0.040f;
                subsystem.NextCpuFrameTime = 0.015f;
                subsystem.NextOverallFrameTime = 0.042f;
                yield return null;
            }

            Assert.AreEqual(PerformanceBottleneck.GPU, ap.PerformanceStatus.PerformanceMetrics.PerformanceBottleneck);
        }

        [UnityTest]
        public IEnumerator CpuBound_When_CpuTime_Is_High()
        {
            var subsystem = AdaptivePerformanceGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<SimulatorAdaptivePerformanceSubsystem>();
            var ap = Holder.Instance;

            for (int i = 0; i < Constants.DefaultAverageFrameCount; ++i)
            {
                subsystem.NextGpuFrameTime = 0.033f;
                subsystem.NextCpuFrameTime = 0.038f;
                subsystem.NextOverallFrameTime = 0.042f;
                yield return null;
            }

            Assert.AreEqual(PerformanceBottleneck.CPU, ap.PerformanceStatus.PerformanceMetrics.PerformanceBottleneck);
        }

        [UnityTest]
        public IEnumerator Unknown_Bottleneck_When_GpuTime_And_CpuTime_Are_Equal()
        {
            var subsystem = AdaptivePerformanceGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<SimulatorAdaptivePerformanceSubsystem>();
            var ap = Holder.Instance;

            for (int i = 0; i < Constants.DefaultAverageFrameCount; ++i)
            {
                subsystem.NextGpuFrameTime = 0.033f;
                subsystem.NextCpuFrameTime = subsystem.NextGpuFrameTime;
                subsystem.NextOverallFrameTime = 0.042f;
                yield return null;
            }

            Assert.AreEqual(PerformanceBottleneck.Unknown, ap.PerformanceStatus.PerformanceMetrics.PerformanceBottleneck);
        }

        [UnityTest]
        public IEnumerator Bottleneck_TargetFrameRate_Works()
        {
            var subsystem = AdaptivePerformanceGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<SimulatorAdaptivePerformanceSubsystem>();
            var ap = Holder.Instance;
            // There is a bug in the editor where target framerate can be 0 and the test will fail and we skip the test. -1 is default in Editor and is not supported either.
            if (AdaptivePerformanceManager.EffectiveTargetFrameRate() <= 0)
            {
                Assert.AreEqual(null, null);
                yield break;
            }

            // very low frame numbers, to avoid failing test
            for (int i = 0; i < Constants.DefaultAverageFrameCount; ++i)
            {
                subsystem.NextCpuFrameTime = 0.0001f;
                subsystem.NextGpuFrameTime = 0.0002f;
                subsystem.NextOverallFrameTime = 0.001f;
                yield return null;
            }

            Assert.AreEqual(PerformanceBottleneck.TargetFrameRate, ap.PerformanceStatus.PerformanceMetrics.PerformanceBottleneck);
        }

        [UnityTest]
        public IEnumerator PerformanceBottleneckChangeEvent_Works()
        {
            var subsystem = AdaptivePerformanceGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<SimulatorAdaptivePerformanceSubsystem>();
            var ap = Holder.Instance;
            int eventCounter = 0;
            var bottleneck = PerformanceBottleneck.Unknown;

            // Change to Undefined bottleneck so we can have one change (as often it's defined due to the other tests)
            for (int i = 0; i < Constants.DefaultAverageFrameCount; ++i)
            {
                subsystem.NextCpuFrameTime = 0.4f;
                subsystem.NextGpuFrameTime = 0.4f;
                subsystem.NextOverallFrameTime = 0.9f;
                yield return null;
            }
            PerformanceBottleneckChangeHandler eventHandler = delegate (PerformanceBottleneckChangeEventArgs args)
            {
                ++eventCounter;
                bottleneck = args.PerformanceBottleneck;
            };

            ap.PerformanceStatus.PerformanceBottleneckChangeEvent += eventHandler;
            // very high frame numbers, to avoid failing test on very slow machines (where targetframe rate is very high
            for (int i = 0; i < Constants.DefaultAverageFrameCount; ++i)
            {
                subsystem.NextCpuFrameTime = 0.1f;
                subsystem.NextGpuFrameTime = 0.9f;
                subsystem.NextOverallFrameTime = 0.9f;
                yield return null;
            }
            Assert.AreEqual(PerformanceBottleneck.GPU, ap.PerformanceStatus.PerformanceMetrics.PerformanceBottleneck);
            Assert.AreEqual(PerformanceBottleneck.GPU, bottleneck);
            Assert.AreEqual(1, eventCounter);
        }

        [UnityTest]
        public IEnumerator PerformanceLevelChangeEvent_Works()
        {
            var subsystem = AdaptivePerformanceGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<SimulatorAdaptivePerformanceSubsystem>();
            subsystem.AcceptsPerformanceLevel = true; // boost mode disables performance level acceptance and those tests can run before this.
            var ap = Holder.Instance;

            var ctrl = ap.DevicePerformanceControl;
            ctrl.AutomaticPerformanceControl = false;
            var ps = ap.PerformanceStatus;

            ctrl.CpuLevel = 1;
            ctrl.GpuLevel = 2;

            yield return null;

            Assert.AreEqual(1, ps.PerformanceMetrics.CurrentCpuLevel);
            Assert.AreEqual(2, ps.PerformanceMetrics.CurrentGpuLevel);

            var eventArgs = new PerformanceLevelChangeEventArgs();
            PerformanceLevelChangeHandler eventHandler = delegate (PerformanceLevelChangeEventArgs args)
            {
                eventArgs = args;
            };
            ps.PerformanceLevelChangeEvent += eventHandler;

            ctrl.CpuLevel = 4;
            ctrl.GpuLevel = 0;

            yield return null;

            Assert.AreEqual(4, ps.PerformanceMetrics.CurrentCpuLevel);
            Assert.AreEqual(4, eventArgs.CpuLevel);
            Assert.AreEqual(0, ps.PerformanceMetrics.CurrentGpuLevel);
            Assert.AreEqual(0, eventArgs.GpuLevel);
            Assert.AreEqual(3, eventArgs.CpuLevelDelta);
            Assert.AreEqual(-2, eventArgs.GpuLevelDelta);
            Assert.AreEqual(false, eventArgs.ManualOverride);
            Assert.AreEqual(PerformanceControlMode.Manual, eventArgs.PerformanceControlMode);
        }

        [UnityTest]
        public IEnumerator ThermalEvent_Works()
        {
            var subsystem = AdaptivePerformanceGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<SimulatorAdaptivePerformanceSubsystem>();
            var ap = Holder.Instance;
            var thermals = ap.ThermalStatus;

            var metrics = new ThermalMetrics();
            ThermalEventHandler eventHandler = delegate (ThermalMetrics args)
            {
                metrics = args;
            };
            thermals.ThermalEvent += eventHandler;

            subsystem.TemperatureLevel = 0.3f;
            subsystem.TemperatureTrend = 0.5f;

            yield return null;

            Assert.AreEqual(0.3f, metrics.TemperatureLevel, 0.0001f);
            Assert.AreEqual(0.5f, metrics.TemperatureTrend, 0.0001f);
        }

        [UnityTest]
        public IEnumerator PerformanceLevels_Are_Reapplied_After_Timeout()
        {
            var subsystem = AdaptivePerformanceGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<SimulatorAdaptivePerformanceSubsystem>();
            var ap = Holder.Instance;

            subsystem.AcceptsPerformanceLevel = true;

            int gpuLevel = 0;
            int cpuLevel = 0;
            ap.DevicePerformanceControl.CpuLevel = gpuLevel;
            ap.DevicePerformanceControl.GpuLevel = cpuLevel;

            yield return null;

            // Samsung Subsystem would do this when "timeout" happens (setLevels changes levels back to default after 10min)
            subsystem.GpuPerformanceLevel = Constants.UnknownPerformanceLevel;
            subsystem.CpuPerformanceLevel = Constants.UnknownPerformanceLevel;

            yield return null;

            // AdaptivePerformance is supposed to reapply the last settings
            Assert.AreEqual(cpuLevel, ap.PerformanceStatus.PerformanceMetrics.CurrentCpuLevel);
            Assert.AreEqual(gpuLevel, ap.PerformanceStatus.PerformanceMetrics.CurrentGpuLevel);
        }

        [UnityTest]
        public IEnumerator PerformanceBoostChangeEvent_Works()
        {
            var subsystem = AdaptivePerformanceGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<SimulatorAdaptivePerformanceSubsystem>();
            var ap = Holder.Instance;

            var ctrl = ap.DevicePerformanceControl;
            var ps = ap.PerformanceStatus;

            ctrl.CpuPerformanceBoost = true;
            ctrl.GpuPerformanceBoost = true;

            yield return null;
            Assert.AreEqual(false, ps.PerformanceMetrics.CpuPerformanceBoost);
            Assert.AreEqual(false, ps.PerformanceMetrics.GpuPerformanceBoost);

            yield return null;
            Assert.AreEqual(true, ps.PerformanceMetrics.CpuPerformanceBoost);
            Assert.AreEqual(true, ps.PerformanceMetrics.GpuPerformanceBoost);

            var eventArgs = new PerformanceBoostChangeEventArgs();
            PerformanceBoostChangeHandler eventHandler = delegate (PerformanceBoostChangeEventArgs args)
            {
                eventArgs = args;
            };
            ps.PerformanceBoostChangeEvent += eventHandler;

            yield return null;

            // Samsung Subsystem would do this when "timeout" happens (enableBoost changes to no boost after 15 sec)
            subsystem.GpuPerformanceBoost = false;
            subsystem.CpuPerformanceBoost = false;

            Assert.AreEqual(true, ps.PerformanceMetrics.CpuPerformanceBoost);
            Assert.AreEqual(false, eventArgs.CpuBoost);
            Assert.AreEqual(true, ps.PerformanceMetrics.GpuPerformanceBoost);
            Assert.AreEqual(false, eventArgs.GpuBoost);

            yield return null;

            Assert.AreEqual(false, ps.PerformanceMetrics.CpuPerformanceBoost);
            Assert.AreEqual(false, eventArgs.CpuBoost);
            Assert.AreEqual(false, ps.PerformanceMetrics.GpuPerformanceBoost);
            Assert.AreEqual(false, eventArgs.GpuBoost);
        }

        [UnityTest]
        public IEnumerator PerformanceBoost_Disables_PerformanceLevels()
        {
            var subsystem = AdaptivePerformanceGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<SimulatorAdaptivePerformanceSubsystem>();
            var ap = Holder.Instance;

            var ctrl = ap.DevicePerformanceControl;
            var ps = ap.PerformanceStatus;

            ctrl.CpuPerformanceBoost = true;
            ctrl.GpuPerformanceBoost = true;

            yield return null;
            Assert.AreEqual(false, ps.PerformanceMetrics.CpuPerformanceBoost);
            Assert.AreEqual(false, ps.PerformanceMetrics.GpuPerformanceBoost);

            yield return null;
            Assert.AreEqual(true, ps.PerformanceMetrics.CpuPerformanceBoost);
            Assert.AreEqual(true, ps.PerformanceMetrics.GpuPerformanceBoost);


            ap.DevicePerformanceControl.CpuLevel = 3;
            ap.DevicePerformanceControl.GpuLevel = 2;

            yield return null;

            // AdaptivePerformance is supposed to not apply levels when bost mode is activated
            Assert.AreEqual(Constants.UnknownPerformanceLevel, ap.PerformanceStatus.PerformanceMetrics.CurrentCpuLevel);
            Assert.AreEqual(Constants.UnknownPerformanceLevel, ap.PerformanceStatus.PerformanceMetrics.CurrentGpuLevel);
        }

        [UnityTest]
        public IEnumerator PerformanceBoost_Is_Off_After_Timeout()
        {
            var subsystem = AdaptivePerformanceGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<SimulatorAdaptivePerformanceSubsystem>();
            var ap = Holder.Instance;

            ap.DevicePerformanceControl.GpuPerformanceBoost = true;
            ap.DevicePerformanceControl.CpuPerformanceBoost = true;

            yield return null;

            // Samsung Subsystem would do this when "timeout" happens (enableBoost changes to no boost after 15 sec)
            subsystem.GpuPerformanceBoost = false;
            subsystem.CpuPerformanceBoost = false;

            yield return null;

            // AdaptivePerformance is supposed to reapply the last settings
            Assert.AreEqual(false, ap.PerformanceStatus.PerformanceMetrics.CpuPerformanceBoost);
            Assert.AreEqual(false, ap.PerformanceStatus.PerformanceMetrics.GpuPerformanceBoost);
        }

        [UnityTest]
        public IEnumerator Feature_ClusterInfo_Is_Supported()
        {
            var subsystem = AdaptivePerformanceGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<SimulatorAdaptivePerformanceSubsystem>();
            var ap = Holder.Instance;

            var clusterInfo = ap.SupportedFeature(Feature.ClusterInfo);
            var bigcores = ap.PerformanceStatus.PerformanceMetrics.ClusterInfo.BigCore;
            var mediumcores = ap.PerformanceStatus.PerformanceMetrics.ClusterInfo.MediumCore;
            var tinycores = ap.PerformanceStatus.PerformanceMetrics.ClusterInfo.LittleCore;

            yield return null;

            ClusterInfo newClusterInfo = new ClusterInfo
            {
                BigCore = 5,
                MediumCore = 4,
                LittleCore = -1
            };
            subsystem.SetClusterInfo(newClusterInfo);
            yield return null;

            Assert.AreEqual(true, clusterInfo);
            Assert.AreEqual(bigcores, 1);
            Assert.AreEqual(mediumcores, 3);
            Assert.AreEqual(tinycores, 4);
            Assert.AreEqual(ap.PerformanceStatus.PerformanceMetrics.ClusterInfo.BigCore, 5);
            Assert.AreEqual(ap.PerformanceStatus.PerformanceMetrics.ClusterInfo.MediumCore, 4);
            Assert.AreEqual(ap.PerformanceStatus.PerformanceMetrics.ClusterInfo.LittleCore, -1);
        }

        /// <summary>
        /// Verifies the Scalers available in the assembly (e.g. created as sub-types from <see cref="AdaptivePerformanceScaler "/>
        /// are all registered and available as Scalers in the <see cref="AdaptivePerformanceIndexer"/>.
        /// </summary>
        [UnityTest]
        public IEnumerator All_Scalers_Available()
        {
            var ap = Holder.Instance;
            var apIndexer = ap.Indexer;

            var ti = typeof(AdaptivePerformanceScaler);
            var assemblyScalers = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(i => i.GetTypes())
                .Count(i => ti.IsAssignableFrom(i) && !i.IsAbstract);

            var indexedScalers = new List<AdaptivePerformanceScaler>();
            apIndexer.GetAllRegisteredScalers(ref indexedScalers);

            yield return null;

            Assert.AreEqual(assemblyScalers, indexedScalers.Count);
        }

        [Test]
        public void Lifecycle_Running_Works()
        {
            Assert.IsTrue(Holder.Instance.Active);
            Assert.IsNotNull(Holder.Instance.Indexer);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.IsProviderInitialized);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.IsProviderStarted);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.Manager.isInitializationComplete);

            var activeLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            Assert.IsNotNull(activeLoader);
            Assert.IsTrue(activeLoader.Initialized);
            Assert.IsTrue(activeLoader.Running);

            var subsystem = activeLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;
            Assert.IsNotNull(subsystem);
            Assert.IsTrue(subsystem.Initialized);
            Assert.IsTrue(subsystem.running);
        }

        [UnityTest]
        public IEnumerator Lifecycle_Initialization_When_Running_NoChange()
        {
            // attempting to initialize when AP is already running should result in no change

            var originalIndexer = Holder.Instance.Indexer;

            Holder.Instance.InitializeAdaptivePerformance();
            yield return null;

            Assert.IsTrue(Holder.Instance.Active);
            Assert.IsNotNull(Holder.Instance.Indexer);
            Assert.AreSame(originalIndexer, Holder.Instance.Indexer);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.IsProviderInitialized);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.IsProviderStarted);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.Manager.isInitializationComplete);

            var activeLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            Assert.IsNotNull(activeLoader);
            Assert.IsTrue(activeLoader.Initialized);
            Assert.IsTrue(activeLoader.Running);

            var subsystem = activeLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;
            Assert.IsNotNull(subsystem);
            Assert.IsTrue(subsystem.Initialized);
            Assert.IsTrue(subsystem.running);
        }

        [UnityTest]
        public IEnumerator Lifecycle_Start_When_Running_NoChange()
        {
            // attempting to start when AP is already running should result in no change

            Holder.Instance.StartAdaptivePerformance();
            yield return null;

            Assert.IsTrue(Holder.Instance.Active);
            Assert.IsNotNull(Holder.Instance.Indexer);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.IsProviderInitialized);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.IsProviderStarted);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.Manager.isInitializationComplete);

            var activeLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            Assert.IsNotNull(activeLoader);
            Assert.IsTrue(activeLoader.Initialized);
            Assert.IsTrue(activeLoader.Running);

            var subsystem = activeLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;
            Assert.IsNotNull(subsystem);
            Assert.IsTrue(subsystem.Initialized);
            Assert.IsTrue(subsystem.running);
        }

        [UnityTest]
        public IEnumerator Lifecycle_Stop_When_Running_Works()
        {
            Holder.Instance.StopAdaptivePerformance();
            yield return null;

            Assert.IsFalse(Holder.Instance.Active);
            Assert.IsNotNull(Holder.Instance.Indexer);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.IsProviderInitialized);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.IsProviderStarted);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.Manager.isInitializationComplete);

            var activeLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            Assert.IsNotNull(activeLoader);
            Assert.IsTrue(activeLoader.Initialized);
            Assert.IsFalse(activeLoader.Running);

            var subsystem = activeLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;
            Assert.IsNotNull(subsystem);
            Assert.IsTrue(subsystem.Initialized);
            Assert.IsFalse(subsystem.running);
        }

        [UnityTest]
        public IEnumerator Lifecycle_Deinitialize_When_Running_Works()
        {
            var previousActiveLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            var previousSubsystem = previousActiveLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;

            Holder.Instance.DeinitializeAdaptivePerformance();
            yield return null;

            Assert.IsFalse(Holder.Instance.Active);
            Assert.IsNull(Holder.Instance.Indexer);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.IsProviderInitialized);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.IsProviderStarted);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.Manager.isInitializationComplete);

            var activeLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            Assert.IsNull(activeLoader);

            // check previous active loader and subsystem if shut down
            Assert.IsFalse(previousActiveLoader.Initialized);
            Assert.IsFalse(previousActiveLoader.Running);
            Assert.IsFalse(previousSubsystem.Initialized);
            Assert.IsFalse(previousSubsystem.running);
        }

        [UnityTest]
        public IEnumerator Lifecycle_Stop_Deinitialize_Works()
        {
            Holder.Instance.StopAdaptivePerformance();
            yield return null;

            Assert.IsFalse(Holder.Instance.Active);
            Assert.IsNotNull(Holder.Instance.Indexer);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.IsProviderInitialized);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.IsProviderStarted);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.Manager.isInitializationComplete);

            var activeLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            Assert.IsNotNull(activeLoader);
            Assert.IsTrue(activeLoader.Initialized);
            Assert.IsFalse(activeLoader.Running);

            var subsystem = activeLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;
            Assert.IsNotNull(subsystem);
            Assert.IsTrue(subsystem.Initialized);
            Assert.IsFalse(subsystem.running);

            Holder.Instance.DeinitializeAdaptivePerformance();
            yield return null;

            Assert.IsFalse(Holder.Instance.Active);
            Assert.IsNull(Holder.Instance.Indexer);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.IsProviderInitialized);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.IsProviderStarted);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.Manager.isInitializationComplete);

            Assert.IsNull(adaptivePerformanceGeneralSettings.Manager.activeLoader);

            // check previous active loader and subsystem if shut down
            Assert.IsFalse(activeLoader.Initialized);
            Assert.IsFalse(activeLoader.Running);
            Assert.IsFalse(subsystem.Initialized);
            Assert.IsFalse(subsystem.running);
        }

        [UnityTest]
        public IEnumerator Lifecycle_Stop_Start_Works()
        {
            var originalIndexer = Holder.Instance.Indexer;
            var originalLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            var originalSubsystem = originalLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;

            Holder.Instance.StopAdaptivePerformance();
            yield return null;

            Assert.IsFalse(Holder.Instance.Active);
            Assert.IsNotNull(Holder.Instance.Indexer);
            Assert.AreSame(originalIndexer, Holder.Instance.Indexer);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.IsProviderInitialized);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.IsProviderStarted);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.Manager.isInitializationComplete);

            var activeLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            Assert.IsNotNull(activeLoader);
            Assert.AreSame(originalLoader, activeLoader);
            Assert.IsTrue(activeLoader.Initialized);
            Assert.IsFalse(activeLoader.Running);

            var subsystem = activeLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;
            Assert.IsNotNull(subsystem);
            Assert.AreSame(originalSubsystem, subsystem);
            Assert.IsTrue(subsystem.Initialized);
            Assert.IsFalse(subsystem.running);

            Holder.Instance.StartAdaptivePerformance();
            yield return null;

            Assert.IsTrue(Holder.Instance.Active);
            Assert.IsNotNull(Holder.Instance.Indexer);
            Assert.AreSame(originalIndexer, Holder.Instance.Indexer);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.IsProviderInitialized);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.IsProviderStarted);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.Manager.isInitializationComplete);

            activeLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            Assert.IsNotNull(activeLoader);
            Assert.AreSame(originalLoader, activeLoader);
            Assert.IsTrue(activeLoader.Initialized);
            Assert.IsTrue(activeLoader.Running);

            subsystem = activeLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;
            Assert.IsNotNull(subsystem);
            Assert.AreSame(originalSubsystem, subsystem);
            Assert.IsTrue(subsystem.Initialized);
            Assert.IsTrue(subsystem.running);
        }

        [UnityTest]
        public IEnumerator Lifecycle_Entire_Workflow_Works()
        {
            var originalIndexer = Holder.Instance.Indexer;
            var originalLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            var originalSubsystem = originalLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;

            Holder.Instance.StopAdaptivePerformance();
            yield return null;

            Assert.IsFalse(Holder.Instance.Active);
            Assert.IsNotNull(Holder.Instance.Indexer);
            Assert.AreSame(originalIndexer, Holder.Instance.Indexer);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.IsProviderInitialized);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.IsProviderStarted);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.Manager.isInitializationComplete);

            var activeLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            Assert.IsNotNull(activeLoader);
            Assert.AreSame(originalLoader, activeLoader);
            Assert.IsTrue(activeLoader.Initialized);
            Assert.IsFalse(activeLoader.Running);

            var subsystem = activeLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;
            Assert.IsNotNull(subsystem);
            Assert.AreSame(originalSubsystem, subsystem);
            Assert.IsTrue(subsystem.Initialized);
            Assert.IsFalse(subsystem.running);

            Holder.Instance.DeinitializeAdaptivePerformance();
            yield return null;

            Assert.IsFalse(Holder.Instance.Active);
            Assert.IsNull(Holder.Instance.Indexer);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.IsProviderInitialized);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.IsProviderStarted);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.Manager.isInitializationComplete);

            Assert.IsNull(adaptivePerformanceGeneralSettings.Manager.activeLoader);

            // check previous active loader and subsystem if shut down
            Assert.IsFalse(activeLoader.Initialized);
            Assert.IsFalse(activeLoader.Running);
            Assert.IsFalse(subsystem.Initialized);
            Assert.IsFalse(subsystem.running);

            Holder.Instance.InitializeAdaptivePerformance();
            yield return null;

            var newIndexer = Holder.Instance.Indexer;

            Assert.IsFalse(Holder.Instance.Active);
            Assert.IsNotNull(Holder.Instance.Indexer);
            Assert.AreNotSame(originalIndexer, newIndexer);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.IsProviderInitialized);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.IsProviderStarted);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.Manager.isInitializationComplete);

            var newLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            Assert.IsNotNull(newLoader);
            Assert.AreSame(originalLoader, newLoader);
            Assert.IsTrue(newLoader.Initialized);
            Assert.IsFalse(newLoader.Running);

            var newSubsystem = newLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;
            Assert.IsNotNull(newSubsystem);
            Assert.AreNotSame(originalSubsystem, newSubsystem);
            Assert.IsTrue(newSubsystem.Initialized);
            Assert.IsFalse(newSubsystem.running);

            Holder.Instance.StartAdaptivePerformance();
            yield return null;

            Assert.IsTrue(Holder.Instance.Active);
            Assert.IsNotNull(Holder.Instance.Indexer);
            Assert.AreSame(newIndexer, Holder.Instance.Indexer);
            Assert.AreNotSame(originalIndexer, Holder.Instance.Indexer);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.IsProviderInitialized);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.IsProviderStarted);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.Manager.isInitializationComplete);

            Assert.AreNotSame(originalSubsystem, adaptivePerformanceGeneralSettings.Manager.activeLoader.GetDefaultSubsystem());

            activeLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            Assert.IsNotNull(activeLoader);
            Assert.AreSame(newLoader, activeLoader);
            Assert.AreSame(originalLoader, activeLoader);
            Assert.IsTrue(activeLoader.Initialized);
            Assert.IsTrue(activeLoader.Running);

            subsystem = activeLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;
            Assert.IsNotNull(subsystem);
            Assert.AreSame(newSubsystem, subsystem);
            Assert.AreNotSame(originalSubsystem, subsystem);
            Assert.IsTrue(subsystem.Initialized);
            Assert.IsTrue(subsystem.running);
        }

        [UnityTest]
        public IEnumerator Lifecycle_Deinitialize_Start_NotAllowed()
        {
            var activeLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            var subsystem = activeLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;

            Holder.Instance.DeinitializeAdaptivePerformance();
            yield return null;

            Holder.Instance.StartAdaptivePerformance();
            yield return null;

            // should not be running b/c initialization was not called first

            Assert.IsFalse(Holder.Instance.Active);
            Assert.IsNull(Holder.Instance.Indexer);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.IsProviderInitialized);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.IsProviderStarted);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.Manager.isInitializationComplete);

            Assert.IsNull(adaptivePerformanceGeneralSettings.Manager.activeLoader);

            // check previous active loader and subsystem if shut down
            Assert.IsFalse(activeLoader.Initialized);
            Assert.IsFalse(activeLoader.Running);
            Assert.IsFalse(subsystem.Initialized);
            Assert.IsFalse(subsystem.running);
        }

        [UnityTest]
        public IEnumerator Lifecycle_Deinitialize_Stop_NowAllowed()
        {
            var activeLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            var subsystem = activeLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;

            Holder.Instance.DeinitializeAdaptivePerformance();
            yield return null;

            Holder.Instance.StopAdaptivePerformance();
            yield return null;

            // should still be torn down

            Assert.IsFalse(Holder.Instance.Active);
            Assert.IsNull(Holder.Instance.Indexer);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.IsProviderInitialized);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.IsProviderStarted);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.Manager.isInitializationComplete);

            Assert.IsNull(adaptivePerformanceGeneralSettings.Manager.activeLoader);

            // check previous active loader and subsystem if shut down
            Assert.IsFalse(activeLoader.Initialized);
            Assert.IsFalse(activeLoader.Running);
            Assert.IsFalse(subsystem.Initialized);
            Assert.IsFalse(subsystem.running);
        }

        [UnityTest]
        public IEnumerator Lifecycle_Double_Initialization_NoChange()
        {
            // attempting to double initialization should result in no change

            var originalIndexer = Holder.Instance.Indexer;
            var originalLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            var originalSubsystem = originalLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;

            Holder.Instance.DeinitializeAdaptivePerformance();
            yield return null;

            Holder.Instance.InitializeAdaptivePerformance();
            yield return null;

            var newIndexer = Holder.Instance.Indexer;
            Assert.IsNotNull(newIndexer);
            Assert.AreNotSame(originalIndexer, newIndexer);

            var newLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            Assert.IsNotNull(newLoader);
            Assert.AreSame(originalLoader, newLoader);

            var newSubsystem = newLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;
            Assert.IsNotNull(newSubsystem);
            Assert.AreNotSame(originalSubsystem, newSubsystem);

            Holder.Instance.InitializeAdaptivePerformance();
            yield return null;

            Assert.IsFalse(Holder.Instance.Active);
            Assert.IsNotNull(Holder.Instance.Indexer);
            Assert.AreSame(newIndexer, Holder.Instance.Indexer);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.IsProviderInitialized);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.IsProviderStarted);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.Manager.isInitializationComplete);

            var activeLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            Assert.IsNotNull(activeLoader);
            Assert.AreSame(newLoader, activeLoader);

            var subsystem = activeLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;
            Assert.IsNotNull(subsystem);
            Assert.AreSame(newSubsystem, subsystem);
        }

        [UnityTest]
        public IEnumerator Lifecycle_Double_Start_NoChange()
        {
            // attempting to double start should result in no change

            var originalIndexer = Holder.Instance.Indexer;
            var originalLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            var originalSubsystem = originalLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;

            Holder.Instance.DeinitializeAdaptivePerformance();
            yield return null;

            Holder.Instance.InitializeAdaptivePerformance();
            yield return null;

            Holder.Instance.StartAdaptivePerformance();
            yield return null;

            var newIndexer = Holder.Instance.Indexer;
            Assert.IsNotNull(newIndexer);
            Assert.AreNotSame(originalIndexer, newIndexer);

            var newLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            Assert.IsNotNull(newLoader);
            Assert.AreSame(originalLoader, newLoader);

            var newSubsystem = newLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;
            Assert.IsNotNull(newSubsystem);
            Assert.AreNotSame(originalSubsystem, newSubsystem);

            Holder.Instance.StartAdaptivePerformance();
            yield return null;

            Assert.IsTrue(Holder.Instance.Active);
            Assert.IsNotNull(Holder.Instance.Indexer);
            Assert.AreSame(newIndexer, Holder.Instance.Indexer);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.IsProviderInitialized);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.IsProviderStarted);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.Manager.isInitializationComplete);

            var activeLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            Assert.IsNotNull(activeLoader);
            Assert.AreSame(newLoader, activeLoader);

            var subsystem = activeLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;
            Assert.IsNotNull(subsystem);
            Assert.AreSame(newSubsystem, subsystem);
        }

        [UnityTest]
        public IEnumerator Lifecycle_Double_Stop_NoChange()
        {
            // attempting to double stop should result in no change

            var originalIndexer = Holder.Instance.Indexer;
            var originalLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            var originalSubsystem = originalLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;

            Holder.Instance.StopAdaptivePerformance();
            yield return null;

            Holder.Instance.StopAdaptivePerformance();
            yield return null;

            Assert.IsFalse(Holder.Instance.Active);
            Assert.IsNotNull(Holder.Instance.Indexer);
            Assert.AreSame(originalIndexer, Holder.Instance.Indexer);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.IsProviderInitialized);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.IsProviderStarted);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.Manager.isInitializationComplete);

            var activeLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            Assert.IsNotNull(activeLoader);
            Assert.AreSame(originalLoader, activeLoader);

            var subsystem = activeLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;
            Assert.IsNotNull(subsystem);
            Assert.AreSame(originalSubsystem, subsystem);
        }

        [UnityTest]
        public IEnumerator Lifecycle_Double_Deinitialize_NoChange()
        {
            // attempting to double deinitialize should result in no change

            var previousActiveLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            var previousSubsystem = previousActiveLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;

            Holder.Instance.DeinitializeAdaptivePerformance();
            yield return null;

            Holder.Instance.DeinitializeAdaptivePerformance();
            yield return null;

            Assert.IsFalse(Holder.Instance.Active);
            Assert.IsNull(Holder.Instance.Indexer);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.IsProviderInitialized);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.IsProviderStarted);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.Manager.isInitializationComplete);

            var activeLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            Assert.IsNull(activeLoader);

            // check previous active loader and subsystem if shut down
            Assert.IsFalse(previousActiveLoader.Initialized);
            Assert.IsFalse(previousActiveLoader.Running);
            Assert.IsFalse(previousSubsystem.Initialized);
            Assert.IsFalse(previousSubsystem.running);
        }
    }
}
