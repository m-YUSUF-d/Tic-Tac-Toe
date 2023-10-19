using UnityEngine.Analytics;

namespace UnityEditor.AdaptivePerformance.Editor.Analytics
{
    internal class AdaptivePerformanceEditorAnalyticsEvent<T> : AdaptivePerformanceAnalyticsEvent<T> where T : struct
    {
        internal AdaptivePerformanceEditorAnalyticsEvent(string tableName,
            int maxEventsPerHour = k_DefaultMaxEventsPerHour, int maxElementCount = k_DefaultMaxElementCount)
            : base(tableName, maxEventsPerHour, maxElementCount) { }

        protected override AnalyticsResult RegisterWithAnalyticsServer() => EditorAnalytics.RegisterEventWithLimit(
            k_EventName,
            m_MaxEventsPerHour,
            m_MaxElementCount,
            AdaptivePerformanceAnalyticsConstants.VendorKey,
            AdaptivePerformanceAnalyticsConstants.Version
        );

        protected override bool SendEvent(T eventArgs)
        {
            // The event shouldn't be able to report if user has disabled analytics
            // but if we know it won't be reported then lets not waste time gathering
            // all the data.
            if (!EditorAnalytics.enabled)
                return false;

            var result = EditorAnalytics.SendEventWithLimit(k_EventName, eventArgs, AdaptivePerformanceAnalyticsConstants.Version);
            return result == AnalyticsResult.Ok;
        }
    }
}
