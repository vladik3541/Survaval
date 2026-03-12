using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Vector3 offset;
    private Transform _target;

    public void Initialize(Transform target)
    {
        _target = target;
    }
    private void LateUpdate()
    {
        if (_target == null) return;
        transform.position = _target.position + offset;
    }
}
