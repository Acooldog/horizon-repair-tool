using test.src.Services.Helpers;

namespace test.src.Services.Managers
{
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
    }
}
