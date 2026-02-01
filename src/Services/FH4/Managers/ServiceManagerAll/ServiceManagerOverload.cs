using System.Collections.Specialized;
using System.Diagnostics;
using System.Management;
using System.ServiceProcess;
using System.Text;
using test.src.Services.PublicFuc.Helpers;
using EnumerationOptions = System.Management.EnumerationOptions;

namespace test.src.Services.FH4.Managers.ServiceManagerAll
{
    // ServiceManager的重载方法
    public partial class ServiceManager
	{
        // 禁用的进度条联动 非异步重载
        #region DisableServicesAsync 通过服务名称禁用指定的 Windows 服务
        /// <summary>
        /// 通过服务名称禁用指定的 Windows 服务 重载 进度条联动
        /// </summary>
        /// <param name="serviceNames">要禁用的服务名称或显示名称数组</param>
        /// <param name="useDisplayName">true=使用显示名称进行操作, false=使用服务名称进行操作</param>
        /// <param name="progressBar">是否与进度条联动</param>
        /// <returns>禁用结果</returns>
        public static void DisableServicesAsync(string[] serviceNames,
            bool useDisplayName = false,
            bool progressBar = false,
            Action<int, string>? ProgressCount = null)
        {
            // 禁用进度条数据模型初始化
            //UnEnableProgressM UnEnableProgress = new();

            if (serviceNames == null || serviceNames.Length == 0)
            {
                Logs.LogWarning("服务名数组为空，没有服务需要禁用");
                return;
            }

            string operationType = useDisplayName ? "显示名称" : "服务名称";
            Logs.LogInfo($"开始禁用服务，共 {serviceNames.Length} 个服务，操作类型: {operationType}");
            Logs.LogInfo($"服务列表: {string.Join(", ", serviceNames)}");

            int successCount = 0;
            int failedCount = 0;
            int notExistCount = 0;
            int nameResolveFailedCount = 0;
            // 赋值给数据模型
            //UnEnableProgress.successCount = successCount;
            //UnEnableProgress.failedCount = failedCount;
            //UnEnableProgress.notExistCount = notExistCount;
            //UnEnableProgress.nameResolveFailedCount = nameResolveFailedCount;
            int fullCount = serviceNames.Length;

            foreach (string name in serviceNames)
            {
                try
                {
                    string actualServiceName = name;
                    // 如果需要使用显示名称，先转换为服务名称
                    if (useDisplayName)
                    {
                        Logs.LogInfo($"正在将显示名称 '{name}' 转换为服务名称...");
                        string? resolvedName = GetServiceNameByDisplayNameAsync(name, true);

                        if (string.IsNullOrEmpty(resolvedName))
                        {
                            Logs.LogError($"无法找到显示名称为 '{name}' 的服务");
                            nameResolveFailedCount++;
                            continue;
                        }

                        actualServiceName = resolvedName;
                        Logs.LogInfo($"显示名称 '{name}' 对应的服务名称为: {actualServiceName}");
                    }

                    DisableSingleServiceAsync(actualServiceName, true);
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
                finally
                {
                    // 进度条值 = ( 当前数值 × 100 ) / 总数值 
                    // 判断哪个值变了 就用Logs输出变量名以及当前值和原来值，最后赋值给数据模型
                    //int successCountS =  JudgeServiceStatus("successCount", UnEnableProgress.successCount, successCount);
                    //int failedCountS = JudgeServiceStatus("failedCount", UnEnableProgress.failedCount, failedCount);
                    //int notExistCountS = JudgeServiceStatus("notExistCount", UnEnableProgress.notExistCount, notExistCount);
                    //int nameResolveFailedCountS = JudgeServiceStatus("nameResolveFailedCount", UnEnableProgress.nameResolveFailedCount, nameResolveFailedCount);
                    Logs.LogInfo($"======== 禁用进度: {(successCount + failedCount + notExistCount + nameResolveFailedCount)}/{fullCount} =========");
                    Logs.LogInfo($"======== 禁用计算: ({(successCount + failedCount + notExistCount + nameResolveFailedCount)} * 100)/{fullCount} = {calculateProgress((successCount + failedCount + notExistCount + nameResolveFailedCount), fullCount)} =========");
                    Logs.LogInfo($"======== 进度:{(successCount + failedCount + notExistCount + nameResolveFailedCount) / fullCount * 100} % ========");
                    ProgressCount?.Invoke(
                            calculateProgress((successCount + failedCount + notExistCount + nameResolveFailedCount), fullCount),
                            $"{calculateProgress((successCount + failedCount + notExistCount + nameResolveFailedCount), fullCount)}%"
                        );
                }
            }

            string result = $"服务禁用完成。成功: {successCount} 个," +
                          $"失败: {failedCount} 个," +
                          $"不存在: {nameResolveFailedCount} 个," +
                          $"名称解析失败: {nameResolveFailedCount} 个," +
                          $"总计: {successCount + failedCount + notExistCount + nameResolveFailedCount} 个";

            Logs.LogInfo(result);
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

        #endregion DisableServicesAsync 通过服务名称禁用指定的 Windows 服务

        // 下面的所有都是重载方法
        #region GetServiceNameByDisplayNameAsync 通过显示名称获取服务名称
        /// <summary>
        /// 通过显示名称获取服务名称 重载 进度条联动
        /// </summary>
        public static string? GetServiceNameByDisplayNameAsync(string displayName, bool Progress = false)
		{
			if (string.IsNullOrWhiteSpace(displayName))
			{
				Logs.LogWarning("显示名称为空");
				return null;
			}

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
		}

		#endregion GetServiceNameByDisplayNameAsync 通过显示名称获取服务名称

		
        #region EnableServicesAsync 通过服务名称启用指定的 Windows 服务

        /// <summary>
        /// 通过服务名称启用指定的 Windows 服务
        /// 重载方法：启用指定的 Windows 服务（保持向后兼容）
        /// </summary>
        /// <param name="serviceNames">要启用的服务名称数组</param>
        /// <param name="autoStart">true=自动启动, false=手动启动</param>
        /// <returns>启用结果</returns>
        public static async Task<string> EnableServicesAsync(string[] serviceNames, bool autoStart)
        {
            return await EnableServicesAsync(serviceNames, autoStart, false);
        }

        #endregion EnableServicesAsync 通过服务名称启用指定的 Windows 服务

    }

    // 重载实现与进度条进行联动 禁用
    public partial class ServiceManager
	{
        #region DisableSingleServiceAsync 通过服务名称禁用单个 Windows 服务

        /// <summary>
        /// 通过服务名称禁用单个 Windows 服务 重载与进度条联动
        /// </summary>
        /// <param name="serviceName">要禁用的服务名称</param>
        /// <param name="progressBar">是否与进度条联动</param>
        private static void DisableSingleServiceAsync(string serviceName, bool progressBar = false)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                Logs.LogWarning("收到空服务名，跳过");
                return;
            }

            Logs.LogInfo($"处理服务: {serviceName}");

            // 1. 检查服务是否存在
            if (!IsServiceExistsAsync(serviceName, true))
            {
                Logs.LogWarning($"服务 '{serviceName}' 不存在，跳过");
                throw new InvalidProgramException($"服务 '{serviceName}' 不存在");
            }

            // 2. 检查当前状态
            ServiceStatus? currentStatus = GetServiceStatusAsync(serviceName, true);
            if (currentStatus == null)
            {
                Logs.LogError($"无法获取服务 '{serviceName}' 的状态");
                return;
            }

            Logs.LogInfo($"服务 '{serviceName}' 当前状态: {currentStatus.Status}，启动类型: {currentStatus.StartType}");

            // 3. 停止服务（如果正在运行）
            if (currentStatus.Status == ServiceControllerStatus.Running)
            {
                StopServiceAsync(serviceName, true);
            }

            // 4. 设置启动类型为禁用
            SetServiceStartTypeAsync(serviceName, "disabled", true);

            // 5. 验证设置结果
            ServiceStatus? newStatus = GetServiceStatusAsync(serviceName, true);
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

        #endregion DisableSingleServiceAsync 通过服务名称禁用单个 Windows 服务

        #region IsServiceExistsAsync 检查服务是否存在

        /// <summary>
        /// 检查服务是否存在
        /// </summary>
        /// <param name="serviceName">要检查的服务名称</param>
        /// <param name="progressBar">是否与进度条联动</param>
        private static bool IsServiceExistsAsync(string serviceName, bool progressBar = false)
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

        #endregion IsServiceExistsAsync 检查服务是否存在

        #region GetServiceStatusAsync 获取服务状态
        /// <summary>
        /// 获取服务状态
        /// </summary>
        /// <param name="serviceName">要获取状态的服务名称</param>
        /// <param name="progressBar">是否与进度条联动</param>
        private static ServiceStatus? GetServiceStatusAsync(string serviceName, bool progressBar = false)
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
                    status.StartType = GetServiceStartTypeFromWMIAsync(serviceName, true) ?? "unknown";

                    return status;
                }
            }
            catch (Exception ex)
            {
                Logs.LogError($"获取服务 '{serviceName}' 状态时出错", ex);
                return null;
            }
        }
        #endregion GetServiceStatusAsync 获取服务状态

        #region GetServiceStartTypeFromWMIAsync 通过 WMI 获取服务启动类型
        /// <summary>
        /// 通过 WMI 获取服务启动类型（带超时控制）
        /// </summary>
        /// <param name="serviceName">要查询的服务名称</param>
        /// <param name="progressBar">是否与进度条联动</param>
        private static string? GetServiceStartTypeFromWMIAsync(string serviceName, bool progressBar = false)
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
        }
        #endregion GetServiceStartTypeFromWMIAsync 通过 WMI 获取服务启动类型

        #region StopServiceAsync 停止服务
        /// <summary>
        /// 停止服务
        /// </summary>
        private static void StopServiceAsync(string serviceName, bool progressBar = false)
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
        #endregion StopServiceAsync 停止服务

        #region SetServiceStartTypeAsync 设置服务启动类型
        /// <summary>
        /// 设置服务启动类型（优先使用SC命令，WMI作为备选）
        /// </summary>
        private static void SetServiceStartTypeAsync(string serviceName, string startType, bool progressBar = false)
        {
            Logs.LogInfo($"设置服务 '{serviceName}' 启动类型为: {startType}");

            try
            {
                // 首选使用SC命令（更稳定）
                SetServiceStartTypeBySCAsync(serviceName, startType, true);
            }
            catch (Exception scEx)
            {
                Logs.LogWarning($"SC命令设置失败，尝试WMI: {scEx.Message}");

                try
                {
                    // 回退到WMI
                    SetServiceStartTypeByWMIAsync(serviceName, startType, true);
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
        #endregion SetServiceStartTypeAsync 设置服务启动类型

        #region SetServiceStartTypeBySCAsync 通过SC命令设置服务启动类型

        /// <summary>
        /// 使用SC命令设置服务启动类型 重载 进度条联动
        /// </summary>
        private static void SetServiceStartTypeBySCAsync(string serviceName, string startType, bool progressBar = false)
        {
            Logs.LogInfo($"使用SC命令设置服务 '{serviceName}' 启动类型为: {startType}");

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
        }

        #endregion SetServiceStartTypeBySCAsync 通过SC命令设置服务启动类型

        #region SetServiceStartTypeByWMIAsync 通过WMI设置服务启动类型
        /// <summary>
        /// 使用 WMI 设置服务启动类型
        /// </summary>
        /// <param name="serviceName">服务名</param>
        /// <param name="startType">启动类型: disabled, manual, auto</param>
        private static void SetServiceStartTypeByWMIAsync(string serviceName, string startType, bool progressBar = false)
        {
            Logs.LogInfo($"使用WMI设置服务 '{serviceName}' 启动类型为: {startType}");

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
        }
        #endregion SetServiceStartTypeByWMIAsync 通过WMI设置服务启动类型

    }

}
