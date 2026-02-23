using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using test.src.UI.Helpers;

namespace test.src.UI.Controls
{
    public class DiagnosticItemControl : UserControl
    {
        private Panel pnlContainer = null!;
        private TableLayoutPanel layoutPanel = null!;
        private Label lblTitle = null!;
        private Label lblDetails = null!;
        private Button btnRepair = null!;
        private ProgressBar progressBar = null!;
        private PictureBox pbStatus = null!;
        private Func<IProgress<int>, Task>? repairAction;

        // State
        private bool isFixed = false;

        public event EventHandler? RepairCompleted;

        public DiagnosticItemControl(string title, string details, Func<IProgress<int>, Task>? repairAction)
        {
            this.repairAction = repairAction;
            InitializeComponent();

            lblTitle.Text = title;
            lblDetails.Text = details;

            if (repairAction == null)
            {
                btnRepair.Visible = false;
            }
        }

        private void InitializeComponent()
        {
            // Setup UserControl
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.BackColor = Color.Transparent;
            this.Padding = new Padding(0, 0, 0, 10); // Bottom margin

            // Initialize Controls
            this.pnlContainer = new Panel();
            this.layoutPanel = new TableLayoutPanel();
            this.lblTitle = new Label();
            this.lblDetails = new Label();
            this.btnRepair = new Button();
            this.progressBar = new ProgressBar();
            this.pbStatus = new PictureBox();

            // 
            // pnlContainer
            // 
            this.pnlContainer.Dock = DockStyle.Top;
            this.pnlContainer.AutoSize = true;
            this.pnlContainer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.pnlContainer.BackColor = Color.White; // 显式使用白色背景
            this.pnlContainer.Padding = new Padding(10);
            this.pnlContainer.Paint += PnlContainer_Paint;

            // 
            // layoutPanel
            // 
            this.layoutPanel.Dock = DockStyle.Top;
            this.layoutPanel.AutoSize = true;
            this.layoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.layoutPanel.ColumnCount = 3;
            this.layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F)); // Icon
            this.layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Text
            this.layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F)); // Button
            this.layoutPanel.RowCount = 3;
            this.layoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Row 0: Title
            this.layoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Row 1: Details
            this.layoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Row 2: Progress
            this.layoutPanel.BackColor = Color.Transparent;

            // 
            // pbStatus
            // 
            this.pbStatus.Size = new Size(24, 24);
            this.pbStatus.Margin = new Padding(3, 5, 3, 3);
            this.pbStatus.BackColor = Color.Transparent;
            this.pbStatus.Paint += PbStatus_Paint;
            this.layoutPanel.Controls.Add(this.pbStatus, 0, 0);

            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.Black; // 显式使用黑色
            this.lblTitle.Text = "Issue Title";
            this.lblTitle.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            this.lblTitle.Margin = new Padding(3, 5, 3, 5);
            this.layoutPanel.Controls.Add(this.lblTitle, 1, 0);

            // 
            // btnRepair
            // 
            this.btnRepair.Size = new Size(80, 30);
            this.btnRepair.Text = "修复";
            this.btnRepair.FlatStyle = FlatStyle.Flat;
            this.btnRepair.FlatAppearance.BorderSize = 0;
            this.btnRepair.BackColor = UIStyleHelper.AccentColor;
            this.btnRepair.ForeColor = Color.White;
            this.btnRepair.Cursor = Cursors.Hand;
            this.btnRepair.Click += async (s, e) => await StartRepair();
            this.btnRepair.Paint += BtnRepair_Paint;
            this.btnRepair.Anchor = AnchorStyles.Right;
            this.layoutPanel.Controls.Add(this.btnRepair, 2, 0);

            // 
            // lblDetails
            // 
            this.lblDetails.AutoSize = true;
            this.lblDetails.Font = new Font("Microsoft YaHei UI", 9F);
            this.lblDetails.ForeColor = Color.DarkGray; // 显式使用深灰色
            this.lblDetails.Text = "Details...";
            this.lblDetails.Dock = DockStyle.Fill;
            this.lblDetails.Margin = new Padding(3, 5, 3, 10);
            this.layoutPanel.Controls.Add(this.lblDetails, 1, 1);
            this.layoutPanel.SetColumnSpan(this.lblDetails, 2); // Span text and button columns

            // 
            // progressBar
            // 
            this.progressBar.Dock = DockStyle.Fill;
            this.progressBar.Height = 5;
            this.progressBar.Visible = false;
            this.progressBar.Style = ProgressBarStyle.Continuous;
            this.progressBar.Margin = new Padding(3, 5, 3, 5);
            this.layoutPanel.Controls.Add(this.progressBar, 0, 2);
            this.layoutPanel.SetColumnSpan(this.progressBar, 3); // Span all columns

            this.pnlContainer.Controls.Add(this.layoutPanel);
            this.Controls.Add(this.pnlContainer);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (lblDetails != null)
            {
                // Set MaximumSize to force wrapping
                // Width - IconCol(40) - Padding(20) - Margin(approx 10)
                int availableWidth = this.Width - 60;
                if (availableWidth > 0)
                {
                    lblDetails.MaximumSize = new Size(availableWidth, 0);
                }
            }
        }

        private void PnlContainer_Paint(object? sender, PaintEventArgs e)
        {
            var p = sender as Panel;
            if (p == null) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
            using (var path = UIStyleHelper.GetRoundedPath(rect, 15))
            {
                using (var brush = new SolidBrush(Color.White)) // 显式使用白色
                {
                    g.FillPath(brush, path);
                }
                using (var pen = new Pen(Color.FromArgb(200, 200, 200))) // 浅灰色边框
                {
                    g.DrawPath(pen, path);
                }
            }
        }

        private void BtnRepair_Paint(object? sender, PaintEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, btn.Width, btn.Height);

            // Clear default background
            g.Clear(Color.White); // 显式使用白色

            using (var path = UIStyleHelper.GetRoundedPath(rect, 8))
            {
                using (var brush = new SolidBrush(btn.Enabled ? UIStyleHelper.AccentColor : Color.Gray))
                {
                    g.FillPath(brush, path);
                }
            }

            // Draw text
            TextRenderer.DrawText(g, btn.Text, btn.Font, rect, btn.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private void PbStatus_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            if (!isFixed)
            {
                // Red Cross
                using (var pen = new Pen(UIStyleHelper.ErrorColor, 2))
                {
                    e.Graphics.DrawLine(pen, 4, 4, 20, 20);
                    e.Graphics.DrawLine(pen, 20, 4, 4, 20);
                }
            }
            else
            {
                // Green Check
                using (var pen = new Pen(UIStyleHelper.SuccessColor, 2))
                {
                    e.Graphics.DrawLines(pen, new Point[] { new Point(2, 12), new Point(8, 18), new Point(22, 4) });
                }
            }
        }

        public async Task StartRepair()
        {
            if (repairAction == null || !btnRepair.Enabled) return;

            btnRepair.Enabled = false;
            btnRepair.Text = "修复中...";
            progressBar.Visible = true;
            progressBar.Value = 0;

            try
            {
                var progress = new Progress<int>(percent =>
                {
                    if (this.IsDisposed) return;

                    if (progressBar.InvokeRequired)
                        progressBar.Invoke(new Action(() => progressBar.Value = percent));
                    else
                        progressBar.Value = percent;
                });

                await repairAction(progress);

                // Assume success if no exception
                isFixed = true;
                btnRepair.Text = "已修复";
                // Keep disabled

                if (!this.IsDisposed)
                {
                    progressBar.Visible = false;
                    pbStatus.Invalidate();
                }

                RepairCompleted?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                if (!this.IsDisposed)
                {
                    btnRepair.Text = "重试";
                    btnRepair.Enabled = true;
                    progressBar.Visible = false;
                    MessageBox.Show($"修复失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
