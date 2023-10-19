using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AdaptivePerformance;

namespace UnityEditor.AdaptivePerformance.Editor.Tests
{

    [TestFixture]
    public class CpuTimeProviderUnitTests
    {
        CpuTimeProvider multiThreadedTestSubject;
        CpuTimeProvider singleThreadedTestSubject;

        [SetUp]
        public void InitializeTests()
        {
            multiThreadedTestSubject = new MultithreadedTimeProvider();
            singleThreadedTestSubject = new SingleThreadedTimeProvider();
        }

        [Test]
        public void CheckCPUFrameTime_AfterInitialisation_WithoutReset_Multithreaded()
        {
            Assert.AreEqual(-1, multiThreadedTestSubject.CpuFrameTime);
        }

        [Test]
        public void CheckCPUFrameTime_AfterInitialisation_WithoutReset_Singlethreaded()
        {
            Assert.AreEqual(-1, singleThreadedTestSubject.CpuFrameTime);
        }

        [Test]
        public void CheckCPUFrameTime_ImmediatelyAfterReset_Multithreaded()
        {
            multiThreadedTestSubject.Reset();
            Assert.AreEqual(-1, multiThreadedTestSubject.CpuFrameTime);
        }

        [Test]
        public void CheckCPUFrameTime_ImmediatelyAfterReset_Singlethreaded()
        {
            singleThreadedTestSubject.Reset();
            Assert.AreEqual(-1, singleThreadedTestSubject.CpuFrameTime);
        }

        [Test]
        public void CheckCPUFrameTime_PerformLatestUpdate_Multithreaded()
        {
            multiThreadedTestSubject.LateUpdate();
            Assert.AreEqual(-1, multiThreadedTestSubject.CpuFrameTime);
        }

        [Test]
        public void CheckCPUFrameTime_PerformLatestUpdate_Singlethreaded()
        {
            singleThreadedTestSubject.LateUpdate();
            Assert.AreEqual(-1, singleThreadedTestSubject.CpuFrameTime);
        }

        [Test]
        public void CheckCPUFrameTime_EndOfFrameDetected_ThenPerformLatestUpdate_Multithreaded()
        {
            multiThreadedTestSubject.EndOfFrame();
            multiThreadedTestSubject.LateUpdate();
            Assert.AreEqual(-1, multiThreadedTestSubject.CpuFrameTime);
        }

        [Test]
        public void CheckCPUFrameTime_EndOfFrameDetected_ThenPerformLatestUpdate_Singlethreaded()
        {
            singleThreadedTestSubject.EndOfFrame();
            singleThreadedTestSubject.LateUpdate();
            Assert.AreEqual(-1, singleThreadedTestSubject.CpuFrameTime);
        }

        [Test]
        public void CheckCPUFrameTime_EndOfFrameDetected_NoUpdateInstruction_Multithreaded()
        {
            multiThreadedTestSubject.EndOfFrame();
            Assert.AreEqual(-1, multiThreadedTestSubject.CpuFrameTime);
        }

        [Test]
        public void CheckCPUFrameTime_EndOfFrameDetected_NoUpdateInstruction_Singlethreaded()
        {
            singleThreadedTestSubject.EndOfFrame();
            Assert.AreEqual(-1, singleThreadedTestSubject.CpuFrameTime);
        }
    }

    internal class MultithreadedTimeProvider : CpuTimeProvider
        {
            protected override bool IsGraphicsMultiThreaded()
            {
                return true;
            }
        }

    internal class SingleThreadedTimeProvider : CpuTimeProvider
    {
        protected override bool IsGraphicsMultiThreaded()
        {
            return false;
        }
    }
}
