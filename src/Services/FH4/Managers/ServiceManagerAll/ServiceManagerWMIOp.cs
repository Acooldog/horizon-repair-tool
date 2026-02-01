using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using test.src.Services.PublicFuc.Helpers;
using EnumerationOptions = System.Management.EnumerationOptions;

namespace test.src.Services.FH4.Managers.ServiceManagerAll
{
    // WMI操作
    public partial class ServiceManager
    {
        #region WMI操作

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

        #endregion WMI操作

        #region WMI查询操作
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

        #endregion WMI查询操作

        #region WMI辅助方法
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

        #endregion WMI辅助方法
    }
}
