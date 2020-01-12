using System.Collections.Generic;

public static class ListPool<T>
{
    private static readonly ObjectPool<List<T>> m_pool = new ObjectPool<List<T>>(null, list=>list.Clear());

    private static readonly object m_locker = new object();

    public static List<T> Get()
    {
        lock(m_locker)
        {
            return m_pool.Get();
        }
    }

    public static void Release(ref List<T> list)
    {
        lock(m_locker)
        {
            m_pool.Release(list);
            list = null;
        }
    }
}