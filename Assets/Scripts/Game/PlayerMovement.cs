using UnityEngine;

/// <summary>
/// Переміщення гравця за допомогою InputService.
/// Гравець рухається по площині XZ і повертається в напрямку руху.
///
/// Прикріпи до кореневого GameObject персонажа.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed     = 5f;
    [SerializeField] private float rotationSpeed = 720f;

    private Rigidbody    _rb;
    private InputService _input;
    private Animator _animator;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _rb.freezeRotation = true;
    }

    private void Start()
    {
        _input = ServiceLocator.Get<InputService>();
    }

    /// <summary>Встановлює швидкість. Викликається PlayerStats.</summary>
    public void SetSpeed(float value) => moveSpeed = value;

    private void FixedUpdate()
    {
        var raw = _input.MoveDirection;
        var dir = new Vector3(raw.x, 0f, raw.y);
        if (dir != Vector3.zero)
        {
            _animator.SetBool("Run", true);
        }
        else
        {
            _animator.SetBool("Run", false);
        }
        // рух
        _rb.MovePosition(_rb.position + dir * (moveSpeed * Time.fixedDeltaTime));

        // поворот в напрямку руху
        if (dir.sqrMagnitude > 0.01f)
        {
            var targetRotation = Quaternion.LookRotation(dir);
            _rb.rotation = Quaternion.RotateTowards(_rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
}
