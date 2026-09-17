using UnityEngine;

// Representa el campo de "sospecha" de un civil o zona vigilada.
// Usa un Trigger (a diferencia del combate, que usa un OverlapSphere físico)
// para dejar clara la diferenciación entre colisiones y triggers que pide la cátedra.
[RequireComponent(typeof(Collider))]
public class DetectionZone : MonoBehaviour
{
    [SerializeField] private Renderer zoneRenderer;
    [SerializeField] private Color safeColor = Color.green;
    [SerializeField] private Color detectedColor = Color.red;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && zoneRenderer != null)
        {
            zoneRenderer.material.color = detectedColor;
            Debug.Log("El jugador fue detectado en la zona.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && zoneRenderer != null)
        {
            zoneRenderer.material.color = safeColor;
        }
    }
}