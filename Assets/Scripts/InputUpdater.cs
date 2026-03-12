using UnityEngine;

public class InputUpdater : MonoBehaviour
{
    private InputService service;
    
    public void Initialize(InputService inputService)
    {
        service = inputService;
    }
    
    void Update()
    {
        service?.UpdateInput();
    }
}