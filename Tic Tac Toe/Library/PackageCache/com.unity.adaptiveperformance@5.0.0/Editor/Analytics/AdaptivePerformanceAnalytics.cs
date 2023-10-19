using System;
using UnityEngine;

namespace UnityEditor.AdaptivePerformance.Editor.Analytics
{
    internal static class AdaptivePerformanceAnalytics
    {
        const string k_UsageEventName = "adaptiveperformance_usage";

        public static readonly AdaptivePerformanceEditorAnalyticsEvent<AdaptivePerformanceUsageAnalyticsArgs> UsageAnalyticEvent = new(k_UsageEventName);

        [InitializeOnLoadMethod]
        static void SetupAndRegister()
        {
            // The check for whether analytics is enabled or not is already done
            // by the Editor Analytics API.
            UsageAnalyticEvent.Register();
        }
    }
}
