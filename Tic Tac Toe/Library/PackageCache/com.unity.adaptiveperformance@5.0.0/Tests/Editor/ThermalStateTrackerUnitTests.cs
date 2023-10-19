#if NUGET_MOQ_AVAILABLE

using System;
using Moq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AdaptivePerformance;

namespace UnityEditor.AdaptivePerformance.Editor.Tests
{
    public class ThermalStateTrackerUnitTests
    {
        IAdaptivePerformance apStub;
        IThermalStatus thermalStatusStub;
        ThermalMetrics tmStub;

        [SetUp]
        public void Initialize()
        {
            apStub = Mock.Of<IAdaptivePerformance>();
            thermalStatusStub = Mock.Of<IThermalStatus>();
            tmStub = new ThermalMetrics();
            Mock.Get(apStub).Setup(h => h.ThermalStatus).Returns(thermalStatusStub);
            Holder.Instance = apStub;
        }

        [TestCase(StateAction.Increase, WarningLevel.NoWarning, 0f, 0f)]
        [TestCase(StateAction.FastDecrease, WarningLevel.Throttling,0f, 0f)]
        [TestCase(StateAction.Stale, WarningLevel.ThrottlingImminent,0f, 0f)]
        [TestCase(StateAction.FastDecrease, WarningLevel.ThrottlingImminent,0f, 0.6f)]
        [TestCase(StateAction.Decrease, WarningLevel.ThrottlingImminent,0f, 0.27f)]
        [TestCase(StateAction.Increase, WarningLevel.NoWarning, 0.8f, -0.4f)]
        [TestCase(StateAction.FastDecrease, WarningLevel.NoWarning, 0.8f, 0.67f)]
        [TestCase(StateAction.Decrease, WarningLevel.NoWarning, 0.8f, 0.28f)]
        public void VerifyStateAction_With_VaryingWarningLevelsUponUpdate(StateAction stateAction, WarningLevel warningLevel, float tempL, float trend)
        {
            tmStub = SetupThermalMetricsInstance(warningLevel, tempL, trend);
            Mock.Get(thermalStatusStub).Setup(t => t.ThermalMetrics).Returns(tmStub);
            Assert.AreEqual(stateAction, new ThermalStateTracker().Update());
        }

        ThermalMetrics SetupThermalMetricsInstance(WarningLevel wl, float tempLevel, float tempTrend)
        {
            tmStub.WarningLevel = wl;
            tmStub.TemperatureLevel = tempLevel;
            tmStub.TemperatureTrend = tempTrend;
            return tmStub;
        }
    }
}

#endif
