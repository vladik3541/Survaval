using DG.Tweening;
using UnityEngine;

/// <summary>
/// Компонент на гравці. Кожний Update перевіряє сферу радіуса pickupRadius,
/// підбирає Gem-и через DOTween-стрибок і нараховує XP.
/// </summary>
public class GemCollector : MonoBehaviour
{
    [Header("Підбір")]
    [SerializeField] private float     pickupRadius = 3f;
    [SerializeField] private LayerMask gemLayer;

    [Header("DOTween")]
    [SerializeField] private float jumpPower    = 2f;
    [SerializeField] private float jumpDuration = 0.4f;

    private readonly Collider[] _buffer = new Collider[32];

    /// <summary>Встановлює радіус підбору. Викликається PlayerStats.</summary>
    public void SetRadius(float value) => pickupRadius = Mathf.Max(0f, value);

    private void Update()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, pickupRadius, _buffer, gemLayer);

        for (int i = 0; i < count; i++)
        {
            var gem = _buffer[i].GetComponent<Gem>();
            if (gem == null) continue;

            // Вже летить до нас — вимикаємо колайдер, щоб не зчитати двічі
            _buffer[i].enabled = false;

            float xpValue      = gem.Data != null ? gem.Data.xpValue : 10f;
            Transform gemTr    = gem.transform;
            Transform playerTr = transform;
            gemTr.parent = playerTr;
            gemTr.DOLocalJump(Vector3.zero, jumpPower, 1, jumpDuration)
                 .OnComplete(() =>
                 {
                     // Growth powerup — множник отриманого XP
                     float growth = PowerUpManager.Instance?.GrowthMultiplier ?? 1f;
                     LevelSystem.Instance?.AddXP(xpValue * growth);
                     Destroy(gemTr.gameObject);
                 });
        }
    }

    // ── Gizmo (для зручності в Editor) ───────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
