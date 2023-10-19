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
    public class AdaptivePerformanceScalerEfficiencyTrackerUnitTests
    {
        FrameTiming ft;
        IAdaptivePerformance m_ap;
        IPerformanceStatus m_perfStat;
        AdaptivePerformanceScaler m_perfScaler;
        AdaptivePerformanceScalerEfficiencyTracker testSubject;

        [SetUp]
        public void startFixture()
        {
            ft = new FrameTiming();
            m_ap = Mock.Of<IAdaptivePerformance>();
            m_perfStat = Mock.Of<IPerformanceStatus>();
            m_perfScaler = Mock.Of<AdaptivePerformanceScaler>();
            Mock.Get(m_perfStat).Setup(s => s.FrameTiming).Returns(ft);
            Mock.Get(m_ap).Setup(p => p.PerformanceStatus).Returns(m_perfStat);
            Holder.Instance = m_ap;
            testSubject = new AdaptivePerformanceScalerEfficiencyTracker();
        }

        [Test]
        public void ScalarNotRunning_WhenTracker_NeitherStarted_OrStopped()
        {
            Assert.AreEqual(false, testSubject.IsRunning);
        }

        [Test]
        public void ScalarNotRunning_WhenStarted_ButProvidedScalerNotInitialized()
        {
            testSubject.Start(null, false);
            Assert.AreEqual(false, testSubject.IsRunning);
        }

        [Test]
        public void ScalarRunning_WhenStarted_ProvidedScalerInitialized()
        {
            testSubject.Start(m_perfScaler, false);
            Assert.AreEqual(true, testSubject.IsRunning);
        }

        [Test]
        public void ScalarNotRunning_WhenStartedThenStopped_ProvidedScalerInitialized()
        {
            testSubject.Start(m_perfScaler, false);
            testSubject.Stop();
            Assert.AreEqual(false, testSubject.IsRunning);
        }

        [Test]
        public void ScalarNotRunning_WhenonlyStopped()
        {
            Assert.Throws<NullReferenceException>(StoppingScalarWithoutRunning);
        }

        void StoppingScalarWithoutRunning()
        {
            testSubject.Stop();
        }
    }
}

#endif
