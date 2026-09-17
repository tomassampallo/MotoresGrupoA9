using UnityEngine;

// Vida simple de un enemigo/monstruo. Al morir, instancia un pickup de sangre
// para que el jugador lo recoja (conecta el combate con el sistema de sangre).
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 30f;
    [SerializeField] private GameObject bloodPickupPrefab;

    private float currentHealth;
    private bool isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        if (bloodPickupPrefab != null)
        {
            // Instantiate: creamos una copia del prefab en la posición del enemigo muerto.
            Instantiate(bloodPickupPrefab, transform.position, Quaternion.identity);
        }

        // Destroy: eliminamos al enemigo de la escena.
        Destroy(gameObject);
    }
}