namespace Code.Framework.RemoteConfiguration
{
    public class ConnectionStringProvider
    {
        public static string GetConnectionString(string key)
        {
            return RemoteConnectionStringCollection.Instance[key];
        }
    }
}