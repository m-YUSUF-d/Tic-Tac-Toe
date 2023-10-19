using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AdaptivePerformance;

namespace UnityEditor.AdaptivePerformance.Editor.Tests
{
    public class RunningAverageUnitTests
    {
        RunningAverage runAverage;

        [SetUp]
        public void SetupTest()
        {
            runAverage = new RunningAverage();
        }

        [Test]
        public void CheckSampleSizeWindowWhenDefaultIsUsed()
        {
            Assert.AreEqual(100, runAverage.GetSampleWindowSize());
        }

        [Test]
        public void CheckSampleSizeWindowWhenSet()
        {
            Assert.AreEqual(4000, new RunningAverage(4000).GetSampleWindowSize());
        }

        [Test]
        public void CheckAverageWhenNoSampleWindowSizeSet_And_NoNewValueGiven()
        {
            runAverage.AddValue(0);
            Assert.AreEqual(1, runAverage.GetNumValues());
        }

        [Test]
        public void CheckAverage_NoSampleSize_And_NewValuesToAddGiven()
        {
            runAverage.AddValue(110);
            Assert.AreEqual(110, runAverage.GetAverageOr(1));
        }

        [Test]
        public void CheckAverage_SampleSizeGiven_And_NewValuesToAddGiven()
        {
            RunningAverage rv = new RunningAverage(230);
            rv.AddValue(156);
            Assert.AreEqual(156, rv.GetAverageOr(90));
        }

        [Test]
        public void CheckMostRecentVal_NoSampleSize_And_NewValuesToAddGiven()
        {
            runAverage.AddValue(249);
            Assert.AreEqual(249, runAverage.GetMostRecentValueOr(24));
        }

        [Test]
        public void CheckMostRecentVal_SampleSizeGiven_And_NewValuesToAddGiven()
        {
            RunningAverage rv = new RunningAverage(220);
            rv.AddValue(973);
            Assert.AreEqual(973, rv.GetMostRecentValueOr(23));
        }

        [Test]
        public void ResetValues_CheckNumberOfValues()
        {
            RunningAverage rv = new RunningAverage(7532);
            rv.Reset();
            Assert.AreEqual(0, rv.GetNumValues());
        }

        [Test]
        public void ResetValues_CheckSampleSizeRemainsSame()
        {
            RunningAverage rv = new RunningAverage(2452);
            rv.Reset();
            Assert.AreEqual(2452, rv.GetSampleWindowSize());
        }

        [Test]
        public void ResetValues_AltersNumValuesAfterMultipleCalls()
        {
            var rv = SetupConsecutiveCalls();
            rv.Reset();
            Assert.AreEqual(0f, rv.GetNumValues());
        }

        [Test]
        public void SampleSizeStore_SizeRemainsSame_AfterReset()
        {
            var rv = SetupConsecutiveCalls();
            rv.Reset();
            Assert.AreEqual(15, rv.GetSampleWindowSize());
        }

        [Test]
        public void VerifyNewValuesAreStored_UponConsecutiveCalls()
        {
            Assert.AreEqual(15, SetupConsecutiveCalls().GetSampleWindowSize());
        }

        [Test]
        public void VerifyMostRecentlyAddedValue_UponConsecutiveCalls()
        {
            Assert.AreEqual(1f, SetupConsecutiveCalls().GetMostRecentValueOr(0f));
        }

        [Test]
        public void VerifyAverageOrValue_50_ConsecutiveCalls()
        {
            Assert.AreEqual(25.5f, AddValuesMultipleTimes(50).GetAverageOr(0f));
        }

        [Test]
        public void VerifyNumValues_50_ConsecutiveCalls()
        {
            Assert.AreEqual(50, AddValuesMultipleTimes(50).GetNumValues());
        }

        [Test]
        public void VerifyAverageOrValue_100_ConsecutiveCalls()
        {
            Assert.AreEqual(50.5f, AddValuesMultipleTimes(100).GetAverageOr(0f));
        }

        [Test]
        public void VerifyNumValues_999_ConsecutiveCalls()
        {
            Assert.AreEqual(999, AddValuesMultipleTimes(999).GetNumValues());
        }

        [Test]
        public void VerifyAverageOrValue_999_ConsecutiveCalls()
        {
            Assert.AreEqual(500.0f, AddValuesMultipleTimes(999).GetAverageOr(0f));
        }

        [Test]
        public void VerifyNumValues_100_ConsecutiveCalls()
        {
            Assert.AreEqual(100, AddValuesMultipleTimes(100).GetNumValues());
        }

        private RunningAverage AddValuesMultipleTimes(int cycles)
        {
            RunningAverage rvInstance = new RunningAverage(cycles);
            for (int i = 1; i < cycles+1; i++) {
                rvInstance.AddValue(i);
            }

            return rvInstance;
        }

        private RunningAverage SetupConsecutiveCalls()
        {
            RunningAverage avgInstance = new RunningAverage(15);
            foreach (int i in new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f })
            {
                avgInstance.AddValue(i);
            }

            return avgInstance;
        }

    }

}
