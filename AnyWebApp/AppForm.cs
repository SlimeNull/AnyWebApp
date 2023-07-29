using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using AnyWebApp.Utils;
using Microsoft.Web.WebView2.Core;

namespace AnyWebApp
{
    public partial class AppForm : Form
    {
        public AppForm()
        {
            InitializeComponent();

            Text = App.Config.WindowTitle;

            if (App.Config.WindowWidth > 0)
                Width = App.Config.WindowWidth;
            if (App.Config.WindowHeight > 0)
                Height = App.Config.WindowHeight;
            if (App.Config.ZoomFactor != 0)
                webView.ZoomFactor = App.Config.ZoomFactor;
            if (App.Config.WindowCenterStart)
                StartPosition = FormStartPosition.CenterScreen;
            if (File.Exists(App.Config.WindowIcon))
                Icon = new Icon(App.Config.WindowIcon);

            UriBase = $"{App.Config.Scheme}://{App.Config.VirtualHostName}";
            _ = webView.EnsureCoreWebView2Async();
        }

        public readonly string UriBase;

        private FileSystemWatcher? fileSystemWatcher;

        private void CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            var coreWebView2 = webView.CoreWebView2;

            coreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
            coreWebView2.WebResourceRequested += CoreWebView2WebResourceRequested;

            coreWebView2.AddHostObjectToScript("$nativeWindow", new Injections.Window(this));

            coreWebView2.Settings.AreDevToolsEnabled = App.Config.EnableDeveloperTools;
            coreWebView2.Settings.IsZoomControlEnabled = App.Config.EnableZoomControl;

            if (App.Config.EnableAutoReload)
                EnableFileSystemWatcher(coreWebView2, App.Config.Root);

            if (App.Config.EnableAutoTitle)
                EnableAutoTitle(coreWebView2);

            if (!string.IsNullOrWhiteSpace(App.Config.UserAgent))
                coreWebView2.Settings.UserAgent = App.Config.UserAgent;

            string absoluteStartupUri = $"{UriBase}{App.Config.StartupUri}";
            if (Uri.TryCreate(absoluteStartupUri, UriKind.Absolute, out Uri? startupUri))
                webView.Source = startupUri;
        }

        private void CoreWebView2WebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            Uri uri = new Uri(e.Request.Uri);

            // 如果是本地请求, 尝试映射本地文件
            if (uri.Host == App.Config.VirtualHostName)
            {
                string path = Path.Combine(App.Config.Root, uri.LocalPath.TrimStart('/'));
                if (File.Exists(path))
                {
                    e.Response = GenerateResponseFromFile(webView.CoreWebView2, path);

                    if (e.Response != null)
                        return;
                }
            }

            if (e.Response != null)
                return;

            // 下面是找不到资源的时候进行的处理

            // 如果是本地请求
            if (uri.Host == App.Config.VirtualHostName)
            {
                if (App.Config.EnableIndexFiles && e.ResourceContext == CoreWebView2WebResourceContext.Document && uri.PathAndQuery == "/")
                {
                    // 根路径, 返回索引文件
                    foreach (var file in App.Config.IndexFiles)
                    {
                        string path = Path.Combine(App.Config.Root, file);
                        if (File.Exists(path))
                        {
                            e.Response = GenerateResponseFromFile(webView.CoreWebView2, path);

                            if (e.Response != null)
                                return;
                        }
                    }
                }
                else if (App.Config.EnableDocumentFallbackFiles && e.ResourceContext == CoreWebView2WebResourceContext.Document)
                {
                    // 启用 fallback 文件, 那么则
                    foreach (var file in App.Config.DocumentFallbackFiles)
                    {
                        string path = Path.Combine(App.Config.Root, file);
                        if (File.Exists(path))
                        {
                            e.Response = GenerateResponseFromFile(webView.CoreWebView2, path);

                            if (e.Response != null)
                                return;
                        }
                    }
                }
            }
        }

        private void EnableFileSystemWatcher(CoreWebView2 coreWebView2, string path)
        {
            FileSystemWatcher watcher = new FileSystemWatcher(path);

            watcher.Filter = "*";
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            watcher.Changed += (s, e) =>
            {
                this.Invoke(() =>
                {
                    coreWebView2.Reload();
                });
            };
        }

        private void EnableAutoTitle(CoreWebView2 coreWebView2)
        {
            coreWebView2.DocumentTitleChanged += (s, e) =>
            {
                Text = coreWebView2.DocumentTitle;
            };
        }

        private static CoreWebView2WebResourceResponse? GenerateResponseFromFile(CoreWebView2 coreWebView2, string path)
        {
            if (!File.Exists(path))
                return null;

            using FileStream fs = File.OpenRead(path);
            MemoryStream ms = new MemoryStream();
            fs.CopyTo(ms);

            string? extension = Path.GetExtension(path);

            string? mimeType;
            if (extension == null || !MimeUtils.TryGetMimeType(extension, out mimeType))
                mimeType = MimeUtils.OctetStream;

            StringBuilder headersBuilder = new StringBuilder();
            headersBuilder.AppendLine($"Content-Type: {mimeType}");
            headersBuilder.AppendLine($"Content-Length: {ms.Length}");

            ms.Seek(0, SeekOrigin.Begin);
            return coreWebView2.Environment.CreateWebResourceResponse(ms, 200, "OK", headersBuilder.ToString());
        }
    }
}