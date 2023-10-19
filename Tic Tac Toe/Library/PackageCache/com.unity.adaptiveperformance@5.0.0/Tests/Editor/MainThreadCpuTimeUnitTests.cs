using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AdaptivePerformance;

namespace UnityEditor.AdaptivePerformance.Editor.Tests
{
    public class MainThreadCpuTimeUnitTests
    {
        MainThreadCpuTime testSubject;

        [SetUp]
        public void SetUp()
        {
            testSubject = new MainThreadCpuTime();
        }

        [Test]
        public void CheckCPUFrameTime_AfterInitialisation_WithoutReset()
        {
            Assert.AreEqual(-1, new MainThreadCpuTime().GetLatestResult());
        }

        [Test]
        public void CheckCPUFrameTime_WithMeasurement_WithoutReset()
        {
            testSubject.Measure();
            Assert.AreEqual(-1, testSubject.GetLatestResult());
        }
    }
}
