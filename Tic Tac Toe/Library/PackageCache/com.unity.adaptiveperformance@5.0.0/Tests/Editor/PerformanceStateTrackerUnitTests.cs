#if NUGET_MOQ_AVAILABLE

using System;
using Moq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AdaptivePerformance;
using UnityEngine.AdaptivePerformance.Provider;
using FrameTiming = UnityEngine.AdaptivePerformance.FrameTiming;

namespace UnityEditor.AdaptivePerformance.Editor.Tests
{
    public class PerformanceStateTrackerUnitTests
    {
        IAdaptivePerformance m_ap;
        IPerformanceStatus m_perfStat;
        FrameTiming ft;

        [SetUp]
        public void InitializeTest()
        {
            m_ap = Mock.Of<IAdaptivePerformance>();
            m_perfStat = Mock.Of<IPerformanceStatus>();
            ft = new FrameTiming();
        }

        [Test]
        public void StateAction_Stale_When_SampleCapacityZero()
        {
            BuildTestConditions(0f);
            Assert.AreEqual(StateAction.Stale, new MyPerformanceStateTracker(0).Update());
        }

        [TestCase((1f / 120), StateAction.Stale)]
        [TestCase((1f / 60), StateAction.Stale)]
        [TestCase((1f / 50), StateAction.Decrease)]
        [TestCase((1f / 45), StateAction.FastDecrease)]
        [TestCase((1f / 30), StateAction.FastDecrease)]
        [TestCase((1f / 15), StateAction.FastDecrease)]
        public void StateAction_When_AverageFrameTime(float averageFrameTime, StateAction stateAction)
        {
            BuildTestConditions(averageFrameTime);
            Assert.AreEqual(stateAction, new MyPerformanceStateTracker(100).Update());
        }

        [Test]
        public void CheckTrendValue_WithSingleUpdateCall_AndSampleCapacityIsZero()
        {
            BuildTestConditions(0f);
            PerformanceStateTracker testSubject = new MyPerformanceStateTracker(0);
            testSubject.Update();
            Assert.AreEqual(Double.NaN, testSubject.Trend);
        }

        [TestCase((1f / 120), 3, 5, -0.5f)]
        [TestCase((1f / 60), 3, 5, 0f)]
        [TestCase((1f / 50), 3, 5, 0.199999914f)]
        [TestCase((1f / 45), 3, 5, 0.333333284f)]
        [TestCase((1f / 30), 3, 5, 1f)]
        [TestCase((1f / 15), 3, 5, 3f)]
        public void CheckTrendValuesForVariousFrameTimes_WhenNumberOfUpdates_FewerThan_SampleCapacity_(float averageFrameTime, int numberOfUpdates, int sampleCapacity, float expectedTrendValue)
        {
            BuildTestConditions(averageFrameTime);
            PerformanceStateTracker testSubject = new MyPerformanceStateTracker(sampleCapacity);
            for (int i = 0; i < numberOfUpdates; i++)
            {
                testSubject.Update();
            }

            Assert.AreEqual(expectedTrendValue, testSubject.Trend);
        }

        [TestCase((1f / 120), 6, 5, -0.5f)]
        [TestCase((1f / 60), 6, 5, 0f)]
        [TestCase((1f / 50), 6, 5, 0.199999914f)]
        [TestCase((1f / 45), 6, 5, 0.333333284f)]
        [TestCase((1f / 30), 6, 5, 1f)]
        [TestCase((1f / 15), 6, 5, 3f)]
        [TestCase((1f / 120), 100, 97, -0.5f)]
        [TestCase((1f / 60), 700, 350, 0f)]
        [TestCase((1f / 50), 1000, 100, 0.200000018f)]
        [TestCase((1f / 45), 5000, 3000, 0.333325475f)]
        public void CheckTrendValuesForFrameTimesGreaterThanZero_AndNumberOfUpdates_GreaterThan_SampleCapacity_(float averageFrameTime, int numberOfUpdates, int sampleCapacity, float expectedTrendValue)
        {
            BuildTestConditions(averageFrameTime);
            PerformanceStateTracker testSubject = new MyPerformanceStateTracker(sampleCapacity);
            for (int i = 0; i < numberOfUpdates; i++)
            {
                testSubject.Update();
            }

            Assert.AreEqual(expectedTrendValue, testSubject.Trend);
        }


        [TestCase(1f, StateAction.Decrease, 10)]
        [TestCase(1000f, StateAction.FastDecrease, 10)]
        [TestCase(0.0000001f, StateAction.Stale, 10)]
        [TestCase(-0.999999f, StateAction.Stale, 10)]
        [TestCase(-0.999999f, StateAction.Stale, 0)] //sample capacity has no effect on StateAction outcome if avgFT is less than zero
        [TestCase(-0.999999f, StateAction.Stale, 500)] //sample capacity has no effect on StateAction outcome if avgFT is less than zero
        [TestCase(0f, StateAction.Stale, 10)]
        [TestCase(-9999.999999f, StateAction.Stale, 10)]
        public void CheckStateAction_When_AverageFrameTimeVaries(float averageFrameTime, StateAction stateAction, int sampleCapacity)
        {
            BuildTestConditions(averageFrameTime);
            Assert.AreEqual(stateAction, new SmallTFRPerformanceStateTracker(sampleCapacity).Update());
        }

        void BuildTestConditions(float averageFrameTime)
        {
            ft.AverageFrameTime = averageFrameTime;
            Mock.Get(m_perfStat).Setup(s => s.FrameTiming).Returns(ft);
            Mock.Get(m_ap).Setup(p => p.PerformanceStatus).Returns(m_perfStat);
            Holder.Instance = m_ap;
        }
    }

    partial class MyPerformanceStateTracker : PerformanceStateTracker
    {
        public MyPerformanceStateTracker(int sampleCapacity)
            : base(sampleCapacity) { }

        protected override float GetEffectiveTargetFrameRate()
        {
            return 60f;
        }
    }

    class SmallTFRPerformanceStateTracker : PerformanceStateTracker
    {
        public SmallTFRPerformanceStateTracker(int sampleCapacity)
            : base(sampleCapacity) { }

        protected override float GetEffectiveTargetFrameRate()
        {
            return 1.2f;
        }
    }
}

#endif
