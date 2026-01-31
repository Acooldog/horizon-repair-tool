using System.Diagnostics;
using System.ServiceProcess;
using System.Management;
using test.src.Services.Helpers;
using System.Text;
using test.src.Services.Model;

namespace test.src.Services.Managers.ServiceManagerAll
{
    /// <summary>
    /// 禁用或启用的服务管理类
    /// </summary>
    public partial class ServiceManager
    {
        #region 禁用服务
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

        // 以下是原有的方法，保持不变
        /// <summary>
        /// 通过服务名称禁用单个 Windows 服务
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

        #endregion 禁用服务

        #region 启用服务
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

        #endregion 启用服务

        #region 设置服务

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

        #endregion 设置服务

    }
}