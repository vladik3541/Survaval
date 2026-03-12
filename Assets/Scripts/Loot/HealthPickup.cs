using DG.Tweening;
using UnityEngine;

/// <summary>
/// Компонент на prefab-і аптечки. При вході гравця у тригер — летить до нього
/// та відновлює HP через PlayerHealth.
/// </summary>
public class HealthPickup : MonoBehaviour
{
    [Header("Відновлення HP")]
    [SerializeField] private float healAmount = 20f;

    [Header("DOTween")]
    [SerializeField] private float flyDuration = 0.3f;

    private bool _isPickedUp;

    private void OnTriggerEnter(Collider other)
    {
        if (_isPickedUp) return;
        if (!other.CompareTag("Player")) return;

        _isPickedUp = true;

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        var playerHealth = other.GetComponent<PlayerHealth>();
        float amount     = healAmount;

        transform.DOMove(other.transform.position, flyDuration)
                 .SetEase(Ease.InBack)
                 .OnComplete(() =>
                 {
                     playerHealth?.Heal(amount);
                     Destroy(gameObject);
                 });
    }
}
