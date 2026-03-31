using UnityEngine;
using UnityEngine.InputSystem;

public class TeleportToggleMode : MonoBehaviour
{
    public GameObject teleportInteractor;
    public InputActionProperty toggleTeleportAction;

    private bool teleportActive = false;

    private void OnEnable()
    {
        if (toggleTeleportAction.action != null)
        {
            toggleTeleportAction.action.Enable();
            toggleTeleportAction.action.performed += ToggleTeleport;
        }
    }

    private void OnDisable()
    {
        if (toggleTeleportAction.action != null)
        {
            toggleTeleportAction.action.performed -= ToggleTeleport;
            toggleTeleportAction.action.Disable();
        }
    }

    private void Start()
    {
        if (teleportInteractor != null)
            teleportInteractor.SetActive(false);
    }

    private void ToggleTeleport(InputAction.CallbackContext context)
    {
        teleportActive = !teleportActive;

        if (teleportInteractor != null)
            teleportInteractor.SetActive(teleportActive);
    }
}