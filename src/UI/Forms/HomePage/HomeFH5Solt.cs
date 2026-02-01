using test.src.Services.PublicFuc.Helpers;

namespace test
{
    partial class Home
    {   
        /// <summary>
        /// 点击FH5一键修复
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FH5Repair_Click(object sender, EventArgs e)
        {
            Logs.LogInfo("用户点击FH5一键修复");
            MessageBox.Show("此功能未开放，请等待更新", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 点击FH5自定义修复
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FH5CustomRepair_Click(object sender, EventArgs e)
        {
            Logs.LogInfo("用户点击FH5自定义修复");
            MessageBox.Show("此功能未开放，请等待更新", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
