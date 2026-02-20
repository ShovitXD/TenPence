using UnityEngine;

public class SettingsPanelToggle : MonoBehaviour
{
    [Header("Drag your Settings Panel GameObject here")]
    public GameObject settingsPanel;

    [Header("If true, starts with panel hidden")]
    public bool startHidden = true;

    private void Awake()
    {
        if (settingsPanel == null)
        {
            Debug.LogError("SettingsPanelToggle: settingsPanel is not assigned.");
            return;
        }

        if (startHidden)
            settingsPanel.SetActive(false);
    }

    // Hook this to your UI Button's OnClick()
    public void ToggleSettingsPanel()
    {
        if (settingsPanel == null) return;
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    // Optional direct controls
    public void OpenSettingsPanel()
    {
        if (settingsPanel == null) return;
        settingsPanel.SetActive(true);
    }

    public void CloseSettingsPanel()
    {
        if (settingsPanel == null) return;
        settingsPanel.SetActive(false);
    }
}
