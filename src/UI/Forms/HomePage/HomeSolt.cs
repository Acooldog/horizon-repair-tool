using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using test.src.Services.FH4.Internetwork;
using test.src.Services.PublicFuc.Managers;

namespace test
{
    partial class Home
    {
        // 是否点击过检查更新
        public bool checkUpdate = false;
        /// <summary>
        /// 点击获取新版本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void NewVersion_Click(object sender, EventArgs e)
        {
            // 没有点击过检查更新
            if (!checkUpdate)
            {
                try
                {
                    (Version v1, Version v2) = await VersionMaster.SetAndGetVersion();
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
                        this.NewVersion.Text = $"新版本：{v2}, 点我下载";
                        checkUpdate = true;
                    }
                    else if (v2 < v1)
                    {
                        this.NewVersion.Text = $"古 v{v2}";
                    }
                    else if (v1 == v2)
                    {
                        this.NewVersion.Text = "当前为最新版本";
                        this.NewVersion.Enabled = false;
                    }
                }
                catch (Exception)
                {
                    this.NowVersion.Text = "无法连接服务器";
                    this.NewVersion.Visible = false;
                    checkUpdate = false;
                }
            }
            // 点击过检查更新
            else
            {
                URLEdit.JumpUrl("https://gitee.com/daoges_x/horizon-repair-tool/releases");
            }
        }
    }
}
