using System.Collections.Generic;

public static class HashSetPool<T>
{
    private static readonly ObjectPool<HashSet<T>> m_pool = new ObjectPool<HashSet<T>>(null, set => set.Clear());

    private static readonly object m_locker = new object();

    public static HashSet<T> Get()
    {
        lock(m_locker)
        {
            return m_pool.Get();
        }
    }

    public static void Release(ref HashSet<T> set)
    {
        lock(m_locker)
        {
            m_pool.Release(set);
            set = null;
        }
    }
}