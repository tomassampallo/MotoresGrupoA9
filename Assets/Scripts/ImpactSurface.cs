using UnityEngine;

// Ejemplo de OnCollision (a diferencia de los triggers usados en BloodPickup y DetectionZone).
// Va en una superficie sólida; reacciona cuando algo con Rigidbody choca contra ella
// (por ejemplo, un objeto que el jugador empuja o lanza durante el combate).
public class ImpactSurface : MonoBehaviour
{
    [SerializeField] private float minImpactForce = 2f;

    private void OnCollisionEnter(Collision collision)
    {
        float impactForce = collision.relativeVelocity.magnitude;

        if (impactForce >= minImpactForce)
        {
            Debug.Log($"Impacto detectado: {collision.gameObject.name} golpeó con fuerza {impactForce:F1}");
        }
    }
}