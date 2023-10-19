#if NUGET_MOQ_AVAILABLE

using System;
using Moq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AdaptivePerformance;
using UnityEngine.AdaptivePerformance.Provider;

namespace UnityEditor.AdaptivePerformance.Editor.Tests
{

    public class DevicePerformanceControlImplUnitTests
    {
        PerformanceLevelChangeEventArgs pce;
        DevicePerformanceControlImpl testSubject;
        IDevicePerformanceLevelControl performanceLevelControl;
        int somecpu = 100;
        int somegpu = -1;

        [SetUp]
        public void InitializeTests()
        {
            pce = new PerformanceLevelChangeEventArgs();
            performanceLevelControl = Mock.Of<IDevicePerformanceLevelControl>();
            testSubject = new DevicePerformanceControlImpl(performanceLevelControl);
        }

        [Test]
        public void UpdateDoesNotOccur_When_ControlModeIsNotSystem_And_CPULevelUnknown()
        {
            Assert.AreEqual(false, testSubject.Update(out pce));
        }

        [Test]
        public void UpdateDoesNotOccur_When_ControlModeIsSystem_And_CPULevelUnknown()
        {
            testSubject.PerformanceControlMode = PerformanceControlMode.System;
            Assert.AreEqual(false, testSubject.Update(out pce));
        }

        [Test]
        public void UpdateDoesNotOccur_When_ControlModeIsSystem_And_CPULevelSet_DefaultAverage()
        {
            testSubject.CurrentCpuLevel = Constants.DefaultAverageFrameCount;
            testSubject.PerformanceControlMode = PerformanceControlMode.System;
            testSubject.Update(out pce);
            Assert.AreEqual(Constants.UnknownPerformanceLevel, pce.CpuLevel);
        }

        [Test]
        public void UpdateDoesNotOccur_When_ControlModeIsNotSystem_And_CPULevelIsAverageFramerate()
        {
            testSubject.CpuLevel = Constants.DefaultAverageFrameCount;
            testSubject.PerformanceControlMode = PerformanceControlMode.Manual;
            testSubject.Update(out pce);
            Assert.AreEqual(false, testSubject.Update(out pce));
        }

        [Test]
        public void UpdateDoesOccur_When_ControlModeIsManual_And_SettingPerformanceLevelSuccessful()
        {
            Mock.Get(performanceLevelControl).Setup(p => p.SetPerformanceLevel(ref somecpu, ref somegpu)).Returns(true);
            testSubject.CpuLevel = Constants.DefaultAverageFrameCount;
            testSubject.PerformanceControlMode = PerformanceControlMode.Manual;
            Assert.AreEqual(true, testSubject.Update(out pce));
        }

        [Test]
        public void UpdateDoesOccur_When_ControlModeIsManual_And_SettingPerformanceLevelNotSuccessful()
        {
            Mock.Get(performanceLevelControl).Setup(p => p.SetPerformanceLevel(ref somecpu, ref somegpu)).Returns(false);
            testSubject.CpuLevel = Constants.DefaultAverageFrameCount;
            testSubject.PerformanceControlMode = PerformanceControlMode.Manual;
            Assert.AreEqual(false, testSubject.Update(out pce));
        }
    }
}

#endif
