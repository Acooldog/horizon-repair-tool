using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test.src.Services.FH4.Managers
{
    public class pathEdit
    {
        /// <summary>
        /// 获取应用程序项目根目录（不是bin/Debug/）
        /// </summary>
        public static string GetApplicationRootDirectory()
        {
            // 方法1：尝试获取启动路径
            string basePath = Application.StartupPath;

            // 检查常见的bin目录结构
            string[] binPatterns =
            {
                "\\bin\\Debug\\",
                "\\bin\\Release\\",
                "\\bin\\x86\\Debug\\",
                "\\bin\\x64\\Debug\\",
                "\\bin\\Debug\\net8.0-windows\\",
                "\\bin\\Release\\net8.0-windows\\"
            };

            foreach (string pattern in binPatterns)
            {
                int index = basePath.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
                if (index > 0)
                {
                    // 截取bin之前的部分
                    return basePath.Substring(0, index);
                }
            }

            // 方法2：尝试查找.csproj文件
            try
            {
                string currentDir = basePath;
                for (int i = 0; i < 5; i++)  // 最多向上查找5级
                {
                    if (Directory.GetFiles(currentDir, "*.csproj").Length > 0)
                    {
                        return currentDir;
                    }

                    var parent = Directory.GetParent(currentDir);
                    if (parent == null) break;
                    currentDir = parent.FullName;
                }
            }
            catch
            {
                // 忽略错误
            }

            // 如果都找不到，返回当前路径
            return basePath;
        }
    }
}
