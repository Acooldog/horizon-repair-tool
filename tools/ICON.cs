using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using test.tools;

namespace test.tools
{
    /// <summary>
    /// FORM的icon的操作
    /// </summary>
    public class IconCon
    {
        public IconCon(Form that)
        {
            try
            {
                // 获取icon
                dynamic ICON = GetIcon();
                string icon_name = ICON.icon;
                // 获取icon路径 将自身路径与icon路径进行拼接
                string icon_path = Path.Combine(pathEdit.GetApplicationRootDirectory(), icon_name);
                // 使用icon
                IconCon.UseIcon(icon_path, that);
            }
            catch (Exception ex)
            {
                Logs.LogError("查找icon出现错误: ", ex);
            }
            finally
            {
                Logs.LogInfo("icon加载完成");
            }
        }
        /// <summary>
        /// 获取图标
        /// </summary>
        /// <returns>Jobject对象
        /// 需要进行dynamicd设置为动态类型才可调用</returns>
        public static JObject GetIcon()
        {
            // 相对路径读取
            string relativePath = "plugins/plugins.json";
            Logs.LogInfo($"当前程序运行路径: {pathEdit.GetApplicationRootDirectory()}");
            string fullPath = Path.Combine(pathEdit.GetApplicationRootDirectory(), relativePath);
            return JsonEdit.ReadJsonFile(fullPath);
        }

        /// <summary>
        /// 判断到底是用json配置的图标还是系统默认图标
        /// </summary>
        /// <param name="icon_path">图标路径</param>
        /// <param name="that">Form父类</param>
        public static void UseIcon(string icon_path, Form that)
        {
            // 文件存在用json配置
            // 不存在用系统默认图标
            if (File.Exists(icon_path))
            {
                that.Icon = new Icon(icon_path);
            }
            else
            {
                that.Icon = SystemIcons.Application;
                throw new Exception($"没有在 {icon_path} 找到相关icon");
            }
        }
    }
}
