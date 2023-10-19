# What's new in version 5.0.0
Summary of changes in Adaptive Performance package version 5.0.0.

The main updates in this release include:

## Removed
* Dependency on com.unity.subsystemregistration

## Added
* Adaptive Performance Subsystem is using the internal Subsystems module now and removed the subsystem registration. This introduces an internal APProvider class.
* New APIs for controlling the lifecycle of Adaptive Performance.
* New `IPerformanceModeStatus` for retrieving performance mode and listening to performance mode changes.
* Support for new the [Android](https://docs.unity3d.com/Packages/com.unity.adaptiveperformance.google.android@latest/index.html) Adaptive Performance provider.
* New property on `IAdaptivePerformance` to access the active subsystem.

## Updated
* Documentation

## Fixed

For a full list of changes and updates in this version, see the [Adaptive Performance package changelog](../changelog/CHANGELOG.html).
