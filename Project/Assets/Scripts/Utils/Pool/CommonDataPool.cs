//经常使用的类可以如下使用缓存池来存取

/// <summary>
/// 测试：经常使用的数据
/// </summary>
public class TestOftenData
{
    public void Clear() { }
}

public static class TestOftenDataPool
{
    private static readonly ObjectPool<TestOftenData> mObjectPool = new ObjectPool<TestOftenData>(null, ActionOnRelease);

    private static readonly object mLocker = new object();

    public static TestOftenData Get()
    {
        lock(mLocker)
        {
            return mObjectPool.Get();
        }
    }

    public static void Release(ref TestOftenData data)
    {
        lock(mLocker)
        {
            mObjectPool.Release(data);
            data = null;
        }
    }

    private static void ActionOnRelease(TestOftenData data)
    {
        lock(mLocker)
        {
            data.Clear();
        }
    }
}