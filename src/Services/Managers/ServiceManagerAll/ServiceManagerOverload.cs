using System.Management;
using test.src.Services.Helpers;

using EnumerationOptions = System.Management.EnumerationOptions;

namespace test.src.Services.Managers.ServiceManagerAll
{   
    // ServiceManager的重载方法
	public partial class ServiceManager
	{
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
				finally
				{
					// 进度条值 = (当前数值 / 总数值) × 100
					// 判断哪个值变了 就用Logs输出变量名以及当前值和原来值，最后赋值给数据模型
					//int successCountS =  JudgeServiceStatus("successCount", UnEnableProgress.successCount, successCount);
					//int failedCountS = JudgeServiceStatus("failedCount", UnEnableProgress.failedCount, failedCount);
					//int notExistCountS = JudgeServiceStatus("notExistCount", UnEnableProgress.notExistCount, notExistCount);
					//int nameResolveFailedCountS = JudgeServiceStatus("nameResolveFailedCount", UnEnableProgress.nameResolveFailedCount, nameResolveFailedCount);

					ProgressCount?.Invoke(
							(successCount + failedCount + notExistCount + nameResolveFailedCount) / fullCount * 100,
							$"{(successCount + failedCount + notExistCount + nameResolveFailedCount) / fullCount * 100}%"
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

}
