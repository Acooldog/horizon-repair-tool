using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using test.src.Services.Helpers;

namespace test.src.Services.Managers.ServiceManagerAll
{
    // ServiceManager 杂类，诸如一些测试的类
    public partial class ServiceManager
    {
        /// <summary>
        /// 判断多少成功
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="oldNum"></param>
        /// <param name="newNum"></param>
        /// <returns></returns>
        private static int JudgeServiceStatus(string Name, int oldNum, int newNum)
        {
            if (newNum != oldNum)
            {
                Logs.LogInfo($"{Name} 原来: {oldNum}," +
                    $"现在: {newNum}," +
                    $"发生变化!");

                oldNum = newNum;

                return oldNum;
            }
            return oldNum;
        }
    }
}
