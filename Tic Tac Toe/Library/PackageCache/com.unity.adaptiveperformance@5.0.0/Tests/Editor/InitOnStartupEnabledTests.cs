using NUnit.Framework;

using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.AdaptivePerformance;
using UnityEngine.AdaptivePerformance.Provider;

namespace UnityEditor.AdaptivePerformance.Editor.Tests
{
    public class InitOnStartupEnabledTests : ManagementTestSetup
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            isInitializingOnStartup = true;
            ProviderAndSettingsSetup();

            yield return new EnterPlayMode();
        }

        [UnityTearDown]
        public IEnumerator Teardown()
        {
            ProviderAndSettingsTearDown();

            yield return new ExitPlayMode();

            AssetCleanUp();
        }

        [UnityTest]
        public IEnumerator Lifecycle_Workflow_Valid()
        {
            var apGameObject = GameObject.Find(AdaptivePerformanceManagerSpawner.AdaptivePerformanceManagerObjectName);
            Assert.IsNotNull(apGameObject);

            var apm = UnityEngine.Object.FindObjectOfType<AdaptivePerformanceManager>();
            Assert.IsNotNull(apm);


            var lifecycleEventCount = 0;
            IAdaptivePerformance eventInstance = null;
            LifecycleChangeType? changeType = null;
            LifecycleEventHandler lifecycleEventHandler =
                delegate(IAdaptivePerformance instance, LifecycleChangeType type)
                {
                    eventInstance = instance;
                    changeType = type;
                    lifecycleEventCount++;
                };
            Holder.LifecycleEventHandler += lifecycleEventHandler;


            var adaptivePerformanceGeneralSettings = AdaptivePerformanceGeneralSettings.Instance;
            var originalInstance = Holder.Instance;
            var originalIndexer = Holder.Instance.Indexer;

            Assert.IsNotNull(originalInstance);
            Assert.IsTrue(Holder.Instance.Active);
            Assert.IsNotNull(originalIndexer);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.IsProviderInitialized);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.IsProviderStarted);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.Manager.isInitializationComplete);

            var originalLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            Assert.IsNotNull(originalLoader);
            Assert.IsTrue(originalLoader.Initialized);
            Assert.IsTrue(originalLoader.Running);

            var originalSubsystem = originalLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;
            Assert.IsNotNull(originalSubsystem);
            Assert.IsTrue(originalSubsystem.Initialized);
            Assert.IsTrue(originalSubsystem.running);


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

            var instance = Holder.Instance;


            Holder.Deinitialize();
            yield return null;

            Assert.AreSame(originalInstance, instance);
            Assert.IsFalse(instance.Active);
            Assert.IsNull(instance.Indexer);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.IsProviderInitialized);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.IsProviderStarted);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.Manager.isInitializationComplete);

            Assert.IsNull(adaptivePerformanceGeneralSettings.Manager.activeLoader);

            yield return null;

            // check previous active loader and subsystem if shut down
            Assert.IsFalse(activeLoader.Initialized);
            Assert.IsFalse(activeLoader.Running);
            Assert.IsFalse(subsystem.Initialized);
            Assert.IsFalse(subsystem.running);

            yield return null;

            // check lifecycle event details
            Assert.AreSame(originalInstance, eventInstance);
            Assert.IsNotNull(changeType);
            Assert.AreEqual(LifecycleChangeType.Destroyed, changeType.Value);


            yield return null;

            apGameObject = GameObject.Find(AdaptivePerformanceManagerSpawner.AdaptivePerformanceManagerObjectName);
            Assert.IsNull(apGameObject);

            apm = UnityEngine.Object.FindObjectOfType<AdaptivePerformanceManager>();
            Assert.IsNull(apm);


            yield return null;

            Assert.AreEqual(1, lifecycleEventCount);
        }
    }
}
