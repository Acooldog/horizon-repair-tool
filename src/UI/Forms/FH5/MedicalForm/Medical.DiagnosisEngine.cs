using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using test.src.Services.PublicFuc.Helpers;
using test.src.Services.PublicFuc.Managers;

using System.Threading.Tasks;

namespace test.src.UI.Forms.FH5.MedicalForm
{
    public partial class Medical
    {
        #region 通用数据类定义

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
            public bool canReDiagnose { get; set; } = false;
        }

        // 其他数据类保持原样...

        #endregion
        #region 诊断引擎核心
        private void DiagnosisWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            combinedResult = new CombinedDiagnosticResult();
            diagnosisStartTime = DateTime.Now; // 记录开始时间

            try
            {
                Logs.LogInfo("开始执行完整网络诊断流程");

                // 使用 Invoke 确保线程安全
                this.Invoke((MethodInvoker)delegate
                {
                    this.TISHI.Visible = true;
                });

                // 步骤1: 网络连接检查
                if (!worker!.CancellationPending)
                {
                    worker.ReportProgress(0, new ProgressData { Step = 1, Message = "开始网络连接检查" });
                    Step1_NetworkCheck(worker, combinedResult);
                }

                // 步骤2: DNS和IP配置诊断
                if (!worker!.CancellationPending)
                {
                    worker.ReportProgress(16, new ProgressData { Step = 2, Message = "开始DNS配置诊断" });
                    Step2_DNSCheck(worker, combinedResult);
                }

                // 步骤3: Xbox服务状态查询
                if (!worker!.CancellationPending)
                {
                    worker.ReportProgress(32, new ProgressData { Step = 3, Message = "开始Xbox服务检查" });
                    Step3_XboxServiceCheck(worker, combinedResult);
                }

                // 步骤4: VPN和防火墙检测
                if (!worker!.CancellationPending)
                {
                    worker.ReportProgress(48, new ProgressData { Step = 4, Message = "开始VPN防火墙检测" });
                    Step4_VPNFirewallCheck(worker, combinedResult);
                }

                // 步骤5: 冲突服务检查
                if (!worker!.CancellationPending)
                {
                    worker.ReportProgress(64, new ProgressData { Step = 5, Message = "检查冲突服务" });
                    Step5_ServiceConflictCheck(worker, combinedResult);
                }

                // 步骤6: 必要服务状态检查
                if (!worker!.CancellationPending)
                {
                    worker.ReportProgress(80, new ProgressData { Step = 6, Message = "检查必要服务" });
                    Step6_ServiceStatusCheck(worker, combinedResult);
                }

                // 生成完整报告
                if (!worker!.CancellationPending)
                {
                    worker.ReportProgress(0, new ProgressData { Step = 0, Message = "生成诊断报告" });
                    string reportPath = GenerateCompleteDiagnosisReport(combinedResult);

                    // 使用 Invoke 确保线程安全
                    this.Invoke((MethodInvoker)delegate
                    {
                        this.CancelBtn.Text = "重新检测";
                        canReDiagnose = true;
                    });

                    combinedResult.EndTime = DateTime.Now;

                    e.Result = new DiagnosisCompleteResult
                    {
                        ReportPath = reportPath,
                        Result = combinedResult
                    };
                }

                // 使用 Invoke 确保线程安全
                this.Invoke((MethodInvoker)delegate
                {
                    this.TISHI.Visible = false;
                });
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"诊断过程中发生错误: {ex.Message}");
                combinedResult.AddIssue("诊断过程异常", ex.Message);
                combinedResult.EndTime = DateTime.Now;

                string reportPath = GenerateCompleteDiagnosisReport(combinedResult);
                e.Result = new DiagnosisCompleteResult
                {
                    ReportPath = reportPath,
                    Result = combinedResult
                };

                // 使用 Invoke 确保线程安全
                this.Invoke((MethodInvoker)delegate
                {
                    this.TISHI.Visible = true;
                });
            }
        }

        private DateTime diagnosisStartTime;

        private void DiagnosisWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is ProgressData progressData)
            {
                // 更新进度条
                if (progressBar != null && !progressBar.IsDisposed)
                {
                    if (progressBar.InvokeRequired)
                    {
                        progressBar.Invoke(new Action(() => progressBar.Value = e.ProgressPercentage));
                    }
                    else
                    {
                        progressBar.Value = e.ProgressPercentage;
                    }
                }

                // 估算剩余时间
                string timeMsg = "";
                if (e.ProgressPercentage > 0 && e.ProgressPercentage < 100)
                {
                    var elapsed = DateTime.Now - diagnosisStartTime;
                    // 简单的线性估算：Remaining = Elapsed / P * (1-P)
                    double p = e.ProgressPercentage / 100.0;
                    var totalSeconds = elapsed.TotalSeconds / p;
                    var remainingSeconds = totalSeconds - elapsed.TotalSeconds;

                    if (remainingSeconds < 60)
                        timeMsg = $"预计还需 {remainingSeconds:F0} 秒";
                    else
                        timeMsg = $"预计还需 {(remainingSeconds / 60):F1} 分钟";
                }
                else if (e.ProgressPercentage == 100)
                {
                    timeMsg = "即将完成...";
                }

                // 更新UI上的提示文字
                if (TISHI.InvokeRequired)
                {
                    TISHI.Invoke(new Action(() => TISHI.Text = $"{progressData.Message} ({timeMsg})"));
                }
                else
                {
                    TISHI.Text = $"{progressData.Message} ({timeMsg})";
                }

                // 更新步骤标签颜色
                UpdateStepLabelColor(progressData.Step);

                // 记录进度
                Logs.LogInfo($"诊断进度: {e.ProgressPercentage}% - {progressData.Message}");
            }
        }

        private void DiagnosisWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Logs.LogInfo($"诊断工作器执行出错: {e.Error.Message}");

                // 显示错误消息
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show($"诊断过程中发生错误: {e.Error.Message}",
                            "诊断错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
                return;
            }

            if (e.Result is DiagnosisCompleteResult completeResult)
            {
                Logs.LogInfo("诊断流程完成");
                ProcessCompleteDiagnosisResult(completeResult.ReportPath, completeResult.Result);
            }
        }

        private void UpdateStepLabelColor(int currentStep)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateStepLabelColor(currentStep)));
                return;
            }

            // 重置所有标签为默认颜色
            ResetStepLabels();

            // 将当前步骤设为绿色
            switch (currentStep)
            {
                case 1:
                    step1Label.ForeColor = System.Drawing.Color.Green;
                    break;
                case 2:
                    step2Label.ForeColor = System.Drawing.Color.Green;
                    break;
                case 3:
                    step3Label.ForeColor = System.Drawing.Color.Green;
                    break;
                case 4:
                    step4Label.ForeColor = System.Drawing.Color.Green;
                    break;
                case 5:
                    step5Label.ForeColor = System.Drawing.Color.Green;
                    break;
                case 6:
                    step6Label.ForeColor = System.Drawing.Color.Green;
                    break;
            }
        }

        private void ResetStepLabels()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ResetStepLabels));
                return;
            }

            step1Label.ForeColor = System.Drawing.SystemColors.ControlText;
            step2Label.ForeColor = System.Drawing.SystemColors.ControlText;
            step3Label.ForeColor = System.Drawing.SystemColors.ControlText;
            step4Label.ForeColor = System.Drawing.SystemColors.ControlText;
            step5Label.ForeColor = System.Drawing.SystemColors.ControlText;
            step6Label.ForeColor = System.Drawing.SystemColors.ControlText;
        }

        private void ResetUI()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ResetUI));
                return;
            }

            ResetStepLabels();
            progressBar.Value = 0;
        }

        private void ProcessCompleteDiagnosisResult(string reportPath, CombinedDiagnosticResult result)
        {
            // 自动打开报告文件 - 已禁用，改为UI显示
            /*
            if (!string.IsNullOrEmpty(reportPath) && File.Exists(reportPath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(reportPath)
                    {
                        UseShellExecute = true
                    });
                    Logs.LogInfo($"已打开诊断报告: {reportPath}");
                }
                catch (Exception ex)
                {
                    Logs.LogInfo($"打开诊断报告失败: {ex.Message}");
                }
            }
            */

            // 触发事件
            OnDiagnosisReportGenerated?.Invoke(reportPath, result);
        }
        #endregion

        #region 结果整合
        private string GenerateCompleteDiagnosisReport(CombinedDiagnosticResult result)
        {
            try
            {
                Logs.LogInfo("生成完整诊断报告...");
                var report = new StringBuilder();

                report.AppendLine($"地平线5网络诊断报告 - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                report.AppendLine("=".PadRight(60, '=') + "\n");

                // 基本信息
                report.AppendLine("📊 诊断概要");
                report.AppendLine("-".PadRight(40, '-'));
                report.AppendLine($"诊断时间: {result.StartTime:yyyy-MM-dd HH:mm:ss}");
                report.AppendLine($"诊断结果: {(result.AllStepsSuccessful ? "✅ 所有检查通过" : "⚠️ 发现问题")}");
                report.AppendLine($"发现问题数: {result.TotalIssues}");

                if (result.TotalDuration.TotalSeconds > 0)
                {
                    report.AppendLine($"总耗时: {result.TotalDuration.TotalSeconds:F1}秒");
                }
                report.AppendLine();

                // 步骤1结果
                report.AppendLine("🔧 第一步：网络连接检查");
                report.AppendLine("-".PadRight(40, '-'));
                if (result.Step1Result != null)
                {
                    report.AppendLine($"状态: {(result.Step1Result.IsHealthy ? "✅ 通过" : "❌ 失败")}");
                    report.AppendLine($"互联网连接: {(result.Step1Result.HasInternetConnection ? "正常" : "异常")}");
                    report.AppendLine($"Teredo适配器: {result.Step1Result.TeredoAdapterStatus}");
                    report.AppendLine($"Teredo启用: {(result.Step1Result.TeredoAdapterEnabled ? "是" : "否")}");
                    report.AppendLine($"NAT类型: {result.Step1Result.NATType}");
                    report.AppendLine($"Xbox服务可达: {(result.Step1Result.XboxNetworkingServiceReachable ? "是" : "否")}");

                    if (!result.Step1Result.IsHealthy)
                    {
                        report.AppendLine($"失败原因: {GetStep1FailureReason(result.Step1Result)}");
                    }
                }
                else
                {
                    report.AppendLine("未完成或结果为空");
                }

                // 步骤2结果
                report.AppendLine("\n🔍 第二步：DNS和IP配置诊断");
                report.AppendLine("-".PadRight(40, '-'));
                if (result.Step2Result != null)
                {
                    report.AppendLine($"状态: {(result.Step2Result.IsDNSHealthy ? "✅ 通过" : "❌ 失败")}");
                    report.AppendLine($"DNS服务器: {result.Step2Result.DNSServers}");
                    report.AppendLine($"IP地址: {result.Step2Result.IPAddress}");
                    report.AppendLine($"DNS解析Xbox: {(result.Step2Result.XboxDNSResolved ? "成功" : "失败")}");
                    report.AppendLine($"DNS缓存状态: {result.Step2Result.DNSCacheStatus}");

                    if (!result.Step2Result.IsDNSHealthy)
                    {
                        report.AppendLine($"失败原因: DNS配置异常");
                    }
                }
                else
                {
                    report.AppendLine("未完成或结果为空");
                }

                // 步骤3结果
                report.AppendLine("\n🎮 第三步：Xbox服务状态查询");
                report.AppendLine("-".PadRight(40, '-'));
                if (result.Step3Result != null)
                {
                    report.AppendLine($"状态: {(result.Step3Result.IsXboxServiceHealthy ? "✅ 通过" : "❌ 失败")}");
                    report.AppendLine($"Xbox Live核心服务: {result.Step3Result.XboxLiveCoreStatus}");
                    report.AppendLine($"Xbox 社交服务: {result.Step3Result.XboxSocialStatus}");
                    report.AppendLine($"Xbox 商店服务: {result.Step3Result.XboxStoreStatus}");
                    report.AppendLine($"本地凭据状态: {result.Step3Result.LocalCredentialsStatus}");
                    report.AppendLine("地平线4注意事项: 地平线4代token有时效限制，意思就是，你登录完账号之后，过一段时间Xbox就会不认你这个账号凭据，你需要重新登录才能连接到线上");
                    report.AppendLine("地平线5注意事项: 在线模式需绑定家园作为spawn点，未设导致连接循环，设置后autosave生效，解锁多人。意思是，你需要开车到你的房子中把这个设置为家，随后重启游戏");

                    if (!result.Step3Result.IsXboxServiceHealthy)
                    {
                        report.AppendLine($"失败原因: {GetStep3FailureReason(result.Step3Result)}");
                    }
                }
                else
                {
                    report.AppendLine("未完成或结果为空");
                }

                // 步骤4结果
                report.AppendLine("\n🛡️ 第四步：VPN和防火墙检测");
                report.AppendLine("-".PadRight(40, '-'));
                if (result.Step4Result != null)
                {
                    report.AppendLine($"状态: {(result.Step4Result.IsNetworkSecure ? "✅ 通过" : "❌ 失败")}");
                    report.AppendLine($"VPN检测: {(result.Step4Result.VPNDetected ? "检测到VPN" : "未检测到VPN")}");
                    report.AppendLine($"IPv6配置: {result.Step4Result.IPv6Status}");
                    report.AppendLine($"防火墙状态: {result.Step4Result.FirewallStatus}");
                    report.AppendLine($"端口3074: {(result.Step4Result.Port3074Open ? "开放" : "阻塞")}");
                    report.AppendLine($"端口3544: {(result.Step4Result.Port3544Open ? "开放" : "阻塞")}");

                    if (!result.Step4Result.IsNetworkSecure)
                    {
                        report.AppendLine($"失败原因: {GetStep4FailureReason(result.Step4Result)}");
                    }
                }
                else
                {
                    report.AppendLine("未完成或结果为空");
                }

                // 步骤5结果
                report.AppendLine("\n⚙️ 第五步：冲突服务检查");
                report.AppendLine("-".PadRight(40, '-'));
                if (result.Step5Result != null)
                {
                    report.AppendLine($"状态: {(result.Step5Result.IsHealthy ? "✅ 通过" : "❌ 失败")}");

                    if (result.Step5Result.ConflictingServices.Count > 0)
                    {
                        report.AppendLine($"发现 {result.Step5Result.ConflictingServices.Count} 个冲突服务:");
                        foreach (var service in result.Step5Result.ConflictingServices)
                        {
                            report.AppendLine($"  ❌ {service.DisplayName} ({service.ServiceName}) - 状态: {service.Status}, 启动类型: {service.StartType}");
                        }
                        report.AppendLine($"失败原因: 发现冲突服务正在运行");
                    }
                    else
                    {
                        report.AppendLine("未发现冲突服务");
                    }
                }
                else
                {
                    report.AppendLine("未完成或结果为空");
                }

                // 步骤6结果
                report.AppendLine("\n🔧 第六步：必要服务状态检查");
                report.AppendLine("-".PadRight(40, '-'));
                if (result.Step6Result != null)
                {
                    report.AppendLine($"状态: {(result.Step6Result.IsHealthy ? "✅ 通过" : "❌ 失败")}");

                    // 手动启动服务检查
                    if (result.Step6Result.ManualServices.Count > 0)
                    {
                        report.AppendLine("手动启动服务检查:");
                        foreach (var service in result.Step6Result.ManualServices)
                        {
                            string statusIcon = service.IsCorrect ? "✅" : "❌";
                            report.AppendLine($"  {statusIcon} {service.DisplayName}: 预期(手动), 实际({service.ActualStartType}), 状态({service.Status})");
                        }
                    }

                    // 自动启动服务检查
                    if (result.Step6Result.AutoServices.Count > 0)
                    {
                        report.AppendLine("自动启动服务检查:");
                        foreach (var service in result.Step6Result.AutoServices)
                        {
                            string statusIcon = service.IsCorrect ? "✅" : "❌";
                            report.AppendLine($"  {statusIcon} {service.DisplayName}: 预期(自动), 实际({service.ActualStartType}), 状态({service.Status})");
                        }
                    }

                    if (!result.Step6Result.IsHealthy)
                    {
                        report.AppendLine($"失败原因: 必要服务配置不正确");
                    }
                }
                else
                {
                    report.AppendLine("未完成或结果为空");
                }

                // 服务器延迟测试结果
                if (result.Step1Result?.TeredoServerResults != null && result.Step1Result.TeredoServerResults.Any())
                {
                    report.AppendLine("\n🌐 Teredo服务器延迟测试");
                    report.AppendLine("-".PadRight(40, '-'));
                    int shownCount = 0;
                    foreach (var server in result.Step1Result.TeredoServerResults)
                    {
                        if (shownCount >= 5) break;
                        string statusIcon = server.IsReachable ? "✅" : "❌";
                        report.AppendLine($"{statusIcon} {server.Address}: {server.Status}");
                        shownCount++;
                    }

                    if (result.Step1Result.FastestTeredoServer is not null)
                    {
                        report.AppendLine($"\n🚀 推荐Teredo服务器: {result.Step1Result.FastestTeredoServer.Address} (延迟: {result.Step1Result.FastestTeredoServer.PingLatency}ms)");
                    }
                }

                if (result.Step1Result?.GameServerResults != null && result.Step1Result.GameServerResults.Any())
                {
                    report.AppendLine("\n🎮 游戏服务器延迟测试");
                    report.AppendLine("-".PadRight(40, '-'));
                    int shownCount = 0;
                    foreach (var server in result.Step1Result.GameServerResults)
                    {
                        if (shownCount >= 5) break;
                        string statusIcon = server.IsReachable ? "✅" : "❌";
                        report.AppendLine($"{statusIcon} {server.ServerName}: {server.Status}");
                        shownCount++;
                    }

                    if (result.Step1Result.FastestGameServer is not null)
                    {
                        report.AppendLine($"\n🚀 推荐游戏服务器: {result.Step1Result.FastestGameServer.ServerName} (延迟: {result.Step1Result.FastestGameServer.PingLatency}ms)");
                    }
                }

                // 发现的问题
                if (result.AllIssues.Any())
                {
                    report.AppendLine("\n⚠️ 发现的问题");
                    report.AppendLine("-".PadRight(40, '-'));
                    int issueCount = 1;
                    foreach (var issue in result.AllIssues)
                    {
                        report.AppendLine($"{issueCount}. {issue.Description}");
                        if (!string.IsNullOrEmpty(issue.Details))
                            report.AppendLine($"   详细信息: {issue.Details}");
                        issueCount++;
                    }
                }

                // 修复建议
                if (result.RepairSuggestions.Any())
                {
                    report.AppendLine("\n💡 修复建议");
                    report.AppendLine("-".PadRight(40, '-'));
                    int suggestionCount = 1;
                    foreach (var suggestion in result.RepairSuggestions)
                    {
                        report.AppendLine($"{suggestionCount}. {suggestion}");
                        suggestionCount++;
                    }
                }
                else
                {
                    // 如果没有生成建议，添加默认建议
                    report.AppendLine("\n💡 修复建议");
                    report.AppendLine("-".PadRight(40, '-'));
                    if (result.AllStepsSuccessful)
                    {
                        report.AppendLine("1. 网络状态良好，可以正常进行游戏");
                    }
                    else
                    {
                        report.AppendLine("1. 根据上述问题列表逐一解决");
                        report.AppendLine("2. 解决后重新运行诊断");
                    }
                }

                string TxtPath = pathEdit.GetApplicationRootDirectory();
                // 创建日志目录
                string TxtDir = Path.Combine(TxtPath, "MedicalReport");
                if (!Directory.Exists(TxtDir))
                {
                    Directory.CreateDirectory(TxtDir);
                    Logs.LogInfo($"目录不存在，创建目录: {TxtDir}");
                }

                // 诊断结束
                report.AppendLine("\n" + "=".PadRight(60, '='));
                report.AppendLine("地平线5网络诊断报告 - 生成完成");
                report.AppendLine($"报告生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                // 保存报告到文件
                string fileName = $"FH5_Diagnosis_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string filePath = Path.Combine(TxtDir, fileName);

                File.WriteAllText(filePath, report.ToString(), Encoding.UTF8);
                Logs.LogInfo($"诊断报告已保存到: {filePath}");

                return filePath;
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"生成诊断报告时出错: {ex.Message}");

                // 创建简单的错误报告
                try
                {
                    string TxtPath = pathEdit.GetApplicationRootDirectory();
                    // 创建日志目录
                    string TxtDir = Path.Combine(TxtPath, "MedicalReport");
                    if (!Directory.Exists(TxtDir))
                    {
                        Directory.CreateDirectory(TxtDir);
                        Logs.LogInfo($"目录不存在，创建目录: {TxtDir}");
                    }

                    string errorReport = $"诊断报告生成失败\n错误: {ex.Message}\n时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                    string fileName = $"FH5_Diagnosis_Error_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                    string filePath = Path.Combine(TxtDir, fileName);
                    File.WriteAllText(filePath, errorReport, Encoding.UTF8);
                    return filePath;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        private string GetStep1FailureReason(NetworkDiagnosticResult result)
        {
            var reasons = new List<string>();

            if (!result.HasInternetConnection)
                reasons.Add("互联网连接失败");

            if (!result.TeredoAdapterExists)
                reasons.Add("未找到Teredo适配器");

            if (!result.TeredoAdapterEnabled)
                reasons.Add("Teredo适配器未启用");

            if (result.NATType != "开放")
                reasons.Add($"NAT类型为{result.NATType}（需要开放）");

            if (!result.XboxNetworkingServiceReachable)
                reasons.Add("Xbox网络服务不可达");

            return reasons.Any() ? string.Join("; ", reasons) : "未知原因";
        }

        private string GetStep3FailureReason(XboxServiceResult result)
        {
            var reasons = new List<string>();

            if (result.XboxLiveCoreStatus != "在线")
                reasons.Add($"Xbox Live核心服务: {result.XboxLiveCoreStatus}");

            if (result.XboxSocialStatus != "在线")
                reasons.Add($"Xbox社交服务: {result.XboxSocialStatus}");

            if (result.LocalCredentialsStatus != "有效" && result.LocalCredentialsStatus != "存在凭据文件")
                reasons.Add($"本地凭据: {result.LocalCredentialsStatus}");

            return reasons.Any() ? string.Join("; ", reasons) : "未知原因";
        }

        private string GetStep4FailureReason(VPNFirewallResult result)
        {
            var reasons = new List<string>();

            if (result.VPNDetected)
                reasons.Add("检测到VPN");

            if (!result.Port3074Open)
                reasons.Add("端口3074阻塞");

            if (!result.Port3544Open)
                reasons.Add("端口3544阻塞");

            return reasons.Any() ? string.Join("; ", reasons) : "未知原因";
        }
        #endregion

        #region 数据类定义
        public class ProgressData
        {
            public int Step { get; set; } // 0=结束, 1-6=步骤编号
            public string Message { get; set; } = string.Empty;
        }

        public class DiagnosisCompleteResult
        {
            public string ReportPath { get; set; } = string.Empty;
            public CombinedDiagnosticResult Result { get; set; } = new CombinedDiagnosticResult();
        }

        public class CombinedDiagnosticResult
        {
            public DateTime StartTime { get; } = DateTime.Now;
            public DateTime EndTime { get; set; }
            public TimeSpan TotalDuration => EndTime - StartTime;

            public NetworkDiagnosticResult? Step1Result { get; set; }
            public DNSDiagnosticResult? Step2Result { get; set; }
            public XboxServiceResult? Step3Result { get; set; }
            public VPNFirewallResult? Step4Result { get; set; }
            public ServiceConflictResult? Step5Result { get; set; }
            public ServiceStatusResult? Step6Result { get; set; }

            public List<DiagnosticIssue> AllIssues { get; } = new List<DiagnosticIssue>();
            public List<string> RepairSuggestions { get; } = new List<string>();

            public int TotalIssues => AllIssues.Count;

            public bool AllStepsSuccessful =>
                (Step1Result?.IsHealthy ?? false) &&
                (Step2Result?.IsDNSHealthy ?? false) &&
                (Step3Result?.IsXboxServiceHealthy ?? false) &&
                (Step4Result?.IsNetworkSecure ?? false) &&
                (Step5Result?.IsHealthy ?? false) &&
                (Step6Result?.IsHealthy ?? false);

            public void AddIssue(string description, string details = "", Func<IProgress<int>, Task>? repairAction = null)
            {
                AllIssues.Add(new DiagnosticIssue { Description = description, Details = details, RepairAction = repairAction });
            }

            public void AddSuggestion(string suggestion)
            {
                RepairSuggestions.Add(suggestion);
            }
        }

        public class DiagnosticIssue
        {
            public string Description { get; set; } = string.Empty;
            public string Details { get; set; } = string.Empty;
            public Func<IProgress<int>, Task>? RepairAction { get; set; }
        }
        #endregion

        // 事件定义
        public delegate void DiagnosisReportHandler(string reportPath, CombinedDiagnosticResult result);
        public event DiagnosisReportHandler? OnDiagnosisReportGenerated;
    }
}