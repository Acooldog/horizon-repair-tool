using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test.src.Services.FH4.Managers.ServiceManagerAll
{   
    // ServiceManager数据模型
    public partial class ServiceManager
    {
        /// <summary>
        /// 服务详细信息类
        /// </summary>
        public class ServiceDetailInfo
        {
            public string ServiceName { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string? StartType { get; set; }
            public string? Description { get; set; }
            public bool CanStop { get; set; }
            public bool CanPauseAndContinue { get; set; }
        }

    }
}

