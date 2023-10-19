using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AdaptivePerformance;

namespace UnityEditor.AdaptivePerformance.Editor.Tests
{
    public class BottleneckUtilUnitTests
    {

        [Test]
        public void CheckTargetFramerateAchieved_WhenLimitNotReached()
        {
            Assert.AreEqual(PerformanceBottleneck.TargetFrameRate, BottleneckUtil.DetermineBottleneck(PerformanceBottleneck.GPU, 0, 0, 1, 3));
        }

        [Test]
        public void CheckTargetFramerate_GPURate_EqualTo_AverageRate()
        {
            Assert.AreEqual(PerformanceBottleneck.GPU, BottleneckUtil.DetermineBottleneck(PerformanceBottleneck.GPU, 0, 0, 0, 0));
        }

        [Test]
        public void CheckTargetFramerate_GPURate_GreaterThan_AverageRate()
        {
            Assert.AreEqual(PerformanceBottleneck.GPU, BottleneckUtil.DetermineBottleneck(PerformanceBottleneck.GPU, 0, 1, 0, 0));
        }

        [Test]
        public void CheckTargetFramerate_CPURate_EqualTo_AverageRate()
        {
            Assert.AreEqual(PerformanceBottleneck.CPU, BottleneckUtil.DetermineBottleneck(PerformanceBottleneck.GPU, 1, 0, 1, 0));
        }

        [Test]
        public void CheckTargetFramerate_CPURate_GreaterThan_AverageRate()
        {
            Assert.AreEqual(PerformanceBottleneck.CPU, BottleneckUtil.DetermineBottleneck(PerformanceBottleneck.GPU, 2, 0, 1, 0));
        }

        [Test]
        public void CheckTargetFramerate_CPUUtilization_GreaterThan_HighCPUThreshold()
        {
            Assert.AreEqual(PerformanceBottleneck.CPU, BottleneckUtil.DetermineBottleneck(PerformanceBottleneck.GPU, 2, 0, 2.1f, 0));
        }

        [Test]
        public void CheckTargetFramerate_AverageGPU_GreaterThan_HighCPUThreshold()
        {
            Assert.AreEqual(PerformanceBottleneck.GPU, BottleneckUtil.DetermineBottleneck(PerformanceBottleneck.GPU, 2, 1, 3, 0));
        }

        [Test]
        public void CheckTargetFramerate_AverageGPU_GreaterThan_AverageCPU()
        {
            Assert.AreEqual(PerformanceBottleneck.GPU, BottleneckUtil.DetermineBottleneck(PerformanceBottleneck.GPU, 0.4f, 0.5f, 0.55f, 0));
        }

        [Test]
        public void CheckTargetFramerate_AverageGPU_NotGreaterThan_AverageCPU_CPUFactorTooSmall()
        {
            Assert.AreEqual(PerformanceBottleneck.Unknown, BottleneckUtil.DetermineBottleneck(PerformanceBottleneck.GPU, 0.3f, 0.29f, 0.4f, 0));
        }

        [Test]
        public void CheckTargetFramerateUnknown_WhenLimitReached_NoChecksApply()
        {
            Assert.AreEqual(PerformanceBottleneck.Unknown, BottleneckUtil.DetermineBottleneck(PerformanceBottleneck.GPU, 0, 0, 1, 0));
        }
    }
}
