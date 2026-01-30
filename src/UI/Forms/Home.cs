using test.src.Services.Helpers;
using test.src.Services.Managers;
using test.src.Services.Model;
using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Security.Policy;
using test.src.Services.Internetwork;

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

            new IconCon(this);

            await GetVersion.GetNowVersion( (version, name, done) =>
            {   
                if (done)
                {
                    this.NowVersion.Text = version;
                }
                else
                {
                    this.NowVersion.Text = "错误";
                    MessageBox.Show("无法获取软件版本！请检查是否修改了软件配置文件", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } );

            this.Text = "地平线修复工具";
        }

        #endregion 窗体加载事件

        #region 点击事件

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

        // 是否点击过检查更新
        public bool checkUpdate = false;
        /// <summary>
        /// 点击获取新版本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewVesion_Click(object sender, EventArgs e)
        {   
            // 没有点击过检查更新
            if (!checkUpdate)
            {
                try
                {
                    VersionMaster.SetAndGetVersion((v1, v2) =>
                    {   
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
                    });
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
                btnStart!.Text = "取消任务";
                Pbar!.Value = 0;
                backgroundWorker.RunWorkerAsync();
            }
            else
            {
                backgroundWorker.CancelAsync();
            }
        }
        #endregion

        #region 进度条更新
        private BackgroundWorker backgroundWorker = null!;
        private void SetupControls()
        {
            // 确保Pbar被正确初始化
            if (Pbar == null)
            {
                Pbar = new ProgressBar();
                Controls.Add(Pbar);
            }

            Pbar.Minimum = 0;
            Pbar.Maximum = 100;
            Pbar.Value = 0;
        }

        private void SetupBackgroundWorker()
        {
            // 在方法中初始化，避免空引用
            backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            // 绑定事件
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
        }

        // 后台执行任务的方法
        private void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            // 使用is检查并安全转换
            if (sender is not BackgroundWorker worker)
            {
                Debug.WriteLine("BackgroundWorker无效");
                return;
            }

            ServiceP.test((num, v) =>
            {
                worker.ReportProgress(
                    num,
                    $"{v}%"
                );
            });

            // 模拟一个耗时任务
            //for (int i = 0; i <= 100; i++)
            //{
            //    // 检查是否请求取消
            //    if (worker.CancellationPending)
            //    {
            //        e.Cancel = true;
            //        return;
            //    }

            //    // 模拟工作
            //    Thread.Sleep(50);

            //    // 报告进度
            //    worker.ReportProgress(
            //        i,
            //        $"{i}%"
            //    );
            //}

        }

        // 进度更新回调
        private void BackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            // 使用null条件运算符安全访问
            Pbar.Value = e.ProgressPercentage;

            // 使用?和??运算符安全访问
            lblStatus!.Text = e.UserState?.ToString() ?? $"进度: {e.ProgressPercentage}%";
            //this.Text = $"进度示例 - {e.ProgressPercentage}%";

            // 可选：改变颜色
            if (Pbar != null)
            {
                Pbar.ForeColor = e.ProgressPercentage switch
                {
                    < 30 => System.Drawing.Color.Red,
                    < 70 => System.Drawing.Color.Orange,
                    _ => System.Drawing.Color.Green
                };
            }
        }

        // 任务完成回调
        private void BackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                lblStatus!.Text = "任务已取消";
                MessageBox.Show("任务已取消", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (e.Error != null)
            {
                lblStatus!.Text = "发生错误";
                MessageBox.Show($"错误: {e.Error.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                Pbar!.Value = 100;
                lblStatus!.Text = "任务完成！";
                MessageBox.Show("任务完成！", "成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // 重置按钮状态
            btnStart!.Text = "开始任务";
            btnStart.Enabled = true;
        }

        #endregion
    }
}

    

