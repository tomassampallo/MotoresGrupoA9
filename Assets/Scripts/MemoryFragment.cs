using UnityEngine;

// Ejemplo concreto de un objeto interactuable: un recuerdo del pasado del protagonista.
public class MemoryFragment : MonoBehaviour, IInteractable
{
    [TextArea]
    [SerializeField] private string memoryText = "Un recuerdo borroso aparece en tu mente...";

    public void Interact()
    {
        Debug.Log(memoryText);
        // mas adelante esto podría disparar un evento de UI o de sonido.
        Destroy(gameObject);
    }
}
