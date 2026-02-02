using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using test.src.Services.PublicFuc.Helpers;

namespace test.src.UI.Forms.FH4.MedicalForm
{
    public partial class Medical
    {
        #region 第二步：DNS和IP配置诊断
        public class DNSDiagnosticResult
        {
            public string DNSServers { get; set; } = "未检测";
            public string IPAddress { get; set; } = "未检测";
            public bool XboxDNSResolved { get; set; }
            public string DNSCacheStatus { get; set; } = "未检测";
            public bool IsDNSHealthy { get; set; }
        }

        private void Step2_DNSCheck(BackgroundWorker worker, CombinedDiagnosticResult combinedResult)
        {
            Logs.LogInfo("执行第二步：DNS和IP配置诊断");
            var result = new DNSDiagnosticResult();
            combinedResult.Step2Result = result;

            try
            {
                // 1. 获取DNS服务器信息
                worker.ReportProgress(30, new ProgressData { Step = 2, Message = "查询DNS服务器..." });
                GetDNSServerInfo(result, combinedResult);

                // 2. 获取IP地址信息
                worker.ReportProgress(35, new ProgressData { Step = 2, Message = "查询IP地址..." });
                GetIPAddressInfo(result, combinedResult);

                // 3. 测试DNS解析
                worker.ReportProgress(40, new ProgressData { Step = 2, Message = "测试DNS解析..." });
                TestDNSResolution(result, combinedResult);

                // 4. 检查DNS缓存
                worker.ReportProgress(45, new ProgressData { Step = 2, Message = "检查DNS缓存..." });
                CheckDNSCache(result, combinedResult);

                result.IsDNSHealthy = result.XboxDNSResolved && result.DNSCacheStatus != "异常";
                Logs.LogInfo($"第二步完成: DNS{(result.IsDNSHealthy ? "正常" : "异常")}");
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"第二步执行错误: {ex.Message}");
                combinedResult.AddIssue("DNS检查过程异常", ex.Message);
            }
        }

        private void GetDNSServerInfo(DNSDiagnosticResult result, CombinedDiagnosticResult combinedResult)
        {
            try
            {
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
                    .ToList();

                var dnsServers = new List<string>();

                foreach (var ni in networkInterfaces)
                {
                    var properties = ni.GetIPProperties();
                    foreach (var dns in properties.DnsAddresses)
                    {
                        dnsServers.Add(dns.ToString());
                    }
                }

                result.DNSServers = dnsServers.Any() ? string.Join(", ", dnsServers.Distinct()) : "未找到";
                Logs.LogInfo($"检测到DNS服务器: {result.DNSServers}");
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"获取DNS服务器信息失败: {ex.Message}");
                combinedResult.AddIssue("获取DNS服务器信息失败", ex.Message);
            }
        }

        private void GetIPAddressInfo(DNSDiagnosticResult result, CombinedDiagnosticResult combinedResult)
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                var ipAddresses = host.AddressList
                    .Where(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .Select(ip => ip.ToString())
                    .ToList();

                result.IPAddress = ipAddresses.Any() ? string.Join(", ", ipAddresses) : "未找到";
                Logs.LogInfo($"检测到IP地址: {result.IPAddress}");
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"获取IP地址信息失败: {ex.Message}");
                combinedResult.AddIssue("获取IP地址信息失败", ex.Message);
            }
        }

        private void TestDNSResolution(DNSDiagnosticResult result, CombinedDiagnosticResult combinedResult)
        {
            var xboxDomains = new[]
            {
                "xboxlive.com",
                "login.live.com",
                "profile.xboxlive.com",
                "xbox.ipv6.microsoft.com"
            };

            int successCount = 0;

            foreach (var domain in xboxDomains)
            {
                try
                {
                    Logs.LogInfo($"解析域名: {domain}");
                    var addresses = Dns.GetHostAddresses(domain);

                    if (addresses.Length > 0)
                    {
                        successCount++;
                        Logs.LogInfo($"域名 {domain} 解析成功: {string.Join(", ", addresses.Select(a => a.ToString()))}");
                    }
                    else
                    {
                        Logs.LogInfo($"域名 {domain} 解析失败: 无地址返回");
                        combinedResult.AddIssue($"DNS解析失败: {domain}", "无法解析域名");
                    }
                }
                catch (Exception ex)
                {
                    Logs.LogInfo($"域名 {domain} 解析异常: {ex.Message}");
                    combinedResult.AddIssue($"DNS解析异常: {domain}", ex.Message);
                }
            }

            result.XboxDNSResolved = successCount >= 3; // 至少3个域名解析成功
            Logs.LogInfo($"DNS解析测试: {successCount}/{xboxDomains.Length} 成功");
        }

        private void CheckDNSCache(DNSDiagnosticResult result, CombinedDiagnosticResult combinedResult)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "ipconfig",
                    Arguments = "/displaydns",
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

                        if (output.Contains("Record Name"))
                        {
                            result.DNSCacheStatus = "正常";
                            Logs.LogInfo("DNS缓存状态: 正常");
                        }
                        else
                        {
                            result.DNSCacheStatus = "无缓存或异常";
                            Logs.LogInfo("DNS缓存状态: 无缓存或异常");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"检查DNS缓存失败: {ex.Message}");
                result.DNSCacheStatus = "检查失败";
                combinedResult.AddIssue("检查DNS缓存失败", ex.Message);
            }
        }
        #endregion
    }
}