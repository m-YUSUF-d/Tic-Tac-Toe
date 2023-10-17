using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace UnityEngine.AdaptivePerformance
{
    /// <summary>
    /// This attribute is used to tag classes as providing build settings support for an Adaptive Performance provider. The unified setting system
    /// will present the settings as an inspectable object in the Project Settings window using the built-in inspector UI.
    ///
    /// The implementor of the settings is able to create their own custom UI and the Project Settings system will use that UI in
    /// place of the build-in one in the Inspector. See the <see href="https://docs.unity3d.com/Manual/ExtendingTheEditor.html">Extending the Editor</see>
    /// page in the Unity Manual for more information.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AdaptivePerformanceConfigurationDataAttribute : Attribute
    {
        /// <summary>
        /// The display name that the user sees in the Project Settings window.
        /// </summary>
        public string displayName { get; set; }

        /// <summary>
        /// The key that will be used to store the singleton instance of these settings within EditorBuildSettings.
        /// For more information, see the <see href="https://docs.unity3d.com/ScriptReference/EditorBuildSettings.html">EditorBuildSettings</see> scripting
        /// API documentation.
        /// </summary>
        public string buildSettingsKey { get; set; }

        private AdaptivePerformanceConfigurationDataAttribute() {}

        /// <summary>Constructor for attribute</summary>
        /// <param name="displayName">The display name to use in the Project Settings window.</param>
        /// <param name="buildSettingsKey">The key to use to get or set build settings with.</param>
        public AdaptivePerformanceConfigurationDataAttribute(string displayName, string buildSettingsKey)
        {
            this.displayName = displayName;
            this.buildSettingsKey = buildSettingsKey;
        }
    }
}
