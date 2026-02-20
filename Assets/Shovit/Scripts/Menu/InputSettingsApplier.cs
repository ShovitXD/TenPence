using UnityEngine;
using UnityEngine.InputSystem;

public class InputSettingsApplier : MonoBehaviour
{
    [Header("Assign your InputActionAsset (from your .inputactions)")]
    public InputActionAsset actions;

    private const string PREF_BIND_OVERRIDES = "input_bind_overrides_json";
    private const string PREF_CONTROL_SCHEME = "input_control_scheme";

    private void Awake()
    {
        if (actions == null)
        {
            Debug.LogError("InputSettingsApplier: actions asset not assigned.");
            return;
        }

        LoadAndApplyBindingOverrides(actions);
        // Control scheme is applied by PlayerInputLoader in Game scene.
    }

    public static void SaveBindingOverrides(InputActionAsset actions)
    {
        if (actions == null) return;
        string json = actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(PREF_BIND_OVERRIDES, json);
        PlayerPrefs.Save();
    }

    public static void LoadAndApplyBindingOverrides(InputActionAsset actions)
    {
        if (actions == null) return;

        string json = PlayerPrefs.GetString(PREF_BIND_OVERRIDES, string.Empty);
        if (!string.IsNullOrEmpty(json))
        {
            actions.LoadBindingOverridesFromJson(json);
        }
    }

    public static void ClearAllBindingOverrides(InputActionAsset actions)
    {
        if (actions == null) return;

        foreach (var map in actions.actionMaps)
            map.RemoveAllBindingOverrides();

        PlayerPrefs.DeleteKey(PREF_BIND_OVERRIDES);
        PlayerPrefs.Save();
    }

    public static void SaveControlScheme(string schemeName)
    {
        PlayerPrefs.SetString(PREF_CONTROL_SCHEME, schemeName);
        PlayerPrefs.Save();
    }

    public static string LoadControlScheme()
    {
        return PlayerPrefs.GetString(PREF_CONTROL_SCHEME, string.Empty);
    }
}
