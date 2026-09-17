using UnityEngine;
using UnityEngine.Events;

// mecánica principal del prototipo, el jugador se fortalece consumiendo sangre
// de los enemigos derrotados, pero si pasa mucho tiempo sin alimentarse, se debilita.
public class BloodMeter : MonoBehaviour
{
    [Header("Sangre / Poder")]
    [SerializeField] private float maxBlood = 100f;
    [SerializeField] private float currentBlood = 100f;

    [Header("Sed (decaimiento con el tiempo)")]
    [SerializeField] private float drainPerSecond = 1.5f;

    [Header("Umbral de debilidad")]
    [SerializeField] private float weakThreshold = 20f;
    [SerializeField] private float weakenedSpeedMultiplier = 0.5f;
    [SerializeField] private float weakenedDamageMultiplier = 0.5f;

    public bool IsWeakened { get; private set; }
    public float CurrentBlood => currentBlood;
    public float BloodPercent => currentBlood / maxBlood;

    // Otros scripts (PlayerController, CombatController, una futura UI) pueden
    // suscribirse a estos eventos sin que BloodMeter necesite conocerlos.
    public UnityEvent onBecomeWeakened;
    public UnityEvent onRecoverStrength;

    private void Update()
    {
        DrainOverTime();
    }

    private void DrainOverTime()
    {
        if (currentBlood <= 0f) return;

        currentBlood -= drainPerSecond * Time.deltaTime;
        currentBlood = Mathf.Clamp(currentBlood, 0f, maxBlood);

        UpdateWeakenedState();
    }

    // Llamado por BloodPickup cuando el jugador recoge la sangre de un enemigo derrotado.
    public void GainBlood(float amount)
    {
        currentBlood = Mathf.Clamp(currentBlood + amount, 0f, maxBlood);
        UpdateWeakenedState();
    }

    private void UpdateWeakenedState()
    {
        bool shouldBeWeakened = currentBlood <= weakThreshold;

        if (shouldBeWeakened && !IsWeakened)
        {
            IsWeakened = true;
            onBecomeWeakened?.Invoke();
        }
        else if (!shouldBeWeakened && IsWeakened)
        {
            IsWeakened = false;
            onRecoverStrength?.Invoke();
        }
    }

    // Utilidades para que otros scripts apliquen la penalización de debilidad.
    public float GetSpeedMultiplier() => IsWeakened ? weakenedSpeedMultiplier : 1f;
    public float GetDamageMultiplier() => IsWeakened ? weakenedDamageMultiplier : 1f;
}
