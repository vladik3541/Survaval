using DG.Tweening;
using UnityEngine;

public class ItemCollector : MonoBehaviour
{
    [Header("Підбір")]
    [SerializeField] private float     pickupRadius = 3f;
    [SerializeField] private LayerMask gemLayer;
    [SerializeField] private LayerMask goldLayer;
    [SerializeField] private LayerMask healthLayer;

    [Header("DOTween")]
    [SerializeField] private float jumpPower    = 2f;
    [SerializeField] private float jumpDuration = 0.4f;

    private readonly Collider[] _buffer = new Collider[32];

    /// <summary>Встановлює радіус підбору. Викликається PlayerStats.</summary>
    public void SetRadius(float value) => pickupRadius = Mathf.Max(0f, value);

    private void Update()
    {
        CollectGems();
        CollectGold();
        CollectHealth();
    }

    private void CollectGems()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, pickupRadius, _buffer, gemLayer);
        for (int i = 0; i < count; i++)
        {
            var gem = _buffer[i].GetComponent<Gem>();
            if (gem == null) continue;

            _buffer[i].enabled = false;

            float     xpValue  = gem.Data != null ? gem.Data.xpValue : 10f;
            Transform gemTr    = gem.transform;
            Transform playerTr = transform;
            gemTr.parent = playerTr;
            gemTr.DOLocalJump(Vector3.zero, jumpPower, 1, jumpDuration)
                 .OnComplete(() =>
                 {
                     float growth = PowerUpManager.Instance?.GrowthMultiplier ?? 1f;
                     LevelSystem.Instance?.AddXP(xpValue * growth);
                     Destroy(gemTr.gameObject);
                 });
        }
    }

    private void CollectGold()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, pickupRadius, _buffer, goldLayer);
        for (int i = 0; i < count; i++)
        {
            var gold = _buffer[i].GetComponent<GoldPickup>();
            if (gold == null) continue;

            _buffer[i].enabled = false;

            int       value    = gold.GoldValue;
            Transform goldTr   = gold.transform;
            goldTr.parent = transform;
            goldTr.DOLocalJump(Vector3.zero, jumpPower, 1, jumpDuration)
                  .OnComplete(() =>
                  {
                      GoldSystem.Instance?.AddGold(value);
                      Destroy(goldTr.gameObject);
                  });
        }
    }

    private void CollectHealth()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, pickupRadius, _buffer, healthLayer);
        for (int i = 0; i < count; i++)
        {
            var pickup = _buffer[i].GetComponent<HealthPickup>();
            if (pickup == null) continue;

            _buffer[i].enabled = false;

            float     amount    = pickup.HealAmount;
            Transform pickupTr  = pickup.transform;
            var       ph        = GetComponent<PlayerHealth>();
            pickupTr.parent = transform;
            pickupTr.DOLocalJump(Vector3.zero, jumpPower, 1, jumpDuration)
                    .OnComplete(() =>
                    {
                        ph?.Heal(amount);
                        Destroy(pickupTr.gameObject);
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
