using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using test.src.Services.Helpers;
using test.src.Services.Model;

namespace test.src.Services.Managers
{   
    /// <summary>
    /// 获取当前版本信息
    /// </summary>
    public class GetVersion
    {
        public GetVersion()
        {
            
        }

        // 定义返回模型
        public class VersionInfo
        {
            public string Version { get; set; } = "";
            public string SoftName { get; set; } = "";
            public bool Done { get; set; } = false;
        }

        public static async Task GetNowVersion(
            Action<string,string,bool>? onCompeled = null)
        {
            await Task.Run(() =>
            {
                VersionInfo result = new VersionInfo();
                try
                {
                    string RPath = "plugins/plugins.json";
                    string fullPath = Path.Combine(pathEdit.GetApplicationRootDirectory(), RPath);
                    dynamic json = JsonEdit.ReadJsonFile(fullPath);

                    Logs.LogInfo($"获取成功，版本号: {json.Application.Version}," +
                        $"软件名: {json.Application.Name}");
                    // 返回结果
                    result.Version = json.Application.Version;
                    result.SoftName = json.Application.Name;
                    result.Done = true;

                }
                catch (Exception ex)
                {   
                    Logs.LogError($"获取版本信息失败: {ex.Message}", ex);
                    result.Done = false;
                    onCompeled?.Invoke(result.Version, result.SoftName, result.Done);
                }
                finally
                {
                    onCompeled?.Invoke(result.Version, result.SoftName, result.Done);  
                }

            });
        }

        /// <summary>
        /// 获取版本信息
        /// </summary>
        /// <param name="onCompeled"></param>
        public static async void SetSoft(
            Action<string, string>? onCompeled = null
            )
        {
            WinProperty winProperty = new WinProperty();
            await GetVersion.GetNowVersion((version, name, compeled) =>
            {
                onCompeled?.Invoke(winProperty.Name, version);
            });
        }
    }

    /// <summary>
    /// 获取仓库版本号
    /// </summary>
    public static class GiteeVersionFetcher
    {
        private const string GITEE_API_URL = "https://gitee.com/api/v5/repos/{owner}/{repo}/releases/latest";

        /// <summary>
        /// 改进版本：更好的错误处理
        /// </summary>
        /// <param name="owner">仓库拥有者的用户名</param>
        /// <param name="repo">仓库名</param>
        /// <returns>包含版本号和成功状态的元组</returns>
        public static async Task<(string? Version, bool Success, string Message)> GetLatestVersionAsync(
            string owner = "daoges_x",
            string repo = "horizon-repair-tool")
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(10);
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");

                    // 构建URL
                    string url = GITEE_API_URL
                        .Replace("{owner}", owner)
                        .Replace("{repo}", repo);

                    Logs.LogInfo($"请求URL: {url}");

                    // 使用 GetAsync 而不是 GetStringAsync，以便检查状态码
                    var response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var release = JObject.Parse(json);
                        string? version = release["tag_name"]?.ToString();

                        Logs.LogInfo($"获取成功: {version}");
                        return (version, true, "成功");
                    }
                    else
                    {
                        // 检查具体的错误状态码
                        string responseContent = await response.Content.ReadAsStringAsync();
                        string errorMessage = $"HTTP {response.StatusCode}";

                        if (!string.IsNullOrEmpty(responseContent))
                        {
                            try
                            {
                                var errorJson = JObject.Parse(responseContent);
                                errorMessage = errorJson["message"]?.ToString() ?? errorMessage;
                            }
                            catch
                            {
                                errorMessage = responseContent;
                            }
                        }

                        Logs.LogError($"API请求失败: {errorMessage}");
                        return (null, false, $"API错误: {errorMessage}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Logs.LogError($"网络请求异常: {ex.Message}", ex);
                return (null, false, $"网络错误: {ex.Message}");
            }
            catch (Exception ex)
            {
                Logs.LogError($"获取版本失败: {ex.Message}", ex);
                return (null, false, $"异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查仓库是否存在
        /// </summary>
        public static async Task<bool> CheckRepositoryExists(string owner = "daoges_x", string repo = "horizon-repair-tool")
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(5);

                    string url = $"https://gitee.com/api/v5/repos/{owner}/{repo}";
                    Logs.LogInfo($"检查仓库是否存在: {url}");
                    var response = await httpClient.GetAsync(url);

                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }
    }

    public class VersionEdit
    {
        /// <summary>
        /// 移除版本号中的 "v" 前缀
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static string RemoveVPrefix(string version)
        {
            if (version?.StartsWith("v", StringComparison.OrdinalIgnoreCase) == true)
            {
                return version.Substring(1);
            }
            return version ?? string.Empty;
        }
    }

    /// <summary>
    /// 使用版本者
    /// </summary>
    public class VersionMaster
    {   
        
        /// <summary>
        /// 设置并获取版本
        /// </summary>
        /// <param name="onCompeled">回调函数，获取本地版本和远程版本</param>
        public static void SetAndGetVersion(Action<Version, Version>? onCompeled = null)
        {   
            // 初始化数据模型
            VersionL versionL = new();
            try
            {
                GetVersion.SetSoft(async (name, version) =>
                {
                    Logs.LogInfo("获取版本完成");
                    ValueTuple<string?, bool, string> result = await GiteeVersionFetcher.GetLatestVersionAsync();
                    // 获取版本成功
                    if (result.Item2)
                    {
                        // 对比版本号
                        // 本地版本号去除v
                        string version1 = VersionEdit.RemoveVPrefix(version);
                        // 远程版本号去除v
                        string version2 = VersionEdit.RemoveVPrefix(result.Item1 ?? version);

                        versionL.v1 = new Version(version1);
                        versionL.v2 = new Version(version2);

                        onCompeled?.Invoke(versionL.v1, versionL.v2);

                    }
                    // 如果获取失败
                    else
                    {
                        onCompeled?.Invoke(new Version(0, 0, 0), new Version(0, 0, 0));
                    }
                });
            }
            catch (Exception ex)
            {
                Logs.LogError($"获取版本失败: {ex.Message}", ex);
                onCompeled?.Invoke(versionL.v1, versionL.v2);
            }
        }
    }
}
