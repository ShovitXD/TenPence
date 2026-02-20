using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class InputSettingsMenu : MonoBehaviour
{
    [Header("Input")]
    public InputActionAsset actions;

    [Tooltip("Action Map name (usually 'Player'). Must match your .inputactions map.")]
    public string actionMapName = "Player";

    [Header("Scene")]
    public string gameSceneName = "Game";

    [Header("Move (2D Vector Composite parts) Label Texts")]
    public TMP_Text moveForwardLabel;
    public TMP_Text moveBackwardLabel;
    public TMP_Text moveLeftLabel;
    public TMP_Text moveRightLabel;

    [Header("Other Actions Label Texts")]
    public TMP_Text interactLabel;
    public TMP_Text teleportRoom0Label;
    public TMP_Text teleportRoom1Label;
    public TMP_Text teleportRoom2Label;
    public TMP_Text dropLLabel;
    public TMP_Text dropRLabel;

    [Header("Action Names (must match Input Actions)")]
    public string moveActionName = "Move";
    public string interactActionName = "Interact";
    public string teleportRoom0ActionName = "TeleportRoom0";
    public string teleportRoom1ActionName = "TeleportRoom1";
    public string teleportRoom2ActionName = "TeleportRoom2";
    public string dropLActionName = "DropL";
    public string dropRActionName = "DropR";

    // Optional: set this if you only want to rebind a specific group (example: "Keyboard&Mouse" or "Gamepad")
    [Header("Optional Binding Group Filter")]
    public string onlyRebindGroup = ""; // leave empty to rebind whatever binding we find first

    private InputActionRebindingExtensions.RebindingOperation _rebindOp;

    private void Awake()
    {
        if (actions == null)
        {
            Debug.LogError("InputSettingsMenu: actions asset not assigned.");
            return;
        }

        // Ensure menu shows current saved bindings
        InputSettingsApplier.LoadAndApplyBindingOverrides(actions);
    }

    private void Start()
    {
        RefreshAllLabels();
    }

    private void OnDisable()
    {
        _rebindOp?.Dispose();
        _rebindOp = null;
    }

    // -------------------------
    // UI WRAPPERS (NO PARAMS) — these show up in Button OnClick
    // -------------------------

    // Move composite parts (WASD-style)
    public void Rebind_MoveForward() => StartRebindMovePart("up", moveForwardLabel);
    public void Rebind_MoveBackward() => StartRebindMovePart("down", moveBackwardLabel);
    public void Rebind_MoveLeft() => StartRebindMovePart("left", moveLeftLabel);
    public void Rebind_MoveRight() => StartRebindMovePart("right", moveRightLabel);

    // Single actions
    public void Rebind_Interact() => StartRebindSingle(interactActionName, interactLabel);
    public void Rebind_TeleportRoom0() => StartRebindSingle(teleportRoom0ActionName, teleportRoom0Label);
    public void Rebind_TeleportRoom1() => StartRebindSingle(teleportRoom1ActionName, teleportRoom1Label);
    public void Rebind_TeleportRoom2() => StartRebindSingle(teleportRoom2ActionName, teleportRoom2Label);
    public void Rebind_DropL() => StartRebindSingle(dropLActionName, dropLLabel);
    public void Rebind_DropR() => StartRebindSingle(dropRActionName, dropRLabel);

    // Existing menu actions
    public void ResetAllBindings()
    {
        if (actions == null) return;
        InputSettingsApplier.ClearAllBindingOverrides(actions);
        RefreshAllLabels();
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // -------------------------
    // Core Rebind Logic
    // -------------------------

    private void StartRebindMovePart(string partName, TMP_Text labelToUpdate)
    {
        var action = GetAction(moveActionName);
        if (action == null) return;

        int bindingIndex = FindCompositePartBindingIndex(action, partName, onlyRebindGroup);
        if (bindingIndex < 0)
        {
            Debug.LogError($"Could not find Move composite part '{partName}'. " +
                           $"Make sure {moveActionName} is a 2D Vector composite with parts named up/down/left/right.");
            return;
        }

        StartInteractiveRebind(action, bindingIndex, labelToUpdate, isMoveCompositePart: true);
    }

    private void StartRebindSingle(string actionName, TMP_Text labelToUpdate)
    {
        var action = GetAction(actionName);
        if (action == null) return;

        int bindingIndex = FindPrimaryBindingIndex(action, onlyRebindGroup);
        if (bindingIndex < 0)
        {
            Debug.LogError($"Could not find a rebindable binding for action '{actionName}'.");
            return;
        }

        StartInteractiveRebind(action, bindingIndex, labelToUpdate, isMoveCompositePart: false);
    }

    private void StartInteractiveRebind(InputAction action, int bindingIndex, TMP_Text labelToUpdate, bool isMoveCompositePart)
    {
        // Cancel previous rebind
        _rebindOp?.Cancel();
        _rebindOp?.Dispose();
        _rebindOp = null;

        // Show "..." while waiting
        if (labelToUpdate != null) labelToUpdate.text = "...";

        action.Disable();

        _rebindOp = action.PerformInteractiveRebinding(bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(op =>
            {
                op.Dispose();
                _rebindOp = null;

                action.Enable();

                // Save immediately for Game scene
                InputSettingsApplier.SaveBindingOverrides(actions);

                // Refresh all labels (safe + easy)
                RefreshAllLabels();
            })
            .OnCancel(op =>
            {
                op.Dispose();
                _rebindOp = null;

                action.Enable();

                // Restore labels
                RefreshAllLabels();
            });

        _rebindOp.Start();
    }

    // -------------------------
    // Label Refresh
    // -------------------------

    public void RefreshAllLabels()
    {
        if (actions == null) return;

        // Move composite labels
        SetMoveLabel("up", moveForwardLabel);
        SetMoveLabel("down", moveBackwardLabel);
        SetMoveLabel("left", moveLeftLabel);
        SetMoveLabel("right", moveRightLabel);

        // Single action labels
        SetSingleLabel(interactActionName, interactLabel);
        SetSingleLabel(teleportRoom0ActionName, teleportRoom0Label);
        SetSingleLabel(teleportRoom1ActionName, teleportRoom1Label);
        SetSingleLabel(teleportRoom2ActionName, teleportRoom2Label);
        SetSingleLabel(dropLActionName, dropLLabel);
        SetSingleLabel(dropRActionName, dropRLabel);
    }

    private void SetMoveLabel(string partName, TMP_Text label)
    {
        if (label == null) return;

        var action = GetAction(moveActionName);
        if (action == null) { label.text = "-"; return; }

        int idx = FindCompositePartBindingIndex(action, partName, onlyRebindGroup);
        label.text = (idx >= 0) ? action.GetBindingDisplayString(idx) : "-";
    }

    private void SetSingleLabel(string actionName, TMP_Text label)
    {
        if (label == null) return;

        var action = GetAction(actionName);
        if (action == null) { label.text = "-"; return; }

        int idx = FindPrimaryBindingIndex(action, onlyRebindGroup);
        label.text = (idx >= 0) ? action.GetBindingDisplayString(idx) : "-";
    }

    // -------------------------
    // Helpers
    // -------------------------

    private InputAction GetAction(string actionName)
    {
        var map = actions.FindActionMap(actionMapName, true);
        if (map == null)
        {
            Debug.LogError($"InputSettingsMenu: ActionMap '{actionMapName}' not found.");
            return null;
        }

        var action = map.FindAction(actionName, true);
        if (action == null)
        {
            Debug.LogError($"InputSettingsMenu: Action '{actionName}' not found in map '{actionMapName}'.");
            return null;
        }

        return action;
    }

    // For single actions: choose first binding that is NOT composite and NOT part-of-composite
    private int FindPrimaryBindingIndex(InputAction action, string groupFilter)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            var b = action.bindings[i];
            if (b.isComposite || b.isPartOfComposite) continue;
            if (!PassesGroupFilter(b, groupFilter)) continue;
            return i;
        }
        return -1;
    }

    // For Move composite: find part by name (up/down/left/right) that is part-of-composite
    private int FindCompositePartBindingIndex(InputAction action, string partName, string groupFilter)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            var b = action.bindings[i];
            if (!b.isPartOfComposite) continue;

            // Composite part names are usually "up", "down", "left", "right"
            if (!string.Equals(b.name, partName, System.StringComparison.OrdinalIgnoreCase))
                continue;

            if (!PassesGroupFilter(b, groupFilter)) continue;
            return i;
        }
        return -1;
    }

    private bool PassesGroupFilter(InputBinding binding, string groupFilter)
    {
        if (string.IsNullOrEmpty(groupFilter)) return true;

        // binding.groups is a semicolon-separated list like "Keyboard&Mouse;Gamepad"
        if (string.IsNullOrEmpty(binding.groups)) return false;

        // cheap contains check is fine here
        return binding.groups.Contains(groupFilter);
    }
}
