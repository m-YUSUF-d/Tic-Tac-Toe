using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AdaptivePerformance;

namespace UnityEditor.AdaptivePerformance.Editor.Tests
{
    public class TemperatureTrendUnitTests
    {
        TemperatureTrend tt;

        [SetUp]
        public void SetupTest()
        {
            tt = new TemperatureTrend(false);
        }

        [Test]
        public void CheckThermalTrend_SameAsProvTrend_WhenProviderTrendUsed()
        {
            var tt = new TemperatureTrend(true);
            tt.Update(0.55f,0.0f,false,0.0f);
            Assert.AreEqual(0.55f, tt.ThermalTrend);
        }

        [Test]
        public void CheckThermalTrend_WhenProviderTrendNotUsed_AndNumValues_Zero()
        {
            tt.Update(0.3f,0.45f,false,0.0f);
            Assert.AreEqual(0.0f, tt.ThermalTrend);
        }

        [Test]
        public void CheckThermalTrend_WhenProviderTrendNotUsed_And_TrendNotUpdated()
        {
            tt.NumValues = 1;
            tt.Update(0.3f,0.67f,false,0.8f);
            Assert.AreEqual(1.0f, tt.ThermalTrend);
        }

        [Test]
        public void CheckThermalTrendIsPositive_TimestampDiffGreaterThanMeasurementTimeframe()
        {
            tt.NumValues = 3;
            tt.Update(0.3f,0.45f,false,25f);
            Assert.AreEqual(0.728999913f, tt.ThermalTrend);
        }

        [Test]
        public void CheckThermalTrendIsNegative_TimestampDiffGreaterThanMeasurementTimeframe()
        {
            tt.NumValues = 3;
            tt.Update(0.3f,-2f,false,0.42f);
            Assert.AreEqual(-1f, tt.ThermalTrend);
        }

        [Test]
        public void CheckThermalTrend_After_10ConsecutiveCalls_NumValuesNotSameAsSample()
        {
            tt.NumValues =100;
            float newTemp = 0.7f;
            float newTempStamp = 0.5f;
            for (int i = 0; i < 10; i++)
            {
                tt.Update(0f, newTemp, true, newTempStamp);
                newTemp += 0.2f;
                newTempStamp += 0.1f;
            }

            Assert.AreEqual(1.0f, tt.ThermalTrend);
        }

        [Test]
        public void CheckThermalTrend_After_50ConsecutiveCalls_With_Intermediate_Reset()
        {
            tt.NumValues = 150;
            for (int i = 0; i < 38; i++)
            {
                tt.Update(0f, 10, true, 5);
            }

            tt.Reset();
            float NewTemp = 0.10f;
            float newTempStamp = 0.11f;
            for (int i = 0; i < 13; i++)
            {
                tt.Update(0f, NewTemp, true, newTempStamp);
                NewTemp += 0.1f;
                newTempStamp += 0.1f;
            }

            Assert.AreEqual(1.0f, tt.ThermalTrend);
        }

        [Test]
        public void CheckThermalTrend_After_100ConsecutiveCalls_WhenSampleSizeSameAsNumValues()
        {
            tt.NumValues =200;
            float newTemp = 0.10f;
            float newTempStamp = 0.4f;
            for (int i = 0; i < 100; i++)
            {
                tt.Update(0f, newTemp, true, newTempStamp);
                newTemp += 0.1f;
                newTempStamp += 0.1f;
            }

            Assert.AreEqual(1.0f, tt.ThermalTrend);
        }

        [Test]
        public void CheckThermalTrend_After_800ConsecutiveCalls_WhenSampleSizeSameAsNumValues()
        {
            tt.NumValues =0;
            float newTemp = 0.44f;
            float newTempStamp = 0.12f;
            for (int i = 0; i < 800; i++)
            {
                tt.Update(0f, newTemp, false, newTempStamp);
                newTemp += 0.01f;
                newTempStamp += 0.02f;
            }

            Assert.AreEqual(1.0f, tt.ThermalTrend);
        }

        [Test]
        public void CheckThermalTrend_AfterReset()
        {
            tt.NumValues =7;
            tt.Update(0f, 0.5f, true, 0.6f);
            tt.Reset();
            Assert.AreEqual(0.0f, tt.ThermalTrend);
        }
    }
}
