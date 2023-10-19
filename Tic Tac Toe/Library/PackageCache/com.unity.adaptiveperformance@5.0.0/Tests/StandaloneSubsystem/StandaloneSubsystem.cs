using System;
using UnityEngine.AdaptivePerformance.Provider;
using UnityEngine.SubsystemsImplementation;

namespace UnityEngine.AdaptivePerformance.Tests.Standalone
{
    public class StandaloneSubsystem : AdaptivePerformanceSubsystem
    {
        public event Action startCalled;
        public event Action stopCalled;
        public event Action destroyCalled;

        public StandaloneSubsystem() { }

        protected override void OnStart()
        {
            base.OnStart();
            if (startCalled != null)
                startCalled.Invoke();
        }

        protected override void OnStop()
        {
            base.OnStop();
            if (stopCalled != null)
                stopCalled.Invoke();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (destroyCalled != null)
                destroyCalled.Invoke();
        }

        public override IApplicationLifecycle ApplicationLifecycle => provider.ApplicationLifecycle;
        public override IDevicePerformanceLevelControl PerformanceLevelControl => provider.PerformanceLevelControl;
        public override Version Version => provider.Version;
        public override Feature Capabilities { get => provider.Capabilities; protected set => provider.Capabilities = value; }
        public override string Stats => provider.Stats;
        public override bool Initialized { get => provider.Initialized; protected set => provider.Initialized = value; }
        public override PerformanceDataRecord Update()
        {
            return provider.Update();
        }
        public class StandaloneProvider : APProvider, IApplicationLifecycle
        {
            override public IApplicationLifecycle ApplicationLifecycle { get { return this; } }
            override public IDevicePerformanceLevelControl PerformanceLevelControl { get { return null; } }

            public StandaloneProvider() { }
            public override void Start() {}

            public override void Stop()  {}

            public override void Destroy() {}

            public override string Stats => $"";
            override public PerformanceDataRecord Update() { return new PerformanceDataRecord(); }

            public override Version Version
            {
                get
                {
                    return new Version("5.0.0");
                }
            }
            public override Feature Capabilities { get; set; }
            public override bool Initialized { get; set; }

            public void ApplicationPause() { }

            public void ApplicationResume() { }
        }
    }
}
