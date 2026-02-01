using Newtonsoft.Json.Linq;
using test.src.Services.FH4.Managers.ServiceManagerAll;
using System.ComponentModel;
using System.Diagnostics;
using test.src.Services.FH4.Managers;
using test.src.Services.PublicFuc.Helpers;

namespace test.src.UI.Forms.FH4.CustomRepair
{
    partial class Fh4CustomRepair
    {   
        /// <summary>
        /// 禁用冲突的服务 solt专用
        /// </summary>
        public void DisableServiceWork(object? sender, DoWorkEventArgs e)
        {
            
            // 使用is检查并安全转换
            if (sender is not BackgroundWorker worker)
            {
                Debug.WriteLine("BackgroundWorker无效");
                return;
            }
            // 拼接json路径
            string jsonPath = pathEdit.GetApplicationRootDirectory() + "\\plugins\\plugins.json";
            // 读取json文件
            JObject ServiceNameList = JsonEdit.ReadJsonFile(jsonPath);
            dynamic ServiceName = ServiceNameList;
            // 获取数组并转换
            JArray? jArray = ServiceName.fix["disableClashName"] as JArray;
            if (jArray is not null)
            {
                List<string> serviceList = new List<string>();
                foreach (var item in jArray)
                {
                    serviceList.Add(item.ToString());
                }

                string[] serviceName = serviceList.ToArray();
                Logs.LogInfo($"{string.Join(", ", serviceName)}");
                // 调用重载更新进度条
                ServiceManager.DisableServicesAsync(serviceName, true, true,
                    (num, v) =>
                    {
                        // 检查取消请求
                        if (worker.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }

                        // 检查外部取消标志
                        if (isCancelling)
                        {
                            e.Cancel = true;
                            return;
                        }

                        worker.ReportProgress(
                             num,
                             v
                         );

                    });
            }
        }
    }
}
