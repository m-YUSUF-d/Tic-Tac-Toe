#if NUGET_MOQ_AVAILABLE

using System;
using Moq;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AdaptivePerformance;

namespace UnityEditor.AdaptivePerformance.Editor.Tests
{
    public class AdaptivePerformanceLoaderHelperUnitTests : AdaptivePerformanceLoaderHelper
    {
        [Test]
        public void CreateSubsystemWithNullDescriptorsProvided_ExceptionThrown()
        {
            try
            {
                CreateSubsystem<ISubsystemDescriptor, ISubsystem>(null, "something");
                Assert.Fail("Meant to throw an Exception of Type 'ArgumentNull': ");
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual(0, m_SubsystemInstanceMap.Count);
            }
        }

        [Test]
        public void CreateSubsystemWithEmptyDescriptorsProvided_NoExceptionThrown()
        {
            CreateSubsystem<ISubsystemDescriptor, ISubsystem>(new List<ISubsystemDescriptor>(), "something");
            Assert.AreEqual(0, m_SubsystemInstanceMap.Count);
        }

        [Test]
        public void CreateSubsystemWithAtLeastOneDescriptor_NoExceptionThrown()
        {
            List<ISubsystemDescriptor> sdList = new List<ISubsystemDescriptor>();
            sdList.Add(Mock.Of<ISubsystemDescriptor>());
            CreateSubsystem<ISubsystemDescriptor, ISubsystem>(sdList, "something");
            Assert.AreEqual(0, m_SubsystemInstanceMap.Count);
        }

        public override bool Initialized { get; }
        public override bool Running { get; }

        public override ISubsystem GetDefaultSubsystem()
        {
            return null;
        }

        public override IAdaptivePerformanceSettings GetSettings()
        {
            return null;
        }
    }
}

#endif
