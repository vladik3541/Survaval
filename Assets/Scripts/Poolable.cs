using UnityEngine;

public class Poolable : MonoBehaviour
{
    public PoolName PoolName;
    
    public void ReturnToPool()
    {
        ServiceLocator.Get<PoolService>().Return(PoolName, gameObject);
    }
    public void ReturnToPool(float seconds)
    {
        Invoke("Return", seconds);
    }

    private void Return()
    {
        ServiceLocator.Get<PoolService>().Return(PoolName, gameObject);
    }
}