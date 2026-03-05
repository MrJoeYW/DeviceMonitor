using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

namespace Device_Monitor_App;
public partial class Form1 : Form
{
    private WebView2 _webView;

    public Form1()
    {
        InitializeComponent();
        _webView = new WebView2();
        _webView.Dock = DockStyle.Fill;
        this.Controls.Add(_webView);

        InitWebViewAsync();
    }

    private async void InitWebViewAsync()
    {
        // 初始化运行环境
        await _webView.EnsureCoreWebView2Async(null);
        // 指向你的 Vue 开发服务器
        _webView.Source = new Uri("http://localhost:5173");
    }
}