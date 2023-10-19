# Upgrading to version 5.0.0 of Adaptive Performance

This page describes how to upgrade from an older version of Adaptive Performance to version 5.0.0.

## Upgrading from Adaptive Performance 4.x, 3.x, 2.2.x

If you upgraded from any of the above versions and use a custom provider, you must upgrade the provider because this package has removed `com.unity.subsystemregistry` as a dependency.

### XRSubsystem

The [XRSubsystem](xref:UnityEngine.XR.ARSubsystems.XRSubsystem%601) is deprecated and Adaptive Performance has removed it as a dependency. If you use a custom provider that used XRSubsystem, you should use [SubsystemWithProvider](xref:UnityEngine.SubsystemsImplementation.SubsystemWithProvider) instead. This is the new Subsystem base class in Unity core and it requires an implementation of [SubsystemDescriptorWithProvider](xref:UnityEngine.SubsystemsImplementation.SubsystemDescriptorWithProvider) and [SubsystemProvider](xref:UnityEngine.SubsystemsImplementation.SubsystemProvider).

- Implementing a subsystem using deprecated APIs:

```c#
public class TestSubsystemDescriptor : SubsystemDescriptor<TestSubsystem>
{ }

public class TestSubsystem : XRSubsystem<TestSubsystemDescriptor>
{
    protected override void OnStart() { }

    protected override void OnStop() { }

    protected override void OnDestroyed() { }
}
```

- Implementing a subsystem using the new APIs:

```c#
public class TestSubsystemDescriptor : SubsystemDescriptorWithProvider<TestSubsystem, TestSubsystemProvider>
{ }

public class TestSubsystem : SubsystemWithProvider<TestSubsystem, TestSubsystemDescriptor, TestSubsystemProvider>
{ }

public class TestSubsystemProvider : SubsystemProvider<TestSubsystem>
{
    public override void Start() { }

    public override void Stop() { }

    public override void Destroy() { }
}
```

### Lifecycle Management Controls

With Adaptive Performance 5.0.0, APIs are available to allow for more control over the state and lifecycle of Adaptive Performance.

In earlier versions, you could start/stop Adaptive Performance via the Loader or via the active state of the attached `GameObject`. The APIs make this process simpler and ensure that the appropriate internal values are in the correct state. Additionally, the APIs make it possible to shut down and re-initialize Adaptive Performance, which was previously impossible.

If Adaptive Performance isn't initialized on startup, or if you previously deinitialized it, you can call `Holder.Initialize();` to trigger the initialization process. This initializes Adaptive Performance but does not start it. To start Adaptive Performance, call `Holder.Instance.StartAdaptivePerformance();`. To stop Adaptive Performance, call `Holder.Instance.StopAdaptivePerformance();`. To trigger the deinitialization process, call `Holder.Deinitialize();`.

The details are demonstrated in the [Lifecycle Management](samples-guide.md#lifecycle-management) sample.

## Upgrading from Adaptive Performance 2.x.x

If you upgrade from any version before 2.2.0, read the 2.2.0 [upgrade guide](upgrade-guide-2-2-0.md) because the same updates apply.
