using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using test.src.Services.PublicFuc.Helpers;

namespace test.src.UI.Forms.FH5.MedicalForm
{
    public partial class Medical
    {
        #region 修复逻辑实现

        private void StartNetworkRepair()
        {
            Logs.LogInfo("开始网络修复流程...");

            // 禁用界面交互
            this.Invoke((MethodInvoker)delegate
            {
                this.CancelBtn.Enabled = false;
                this.TISHI.Text = "正在修复网络问题，请稍候...";
                this.TISHI.Visible = true;
                this.progressBar.Value = 0;
            });

            // 启动后台修复任务
            Task.Run(async () =>
            {
                try
                {
                    // 1. 修复Teredo适配器
                    await RepairTeredoAdapter();
                    ReportRepairProgress(20, "Teredo适配器重置完成");

                    // 2. 刷新DNS和重置网络
                    await RepairNetworkConfig();
                    ReportRepairProgress(40, "DNS缓存已清除，网络重置完成");

                    // 3. 修复Xbox服务
                    await RepairXboxServices();
                    ReportRepairProgress(60, "Xbox服务已重启");

                    // 4. 清除凭据
                    await RepairCredentials();
                    ReportRepairProgress(80, "Xbox凭据已刷新");

                    // 5. 修复冲突服务
                    await RepairConflictingServices();
                    ReportRepairProgress(90, "冲突服务已处理");

                    // 6. 确保必要服务运行
                    await RepairRequiredServices();
                    ReportRepairProgress(100, "必要服务已启动");

                    // 修复完成，重新诊断
                    this.Invoke((MethodInvoker)delegate
                    {
                        MessageBox.Show("修复操作已完成！即将重新开始诊断以验证修复效果。", "修复完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.CancelBtn.Enabled = true;
                        this.TISHI.Visible = false;
                        this.canReDiagnose = false;

                        // 重新开始诊断
                        StartCompleteDiagnosis();
                    });
                }
                catch (Exception ex)
                {
                    Logs.LogInfo($"修复过程中出错: {ex.Message}");
                    this.Invoke((MethodInvoker)delegate
                    {
                        MessageBox.Show($"修复过程中发生错误: {ex.Message}", "修复失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.CancelBtn.Enabled = true;
                        this.TISHI.Visible = false;
                    });
                }
            });
        }

        private void ReportRepairProgress(int value, string message)
        {
            this.Invoke((MethodInvoker)delegate
            {
                this.progressBar.Value = value;
                this.TISHI.Text = message;
                Logs.LogInfo($"修复进度: {value}% - {message}");
            });
        }

        private async Task RepairTeredoAdapter(IProgress<int>? progress = null)
        {
            Logs.LogInfo("正在重置Teredo适配器...");
            progress?.Report(10);
            await RunCommandAsync("netsh", "interface teredo set state disable");
            await Task.Delay(1000);
            progress?.Report(30);
            await RunCommandAsync("netsh", "interface teredo set state type=default");
            await Task.Delay(1000);
            progress?.Report(50);
            await RunCommandAsync("netsh", "interface teredo set state enterpriseclient"); // 通常enterpriseclient更稳定
            await Task.Delay(1000);
            progress?.Report(80);
            await RunCommandAsync("netsh", "interface teredo set state servername=win1910.ipv6.microsoft.com");
            progress?.Report(100);
        }

        private async Task RepairNetworkConfig(IProgress<int>? progress = null)
        {
            Logs.LogInfo("正在刷新DNS和重置Winsock...");
            progress?.Report(10);
            await RunCommandAsync("ipconfig", "/flushdns");
            progress?.Report(30);
            await RunCommandAsync("ipconfig", "/release");
            progress?.Report(50);
            await RunCommandAsync("ipconfig", "/renew");
            progress?.Report(70);
            await RunCommandAsync("netsh", "winsock reset");
            progress?.Report(90);
            await RunCommandAsync("netsh", "int ip reset");
            progress?.Report(100);
        }

        private async Task RepairXboxServices(IProgress<int>? progress = null)
        {
            Logs.LogInfo("正在重启Xbox相关服务...");
            // 需要管理员权限，这里假设程序已提权
            string[] services = { "iphlpsvc", "XblAuthManager", "XblGameSave", "XboxNetApiSvc" };

            int count = 0;
            foreach (var service in services)
            {
                await RunCommandAsync("net", $"stop {service} /y");
                await Task.Delay(500);
                await RunCommandAsync("net", $"start {service}");

                count++;
                if (progress != null) progress.Report((int)((double)count / services.Length * 100));
            }
        }

        private async Task RepairCredentials(IProgress<int>? progress = null)
        {
            Logs.LogInfo("尝试清除Xbox凭据...");
            progress?.Report(10);
            // 使用cmdkey列出并删除
            // 注意：这可能需要用户交互或更复杂的API调用，这里使用简单的cmdkey尝试
            // 实际上清除凭据最好引导用户手动操作或使用CredentialManager API（比较复杂）
            // 这里我们尝试删除通用的Xbl凭据

            await Task.Run(() =>
            {
                try
                {
                    // 模拟清除凭据的逻辑，实际上可能需要调用VaultCli或其他库
                    // 这里我们引导用户注销，或者删除特定的文件缓存
                    string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string tokenBrokerPath = Path.Combine(localAppData, "Packages", "Microsoft.XboxIdentityProvider_8wekyb3d8bbwe", "AC", "TokenBroker");

                    progress?.Report(50);

                    if (Directory.Exists(tokenBrokerPath))
                    {
                        try
                        {
                            Directory.Delete(tokenBrokerPath, true);
                            Logs.LogInfo("已清除TokenBroker缓存");
                        }
                        catch (Exception ex)
                        {
                            Logs.LogInfo($"清除TokenBroker缓存失败: {ex.Message}");
                        }
                    }
                    progress?.Report(100);
                }
                catch (Exception ex)
                {
                    Logs.LogInfo($"清除凭据部分失败: {ex.Message}");
                    progress?.Report(100);
                }
            });
        }

        private async Task RepairConflictingServices(IProgress<int>? progress = null)
        {
            Logs.LogInfo("正在停止冲突服务...");
            if (combinedResult.Step5Result != null && combinedResult.Step5Result.HasConflicts)
            {
                int total = combinedResult.Step5Result.ConflictingServices.Count;
                int current = 0;
                foreach (var service in combinedResult.Step5Result.ConflictingServices)
                {
                    await RunCommandAsync("net", $"stop \"{service.ServiceName}\" /y");
                    await RunCommandAsync("sc", $"config \"{service.ServiceName}\" start= disabled");

                    current++;
                    if (progress != null) progress.Report((int)((double)current / total * 100));
                }
            }
            else
            {
                progress?.Report(100);
            }
        }

        private async Task RepairRequiredServices(IProgress<int>? progress = null)
        {
            Logs.LogInfo("正在启动必要服务...");
            if (combinedResult.Step6Result != null)
            {
                int total = combinedResult.Step6Result.AutoServices.Count + combinedResult.Step6Result.ManualServices.Count;
                int current = 0;

                foreach (var service in combinedResult.Step6Result.AutoServices)
                {
                    if (!service.IsCorrect)
                    {
                        await RunCommandAsync("sc", $"config \"{service.ServiceName}\" start= auto");
                        await RunCommandAsync("net", $"start \"{service.ServiceName}\"");
                    }
                    current++;
                    if (total > 0 && progress != null) progress.Report((int)((double)current / total * 100));
                }

                foreach (var service in combinedResult.Step6Result.ManualServices)
                {
                    if (!service.IsCorrect)
                    {
                        await RunCommandAsync("sc", $"config \"{service.ServiceName}\" start= demand");
                        // 手动服务不需要立即启动，只需配置正确
                    }
                    current++;
                    if (total > 0 && progress != null) progress.Report((int)((double)current / total * 100));
                }
            }
            progress?.Report(100);
        }

        private Task RunCommandAsync(string fileName, string arguments)
        {
            var tcs = new TaskCompletionSource<bool>();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Verb = "runas" // 请求管理员权限
                },
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(true);
                process.Dispose();
            };

            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                Logs.LogInfo($"无法执行命令 {fileName} {arguments}: {ex.Message}");
                tcs.SetResult(false);
            }

            return tcs.Task;
        }

        #endregion
    }
}
