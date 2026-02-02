using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

// 添加日志和动画命名空间
using test.src.Services.PublicFuc.Helpers;
using test.src.Services.PublicFuc.Managers;
using test.src.Services.PublicFuc.Animation;

namespace test.src.UI.Forms.FH4.MedicalForm
{
    partial class Medical : Form
    {
        

        #region 网络诊断相关类定义
        public class NetworkDiagnosticResult
        {
            public bool HasInternetConnection { get; set; }
            public bool TeredoAdapterExists { get; set; }
            public bool TeredoAdapterEnabled { get; set; }
            public string TeredoAdapterStatus { get; set; } = "未找到";
            public string NATType { get; set; } = "未知";
            public bool XboxNetworkingServiceReachable { get; set; }
            public List<string> Issues { get; set; } = new List<string>();
            public string Summary { get; set; } = "未完成诊断";
            public List<ServerTestResult> TeredoServerResults { get; set; } = new List<ServerTestResult>();
            public List<ServerTestResult> GameServerResults { get; set; } = new List<ServerTestResult>();
            public ServerTestResult? FastestTeredoServer { get; set; }
            public ServerTestResult? FastestGameServer { get; set; }

            public bool IsNetworkHealthy =>
                HasInternetConnection &&
                TeredoAdapterExists &&
                TeredoAdapterEnabled &&
                NATType == "开放" &&
                XboxNetworkingServiceReachable;
        }

        public class ServerTestResult
        {
            public string ServerName { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
            public int Port { get; set; } = 3544; // Teredo默认端口
            public bool IsReachable { get; set; }
            public long? PingLatency { get; set; } // ms
            public long? TcpLatency { get; set; } // ms
            public string Status { get; set; } = "未测试";
            public string ErrorMessage { get; set; } = string.Empty;
        }

        public class XboxServerInfo
        {
            public string ServerName { get; set; } = string.Empty;
            public string Host { get; set; } = string.Empty;
            public int Port { get; set; } = 443;
            public bool Required { get; set; } = true;
        }
        #endregion

        #region 网络诊断方法
        private void StartNetworkDiagnosis()
        {
            Logs.LogInfo("开始网络诊断...");

            if (networkDiagnosisWorker == null)
            {
                networkDiagnosisWorker = new System.ComponentModel.BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };

                networkDiagnosisWorker.DoWork += NetworkDiagnosisWorker_DoWork;
                networkDiagnosisWorker.ProgressChanged += NetworkDiagnosisWorker_ProgressChanged;
                networkDiagnosisWorker.RunWorkerCompleted += NetworkDiagnosisWorker_RunWorkerCompleted;
            }

            if (!networkDiagnosisWorker.IsBusy)
            {
                // 重置进度条
                if (progressBar != null && progressBar.InvokeRequired)
                {
                    progressBar.Invoke(new Action(() =>
                    {
                        progressBar.Value = 0;
                        progressBar.Maximum = 100;
                    }));
                }

                networkDiagnosisWorker.RunWorkerAsync();
                Logs.LogInfo("网络诊断后台工作器已启动");
            }
            else
            {
                Logs.LogInfo("网络诊断后台工作器正在运行中");
            }
        }

        private void NetworkDiagnosisWorker_DoWork(object? sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var worker = sender as System.ComponentModel.BackgroundWorker;
            var result = new NetworkDiagnosticResult();

            try
            {
                Logs.LogInfo("开始执行网络诊断工作流程");

                // 步骤1: 检查基本网络连接 (5%)
                worker?.ReportProgress(5, "检查互联网连接...");
                result.HasInternetConnection = CheckInternetConnectivity();

                if (!result.HasInternetConnection)
                {
                    Logs.LogInfo("无法连接到互联网");
                    result.Issues.Add("无法连接到互联网");
                    e.Result = result;
                    return;
                }

                // 步骤2: 扫描网络适配器 (10%)
                worker?.ReportProgress(10, "扫描网络适配器...");
                ScanNetworkAdapters(result, worker);

                // 步骤3: 检查Teredo适配器状态 (20%)
                worker?.ReportProgress(20, "检查Teredo适配器...");
                CheckTeredoAdapter(result, worker);

                // 步骤4: 检查NAT类型 (30%)
                worker?.ReportProgress(30, "检测NAT类型...");
                CheckNATType(result, worker);

                // 步骤5: 读取JSON配置文件 (35%)
                worker?.ReportProgress(35, "读取服务器配置...");
                var serverConfig = ReadServerConfig();

                if (serverConfig != null)
                {
                    // 步骤6: 测试Teredo服务器连接 (50%)
                    worker?.ReportProgress(50, "测试Teredo服务器...");
                    TestTeredoServers(result, serverConfig, worker);

                    // 步骤7: 测试游戏服务器连接 (70%)
                    worker?.ReportProgress(70, "测试游戏服务器...");
                    TestGameServers(result, serverConfig, worker);
                }

                // 步骤8: 检查Xbox网络服务连接性 (85%)
                worker?.ReportProgress(85, "测试Xbox网络服务...");
                CheckXboxNetworkingServices(result, worker);

                // 步骤9: 生成报告 (100%)
                worker?.ReportProgress(100, "生成诊断报告...");
                GenerateDiagnosisSummary(result);

                Logs.LogInfo($"网络诊断完成: {(result.IsNetworkHealthy ? "网络状态良好" : "发现问题")}");
                e.Result = result;
            }
            catch (Exception ex)
            {
                string errorMsg = $"诊断过程中发生错误: {ex.Message}";
                Logs.LogInfo(errorMsg);
                result.Issues.Add(errorMsg);
                result.Summary = "诊断失败";
                e.Result = result;
            }
        }

        private void NetworkDiagnosisWorker_ProgressChanged(object? sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            if (progressBar != null)
            {
                if (progressBar.InvokeRequired)
                {
                    progressBar.Invoke(new Action(() =>
                    {
                        progressBar.Value = e.ProgressPercentage;
                    }));
                }
                else
                {
                    progressBar.Value = e.ProgressPercentage;
                }
            }
        }

        private void NetworkDiagnosisWorker_RunWorkerCompleted(object? sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Logs.LogInfo($"网络诊断工作器执行出错: {e.Error.Message}");
                return;
            }

            if (e.Result is NetworkDiagnosticResult result)
            {
                Logs.LogInfo("开始处理网络诊断结果");
                ProcessDiagnosisResult(result);
            }
        }
        #endregion

        #region 网络诊断核心方法
        private bool CheckInternetConnectivity()
        {
            try
            {
                Logs.LogInfo("开始检查互联网连接，ping 8.8.8.8...");
                using (var ping = new Ping())
                {
                    var reply = ping.Send("8.8.8.8", 2000);
                    bool connected = reply?.Status == IPStatus.Success;
                    Logs.LogInfo($"互联网连接检查: {(connected ? "成功" : "失败")}");
                    return connected;
                }
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"检查互联网连接时出错: {ex.Message}");
                return false;
            }
        }

        private void ScanNetworkAdapters(NetworkDiagnosticResult result, System.ComponentModel.BackgroundWorker? worker)
        {
            try
            {
                Logs.LogInfo("开始扫描网络适配器...");
                var adapters = NetworkInterface.GetAllNetworkInterfaces();
                bool foundTeredo = false;
                int adapterCount = 0;

                foreach (var adapter in adapters)
                {
                    adapterCount++;
                    Logs.LogInfo($"发现网络适配器: {adapter.Name} ({adapter.Description})");

                    if (adapter.Description.Contains("Teredo", StringComparison.OrdinalIgnoreCase) ||
                        adapter.Description.Contains("Tunneling", StringComparison.OrdinalIgnoreCase))
                    {
                        Logs.LogInfo($"找到Teredo适配器: {adapter.Name}, 状态: {adapter.OperationalStatus}");
                        result.TeredoAdapterExists = true;
                        result.TeredoAdapterStatus = adapter.OperationalStatus.ToString();
                        foundTeredo = true;
                        break;
                    }
                }

                Logs.LogInfo($"扫描完成，共发现 {adapterCount} 个网络适配器");

                if (!foundTeredo)
                {
                    Logs.LogInfo("未找到Teredo隧道适配器");
                    result.Issues.Add("未找到Teredo隧道适配器");
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"扫描网络适配器时出错: {ex.Message}";
                Logs.LogInfo(errorMsg);
                result.Issues.Add(errorMsg);
            }
        }

        private void CheckTeredoAdapter(NetworkDiagnosticResult result, System.ComponentModel.BackgroundWorker? worker)
        {
            try
            {
                Logs.LogInfo("开始检查Teredo适配器...");
                // 方法1: 检查注册表中的Teredo设置
                CheckTeredoInRegistry(result);

                // 方法2: 通过netsh命令检查Teredo状态
                CheckTeredoViaNetsh(result);

                if (!result.TeredoAdapterEnabled)
                {
                    Logs.LogInfo("Teredo适配器未启用或被禁用");
                    result.Issues.Add("Teredo适配器未启用或被禁用");
                }
                else
                {
                    Logs.LogInfo($"Teredo适配器状态: {result.TeredoAdapterStatus}");
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"检查Teredo适配器时出错: {ex.Message}";
                Logs.LogInfo(errorMsg);
                result.Issues.Add(errorMsg);
            }
        }

        private void CheckTeredoInRegistry(NetworkDiagnosticResult result)
        {
            try
            {
                Logs.LogInfo("检查注册表中的Teredo设置...");
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip6\Parameters"))
                {
                    if (key != null)
                    {
                        var disabledComponents = key.GetValue("DisabledComponents");
                        if (disabledComponents != null && disabledComponents is int disabledValue)
                        {
                            // 检查Teredo是否被禁用
                            if ((disabledValue & 0x8) == 0x8) // 0x8 表示禁用Teredo
                            {
                                Logs.LogInfo("注册表显示Teredo被禁用");
                                result.TeredoAdapterEnabled = false;
                            }
                            else
                            {
                                Logs.LogInfo("注册表显示Teredo已启用");
                                result.TeredoAdapterEnabled = true;
                            }
                        }
                        else
                        {
                            Logs.LogInfo("未找到DisabledComponents值，Teredo默认启用");
                            result.TeredoAdapterEnabled = true; // 默认启用
                        }
                    }
                    else
                    {
                        Logs.LogInfo("无法打开Tcpip6注册表项");
                        result.TeredoAdapterEnabled = true; // 假设启用
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"读取注册表时出错，假设Teredo是启用的: {ex.Message}");
                result.TeredoAdapterEnabled = true; // 如果无法读取注册表，假设Teredo是启用的
            }
        }

        private void CheckTeredoViaNetsh(NetworkDiagnosticResult result)
        {
            try
            {
                Logs.LogInfo("通过netsh命令检查Teredo状态...");
                var processInfo = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "interface teredo show state",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    Verb = "runas"
                };

                using (var process = Process.Start(processInfo))
                {
                    if (process == null)
                    {
                        Logs.LogInfo("无法启动netsh进程");
                        return;
                    }

                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(5000);

                    Logs.LogInfo($"netsh输出: {output}");

                    if (output.Contains("状态") || output.Contains("State"))
                    {
                        if (output.Contains("qualified", StringComparison.OrdinalIgnoreCase) ||
                            output.Contains("合格", StringComparison.OrdinalIgnoreCase))
                        {
                            result.TeredoAdapterStatus = "合格";
                            result.TeredoAdapterEnabled = true;
                            Logs.LogInfo("Teredo状态: 合格");
                        }
                        else if (output.Contains("client", StringComparison.OrdinalIgnoreCase) ||
                                 output.Contains("客户端", StringComparison.OrdinalIgnoreCase))
                        {
                            result.TeredoAdapterStatus = "客户端模式";
                            result.TeredoAdapterEnabled = true;
                            Logs.LogInfo("Teredo状态: 客户端模式");
                        }
                        else
                        {
                            result.TeredoAdapterStatus = "不合格";
                            result.TeredoAdapterEnabled = false;
                            Logs.LogInfo("Teredo状态: 不合格");
                        }
                    }
                    else
                    {
                        Logs.LogInfo("netsh输出未包含状态信息");
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"执行netsh命令时出错: {ex.Message}");
                // 如果netsh命令失败，使用注册表检查的结果
            }
        }

        private void CheckNATType(NetworkDiagnosticResult result, System.ComponentModel.BackgroundWorker? worker)
        {
            try
            {
                Logs.LogInfo("开始检测NAT类型...");
                // 方法1: 通过Xbox Networking检查NAT类型
                var processInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "-Command \"Get-NetConnectionProfile | Select-Object -ExpandProperty IPv4Connectivity\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processInfo))
                {
                    if (process == null)
                    {
                        Logs.LogInfo("无法启动PowerShell进程");
                    }
                    else
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit(5000);

                        Logs.LogInfo($"PowerShell输出: {output}");

                        if (output.Contains("Internet", StringComparison.OrdinalIgnoreCase))
                        {
                            result.NATType = "开放";
                        }
                        else if (output.Contains("Local", StringComparison.OrdinalIgnoreCase))
                        {
                            result.NATType = "中等";
                        }
                        else if (output.Contains("Restricted", StringComparison.OrdinalIgnoreCase))
                        {
                            result.NATType = "严格";
                        }
                    }
                }

                // 方法2: 尝试P2P连接测试
                if (result.NATType == "未知")
                {
                    Logs.LogInfo("通过P2P连接测试检测NAT类型...");
                    result.NATType = TestNATTypeByP2P();
                }

                Logs.LogInfo($"检测到NAT类型: {result.NATType}");

                if (result.NATType != "开放")
                {
                    string warningMsg = $"NAT类型为{result.NATType}，可能导致联机问题";
                    Logs.LogInfo(warningMsg);
                    result.Issues.Add(warningMsg);
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"检测NAT类型时出错: {ex.Message}";
                Logs.LogInfo(errorMsg);
                result.Issues.Add(errorMsg);
            }
        }

        private string TestNATTypeByP2P()
        {
            try
            {
                Logs.LogInfo("尝试通过UDP Socket测试NAT穿透能力...");
                using (var udpClient = new UdpClient(0))
                {
                    udpClient.Client.ReceiveTimeout = 3000;

                    var stunServers = new[]
                    {
                        new { Host = "stun.l.google.com", Port = 19302 },
                        new { Host = "stun1.l.google.com", Port = 19302 }
                    };

                    foreach (var server in stunServers)
                    {
                        try
                        {
                            Logs.LogInfo($"尝试连接STUN服务器: {server.Host}:{server.Port}");
                            var addresses = Dns.GetHostAddresses(server.Host);
                            if (addresses.Length > 0)
                            {
                                udpClient.Connect(addresses[0], server.Port);
                                byte[] data = Encoding.ASCII.GetBytes("TEST");
                                udpClient.Send(data, data.Length);

                                var result = udpClient.BeginReceive(null, null);
                                if (result.AsyncWaitHandle.WaitOne(2000))
                                {
                                    Logs.LogInfo("STUN服务器响应成功，NAT类型: 开放");
                                    return "开放";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logs.LogInfo($"连接STUN服务器 {server.Host} 失败: {ex.Message}");
                            continue;
                        }
                    }

                    Logs.LogInfo("无法连接到任何STUN服务器，NAT类型: 严格");
                    return "严格";
                }
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"P2P连接测试时出错: {ex.Message}");
                return "未知";
            }
        }

        private JObject? ReadServerConfig()
        {
            try
            {
                string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "plugins.json");
                Logs.LogInfo($"读取JSON配置文件: {jsonPath}");

                if (!File.Exists(jsonPath))
                {
                    Logs.LogInfo($"JSON配置文件不存在: {jsonPath}");
                    return null;
                }

                var jsonObj = JsonEdit.ReadJsonFile(jsonPath);

                if (jsonObj == null)
                {
                    Logs.LogInfo("读取JSON文件失败");
                    return null;
                }

                Logs.LogInfo("JSON配置文件读取成功");
                return jsonObj;
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"读取JSON配置文件时出错: {ex.Message}");
                return null;
            }
        }

        private void TestTeredoServers(NetworkDiagnosticResult result, JObject config, System.ComponentModel.BackgroundWorker? worker)
        {
            try
            {
                var teredoServers = config["FH4MedicalTeredo"]?["teredo_servers"] as JArray;
                if (teredoServers == null || teredoServers.Count == 0)
                {
                    Logs.LogInfo("未找到Teredo服务器配置");
                    return;
                }

                Logs.LogInfo($"开始测试 {teredoServers.Count} 个Teredo服务器...");
                int totalServers = teredoServers.Count;
                int completed = 0;

                foreach (var server in teredoServers)
                {
                    string serverAddress = server.ToString();
                    Logs.LogInfo($"测试Teredo服务器: {serverAddress}");

                    var testResult = TestServerWithPingAndTcp(serverAddress, 3544, "Teredo服务器");
                    result.TeredoServerResults.Add(testResult);

                    completed++;
                    int progress = 50 + (int)((completed / (double)totalServers) * 20);
                    worker?.ReportProgress(progress, $"测试Teredo服务器 {completed}/{totalServers}...");
                }

                // 找出延迟最低的Teredo服务器
                var reachableServers = result.TeredoServerResults
                    .Where(r => r.IsReachable && r.PingLatency.HasValue)
                    .OrderBy(r => r.PingLatency)
                    .ToList();

                if (reachableServers.Any())
                {
                    result.FastestTeredoServer = reachableServers.First();
                    Logs.LogInfo($"最快的Teredo服务器: {result.FastestTeredoServer.Address} (延迟: {result.FastestTeredoServer.PingLatency}ms)");
                }
                else
                {
                    Logs.LogInfo("没有可用的Teredo服务器");
                }
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"测试Teredo服务器时出错: {ex.Message}");
            }
        }

        private void TestGameServers(NetworkDiagnosticResult result, JObject config, System.ComponentModel.BackgroundWorker? worker)
        {
            try
            {
                var serverInfoDict = config["FH4MedicalTeredo"]?["server_info_dict"] as JObject;
                if (serverInfoDict == null)
                {
                    Logs.LogInfo("未找到游戏服务器配置");
                    return;
                }

                Logs.LogInfo($"开始测试 {serverInfoDict.Count} 个游戏服务器...");
                int totalServers = serverInfoDict.Count;
                int completed = 0;

                foreach (var server in serverInfoDict)
                {
                    string serverName = server.Key;
                    string serverAddress = server.Value?.ToString() ?? string.Empty;

                    Logs.LogInfo($"测试游戏服务器: {serverName} ({serverAddress})");

                    var testResult = TestServerWithPingAndTcp(serverAddress, 443, serverName);
                    result.GameServerResults.Add(testResult);

                    completed++;
                    int progress = 70 + (int)((completed / (double)totalServers) * 15);
                    worker?.ReportProgress(progress, $"测试游戏服务器 {completed}/{totalServers}...");
                }

                // 找出延迟最低的游戏服务器
                var reachableServers = result.GameServerResults
                    .Where(r => r.IsReachable && r.PingLatency.HasValue)
                    .OrderBy(r => r.PingLatency)
                    .ToList();

                if (reachableServers.Any())
                {
                    result.FastestGameServer = reachableServers.First();
                    Logs.LogInfo($"最快的游戏服务器: {result.FastestGameServer.ServerName} - {result.FastestGameServer.Address} (延迟: {result.FastestGameServer.PingLatency}ms)");
                }
                else
                {
                    Logs.LogInfo("没有可用的游戏服务器");
                }
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"测试游戏服务器时出错: {ex.Message}");
            }
        }

        private ServerTestResult TestServerWithPingAndTcp(string address, int port, string serverName = "")
        {
            var result = new ServerTestResult
            {
                ServerName = string.IsNullOrEmpty(serverName) ? address : serverName,
                Address = address,
                Port = port
            };

            try
            {
                // 测试Ping
                using (var ping = new Ping())
                {
                    var pingReply = ping.Send(address, 2000);
                    if (pingReply?.Status == IPStatus.Success)
                    {
                        result.PingLatency = pingReply.RoundtripTime;
                        result.IsReachable = true;
                        result.Status = $"Ping延迟: {pingReply.RoundtripTime}ms";
                        Logs.LogInfo($"{address} Ping成功, 延迟: {pingReply.RoundtripTime}ms");
                    }
                    else
                    {
                        result.Status = "Ping失败";
                        Logs.LogInfo($"{address} Ping失败");
                    }
                }

                // 测试TCP连接
                try
                {
                    using (var tcpClient = new TcpClient())
                    {
                        var startTime = DateTime.Now;
                        var asyncResult = tcpClient.BeginConnect(address, port, null, null);
                        var success = asyncResult.AsyncWaitHandle.WaitOne(2000);

                        if (success)
                        {
                            tcpClient.EndConnect(asyncResult);
                            var endTime = DateTime.Now;
                            result.TcpLatency = (long)(endTime - startTime).TotalMilliseconds;
                            result.IsReachable = true;
                            result.Status += $", TCP延迟: {result.TcpLatency}ms";
                            Logs.LogInfo($"{address}:{port} TCP连接成功, 延迟: {result.TcpLatency}ms");
                        }
                        else
                        {
                            tcpClient.Close();
                            result.Status += ", TCP连接失败";
                            Logs.LogInfo($"{address}:{port} TCP连接失败");
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Status += ", TCP连接异常";
                    result.ErrorMessage = ex.Message;
                    Logs.LogInfo($"{address}:{port} TCP连接异常: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                result.Status = "测试失败";
                result.ErrorMessage = ex.Message;
                Logs.LogInfo($"测试服务器 {address} 时出错: {ex.Message}");
            }

            return result;
        }

        private void CheckXboxNetworkingServices(NetworkDiagnosticResult result, System.ComponentModel.BackgroundWorker? worker)
        {
            var xboxServers = new List<XboxServerInfo>
            {
                new XboxServerInfo { ServerName = "Xbox Live 认证服务器", Host = "login.live.com", Port = 443 },
                new XboxServerInfo { ServerName = "Xbox 社交服务", Host = "profile.xboxlive.com", Port = 443 },
                new XboxServerInfo { ServerName = "Xbox 联机服务", Host = "xbox.ipv6.microsoft.com", Port = 443 },
                new XboxServerInfo { ServerName = "Teredo 服务器", Host = "teredo.ipv6.microsoft.com", Port = 3544 }
            };

            int successfulConnections = 0;
            Logs.LogInfo($"开始测试{xboxServers.Count}个Xbox网络服务连接性...");

            foreach (var server in xboxServers)
            {
                Logs.LogInfo($"测试连接: {server.ServerName}({server.Host}:{server.Port})");
                if (TestServerConnectivity(server.Host, server.Port))
                {
                    successfulConnections++;
                    Logs.LogInfo($"连接成功: {server.ServerName}");
                }
                else if (server.Required)
                {
                    string errorMsg = $"{server.ServerName}({server.Host}:{server.Port}) 连接失败";
                    Logs.LogInfo(errorMsg);
                    result.Issues.Add(errorMsg);
                }
            }

            result.XboxNetworkingServiceReachable = successfulConnections >= 3; // 至少连接3个主要服务器
            Logs.LogInfo($"Xbox网络服务连接测试完成，成功{successfulConnections}个，要求至少3个");

            if (!result.XboxNetworkingServiceReachable)
            {
                Logs.LogInfo("Xbox网络服务连接不稳定");
                result.Issues.Add("Xbox网络服务连接不稳定");
            }
        }

        private bool TestServerConnectivity(string host, int port, int timeout = 3000)
        {
            try
            {
                using (var tcpClient = new TcpClient())
                {
                    var result = tcpClient.BeginConnect(host, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(timeout);

                    if (success)
                    {
                        tcpClient.EndConnect(result);
                        return true;
                    }
                    else
                    {
                        tcpClient.Close();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"连接测试失败 {host}:{port}: {ex.Message}");
                return false;
            }
        }

        private void GenerateDiagnosisSummary(NetworkDiagnosticResult result)
        {
            Logs.LogInfo("生成诊断报告摘要...");
            var summary = new StringBuilder();

            if (result.IsNetworkHealthy)
            {
                summary.AppendLine("✅ 网络诊断完成 - 网络状态良好");
                summary.AppendLine($"• 互联网连接: {(result.HasInternetConnection ? "正常" : "异常")}");
                summary.AppendLine($"• Teredo适配器: {result.TeredoAdapterStatus}");
                summary.AppendLine($"• NAT类型: {result.NATType}");
                summary.AppendLine($"• Xbox服务: {(result.XboxNetworkingServiceReachable ? "可访问" : "不可访问")}");
                Logs.LogInfo("网络状态良好");
            }
            else
            {
                summary.AppendLine("⚠️ 网络诊断完成 - 发现问题");

                foreach (var issue in result.Issues)
                {
                    summary.AppendLine($"• {issue}");
                }

                if (result.Issues.Count == 0)
                {
                    summary.AppendLine("• 未发现具体问题，但网络状态不理想");
                }

                Logs.LogInfo($"发现{result.Issues.Count}个网络问题");
            }

            result.Summary = summary.ToString();
        }
        #endregion

        #region 公共方法和属性
        public void StartNetworkCheck()
        {
            Logs.LogInfo("用户启动网络检查");
            StartNetworkDiagnosis();
        }

        public void CancelNetworkCheck()
        {
            if (networkDiagnosisWorker != null && networkDiagnosisWorker.IsBusy)
            {
                Logs.LogInfo("用户取消网络检查");
                networkDiagnosisWorker.CancelAsync();
            }
        }

        // 将字段标记为可为null
        private System.ComponentModel.BackgroundWorker? networkDiagnosisWorker;

        private void ProcessDiagnosisResult(NetworkDiagnosticResult result)
        {
            Logs.LogInfo("处理诊断结果...");

            // 生成诊断报告文件
            string reportPath = GenerateDiagnosisReport(result);

            // 自动打开报告文件
            if (!string.IsNullOrEmpty(reportPath) && File.Exists(reportPath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(reportPath) { UseShellExecute = true });
                    Logs.LogInfo($"已打开诊断报告: {reportPath}");
                }
                catch (Exception ex)
                {
                    Logs.LogInfo($"打开诊断报告失败: {ex.Message}");
                }
            }

            // 触发事件
            OnDiagnosisReportGenerated?.Invoke(reportPath, result);
        }

        private string GenerateDiagnosisReport(NetworkDiagnosticResult result)
        {
            try
            {
                Logs.LogInfo("生成详细诊断报告...");
                var report = new StringBuilder();
                report.AppendLine($"网络诊断报告 - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                report.AppendLine("=".PadRight(50, '=') + "\n");

                report.AppendLine($"诊断时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                report.AppendLine($"网络状态: {(result.IsNetworkHealthy ? "健康" : "存在问题")}\n");

                report.AppendLine("基本网络状态:");
                report.AppendLine($"• 互联网连接: {(result.HasInternetConnection ? "✓ 正常" : "✗ 异常")}");
                report.AppendLine($"• Teredo适配器: {(result.TeredoAdapterExists ? "存在" : "不存在")}");
                report.AppendLine($"• Teredo状态: {result.TeredoAdapterStatus}");
                report.AppendLine($"• Teredo启用: {(result.TeredoAdapterEnabled ? "是" : "否")}");
                report.AppendLine($"• NAT类型: {result.NATType}");
                report.AppendLine($"• Xbox服务可达: {(result.XboxNetworkingServiceReachable ? "是" : "否")}\n");

                // Teredo服务器测试结果
                if (result.TeredoServerResults.Any())
                {
                    report.AppendLine("Teredo服务器连接测试结果:");
                    foreach (var server in result.TeredoServerResults)
                    {
                        string statusIcon = server.IsReachable ? "✓" : "✗";
                        report.AppendLine($"  {statusIcon} {server.Address}: {server.Status}");
                    }

                    if (result.FastestTeredoServer != null)
                    {
                        report.AppendLine($"  → 延迟最低的Teredo服务器: {result.FastestTeredoServer.Address} ({result.FastestTeredoServer.PingLatency}ms)\n");
                    }
                }

                // 游戏服务器测试结果
                if (result.GameServerResults.Any())
                {
                    report.AppendLine("游戏服务器连接测试结果:");
                    foreach (var server in result.GameServerResults)
                    {
                        string statusIcon = server.IsReachable ? "✓" : "✗";
                        report.AppendLine($"  {statusIcon} {server.ServerName} ({server.Address}): {server.Status}");
                    }

                    if (result.FastestGameServer != null)
                    {
                        report.AppendLine($"  → 延迟最低的游戏服务器: {result.FastestGameServer.ServerName} - {result.FastestGameServer.Address} ({result.FastestGameServer.PingLatency}ms)\n");
                    }
                }

                if (result.Issues.Count > 0)
                {
                    report.AppendLine("发现的问题:");
                    foreach (var issue in result.Issues)
                    {
                        report.AppendLine($"• {issue}");
                    }
                    report.AppendLine();
                }

                report.AppendLine("建议:");
                if (result.IsNetworkHealthy)
                {
                    report.AppendLine("• 网络配置正常，可以正常进行联机游戏");
                }
                else
                {
                    if (!result.TeredoAdapterExists || !result.TeredoAdapterEnabled)
                    {
                        report.AppendLine("• 建议启用Teredo隧道适配器");
                        report.AppendLine("• 运行管理员模式的命令提示符，执行: netsh interface teredo set state client");
                    }

                    if (result.NATType != "开放")
                    {
                        report.AppendLine("• 建议配置路由器开启UPnP或设置端口转发");
                        report.AppendLine("• 需要转发端口: UDP 3074, 3544, 4500");
                    }

                    if (!result.XboxNetworkingServiceReachable)
                    {
                        report.AppendLine("• 检查防火墙设置，确保Xbox相关服务未被阻止");
                        report.AppendLine("• 在Windows防火墙中允许Xbox Live网络服务");
                    }

                    if (result.FastestTeredoServer != null)
                    {
                        report.AppendLine($"• 建议使用延迟最低的Teredo服务器: {result.FastestTeredoServer.Address}");
                    }

                    if (result.FastestGameServer != null)
                    {
                        report.AppendLine($"• 建议使用延迟最低的游戏服务器: {result.FastestGameServer.ServerName} ({result.FastestGameServer.Address})");
                    }
                }

                // 保存报告到文件
                string fileName = $"NetworkDiagnosis_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

                File.WriteAllText(filePath, report.ToString(), Encoding.UTF8);
                Logs.LogInfo($"诊断报告已保存到: {filePath}");

                return filePath;
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"保存诊断报告时出错: {ex.Message}");
                return string.Empty;
            }
        }

        // 将事件标记为可为null
        public delegate void DiagnosisReportHandler(string reportPath, NetworkDiagnosticResult result);
        public event DiagnosisReportHandler? OnDiagnosisReportGenerated;
        #endregion
    }
}