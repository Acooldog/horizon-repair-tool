using System.ComponentModel;
using test.src.Services.PublicFuc.Managers;

namespace test.src.UI.Forms.FH4.CustomRepair
{
    // 进度条事件监听
    partial class Fh4CustomRepair
    {
        private volatile bool isCancelling = false;  // 添加这行代码，声明取消标志变量
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
            if ((e.Argument as string) == "disable")
            {
                DisableServiceWork(sender, e);
            }
            
        }

        // 进度更新回调
        private void BackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            // 使用null条件运算符安全访问
            Pbar.Value = e.ProgressPercentage;

            // 使用?和??运算符安全访问
            //lblStatus!.Text = e.UserState?.ToString() ?? $"进度: {e.ProgressPercentage}%";
            this.Text = $"处理进度 - {e.ProgressPercentage}%";

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
                //lblStatus!.Text = "任务已取消";
                MessageBox.Show("任务已取消", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (e.Error != null)
            {
                //lblStatus!.Text = "发生错误";
                MessageBox.Show($"错误: {e.Error.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                Pbar!.Value = 100;
                //lblStatus!.Text = "任务完成！";
                MessageBox.Show("任务完成！", "成功",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                string softname = GetVersion.GetSoftName();
                // 如果获取的名称不为空
                if (!(string.IsNullOrEmpty(softname)))
                {
                    this.Text = softname;
                }
            }

            // 重置按钮状态
            //btnStart!.Text = "开始任务";
            this.disableService.Enabled = true;
            this.Pbar.Visible = false;
        }

        #endregion
    }
}
