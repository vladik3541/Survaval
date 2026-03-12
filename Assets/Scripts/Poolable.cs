using UnityEngine;

public class Poolable : MonoBehaviour
{
    public string PoolName;
    
    public void ReturnToPool()
    {
        ServiceLocator.Get<PoolService>().Return(PoolName, gameObject);
    }
}