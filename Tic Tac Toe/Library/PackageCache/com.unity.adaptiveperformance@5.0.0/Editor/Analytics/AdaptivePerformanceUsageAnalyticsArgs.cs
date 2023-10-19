using System;
using UnityEngine;

namespace UnityEditor.AdaptivePerformance.Editor.Analytics
{
    [Serializable]
    struct AdaptivePerformanceUsageAnalyticsArgs
    {
        /// <summary>
        /// The actual event type which define the context of the event under a common table.
        /// </summary>
        [SerializeField]
        private string eventType;

        /// <summary>
        /// The <see cref="GUID"/> of the build.
        /// </summary>
        [SerializeField]
        private string buildGuid;

        /// <summary>
        /// The target platform.
        /// </summary>
        [SerializeField]
        private string targetPlatform;

        /// <summary>
        /// The target platform version.
        /// </summary>
        [SerializeField]
        private string targetPlatformVersion;

        /// <summary>
        /// List of Adaptive Performance Providers installed.
        /// </summary>
        [SerializeField]
        private AdaptivePerformanceInfo[] apProvidersInfo;

        public AdaptivePerformanceUsageAnalyticsArgs(
            EventType eventType,
            AdaptivePerformanceInfo[] apProvidersInfo,
            GUID? buildGuid = null,
            BuildTarget? targetPlatform = null,
            string targetPlatformVersion = null)
        {
            this.eventType = eventType.ToString();
            this.buildGuid = buildGuid.ToString();
            this.targetPlatform = targetPlatform?.ToString();
            this.apProvidersInfo = apProvidersInfo;
            this.targetPlatformVersion = targetPlatformVersion;
        }

        public enum EventType
        {
            BuildPlayer
        }

        [Serializable]
        public struct AdaptivePerformanceInfo
        {
            /// <summary>
            /// Name of the AR Manager.
            /// </summary>
            public string name;

            /// <summary>
            /// <c>True</c> if the AR Manager is active in the scene, otherwise <c>False</c>.
            /// </summary>
            public bool active;
        }
    }
}
