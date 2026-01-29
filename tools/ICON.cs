using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
                // 获取icon路径
                string icon_path = ICON.icon;
                // 使用icon
                IconCon.UseIcon(icon_path, that);
            }
            catch (Exception ex)
            {
                Logs.LogError("ICON.cs", ex);
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
            string fullPath = Path.Combine(Application.StartupPath, relativePath);
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
            }
        }
    }
}
