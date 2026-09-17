using UnityEngine;

// Ejemplo de OnCollision
// Va en una superficie sólida; reacciona cuando algo con Rigidbody choca contra ella
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
