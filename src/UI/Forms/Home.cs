using test.src.Services.Helpers;
using test.src.Services.Managers;
using test.src.Services.Model;
using System;
using System.Diagnostics;

namespace test
{
    public partial class Home : Form
    {
        public Home()
        {
            string SoftName = string.Empty;
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedDialog;  // 固定对话框，不可调整大小
            Form1_Load();

        }

        #region 窗体加载事件

        /// <summary>
        /// 加载窗体的函数
        /// </summary>
        private void Form1_Load()
        {

            new IconCon(this);

            try
            {
                VersionMaster.SetAndGetVersion((v1, v2) =>
                {
                    this.NowVersion.Text = $"v{v1}";
                    // 本地版本号小于远程版本号
                    if (v1 < v2)
                    {
                        this.NewVesion.Text = $"新版本：{v2}, 点我下载";
                    }
                    else if (v2 < v1)
                    {
                        this.NewVesion.Text = $"古 v{v2}";
                    }
                    else
                    {
                        this.NewVesion.Visible = false;
                    }
                });
            }
            catch (Exception)
            {
                this.NowVersion.Text = "无法连接服务器";
                this.NewVesion.Visible = false;
            }

            this.Text = "地平线修复工具";
        }

        #endregion 窗体加载事件

        #region 按钮点击事件

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

        #endregion

        private void NewVesion_Click(object sender, EventArgs e)
        {
            try
            {
                string url = "https://gitee.com/daoges_x/horizon-repair-tool/releases";

                // 使用ProcessStartInfo更安全
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法打开链接：{ex.Message}", "错误",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

    

