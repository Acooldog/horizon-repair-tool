using System;
using System.ComponentModel;
using System.Windows.Forms;
using test.src.Services.PublicFuc.Helpers;
using test.src.Services.PublicFuc.Animation;

namespace test.src.UI.Forms.FH4.MedicalForm
{
    public partial class Medical : Form
    {
        // 背景工作器
        private BackgroundWorker? diagnosisWorker;

        // 诊断结果
        private CombinedDiagnosticResult combinedResult = new CombinedDiagnosticResult();

        public Medical()
        {
            InitializeComponent();
            InitializeDiagnosisEngine();
            StartCompleteDiagnosis();
        }

        private void InitializeDiagnosisEngine()
        {
            // 绑定诊断完成事件
            this.OnDiagnosisReportGenerated += Medical_OnDiagnosisReportGenerated;

            // 绑定按钮事件
            //this.startBtn.Click += StartBtn_Click;
            //this.cancelBtn.Click += CancelBtn_Click;
        }

        private void Medical_OnDiagnosisReportGenerated(string reportPath, CombinedDiagnosticResult result)
        {
            // 在这里处理诊断报告
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    // 使用FadeIn淡入效果
                    this.FadeIn(300).Wait();

                    if (result.AllStepsSuccessful)
                    {
                        MessageBox.Show("网络诊断完成，所有检查通过！\n报告已保存到桌面。",
                            "诊断完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        DialogResult dialogResult = MessageBox.Show(
                            $"发现网络问题，是否立即修复？\n\n发现 {result.TotalIssues} 个问题",
                            "发现网络问题",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (dialogResult == DialogResult.Yes)
                        {
                            StartNetworkRepair();
                        }
                    }
                }));
            }
        }

        private void StartNetworkRepair()
        {
            Logs.LogInfo("开始网络修复流程...");
            // 修复逻辑将在后续实现
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {
            StartCompleteDiagnosis();
        }

        private void CancelBtn_Click_1(object sender, EventArgs e)
        {
            CancelDiagnosis();
        }

        public void StartCompleteDiagnosis()
        {
            if (diagnosisWorker != null && diagnosisWorker.IsBusy)
            {
                Logs.LogInfo("诊断已在运行中");
                return;
            }

            // 重置UI状态
            ResetUI();

            // 初始化背景工作器
            diagnosisWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            diagnosisWorker.DoWork += DiagnosisWorker_DoWork;
            diagnosisWorker.ProgressChanged += DiagnosisWorker_ProgressChanged;
            diagnosisWorker.RunWorkerCompleted += DiagnosisWorker_RunWorkerCompleted;

            // 启动诊断
            diagnosisWorker.RunWorkerAsync();
            Logs.LogInfo("启动完整网络诊断流程");
        }

        public void CancelDiagnosis()
        {
            if (diagnosisWorker != null && diagnosisWorker.IsBusy)
            {
                Logs.LogInfo("用户取消网络诊断");
                diagnosisWorker.CancelAsync();
            }
        }

        
    }
}