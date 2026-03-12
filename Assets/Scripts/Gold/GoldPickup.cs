using DG.Tweening;
using UnityEngine;

/// <summary>
/// Компонент на prefab-і золота. При вході гравця у тригер — летить до нього,
/// нараховує золото через GoldSystem та знищує себе.
/// </summary>
public class GoldPickup : MonoBehaviour
{
    [Header("Цінність")]
    [SerializeField] private int goldValue = 5;

    [Header("DOTween")]
    [SerializeField] private float flyDuration = 0.3f;

    private bool isPickedUp;

    private void OnTriggerEnter(Collider other)
    {
        if (isPickedUp) return;
        if (!other.CompareTag("Player")) return;

        isPickedUp = true;

        // Вимикаємо колайдер, щоб не спрацював двічі
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Transform playerTr = other.transform;
        int value          = goldValue;

        transform.DOMove(playerTr.position, flyDuration)
                 .SetEase(Ease.InBack)
                 .OnComplete(() =>
                 {
                     GoldSystem.Instance?.AddGold(value);
                     Destroy(gameObject);
                 });
    }
}
