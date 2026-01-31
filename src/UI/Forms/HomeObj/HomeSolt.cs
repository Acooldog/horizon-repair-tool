using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using test.src.Services.Managers;

namespace test
{   
    // Home的点击事件
    partial class Home
    {
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
        private void disableService_Click(object sender, EventArgs e)
        {
            this.disableService.Enabled = false;
            this.Pbar.Visible = true;
            //await fixSoft.ChangeWinService("disableClashName", (isSuccess, result) =>
            //{
            //    if (isSuccess)
            //    {
            //        MessageBox.Show(result, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        this.disableService.Enabled = true;
            //    }
            //    else
            //    {
            //        MessageBox.Show("错误，请确保你并没有篡改任何文件，并把logs文件夹发送给开发者！", "警告",
            //                MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        this.disableService.Enabled = true;
            //    }
            //});
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

        #endregion 点击事件

    }
}
