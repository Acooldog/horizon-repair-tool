using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;
using test.src.Services.PublicFuc.Helpers;

namespace test.src.UI.Forms.FH4.MedicalForm
{
    public partial class Medical
    {
        #region 服务检查辅助类

        public class ServiceHelper
        {
            /// <summary>
            /// 通过显示名称获取服务名称
            /// </summary>
            public static string GetServiceNameByDisplayName(string displayName)
            {
                try
                {
                    // 方法1: 使用WMI查询
                    using (var searcher = new ManagementObjectSearcher(
                        $"SELECT Name FROM Win32_Service WHERE DisplayName = '{EscapeWMIString(displayName)}'"))
                    {
                        var services = searcher.Get();
                        foreach (ManagementObject service in services)
                        {
                            string? serviceName = service["Name"]?.ToString();
                            if (!string.IsNullOrEmpty(serviceName))
                            {
                                Logs.LogInfo($"WMI找到服务: 显示名称='{displayName}', 服务名称='{serviceName}'");
                                return serviceName;
                            }
                        }
                    }

                    // 方法2: 通过ServiceController查询
                    ServiceController[] servicesArray = ServiceController.GetServices();
                    foreach (var service in servicesArray)
                    {
                        if (service.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase))
                        {
                            Logs.LogInfo($"ServiceController找到服务: 显示名称='{displayName}', 服务名称='{service.ServiceName}'");
                            return service.ServiceName;
                        }
                    }

                    // 方法3: 通过sc命令查询
                    string serviceNameBySC = GetServiceNameBySCCommand(displayName);
                    if (!string.IsNullOrEmpty(serviceNameBySC))
                    {
                        return serviceNameBySC;
                    }

                    Logs.LogInfo($"未找到服务: {displayName}");
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    Logs.LogInfo($"获取服务名称失败: {displayName}, 错误: {ex.Message}");
                    return string.Empty;
                }
            }

            private static string GetServiceNameBySCCommand(string displayName)
            {
                try
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "sc",
                        Arguments = "queryex type= service state= all",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8
                    };

                    using (var process = Process.Start(processInfo))
                    {
                        if (process == null) return string.Empty;

                        process.WaitForExit(10000);
                        string output = process.StandardOutput.ReadToEnd();

                        // 解析输出，查找显示名称
                        var lines = output.Split('\n');
                        string currentServiceName = string.Empty;

                        foreach (var line in lines)
                        {
                            string trimmedLine = line.Trim();

                            if (trimmedLine.StartsWith("SERVICE_NAME:"))
                            {
                                currentServiceName = trimmedLine.Substring("SERVICE_NAME:".Length).Trim();
                            }
                            else if (trimmedLine.StartsWith("DISPLAY_NAME:") &&
                                     trimmedLine.Contains(displayName))
                            {
                                Logs.LogInfo($"SC命令找到服务: 显示名称='{displayName}', 服务名称='{currentServiceName}'");
                                return currentServiceName;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logs.LogInfo($"SC命令查询服务失败: {ex.Message}");
                }

                return string.Empty;
            }

            /// <summary>
            /// 获取服务启动类型
            /// </summary>
            public static string GetServiceStartType(string serviceName)
            {
                try
                {
                    // 优先使用SC命令
                    string startTypeBySC = GetServiceStartTypeBySC(serviceName);
                    if (!string.IsNullOrEmpty(startTypeBySC))
                    {
                        return startTypeBySC;
                    }

                    // 回退到WMI
                    using (var searcher = new ManagementObjectSearcher(
                        $"SELECT StartMode FROM Win32_Service WHERE Name = '{serviceName}'"))
                    {
                        foreach (ManagementObject service in searcher.Get())
                        {
                            string? startMode = service["StartMode"]?.ToString();
                            if (!string.IsNullOrEmpty(startMode))
                            {
                                return ConvertWMIStartTypeToStandard(startMode);
                            }
                        }
                    }

                    return "未知";
                }
                catch (Exception ex)
                {
                    Logs.LogInfo($"获取服务启动类型失败: {serviceName}, 错误: {ex.Message}");
                    return "未知";
                }
            }

            private static string GetServiceStartTypeBySC(string serviceName)
            {
                try
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "sc",
                        Arguments = $"qc \"{serviceName}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8
                    };

                    using (var process = Process.Start(processInfo))
                    {
                        if (process == null) return string.Empty;

                        process.WaitForExit(10000);
                        string output = process.StandardOutput.ReadToEnd();

                        // 解析启动类型
                        var lines = output.Split('\n');
                        foreach (var line in lines)
                        {
                            string trimmedLine = line.Trim();
                            if (trimmedLine.StartsWith("START_TYPE"))
                            {
                                if (trimmedLine.Contains("DISABLED") || trimmedLine.Contains("4"))
                                    return "禁用";
                                else if (trimmedLine.Contains("DEMAND_START") || trimmedLine.Contains("3"))
                                    return "手动";
                                else if (trimmedLine.Contains("AUTO_START") || trimmedLine.Contains("2"))
                                    return "自动";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logs.LogInfo($"SC命令查询启动类型失败: {ex.Message}");
                }

                return string.Empty;
            }

            /// <summary>
            /// 获取服务状态
            /// </summary>
            public static string GetServiceStatus(string serviceName)
            {
                try
                {
                    using (var sc = new ServiceController(serviceName))
                    {
                        switch (sc.Status)
                        {
                            case ServiceControllerStatus.Running:
                                return "正在运行";
                            case ServiceControllerStatus.Stopped:
                                return "已停止";
                            case ServiceControllerStatus.Paused:
                                return "已暂停";
                            case ServiceControllerStatus.StartPending:
                                return "启动中";
                            case ServiceControllerStatus.StopPending:
                                return "停止中";
                            default:
                                return "未知";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logs.LogInfo($"获取服务状态失败: {serviceName}, 错误: {ex.Message}");
                    return "未知";
                }
            }

            /// <summary>
            /// 检查服务是否存在
            /// </summary>
            public static bool CheckServiceExists(string serviceName)
            {
                try
                {
                    using (var sc = new ServiceController(serviceName))
                    {
                        // 如果能获取到服务名称，说明存在
                        string name = sc.ServiceName;
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }

            /// <summary>
            /// 获取服务显示名称
            /// </summary>
            public static string GetServiceDisplayName(string serviceName)
            {
                try
                {
                    using (var sc = new ServiceController(serviceName))
                    {
                        return sc.DisplayName;
                    }
                }
                catch
                {
                    return serviceName;
                }
            }

            private static string EscapeWMIString(string input)
            {
                if (string.IsNullOrEmpty(input)) return input;
                return input.Replace("'", "''").Replace("\\", "\\\\");
            }

            private static string ConvertWMIStartTypeToStandard(string wmiStartType)
            {
                return wmiStartType.ToLower() switch
                {
                    "automatic" => "自动",
                    "manual" => "手动",
                    "disabled" => "禁用",
                    _ => wmiStartType
                };
            }
        }

        public class ServiceInfo
        {
            public string DisplayName { get; set; } = string.Empty;
            public string ServiceName { get; set; } = string.Empty;
            public string ExpectedStartType { get; set; } = string.Empty;
            public string ActualStartType { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public bool IsCorrect { get; set; }
            public bool IsRunning { get; set; }
            public bool Exists { get; set; } = true;
            public string ErrorMessage { get; set; } = string.Empty;
        }

        #endregion
    }
}