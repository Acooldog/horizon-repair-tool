using test.tools;
using Newtonsoft.Json.Linq;

namespace test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Form1_Load();
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void click_test(object sender, EventArgs e)
        {
            Form2 dlog = new Form2();
            dlog.ShowDialog();
        }

        private void Form1_Load()
        {

            new IconCon(this);
        }


        /// <summary>
        /// 禁用与地平线冲突的服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void disableService_Click(object sender, EventArgs e)
        {   
            // 拼接json路径
            string jsonPath = pathEdit.GetApplicationRootDirectory() + "\\plugins\\plugins.json";
            // 读取json文件
            JObject ServiceNameList = JsonEdit.ReadJsonFile(jsonPath);
            dynamic ServiceName = ServiceNameList;
            // 获取数组并转换
            JArray? jArray = ServiceName.disableClashName as JArray;
            if (jArray != null)
            {
                List<string> serviceList = new List<string>();
                foreach (var item in jArray)
                {
                    serviceList.Add(item.ToString());
                }

                string[] serviceName = serviceList.ToArray();
                Logs.LogInfo($"冲突的服务列表：{string.Join(", ", serviceName)}");
                string result = ServiceManager.DisableServices(serviceName);
                // 判断输出结果，None为"服务名数组为空，没有服务需要禁用"
                if (result != "None") 
                {
                    MessageBox.Show(result, "完成", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                else
                {
                    MessageBox.Show("错误，请确保你并没有篡改任何文件，并把logs文件夹发送给开发者！", "警告",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
            }
        }
    }


}
