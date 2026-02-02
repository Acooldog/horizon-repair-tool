using test.src.Services.PublicFuc.Animation;
using test.src.Services.PublicFuc.Helpers;

namespace test.src.UI.Forms.FH4.MedicalForm
{
    public partial class Medical : Form
    {
        public Medical()
        {
            InitializeComponent();
            InitializeNetworkDiagnosis();
            StartNetworkCheck(); // 初始化完成后启动网络检查
        }

        // 初始化方法
        private void InitializeNetworkDiagnosis()
        {
            // 绑定诊断完成事件
            this.OnDiagnosisReportGenerated += Medical_OnDiagnosisReportGenerated;
        }

        private void Medical_OnDiagnosisReportGenerated(string reportPath, NetworkDiagnosticResult result)
        {
            // 在这里处理诊断报告
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    // 使用FadeIn淡入效果
                    this.FadeIn(300).Wait();

                    if (result.IsNetworkHealthy)
                    {
                        MessageBox.Show("网络诊断完成，状态良好！\n报告已保存到桌面。",
                            "诊断完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        DialogResult dialogResult = MessageBox.Show(
                            $"发现网络问题，是否立即修复？\n\n问题列表:\n{string.Join("\n", result.Issues.Take(3))}...",
                            "发现网络问题",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (dialogResult == DialogResult.Yes)
                        {
                            // 触发修复流程
                            StartNetworkRepair();
                        }
                    }
                }));
            }
        }

        private void StartNetworkRepair()
        {
            // 这里可以添加网络修复逻辑
            Logs.LogInfo("开始网络修复流程...");
            StartNetworkCheck();
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            CancelNetworkCheck();
        }
    }
}
