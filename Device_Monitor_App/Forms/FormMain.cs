using Device_Monitor_App.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using System.Text.Json;

namespace Device_Monitor_App.Forms;

/// <summary>
/// 主窗体：WebView2 宿主，通过构造函数注入依赖
/// </summary>
public partial class FormMain : Form
{
    private readonly DeviceController _deviceController;
    private readonly ILogger<FormMain> _logger;

    public FormMain(DeviceController deviceController, ILogger<FormMain> logger)
    {
        _deviceController = deviceController;
        _logger = logger;
        InitializeComponent();
    }

    private async void FormMain_Load(object sender, EventArgs e)
    {
        _logger.LogInformation("主窗体加载，初始化 WebView2");
        try
        {
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

#if DEBUG
            // 开发时加载 Vite 开发服务器
            webView.Source = new Uri("http://localhost:5173");
#else
            // 生产时加载打包后的 index.html
            var indexPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "index.html");
            webView.Source = new Uri(indexPath);
#endif
            _logger.LogInformation("WebView2 初始化完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WebView2 初始化失败");
            MessageBox.Show($"WebView2 初始化失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// 接收来自前端 JS 的消息并分发到对应 Controller 方法
    /// </summary>
    private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            var raw = e.TryGetWebMessageAsString();
            _logger.LogDebug("收到前端消息: {Msg}", raw);
            // TODO: 按需解析 JSON action 并路由到对应 Controller 方法
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理前端消息异常");
        }
    }

    /// <summary>
    /// 向前端发送 JSON 消息
    /// </summary>
    public void PostMessageToFront(object payload)
    {
        var json = JsonSerializer.Serialize(payload);
        webView.CoreWebView2?.PostWebMessageAsString(json);
    }
}
