public abstract class Singleton<T> where T: class,new()
{
    protected static T _instance;
    public static T GetInstance
    {
        get
        {
            lock (_instance)
            {
                if (_instance == null)
                {
                    _instance = new T();
                }
            }
            return _instance;
        }
    }
}
