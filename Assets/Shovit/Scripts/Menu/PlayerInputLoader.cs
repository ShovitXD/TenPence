using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputLoader : MonoBehaviour
{
    private void Awake()
    {
        var playerInput = GetComponent<PlayerInput>();

        // Load and apply binding overrides to the PlayerInput actions
        if (playerInput.actions != null)
        {
            InputSettingsApplier.LoadAndApplyBindingOverrides(playerInput.actions);
        }
        else
        {
            Debug.LogWarning("PlayerInputLoader: PlayerInput.actions is null. Assign your InputActionAsset in PlayerInput.");
        }

        // Optional: apply saved control scheme (only matters if you use schemes)
        string scheme = InputSettingsApplier.LoadControlScheme();
        if (!string.IsNullOrEmpty(scheme))
        {
            TrySwitchScheme(playerInput, scheme);
        }
    }

    private void TrySwitchScheme(PlayerInput playerInput, string scheme)
    {
        // This is safe: it just tries and won't crash your game if devices aren't present.
        try
        {
            if (scheme == "Keyboard&Mouse")
                playerInput.SwitchCurrentControlScheme("Keyboard&Mouse", Keyboard.current, Mouse.current);
            else if (scheme == "Gamepad")
                playerInput.SwitchCurrentControlScheme("Gamepad", Gamepad.current);

            Debug.Log($"Switched control scheme to: {scheme}");
        }
        catch
        {
            Debug.LogWarning($"Could not switch control scheme to '{scheme}' (device may not be connected).");
        }
    }
}
