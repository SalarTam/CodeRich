namespace Code.Framework.RemoteConfiguration
{
    public class AppSettingProvider
    {
        public static string GetAppSetting(string key)
        {
            return RemoteAppSettingCollection.Instance[key];
        }
    }
}