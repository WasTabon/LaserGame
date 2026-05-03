using UnityEngine;

public static class HapticManager
{
    public enum HapticType
    {
        Light,
        Medium,
        Heavy,
        Success,
        Warning,
        Failure
    }

    public static void Trigger(HapticType type)
    {
        if (!SaveSystem.Data.hapticsEnabled) return;

#if UNITY_IOS && !UNITY_EDITOR
        TriggerIOS(type);
#elif UNITY_ANDROID && !UNITY_EDITOR
        Handheld.Vibrate();
#endif
    }

#if UNITY_IOS && !UNITY_EDITOR
    private static void TriggerIOS(HapticType type)
    {
        Handheld.Vibrate();
    }
#endif
}
