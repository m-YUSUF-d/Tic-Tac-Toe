#if NUGET_MOQ_AVAILABLE

using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AdaptivePerformance;
using FrameTiming = UnityEngine.AdaptivePerformance.FrameTiming;

namespace UnityEditor.AdaptivePerformance.Editor.Tests
{
    public class AdaptivePerformanceIndexerUnitTests
    {
        FrameTiming ft;
        ThermalMetrics tm;
        IThermalStatus thermalStatus;
        IAdaptivePerformance defaultAP;
        AdaptivePerformanceIndexer testSubject;
        IPerformanceStatus defaultPerformanceStatus;
        AdaptivePerformanceIndexerSettings idxSettings;
        IAdaptivePerformanceSettings perfSettings;
        PerformanceStateTracker idxPerformanceStateTracker;

        [SetUp]
        public void InitializeTest()
        {
            tm = new ThermalMetrics();
            ft = new FrameTiming();
            idxSettings = new AdaptivePerformanceIndexerSettings();
            perfSettings = new IAdaptivePerformanceSettings();
            idxPerformanceStateTracker = new IndexedPerformanceStateTracker(120);
            defaultAP = Mock.Of<IAdaptivePerformance>();
            thermalStatus = Mock.Of<IThermalStatus>();
            defaultPerformanceStatus = Mock.Of<IPerformanceStatus>();
        }

        [Test]
        public void VerifyScalarCapacityWhenTwoRegisteredScalersAdded()
        {
            testSubject = InitializeTestSubject(0.25f, WarningLevel.Throttling, 0.25f, 0.55f);
            AdaptivePerformanceScaler sC1 = new MyPerformanceScaler();
            AdaptivePerformanceScaler sC2 = new MyPerformanceScaler();
            List<AdaptivePerformanceScaler> scalers = new List<AdaptivePerformanceScaler>();
            scalers.Add(sC1);
            scalers.Add(sC2);
            testSubject.GetAllRegisteredScalers(ref scalers);
            Assert.AreEqual(4, scalers.Capacity);
        }

        [TestCase(0.0149999997f, 0.015f, WarningLevel.Throttling, 0.25f, 0.55f, 1)]
        [TestCase(0f, 0f, WarningLevel.Throttling, 0.25f, 0.55f, 1)]
        [TestCase(0.5f, 0.5f, WarningLevel.ThrottlingImminent, 0.25f, 0.55f, 1)]
        [TestCase(0.5f, 0.5f, WarningLevel.ThrottlingImminent, 0.25f, 0.55f, 199)]
        [TestCase(0f, 0f, WarningLevel.ThrottlingImminent, 0.25f, 0.55f, 1)]
        [TestCase(0.370000005f, 0.37f, WarningLevel.NoWarning, -0.4f, 0.55f, 1)]
        [TestCase(0f, 0f, WarningLevel.NoWarning, -0.4f, 0.55f, 1)]
        [TestCase(0f, 0f, WarningLevel.ThrottlingImminent, -0.4f, 0.55f, 1)]
        [TestCase(0f, 0f, WarningLevel.ThrottlingImminent, 0.75f, 1f, 1)]
        [TestCase(0f, 0f, WarningLevel.ThrottlingImminent, 0.75f, 0.55f, 1)]
        [TestCase(0.310000002f, 0.31f, WarningLevel.ThrottlingImminent, 0.75f, 0.55f, 1)]
        [TestCase(0f, 0f, WarningLevel.ThrottlingImminent, 0.75f, 0.55f, 999)]
        [TestCase(0.310000002f, 0.31f, WarningLevel.ThrottlingImminent, 0.75f, 0.55f, 400)]
        public void VerifyTimeUntilNextAction_VaryingWarningsAndTrends_VaryingDelays(float expected, float idxSettingValue, WarningLevel warnLevel, float tempTrend, float avFrameTime, int numOfUpdateCalls)
        {
            testSubject = InitializeTestSubject(idxSettingValue, warnLevel, tempTrend, avFrameTime);
            for (int i = 0; i < numOfUpdateCalls; i++)
            {
                testSubject.Update();
            }
            Assert.AreEqual(expected, testSubject.TimeUntilNextAction);
        }

        internal AdaptivePerformanceIndexer InitializeTestSubject(float idxSettingValue, WarningLevel warnLevel, float tempTrend, float avFrameTime)
        {
            tm.WarningLevel = warnLevel;
            tm.TemperatureTrend = tempTrend;
            ft.AverageFrameTime = avFrameTime;

            Mock.Get(defaultPerformanceStatus).Setup(ps => ps.FrameTiming).Returns(ft);
            Mock.Get(thermalStatus).Setup(ts => ts.ThermalMetrics).Returns(tm);
            Mock.Get(defaultAP).Setup(d => d.ThermalStatus).Returns(thermalStatus);
            Mock.Get(defaultAP).Setup(d => d.PerformanceStatus).Returns(defaultPerformanceStatus);

            Holder.Instance = defaultAP;
            idxSettings.thermalActionDelay = idxSettingValue;
            perfSettings.indexerSettings = idxSettings;
            return new ZeroDeltaTimeAdaptivePerformanceIndexer(ref perfSettings, idxPerformanceStateTracker);
        }
    }

    public class MyPerformanceScaler : AdaptivePerformanceScaler
    {
    }

    partial class IndexedPerformanceStateTracker : PerformanceStateTracker
    {
        public IndexedPerformanceStateTracker(int sampleCapacity)
            : base(sampleCapacity) { }

        protected override float GetEffectiveTargetFrameRate()
        {
            return 0f;
        }
    }

    public class ZeroDeltaTimeAdaptivePerformanceIndexer : AdaptivePerformanceIndexer
    {
        internal ZeroDeltaTimeAdaptivePerformanceIndexer(ref IAdaptivePerformanceSettings settings, PerformanceStateTracker idxtracker)
            : base(ref settings, idxtracker) { }

        protected override float DeltaTime()
        {
            return 0f;
        }
    }
}

#endif
