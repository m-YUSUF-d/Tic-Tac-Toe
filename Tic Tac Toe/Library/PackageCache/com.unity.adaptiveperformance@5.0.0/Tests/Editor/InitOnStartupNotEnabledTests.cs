using NUnit.Framework;

using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.AdaptivePerformance;
using UnityEngine.AdaptivePerformance.Provider;

namespace UnityEditor.AdaptivePerformance.Editor.Tests
{
    public class InitOnStartupNotEnabledTests : ManagementTestSetup
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            isInitializingOnStartup = false;
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
            Assert.IsNull(apGameObject);

            var apm = UnityEngine.Object.FindObjectOfType<AdaptivePerformanceManager>();
            Assert.IsNull(apm);


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


            Assert.IsNull(Holder.Instance);


            var adaptivePerformanceGeneralSettings = AdaptivePerformanceGeneralSettings.Instance;
            Assert.IsFalse(adaptivePerformanceGeneralSettings.IsProviderInitialized);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.IsProviderStarted);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.Manager.isInitializationComplete);

            var originalLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            Assert.IsNull(originalLoader);


            Holder.Initialize();
            yield return null;

            Assert.IsNotNull(Holder.Instance);

            var originalInstance = Holder.Instance;

            // check lifecycle event details
            Assert.AreSame(eventInstance, originalInstance);
            Assert.IsNotNull(changeType);
            Assert.AreEqual(LifecycleChangeType.Created, changeType.Value);


            // check game object
            apGameObject = GameObject.Find(AdaptivePerformanceManagerSpawner.AdaptivePerformanceManagerObjectName);
            Assert.IsNotNull(apGameObject);

            apm = UnityEngine.Object.FindObjectOfType<AdaptivePerformanceManager>();
            Assert.IsNotNull(apm);


            var originalIndexer = Holder.Instance.Indexer;

            Assert.IsFalse(Holder.Instance.Active);
            Assert.IsNotNull(originalIndexer);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.IsProviderInitialized);
            Assert.IsFalse(adaptivePerformanceGeneralSettings.IsProviderStarted);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.Manager.isInitializationComplete);

            originalLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            Assert.IsNotNull(originalLoader);
            Assert.IsTrue(originalLoader.Initialized);
            Assert.IsFalse(originalLoader.Running);

            var originalSubsystem = originalLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;
            Assert.IsNotNull(originalSubsystem);
            Assert.IsTrue(originalSubsystem.Initialized);
            Assert.IsFalse(originalSubsystem.running);


            Holder.Instance.StartAdaptivePerformance();
            yield return null;

            var newIndexer = Holder.Instance.Indexer;

            Assert.IsTrue(Holder.Instance.Active);
            Assert.IsNotNull(newIndexer);
            Assert.AreSame(originalIndexer, newIndexer);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.IsProviderInitialized);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.IsProviderStarted);
            Assert.IsTrue(adaptivePerformanceGeneralSettings.Manager.isInitializationComplete);

            var newLoader = adaptivePerformanceGeneralSettings.Manager.activeLoader;
            Assert.IsNotNull(newLoader);
            Assert.AreSame(originalLoader, newLoader);
            Assert.IsTrue(newLoader.Initialized);
            Assert.IsTrue(newLoader.Running);

            var newSubsystem = newLoader.GetDefaultSubsystem() as AdaptivePerformanceSubsystem;
            Assert.IsNotNull(newSubsystem);
            Assert.AreSame(originalSubsystem, newSubsystem);
            Assert.IsTrue(newSubsystem.Initialized);
            Assert.IsTrue(newSubsystem.running);


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

            Assert.AreEqual(2, lifecycleEventCount);
        }
    }
}
