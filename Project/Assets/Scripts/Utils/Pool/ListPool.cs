using System.Collections.Generic;

public static class ListPool<T>
{
    private static readonly ObjectPool<List<T>> mObjectPool = new ObjectPool<List<T>>(null, list=>list.Clear());

    private static readonly object mLocker = new object();

    public static List<T> Get()
    {
        lock(mLocker)
        {
            return mObjectPool.Get();
        }
    }

    public static void Release(ref List<T> list)
    {
        lock(mLocker)
        {
            mObjectPool.Release(list);
            list = null;
        }
    }
}