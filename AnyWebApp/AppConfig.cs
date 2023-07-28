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
        public string Root { get; set; } = "wwwroot";
        public string Scheme { get; set; } = "http";
        public string StartupUri { get; set; } = "/";



        public bool EnableIndexFiles { get; set; } = true;
        public bool EnableDocumentFallbackFiles { get; set; } = false;

        public string[] IndexFiles { get; set; } = new string[]
        {
            "index.htm", "index.html", "default.htm", "default.html"
        };

        public string[] DocumentFallbackFiles { get; set; } = new string[]
        {
            "index.htm", "index.html", "default.htm", "default.html"
        };

        public string VirtualHostName { get; set; } = "localhost.app";

        public string WindowTitle { get; set; } = "AnyWebApp";
        public string WindowIcon { get; set; } = "icon.png";
        public int WindowWidth { get; set; } = -1;
        public int WindowHeight { get; set; } = -1;
        public double ZoomFactor { get; set; } = 1;

        public static readonly Regex RegexHostName = new Regex(@"^([a-zA-Z0-9-]+\.)([a-zA-Z]{2,})(\.[a-zA-Z]{2,})?$");

        public static void Check(AppConfig config)
        {
            if (!RegexHostName.IsMatch(config.VirtualHostName))
                throw new InvalidOperationException("AppConfig.VirtualHostName is not valid value");
        }
    }
}
