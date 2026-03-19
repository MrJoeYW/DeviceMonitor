using Device_Monitor_App.Controllers;
using Device_Monitor_App.Models;
using Device_Monitor_App.Services;
using Device_Monitor_App.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Device_Monitor_App.Forms;

/// <summary>
/// 主窗体：WebView2 宿主，通过构造函数注入依赖
/// </summary>
public partial class FormMain : Form
{
    private readonly IntegratorController _integratorController;
    private readonly DeviceController _deviceController;
    private readonly DeviceTagMappingController _tagMappingController;
    private readonly ISnapshotService _snapshotService;
    private readonly ILogger<FormMain> _logger;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true // 增加容错性
    };

    public FormMain(
        IntegratorController integratorController,
        DeviceController deviceController,
        DeviceTagMappingController tagMappingController,
        ISnapshotService snapshotService,
        ILogger<FormMain> logger)
    {
        _integratorController = integratorController;
        _deviceController = deviceController;
        _tagMappingController = tagMappingController;
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
    /// 消息格式: { "action": "integrator:getAll", "reqId": "xxx", "payload": {} }
    /// </summary>
    private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        string? reqId = null;
        try
        {
            var raw = e.TryGetWebMessageAsString();
            _logger.LogDebug("收到前端原始消息: {Msg}", raw);

            var node = JsonNode.Parse(raw) ?? throw new InvalidOperationException("消息解析为 null");
            var action = node["action"]?.GetValue<string>() ?? throw new InvalidOperationException("缺少 action 字段");
            reqId = node["reqId"]?.GetValue<string>() ?? string.Empty;
            var payload = node["payload"];

            _logger.LogInformation(">>> 路由请求: action={Action}, reqId={ReqId}", action, reqId);

            object? result = action switch
            {
                // ========== 集成设备 ==========
                "integrator:getAll" => _integratorController.GetAll(),

                "integrator:add" => HandleIntegratorAdd(payload),

                "integrator:update" => HandleIntegratorUpdate(payload),

                "integrator:delete" => _integratorController.Delete(
                    payload!["id"]!.GetValue<int>()),

                // ========== 子设备 ==========
                "device:getAll" => _deviceController.GetAll(),

                "device:add" => HandleDeviceAdd(payload),

                "device:update" => HandleDeviceUpdate(payload),

                "device:delete" => _deviceController.Delete(
                    payload!["id"]!.GetValue<int>()),

                // ========== 标签映射 ==========
                "tag:getByDeviceId" => _tagMappingController.GetByDeviceId(
                    payload!["deviceId"]!.GetValue<int>()),

                "tag:add" => HandleTagAdd(payload),

                "tag:update" => HandleTagUpdate(payload),

                "tag:delete" => _tagMappingController.Delete(
                    payload!["id"]!.GetValue<int>()),

                // ========== 实时数据快照 ==========
                "snapshot:getAll" => _snapshotService.GetAllSnapshots(),

                _ => throw new NotSupportedException($"未知 action: {action}")
            };

            Reply(reqId, true, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理前端消息异常: {Msg}", ex.Message);
            Reply(reqId ?? string.Empty, false, null, ex.Message);
        }
    }

    // ---------- 集成设备辅助 ----------

    private int HandleIntegratorAdd(JsonNode? payload)
    {
        var obj = payload.Deserialize<Integrator>(_jsonOpts)
                  ?? throw new ArgumentException("集成设备数据无效");
        return _integratorController.Add(obj.Name, obj.IpAddress, obj.Port, obj.PlcBaseAddress, obj.PlcBlockSize);
    }

    private bool HandleIntegratorUpdate(JsonNode? payload)
    {
        var obj = payload.Deserialize<Integrator>(_jsonOpts)
                  ?? throw new ArgumentException("集成设备数据无效");
        return _integratorController.Update(obj);
    }

    // ---------- 子设备辅助 ----------

    private int HandleDeviceAdd(JsonNode? payload)
    {
        _logger.LogInformation("新增子设备原始 Payload: {Payload}", payload?.ToJsonString());
        var obj = payload.Deserialize<Device>(_jsonOpts)
                  ?? throw new ArgumentException("子设备数据无效");
        _logger.LogInformation("新增子设备反序列化结果: Name={Name}, IsEnabled={Enabled}", obj.Name, obj.IsEnabled);
        return _deviceController.Add(obj);
    }

    private bool HandleDeviceUpdate(JsonNode? payload)
    {
        _logger.LogInformation("更新子设备原始 Payload: {Payload}", payload?.ToJsonString());
        var obj = payload.Deserialize<Device>(_jsonOpts)
                  ?? throw new ArgumentException("子设备数据无效");
        _logger.LogInformation("更新子设备反序列化结果: ID={Id}, Name={Name}, IsEnabled={Enabled}", obj.Id, obj.Name, obj.IsEnabled);
        return _deviceController.Update(obj);
    }

    // ---------- 标签映射辅助 ----------

    private int HandleTagAdd(JsonNode? payload)
    {
        var obj = payload.Deserialize<DeviceTagMapping>(_jsonOpts)
                  ?? throw new ArgumentException("标签映射数据无效");
        return _tagMappingController.Add(obj);
    }

    private bool HandleTagUpdate(JsonNode? payload)
    {
        var obj = payload.Deserialize<DeviceTagMapping>(_jsonOpts)
                  ?? throw new ArgumentException("标签映射数据无效");
        return _tagMappingController.Update(obj);
    }

    // ---------- 响应工具 ----------

    private void Reply(string reqId, bool ok, object? data, string? error = null)
    {
        var response = new
        {
            reqId,
            ok,
            data,
            error
        };
        var json = JsonSerializer.Serialize(response, _jsonOpts);
        _logger.LogDebug("<<< 回复前端: {Json}", json);

        // PostWebMessageAsString 必须在 UI 线程调用
        if (InvokeRequired)
            Invoke(() => webView.CoreWebView2?.PostWebMessageAsString(json));
        else
            webView.CoreWebView2?.PostWebMessageAsString(json);
    }

    /// <summary>
    /// 主动向前端推送 JSON 消息（无需 reqId）
    /// </summary>
    public void PostMessageToFront(object payload)
    {
        var json = JsonSerializer.Serialize(payload, _jsonOpts);
        if (InvokeRequired)
            Invoke(() => webView.CoreWebView2?.PostWebMessageAsString(json));
        else
            webView.CoreWebView2?.PostWebMessageAsString(json);
    }
}
