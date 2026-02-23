using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using test.src.Services.PublicFuc.Helpers;
using test.src.Services.PublicFuc.Managers;

namespace test.src.UI.Forms.FH5.MedicalForm
{
    public partial class Medical
    {
        #region 第三步：Xbox服务状态查询
        public class XboxServiceResult
        {
            public string XboxLiveCoreStatus { get; set; } = "未知";
            public string XboxSocialStatus { get; set; } = "未知";
            public string XboxStoreStatus { get; set; } = "未知";
            public string LocalCredentialsStatus { get; set; } = "未知";
            public bool IsXboxServiceHealthy { get; set; }
        }

        private void Step3_XboxServiceCheck(BackgroundWorker worker, CombinedDiagnosticResult combinedResult)
        {
            Logs.LogInfo("执行第三步：Xbox服务状态查询");
            var result = new XboxServiceResult();
            combinedResult.Step3Result = result;

            try
            {
                // 1. 检查Xbox Live核心服务
                worker.ReportProgress(55, new ProgressData { Step = 3, Message = "检查Xbox Live服务..." });
                CheckXboxLiveCoreService(result, combinedResult);

                // 2. 检查Xbox社交服务
                worker.ReportProgress(60, new ProgressData { Step = 3, Message = "检查Xbox社交服务..." });
                CheckXboxSocialService(result, combinedResult);

                // 3. 检查Xbox商店服务
                worker.ReportProgress(65, new ProgressData { Step = 3, Message = "检查Xbox商店服务..." });
                CheckXboxStoreService(result, combinedResult);

                // 4. 检查本地凭据
                worker.ReportProgress(70, new ProgressData { Step = 3, Message = "检查本地凭据..." });
                CheckLocalCredentials(result, combinedResult);

                result.IsXboxServiceHealthy = result.XboxLiveCoreStatus == "在线" &&
                                             result.LocalCredentialsStatus == "存在凭据文件";
                Logs.LogInfo($"第三步完成: Xbox服务{(result.IsXboxServiceHealthy ? "正常" : "异常")}");
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"第三步执行错误: {ex.Message}");
                combinedResult.AddIssue("Xbox服务检查过程异常", ex.Message);
            }
        }

        private void CheckXboxLiveCoreService(XboxServiceResult result, CombinedDiagnosticResult combinedResult)
        {
            try
            {
                // 使用HttpClient替代WebClient
                using (var httpClient = new HttpClient())
                {
                    // 设置User-Agent
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
                    // 设置超时时间
                    httpClient.Timeout = TimeSpan.FromSeconds(10);

                    // 尝试访问Xbox状态页面
                    string apiUrl = "https://www.xbox.com";

                    var response = httpClient.GetAsync(apiUrl).GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        if (responseContent.Contains("Xbox", StringComparison.OrdinalIgnoreCase) ||
                            responseContent.Contains("xboxlive", StringComparison.OrdinalIgnoreCase))
                        {
                            result.XboxLiveCoreStatus = "在线";
                            Logs.LogInfo("Xbox Live核心服务: 在线");
                        }
                        else
                        {
                            result.XboxLiveCoreStatus = "响应异常";
                            Logs.LogInfo("Xbox Live核心服务: 响应异常");
                        }
                    }
                    else
                    {
                        result.XboxLiveCoreStatus = $"HTTP错误: {(int)response.StatusCode}";
                        Logs.LogInfo($"Xbox Live核心服务: HTTP错误 {(int)response.StatusCode}");
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                Logs.LogInfo($"检查Xbox Live服务失败 (HTTP错误): {httpEx.Message}");
                result.XboxLiveCoreStatus = "网络错误";
                combinedResult.AddIssue("Xbox Live核心服务网络错误", httpEx.Message, async (p) => await RepairXboxServices(p));
            }
            catch (TaskCanceledException)
            {
                Logs.LogInfo("检查Xbox Live服务失败: 请求超时");
                result.XboxLiveCoreStatus = "请求超时";
                combinedResult.AddIssue("Xbox Live核心服务请求超时", "网络连接慢或服务器无响应", async (p) => await RepairXboxServices(p));
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"检查Xbox Live服务失败: {ex.Message}");
                result.XboxLiveCoreStatus = "无法访问";
                combinedResult.AddIssue("Xbox Live核心服务无法访问", ex.Message, async (p) => await RepairXboxServices(p));
            }
        }

        private void CheckXboxSocialService(XboxServiceResult result, CombinedDiagnosticResult combinedResult)
        {
            try
            {
                // 测试社交服务连接
                if (TestServerConnectivityXbox("profile.xboxlive.com", 443))
                {
                    result.XboxSocialStatus = "在线";
                    Logs.LogInfo("Xbox社交服务: 在线");
                }
                else
                {
                    result.XboxSocialStatus = "离线";
                    Logs.LogInfo("Xbox社交服务: 离线");
                    combinedResult.AddIssue("Xbox社交服务离线", "无法连接到profile.xboxlive.com", async (p) => { await RepairXboxServices(p); await RepairNetworkConfig(p); });
                }
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"检查Xbox社交服务失败: {ex.Message}");
                result.XboxSocialStatus = "检查失败";
                combinedResult.AddIssue("Xbox社交服务检查失败", ex.Message, async (p) => await RepairXboxServices(p));
            }
        }

        private void CheckXboxStoreService(XboxServiceResult result, CombinedDiagnosticResult combinedResult)
        {
            try
            {
                // 测试商店服务连接
                if (TestServerConnectivityXbox("www.microsoft.com", 443))
                {
                    result.XboxStoreStatus = "在线";
                    Logs.LogInfo("Xbox商店服务: 在线");
                }
                else
                {
                    result.XboxStoreStatus = "离线";
                    Logs.LogInfo("Xbox商店服务: 离线");
                }
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"检查Xbox商店服务失败: {ex.Message}");
                result.XboxStoreStatus = "检查失败";
            }
        }

        private void CheckLocalCredentials(XboxServiceResult result, CombinedDiagnosticResult combinedResult)
        {
            try
            {
                // 检查本地凭据文件是否存在
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string xboxFolder = Path.Combine(localAppData, "Packages", "Microsoft.XboxApp_8wekyb3d8bbwe");

                if (Directory.Exists(xboxFolder))
                {
                    result.LocalCredentialsStatus = "存在凭据文件";
                    Logs.LogInfo("本地Xbox凭据: 存在");

                    // FH5 特有提示
                    combinedResult.AddSuggestion("请确保在地平线5中已将当前房屋设置为【家】(Home)，否则可能无法连接Horizon Life");
                }
                else
                {
                    result.LocalCredentialsStatus = "无凭据文件";
                    Logs.LogInfo("本地Xbox凭据: 不存在");
                }
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"检查本地凭据失败: {ex.Message}");
                result.LocalCredentialsStatus = "检查失败";
            }
        }

        private bool TestServerConnectivityXbox(string host, int port, int timeout = 3000)
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
            catch
            {
                return false;
            }
        }
        #endregion
    }
}