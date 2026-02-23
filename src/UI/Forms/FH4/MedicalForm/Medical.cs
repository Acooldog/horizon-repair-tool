using System;
using System.ComponentModel;
using System.Windows.Forms;
using test.src.Services.PublicFuc.Helpers;
using test.src.Services.PublicFuc.Animation;
using test.src.UI.Helpers;
using test.src.UI.Controls;
using System.Drawing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace test.src.UI.Forms.FH4.MedicalForm
{
    public partial class Medical : Form
    {
        // 背景工作器
        private BackgroundWorker? diagnosisWorker;

        // 诊断结果
        private CombinedDiagnosticResult combinedResult = new CombinedDiagnosticResult();

        // 是否能够重新检测
        private bool canReDiagnose = false;

        private string Wintitle = "生成诊断报告";

        // Report UI
        private Panel pnlReport = null!;
        private FlowLayoutPanel flowPanelResults = null!;
        private Button btnOneClickRepair = null!;

        public Medical()
        {
            InitializeComponent();
            // InitializeModernUI(); // 移除UI修改
            InitializeReportUI();
            InitializeDiagnosisEngine();
            // StartCompleteDiagnosis(); // 移至Shown事件

            this.Text = $"{Wintitle}";

            // 设置窗体居中
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Shown += Medical_Shown;
        }

        private void Medical_Shown(object? sender, EventArgs e)
        {
            StartCompleteDiagnosis();
        }

        private void InitializeModernUI()
        {
            UIStyleHelper.ApplyModernStyle(this);

            // 调整ProgressBar样式（如果可能，或者保持原样但调整颜色）
            // WinForms ProgressBar 颜色很难改，除非重绘。这里暂时保持原样或隐藏背景

            // 将标签包装在卡片Panel中
            WrapLabelInCard(step1Label, 0);
            WrapLabelInCard(step2Label, 1);
            WrapLabelInCard(step3Label, 2);
            WrapLabelInCard(step4Label, 3);
            WrapLabelInCard(step5Label, 4);
            WrapLabelInCard(step6Label, 5);

            // 调整TISHI标签样式
            TISHI.ForeColor = UIStyleHelper.AccentColor;
            TISHI.Font = new Font(TISHI.Font, FontStyle.Bold);
        }

        private void WrapLabelInCard(Label label, int row)
        {
            // 创建卡片Panel
            var panel = UIStyleHelper.CreateCardPanel();
            panel.Dock = DockStyle.Fill;
            panel.Margin = new Padding(3, 3, 3, 3); // 卡片间距

            // 从TableLayoutPanel中移除Label
            this.tableLayoutPanel1.Controls.Remove(label);

            // 配置Label样式
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.AutoSize = false;
            label.BackColor = Color.Transparent; // 确保透明

            // 将Label添加到Panel
            panel.Controls.Add(label);

            // 将Panel添加到TableLayoutPanel的原位置
            this.tableLayoutPanel1.Controls.Add(panel, 0, row);
        }

        private void InitializeReportUI()
        {
            // Create pnlReport covering the same area as tableLayoutPanel1
            pnlReport = new Panel();
            pnlReport.Dock = DockStyle.Fill; // 覆盖整个窗体
            pnlReport.Visible = false;
            pnlReport.BackColor = Color.White; // 确保有背景色，防止透明导致看起来“白了”或透视
            // 或者使用 Control 颜色
            pnlReport.BackColor = SystemColors.Control;

            // Create One Click Repair Button at bottom
            btnOneClickRepair = new Button();
            btnOneClickRepair.Text = "一键修复所有问题";
            btnOneClickRepair.Dock = DockStyle.Bottom;
            btnOneClickRepair.Height = 40;
            btnOneClickRepair.FlatStyle = FlatStyle.Flat;
            btnOneClickRepair.FlatAppearance.BorderSize = 0;
            btnOneClickRepair.BackColor = UIStyleHelper.AccentColor;
            btnOneClickRepair.ForeColor = Color.White;
            btnOneClickRepair.Cursor = Cursors.Hand;
            btnOneClickRepair.Click += BtnOneClickRepair_Click;

            // Create FlowLayoutPanel
            flowPanelResults = new FlowLayoutPanel();
            flowPanelResults.Dock = DockStyle.Fill;
            flowPanelResults.AutoScroll = true; // 启用滚动条
            flowPanelResults.FlowDirection = FlowDirection.TopDown;
            flowPanelResults.WrapContents = false; // 垂直堆叠，不换行
            flowPanelResults.BackColor = SystemColors.Control;
            flowPanelResults.Padding = new Padding(10);

            // 重要：处理 resize 以调整子控件宽度
            flowPanelResults.SizeChanged += (s, e) =>
            {
                foreach (Control ctrl in flowPanelResults.Controls)
                {
                    // 减去滚动条宽度和padding
                    ctrl.Width = flowPanelResults.ClientSize.Width - 25;
                }
            };

            pnlReport.Controls.Add(flowPanelResults);
            pnlReport.Controls.Add(btnOneClickRepair);

            this.Controls.Add(pnlReport);
            pnlReport.BringToFront(); // Initially hidden
        }

        private void InitializeDiagnosisEngine()
        {
            // 绑定诊断完成事件
            this.OnDiagnosisReportGenerated += Medical_OnDiagnosisReportGenerated;
        }

        private async void Medical_OnDiagnosisReportGenerated(string reportPath, CombinedDiagnosticResult result)
        {
            // 在这里处理诊断报告
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => Medical_OnDiagnosisReportGenerated(reportPath, result)));
                return;
            }

            // 使用FadeIn淡入效果
            // this.FadeIn(300).Wait(); // .Wait() 在 UI 线程会导致死锁，改为 async/await
            await Task.Delay(300); // 简单的延迟，或者使用正确的异步 FadeIn 如果支持

            // 确保不阻塞 UI
            if (result.AllStepsSuccessful)
            {
                ShowReportUI(result);
            }
            else
            {
                // Switch to Report View
                ShowReportUI(result);
            }
        }

        private void ShowReportUI(CombinedDiagnosticResult result)
        {
            tableLayoutPanel1.Visible = false;
            pnlReport.Visible = true;
            pnlReport.BringToFront();

            // Ensure pnlReport bounds match tableLayoutPanel1 if not docked
            pnlReport.Bounds = tableLayoutPanel1.Bounds;

            PopulateReportList(result);
        }

        private void PopulateReportList(CombinedDiagnosticResult result)
        {
            flowPanelResults.Controls.Clear();
            flowPanelResults.SuspendLayout();

            if (result.AllStepsSuccessful)
            {
                Label lblSuccess = new Label();
                lblSuccess.Text = "🎉 恭喜！未发现网络问题。";
                lblSuccess.Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Bold);
                lblSuccess.ForeColor = UIStyleHelper.SuccessColor;
                lblSuccess.AutoSize = true;
                lblSuccess.Padding = new Padding(20);
                flowPanelResults.Controls.Add(lblSuccess);
                btnOneClickRepair.Visible = false;
            }
            else
            {
                btnOneClickRepair.Visible = true;
                foreach (var issue in result.AllIssues)
                {
                    var itemControl = new DiagnosticItemControl(
                        issue.Description,
                        issue.Details,
                        issue.RepairAction
                    );
                    itemControl.Width = flowPanelResults.ClientSize.Width - 30; // Adjust width
                    flowPanelResults.Controls.Add(itemControl);
                }
            }

            flowPanelResults.ResumeLayout();
        }

        private async void BtnOneClickRepair_Click(object? sender, EventArgs e)
        {
            if (btnOneClickRepair == null) return;
            btnOneClickRepair.Enabled = false;
            btnOneClickRepair.Text = "正在修复所有问题...";

            foreach (Control ctrl in flowPanelResults.Controls)
            {
                if (ctrl is DiagnosticItemControl item)
                {
                    await item.StartRepair();
                }
            }

            btnOneClickRepair.Text = "修复完成";
            MessageBox.Show("所有修复操作已执行完毕，建议重新检测以验证。", "修复完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {
            StartCompleteDiagnosis();
        }

        private void CancelBtn_Click_1(object sender, EventArgs e)
        {
            Logs.LogInfo($"===== canReDiagnose : {canReDiagnose} =====");
            if (canReDiagnose)
            {
                DialogResult dialogResult = MessageBox.Show(
                    "是否重新开始诊断？",
                    "重新开始诊断",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    this.CancelBtn.Text = "取消";
                    canReDiagnose = false;

                    // Switch back to diagnosis view
                    pnlReport.Visible = false;
                    tableLayoutPanel1.Visible = true;

                    StartCompleteDiagnosis();
                }
            }
            else
            {
                DialogResult dialogResult = MessageBox.Show(
                    "是否取消诊断？",
                    "取消诊断",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    CancelDiagnosis();
                    this.Close();
                }
            }
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
