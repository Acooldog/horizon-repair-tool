using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using test.src.UI.Controls;
using test.src.UI.Helpers;
using static test.src.UI.Forms.FH4.MedicalForm.Medical;

namespace test.src.UI.Forms.FH4.MedicalForm
{
    public class MedicalReportForm : Form
    {
        private CombinedDiagnosticResult _result;
        private string _reportPath;

        // UI Controls - Raw View
        private Panel pnlRawView = null!;
        private Label lblErrorCount = null!;
        // private Button btnParse = null!; // Removed as per request
        private TextBox txtRawReport = null!;

        // UI Controls - Parsed View
        private Panel pnlParsedView = null!;
        private FlowLayoutPanel flowPanelErrors = null!;

        // UI Controls - Common
        private Button btnRepairAll = null!;

        public MedicalReportForm(string reportPath, CombinedDiagnosticResult result)
        {
            _reportPath = reportPath;
            _result = result;

            InitializeComponent();
            InitializeData();
        }

        private void InitializeComponent()
        {
            this.Text = "诊断最终窗口";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White; // 确保背景干净

            // 1. Common Bottom Button (Repair All)
            btnRepairAll = new Button();
            btnRepairAll.Text = "修复所有错误";
            btnRepairAll.Dock = DockStyle.Bottom;
            btnRepairAll.Height = 50;
            btnRepairAll.FlatStyle = FlatStyle.Flat;
            btnRepairAll.FlatAppearance.BorderSize = 2; // 显式边框
            btnRepairAll.FlatAppearance.BorderColor = Color.Black;
            btnRepairAll.BackColor = Color.White;
            btnRepairAll.ForeColor = Color.Black;
            btnRepairAll.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);
            btnRepairAll.Cursor = Cursors.Hand;
            btnRepairAll.Click += BtnRepairAll_Click;
            btnRepairAll.Padding = new Padding(10);

            // 2. Raw View Panel
            pnlRawView = new Panel();
            pnlRawView.Dock = DockStyle.Fill;
            pnlRawView.Padding = new Padding(20);

            lblErrorCount = new Label();
            lblErrorCount.Dock = DockStyle.Top;
            lblErrorCount.Height = 40;
            lblErrorCount.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);
            lblErrorCount.TextAlign = ContentAlignment.MiddleLeft;
            lblErrorCount.ForeColor = Color.Black;

            // btnParse Removed
            /*
            btnParse = new Button();
            btnParse.Text = "解析所有错误?";
            btnParse.Dock = DockStyle.Top;
            btnParse.Height = 40;
            btnParse.FlatStyle = FlatStyle.Flat;
            btnParse.FlatAppearance.BorderSize = 2;
            btnParse.FlatAppearance.BorderColor = Color.Black;
            btnParse.BackColor = Color.White;
            btnParse.ForeColor = Color.Black;
            btnParse.Font = new Font("Microsoft YaHei UI", 10F);
            btnParse.Cursor = Cursors.Hand;
            btnParse.Margin = new Padding(0, 10, 0, 10);
            btnParse.Click += BtnParse_Click;
            */

            txtRawReport = new TextBox();
            txtRawReport.Multiline = true;
            txtRawReport.ReadOnly = true;
            txtRawReport.ScrollBars = ScrollBars.Vertical;
            txtRawReport.Dock = DockStyle.Fill;
            txtRawReport.BackColor = Color.White; // 背景无（白色）
            txtRawReport.ForeColor = Color.Black; // 字体黑色
            txtRawReport.Font = new Font("Consolas", 10F);
            txtRawReport.BorderStyle = BorderStyle.FixedSingle;

            // Layout Raw View
            // Note: Controls are added in reverse dock order
            pnlRawView.Controls.Add(txtRawReport);
            pnlRawView.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 10 }); // Spacer
            // pnlRawView.Controls.Add(btnParse); // Removed
            pnlRawView.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 10 }); // Spacer
            pnlRawView.Controls.Add(lblErrorCount);

            // 3. Parsed View Panel (Initially Hidden)
            pnlParsedView = new Panel();
            pnlParsedView.Dock = DockStyle.Fill;
            pnlParsedView.Padding = new Padding(20);
            pnlParsedView.Visible = false;

            flowPanelErrors = new FlowLayoutPanel();
            flowPanelErrors.Dock = DockStyle.Fill;
            flowPanelErrors.AutoScroll = true;
            flowPanelErrors.FlowDirection = FlowDirection.TopDown;
            flowPanelErrors.WrapContents = false;
            flowPanelErrors.BackColor = Color.White;

            // Handle Resize for FlowPanel items
            flowPanelErrors.SizeChanged += (s, e) =>
            {
                foreach (Control ctrl in flowPanelErrors.Controls)
                {
                    ctrl.Width = flowPanelErrors.ClientSize.Width - 25;
                }
            };

            pnlParsedView.Controls.Add(flowPanelErrors);

            // Add main panels
            this.Controls.Add(pnlRawView);
            this.Controls.Add(pnlParsedView);
            this.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 10 }); // Spacer above bottom button
            this.Controls.Add(btnRepairAll);
        }

        private void InitializeData()
        {
            // Set Label
            if (_result.AllStepsSuccessful)
            {
                lblErrorCount.Text = "检测到 0 项错误";
                lblErrorCount.ForeColor = Color.Green;
                btnRepairAll.Enabled = false;
                btnRepairAll.Text = "无需修复";
            }
            else
            {
                lblErrorCount.Text = $"检测到 {_result.TotalIssues} 项错误";
                lblErrorCount.ForeColor = Color.Red;
            }

            // Load Text Content
            if (!string.IsNullOrEmpty(_reportPath) && File.Exists(_reportPath))
            {
                try
                {
                    txtRawReport.Text = File.ReadAllText(_reportPath);
                }
                catch (Exception ex)
                {
                    txtRawReport.Text = $"无法读取报告文件: {ex.Message}";
                }
            }
            else
            {
                txtRawReport.Text = "报告文件未找到，但在内存中存在诊断结果。";
            }
        }

        // Logic to switch view, used internally by Repair
        private void SwitchToParsedView()
        {
            // Switch UI
            pnlRawView.Visible = false;
            pnlParsedView.Visible = true;
            pnlParsedView.BringToFront();

            // Populate Errors
            PopulateErrors();
        }

        private void PopulateErrors()
        {
            flowPanelErrors.SuspendLayout();
            flowPanelErrors.Controls.Clear();

            if (_result.AllStepsSuccessful)
            {
                Label lblSuccess = new Label();
                lblSuccess.Text = "没有发现错误。";
                lblSuccess.AutoSize = true;
                lblSuccess.Font = new Font("Microsoft YaHei UI", 12F);
                flowPanelErrors.Controls.Add(lblSuccess);
            }
            else
            {
                foreach (var issue in _result.AllIssues)
                {
                    // Create DiagnosticItemControl
                    // Ensure it matches the requested style: transparent background (on white), black text
                    var itemControl = new DiagnosticItemControl(
                        issue.Description,
                        issue.Details,
                        issue.RepairAction
                    );

                    // Force style overrides if necessary
                    itemControl.BackColor = Color.White;

                    itemControl.Width = flowPanelErrors.ClientSize.Width - 25;
                    itemControl.Anchor = AnchorStyles.Left | AnchorStyles.Right;

                    flowPanelErrors.Controls.Add(itemControl);
                    itemControl.PerformLayout();
                }
            }

            flowPanelErrors.ResumeLayout();
        }

        private async void BtnRepairAll_Click(object? sender, EventArgs e)
        {
            if (_result.AllStepsSuccessful) return;

            btnRepairAll.Enabled = false;
            btnRepairAll.Text = "正在修复所有问题...";

            // Automatically switch to parsed view to show progress
            if (pnlRawView.Visible)
            {
                SwitchToParsedView();
            }

            foreach (Control ctrl in flowPanelErrors.Controls)
            {
                if (ctrl is DiagnosticItemControl item)
                {
                    await item.StartRepair();
                }
            }

            btnRepairAll.Text = "修复完成";
            MessageBox.Show("所有修复操作已执行完毕，建议重新检测以验证。", "修复完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
