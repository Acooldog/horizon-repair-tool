using System.Diagnostics;
using System.ServiceProcess;
using System.Management;
namespace test.tools
{
    public class ServiceManager
    {
        /// <summary>
        /// 禁用指定的 Windows 服务
        /// </summary>
        /// <param name="serviceNames">要禁用的服务名称数组</param>
        /// <returns>禁用结果</returns>
        public static string DisableServices(string[] serviceNames)
        {
            if (serviceNames == null || serviceNames.Length == 0)
            {
                Logs.LogWarning("服务名数组为空，没有服务需要禁用");
                return "None";
            }

            Logs.LogInfo($"开始禁用服务，共 {serviceNames.Length} 个服务");
            Logs.LogInfo($"服务列表: {string.Join(", ", serviceNames)}");

            int successCount = 0;
            int failedCount = 0;
            int notExistCount = 0;

            foreach (string serviceName in serviceNames)
            {
                try
                {
                    DisableSingleService(serviceName);
                    successCount++;
                }
                catch (InvalidProgramException)
                {
                    Logs.LogWarning($"服务 '{serviceName}' 不存在，跳过");
                    notExistCount++;
                }
                catch (InvalidOperationException)
                {
                    Logs.LogError($"禁用服务 '{serviceName}' 失败");
                    failedCount++;
                }
            }

            Logs.LogInfo($"服务禁用完成。成功: {successCount} 个, " +
                $"失败: {failedCount} 个, " + 
                $"不存在: {notExistCount} 个," +
                $"总计: {successCount + failedCount + notExistCount}");

            return $"服务禁用完成。成功: {successCount} 个, " +
                $"失败: {failedCount} 个, " +
                $"不存在: {notExistCount} 个," +
                $"总计: {successCount + failedCount + notExistCount}";
        }

        /// <summary>
        /// 禁用单个 Windows 服务
        /// </summary>
        private static void DisableSingleService(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                Logs.LogWarning("收到空服务名，跳过");
                return;
            }

            // 验证服务名格式，防止命令注入
            //if (!IsValidServiceName(serviceName))
            //{
            //    Logs.LogError($"服务名 '{serviceName}' 包含非法字符，跳过");
            //    return;
            //}

            Logs.LogInfo($"处理服务: {serviceName}");

            // 1. 检查服务是否存在
            if (!IsServiceExists(serviceName))
            {
                Logs.LogWarning($"服务 '{serviceName}' 不存在，跳过");
                throw new InvalidProgramException($"服务 '{serviceName}' 不存在");
            }

            // 2. 检查当前状态 - 修复第76行警告
            ServiceStatus? currentStatus = GetServiceStatus(serviceName);
            if (currentStatus == null)
            {
                Logs.LogError($"无法获取服务 '{serviceName}' 的状态");
                return;
            }

            Logs.LogInfo($"服务 '{serviceName}' 当前状态: {currentStatus.Status}，启动类型: {currentStatus.StartType}");

            // 3. 停止服务（如果正在运行）
            if (currentStatus.Status == ServiceControllerStatus.Running)
            {
                StopService(serviceName);
            }

            // 4. 设置启动类型为禁用
            SetServiceStartType(serviceName, "disabled");

            // 5. 验证设置结果 - 修复第95行警告
            ServiceStatus? newStatus = GetServiceStatus(serviceName);
            Logs.LogInfo($"服务 '{serviceName}' 新状态: {newStatus?.Status}，启动类型: {newStatus?.StartType}");
            if (newStatus == null)
            {
                Logs.LogError($"无法验证服务 '{serviceName}' 的状态");
                return;
            }

            if (newStatus.StartType?.ToLower() == "disabled")
            {
                Logs.LogInfo($"服务 '{serviceName}' 已成功设置为禁用");
            }
            else
            {
                string currentStartType = newStatus.StartType ?? "未知";
                throw new InvalidOperationException($"服务 '{serviceName}' 设置禁用失败，当前启动类型: {currentStartType}");
            }
        }

        /// <summary>
        /// 验证服务名是否合法，防止命令注入
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <returns>是否合法</returns>
        private static bool IsValidServiceName(string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName))
                return false;

            // 服务名通常只包含字母、数字、连字符和下划线
            foreach (char c in serviceName)
            {
                if (!(char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.'))
                {
                    return false;
                }
            }

            // 长度限制
            if (serviceName.Length > 256)
                return false;

            return true;
        }

        /// <summary>
        /// 检查服务是否存在
        /// </summary>
        private static bool IsServiceExists(string serviceName)
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
        }


        /// <summary>
        /// 获取服务状态
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private static ServiceStatus? GetServiceStatus(string serviceName)
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
                    status.StartType = GetServiceStartTypeFromWMI(serviceName) ?? "unknown";

                    return status;
                }
            }
            catch (Exception ex)
            {
                Logs.LogError($"获取服务 '{serviceName}' 状态时出错", ex);
                return null;
            }
        }

        /// <summary>
        /// 通过 WMI 获取服务启动类型
        /// </summary>
        private static string? GetServiceStartTypeFromWMI(string serviceName)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(
                    $"SELECT StartMode FROM Win32_Service WHERE Name = '{serviceName}'"))
                {
                    foreach (ManagementObject service in searcher.Get())
                    {
                        return service["StartMode"]?.ToString();
                    }
                }
            }
            catch
            {
                // 回退到 sc query
                return GetStartTypeFromSC(serviceName);
            }

            return null;
        }

        /// <summary>
        /// 通过 sc qc 命令获取启动类型
        /// </summary>
        private static string? GetStartTypeFromSC(string serviceName)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = $"qc \"{serviceName}\"",  // qc 查询配置
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    if (process == null) return null;

                    process.WaitForExit();
                    string output = process.StandardOutput.ReadToEnd();

                    // 解析 START_TYPE
                    var lines = output.Split('\n');
                    foreach (var line in lines)
                    {
                        if (line.Contains("START_TYPE"))
                        {
                            if (line.Contains("DISABLED"))
                                return "disabled";
                            else if (line.Contains("DEMAND_START"))
                                return "manual";
                            else if (line.Contains("AUTO_START"))
                                return "auto";
                        }
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        private static void StopService(string serviceName)
        {
            Logs.LogInfo($"正在停止服务: {serviceName}");

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
                    if (process == null)  // 新增：空值检查
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

        /// <summary>
        /// 设置服务启动类型
        /// </summary>
        /// <param name="serviceName">服务名</param>
        /// <param name="startType">启动类型: disabled, manual, auto</param>
        private static void SetServiceStartType(string serviceName, string startType)
        {
            Logs.LogInfo($"设置服务 '{serviceName}' 启动类型为: {startType}");

            try
            {
                // 使用 sc config 命令设置启动类型
                string scStartType = "";
                switch (startType.ToLower())
                {
                    case "disabled":
                        scStartType = "disabled";
                        break;
                    case "manual":
                        scStartType = "demand";
                        break;
                    case "auto":
                        scStartType = "auto";
                        break;
                    default:
                        throw new ArgumentException($"不支持的启动类型: {startType}");
                }

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
                    if (process == null)  // 新增：空值检查
                    {
                        throw new InvalidOperationException($"无法启动进程来设置服务启动类型: {serviceName}");
                    }

                    process.WaitForExit();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    if (process.ExitCode != 0)
                    {
                        throw new InvalidOperationException($"设置启动类型失败: {error}");
                    }

                    Logs.LogInfo($"服务 '{serviceName}' 启动类型已设置为 {startType}");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"设置服务 '{serviceName}' 启动类型失败", ex);
            }
        }

        /// <summary>
        /// 获取系统中所有服务列表
        /// </summary>
        public static void ListAllServices()
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
        }

        /// <summary>
        /// 检查当前是否有管理员权限
        /// </summary>
        /// <returns>是否具有管理员权限</returns>
        public static bool CheckAdministratorPrivileges()
        {
            try
            {
                // 尝试执行需要管理员权限的操作 - 获取服务列表
                ServiceController[] services = ServiceController.GetServices();
                return true;
            }
            catch (System.Security.SecurityException)
            {
                return false;
            }
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
    }
}