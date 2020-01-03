using System.Collections.Generic;

public static class DictionaryPool<K, V>
{
    private static readonly ObjectPool<Dictionary<K, V>> mObjectPool = new ObjectPool<Dictionary<K, V>>(null, dict => dict.Clear());

    private static readonly object mLocker = new object();

    public static Dictionary<K, V> Get()
    {
        lock(mLocker)
        {
            return mObjectPool.Get();
        }
    }

    public static void Release(ref Dictionary<K, V> dict)
    {
        lock(mLocker)
        {
            mObjectPool.Release(dict);
            dict = null;
        }
    }
}