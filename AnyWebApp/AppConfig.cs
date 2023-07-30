using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnyWebApp
{
    public class AppConfig
    {
        public enum Theme
        {
            Auto, Light, Dark
        }

        public string Root { get; set; } = "wwwroot";
        public string Scheme { get; set; } = "https";
        public string UserAgent { get; set; }= string.Empty;
        public string StartupUri { get; set; } = "/";
        public string StartupBackground { get; set; } = string.Empty;


        public bool EnableIndexFiles { get; set; } = true;
        public bool EnableDocumentFallbackFiles { get; set; } = false;

        public bool EnableDeveloperTools { get; set; } = false;
        public bool EnableZoomControl { get; set; } = false;
        public bool EnableAutoReload { get; set; } = false;
        public bool EnableAutoTitle { get; set; } = false;
        public bool EnableFakeCors { get; set; } = false;

        public string[] IndexFiles { get; set; } = new string[]
        {
            "index.htm", "index.html", "default.htm", "default.html"
        };

        public string[] DocumentFallbackFiles { get; set; } = new string[]
        {
            "index.htm", "index.html", "default.htm", "default.html"
        };

        public string[] FakeCorsTargetOrigins { get; set; } = new string[]
        {

        };

        public string VirtualHostName { get; set; } = "localhost.app";

        public string WindowTitle { get; set; } = "AnyWebApp";
        public string WindowIcon { get; set; } = "icon.png";
        public Theme WindowTheme { get; set; } = Theme.Auto;
        public int WindowWidth { get; set; } = -1;
        public int WindowHeight { get; set; } = -1;
        public bool WindowCenterStart { get; set; } = false;
        public double ZoomFactor { get; set; } = 1;

        public static readonly Regex RegexHostName = new Regex(@"^([a-zA-Z0-9-]+\.)([a-zA-Z]{2,})(\.[a-zA-Z]{2,})?$");

        public static void Check(AppConfig config)
        {
            if (!RegexHostName.IsMatch(config.VirtualHostName))
                throw new InvalidOperationException("AppConfig.VirtualHostName is not valid value");
        }
    }
}
