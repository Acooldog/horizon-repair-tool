using test.src.Services.PublicFuc.Helpers;
using test.src.UI.Forms.FH4.CustomRepair;
using test.src.UI.Forms.FH4.MedicalForm;

namespace test
{
    partial class Home
    {   
        /// <summary>
        /// 点击地平线4一键修复功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FH4Repair_Click(object sender, EventArgs e)
        {
            Logs.LogInfo("用户选择了地平线4修复功能");
            MessageBox.Show("此功能未开放，请等待更新", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 点击地平线4自定义修复功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FH4CustomRepair_Click(object sender, EventArgs e)
        {
            Logs.LogInfo("用户选择了地平线4自定义修复功能");
            Fh4CustomRepair fh4CustomRepair = new Fh4CustomRepair();
            fh4CustomRepair.Show();
            fh4CustomRepair.Activate();
            fh4CustomRepair.BringToFront();
            fh4CustomRepair.Focus();
        }

        // <summary>
        /// 点击地平线4Medical功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void medical_Click(object sender, EventArgs e)
        {
            Medical medical = new Medical();
            medical.Show();
            medical.Activate();
            medical.BringToFront();
            medical.Focus();
            Logs.LogInfo("用户选择了地平线4Medical功能");
        }

    }
}
