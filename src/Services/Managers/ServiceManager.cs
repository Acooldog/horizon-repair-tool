using System.Diagnostics;
using System.ServiceProcess;
using System.Management;
using test.src.Services.Helpers;
using System.Text;

using EnumerationOptions = System.Management.EnumerationOptions; // 明确使用Management命名空间的EnumerationOptions

namespace test.src.Services.Managers
{
    public class ServiceManager
    {
        /// <summary>
        /// 通过显示名称获取服务名称（使用WMI查询）
        /// </summary>
        public static async Task<string?> GetServiceNameByDisplayNameAsync(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                Logs.LogWarning("显示名称为空");
                return null;
            }

            return await Task.Run(() =>
            {
                try
                {
                    Logs.LogInfo($"正在通过WMI查询服务名称: '{displayName}'");

                    // 使用WMI查询服务名称
                    using (var searcher = new ManagementObjectSearcher(
                        $"SELECT Name FROM Win32_Service WHERE DisplayName = '{displayName.Replace("'", "''")}'"))
                    {
                        var options = new EnumerationOptions
                        {
                            Timeout = TimeSpan.FromSeconds(10), // 设置超时
                            ReturnImmediately = true
                        };
                        searcher.Options = options;

                        ManagementObjectCollection? services = null;

                        // 使用Task包装WMI查询，便于超时控制
                        var wmiTask = Task.Run(() => searcher.Get());
                        if (wmiTask.Wait(TimeSpan.FromSeconds(15)))
                        {
                            services = wmiTask.Result;
                        }
                        else
                        {
                            Logs.LogWarning($"WMI查询超时，尝试回退方法");
                            throw new System.TimeoutException("WMI查询超时");
                        }

                        foreach (ManagementObject service in services)
                        {
                            string serviceName = service["Name"]?.ToString() ?? "";
                            if (!string.IsNullOrEmpty(serviceName))
                            {
                                Logs.LogInfo($"WMI找到服务: 显示名称='{displayName}', 服务名称='{serviceName}'");
                                return serviceName;
                            }
                        }

                        // 如果WMI查询失败，回退到通过ServiceController查询
                        Logs.LogWarning($"WMI未找到服务，尝试通过ServiceController查询: '{displayName}'");
                        return GetServiceNameByDisplayNameFallback(displayName);
                    }
                }
                catch (System.TimeoutException)
                {
                    Logs.LogWarning($"WMI查询超时，回退到ServiceController查询");
                    return GetServiceNameByDisplayNameFallback(displayName);
                }
                catch (Exception ex)
                {
                    Logs.LogError($"通过WMI查询服务名称失败: '{displayName}'", ex);

                    // 回退到通过ServiceController查询
                    try
                    {
                        return GetServiceNameByDisplayNameFallback(displayName);
                    }
                    catch
                    {
                        return null;
                    }
                }
            });
        }

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
        /// 禁用指定的 Windows 服务
        /// </summary>
        /// <param name="serviceNames">要禁用的服务名称或显示名称数组</param>
        /// <param name="useDisplayName">true=使用显示名称进行操作, false=使用服务名称进行操作</param>
        /// <returns>禁用结果</returns>
        public static async Task<string> DisableServicesAsync(string[] serviceNames, bool useDisplayName = false)
        {
            if (serviceNames == null || serviceNames.Length == 0)
            {
                Logs.LogWarning("服务名数组为空，没有服务需要禁用");
                return "None";
            }

            string operationType = useDisplayName ? "显示名称" : "服务名称";
            Logs.LogInfo($"开始禁用服务，共 {serviceNames.Length} 个服务，操作类型: {operationType}");
            Logs.LogInfo($"服务列表: {string.Join(", ", serviceNames)}");

            int successCount = 0;
            int failedCount = 0;
            int notExistCount = 0;
            int nameResolveFailedCount = 0;

            foreach (string name in serviceNames)
            {
                try
                {
                    string actualServiceName = name;

                    // 如果需要使用显示名称，先转换为服务名称
                    if (useDisplayName)
                    {
                        Logs.LogInfo($"正在将显示名称 '{name}' 转换为服务名称...");
                        string? resolvedName = await GetServiceNameByDisplayNameAsync(name);

                        if (string.IsNullOrEmpty(resolvedName))
                        {
                            Logs.LogError($"无法找到显示名称为 '{name}' 的服务");
                            nameResolveFailedCount++;
                            continue;
                        }

                        actualServiceName = resolvedName;
                        Logs.LogInfo($"显示名称 '{name}' 对应的服务名称为: {actualServiceName}");
                    }

                    await DisableSingleServiceAsync(actualServiceName);
                    successCount++;
                }
                catch (InvalidProgramException)
                {
                    Logs.LogWarning($"服务 '{name}' 不存在，跳过");
                    notExistCount++;
                }
                catch (InvalidOperationException ex)
                {
                    if (useDisplayName && ex.Message.Contains("无法找到显示名称"))
                    {
                        nameResolveFailedCount++;
                    }
                    else
                    {
                        Logs.LogError($"禁用服务 '{name}' 失败: {ex.Message}");
                        failedCount++;
                    }
                }
                catch (Exception ex)
                {
                    Logs.LogError($"禁用服务 '{name}' 时发生未知错误: {ex.Message}");
                    failedCount++;
                }
            }

            string result = $"服务禁用完成。成功: {successCount} 个," +
                          $"失败: {failedCount} 个," +
                          $"不存在: {nameResolveFailedCount} 个," +
                          $"名称解析失败: {nameResolveFailedCount} 个," +
                          $"总计: {successCount + failedCount + notExistCount + nameResolveFailedCount} 个";

            Logs.LogInfo(result);
            return result;
        }

        /// <summary>
        /// 启用指定的 Windows 服务
        /// </summary>
        /// <param name="serviceNames">要启用的服务名称或显示名称数组</param>
        /// <param name="autoStart">true=自动启动, false=手动启动</param>
        /// <param name="useDisplayName">true=使用显示名称进行操作, false=使用服务名称进行操作</param>
        /// <returns>启用结果</returns>
        public static async Task<string> EnableServicesAsync(string[] serviceNames, bool autoStart, bool useDisplayName = false)
        {
            if (serviceNames == null || serviceNames.Length == 0)
            {
                Logs.LogWarning("服务名数组为空，没有服务需要启用");
                return "None";
            }

            string startType = autoStart ? "自动启动" : "手动启动";
            string operationType = useDisplayName ? "显示名称" : "服务名称";
            Logs.LogInfo($"开始启用服务，共 {serviceNames.Length} 个服务，启动类型: {startType}，操作类型: {operationType}");
            Logs.LogInfo($"服务列表: {string.Join(", ", serviceNames)}");

            int successCount = 0;
            int failedCount = 0;
            int notExistCount = 0;
            int nameResolveFailedCount = 0;

            foreach (string name in serviceNames)
            {
                try
                {
                    string actualServiceName = name;

                    // 如果需要使用显示名称，先转换为服务名称
                    if (useDisplayName)
                    {
                        Logs.LogInfo($"正在将显示名称 '{name}' 转换为服务名称...");
                        string? resolvedName = await GetServiceNameByDisplayNameAsync(name);

                        if (string.IsNullOrEmpty(resolvedName))
                        {
                            Logs.LogError($"无法找到显示名称为 '{name}' 的服务");
                            nameResolveFailedCount++;
                            continue;
                        }

                        actualServiceName = resolvedName;
                        Logs.LogInfo($"显示名称 '{name}' 对应的服务名称为: {actualServiceName}");
                    }

                    await EnableSingleServiceAsync(actualServiceName, autoStart);
                    successCount++;
                }
                catch (InvalidProgramException)
                {
                    Logs.LogWarning($"服务 '{name}' 不存在，跳过");
                    notExistCount++;
                }
                catch (InvalidOperationException ex)
                {
                    if (useDisplayName && ex.Message.Contains("无法找到显示名称"))
                    {
                        nameResolveFailedCount++;
                    }
                    else
                    {
                        Logs.LogError($"启用服务 '{name}' 失败: {ex.Message}");
                        failedCount++;
                    }
                }
                catch (Exception ex)
                {
                    Logs.LogError($"启用服务 '{name}' 时发生未知错误: {ex.Message}");
                    failedCount++;
                }
            }

            string result = $"服务启用完成。成功: {successCount} 个," +
                          $"失败: {failedCount} 个, " +
                          $"不存在: {nameResolveFailedCount} 个," +
                          $"名称解析失败: {nameResolveFailedCount} 个," +
                          $"总计: {successCount + failedCount + notExistCount + nameResolveFailedCount} 个," +
                          $"启动类型: {startType}";

            Logs.LogInfo(result);
            return result;
        }

        /// <summary>
        /// 重载方法：禁用指定的 Windows 服务（保持向后兼容）
        /// </summary>
        /// <param name="serviceNames">要禁用的服务名称数组</param>
        /// <returns>禁用结果</returns>
        public static async Task<string> DisableServicesAsync(string[] serviceNames)
        {
            return await DisableServicesAsync(serviceNames, false);
        }

        /// <summary>
        /// 重载方法：启用指定的 Windows 服务（保持向后兼容）
        /// </summary>
        /// <param name="serviceNames">要启用的服务名称数组</param>
        /// <param name="autoStart">true=自动启动, false=手动启动</param>
        /// <returns>启用结果</returns>
        public static async Task<string> EnableServicesAsync(string[] serviceNames, bool autoStart)
        {
            return await EnableServicesAsync(serviceNames, autoStart, false);
        }

        // 以下是原有的方法，保持不变
        /// <summary>
        /// 禁用单个 Windows 服务
        /// </summary>
        private static async Task DisableSingleServiceAsync(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                Logs.LogWarning("收到空服务名，跳过");
                return;
            }

            Logs.LogInfo($"处理服务: {serviceName}");

            // 1. 检查服务是否存在
            if (!await IsServiceExistsAsync(serviceName))
            {
                Logs.LogWarning($"服务 '{serviceName}' 不存在，跳过");
                throw new InvalidProgramException($"服务 '{serviceName}' 不存在");
            }

            // 2. 检查当前状态
            ServiceStatus? currentStatus = await GetServiceStatusAsync(serviceName);
            if (currentStatus == null)
            {
                Logs.LogError($"无法获取服务 '{serviceName}' 的状态");
                return;
            }

            Logs.LogInfo($"服务 '{serviceName}' 当前状态: {currentStatus.Status}，启动类型: {currentStatus.StartType}");

            // 3. 停止服务（如果正在运行）
            if (currentStatus.Status == ServiceControllerStatus.Running)
            {
                await StopServiceAsync(serviceName);
            }

            // 4. 设置启动类型为禁用
            await SetServiceStartTypeAsync(serviceName, "disabled");

            // 5. 验证设置结果
            ServiceStatus? newStatus = await GetServiceStatusAsync(serviceName);
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
        /// 启用单个 Windows 服务
        /// </summary>
        /// <param name="serviceName">服务名称</param>
        /// <param name="autoStart">true=自动启动, false=手动启动</param>
        private static async Task EnableSingleServiceAsync(string serviceName, bool autoStart)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                Logs.LogWarning("收到空服务名，跳过");
                return;
            }

            Logs.LogInfo($"处理服务: {serviceName}");

            // 1. 检查服务是否存在
            if (!await IsServiceExistsAsync(serviceName))
            {
                Logs.LogWarning($"服务 '{serviceName}' 不存在，跳过");
                throw new InvalidProgramException($"服务 '{serviceName}' 不存在");
            }

            // 2. 检查当前状态
            ServiceStatus? currentStatus = await GetServiceStatusAsync(serviceName);
            if (currentStatus == null)
            {
                Logs.LogError($"无法获取服务 '{serviceName}' 的状态");
                return;
            }

            Logs.LogInfo($"服务 '{serviceName}' 当前状态: {currentStatus.Status}，启动类型: {currentStatus.StartType}");

            // 3. 设置启动类型
            string startType = autoStart ? "auto" : "manual";
            await SetServiceStartTypeAsync(serviceName, startType);

            // 4. 如果设置为自动启动且当前已停止，则启动服务
            if (autoStart && currentStatus.Status == ServiceControllerStatus.Stopped)
            {
                try
                {
                    await StartServiceAsync(serviceName);
                }
                catch (Exception ex)
                {
                    Logs.LogWarning($"服务 '{serviceName}' 启动失败: {ex.Message}");
                    // 启动失败不影响启动类型设置
                }
            }

            // 5. 验证设置结果
            ServiceStatus? newStatus = await GetServiceStatusAsync(serviceName);
            Logs.LogInfo($"服务 '{serviceName}' 新状态: {newStatus?.Status}，启动类型: {newStatus?.StartType}");

            if (newStatus == null)
            {
                Logs.LogError($"无法验证服务 '{serviceName}' 的状态");
                return;
            }

            string expectedStartType = autoStart ? "auto" : "manual";
            if (newStatus.StartType?.ToLower() == expectedStartType)
            {
                Logs.LogInfo($"服务 '{serviceName}' 已成功设置为{expectedStartType}");
            }
            else
            {
                string currentStartType = newStatus.StartType ?? "未知";
                throw new InvalidOperationException($"服务 '{serviceName}' 设置启用失败，当前启动类型: {currentStartType}");
            }
        }

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
        /// 通过 WMI 获取服务启动类型（带超时控制）
        /// </summary>
        private static async Task<string?> GetServiceStartTypeFromWMIAsync(string serviceName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var searcher = new ManagementObjectSearcher(
                        $"SELECT StartMode FROM Win32_Service WHERE Name = '{serviceName}'"))
                    {
                        var options = new EnumerationOptions
                        {
                            Timeout = TimeSpan.FromSeconds(10),
                            ReturnImmediately = true
                        };
                        searcher.Options = options;

                        // 使用超时控制
                        var getTask = Task.Run(() => searcher.Get());
                        if (getTask.Wait(TimeSpan.FromSeconds(15)))
                        {
                            foreach (ManagementObject service in getTask.Result)
                            {
                                return service["StartMode"]?.ToString();
                            }
                        }
                        else
                        {
                            Logs.LogWarning($"WMI查询启动类型超时，回退到SC命令");
                            return GetStartTypeFromSC(serviceName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logs.LogWarning($"WMI查询启动类型失败: {ex.Message}，回退到SC命令");
                    return GetStartTypeFromSC(serviceName);
                }

                return null;
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

        /// <summary>
        /// 设置服务启动类型（优先使用SC命令，WMI作为备选）
        /// </summary>
        private static async Task SetServiceStartTypeAsync(string serviceName, string startType)
        {
            Logs.LogInfo($"设置服务 '{serviceName}' 启动类型为: {startType}");

            try
            {
                // 首选使用SC命令（更稳定）
                await SetServiceStartTypeBySCAsync(serviceName, startType);
            }
            catch (Exception scEx)
            {
                Logs.LogWarning($"SC命令设置失败，尝试WMI: {scEx.Message}");

                try
                {
                    // 回退到WMI
                    await SetServiceStartTypeByWMIAsync(serviceName, startType);
                }
                catch (Exception wmiEx)
                {
                    // 记录详细的错误信息
                    Logs.LogError($"所有设置方法都失败", wmiEx);

                    // 抛出合并的异常信息
                    throw new InvalidOperationException(
                        $"无法设置服务 '{serviceName}' 的启动类型\n" +
                        $"SC命令错误: {scEx.Message}\n" +
                        $"WMI错误: {wmiEx.Message}",
                        wmiEx
                    );
                }
            }
        }

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

        /// <summary>
        /// 服务详细信息类
        /// </summary>
        public class ServiceDetailInfo
        {
            public string ServiceName { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string? StartType { get; set; }
            public string? Description { get; set; }
            public bool CanStop { get; set; }
            public bool CanPauseAndContinue { get; set; }
        }

        /// <summary>
        /// 使用 WMI 设置服务启动类型
        /// </summary>
        /// <param name="serviceName">服务名</param>
        /// <param name="startType">启动类型: disabled, manual, auto</param>
        private static async Task SetServiceStartTypeByWMIAsync(string serviceName, string startType)
        {
            Logs.LogInfo($"使用WMI设置服务 '{serviceName}' 启动类型为: {startType}");

            await Task.Run(() =>
            {
                try
                {
                    // 将用户友好的类型转换为 WMI 类型
                    string wmiStartType = ConvertToWMIStartType(startType);

                    // 创建 WMI 查询
                    using (var searcher = new ManagementObjectSearcher(
                        $"SELECT * FROM Win32_Service WHERE Name = '{serviceName}'"))
                    {
                        var services = searcher.Get();

                        Logs.LogInfo($"正在查找服务: {serviceName}");
                        if (services.Count == 0)
                        {   
                            throw new InvalidOperationException($"找不到服务: {serviceName}");
                        }
                        Logs.LogInfo($"找到服务: {serviceName}");
                        foreach (ManagementObject service in services)
                        {
                            try
                            {
                                // 获取当前启动类型
                                string currentStartMode = service["StartMode"]?.ToString() ?? "未知";
                                Logs.LogInfo($"服务 '{serviceName}' 当前启动类型: {currentStartMode}");

                                // 如果需要修改
                                if (string.Equals(currentStartMode, wmiStartType, StringComparison.OrdinalIgnoreCase))
                                {
                                    Logs.LogInfo($"服务 '{serviceName}' 已经是 {startType} 模式，无需修改");
                                    return;
                                }

                                // 修改启动类型
                                // 注意：WMI 的 ChangeStartMode 方法在某些情况下可能不可用
                                // 所以我们使用 ManagementObject 的 InvokeMethod
                                var inParams = service.GetMethodParameters("Change");
                                inParams["StartMode"] = wmiStartType;

                                var result = service.InvokeMethod("Change", inParams, null);

                                // 检查返回结果
                                uint returnValue = (uint)result["ReturnValue"];

                                if (returnValue == 0)
                                {
                                    Logs.LogInfo($"服务 '{serviceName}' 启动类型已成功设置为 {startType} (WMI)");
                                }
                                else
                                {
                                    string errorMsg = GetWMIErrorMessage(returnValue);
                                    throw new InvalidOperationException($"WMI设置失败 (错误码: {returnValue}): {errorMsg}");
                                }
                            }
                            finally
                            {
                                service.Dispose();
                            }
                        }
                    }
                }
                catch (ManagementException ex)
                {
                    Logs.LogError($"WMI操作失败: {ex.Message}", ex);
                    // 回退到 sc 命令
                    FallbackToSCCommand(serviceName, startType);
                }
                catch (Exception ex)
                {
                    Logs.LogError($"设置服务 '{serviceName}' 启动类型失败", ex);
                    throw new InvalidOperationException($"设置服务 '{serviceName}' 启动类型失败", ex);
                }
            });
        }

        /// <summary>
        /// 将用户友好的启动类型转换为 WMI 类型
        /// </summary>
        private static string ConvertToWMIStartType(string userStartType)
        {
            return userStartType.ToLower() switch
            {
                "disabled" => "Disabled",
                "manual" => "Manual",
                "auto" => "Automatic",
                "automatic" => "Automatic",  // 额外支持
                "demand" => "Manual",        // sc 的 manual 对应 WMI 的 Manual
                _ => throw new ArgumentException($"不支持的启动类型: {userStartType}")
            };
        }

        /// <summary>
        /// 获取 WMI 错误消息
        /// </summary>
        private static string GetWMIErrorMessage(uint errorCode)
        {
            return errorCode switch
            {
                0 => "成功",
                1 => "不支持",
                2 => "访问被拒绝",
                3 => "依赖服务正在运行",
                4 => "无效的服务控制",
                5 => "服务无法接受控制",
                6 => "服务未运行",
                7 => "服务超时",
                8 => "未知错误",
                9 => "路径未找到",
                10 => "服务已运行",
                11 => "服务数据库被锁定",
                12 => "服务依赖被删除",
                13 => "服务依赖无效",
                14 => "服务已禁用",
                15 => "服务登录失败",
                16 => "服务被标记为删除",
                17 => "服务无线程",
                18 => "状态循环依赖",
                19 => "状态重复名称",
                20 => "状态无效名称",
                21 => "状态无效参数",
                22 => "状态服务不存在",
                23 => "服务已存在",
                24 => "已暂停",
                _ => $"未知错误代码: {errorCode}"
            };
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
    }
}