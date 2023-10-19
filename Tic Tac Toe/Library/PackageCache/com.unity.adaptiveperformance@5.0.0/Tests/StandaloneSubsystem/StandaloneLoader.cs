using System.Collections.Generic;
using UnityEngine.AdaptivePerformance.Provider;

namespace UnityEngine.AdaptivePerformance.Tests.Standalone
{
    public class StandaloneLoader : AdaptivePerformanceLoaderHelper
    {
        static List<AdaptivePerformanceSubsystemDescriptor> s_StandaloneSubsystemDescriptors = new List<AdaptivePerformanceSubsystemDescriptor>();

        public override bool Initialized
        {
            get { return standaloneSubsystem != null; }
        }

        public override bool Running
        {
            get { return standaloneSubsystem != null && standaloneSubsystem.running; }
        }

        public StandaloneSubsystem standaloneSubsystem
        {
            get
            {
                return GetLoadedSubsystem<StandaloneSubsystem>();
            }
        }

        public bool started { get; protected set; }
        public bool stopped { get; protected set; }
        public bool deInitialized { get; protected set; }

        void OnStartCalled()
        {
            started = true;
        }

        void OnStopCalled()
        {
            stopped = true;
        }

        void OnDestroyCalled()
        {
            deInitialized = true;
        }

        public override ISubsystem GetDefaultSubsystem()
        {
            return GetLoadedSubsystem<StandaloneSubsystem>();
        }

        public override IAdaptivePerformanceSettings GetSettings()
        {
            return null;
        }

        public override bool Initialize()
        {
            started = false;
            stopped = false;
            deInitialized = false;

            CreateSubsystem<AdaptivePerformanceSubsystemDescriptor, StandaloneSubsystem>(s_StandaloneSubsystemDescriptors, "Standalone Subsystem");
            if (standaloneSubsystem == null)
                return false;

            standaloneSubsystem.startCalled += OnStartCalled;
            standaloneSubsystem.stopCalled += OnStopCalled;
            standaloneSubsystem.destroyCalled += OnDestroyCalled;
            return true;
        }

        public override bool Start()
        {
            if (standaloneSubsystem != null)
                StartSubsystem<StandaloneSubsystem>();
            return true;
        }

        public override bool Stop()
        {
            if (standaloneSubsystem != null)
                StopSubsystem<StandaloneSubsystem>();
            return true;
        }

        public override bool Deinitialize()
        {
            DestroySubsystem<StandaloneSubsystem>();
            if (standaloneSubsystem != null)
            {
                standaloneSubsystem.startCalled -= OnStartCalled;
                standaloneSubsystem.stopCalled -= OnStopCalled;
                standaloneSubsystem.destroyCalled -= OnDestroyCalled;
            }
            return base.Deinitialize();
        }
    }
}
