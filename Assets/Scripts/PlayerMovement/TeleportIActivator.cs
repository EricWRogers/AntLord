using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class TeleportIActivator : MonoBehaviour
{
    [Header("References")]
    public XRRayInteractor teleportInteractor;
    public InputActionProperty teleportActivatorAction;

    private void OnEnable()
    {
        if (teleportInteractor != null)
            teleportInteractor.gameObject.SetActive(false);

        if (teleportActivatorAction.action != null)
            teleportActivatorAction.action.Enable();
    }

    private void OnDisable()
    {
        if (teleportActivatorAction.action != null)
            teleportActivatorAction.action.Disable();
    }

    private void Update()
    {
        if (teleportInteractor == null || teleportActivatorAction.action == null)
            return;

        float inputValue = teleportActivatorAction.action.ReadValue<Vector2>().y;

        teleportInteractor.gameObject.SetActive(inputValue > 0.7f);
    }
}