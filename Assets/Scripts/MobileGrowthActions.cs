using System;
using UnityEngine;

public static class MobileGrowthActions
{
    public const string ProductionPackageId = "com.oreniq.endlessdodge";
    private const string StoreUrlPrefix = "https://play.google.com/store/apps/details?id=";

    public static string GetProductionStoreUrl()
    {
        return StoreUrlPrefix + ProductionPackageId;
    }

    public static bool ShareText(string chooserTitle, string subject, string body)
    {
        string resolvedSubject = string.IsNullOrWhiteSpace(subject) ? "Endless Dodge" : subject.Trim();
        string resolvedBody = string.IsNullOrWhiteSpace(body) ? GetProductionStoreUrl() : body.Trim();

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
            using (AndroidJavaObject sendIntent = new AndroidJavaObject("android.content.Intent"))
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                sendIntent.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
                sendIntent.Call<AndroidJavaObject>("setType", "text/plain");
                sendIntent.Call<AndroidJavaObject>(
                    "putExtra",
                    intentClass.GetStatic<string>("EXTRA_SUBJECT"),
                    resolvedSubject);
                sendIntent.Call<AndroidJavaObject>(
                    "putExtra",
                    intentClass.GetStatic<string>("EXTRA_TEXT"),
                    resolvedBody);

                using (AndroidJavaObject chooserIntent = intentClass.CallStatic<AndroidJavaObject>(
                    "createChooser",
                    sendIntent,
                    chooserTitle))
                {
                    currentActivity.Call("startActivity", chooserIntent);
                }
            }

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Share sheet launch failed: " + exception.Message);
        }
#endif

        GUIUtility.systemCopyBuffer = resolvedBody;
        return false;
    }

    public static bool OpenStoreListing()
    {
        string storeUrl = GetProductionStoreUrl();

#if UNITY_ANDROID && !UNITY_EDITOR
        return TryOpenAndroidUri("market://details?id=" + ProductionPackageId) ||
               TryOpenAndroidUri(storeUrl);
#else
        Application.OpenURL(storeUrl);
        return true;
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static bool TryOpenAndroidUri(string uri)
    {
        try
        {
            using (AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
            using (AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri"))
            using (AndroidJavaObject parsedUri = uriClass.CallStatic<AndroidJavaObject>("parse", uri))
            using (AndroidJavaObject intentObject = new AndroidJavaObject(
                "android.content.Intent",
                intentClass.GetStatic<string>("ACTION_VIEW"),
                parsedUri))
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                currentActivity.Call("startActivity", intentObject);
            }

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Store listing launch failed for " + uri + ": " + exception.Message);
            return false;
        }
    }
#endif
}
