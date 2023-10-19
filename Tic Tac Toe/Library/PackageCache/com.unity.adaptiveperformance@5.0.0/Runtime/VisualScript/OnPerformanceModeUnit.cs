#if VISUAL_SCRIPTING_ENABLED

using Unity.VisualScripting;

namespace UnityEngine.AdaptivePerformance.VisualScripting
{
    [UnitShortTitle("On Performance Mode")]
    [UnitSubtitle("Performance Mode")]
    [UnitCategory("AdaptivePerformance/Performance")]
    public class OnPerformanceModeUnit : EventUnit<PerformanceMode>
    {
        [DoNotSerialize]
        public ValueOutput PerformanceMode { get; private set; }

        [DoNotSerialize]
        public ValueOutput IsStandard { get; private set; }

        [DoNotSerialize]
        public ValueOutput IsBattery { get; private set; }

        [DoNotSerialize]
        public ValueOutput IsOptimize { get; private set; }

        [DoNotSerialize]
        public ValueOutput IsCpu { get; private set; }

        [DoNotSerialize]
        public ValueOutput IsGpu { get; private set; }

        string m_PerformanceMode = AdaptivePerformance.PerformanceMode.Unknown.ToString();
        bool m_IsStandard = false;
        bool m_IsBattery = false;
        bool m_IsOptimize = false;
        bool m_IsCpu = false;
        bool m_IsGpu = false;

        protected override bool register => true;

        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(AdaptivePerformanceEventHooks.OnPerformanceModeEvent);
        }

        protected override void AssignArguments(Flow flow, PerformanceMode mode)
        {
            flow.SetValue(PerformanceMode, mode.ToString());
            flow.SetValue(IsStandard, mode == AdaptivePerformance.PerformanceMode.Standard);
            flow.SetValue(IsBattery, mode == AdaptivePerformance.PerformanceMode.Battery);
            flow.SetValue(IsOptimize, mode == AdaptivePerformance.PerformanceMode.Optimize);
            flow.SetValue(IsCpu, mode == AdaptivePerformance.PerformanceMode.CPU);
            flow.SetValue(IsGpu, mode == AdaptivePerformance.PerformanceMode.GPU);
        }

        protected override void Definition()
        {
            base.Definition();
            PerformanceMode = ValueOutput<string>(nameof(PerformanceMode), (flow) => { UpdateStats(); return m_PerformanceMode; });
            IsStandard = ValueOutput<bool>(nameof(IsStandard), (flow) => { UpdateStats(); return m_IsStandard; });
            IsBattery = ValueOutput<bool>(nameof(IsBattery), (flow) => { UpdateStats(); return m_IsBattery; });
            IsOptimize = ValueOutput<bool>(nameof(IsOptimize), (flow) => { UpdateStats(); return m_IsOptimize; });
            IsCpu = ValueOutput<bool>(nameof(IsCpu), (flow) => { UpdateStats(); return m_IsCpu; });
            IsGpu = ValueOutput<bool>(nameof(IsGpu), (flow) => { UpdateStats(); return m_IsGpu; });
        }

        void UpdateStats()
        {
            if (Application.isPlaying && Holder.Instance != null)
            {
                var pm = Holder.Instance.PerformanceModeStatus.PerformanceMode;
                m_PerformanceMode = pm.ToString();
                m_IsStandard = pm == AdaptivePerformance.PerformanceMode.Standard;
                m_IsBattery = pm == AdaptivePerformance.PerformanceMode.Battery;
                m_IsOptimize = pm == AdaptivePerformance.PerformanceMode.Optimize;
                m_IsCpu = pm == AdaptivePerformance.PerformanceMode.CPU;
                m_IsGpu = pm == AdaptivePerformance.PerformanceMode.GPU;
            }
        }
    }
}
#endif
