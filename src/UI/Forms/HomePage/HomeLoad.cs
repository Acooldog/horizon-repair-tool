using test.src.Services.FH4.Managers;
using test.src.Services.PublicFuc.Managers;

namespace test
{
    // Home的Load分部类
    partial class Home
    {
        private void HomeLoad()
        {   
            // 获取图标
            new IconCon(this);
            // 设置窗口启动位置为屏幕中央
            this.StartPosition = FormStartPosition.CenterScreen;

            // 获取当前版本
            GetNowVersion();

            this.Text = $"地平线修复工具 - {WinTitle}";
        }

        private async void GetNowVersion()
        {
            // 获取当前版本
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
        }

    }
}
