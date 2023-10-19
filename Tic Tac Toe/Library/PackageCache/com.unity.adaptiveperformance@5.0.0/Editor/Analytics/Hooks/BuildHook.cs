using System;
using System.Linq;
using UnityEditor.AdaptivePerformance.Editor.Metadata;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.AdaptivePerformance;
using UnityEngine.SceneManagement;

namespace UnityEditor.AdaptivePerformance.Editor.Analytics.Hooks
{
    class BuildHook : IProcessSceneWithReport
    {
        int IOrderedCallback.callbackOrder => 1;

        void IProcessSceneWithReport.OnProcessScene(Scene scene, BuildReport report)
        {
            if (report == null)
                return;

            AdaptivePerformanceAnalytics.UsageAnalyticEvent.Send(new AdaptivePerformanceUsageAnalyticsArgs(
                eventType: AdaptivePerformanceUsageAnalyticsArgs.EventType.BuildPlayer,
                buildGuid: report.summary.guid,
                targetPlatform: report.summary.platform,
                targetPlatformVersion: GetTargetPlatformVersion(report.summary.platform),
                apProvidersInfo: GetInstalledProviders(report.summary.platformGroup)));
        }

        string GetTargetPlatformVersion(BuildTarget buildTarget)
        {
            // only supporting Android right now
            if (buildTarget != BuildTarget.Android)
                return null;

            var settings = PlayerSettings.Android.targetSdkVersion;
            return settings.ToString();
        }

        AdaptivePerformanceUsageAnalyticsArgs.AdaptivePerformanceInfo[] GetInstalledProviders(BuildTargetGroup buildTarget)
        {
            var loaders = AdaptivePerformancePackageMetadataStore.GetLoadersForBuildTarget(buildTarget)
                .Where(i => AdaptivePerformancePackageMetadataStore.IsPackageInstalled(i.packageId))
                .Select(i => new AdaptivePerformanceUsageAnalyticsArgs.AdaptivePerformanceInfo
                {
                    name = i.loaderName,
                    active = AdaptivePerformancePackageMetadataStore.IsLoaderAssigned(i.loaderType, buildTarget)
                }).ToList();

            return loaders.Any() ? loaders.ToArray() : null;
        }
    }
}
