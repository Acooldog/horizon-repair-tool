using Newtonsoft.Json.Linq;
using test.src.Services.Helpers;
using test.src.Services.Managers.ServiceManagerAll;

namespace test.src.Services.Managers
{
    /// <summary>
    /// 修复程序
    /// </summary>
    public class fixSoft
    {
        public fixSoft()
        {

        }

        /// <summary>
        /// 禁用冲突的重载
        /// </summary>
        /// <param name="fucName">功能名，如disableClashName</param>
        /// <param name="onCompleted">回调函数</param>
        public static async Task ChangeWinService(string fucName = "", Action<bool, string>? onCompleted = null)
        {
            // 是否完成
            bool isSuccess = false;
            // 定义空字符串
            string result = string.Empty;

            try
            {

                // 如果用户没有设置功能名
                if (string.IsNullOrWhiteSpace(fucName))
                {
                    Logs.LogWarning("请选择功能名!!!");
                    return;
                }
                // 拼接json路径
                string jsonPath = pathEdit.GetApplicationRootDirectory() + "\\plugins\\plugins.json";
                // 读取json文件
                JObject ServiceNameList = JsonEdit.ReadJsonFile(jsonPath);
                dynamic ServiceName = ServiceNameList;
                // 获取数组并转换
                JArray? jArray = ServiceName.fix[fucName] as JArray;
                if (jArray is not null)
                {
                    List<string> serviceList = new List<string>();
                    foreach (var item in jArray)
                    {
                        serviceList.Add(item.ToString());
                    }

                    string[] serviceName = serviceList.ToArray();
                    Logs.LogInfo($"{fucName}: {string.Join(", ", serviceName)}");
                    result = await ServiceManager.DisableServicesAsync(serviceName, true);
                    // 判断输出结果，None为"服务名数组为空，没有服务需要禁用"
                    isSuccess = result != "None";
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
                result = $"处理 {fucName} 时发生异常: {ex.Message}";
                Logs.LogError(result, ex);
            }
            finally
            {
                // 调用委托
                onCompleted?.Invoke(isSuccess, result);
            }
        }

        /// <summary>
        /// 启用必要服务重载
        /// </summary>
        /// <param name="fucName">功能名，如disableClashName</param>
        /// <param name="onCompleted">回调函数</param>
        /// <param name="Auto">是否启用自动，false手动，true自动</param>
        /// <returns></returns>
        public static async Task ChangeWinService(string fucName = "",
            bool Auto = false, Action<bool, string>? onCompleted = null)
        {
            // 是否完成
            bool isSuccess = false;
            // 定义空字符串
            string result = string.Empty;

            try
            {

                // 如果用户没有设置功能名
                if (string.IsNullOrWhiteSpace(fucName))
                {
                    Logs.LogWarning("请选择功能名!!!");
                    return;
                }
                // 拼接json路径
                string jsonPath = pathEdit.GetApplicationRootDirectory() + "\\plugins\\plugins.json";
                // 读取json文件
                JObject ServiceNameList = JsonEdit.ReadJsonFile(jsonPath);
                dynamic ServiceName = ServiceNameList;
                // 获取数组并转换
                JArray? jArray = ServiceName.fix[fucName] as JArray;
                if (jArray is not null)
                {
                    List<string> serviceList = new List<string>();
                    foreach (var item in jArray)
                    {
                        serviceList.Add(item.ToString());
                    }

                    string[] serviceName = serviceList.ToArray();
                    Logs.LogInfo($"{fucName}: {string.Join(", ", serviceName)}");
                    result = await ServiceManager.EnableServicesAsync(serviceName, Auto, true);
                    // 判断输出结果，None为"服务名数组为空，没有服务需要禁用"
                    isSuccess = result != "None";
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
                result = $"处理 {fucName} 时发生异常: {ex.Message}";
                Logs.LogError(result, ex);
            }
            finally
            {
                // 调用委托
                onCompleted?.Invoke(isSuccess, result);
            }
        }
    }


}
