using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test.src.Services.Model
{   
    /// <summary>
    /// Windows系统属性
    /// </summary>
    public class WinProperty
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// 定义VersionMaster的数据模型
    /// </summary>
    public class VersionL
    {
        public Version v1 { get; set; } = new Version(1, 0, 0);
        public Version v2 { get; set; } = new Version(1, 0, 0);
    }

}
