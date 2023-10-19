using System;
using System.IO;
using UnityEngine;

using UnityEngine.AdaptivePerformance;
using UnityEditor.AdaptivePerformance.Simulator.Editor;
using UnityEditor.AdaptivePerformance.Editor.Metadata;

namespace UnityEditor.AdaptivePerformance.Editor.Tests
{
    public abstract class ManagementTestSetup
    {
        protected static readonly string[] s_TestGeneralSettings = { "Temp", "Test" };
        protected static readonly string[] s_TempSettingsPath = { "Temp", "Test", "Settings" };

        static protected bool isInitializingOnStartup = true;

        public virtual void ProviderAndSettingsSetup()
        {
            AssetDatabase.DeleteAsset("Assets/Adaptive Performance");
            AssetDatabase.CreateFolder("Assets", "Adaptive Performance");

            var testManager = ScriptableObject.CreateInstance<AdaptivePerformanceManagerSettings>();

            var adaptivePerformanceGeneralSettings = ScriptableObject.CreateInstance<AdaptivePerformanceGeneralSettings>() as AdaptivePerformanceGeneralSettings;
            adaptivePerformanceGeneralSettings.Manager = testManager;
            adaptivePerformanceGeneralSettings.m_InitManagerOnStart = isInitializingOnStartup;

            var testPathToSettings = GetAssetPathForComponents(s_TempSettingsPath);
            testPathToSettings = Path.Combine(testPathToSettings, $"ManualTest_{ typeof(AdaptivePerformanceGeneralSettings).Name}.asset");
            if (!string.IsNullOrEmpty(testPathToSettings))
            {
                AssetDatabase.CreateAsset(adaptivePerformanceGeneralSettings, testPathToSettings);
                AssetDatabase.AddObjectToAsset(testManager, adaptivePerformanceGeneralSettings);

                AssetDatabase.SaveAssets();
            }

            var testPathToGeneralSettings = GetAssetPathForComponents(s_TestGeneralSettings);
            testPathToGeneralSettings = Path.Combine(testPathToGeneralSettings, $"ManualTest_{typeof(AdaptivePerformanceGeneralSettingsPerBuildTarget).Name}.asset");

            var buildTargetSettings = ScriptableObject.CreateInstance<AdaptivePerformanceGeneralSettingsPerBuildTarget>();
            buildTargetSettings.SetSettingsForBuildTarget(BuildTargetGroup.Standalone, adaptivePerformanceGeneralSettings);
            if (!string.IsNullOrEmpty(testPathToSettings))
            {
                AssetDatabase.CreateAsset(buildTargetSettings, testPathToGeneralSettings);
                AssetDatabase.SaveAssets();

                UnityEngine.Object currentSettings;
                EditorBuildSettings.TryGetConfigObject(AdaptivePerformanceGeneralSettings.k_SettingsKey, out currentSettings);
                EditorBuildSettings.AddConfigObject(AdaptivePerformanceGeneralSettings.k_SettingsKey, buildTargetSettings, true);
            }

            var testPathToLoader = GetAssetPathForComponents(s_TempSettingsPath);
            // Setup Loader
            var loader = ScriptableObject.CreateInstance(typeof(SimulatorProviderLoader)) as SimulatorProviderLoader;
            AssetDatabase.CreateAsset(loader, Path.Combine(testPathToLoader, $"ManualTest_{typeof(SimulatorProviderLoader).Name}.asset"));
            testManager.loaders.Add(loader);

            // Setup Settings
            var settings = ScriptableObject.CreateInstance(typeof(SimulatorProviderSettings)) as SimulatorProviderSettings;
            AssetDatabase.CreateAsset(settings, Path.Combine(testPathToLoader, $"ManualTest_{typeof(SimulatorProviderSettings).Name}.asset"));
            //settings.logging = false;
            EditorBuildSettings.AddConfigObject(SimulatorProviderConstants.k_SettingsKey, settings, true);

            // Due to the Settings menu, we have to manually assigned the Simulator loader in tests.
            AdaptivePerformancePackageMetadataStore.AssignLoader(AdaptivePerformanceGeneralSettings.Instance.Manager, typeof(SimulatorProviderLoader).Name, BuildTargetGroup.Standalone);
        }

        public virtual void ProviderAndSettingsTearDown()
        {
            var adaptivePerformanceManager = Holder.Instance as AdaptivePerformanceManager;

            var adaptivePerformanceGeneralSettings = AdaptivePerformanceGeneralSettings.Instance;
            var testManager = adaptivePerformanceGeneralSettings.Manager;

            if (testManager.isInitializationComplete)
                testManager.DeinitializeLoader();

            var loader = testManager.activeLoader;
            testManager.loaders.Remove(loader);

            EditorBuildSettings.RemoveConfigObject(AdaptivePerformanceGeneralSettings.k_SettingsKey);

            UnityEngine.Object.DestroyImmediate(adaptivePerformanceGeneralSettings, true);
            UnityEngine.Object.DestroyImmediate(testManager, true);
            UnityEngine.Object.DestroyImmediate(loader, true);
            UnityEngine.Object.DestroyImmediate(adaptivePerformanceManager, true);
        }

        public virtual void AssetCleanUp()
        {
            AssetDatabase.DeleteAsset(Path.Combine("Assets", "Temp"));
            AssetDatabase.DeleteAsset("Assets/Adaptive Performance");
        }

        public static string GetAssetPathForComponents(string[] pathComponents, string root = "Assets")
        {
            if (pathComponents.Length <= 0)
                return null;

            string path = root;
            foreach (var pc in pathComponents)
            {
                string subFolder = Path.Combine(path, pc);
                bool shouldCreate = true;
                foreach (var f in AssetDatabase.GetSubFolders(path))
                {
                    if (String.Compare(Path.GetFullPath(f), Path.GetFullPath(subFolder), true) == 0)
                    {
                        shouldCreate = false;
                        break;
                    }
                }

                if (shouldCreate)
                    AssetDatabase.CreateFolder(path, pc);
                path = subFolder;
            }

            return path;
        }
    }
}
