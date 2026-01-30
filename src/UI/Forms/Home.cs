using test.src.Services.Helpers;
using test.src.Services.Managers;

namespace test
{
    public partial class Home : Form
    {
        public Home()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedDialog;  // 固定对话框，不可调整大小
            Form1_Load();
            SetSoft();
        }

        private async void SetSoft()
        {
            await GetVersion.GetNowVersion((version, name, compeled) =>
            {
                if (compeled)
                {   
                    // 设置版本
                    this.NowVersion.Text = $"版本{version}";
                    // 根据文本内容扩充
                    this.NowVersion.AutoSize = true;
                }
                else
                {
                    this.NowVersion.Text = "版本获取失败";
                }
            });
        }

        /// <summary>
        /// 启用必要服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ClickEnableService(object sender, EventArgs e)
        {
            int completeCount = 0;
            this.EnableService.Enabled = false;

            // 启用手动服务
            await fixSoft.ChangeWinService("EnableNotAuto", false, (isSuccess, result) =>
            {
                if (isSuccess)
                {
                    MessageBox.Show(result, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    completeCount++;
                }
                else
                {
                    MessageBox.Show("错误，请确保你并没有篡改任何文件，并把logs文件夹发送给开发者！", "警告",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    completeCount++;
                }
            });

            // 启用自动服务
            await fixSoft.ChangeWinService("EnableAuto", true, (isSuccess, result) =>
            {
                if (isSuccess)
                {
                    MessageBox.Show(result, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    completeCount++;
                }
                else
                {
                    MessageBox.Show("错误，请确保你并没有篡改任何文件，并把logs文件夹发送给开发者！", "警告",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    completeCount++;
                }
            });

            if (completeCount == 2)
            {
                this.EnableService.Enabled = true;
            }
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
        private async void disableService_Click(object sender, EventArgs e)
        {
            this.disableService.Enabled = false;
            await fixSoft.ChangeWinService("disableClashName", (isSuccess, result) =>
            {
                if (isSuccess)
                {
                    MessageBox.Show(result, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.disableService.Enabled = true;
                }
                else
                {
                    MessageBox.Show("错误，请确保你并没有篡改任何文件，并把logs文件夹发送给开发者！", "警告",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.disableService.Enabled = true;
                }
            });
        }
    }
}

    

