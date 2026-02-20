using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;

public class CVDVolumeProfile : MonoBehaviour
{
    [Serializable]
    public class NamedProfile
    {
        public string name;
        public VolumeProfile profile;
    }

    [Header("UI")]
    [SerializeField] private TMP_Dropdown dropdown;

    [Header("Volume")]
    [SerializeField] private Volume targetVolume;

    [Tooltip("Index 0 MUST be Normal.")]
    [SerializeField] private List<NamedProfile> profiles = new List<NamedProfile>();

    [Header("Default")]
    [Tooltip("Defaults to 0 (Normal).")]
    [SerializeField] private int defaultIndex = 0;

    void Awake()
    {
        if (!dropdown || !targetVolume)
        {
            Debug.LogError($"{nameof(CVDVolumeProfile)} missing references.", this);
            enabled = false;
            return;
        }

        if (profiles == null || profiles.Count == 0)
        {
            Debug.LogError($"{nameof(CVDVolumeProfile)} has no profiles assigned.", this);
            enabled = false;
            return;
        }

        // Build options at runtime
        dropdown.ClearOptions();
        var options = new List<string>(profiles.Count);
        for (int i = 0; i < profiles.Count; i++)
        {
            var n = profiles[i];
            options.Add(string.IsNullOrWhiteSpace(n.name)
                ? (n.profile ? n.profile.name : $"Profile {i}")
                : n.name);
        }
        dropdown.AddOptions(options);

        // Hook change event once
        dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
        dropdown.onValueChanged.AddListener(OnDropdownChanged);

        // Set default (Normal)
        defaultIndex = Mathf.Clamp(defaultIndex, 0, profiles.Count - 1);
        dropdown.SetValueWithoutNotify(defaultIndex);
        ApplyProfile(defaultIndex);
    }

    private void OnDropdownChanged(int index) => ApplyProfile(index);

    private void ApplyProfile(int index)
    {
        if (index < 0 || index >= profiles.Count) return;

        var p = profiles[index].profile;
        if (!p)
        {
            Debug.LogWarning($"Selected profile is null at index {index}.", this);
            return;
        }

        targetVolume.profile = p;
    }
}
