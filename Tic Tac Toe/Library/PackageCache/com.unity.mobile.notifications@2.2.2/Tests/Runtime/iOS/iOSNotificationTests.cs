using System;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using Unity.Notifications.iOS;
#if UNITY_EDITOR
using Unity.Notifications;
using UnityEditor;
#endif

class iOSNotificationTests
    : IPrebuildSetup, IPostBuildCleanup
{
    private static int receivedNotificationCount = 0;
    private static iOSNotification lastReceivedNotification = null;
#if UNITY_EDITOR
    private static iOSSdkVersion originaliOSSDK;
    private static bool originalRequestAuthorizationOnAppLaunch;
    private static AuthorizationOption originalAuthorizationOptions;
    private static bool originalAddRemoteNotificationCapability;
    private static bool originalRequestRemoteOnLaunch;
#endif

    public void Setup()
    {
#if UNITY_EDITOR
        originaliOSSDK = PlayerSettings.iOS.sdkVersion;
        originalRequestAuthorizationOnAppLaunch = NotificationSettings.iOSSettings.RequestAuthorizationOnAppLaunch;
        originalAuthorizationOptions = NotificationSettings.iOSSettings.DefaultAuthorizationOptions;
        originalAddRemoteNotificationCapability = NotificationSettings.iOSSettings.AddRemoteNotificationCapability;
        originalRequestRemoteOnLaunch = NotificationSettings.iOSSettings.NotificationRequestAuthorizationForRemoteNotificationsOnAppLaunch;

        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.SimulatorSDK;
        NotificationSettings.iOSSettings.RequestAuthorizationOnAppLaunch = true;
        NotificationSettings.iOSSettings.DefaultAuthorizationOptions = originalAuthorizationOptions | AuthorizationOption.Provisional;
        NotificationSettings.iOSSettings.AddRemoteNotificationCapability = false;
        NotificationSettings.iOSSettings.NotificationRequestAuthorizationForRemoteNotificationsOnAppLaunch = false;
#endif
    }

    public void Cleanup()
    {
#if UNITY_EDITOR
        PlayerSettings.iOS.sdkVersion = originaliOSSDK;
        NotificationSettings.iOSSettings.RequestAuthorizationOnAppLaunch = originalRequestAuthorizationOnAppLaunch;
        NotificationSettings.iOSSettings.DefaultAuthorizationOptions = originalAuthorizationOptions;
        NotificationSettings.iOSSettings.AddRemoteNotificationCapability = originalAddRemoteNotificationCapability;
        NotificationSettings.iOSSettings.NotificationRequestAuthorizationForRemoteNotificationsOnAppLaunch = originalRequestRemoteOnLaunch;
#endif
    }

#if !UNITY_EDITOR
    [OneTimeSetUp]
    public void BeforeTests()
    {
        iOSNotificationCenter.OnNotificationReceived += receivedNotification =>
        {
            receivedNotificationCount += 1;
            lastReceivedNotification = receivedNotification;
            var msg = "Notification received : " + receivedNotification.Identifier + "\n";
            msg += "\n Notification received: ";
            msg += "\n .Title: " + receivedNotification.Title;
            msg += "\n .Badge: " + receivedNotification.Badge;
            msg += "\n .Body: " + receivedNotification.Body;
            msg += "\n .CategoryIdentifier: " + receivedNotification.CategoryIdentifier;
            msg += "\n .Subtitle: " + receivedNotification.Subtitle;
            Debug.Log(msg);
        };
    }

    [TearDown]
    public void AfterEachTest()
    {
        receivedNotificationCount = 0;
        lastReceivedNotification = null;
        iOSNotificationCenter.RemoveAllScheduledNotifications();
    }
#endif

    IEnumerator WaitForNotification(float timeout)
    {
        var startCount = receivedNotificationCount;
        float timePassed = 0;
        while (receivedNotificationCount == startCount && timePassed < timeout)
        {
            yield return null;
            timePassed += Time.deltaTime;
        }
    }

    [UnityTest]
    [UnityPlatform(RuntimePlatform.IPhonePlayer)]
    public IEnumerator SendSimpleNotification_NotificationIsReceived()
    {
        var timeTrigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = new TimeSpan(0, 0, 5),
            Repeats = false
        };

        // You can optionally specify a custom Identifier which can later be
        // used to cancel the notification, if you don't set one, an unique
        // string will be generated automatically.
        var notification = new iOSNotification()
        {
            Identifier = "_notification_01",
            Title = "SendSimpleNotification_NotificationIsReceived",
            Body = "Scheduled at: " + DateTime.Now.ToShortDateString() + " triggered in 5 seconds",
            Subtitle = "This is a subtitle, something, something important...",
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert |
                PresentationOption.Sound),
            CategoryIdentifier = "category_a",
            ThreadIdentifier = "thread1",
            Trigger = timeTrigger,
        };

        iOSNotificationCenter.ScheduleNotification(notification);

        yield return WaitForNotification(10.0f);
        Assert.AreEqual(1, receivedNotificationCount);
    }

    [UnityTest]
    [UnityPlatform(RuntimePlatform.IPhonePlayer)]
    public IEnumerator SendNotificationWithUserInfo_NotificationIsReceivedWithSameUserInfo()
    {
        var timeTrigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = new TimeSpan(0, 0, 5),
            Repeats = false
        };

        var notification = new iOSNotification()
        {
            Identifier = "_notification_02",
            Title = "SendNotificationWithUserInfo_NotificationIsReceivedWithSameUserInfo",
            Body = "Scheduled at: " + DateTime.Now.ToShortDateString() + " triggered in 5 seconds",
            Subtitle = "This is a subtitle, something, something important...",
            ShowInForeground = true,
            ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
            CategoryIdentifier = "category_a",
            ThreadIdentifier = "thread1",
            Trigger = timeTrigger,
        };

        notification.UserInfo.Add("key1", "value1");

        iOSNotificationCenter.ScheduleNotification(notification);

        yield return WaitForNotification(10.0f);
        Assert.AreEqual(1, receivedNotificationCount);
        Assert.IsNotNull(lastReceivedNotification);
        Assert.IsTrue(lastReceivedNotification.UserInfo.ContainsKey("key1"));
        Assert.AreEqual("value1", lastReceivedNotification.UserInfo["key1"]);
    }

    IEnumerator SendNotificationUsingCalendarTrigger_NotificationIsReceived(string text, bool useUtc)
    {
        var dateTime = useUtc ? DateTime.UtcNow : DateTime.Now;
        var dt = dateTime.AddSeconds(5);
        var trigger = new iOSNotificationCalendarTrigger()
        {
            Year = dt.Year,
            Month = dt.Month,
            Day = dt.Day,
            Hour = dt.Hour,
            Minute = dt.Minute,
            Second = dt.Second,
            UtcTime = useUtc,
        };

        var notification = new iOSNotification()
        {
            Title = text,
            Body = text,
            ShowInForeground = true,
            ForegroundPresentationOption = PresentationOption.Alert,
            Trigger = trigger,
        };

        iOSNotificationCenter.ScheduleNotification(notification);
        Debug.Log($"SendNotificationUsingCalendarTrigger_NotificationIsReceived, Now: {dateTime}, Notification should arrive on: {dt}");
        yield return WaitForNotification(20.0f);
        Debug.Log($"SendNotificationUsingCalendarTrigger_NotificationIsReceived, wait finished at: {DateTime.Now}");
        Assert.AreEqual(1, receivedNotificationCount);
        Assert.IsNotNull(lastReceivedNotification);
        Assert.AreEqual(text, lastReceivedNotification.Title);
        var retTrigger = (iOSNotificationCalendarTrigger)lastReceivedNotification.Trigger;
        Assert.AreEqual(useUtc, retTrigger.UtcTime);
    }

    [UnityTest]
    [UnityPlatform(RuntimePlatform.IPhonePlayer)]
    public IEnumerator SendNotificationUsingCalendarTriggerLocalTime_NotificationIsReceived()
    {
        yield return SendNotificationUsingCalendarTrigger_NotificationIsReceived("SendNotificationUsingCalendarTriggerLocalTime_NotificationIsReceived", false);
    }

    [UnityTest]
    [UnityPlatform(RuntimePlatform.IPhonePlayer)]
    public IEnumerator SendNotificationUsingCalendarTriggerUtcTime_NotificationIsReceived()
    {
        yield return SendNotificationUsingCalendarTrigger_NotificationIsReceived("SendNotificationUsingCalendarTriggerUtcTime_NotificationIsReceived", true);
    }

    [UnityTest]
    [UnityPlatform(RuntimePlatform.IPhonePlayer)]
    public IEnumerator SendNotification_AllPropertiesRoundtrip()
    {
        var trigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = new TimeSpan(0, 0, 5),
            Repeats = false,
        };

        var notification = new iOSNotification()
        {
            Trigger = trigger,
            Identifier = "roundtrip",
            CategoryIdentifier = "category",
            ThreadIdentifier = "thread",
            Title = "Title",
            Subtitle = "subtitle",
            Body = "body",
            Badge = 2,
            InterruptionLevel = NotificationInterruptionLevel.Critical,
            RelevanceScore = 1.0,
            ShowInForeground = true,
            ForegroundPresentationOption = PresentationOption.Alert,
        };

        iOSNotificationCenter.ScheduleNotification(notification);

        yield return WaitForNotification(10.0f);
        Assert.AreEqual(1, receivedNotificationCount);
        Assert.IsNotNull(lastReceivedNotification);
        CompareNotifications(notification, lastReceivedNotification);
    }

    void CompareNotifications(iOSNotification expected, iOSNotification actual)
    {
        // these don't roundtrip or are tested separately
        var ignoredProps = new[]
        {
            "Attachments",
            "SoundName",
            "SoundType",
            "SoundVolume",
            "UserInfo",
        };
        foreach (var prop in typeof(iOSNotification).GetProperties())
        {
            if (ignoredProps.Contains(prop.Name))
                continue;
            var v1 = prop.GetValue(expected);
            var v2 = prop.GetValue(actual);
            if (prop.PropertyType.IsValueType)
            {
                if (prop.PropertyType.IsPrimitive)
                {
                    if (prop.PropertyType == typeof(bool))
                    {
                        if (!(bool)v2)
                            Assert.Fail($"Expected property {prop.Name} to be true");
                    }
                    else
                    {
                        int intVal = ((IConvertible)v2).ToInt32(System.Globalization.CultureInfo.InvariantCulture);
                        if (intVal == 0)
                            Assert.Fail($"Expected property {prop.Name} to be non-zero");
                    }
                }
                else if (prop.PropertyType.IsEnum)
                {
                    int intVal = (int)v2;
                    if (intVal == 0)
                        Assert.Fail($"Expected property {prop.Name} to be non-zero");
                }
                else
                    Assert.Fail($"Property {prop.Name} not handled");
            }
            else
            {
                if (v2 == null)
                    Assert.Fail($"Property {prop.Name} value is null");
            }

            if (!object.Equals(v1, v2))
                Assert.Fail($"Value missmatch for property {prop.Name}: '{v1}' vs '{v2}'");
        }
    }

    [Test]
    public void iOSNotificationCalendarTrigger_ToUtc_DoesNotConvertUtcTrigger()
    {
        var trigger = new iOSNotificationCalendarTrigger()
        {
            Hour = 5,
            Minute = 5,
            UtcTime = true,
        };

        var utcTrigger = trigger.ToUtc();

        Assert.AreEqual(5, utcTrigger.Hour);
        Assert.AreEqual(5, utcTrigger.Minute);
    }

    [Test]
    public void iOSNotificationCalendarTrigger_ToUtc_ConvertsLocalTrigger()
    {
        var localTime = DateTime.Now;
        var utcTime = localTime.ToUniversalTime();
        if (DateTime.Compare(localTime, utcTime) == 0)
            return; // running test in GMT time zode

        var trigger = new iOSNotificationCalendarTrigger()
        {
            Hour = localTime.Hour,
            Minute = localTime.Minute,
            UtcTime = false,
        };

        var utcTrigger = trigger.ToUtc();

        Assert.AreEqual(utcTime.Hour, utcTrigger.Hour);
        Assert.AreEqual(utcTime.Minute, utcTrigger.Minute);
    }

    [Test]
    public void iOSNotificationCalendarTrigger_ToLocal_DoesNotConvertLocalTrigger()
    {
        var trigger = new iOSNotificationCalendarTrigger()
        {
            Hour = 5,
            Minute = 5,
            UtcTime = false,
        };

        var localTrigger = trigger.ToLocal();

        Assert.AreEqual(5, localTrigger.Hour);
        Assert.AreEqual(5, localTrigger.Minute);
    }

    [Test]
    public void iOSNotificationCalendarTrigger_ToLocal_ConvertsUtcTrigger()
    {
        var localTime = DateTime.Now;
        var utcTime = localTime.ToUniversalTime();
        if (DateTime.Compare(localTime, utcTime) == 0)
            return; // running test in GMT time zode

        var trigger = new iOSNotificationCalendarTrigger()
        {
            Hour = utcTime.Hour,
            Minute = utcTime.Minute,
            UtcTime = true,
        };

        var localTrigger = trigger.ToLocal();

        Assert.AreEqual(localTime.Hour, localTrigger.Hour);
        Assert.AreEqual(localTime.Minute, localTrigger.Minute);
    }

    [Test]
    public void iOSNotificationCalendarTrigger_AssignDateTimeComponents_OnlyChangesNonNullFields()
    {
        var dt = new DateTime(2025, 5, 5, 6, 6, 6);

        var trigger = new iOSNotificationCalendarTrigger()
        {
            Year = 2020,
            Month = 10,
            Day = 8,
        };

        var check = trigger.AssignDateTimeComponents(dt);
        Assert.AreEqual(2020, check.Year);
        Assert.AreEqual(10, check.Month);
        Assert.AreEqual(8, check.Day);
        Assert.AreEqual(6, check.Hour);
        Assert.AreEqual(6, check.Minute);
        Assert.AreEqual(6, check.Second);

        trigger = new iOSNotificationCalendarTrigger()
        {
            Hour = 3,
            Minute = 4,
            Second = 20,
        };

        check = trigger.AssignDateTimeComponents(dt);
        Assert.AreEqual(2025, check.Year);
        Assert.AreEqual(5, check.Month);
        Assert.AreEqual(5, check.Day);
        Assert.AreEqual(3, check.Hour);
        Assert.AreEqual(4, check.Minute);
        Assert.AreEqual(20, check.Second);
    }

    [Test]
    public void iOSNotificationCalendarTrigger_AssignNonEmptyComponents_Works()
    {
        var dt = new DateTime(2025, 1, 2, 3, 4, 5);

        var trigger = new iOSNotificationCalendarTrigger()
        {
            Year = 2020,
            Month = 10,
            Day = 10,
        };

        trigger.AssignNonEmptyComponents(dt);
        Assert.AreEqual(2025, trigger.Year);
        Assert.AreEqual(1, trigger.Month);
        Assert.AreEqual(2, trigger.Day);
        Assert.IsTrue(null == trigger.Hour);
        Assert.IsTrue(null == trigger.Minute);
        Assert.IsTrue(null == trigger.Second);

        trigger = new iOSNotificationCalendarTrigger()
        {
            Hour = 10,
            Minute = 10,
            Second = 10,
        };

        trigger.AssignNonEmptyComponents(dt);
        Assert.IsTrue(null == trigger.Year);
        Assert.IsTrue(null == trigger.Month);
        Assert.IsTrue(null == trigger.Day);
        Assert.AreEqual(3, trigger.Hour);
        Assert.AreEqual(4, trigger.Minute);
        Assert.AreEqual(5, trigger.Second);
    }

    [Test]
    public void iOSNotification_CalendarTrigger_ReturnsSameKindDateTime()
    {
        var trigger1 = new iOSNotificationCalendarTrigger()
        {
            Hour = 8,
            Minute = 30,
            UtcTime = false,
        };

        var trigger2 = new iOSNotificationCalendarTrigger()
        {
            Hour = 8,
            Minute = 30,
            UtcTime = false,
        };

        var notification = new iOSNotification()
        {
            Title = "text",
            Body = "text",
            Trigger = trigger1,
        };

        var retTrigger = (iOSNotificationCalendarTrigger)notification.Trigger;

        Assert.AreEqual(trigger1.Hour, retTrigger.Hour);
        Assert.AreEqual(trigger1.Minute, retTrigger.Minute);
        Assert.AreEqual(trigger1.UtcTime, retTrigger.UtcTime);

        notification.Trigger = trigger2;
        retTrigger = (iOSNotificationCalendarTrigger)notification.Trigger;

        Assert.AreEqual(trigger2.Hour, retTrigger.Hour);
        Assert.AreEqual(trigger2.Minute, retTrigger.Minute);
        Assert.AreEqual(trigger2.UtcTime, retTrigger.UtcTime);
    }

    [Test]
    public void iOSNotificationCalendarTrigger_HandlesMissingUtcField()
    {
        var original = new iOSNotificationCalendarTrigger()
        {
            Day = 5,
        };

        var notification = new iOSNotification()
        {
            Trigger = original,
        };

        // clear UserInfo, where UTC flag is stored
        notification.UserInfo.Clear();

        Assert.AreEqual(iOSNotificationTriggerType.Calendar, notification.Trigger.Type);
        var result = (iOSNotificationCalendarTrigger)notification.Trigger;
        Assert.AreEqual(5, result.Day);
        Assert.IsFalse(result.UtcTime);
    }
}
