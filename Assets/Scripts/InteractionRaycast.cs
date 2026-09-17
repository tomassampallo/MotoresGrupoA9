using UnityEngine;

// El Interactable Mask debe incluir SOLO la layer "Interactable" (no "Everything"),
// así el rayo no choca contra el propio Player que está siempre en el camino.
public class InteractionRaycast : MonoBehaviour
{
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactableMask;

    private Camera playerCamera;
    private PlayerInputActions inputActions;

    private void Awake()
    {
        playerCamera = Camera.main;
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Interact.performed += ctx => TryInteract();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void TryInteract()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableMask))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            interactable?.Interact();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (playerCamera == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(playerCamera.transform.position, playerCamera.transform.position + playerCamera.transform.forward * interactRange);
    }
}
