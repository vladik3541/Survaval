using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Vector3 offset;
    public CinemachineVirtualCamera vcam;
    private Transform _target;


    public void Initialize(Transform target)
    {
        _target = target;
        vcam.Follow = _target;
    }

}
