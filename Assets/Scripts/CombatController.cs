using UnityEngine;

// Ataque cuerpo a cuerpo simple: al presionar Attack, revisamos si hay
// algún enemigo dentro de un radio corto frente al jugador.
public class CombatController : MonoBehaviour
{
    [Header("Ataque")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private Transform attackOrigin; // objeto vacío delante del personaje

    private PlayerInputActions inputActions;
    private BloodMeter bloodMeter;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        bloodMeter = GetComponent<BloodMeter>();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Attack.performed += ctx => TryAttack();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void TryAttack()
    {
        Collider[] hits = Physics.OverlapSphere(attackOrigin.position, attackRange, enemyMask);

        float damageMultiplier = bloodMeter != null ? bloodMeter.GetDamageMultiplier() : 1f;

        foreach (Collider hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage * damageMultiplier);
            }
        }
    }

    // Ayuda visual en el editor para ubicar bien el punto de ataque (no afecta el build).
    private void OnDrawGizmosSelected()
    {
        if (attackOrigin == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackOrigin.position, attackRange);
    }
}