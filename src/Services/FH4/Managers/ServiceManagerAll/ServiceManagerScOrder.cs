using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using test.src.Services.PublicFuc.Helpers;

namespace test.src.Services.FH4.Managers.ServiceManagerAll
{
    // ServiceManager 的 SC 命令操作
    public partial class ServiceManager
    {
        #region SC 命令操作

        /// <summary>
        /// 使用SC命令设置服务启动类型（新增方法）
        /// </summary>
        private static async Task SetServiceStartTypeBySCAsync(string serviceName, string startType)
        {
            Logs.LogInfo($"使用SC命令设置服务 '{serviceName}' 启动类型为: {startType}");

            await Task.Run(() =>
            {
                try
                {
                    string scStartType = startType.ToLower() switch
                    {
                        "disabled" => "disabled",
                        "manual" => "demand",
                        "auto" => "auto",
                        _ => throw new ArgumentException($"不支持的启动类型: {startType}")
                    };

                    var psi = new ProcessStartInfo
                    {
                        FileName = "sc",
                        Arguments = $"config \"{serviceName}\" start= {scStartType}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var process = new Process())
                    {
                        process.StartInfo = psi;
                        process.Start();

                        // 异步读取输出
                        var outputTask = process.StandardOutput.ReadToEndAsync();
                        var errorTask = process.StandardError.ReadToEndAsync();

                        // 设置超时
                        if (!process.WaitForExit(30000)) // 30秒超时
                        {
                            process.Kill();
                            throw new System.TimeoutException("SC命令执行超时");
                        }

                        string output = outputTask.Result;
                        string error = errorTask.Result;

                        if (process.ExitCode == 0)
                        {
                            Logs.LogInfo($"SC命令成功设置服务 '{serviceName}' 启动类型为: {startType}");
                        }
                        else
                        {
                            // 检查错误类型
                            if (error.Contains("1060") || output.Contains("指定的服务未安装"))
                            {
                                throw new InvalidOperationException($"服务 '{serviceName}' 不存在");
                            }
                            throw new InvalidOperationException($"SC命令失败 (退出码: {process.ExitCode}): {error}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logs.LogError($"SC命令设置服务启动类型失败: {ex.Message}");
                    throw;
                }
            });
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        private static async Task StopServiceAsync(string serviceName)
        {
            Logs.LogInfo($"正在停止服务: {serviceName}");

            await Task.Run(() =>
            {
                try
                {
                    using (ServiceController sc = new ServiceController(serviceName))
                    {
                        if (sc.Status == ServiceControllerStatus.Running)
                        {
                            sc.Stop();
                            sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                            Logs.LogInfo($"服务 '{serviceName}' 已停止");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 如果普通停止失败，尝试强制停止
                    try
                    {
                        Logs.LogWarning($"普通停止失败，尝试强制停止服务: {serviceName}");
                        ForceStopService(serviceName);
                    }
                    catch
                    {
                        // 重新抛出原始异常
                        throw new InvalidOperationException($"无法停止服务 '{serviceName}'", ex);
                    }
                }
            });
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        private static async Task StartServiceAsync(string serviceName)
        {
            Logs.LogInfo($"正在启动服务: {serviceName}");

            await Task.Run(() =>
            {
                try
                {
                    using (ServiceController sc = new ServiceController(serviceName))
                    {
                        if (sc.Status == ServiceControllerStatus.Stopped)
                        {
                            sc.Start();
                            sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                            Logs.LogInfo($"服务 '{serviceName}' 已启动");
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"无法启动服务 '{serviceName}'", ex);
                }
            });
        }

        /// <summary>
        /// 强制停止服务
        /// </summary>
        private static void ForceStopService(string serviceName)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = $"stop \"{serviceName}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    if (process == null)  // 空值检查
                    {
                        throw new InvalidOperationException($"无法启动进程来停止服务: {serviceName}");
                    }

                    process.WaitForExit();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    if (process.ExitCode == 0)
                    {
                        Logs.LogInfo($"服务 '{serviceName}' 已强制停止");
                    }
                    else
                    {
                        throw new InvalidOperationException($"sc stop 命令失败: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"强制停止服务 '{serviceName}' 失败", ex);
            }
        }

        #endregion SC 命令操作

        #region SC 命令查询

        /// <summary>
        /// 检查服务是否存在
        /// </summary>
        private static async Task<bool> IsServiceExistsAsync(string serviceName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (ServiceController sc = new ServiceController(serviceName))
                    {
                        // 如果能获取到服务名称，说明服务存在
                        string name = sc.ServiceName;
                        return true;
                    }
                }
                catch (InvalidOperationException)
                {
                    // 服务不存在
                    return false;
                }
                catch (Exception ex)
                {
                    Logs.LogError($"检查服务 '{serviceName}' 是否存在时出错", ex);
                    return false;
                }
            });
        }

        /// <summary>
        /// 获取服务状态
        /// </summary>
        private static async Task<ServiceStatus?> GetServiceStatusAsync(string serviceName)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    using (ServiceController sc = new ServiceController(serviceName))
                    {
                        var status = new ServiceStatus
                        {
                            Status = sc.Status
                        };

                        // 使用 WMI 查询启动类型
                        status.StartType = await GetServiceStartTypeFromWMIAsync(serviceName) ?? "unknown";

                        return status;
                    }
                }
                catch (Exception ex)
                {
                    Logs.LogError($"获取服务 '{serviceName}' 状态时出错", ex);
                    return null;
                }
            });
        }


        /// <summary>
        /// 通过 sc qc 命令获取启动类型（增强解析）
        /// </summary>
        private static string? GetStartTypeFromSC(string serviceName)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = $"qc \"{serviceName}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8
                };

                using (var process = Process.Start(psi))
                {
                    if (process == null) return null;

                    process.WaitForExit(15000); // 15秒超时
                    string output = process.StandardOutput.ReadToEnd();

                    // 增强解析逻辑
                    var lines = output.Split('\n');
                    foreach (var line in lines)
                    {
                        string trimmedLine = line.Trim();

                        if (trimmedLine.Contains("START_TYPE"))
                        {
                            if (trimmedLine.Contains("DISABLED") || trimmedLine.Contains("4"))
                                return "disabled";
                            else if (trimmedLine.Contains("DEMAND_START") || trimmedLine.Contains("3"))
                                return "manual";
                            else if (trimmedLine.Contains("AUTO_START") || trimmedLine.Contains("2"))
                                return "auto";
                        }

                        // 同时检查启动模式行
                        if (trimmedLine.Contains("START_TYPE") && trimmedLine.Contains(":"))
                        {
                            var parts = trimmedLine.Split(':');
                            if (parts.Length >= 2)
                            {
                                string value = parts[1].Trim();
                                if (value.Contains("DISABLED")) return "disabled";
                                if (value.Contains("DEMAND_START")) return "manual";
                                if (value.Contains("AUTO_START")) return "auto";
                            }
                        }
                    }

                    Logs.LogWarning($"无法从SC输出中解析启动类型: {output}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logs.LogError($"SC命令查询启动类型失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取系统中所有服务列表
        /// </summary>
        public static async Task ListAllServicesAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    Logs.LogInfo("=== 系统服务列表 ===");
                    ServiceController[] services = ServiceController.GetServices();

                    foreach (var service in services)
                    {
                        string displayName = service.DisplayName ?? "无显示名";
                        Logs.LogInfo($"[{service.Status}] {service.ServiceName} - {displayName}");
                    }

                    Logs.LogInfo($"=== 共 {services.Length} 个服务 ===");
                }
                catch (Exception ex)
                {
                    Logs.LogError("获取服务列表失败", ex);
                }
            });
        }

        /// <summary>
        /// 获取服务的详细信息
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns>服务详细信息，如果获取失败则返回null</returns>
        public static async Task<ServiceDetailInfo?> GetServiceDetailAsync(string serviceName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (ServiceController sc = new ServiceController(serviceName))
                    {
                        var detail = new ServiceDetailInfo
                        {
                            ServiceName = sc.ServiceName,
                            DisplayName = sc.DisplayName,
                            Status = sc.Status.ToString(),
                            CanStop = sc.CanStop,
                            CanPauseAndContinue = sc.CanPauseAndContinue
                        };

                        // 获取启动类型
                        using (var searcher = new ManagementObjectSearcher(
                            $"SELECT StartMode, Description FROM Win32_Service WHERE Name = '{serviceName}'"))
                        {
                            foreach (ManagementObject service in searcher.Get())
                            {
                                detail.StartType = service["StartMode"]?.ToString();
                                detail.Description = service["Description"]?.ToString();
                                break;
                            }
                        }

                        return detail;
                    }
                }
                catch (InvalidOperationException)
                {
                    // 服务不存在
                    Logs.LogWarning($"服务 '{serviceName}' 不存在");
                    return null;
                }
                catch (Exception ex)
                {
                    Logs.LogError($"获取服务 '{serviceName}' 详细信息失败", ex);
                    return null;
                }
            });
        }

        /// <summary>
        /// 服务状态信息类
        /// </summary>
        private class ServiceStatus
        {
            public ServiceControllerStatus Status { get; set; }
            public string? StartType { get; set; }  // 改为可空类型

            public ServiceStatus()
            {
                // 设置默认值
                Status = ServiceControllerStatus.Stopped;
                StartType = "unknown";  // 在构造函数中初始化
            }
        }

        #endregion SC 命令查询

        #region 辅助功能

        /// <summary>
        /// 回退方法：通过ServiceController查询服务名称
        /// </summary>
        private static string? GetServiceNameByDisplayNameFallback(string displayName)
        {
            try
            {
                ServiceController[] services = ServiceController.GetServices();
                foreach (var service in services)
                {
                    if (service.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase))
                    {
                        Logs.LogInfo($"回退方法找到服务: 显示名称='{displayName}', 服务名称='{service.ServiceName}'");
                        return service.ServiceName;
                    }
                }

                Logs.LogWarning($"未找到显示名称为 '{displayName}' 的服务");
                return null;
            }
            catch (Exception ex)
            {
                Logs.LogError($"回退方法查询服务失败: '{displayName}'", ex);
                return null;
            }
        }

        /// <summary>
        /// 回退到 sc 命令
        /// </summary>
        private static void FallbackToSCCommand(string serviceName, string startType)
        {
            Logs.LogInfo($"回退到 sc 命令设置服务 '{serviceName}' 启动类型");

            try
            {
                // 使用 sc config 命令
                string scStartType = startType.ToLower() switch
                {
                    "disabled" => "disabled",
                    "manual" => "demand",
                    "auto" => "auto",
                    _ => throw new ArgumentException($"不支持的启动类型: {startType}")
                };

                var psi = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = $"config \"{serviceName}\" start= {scStartType}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    if (process == null)
                    {
                        throw new InvalidOperationException($"无法启动进程");
                    }

                    process.WaitForExit();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    if (process.ExitCode != 0)
                    {
                        throw new InvalidOperationException($"sc命令失败: {error}");
                    }

                    Logs.LogInfo($"sc命令成功设置服务启动类型");
                }
            }
            catch (Exception ex)
            {
                Logs.LogError($"回退到 sc 命令也失败: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// 检查当前是否有管理员权限
        /// </summary>
        /// <returns>是否具有管理员权限</returns>
        public static bool CheckAdministratorPrivilegesAsync()
        {
            try
            {
                // 尝试执行需要管理员权限的操作 - 获取服务列表
                ServiceController[] services = ServiceController.GetServices();
                Logs.LogInfo("用户以管理员启动");
                return true;
            }
            catch (System.Security.SecurityException)
            {
                Logs.LogInfo("用户未以管理员启动，警告并退出");
                return false;
            }
        }

        #endregion 辅助功能

    }
}
