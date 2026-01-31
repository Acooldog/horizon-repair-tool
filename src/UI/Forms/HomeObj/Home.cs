using test.src.Services.Helpers;
using test.src.Services.Managers;
using test.src.Services.Managers.ServiceManagerAll;
using test.src.Services.Model;
using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Security.Policy;
using test.src.Services.Internetwork;
using Newtonsoft.Json.Linq;

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

            SetupControls();
            SetupBackgroundWorker();

        }

        #region 窗体加载事件

        /// <summary>
        /// 加载窗体的函数
        /// </summary>
        private async void Form1_Load()
        {   
            // 自动调整大小
            this.AutoSize = true;

            new IconCon(this);

            string? result = await GetVersion.GetNowVersion();
            if (!string.IsNullOrEmpty(result))
            {
                this.NowVersion.Text = result;
            }
            else
            {
                this.NowVersion.Text = "错误";
                MessageBox.Show("无法获取版本信息！请检查是否修改了软件配置文件", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            this.Text = "地平线修复工具";
        }

        #endregion 窗体加载事件


        

        // 是否点击过检查更新
        public bool checkUpdate = false;
        /// <summary>
        /// 点击获取新版本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void NewVesion_Click(object sender, EventArgs e)
        {   
            // 没有点击过检查更新
            if (!checkUpdate)
            {
                try
                {
                    (Version v1, Version v2) =  await VersionMaster.SetAndGetVersion();
                    // 如果版本获取失败
                    if (v1 == new Version(0, 0, 0))
                    {
                        if (DialogResult.Yes == MessageBox.Show("检查更新失败，可能是API请求频繁，可以手动去github或者gitee看！" +
                            "是否要打开gitee？",
                            "获取版本失败",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Error))
                        {   
                            // 打开网页
                            URLEdit.JumpUrl("https://gitee.com/daoges_x/horizon-repair-tool/releases");
                        }
                    }
                    // 本地版本号小于远程版本号
                    if (v1 < v2)
                    {
                        this.NewVesion.Text = $"新版本：{v2}, 点我下载";
                        checkUpdate = true;
                    }
                    else if (v2 < v1)
                    {
                        this.NewVesion.Text = $"古 v{v2}";
                    }
                    else if (v1 == v2)
                    {
                        this.NewVesion.Text = "当前为最新版本";
                        this.NewVesion.Enabled = false;
                    }
                }
                catch (Exception)
                {
                    this.NowVersion.Text = "无法连接服务器";
                    this.NewVesion.Visible = false;
                    checkUpdate = false;
                }
            }
            // 点击过检查更新
            else
            {
                URLEdit.JumpUrl("https://gitee.com/daoges_x/horizon-repair-tool/releases");
            }
        }

        private void btnStart_Click_1(object sender, EventArgs e)
        {
            // 添加null检查
            if (backgroundWorker == null)
            {
                MessageBox.Show("BackgroundWorker未初始化");
                return;
            }

            if (!backgroundWorker.IsBusy)
            {
                
                Pbar!.Value = 0;
                backgroundWorker.RunWorkerAsync("disable");
                // 设置取消标志为false
                isCancelling = false;
            }
            else
            {   
                // 取消任务
                isCancelling = true;
                backgroundWorker.CancelAsync();
            }
        }
        

        
    }
}

    

