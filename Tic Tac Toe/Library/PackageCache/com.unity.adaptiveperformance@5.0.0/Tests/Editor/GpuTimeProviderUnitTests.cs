using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AdaptivePerformance;

namespace UnityEditor.AdaptivePerformance.Editor.Tests
{
    public class  GpuTimeProviderUnitTests
    {
        GpuTimeProvider belowTestSubject;
        GpuTimeProvider aboveTestSubject;

        [Test]
        public void VerifyFrameTime_WhenlatestTiming_IsBelowUnit()
        {
            Assert.AreEqual(-1.0f,new BelowUnitGpuTimeProvider().GpuFrameTime);
        }

        [Test]
        public void VerifyFrameTime_WhenlatestTiming_IsAboveUnit()
        {
            Assert.AreEqual(-1.0f,new AboveUnitGpuTimeProvider().GpuFrameTime);
        }

        internal class BelowUnitGpuTimeProvider : GpuTimeProvider
        {
            protected override uint GetLatestTimings()
            {
                return (uint)0.8;
            }
        }

        internal class AboveUnitGpuTimeProvider : GpuTimeProvider
        {
            protected override uint GetLatestTimings()
            {
                return (uint)1.7;
            }
        }
    }
}
