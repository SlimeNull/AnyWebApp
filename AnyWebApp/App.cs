using System.Text.Json;
using System.Text.RegularExpressions;

namespace AnyWebApp
{
    internal static class App
    {
        const string AppConfigFile = "AppConfig.json";

        public static readonly AppConfig Config;

        static App()
        {
            if (File.Exists(AppConfigFile))
                Config = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(AppConfigFile)) ?? throw new InvalidOperationException("AppConfig is null");
            else
                Config = new AppConfig();

            if (!File.Exists(AppConfigFile))
                File.WriteAllText(AppConfigFile, JsonSerializer.Serialize(Config));

            AppConfig.Check(Config);
        }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new AppForm());
        }
    }
}