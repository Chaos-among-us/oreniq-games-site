using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

public class UnityServicesBootstrap : MonoBehaviour
{
    public static bool IsReady { get; private set; }
    public static string LastError { get; private set; } = string.Empty;

    private static UnityServicesBootstrap instance;
    private static Task initializationTask;
    private static bool missingPackagesLogged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureBootstrapExists()
    {
        if (FindAnyObjectByType<UnityServicesBootstrap>() != null)
            return;

        GameObject bootstrapObject = new GameObject("UnityServicesBootstrap");
        bootstrapObject.AddComponent<UnityServicesBootstrap>();
    }

    async void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        await EnsureInitializedAsync();
    }

    public static async Task<bool> EnsureInitializedAsync()
    {
        if (IsReady)
            return true;

        if (initializationTask == null)
            initializationTask = InitializeInternalAsync();

        await initializationTask;
        return IsReady;
    }

    static async Task InitializeInternalAsync()
    {
        try
        {
            Type unityServicesType = FindType("Unity.Services.Core.UnityServices");

            if (unityServicesType == null)
            {
                LastError = "Unity Services packages are not installed yet.";

                if (!missingPackagesLogged)
                {
                    missingPackagesLogged = true;
                    Debug.LogWarning("Unity Services packages are not available yet. Open the project in Unity so Package Manager can restore newly added packages.");
                }

                return;
            }

            MethodInfo initializeMethod = unityServicesType.GetMethod(
                "InitializeAsync",
                BindingFlags.Public | BindingFlags.Static,
                null,
                Type.EmptyTypes,
                null);

            if (initializeMethod == null)
            {
                LastError = "UnityServices.InitializeAsync() was not found.";
                Debug.LogWarning(LastError);
                return;
            }

            if (initializeMethod.Invoke(null, null) is Task initializeTask)
                await initializeTask;

            await TryAnonymousSignInAsync();

            IsReady = true;
            LastError = string.Empty;
            LaunchAnalytics.RecordSessionStarted();
            LaunchAnalytics.FlushPendingEvents();
            Debug.Log("Unity Services bootstrap completed.");
        }
        catch (Exception exception)
        {
            LastError = exception.Message;
            Debug.LogWarning("Unity Services bootstrap failed: " + exception);
        }
    }

    static async Task TryAnonymousSignInAsync()
    {
        Type authenticationServiceType = FindType("Unity.Services.Authentication.AuthenticationService");

        if (authenticationServiceType == null)
            return;

        PropertyInfo instanceProperty = authenticationServiceType.GetProperty(
            "Instance",
            BindingFlags.Public | BindingFlags.Static);

        if (instanceProperty == null)
            return;

        object authenticationInstance = instanceProperty.GetValue(null);

        if (authenticationInstance == null)
            return;

        MethodInfo signInMethod = authenticationInstance.GetType().GetMethod(
            "SignInAnonymouslyAsync",
            BindingFlags.Public | BindingFlags.Instance,
            null,
            Type.EmptyTypes,
            null);

        if (signInMethod == null)
            return;

        if (signInMethod.Invoke(authenticationInstance, null) is Task signInTask)
            await signInTask;
    }

    static Type FindType(string fullName)
    {
        Type directMatch = Type.GetType(fullName);

        if (directMatch != null)
            return directMatch;

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        for (int i = 0; i < assemblies.Length; i++)
        {
            Type assemblyMatch = assemblies[i].GetType(fullName);

            if (assemblyMatch != null)
                return assemblyMatch;
        }

        return null;
    }
}
