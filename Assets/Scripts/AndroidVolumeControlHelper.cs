using System;
using UnityEngine;

public static class AndroidVolumeControlHelper
{
    private const int StreamMusic = 3;
    private static bool warningLogged;

    public static void BindHardwareVolumeKeysToMusic()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (activity == null)
                return;

            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                try
                {
                    activity.Call("setVolumeControlStream", StreamMusic);
                }
                catch (Exception exception)
                {
                    LogWarningOnce("Failed to bind Android hardware volume controls to music: " + exception.Message);
                }
            }));
        }
        catch (Exception exception)
        {
            LogWarningOnce("Failed to access the Android activity for hardware volume binding: " + exception.Message);
        }
#endif
    }

    private static void LogWarningOnce(string message)
    {
        if (warningLogged)
            return;

        warningLogged = true;
        Debug.LogWarning(message);
    }
}
