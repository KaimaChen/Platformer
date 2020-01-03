using System.Collections.Generic;

public static class HashSetPool<T>
{
    private static readonly ObjectPool<HashSet<T>> mObjectPool = new ObjectPool<HashSet<T>>(null, set => set.Clear());

    private static readonly object mLocker = new object();

    public static HashSet<T> Get()
    {
        lock(mLocker)
        {
            return mObjectPool.Get();
        }
    }

    public static void Release(ref HashSet<T> set)
    {
        lock(mLocker)
        {
            mObjectPool.Release(set);
            set = null;
        }
    }
}