namespace Miyo.Core.ObjectPool
{
    public interface IPoolable
    {
        void OnSpawn();
        void OnDespawn();
    }
}
