namespace BMSOverlay
{
    public static class ConfigFileUtils
    {
        public static string GetConfigPath(string fileName)
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string standardConfigPath = Path.Combine(appDataPath, "BMSOverlay", fileName);

            if (File.Exists(standardConfigPath))
            {
                return standardConfigPath;
            }
            else
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", fileName);
            }
        }
    }
}