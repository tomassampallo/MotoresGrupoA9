using UnityEngine;

// Objeto instanciado al morir un enemigo. Usa un Trigger (no una colisión física)
// porque no necesitamos que bloquee el paso del jugador, solo detectar que lo tocó.
[RequireComponent(typeof(Collider))]
public class BloodPickup : MonoBehaviour
{
    [SerializeField] private float bloodAmount = 25f;

    private void Reset()
    {
        // Nos aseguramos de que el collider sea un trigger apenas se agrega el script.
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        BloodMeter bloodMeter = other.GetComponent<BloodMeter>();

        if (bloodMeter != null)
        {
            bloodMeter.GainBlood(bloodAmount);
            Destroy(gameObject);
        }
    }
}