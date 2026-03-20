using Device_Monitor_App.Controllers;
using Device_Monitor_App.Models;
using Device_Monitor_App.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Device_Monitor_App.Forms;

public partial class FormMain : Form
{
    private readonly IntegratorController _integratorController;
    private readonly DeviceController _deviceController;
    private readonly DeviceReadGroupController _readGroupController;
    private readonly DevicePointController _pointController;
    private readonly ISnapshotService _snapshotService;
    private readonly ILogger<FormMain> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    public FormMain(
        IntegratorController integratorController,
        DeviceController deviceController,
        DeviceReadGroupController readGroupController,
        DevicePointController pointController,
        ISnapshotService snapshotService,
        ILogger<FormMain> logger)
    {
        _integratorController = integratorController;
        _deviceController = deviceController;
        _readGroupController = readGroupController;
        _pointController = pointController;
        _snapshotService = snapshotService;
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
            webView.Source = new Uri("http://localhost:5173");
#else
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

    private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        string? reqId = null;

        try
        {
            var raw = e.TryGetWebMessageAsString();
            var node = JsonNode.Parse(raw) ?? throw new InvalidOperationException("消息解析失败");
            var action = node["action"]?.GetValue<string>() ?? throw new InvalidOperationException("缺少 action 字段");
            reqId = node["reqId"]?.GetValue<string>() ?? string.Empty;
            var payload = node["payload"];

            _logger.LogInformation(">>> 路由请求: action={Action}, reqId={ReqId}", action, reqId);

            object? result = action switch
            {
                "integrator:getAll" => _integratorController.GetAll(),
                "integrator:add" => _integratorController.Add(DeserializePayload<Integrator>(payload, "网关数据无效")),
                "integrator:update" => _integratorController.Update(DeserializePayload<Integrator>(payload, "网关数据无效")),
                "integrator:delete" => _integratorController.Delete(payload!["id"]!.GetValue<int>()),

                "device:getAll" => _deviceController.GetAll(),
                "device:getTemplates" => _deviceController.GetTemplates(),
                "device:add" => _deviceController.Add(DeserializePayload<Device>(payload, "设备数据无效")),
                "device:update" => _deviceController.Update(DeserializePayload<Device>(payload, "设备数据无效")),
                "device:delete" => _deviceController.Delete(payload!["id"]!.GetValue<int>()),
                "device:rebuildTemplate" => _deviceController.RebuildTemplate(payload!["id"]!.GetValue<int>()),

                "readGroup:getByDeviceId" => _readGroupController.GetByDeviceId(payload!["deviceId"]!.GetValue<int>()),
                "readGroup:add" => _readGroupController.Add(DeserializePayload<DeviceReadGroup>(payload, "采集块数据无效")),
                "readGroup:update" => _readGroupController.Update(DeserializePayload<DeviceReadGroup>(payload, "采集块数据无效")),
                "readGroup:delete" => _readGroupController.Delete(payload!["id"]!.GetValue<int>()),

                "point:getByDeviceId" => _pointController.GetByDeviceId(payload!["deviceId"]!.GetValue<int>()),
                "point:add" => _pointController.Add(DeserializePayload<DevicePoint>(payload, "测点数据无效")),
                "point:update" => _pointController.Update(DeserializePayload<DevicePoint>(payload, "测点数据无效")),
                "point:delete" => _pointController.Delete(payload!["id"]!.GetValue<int>()),

                "snapshot:getAll" => _snapshotService.GetAllSnapshots(),

                _ => throw new NotSupportedException($"未知 action: {action}")
            };

            Reply(reqId, true, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理前端消息异常");
            Reply(reqId ?? string.Empty, false, null, ex.Message);
        }
    }

    private static T DeserializePayload<T>(JsonNode? payload, string errorMessage) where T : class
    {
        var value = payload?.Deserialize<T>(JsonOptions);
        return value ?? throw new ArgumentException(errorMessage);
    }

    private void Reply(string reqId, bool ok, object? data, string? error = null)
    {
        var response = new
        {
            reqId,
            ok,
            data,
            error
        };

        var json = JsonSerializer.Serialize(response, JsonOptions);

        if (InvokeRequired)
        {
            Invoke(() => webView.CoreWebView2?.PostWebMessageAsString(json));
        }
        else
        {
            webView.CoreWebView2?.PostWebMessageAsString(json);
        }
    }

    public void PostMessageToFront(object payload)
    {
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        if (InvokeRequired)
        {
            Invoke(() => webView.CoreWebView2?.PostWebMessageAsString(json));
        }
        else
        {
            webView.CoreWebView2?.PostWebMessageAsString(json);
        }
    }
}
