using System.Text;
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
            if (File.Exists(App.Config.WindowIcon))
                Icon = new Icon(App.Config.WindowIcon);

            UriBase = $"{App.Config.Scheme}://{App.Config.VirtualHostName}";
        }

        public readonly string UriBase;

        private void AppFormLoaded(object sender, EventArgs e)
        {
            _ = webView.EnsureCoreWebView2Async();
        }

        private void CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            var coreWebView2 = webView.CoreWebView2;

            coreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
            coreWebView2.WebResourceRequested += CoreWebView2WebResourceRequested;

            string absoluteStartupUri = $"{UriBase}{App.Config.StartupUri}";
            if (Uri.TryCreate(absoluteStartupUri, UriKind.Absolute, out Uri? startupUri))
                webView.Source = startupUri;
        }

        private void CoreWebView2WebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            Uri uri = new Uri(e.Request.Uri);

            // ����Ǳ�������, ����ӳ�䱾���ļ�
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

            // �������Ҳ�����Դ��ʱ����еĴ���

            // ����Ǳ�������
            if (uri.Host == App.Config.VirtualHostName)
            {
                if (App.Config.EnableIndexFiles && e.ResourceContext == CoreWebView2WebResourceContext.Document && uri.PathAndQuery == "/")
                {
                    // ��·��, ���������ļ�
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
                    // ���� fallback �ļ�, ��ô��
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