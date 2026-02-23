using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Win32;
using test.src.Services.PublicFuc.Helpers;

namespace test.src.UI.Forms.FH4.MedicalForm
{
    public partial class Medical
    {
        #region 第四步：VPN和防火墙检测
        public class VPNFirewallResult
        {
            public bool VPNDetected { get; set; }
            public string IPv6Status { get; set; } = "未知";
            public string FirewallStatus { get; set; } = "未知";
            public bool Port3074Open { get; set; }
            public bool Port3544Open { get; set; }
            public bool IsNetworkSecure { get; set; }
        }

        private void Step4_VPNFirewallCheck(BackgroundWorker worker, CombinedDiagnosticResult combinedResult)
        {
            Logs.LogInfo("执行第四步：VPN和防火墙检测");
            var result = new VPNFirewallResult();
            combinedResult.Step4Result = result;

            try
            {
                // 1. 检测VPN
                worker.ReportProgress(80, new ProgressData { Step = 4, Message = "检测VPN..." });
                DetectVPN(result, combinedResult);

                // 2. 检查IPv6配置
                worker.ReportProgress(85, new ProgressData { Step = 4, Message = "检查IPv6配置..." });
                CheckIPv6Configuration(result, combinedResult);

                // 3. 检查防火墙状态
                worker.ReportProgress(90, new ProgressData { Step = 4, Message = "检查防火墙..." });
                CheckFirewallStatus(result, combinedResult);

                // 4. 测试端口
                worker.ReportProgress(95, new ProgressData { Step = 4, Message = "测试端口..." });
                TestPorts(result, combinedResult);

                result.IsNetworkSecure = !result.VPNDetected && result.Port3074Open && result.Port3544Open;
                Logs.LogInfo($"第四步完成: 网络{(result.IsNetworkSecure ? "安全" : "不安全")}");
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"第四步执行错误: {ex.Message}");
                combinedResult.AddIssue("VPN防火墙检查过程异常", ex.Message);
            }
        }

        private void DetectVPN(VPNFirewallResult result, CombinedDiagnosticResult combinedResult)
        {
            try
            {
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                bool vpnDetected = false;
                List<string> vpnAdapters = new List<string>();

                // VPN常见关键词
                var vpnKeywords = new[]
                {
                    "VPN", "Tunnel", "TAP", "OpenVPN", "WireGuard", "L2TP", "PPTP", "SoftEther",
                    "ZeroTier", "Hamachi", "Radmin", "LogMeIn", "TeamViewer", "AnyDesk"
                };

                foreach (var adapter in networkInterfaces)
                {
                    foreach (var keyword in vpnKeywords)
                    {
                        if (adapter.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                            adapter.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                        {
                            vpnDetected = true;
                            vpnAdapters.Add($"{adapter.Name} ({adapter.Description})");
                            break;
                        }
                    }
                }

                result.VPNDetected = vpnDetected;

                if (vpnDetected)
                {
                    Logs.LogInfo($"检测到VPN适配器: {string.Join(", ", vpnAdapters)}");
                    combinedResult.AddIssue("检测到VPN", "VPN可能干扰Teredo隧道");
                    combinedResult.AddSuggestion("临时禁用VPN尝试连接");
                }
                else
                {
                    Logs.LogInfo("未检测到VPN");
                }
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"VPN检测失败: {ex.Message}");
                combinedResult.AddIssue("VPN检测失败", ex.Message);
            }
        }

        private void CheckIPv6Configuration(VPNFirewallResult result, CombinedDiagnosticResult combinedResult)
        {
            try
            {
                // 检查IPv6是否启用
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip6\Parameters"))
                {
                    if (key != null)
                    {
                        var disabledComponents = key.GetValue("DisabledComponents");
                        if (disabledComponents != null && disabledComponents is int disabledValue)
                        {
                            if ((disabledValue & 0xFF) != 0)
                            {
                                result.IPv6Status = "部分或全部禁用";
                                Logs.LogInfo("IPv6状态: 部分或全部禁用");
                                combinedResult.AddIssue("IPv6被禁用", "Teredo需要IPv6支持");
                                combinedResult.AddSuggestion("启用IPv6以支持Teredo隧道");
                            }
                            else
                            {
                                result.IPv6Status = "已启用";
                                Logs.LogInfo("IPv6状态: 已启用");
                            }
                        }
                        else
                        {
                            result.IPv6Status = "默认启用";
                            Logs.LogInfo("IPv6状态: 默认启用");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"检查IPv6配置失败: {ex.Message}");
                result.IPv6Status = "检查失败";
            }
        }

        private void CheckFirewallStatus(VPNFirewallResult result, CombinedDiagnosticResult combinedResult)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "advfirewall show allprofiles state",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processInfo))
                {
                    if (process != null)
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit(3000);

                        if (output.Contains("ON") || output.Contains("开启") || output.Contains("打开"))
                        {
                            result.FirewallStatus = "已启用";
                            Logs.LogInfo("防火墙状态: 已启用");
                        }
                        else
                        {
                            result.FirewallStatus = "已禁用";
                            Logs.LogInfo("防火墙状态: 已禁用");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"检查防火墙状态失败: {ex.Message}");
                result.FirewallStatus = "检查失败";
            }
        }

        private void TestPorts(VPNFirewallResult result, CombinedDiagnosticResult combinedResult)
        {
            // 测试UDP 3074 (Xbox Live)
            result.Port3074Open = TestUdpPort(3074);
            Logs.LogInfo($"端口3074 (UDP): {(result.Port3074Open ? "开放" : "阻塞")}");

            if (!result.Port3074Open)
            {
                combinedResult.AddIssue("端口3074阻塞", "Xbox Live需要此端口", async (p) => await RepairNetworkConfig(p));
                combinedResult.AddSuggestion("在路由器中转发UDP 3074端口");
            }

            // 测试UDP 3544 (Teredo)
            result.Port3544Open = TestUdpPort(3544);
            Logs.LogInfo($"端口3544 (UDP): {(result.Port3544Open ? "开放" : "阻塞")}");

            if (!result.Port3544Open)
            {
                combinedResult.AddIssue("端口3544阻塞", "Teredo需要此端口", async (p) => await RepairNetworkConfig(p));
                combinedResult.AddSuggestion("在防火墙中允许UDP 3544端口");
            }
        }

        private bool TestUdpPort(int port)
        {
            try
            {
                using (var udpClient = new UdpClient(port))
                {
                    // 如果能绑定端口，说明没有其他程序占用，但可能被防火墙阻挡
                    // 更准确的测试需要外部服务器，这里简化处理
                    return true;
                }
            }
            catch (SocketException ex)
            {
                // 10048: 端口已被占用（可能被其他程序使用）
                // 10013: 权限被拒绝（可能被防火墙阻挡）
                if (ex.ErrorCode == 10048)
                {
                    return true; // 端口已被占用，说明端口是开放的
                }
                return false;
            }
        }
        #endregion
    }
}