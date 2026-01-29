using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using test.tools;

namespace test.tools
{
    public class JsonEdit
    {   
        // 1. 读取JSON文件
        public static JObject ReadJsonFile(string filePath)
        {   
            if (!File.Exists(filePath))
            {   
                Logs.LogWarning($"文件不存在: {filePath}");
                // 如果文件不存在，创建空JSON对象
                return new JObject();
            }

            string jsonContent = File.ReadAllText(filePath);
            return JObject.Parse(jsonContent);
        }

        // 2. 修改JSON值
        public static void UpdateJsonValue(string filePath, string key, object value)
        {
            JObject jsonObj = ReadJsonFile(filePath);

            // 修改或添加值
            jsonObj[key] = JToken.FromObject(value);

            // 写回文件
            WriteJsonFile(filePath, jsonObj);
        }

        // 3. 保存JSON文件
        public static void WriteJsonFile(string filePath, JObject jsonObject)
        {
            // 格式化JSON（缩进，可读）
            string formattedJson = jsonObject.ToString(Formatting.Indented);
            File.WriteAllText(filePath, formattedJson);
        }
    }
}
