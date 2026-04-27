using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class UiInputDebugProbe : MonoBehaviour
{
    private const string TracePrefix = "[UI-INPUT]";
    private const int MaxLoggedTouches = 12;

    private static UiInputDebugProbe instance;
    private static int loggedTouchCount;
    private Coroutine startupRecoveryRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureExists()
    {
        if (!Application.identifier.EndsWith(".secondary"))
            return;

        if (instance != null)
            return;

        GameObject probeObject = new GameObject("UiInputDebugProbe");
        instance = probeObject.AddComponent<UiInputDebugProbe>();
        DontDestroyOnLoad(probeObject);
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        instance.LogProbeState("Probe booted");
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (instance != null)
            instance.LogProbeState("Scene loaded name=" + scene.name + " mode=" + mode);
    }

    private void Update()
    {
        if (loggedTouchCount >= MaxLoggedTouches)
            return;

        Vector2? touchPosition = GetTouchBeganPosition();

        if (!touchPosition.HasValue)
            return;

        loggedTouchCount++;
        LogTouch(touchPosition.Value, "Touch began #" + loggedTouchCount);
    }

    private void Start()
    {
        if (startupRecoveryRoutine != null)
            StopCoroutine(startupRecoveryRoutine);

        startupRecoveryRoutine = StartCoroutine(ForceActivateInputModuleRoutine());
    }

    private Vector2? GetTouchBeganPosition()
    {
        Touchscreen touchscreen = Touchscreen.current;

        if (touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame)
            return touchscreen.primaryTouch.position.ReadValue();

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.phase == UnityEngine.TouchPhase.Began)
                return touch.position;
        }

        return null;
    }

    private void LogTouch(Vector2 screenPosition, string reason)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(TracePrefix);
        builder.Append(' ');
        builder.Append(reason);
        builder.Append(" | scene=");
        builder.Append(GetSceneName());
        builder.Append(" | pos=");
        builder.Append(screenPosition);
        builder.Append(" | eventSystem=");
        builder.Append(DescribeEventSystem());
        builder.Append(" | raycasts=");
        builder.Append(DescribeRaycasts(screenPosition));
        Debug.Log(builder.ToString());
    }

    private void LogProbeState(string reason)
    {
        Debug.Log(
            TracePrefix +
            " " +
            reason +
            " | scene=" +
            GetSceneName() +
            " | eventSystem=" +
            DescribeEventSystem() +
            " | devices=" +
            DescribeInputDevices());
    }

    private static string GetSceneName()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        return activeScene.IsValid() ? activeScene.name : "<invalid>";
    }

    private static string DescribeEventSystem()
    {
        EventSystem eventSystem = EventSystem.current;

        if (eventSystem == null)
            return "<none>";

        BaseInputModule inputModule = eventSystem.currentInputModule;

        return
            eventSystem.name +
            ",enabled=" +
            eventSystem.enabled +
            ",module=" +
            (inputModule != null ? inputModule.GetType().Name : "<none>") +
            ",modules=" +
            DescribeAvailableModules(eventSystem) +
            ",ui=" +
            DescribeUiModule(eventSystem);
    }

    private static string DescribeAvailableModules(EventSystem eventSystem)
    {
        BaseInputModule[] modules = eventSystem.GetComponents<BaseInputModule>();

        if (modules == null || modules.Length == 0)
            return "<none>";

        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < modules.Length; i++)
        {
            if (i > 0)
                builder.Append('|');

            BaseInputModule module = modules[i];

            if (module == null)
            {
                builder.Append("<null>");
                continue;
            }

            builder.Append(module.GetType().Name);
            builder.Append("(enabled=");
            builder.Append(module.enabled);
            builder.Append(",active=");
            builder.Append(module.isActiveAndEnabled);
            builder.Append(')');
        }

        return builder.ToString();
    }

    private static string DescribeUiModule(EventSystem eventSystem)
    {
        InputSystemUIInputModule module = eventSystem.GetComponent<InputSystemUIInputModule>();

        if (module == null)
            return "<none>";

        StringBuilder builder = new StringBuilder();
        builder.Append("asset=");
        builder.Append(module.actionsAsset != null ? module.actionsAsset.name : "<null>");
        builder.Append(",point=");
        builder.Append(DescribeActionReference(module.point));
        builder.Append(",leftClick=");
        builder.Append(DescribeActionReference(module.leftClick));
        builder.Append(",submit=");
        builder.Append(DescribeActionReference(module.submit));
        builder.Append(",cancel=");
        builder.Append(DescribeActionReference(module.cancel));
        return builder.ToString();
    }

    private static string DescribeActionReference(InputActionReference reference)
    {
        if (reference == null)
            return "<null-ref>";

        InputAction action = reference.action;

        if (action == null)
            return reference.name + ":<null-action>";

        return
            action.actionMap?.name +
            "/" +
            action.name +
            "(enabled=" +
            action.enabled +
            ")";
    }

    private static string DescribeInputDevices()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("touchscreen=");
        builder.Append(Touchscreen.current != null ? Touchscreen.current.displayName + "#" + Touchscreen.current.deviceId : "<none>");
        builder.Append(",legacyTouchSupported=");
        builder.Append(Input.touchSupported);
        builder.Append(",legacyTouchCount=");
        builder.Append(Input.touchCount);
        builder.Append(",devices=");

        if (InputSystem.devices.Count == 0)
        {
            builder.Append("<none>");
            return builder.ToString();
        }

        builder.Append(string.Join(
            "|",
            InputSystem.devices.Select(device => device.layout + ":" + device.displayName + "#" + device.deviceId)));
        return builder.ToString();
    }

    private IEnumerator ForceActivateInputModuleRoutine()
    {
        float[] delays = { 0f, 0.1f, 0.4f };

        for (int i = 0; i < delays.Length; i++)
        {
            if (delays[i] > 0f)
                yield return new WaitForSecondsRealtime(delays[i]);

            ForceActivateInputModule("startup-" + i);
        }

        startupRecoveryRoutine = null;
    }

    private void ForceActivateInputModule(string reason)
    {
        EventSystem eventSystem = EventSystem.current;

        if (eventSystem == null)
        {
            Debug.Log(TracePrefix + " Startup activate skipped " + reason + " | eventSystem=<none>");
            return;
        }

        BaseInputModule[] modules = eventSystem.GetComponents<BaseInputModule>();
        Debug.Log(TracePrefix + " Startup activate begin " + reason + " | " + DescribeEventSystem());

        for (int i = 0; i < modules.Length; i++)
        {
            BaseInputModule module = modules[i];

            if (module == null || !module.enabled)
                continue;

            module.ActivateModule();
        }

        eventSystem.UpdateModules();
        Debug.Log(TracePrefix + " Startup activate end " + reason + " | " + DescribeEventSystem());
    }

    private static string DescribeRaycasts(Vector2 screenPosition)
    {
        EventSystem eventSystem = EventSystem.current;

        if (eventSystem == null)
            return "no-event-system";

        GraphicRaycaster[] raycasters = FindObjectsByType<GraphicRaycaster>(FindObjectsInactive.Exclude);

        if (raycasters == null || raycasters.Length == 0)
            return "no-graphic-raycasters";

        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = screenPosition
        };
        List<RaycastResult> results = new List<RaycastResult>(16);

        for (int i = 0; i < raycasters.Length; i++)
        {
            GraphicRaycaster raycaster = raycasters[i];

            if (raycaster == null || !raycaster.isActiveAndEnabled)
                continue;

            raycaster.Raycast(pointerData, results);
        }

        if (results.Count == 0)
            return "none";

        StringBuilder builder = new StringBuilder();
        int limit = Mathf.Min(4, results.Count);

        for (int i = 0; i < limit; i++)
        {
            if (i > 0)
                builder.Append(" > ");

            RaycastResult result = results[i];
            builder.Append(result.gameObject != null ? result.gameObject.name : "<null>");
            builder.Append('(');
            builder.Append(result.module != null ? result.module.GetType().Name : "<no-module>");
            builder.Append(')');
        }

        return builder.ToString();
    }
}

public sealed class UiInputRecoveryHelper : MonoBehaviour
{
    private const string TracePrefix = "[UI-INPUT]";

    private static UiInputRecoveryHelper instance;
    private Coroutine recoveryRoutine;

    public static void RecoverAfterQaPermissionFlow()
    {
        UiInputRecoveryHelper helper = EnsureExists();

        if (helper != null)
            helper.BeginRecovery();
    }

    private static UiInputRecoveryHelper EnsureExists()
    {
        if (instance != null)
            return instance;

        UiInputRecoveryHelper existing = FindAnyObjectByType<UiInputRecoveryHelper>();

        if (existing != null)
            return existing;

        GameObject helperObject = new GameObject("UiInputRecoveryHelper");
        instance = helperObject.AddComponent<UiInputRecoveryHelper>();
        DontDestroyOnLoad(helperObject);
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void BeginRecovery()
    {
        if (recoveryRoutine != null)
            StopCoroutine(recoveryRoutine);

        recoveryRoutine = StartCoroutine(RecoverRoutine());
    }

    private IEnumerator RecoverRoutine()
    {
        float[] delays = { 0f, 0.06f, 0.18f, 0.45f };

        for (int i = 0; i < delays.Length; i++)
        {
            if (delays[i] > 0f)
                yield return new WaitForSecondsRealtime(delays[i]);

            AttemptRecovery("qa-prompt-" + i);
        }

        recoveryRoutine = null;
    }

    private void AttemptRecovery(string reason)
    {
        EventSystem eventSystem = EventSystem.current;

        if (eventSystem == null)
        {
            Debug.Log(TracePrefix + " Recovery skipped " + reason + " | eventSystem=<none>");
            return;
        }

        BaseInputModule[] modules = eventSystem.GetComponents<BaseInputModule>();
        Debug.Log(TracePrefix + " Recovery begin " + reason + " | " + DescribeEventSystem(eventSystem, modules));

        for (int i = 0; i < modules.Length; i++)
        {
            BaseInputModule module = modules[i];

            if (module == null)
                continue;

            module.enabled = false;
            module.enabled = true;
            module.ActivateModule();
        }

        eventSystem.enabled = false;
        eventSystem.enabled = true;
        eventSystem.UpdateModules();
        eventSystem.SetSelectedGameObject(null);

        modules = eventSystem.GetComponents<BaseInputModule>();
        Debug.Log(TracePrefix + " Recovery end " + reason + " | " + DescribeEventSystem(eventSystem, modules));
    }

    private static string DescribeEventSystem(EventSystem eventSystem, BaseInputModule[] modules)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("eventSystem=");
        builder.Append(eventSystem.name);
        builder.Append(",enabled=");
        builder.Append(eventSystem.enabled);
        builder.Append(",current=");
        builder.Append(eventSystem.currentInputModule != null ? eventSystem.currentInputModule.GetType().Name : "<none>");
        builder.Append(",modules=");

        if (modules == null || modules.Length == 0)
        {
            builder.Append("<none>");
            return builder.ToString();
        }

        for (int i = 0; i < modules.Length; i++)
        {
            if (i > 0)
                builder.Append('|');

            BaseInputModule module = modules[i];

            if (module == null)
            {
                builder.Append("<null>");
                continue;
            }

            builder.Append(module.GetType().Name);
            builder.Append("(enabled=");
            builder.Append(module.enabled);
            builder.Append(",active=");
            builder.Append(module.isActiveAndEnabled);
            builder.Append(')');
        }

        return builder.ToString();
    }
}
