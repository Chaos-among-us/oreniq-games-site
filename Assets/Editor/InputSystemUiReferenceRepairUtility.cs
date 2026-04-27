#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public static class InputSystemUiReferenceRepairUtility
{
    private const string InputActionsAssetPath = "Assets/InputSystem_Actions.inputactions";

    private static readonly string[] TargetScenes =
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/Game.unity",
        "Assets/Scenes/Shop.unity",
        "Assets/Scenes/Inventory.unity"
    };

    private static readonly (string key, System.Action<InputSystemUIInputModule, InputActionReference> assign)[] UiBindings =
    {
        ("UI/Point", (module, reference) => module.point = reference),
        ("UI/Navigate", (module, reference) => module.move = reference),
        ("UI/Submit", (module, reference) => module.submit = reference),
        ("UI/Cancel", (module, reference) => module.cancel = reference),
        ("UI/Click", (module, reference) => module.leftClick = reference),
        ("UI/MiddleClick", (module, reference) => module.middleClick = reference),
        ("UI/RightClick", (module, reference) => module.rightClick = reference),
        ("UI/ScrollWheel", (module, reference) => module.scrollWheel = reference),
        ("UI/TrackedDevicePosition", (module, reference) => module.trackedDevicePosition = reference),
        ("UI/TrackedDeviceOrientation", (module, reference) => module.trackedDeviceOrientation = reference)
    };

    [MenuItem("Tools/Android/Repair InputSystem UI References")]
    public static void RepairFromMenu()
    {
        RepairAllTargetScenes(logResult: true);
    }

    public static void RepairAllTargetScenesBatchmode()
    {
        if (!RepairAllTargetScenes(logResult: true))
            throw new System.Exception("InputSystem UI reference repair failed.");
    }

    [MenuItem("Tools/Android/Log InputSystem UI Reference Ids")]
    public static void LogReferenceIdsFromMenu()
    {
        LogReferenceIds();
    }

    public static void LogReferenceIdsBatchmode()
    {
        LogReferenceIds();
    }

    private static bool RepairAllTargetScenes(bool logResult)
    {
        InputActionAsset actionsAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsAssetPath);

        if (actionsAsset == null)
        {
            Debug.LogError("Input actions asset was not found at " + InputActionsAssetPath + ".");
            return false;
        }

        Dictionary<string, InputActionReference> referencesByKey = LoadReferences(actionsAsset);
        List<string> missingKeys = UiBindings
            .Where(binding => !referencesByKey.ContainsKey(binding.key))
            .Select(binding => binding.key)
            .ToList();

        if (missingKeys.Count > 0)
        {
            Debug.LogError(
                "InputSystem UI reference repair could not continue because these UI action references were missing: " +
                string.Join(", ", missingKeys));
            return false;
        }

        SceneSetup[] previousSceneSetup = EditorSceneManager.GetSceneManagerSetup();

        try
        {
            for (int i = 0; i < TargetScenes.Length; i++)
            {
                string scenePath = TargetScenes[i];
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                InputSystemUIInputModule[] modules = Object.FindObjectsByType<InputSystemUIInputModule>(
                    FindObjectsInactive.Include);

                if (modules == null || modules.Length == 0)
                {
                    Debug.LogWarning("No InputSystemUIInputModule was found in scene " + scenePath + ".");
                    continue;
                }

                bool sceneChanged = false;

                for (int moduleIndex = 0; moduleIndex < modules.Length; moduleIndex++)
                {
                    InputSystemUIInputModule module = modules[moduleIndex];

                    Undo.RecordObject(module, "Repair InputSystem UI references");
                    module.actionsAsset = actionsAsset;

                    for (int bindingIndex = 0; bindingIndex < UiBindings.Length; bindingIndex++)
                    {
                        (string key, System.Action<InputSystemUIInputModule, InputActionReference> assign) = UiBindings[bindingIndex];
                        assign(module, referencesByKey[key]);
                    }

                    EditorUtility.SetDirty(module);
                    sceneChanged = true;
                }

                if (sceneChanged)
                    EditorSceneManager.SaveScene(scene);

                if (logResult)
                    Debug.Log("Repaired InputSystem UI references in scene: " + scene.path);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;
        }
        finally
        {
            if (previousSceneSetup != null && previousSceneSetup.Length > 0)
                EditorSceneManager.RestoreSceneManagerSetup(previousSceneSetup);
        }
    }

    private static Dictionary<string, InputActionReference> LoadReferences(InputActionAsset actionsAsset)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(InputActionsAssetPath);
        Dictionary<string, InputActionReference> references = new Dictionary<string, InputActionReference>();

        for (int i = 0; i < assets.Length; i++)
        {
            InputActionReference reference = assets[i] as InputActionReference;

            if (reference == null || reference.action == null || reference.action.actionMap == null)
                continue;

            string key = reference.action.actionMap.name + "/" + reference.action.name;
            references[key] = reference;
        }

        if (!references.ContainsKey("UI/Navigate"))
        {
            InputAction moveAction = actionsAsset.FindAction("UI/Move", throwIfNotFound: false);
            InputActionReference moveReference = moveAction != null
                ? assets.OfType<InputActionReference>().FirstOrDefault(reference => reference.action == moveAction)
                : null;

            if (moveReference != null)
                references["UI/Navigate"] = moveReference;
        }

        return references;
    }

    private static void LogReferenceIds()
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(InputActionsAssetPath);

        foreach (InputActionReference reference in assets.OfType<InputActionReference>())
        {
            if (reference.action == null || reference.action.actionMap == null)
                continue;

            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(reference, out string guid, out long localId);
            Debug.Log(
                "InputActionReference " +
                reference.action.actionMap.name +
                "/" +
                reference.action.name +
                " | guid=" +
                guid +
                " | localId=" +
                localId);
        }
    }
}
#endif
