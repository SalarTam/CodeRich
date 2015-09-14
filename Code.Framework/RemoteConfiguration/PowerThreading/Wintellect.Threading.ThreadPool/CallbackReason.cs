namespace Wintellect.Threading.ThreadPool
{
    public enum CallbackReason
    {
        AddingThreadToPool,
        RemovingThreadFromPool,
        MethodCall,
        MethodReturn
    }
}