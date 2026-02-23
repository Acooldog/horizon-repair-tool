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
        #region 第五步：冲突服务检查

        public class ServiceConflictResult
        {
            public List<ServiceConflictInfo> ConflictingServices { get; set; } = new List<ServiceConflictInfo>();
            public bool HasConflicts => ConflictingServices.Count > 0;
            public bool IsHealthy => !HasConflicts;
        }

        public class ServiceConflictInfo
        {
            public string DisplayName { get; set; } = string.Empty;
            public string ServiceName { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string StartType { get; set; } = string.Empty;
            public bool ShouldBeDisabled { get; set; } = true;
        }

        private void Step5_ServiceConflictCheck(BackgroundWorker worker, CombinedDiagnosticResult combinedResult)
        {
            Logs.LogInfo("执行第五步：冲突服务检查");
            var result = new ServiceConflictResult();
            combinedResult.Step5Result = result;

            try
            {
                // 1. 读取配置文件
                worker.ReportProgress(80, new ProgressData { Step = 5, Message = "读取配置文件..." });
                var config = ReadServerConfig();
                if (config == null)
                {
                    Logs.LogInfo("无法读取配置文件，跳过冲突服务检查");
                    combinedResult.AddIssue("配置文件读取失败", "无法检查冲突服务");
                    return;
                }

                // 2. 获取需要禁用的服务列表
                var disableServices = config["fix"]?["disableClashName"] as JArray;
                if (disableServices == null || disableServices.Count == 0)
                {
                    Logs.LogInfo("未找到冲突服务配置，跳过检查");
                    return;
                }

                Logs.LogInfo($"开始检查 {disableServices.Count} 个冲突服务...");

                // 3. 检查每个服务
                int totalServices = disableServices.Count;
                int checkedCount = 0;

                foreach (var serviceItem in disableServices)
                {
                    // 使用 Invoke 确保线程安全
                    this.Invoke((MethodInvoker)delegate
                    {
                        // 进度条值 = ( 当前数值 × 100 ) / 总数值 
                        this.step5Label.Text = $"5. 查看是否有冲突的服务正在运行 - {(checkedCount * 100) / totalServices}%";
                    });
                    worker.ReportProgress(64 + checkedCount, new ProgressData { Step = 5, Message = "检查冲突服务" });
                    string displayName = serviceItem.ToString();
                    Logs.LogInfo($"检查冲突服务: {displayName}");

                    // 获取服务名称
                    string serviceName = ServiceHelper.GetServiceNameByDisplayName(displayName);

                    if (string.IsNullOrEmpty(serviceName))
                    {
                        Logs.LogInfo($"未找到服务: {displayName}");
                        checkedCount++;
                        continue;
                    }

                    // 获取服务状态
                    string status = ServiceHelper.GetServiceStatus(serviceName);
                    string startType = ServiceHelper.GetServiceStartType(serviceName);

                    Logs.LogInfo($"服务: {displayName}, 状态: {status}, 启动类型: {startType}");

                    // 如果服务正在运行，则视为冲突
                    if (status == "正在运行")
                    {
                        var conflictInfo = new ServiceConflictInfo
                        {
                            DisplayName = displayName,
                            ServiceName = serviceName,
                            Status = status,
                            StartType = startType
                        };

                        result.ConflictingServices.Add(conflictInfo);

                        string issueMsg = $"冲突服务正在运行: {displayName} ({serviceName})";
                        string detailsMsg = $"该服务可能会干扰地平线4的网络连接，建议停止并禁用此服务";
                        combinedResult.AddIssue(issueMsg, detailsMsg, async (p) => await RepairConflictingServices(p));

                        Logs.LogInfo($"发现冲突服务: {displayName}");
                    }
                    else
                    {
                        Logs.LogInfo($"服务 {displayName} 未运行，正常");
                    }

                    checkedCount++;
                    int progress = 80 + (int)((checkedCount / (double)totalServices) * 8);
                    worker.ReportProgress(progress, new ProgressData
                    {
                        Step = 5,
                        Message = $"检查冲突服务 {checkedCount}/{totalServices}..."
                    });
                }

                Logs.LogInfo($"第五步完成: 发现 {result.ConflictingServices.Count} 个冲突服务");
                // 使用 Invoke 确保线程安全
                this.Invoke((MethodInvoker)delegate
                {
                    this.step5Label.Text = $"5. 查看是否有冲突的服务正在运行";
                });

                if (result.HasConflicts)
                {
                    combinedResult.AddSuggestion("停止并禁用发现的冲突服务");
                }
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"第五步执行错误: {ex.Message}");
                combinedResult.AddIssue("冲突服务检查过程异常", ex.Message);
            }
        }

        #endregion
    }
}