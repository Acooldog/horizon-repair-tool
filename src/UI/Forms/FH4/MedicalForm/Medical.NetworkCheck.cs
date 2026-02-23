using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using test.src.Services.PublicFuc.Helpers;
using test.src.Services.PublicFuc.Managers;

namespace test.src.UI.Forms.FH4.MedicalForm
{
    public partial class Medical
    {
        #region 第一步：网络连接检查的具体实现方法

        public class NetworkDiagnosticResult
        {
            public bool HasInternetConnection { get; set; }
            public bool TeredoAdapterExists { get; set; }
            public bool TeredoAdapterEnabled { get; set; }
            public string TeredoAdapterStatus { get; set; } = "未找到";
            public string NATType { get; set; } = "未知";
            public bool XboxNetworkingServiceReachable { get; set; }
            public List<ServerTestResult> TeredoServerResults { get; set; } = new List<ServerTestResult>();
            public List<ServerTestResult> GameServerResults { get; set; } = new List<ServerTestResult>();
            public ServerTestResult? FastestTeredoServer { get; set; }
            public ServerTestResult? FastestGameServer { get; set; }
            public bool IsHealthy => HasInternetConnection && TeredoAdapterExists && TeredoAdapterEnabled &&
                                   NATType == "开放" && XboxNetworkingServiceReachable;
        }

        private void Step1_NetworkCheck(BackgroundWorker worker, CombinedDiagnosticResult combinedResult)
        {
            Logs.LogInfo("执行第一步：网络连接检查");
            var result = new NetworkDiagnosticResult();
            combinedResult.Step1Result = result;

            try
            {
                // 1. 检查基本网络连接
                worker.ReportProgress(5, new ProgressData { Step = 1, Message = "检查互联网连接..." });
                result.HasInternetConnection = CheckInternetConnectivity();

                if (!result.HasInternetConnection)
                {
                    combinedResult.AddIssue("无法连接到互联网", "请检查网络连接", async (p) => await RepairNetworkConfig(p));
                    combinedResult.AddSuggestion("检查网络连接是否正常");
                    return;
                }

                // 2. 扫描网络适配器
                worker.ReportProgress(10, new ProgressData { Step = 1, Message = "扫描网络适配器..." });
                ScanNetworkAdapters(result, combinedResult);

                // 3. 检查Teredo适配器
                worker.ReportProgress(15, new ProgressData { Step = 1, Message = "检查Teredo适配器..." });
                CheckTeredoAdapter(result, combinedResult);

                // 4. 检查NAT类型
                worker.ReportProgress(20, new ProgressData { Step = 1, Message = "检测NAT类型..." });
                CheckNATType(result, combinedResult);

                // 5. 测试服务器连接
                worker.ReportProgress(25, new ProgressData { Step = 1, Message = "测试服务器连接..." });
                TestAllServers(worker, result, combinedResult);

                // 6. 检查Xbox服务
                worker.ReportProgress(30, new ProgressData { Step = 1, Message = "测试Xbox网络服务..." });
                CheckXboxNetworkingServices(result, combinedResult);

                Logs.LogInfo($"第一步完成: {(result.IsHealthy ? "网络状态良好" : "发现问题")}");
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"第一步执行错误: {ex.Message}");
                combinedResult.AddIssue("网络检查过程异常", ex.Message);
            }
        }

        #region 网络检查核心方法

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

        private void ScanNetworkAdapters(NetworkDiagnosticResult result, CombinedDiagnosticResult combinedResult)
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
                    combinedResult.AddIssue("未找到Teredo隧道适配器", "Teredo适配器是Xbox联机必需的", async (p) => await RepairTeredoAdapter(p));
                    combinedResult.AddSuggestion("启用Teredo适配器: netsh interface teredo set state client");
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"扫描网络适配器时出错: {ex.Message}";
                Logs.LogInfo(errorMsg);
                combinedResult.AddIssue("扫描网络适配器失败", ex.Message);
            }
        }

        private void CheckTeredoAdapter(NetworkDiagnosticResult result, CombinedDiagnosticResult combinedResult)
        {
            try
            {
                Logs.LogInfo("开始检查Teredo适配器...");
                // 方法1: 检查注册表中的Teredo设置
                CheckTeredoInRegistry(result, combinedResult);

                // 方法2: 通过netsh命令检查Teredo状态
                CheckTeredoViaNetsh(result, combinedResult);

                if (!result.TeredoAdapterEnabled)
                {
                    Logs.LogInfo("Teredo适配器未启用或被禁用");
                    combinedResult.AddIssue("Teredo适配器未启用或被禁用", "Teredo适配器是Xbox联机必需的", async (p) => await RepairTeredoAdapter(p));
                    combinedResult.AddSuggestion("以管理员身份运行: netsh interface teredo set state client");
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
                combinedResult.AddIssue("检查Teredo适配器失败", ex.Message);
            }
        }

        private void CheckTeredoInRegistry(NetworkDiagnosticResult result, CombinedDiagnosticResult combinedResult)
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

        private void CheckTeredoViaNetsh(NetworkDiagnosticResult result, CombinedDiagnosticResult combinedResult)
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

        private void CheckNATType(NetworkDiagnosticResult result, CombinedDiagnosticResult combinedResult)
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
                    combinedResult.AddIssue(warningMsg, "NAT类型会影响P2P连接", async (p) => await RepairTeredoAdapter(p));

                    if (result.NATType == "严格")
                    {
                        combinedResult.AddSuggestion("在路由器中启用UPnP或设置端口转发");
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"检测NAT类型时出错: {ex.Message}";
                Logs.LogInfo(errorMsg);
                combinedResult.AddIssue("检测NAT类型失败", ex.Message);
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

        private void TestAllServers(BackgroundWorker worker, NetworkDiagnosticResult result, CombinedDiagnosticResult combinedResult)
        {
            try
            {
                Logs.LogInfo("开始测试所有服务器连接...");

                // 读取JSON配置文件
                worker.ReportProgress(25, new ProgressData { Step = 1, Message = "读取服务器配置..." });
                var serverConfig = ReadServerConfig();

                if (serverConfig != null)
                {
                    // 测试Teredo服务器连接
                    worker.ReportProgress(30, new ProgressData { Step = 1, Message = "测试Teredo服务器..." });
                    TestTeredoServers(result, serverConfig, combinedResult);

                    // 测试游戏服务器连接
                    worker.ReportProgress(35, new ProgressData { Step = 1, Message = "测试游戏服务器..." });
                    TestGameServers(result, serverConfig, combinedResult);
                }
                else
                {
                    Logs.LogInfo("未找到服务器配置文件");
                    combinedResult.AddIssue("未找到服务器配置文件", "无法测试服务器连接");
                }
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"测试服务器连接时出错: {ex.Message}");
                combinedResult.AddIssue("测试服务器连接失败", ex.Message);
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

        private void TestTeredoServers(NetworkDiagnosticResult result, JObject config, CombinedDiagnosticResult combinedResult)
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
                int successfulConnections = 0;

                foreach (var server in teredoServers)
                {
                    string serverAddress = server.ToString();
                    Logs.LogInfo($"测试Teredo服务器: {serverAddress}");

                    var testResult = TestServerWithPingAndTcp(serverAddress, 3544, "Teredo服务器");
                    result.TeredoServerResults.Add(testResult);

                    if (testResult.IsReachable)
                    {
                        successfulConnections++;
                    }
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

                    if (reachableServers.Count < teredoServers.Count)
                    {
                        combinedResult.AddSuggestion($"使用延迟最低的Teredo服务器: {result.FastestTeredoServer.Address}");
                    }
                }
                else
                {
                    Logs.LogInfo("没有可用的Teredo服务器");
                    combinedResult.AddIssue("所有Teredo服务器连接失败", "无法连接到任何Teredo服务器");
                }
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"测试Teredo服务器时出错: {ex.Message}");
                combinedResult.AddIssue("测试Teredo服务器失败", ex.Message);
            }
        }

        private void TestGameServers(NetworkDiagnosticResult result, JObject config, CombinedDiagnosticResult combinedResult)
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

                foreach (var server in serverInfoDict)
                {
                    string serverName = server.Key;
                    string serverAddress = server.Value?.ToString() ?? string.Empty;

                    Logs.LogInfo($"测试游戏服务器: {serverName} ({serverAddress})");

                    var testResult = TestServerWithPingAndTcp(serverAddress, 443, serverName);
                    result.GameServerResults.Add(testResult);
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
                combinedResult.AddIssue("测试游戏服务器失败", ex.Message);
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

        private void CheckXboxNetworkingServices(NetworkDiagnosticResult result, CombinedDiagnosticResult combinedResult)
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
                    combinedResult.AddIssue(errorMsg, "无法连接到Xbox服务");
                }
            }

            result.XboxNetworkingServiceReachable = successfulConnections >= 3; // 至少连接3个主要服务器
            Logs.LogInfo($"Xbox网络服务连接测试完成，成功{successfulConnections}个，要求至少3个");

            if (!result.XboxNetworkingServiceReachable)
            {
                Logs.LogInfo("Xbox网络服务连接不稳定");
                combinedResult.AddIssue("Xbox网络服务连接不稳定", "可能导致无法登录Xbox Live", async (p) => { await RepairXboxServices(p); await RepairNetworkConfig(p); });
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

        public class XboxServerInfo
        {
            public string ServerName { get; set; } = string.Empty;
            public string Host { get; set; } = string.Empty;
            public int Port { get; set; } = 443;
            public bool Required { get; set; } = true;
        }

        #endregion

        #endregion
    }
}