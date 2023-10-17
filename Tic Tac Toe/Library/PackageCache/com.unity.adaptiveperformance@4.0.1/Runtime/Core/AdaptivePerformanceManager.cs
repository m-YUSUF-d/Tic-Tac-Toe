using UnityEngine.Profiling;
#if VISUAL_SCRIPTING_ENABLED
using Unity.VisualScripting;
#endif

namespace UnityEngine.AdaptivePerformance
{
    internal class AdaptivePerformanceManager
        : MonoBehaviour
        , IAdaptivePerformance
        , IThermalStatus, IPerformanceStatus, IDevicePerformanceControl, IDevelopmentSettings
    {
        public event ThermalEventHandler ThermalEvent;
        public event PerformanceBottleneckChangeHandler PerformanceBottleneckChangeEvent;
        public event PerformanceLevelChangeHandler PerformanceLevelChangeEvent;
        public event PerformanceBoostChangeHandler PerformanceBoostChangeEvent;

        private Provider.AdaptivePerformanceSubsystem m_Subsystem = null;

        private bool m_JustResumed = false;

        private int m_RequestedCpuLevel = Constants.UnknownPerformanceLevel;
        private int m_RequestedGpuLevel = Constants.UnknownPerformanceLevel;
        private bool m_NewUserPerformanceLevelRequest = false;
        private bool m_RequestedCpuBoost = false;
        private bool m_RequestedGpuBoost = false;
        private bool m_NewUserCpuPerformanceBoostRequest = false;
        private bool m_NewUserGpuPerformanceBoostRequest = false;

        private ThermalMetrics m_ThermalMetrics = new ThermalMetrics
        {
            WarningLevel = WarningLevel.NoWarning,
            TemperatureLevel = -1.0f,
            TemperatureTrend = 0.0f,
        };

        public ThermalMetrics ThermalMetrics { get { return m_ThermalMetrics; } }

        private PerformanceMetrics m_PerformanceMetrics = new PerformanceMetrics
        {
            CurrentCpuLevel = Constants.UnknownPerformanceLevel,
            CurrentGpuLevel = Constants.UnknownPerformanceLevel,
            PerformanceBottleneck = PerformanceBottleneck.Unknown
        };

        public PerformanceMetrics PerformanceMetrics { get { return m_PerformanceMetrics; } }

        private FrameTiming m_FrameTiming = new FrameTiming
        {
            CurrentFrameTime = -1.0f,
            AverageFrameTime = -1.0f,
            CurrentGpuFrameTime = -1.0f,
            AverageGpuFrameTime = -1.0f,
            CurrentCpuFrameTime = -1.0f,
            AverageCpuFrameTime = -1.0f
        };

        public FrameTiming FrameTiming { get { return m_FrameTiming; } }

        public bool Logging
        {
            get { return APLog.enabled; }
            set { APLog.enabled = value; }
        }

        public int LoggingFrequencyInFrames { get; set; }

        public bool Active { get { return m_Subsystem != null; } }

        public int MaxCpuPerformanceLevel { get { return m_DevicePerfControl != null ? m_DevicePerfControl.MaxCpuPerformanceLevel : Constants.UnknownPerformanceLevel; } }

        public int MaxGpuPerformanceLevel { get { return m_DevicePerfControl != null ? m_DevicePerfControl.MaxGpuPerformanceLevel : Constants.UnknownPerformanceLevel; } }

        private bool m_AutomaticPerformanceControl;
        private bool m_AutomaticPerformanceControlChanged;
        public bool AutomaticPerformanceControl
        {
            get { return m_AutomaticPerformanceControl; }
            set
            {
                m_AutomaticPerformanceControl = value;
                m_AutomaticPerformanceControlChanged = true;
            }
        }

        public PerformanceControlMode PerformanceControlMode { get { return m_DevicePerfControl != null ? m_DevicePerfControl.PerformanceControlMode : PerformanceControlMode.System; } }

        public int CpuLevel
        {
            get { return m_RequestedCpuLevel; }
            set
            {
                m_RequestedCpuLevel = value;
                m_NewUserPerformanceLevelRequest = true;
            }
        }

        public int GpuLevel
        {
            get { return m_RequestedGpuLevel; }
            set
            {
                m_RequestedGpuLevel = value;
                m_NewUserPerformanceLevelRequest = true;
            }
        }

        public bool CpuPerformanceBoost
        {
            get { return m_RequestedCpuBoost; }
            set
            {
                m_RequestedCpuBoost = value;
                m_NewUserCpuPerformanceBoostRequest = true;
            }
        }

        public bool GpuPerformanceBoost
        {
            get { return m_RequestedGpuBoost; }
            set
            {
                m_RequestedGpuBoost = value;
                m_NewUserGpuPerformanceBoostRequest = true;
            }
        }

        public IDevelopmentSettings DevelopmentSettings { get { return this; } }
        public IThermalStatus ThermalStatus { get { return this; } }
        public IPerformanceStatus PerformanceStatus { get { return this; } }
        public IDevicePerformanceControl DevicePerformanceControl { get { return this; } }

        public AdaptivePerformanceIndexer Indexer { get; private set; }

        private IAdaptivePerformanceSettings m_Settings;
        public IAdaptivePerformanceSettings Settings { get { return m_Settings; }  private set { m_Settings = value; } }

        DevicePerformanceControlImpl m_DevicePerfControl;
        AutoPerformanceLevelController m_AutoPerformanceLevelController;
        CpuTimeProvider m_CpuFrameTimeProvider;
        GpuTimeProvider m_GpuFrameTimeProvider;
        Provider.IApplicationLifecycle m_AppLifecycle;
        TemperatureTrend m_TemperatureTrend;
        bool m_UseProviderOverallFrameTime = false;

        public bool SupportedFeature(Provider.Feature feature)
        {
            return m_Subsystem != null ? m_Subsystem.Capabilities.HasFlag(feature) : false;
        }

        public void Awake()
        {
            APLog.enabled = true;
            if (AdaptivePerformanceGeneralSettings.Instance == null)
            {
                APLog.Debug("No Provider was configured for use. Make sure you added at least one Provider in the Adaptive Performance Settings.");
                AdaptivePerformanceAnalytics.SendAdaptiveStartupEvent(m_Subsystem);
                return;
            }

            if (!AdaptivePerformanceGeneralSettings.Instance.InitManagerOnStart)
            {
                APLog.Debug("Adaptive Performance is disabled via Settings.");
                AdaptivePerformanceAnalytics.SendAdaptiveStartupEvent(m_Subsystem);
                return;
            }

            var activeLoader = AdaptivePerformanceGeneralSettings.Instance.Manager.ActiveLoaderAs<AdaptivePerformanceLoader>();
            if (activeLoader == null)
            {
                APLog.Debug("No Active Loader was found. Make sure to select your loader in the Adaptive Performance Settings for this platform.");
                AdaptivePerformanceAnalytics.SendAdaptiveStartupEvent(m_Subsystem);
                return;
            }

            m_Settings = activeLoader.GetSettings();
            if (m_Settings == null)
            {
                APLog.Debug("No Settings available. Did the Post Process Buildstep fail?");
                AdaptivePerformanceAnalytics.SendAdaptiveStartupEvent(m_Subsystem);
                return;
            }

            AutomaticPerformanceControl = m_Settings.automaticPerformanceMode;
            LoggingFrequencyInFrames = m_Settings.statsLoggingFrequencyInFrames;
            APLog.enabled = m_Settings.logging;

            if (m_Subsystem == null)
            {
                var subsystem = (Provider.AdaptivePerformanceSubsystem)activeLoader.GetDefaultSubsystem();

                if (subsystem != null)
                {
                    if (subsystem.initialized)
                    {
                        m_Subsystem = subsystem;
                        APLog.Debug("Subsystem version={0}", m_Subsystem.Version);
                    }
                    else
                    {
                        subsystem.Destroy();
                        APLog.Debug("Subsystem not initialized.");
                        AdaptivePerformanceAnalytics.SendAdaptiveStartupEvent(m_Subsystem);
                        return;
                    }
                }
            }

            if (m_Subsystem != null)
            {
                m_UseProviderOverallFrameTime = m_Subsystem.Capabilities.HasFlag(Provider.Feature.OverallFrameTime);
                m_DevicePerfControl = new DevicePerformanceControlImpl(m_Subsystem.PerformanceLevelControl);
                m_AutoPerformanceLevelController = new AutoPerformanceLevelController(m_DevicePerfControl, PerformanceStatus, ThermalStatus);

                m_AppLifecycle = m_Subsystem.ApplicationLifecycle;

                if (!m_Subsystem.Capabilities.HasFlag(Provider.Feature.CpuFrameTime))
                {
                    m_CpuFrameTimeProvider = new CpuTimeProvider();
                    StartCoroutine(InvokeEndOfFrame());
                }

                if (!m_Subsystem.Capabilities.HasFlag(Provider.Feature.GpuFrameTime))
                {
                    m_GpuFrameTimeProvider = new GpuTimeProvider();
                }

                m_TemperatureTrend = new TemperatureTrend(m_Subsystem.Capabilities.HasFlag(Provider.Feature.TemperatureTrend));

                // Request maximum performance by default
                if (m_RequestedCpuLevel == Constants.UnknownPerformanceLevel)
                    m_RequestedCpuLevel = m_DevicePerfControl.MaxCpuPerformanceLevel;
                if (m_RequestedGpuLevel == Constants.UnknownPerformanceLevel)
                    m_RequestedGpuLevel = m_DevicePerfControl.MaxGpuPerformanceLevel;

                // Override to get maximum performance by default in 'auto' mode
                m_NewUserPerformanceLevelRequest = true;

                if (m_Subsystem.PerformanceLevelControl == null)
                    m_DevicePerfControl.PerformanceControlMode = PerformanceControlMode.System;
                else if (AutomaticPerformanceControl)
                    m_DevicePerfControl.PerformanceControlMode = PerformanceControlMode.Automatic;
                else
                    m_DevicePerfControl.PerformanceControlMode = PerformanceControlMode.Manual;

                ThermalEvent += (ThermalMetrics thermalMetrics) => LogThermalEvent(thermalMetrics);
                PerformanceBottleneckChangeEvent += (PerformanceBottleneckChangeEventArgs ev) => LogBottleneckEvent(ev);
                PerformanceLevelChangeEvent += (PerformanceLevelChangeEventArgs ev) => LogPerformanceLevelEvent(ev);

                if (m_Subsystem.Capabilities.HasFlag(Provider.Feature.CpuPerformanceBoost))
                    PerformanceBoostChangeEvent += (PerformanceBoostChangeEventArgs ev) => LogBoostEvent(ev);

                Indexer = new AdaptivePerformanceIndexer(ref m_Settings);

                UpdateSubsystem();
            }
            AdaptivePerformanceAnalytics.SendAdaptiveStartupEvent(m_Subsystem);
        }

        private void LogThermalEvent(ThermalMetrics ev)
        {
            AdaptivePerformanceAnalytics.SendAdaptivePerformanceThermalEvent(ev);
            APLog.Debug("[thermal event] temperature level: {0}, warning level: {1}, thermal trend: {2}", ev.TemperatureLevel, ev.WarningLevel, ev.TemperatureTrend);
            #if VISUAL_SCRIPTING_ENABLED
            EventBus.Trigger(VisualScripting.AdaptivePerformanceEventHooks.OnThermalEvent, ev.WarningLevel);
            #endif
        }

        private void LogBottleneckEvent(PerformanceBottleneckChangeEventArgs ev)
        {
            APLog.Debug("[perf event] bottleneck: {0}", ev.PerformanceBottleneck);
            #if VISUAL_SCRIPTING_ENABLED
            EventBus.Trigger(VisualScripting.AdaptivePerformanceEventHooks.OnBottleneckEvent, ev.PerformanceBottleneck);
            #endif
        }

        private void LogBoostEvent(PerformanceBoostChangeEventArgs ev)
        {
            APLog.Debug("[perf event] CPU boost: {0}, GPU boost: {1}", ev.CpuBoost, ev.GpuBoost);
            #if VISUAL_SCRIPTING_ENABLED
            EventBus.Trigger(VisualScripting.AdaptivePerformanceEventHooks.OnBoostEvent, ev);
            #endif
        }

        private static string ToStringWithSign(int x)
        {
            return x.ToString("+#;-#;0");
        }

        private void LogPerformanceLevelEvent(PerformanceLevelChangeEventArgs ev)
        {
            APLog.Debug("[perf level change] cpu: {0}({1}) gpu: {2}({3}) control mode: {4} manual override: {5}", ev.CpuLevel, ToStringWithSign(ev.CpuLevelDelta), ev.GpuLevel, ToStringWithSign(ev.GpuLevelDelta), ev.PerformanceControlMode, ev.ManualOverride);
            #if VISUAL_SCRIPTING_ENABLED
            EventBus.Trigger(VisualScripting.AdaptivePerformanceEventHooks.OnPerformanceLevelEvent, ev);
            #endif
        }

        private void AddNonNegativeValue(RunningAverage runningAverage, float value)
        {
            if (value >= 0.0f && value < 1.0f) // don't add frames that took longer than 1s
                runningAverage.AddValue(value);
        }

        private WaitForEndOfFrame m_WaitForEndOfFrame = new WaitForEndOfFrame();

        private System.Collections.IEnumerator InvokeEndOfFrame()
        {
            while (true)
            {
                yield return m_WaitForEndOfFrame;
                if (m_CpuFrameTimeProvider != null)
                    m_CpuFrameTimeProvider.EndOfFrame();
            }
        }

        public void LateUpdate()
        {
            // m_RenderThreadCpuTime uses native plugin event to get CPU time of render thread.
            // We don't want to do this at end of frame because it might introduce an unnecessary sync when GraphicsJobs are used.
            // Alternative would be to use Vulkan native plugin API to configure the event.
            if (m_CpuFrameTimeProvider != null || m_GpuFrameTimeProvider != null)
            {
                // InvokeEndOfFrame is not executed in non-render frames, so we make sure to also keep this in sync
                if (WillCurrentFrameRender())
                {
                    if (m_CpuFrameTimeProvider != null)
                        m_CpuFrameTimeProvider.LateUpdate();

                    if (m_GpuFrameTimeProvider != null)
                        m_GpuFrameTimeProvider.Measure();
                }
            }
        }

        public void Update()
        {
            if (m_Subsystem == null)
                return;

            UpdateSubsystem();

            Indexer.Update();

            if (Profiler.enabled)
                CollectProfilerStats();

            if (APLog.enabled && LoggingFrequencyInFrames > 0)
            {
                m_FrameCount++;
                if (m_FrameCount % LoggingFrequencyInFrames == 0)
                {
                    APLog.Debug(m_Subsystem.Stats);
                    APLog.Debug("Performance level CPU={0}/{1} GPU={2}/{3} thermal warn={4}({5}) thermal level={6} mode={7}", m_PerformanceMetrics.CurrentCpuLevel, MaxCpuPerformanceLevel,
                        m_PerformanceMetrics.CurrentGpuLevel, MaxGpuPerformanceLevel, m_ThermalMetrics.WarningLevel, (int)m_ThermalMetrics.WarningLevel, m_ThermalMetrics.TemperatureLevel, m_DevicePerfControl.PerformanceControlMode);
                    APLog.Debug("Average GPU frametime = {0} ms (Current = {1} ms)", m_FrameTiming.AverageGpuFrameTime * 1000.0f , m_FrameTiming.CurrentGpuFrameTime * 1000.0f);
                    APLog.Debug("Average CPU frametime = {0} ms (Current = {1} ms)", m_FrameTiming.AverageCpuFrameTime * 1000.0f, m_FrameTiming.CurrentCpuFrameTime * 1000.0f);
                    APLog.Debug("Average frametime = {0} ms (Current = {1} ms)", m_FrameTiming.AverageFrameTime * 1000.0f, m_FrameTiming.CurrentFrameTime * 1000.0f);
                    APLog.Debug("Bottleneck {0}, ThermalTrend {1}", m_PerformanceMetrics.PerformanceBottleneck, m_ThermalMetrics.TemperatureTrend);
                    APLog.Debug("CPU Boost Mode {0}, GPU Boost Mode {1}", m_PerformanceMetrics.CpuPerformanceBoost, m_PerformanceMetrics.GpuPerformanceBoost);
                    APLog.Debug("Cluster Info = Big Cores: {0} Medium Cores: {1} Little Cores: {2}", m_PerformanceMetrics.ClusterInfo.BigCore, m_PerformanceMetrics.ClusterInfo.MediumCore, m_PerformanceMetrics.ClusterInfo.LittleCore);
                    APLog.Debug("FPS = {0}", 1.0f / m_FrameTiming.AverageFrameTime);
                }
            }
        }

        private void CollectProfilerStats()
        {
            AdaptivePerformanceProfilerStats.CurrentCPUCounter.Sample(m_FrameTiming.CurrentCpuFrameTime * 1000000000.0f);
            AdaptivePerformanceProfilerStats.AvgCPUCounter.Sample(m_FrameTiming.AverageCpuFrameTime * 1000000000.0f);
            AdaptivePerformanceProfilerStats.CurrentGPUCounter.Sample(m_FrameTiming.CurrentGpuFrameTime * 1000000000.0f);
            AdaptivePerformanceProfilerStats.AvgGPUCounter.Sample(m_FrameTiming.AverageGpuFrameTime * 1000000000.0f);
            AdaptivePerformanceProfilerStats.CurrentCPULevelCounter.Sample(m_PerformanceMetrics.CurrentCpuLevel);
            AdaptivePerformanceProfilerStats.CurrentGPULevelCounter.Sample(m_PerformanceMetrics.CurrentGpuLevel);
            AdaptivePerformanceProfilerStats.CurrentFrametimeCounter.Sample(m_FrameTiming.CurrentFrameTime * 1000000000.0f);
            AdaptivePerformanceProfilerStats.AvgFrametimeCounter.Sample(m_FrameTiming.AverageFrameTime * 1000000000.0f);
            AdaptivePerformanceProfilerStats.WarningLevelCounter.Sample((int)m_ThermalMetrics.WarningLevel);
            AdaptivePerformanceProfilerStats.TemperatureLevelCounter.Sample(m_ThermalMetrics.TemperatureLevel);
            AdaptivePerformanceProfilerStats.TemperatureTrendCounter.Sample(m_ThermalMetrics.TemperatureTrend);
            AdaptivePerformanceProfilerStats.BottleneckCounter.Sample((int)m_PerformanceMetrics.PerformanceBottleneck);
        }

        private void AccumulateTimingValue(ref float accu, float newValue)
        {
            if (accu < 0.0f)
                return; // already invalid

            if (newValue >= 0.0f)
                accu += newValue;
            else
                accu = -1.0f; // set invalid
        }

        private void UpdateSubsystem()
        {
            Provider.PerformanceDataRecord updateResult = m_Subsystem.Update();

            m_ThermalMetrics.WarningLevel = updateResult.WarningLevel;
            m_ThermalMetrics.TemperatureLevel = updateResult.TemperatureLevel;

            if (!m_JustResumed)
            {
                // Update overall frame time
                if (!m_UseProviderOverallFrameTime)
                    AccumulateTimingValue(ref m_OverallFrameTimeAccu, Time.unscaledDeltaTime);

                if (WillCurrentFrameRender())
                {
                    AddNonNegativeValue(m_OverallFrameTime, m_UseProviderOverallFrameTime ? updateResult.OverallFrameTime : m_OverallFrameTimeAccu);
                    AddNonNegativeValue(m_GpuFrameTime, m_GpuFrameTimeProvider == null ? updateResult.GpuFrameTime : m_GpuFrameTimeProvider.GpuFrameTime);
                    AddNonNegativeValue(m_CpuFrameTime, m_CpuFrameTimeProvider == null ? updateResult.CpuFrameTime : m_CpuFrameTimeProvider.CpuFrameTime);

                    m_OverallFrameTimeAccu = 0.0f;
                }

                m_TemperatureTrend.Update(updateResult.TemperatureTrend, updateResult.TemperatureLevel, updateResult.ChangeFlags.HasFlag(Provider.Feature.TemperatureLevel), Time.time);
            }
            else
            {
                m_TemperatureTrend.Reset();
                m_JustResumed = false;
            }

            m_ThermalMetrics.TemperatureTrend = m_TemperatureTrend.ThermalTrend;

            // Update frame timing info and calculate performance bottleneck
            const float invalidTimingValue = -1.0f;
            m_FrameTiming.AverageFrameTime = m_OverallFrameTime.GetAverageOr(invalidTimingValue);
            m_FrameTiming.CurrentFrameTime = m_OverallFrameTime.GetMostRecentValueOr(invalidTimingValue);
            m_FrameTiming.AverageGpuFrameTime = m_GpuFrameTime.GetAverageOr(invalidTimingValue);
            m_FrameTiming.CurrentGpuFrameTime = m_GpuFrameTime.GetMostRecentValueOr(invalidTimingValue);
            m_FrameTiming.AverageCpuFrameTime = m_CpuFrameTime.GetAverageOr(invalidTimingValue);
            m_FrameTiming.CurrentCpuFrameTime = m_CpuFrameTime.GetMostRecentValueOr(invalidTimingValue);

            float targerFrameRate = EffectiveTargetFrameRate();
            float targetFrameTime = -1.0f;
            if (targerFrameRate > 0)
                targetFrameTime = 1.0f / targerFrameRate;

            bool triggerPerformanceBottleneckChangeEvent = false;
            bool triggerThermalEventEvent = false;
            bool triggerPerformanceBoostChangeEvent = false;
            var performanceBottleneckChangeEventArgs = new PerformanceBottleneckChangeEventArgs();
            var performanceBoostChangeEventArgs = new PerformanceBoostChangeEventArgs();

            if (m_OverallFrameTime.GetNumValues() == m_OverallFrameTime.GetSampleWindowSize() &&
                m_GpuFrameTime.GetNumValues() == m_GpuFrameTime.GetSampleWindowSize() &&
                m_CpuFrameTime.GetNumValues() == m_CpuFrameTime.GetSampleWindowSize())
            {
                PerformanceBottleneck bottleneck = BottleneckUtil.DetermineBottleneck(m_PerformanceMetrics.PerformanceBottleneck, m_FrameTiming.AverageCpuFrameTime,
                    m_FrameTiming.AverageGpuFrameTime, m_FrameTiming.AverageFrameTime, targetFrameTime);

                if (bottleneck != m_PerformanceMetrics.PerformanceBottleneck)
                {
                    m_PerformanceMetrics.PerformanceBottleneck = bottleneck;
                    performanceBottleneckChangeEventArgs.PerformanceBottleneck = bottleneck;
                    triggerPerformanceBottleneckChangeEvent = (PerformanceBottleneckChangeEvent != null);
                }
            }

            triggerThermalEventEvent = (ThermalEvent != null) &&
                (updateResult.ChangeFlags.HasFlag(Provider.Feature.WarningLevel) ||
                    updateResult.ChangeFlags.HasFlag(Provider.Feature.TemperatureLevel) ||
                    updateResult.ChangeFlags.HasFlag(Provider.Feature.TemperatureTrend));

            // The Subsystem may have changed the current levels (e.g. "timeout" of Samsung subsystem)
            if (updateResult.ChangeFlags.HasFlag(Provider.Feature.CpuPerformanceLevel))
                m_DevicePerfControl.CurrentCpuLevel = updateResult.CpuPerformanceLevel;
            if (updateResult.ChangeFlags.HasFlag(Provider.Feature.GpuPerformanceLevel))
                m_DevicePerfControl.CurrentGpuLevel = updateResult.GpuPerformanceLevel;

            // Update PerformanceControlMode
            if (updateResult.ChangeFlags.HasFlag(Provider.Feature.PerformanceLevelControl) || m_AutomaticPerformanceControlChanged)
            {
                m_AutomaticPerformanceControlChanged = false;
                if (updateResult.PerformanceLevelControlAvailable)
                {
                    if (AutomaticPerformanceControl)
                        m_DevicePerfControl.PerformanceControlMode = PerformanceControlMode.Automatic;
                    else
                        m_DevicePerfControl.PerformanceControlMode = PerformanceControlMode.Manual;
                }
                else
                {
                    m_DevicePerfControl.PerformanceControlMode = PerformanceControlMode.System;
                }
            }

            // Apply performance levels according to PerformanceControlMode
            m_AutoPerformanceLevelController.TargetFrameTime = targetFrameTime;
            m_AutoPerformanceLevelController.Enabled = (m_DevicePerfControl.PerformanceControlMode == PerformanceControlMode.Automatic);

            PerformanceLevelChangeEventArgs levelChangeEventArgs = new PerformanceLevelChangeEventArgs();
            if (m_DevicePerfControl.PerformanceControlMode != PerformanceControlMode.System)
            {
                if (m_AutoPerformanceLevelController.Enabled)
                {
                    if (m_NewUserPerformanceLevelRequest)
                    {
                        m_AutoPerformanceLevelController.Override(m_RequestedCpuLevel, m_RequestedGpuLevel);
                        levelChangeEventArgs.ManualOverride = true;
                    }

                    m_AutoPerformanceLevelController.Update();
                }
                else
                {
                    if (m_NewUserPerformanceLevelRequest)
                    {
                        m_DevicePerfControl.CpuLevel = m_RequestedCpuLevel;
                        m_DevicePerfControl.GpuLevel = m_RequestedGpuLevel;
                    }
                }
            }

            triggerPerformanceBoostChangeEvent = (PerformanceBoostChangeEvent != null) &&
                (updateResult.ChangeFlags.HasFlag(Provider.Feature.CpuPerformanceBoost) ||
                    updateResult.ChangeFlags.HasFlag(Provider.Feature.GpuPerformanceBoost));

            // The Subsystem may have changed the current modes (e.g. "timeout" of Samsung subsystem)
            if (updateResult.ChangeFlags.HasFlag(Provider.Feature.CpuPerformanceBoost))
            {
                if (m_DevicePerfControl.CpuPerformanceBoost != updateResult.CpuPerformanceBoost)
                {
                    m_DevicePerfControl.CpuPerformanceBoost = updateResult.CpuPerformanceBoost;
                    m_RequestedCpuBoost = updateResult.CpuPerformanceBoost;
                }
            }

            if (updateResult.ChangeFlags.HasFlag(Provider.Feature.GpuPerformanceBoost))
            {
                if (m_DevicePerfControl.GpuPerformanceBoost != updateResult.GpuPerformanceBoost)
                {
                    m_DevicePerfControl.GpuPerformanceBoost = updateResult.GpuPerformanceBoost;
                    m_RequestedGpuBoost = updateResult.GpuPerformanceBoost;
                }
            }

            if (m_NewUserCpuPerformanceBoostRequest && PerformanceBoostChangeEvent != null)
            {
                m_NewUserCpuPerformanceBoostRequest = false;

                m_Subsystem.PerformanceLevelControl.EnableCpuBoost();
            }

            if (m_NewUserGpuPerformanceBoostRequest && PerformanceBoostChangeEvent != null)
            {
                m_NewUserGpuPerformanceBoostRequest = false;

                m_Subsystem.PerformanceLevelControl.EnableGpuBoost();
            }

            if (m_DevicePerfControl.Update(out levelChangeEventArgs) && PerformanceLevelChangeEvent != null)
                PerformanceLevelChangeEvent.Invoke(levelChangeEventArgs);

            m_PerformanceMetrics.CurrentCpuLevel = m_DevicePerfControl.CurrentCpuLevel;
            m_PerformanceMetrics.CurrentGpuLevel = m_DevicePerfControl.CurrentGpuLevel;

            m_PerformanceMetrics.CpuPerformanceBoost = m_DevicePerfControl.CpuPerformanceBoost;
            m_PerformanceMetrics.GpuPerformanceBoost = m_DevicePerfControl.GpuPerformanceBoost;

            m_NewUserPerformanceLevelRequest = false;

            if (updateResult.ChangeFlags.HasFlag(Provider.Feature.ClusterInfo))
            {
                m_PerformanceMetrics.ClusterInfo = updateResult.ClusterInfo;
            }
            // PerformanceLevelChangeEvent and BoostModeChangeEvent triggers before those since it's useful for the user to know when the auto cpu/gpu level controller already made adjustments
            if (triggerThermalEventEvent)
                ThermalEvent.Invoke(m_ThermalMetrics);
            if (triggerPerformanceBottleneckChangeEvent)
                PerformanceBottleneckChangeEvent(performanceBottleneckChangeEventArgs);
            if (triggerPerformanceBoostChangeEvent)
            {
                performanceBoostChangeEventArgs.CpuBoost = m_DevicePerfControl.CpuPerformanceBoost;
                performanceBoostChangeEventArgs.GpuBoost = m_DevicePerfControl.GpuPerformanceBoost;
                PerformanceBoostChangeEvent(performanceBoostChangeEventArgs);
            }
        }

        private static bool WillCurrentFrameRender()
        {
            return Rendering.OnDemandRendering.willCurrentFrameRender;
        }

        public static float EffectiveTargetFrameRate()
        {
            return UnityEngine.Rendering.OnDemandRendering.effectiveRenderFrameRate;
        }

        public void OnDestroy()
        {
            if (m_Subsystem != null)
                m_Subsystem.Destroy();
            if (Indexer != null)
                Indexer.UnapplyAllScalers();
        }

        public void OnApplicationPause(bool pause)
        {
            if (m_Subsystem != null)
            {
                if (pause)
                {
                    if (m_AppLifecycle != null)
                        m_AppLifecycle.ApplicationPause();
                    m_OverallFrameTime.Reset();
                    m_GpuFrameTime.Reset();
                    m_CpuFrameTime.Reset();
                    if (m_CpuFrameTimeProvider != null)
                        m_CpuFrameTimeProvider.Reset();
                }
                else
                {
                    m_ThermalMetrics.WarningLevel = WarningLevel.NoWarning;
                    if (m_AppLifecycle != null)
                        m_AppLifecycle.ApplicationResume();
                    m_JustResumed = true;
                }
            }
        }

        private int m_FrameCount = 0;

        private RunningAverage m_OverallFrameTime = new RunningAverage();   // In seconds
        private float m_OverallFrameTimeAccu = 0.0f;
        private RunningAverage m_GpuFrameTime = new RunningAverage();   // In seconds
        private RunningAverage m_CpuFrameTime = new RunningAverage();   // In seconds
    }
}
