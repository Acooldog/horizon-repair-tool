using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json.Linq;
using test.src.Services.PublicFuc.Helpers;
using test.src.Services.PublicFuc.Managers;

namespace test.src.UI.Forms.FH4.MedicalForm
{
    public partial class Medical
    {
        #region 第六步：必要服务状态检查

        public class ServiceStatusResult
        {
            public List<ServiceInfo> ManualServices { get; set; } = new List<ServiceInfo>();
            public List<ServiceInfo> AutoServices { get; set; } = new List<ServiceInfo>();
            public bool AllServicesCorrect { get; set; }
            public bool IsHealthy => AllServicesCorrect;
        }

        private void Step6_ServiceStatusCheck(BackgroundWorker worker, CombinedDiagnosticResult combinedResult)
        {
            Logs.LogInfo("执行第六步：必要服务状态检查");
            var result = new ServiceStatusResult();
            combinedResult.Step6Result = result;

            try
            {
                // 1. 读取配置文件
                worker.ReportProgress(88, new ProgressData { Step = 6, Message = "读取配置文件..." });
                var config = ReadServerConfig();
                if (config == null)
                {
                    Logs.LogInfo("无法读取配置文件，跳过必要服务检查");
                    combinedResult.AddIssue("配置文件读取失败", "无法检查必要服务");
                    return;
                }

                // 2. 检查手动启动服务
                var manualServices = config["fix"]?["EnableNotAuto"] as JArray;
                if (manualServices != null && manualServices.Count > 0)
                {
                    Logs.LogInfo($"开始检查 {manualServices.Count} 个手动启动服务...");
                    CheckServiceList(manualServices, "手动", result.ManualServices, worker, combinedResult, 88, 4);
                }

                // 3. 检查自动启动服务
                var autoServices = config["fix"]?["EnableAuto"] as JArray;
                if (autoServices != null && autoServices.Count > 0)
                {
                    Logs.LogInfo($"开始检查 {autoServices.Count} 个自动启动服务...");
                    CheckServiceList(autoServices, "自动", result.AutoServices, worker, combinedResult, 92, 4);
                }

                // 4. 计算总体结果
                var allServices = new List<ServiceInfo>();
                allServices.AddRange(result.ManualServices);
                allServices.AddRange(result.AutoServices);

                int correctCount = allServices.Count(s => s.IsCorrect);
                int totalCount = allServices.Count;

                result.AllServicesCorrect = correctCount == totalCount && totalCount > 0;

                Logs.LogInfo($"第六步完成: 检查了 {totalCount} 个服务，{correctCount} 个正确");

                if (!result.AllServicesCorrect)
                {
                    combinedResult.AddSuggestion("根据诊断报告调整服务的启动类型和状态");
                }
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"第六步执行错误: {ex.Message}");
                combinedResult.AddIssue("必要服务检查过程异常", ex.Message);
            }
        }

        private void CheckServiceList(JArray serviceArray, string expectedStartType,
            List<ServiceInfo> resultList, BackgroundWorker worker,
            CombinedDiagnosticResult combinedResult, int baseProgress, int progressRange)
        {
            int totalServices = serviceArray.Count;
            int checkedCount = 0;

            foreach (var serviceItem in serviceArray)
            {
                string displayName = serviceItem.ToString();
                Logs.LogInfo($"检查服务: {displayName}, 预期启动类型: {expectedStartType}");

                var serviceInfo = new ServiceInfo
                {
                    DisplayName = displayName,
                    ExpectedStartType = expectedStartType
                };

                try
                {
                    // 获取服务名称
                    string serviceName = ServiceHelper.GetServiceNameByDisplayName(displayName);

                    if (string.IsNullOrEmpty(serviceName))
                    {
                        serviceInfo.Exists = false;
                        serviceInfo.ErrorMessage = "服务未找到";
                        serviceInfo.IsCorrect = false;

                        string issueMsg = $"必要服务未找到: {displayName}";
                        string detailsMsg = $"该服务是地平线4运行所必需的";
                        combinedResult.AddIssue(issueMsg, detailsMsg);

                        Logs.LogInfo($"服务未找到: {displayName}");
                    }
                    else
                    {
                        serviceInfo.ServiceName = serviceName;
                        serviceInfo.Exists = true;

                        // 获取服务状态
                        serviceInfo.Status = ServiceHelper.GetServiceStatus(serviceName);
                        serviceInfo.IsRunning = serviceInfo.Status == "正在运行";

                        // 获取启动类型
                        serviceInfo.ActualStartType = ServiceHelper.GetServiceStartType(serviceName);

                        // 检查是否正确
                        serviceInfo.IsCorrect = CheckServiceCorrectness(serviceInfo, expectedStartType);

                        if (!serviceInfo.IsCorrect)
                        {   

                            string issueMsg = $"服务配置不正确: {displayName}";
                            string detailsMsg = string.Format(
                                "预期：{0}/{1}，实际：{2}/{3}",
                                expectedStartType,
                                expectedStartType == "自动" ? "正在运行" : "不要求运行",
                                serviceInfo.ActualStartType,
                                serviceInfo.Status
                            );
                            combinedResult.AddIssue(issueMsg, detailsMsg);
                        }

                        Logs.LogInfo($"服务: {displayName}, 状态: {serviceInfo.Status}, " +
                                   $"启动类型: {serviceInfo.ActualStartType}, 正确: {serviceInfo.IsCorrect}");
                    }
                }
                catch (Exception ex)
                {
                    serviceInfo.Exists = false;
                    serviceInfo.ErrorMessage = ex.Message;
                    serviceInfo.IsCorrect = false;
                    Logs.LogInfo($"检查服务失败: {displayName}, 错误: {ex.Message}");
                }

                resultList.Add(serviceInfo);
                checkedCount++;

                int progress = baseProgress + (int)((checkedCount / (double)totalServices) * progressRange);
                worker.ReportProgress(progress, new ProgressData
                {
                    Step = 6,
                    Message = $"检查{expectedStartType}服务 {checkedCount}/{totalServices}..."
                });
            }
        }

        private bool CheckServiceCorrectness(ServiceInfo serviceInfo, string expectedStartType)
        {
            if (!serviceInfo.Exists) return false;

            // 检查启动类型
            bool startTypeCorrect = serviceInfo.ActualStartType == expectedStartType;

            // 对于自动启动的服务，还需要检查是否正在运行
            if (expectedStartType == "自动")
            {
                return startTypeCorrect && serviceInfo.IsRunning;
            }

            // 对于手动启动的服务，不要求运行状态
            return startTypeCorrect;
        }

        #endregion
    }
}