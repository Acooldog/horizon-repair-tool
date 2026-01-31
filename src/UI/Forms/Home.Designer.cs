
namespace test
{
    partial class Home
    {   

        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            EnableService = new Button();
            disableService = new Button();
            statusStrip1 = new StatusStrip();
            NowVersion = new ToolStripStatusLabel();
            NewVesion = new ToolStripStatusLabel();
            btnStart = new Button();
            Pbar = new ProgressBar();
            lblStatus = new Label();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // EnableService
            // 
            EnableService.Location = new Point(170, 12);
            EnableService.Name = "EnableService";
            EnableService.Size = new Size(90, 23);
            EnableService.TabIndex = 0;
            EnableService.Text = "启用必要服务";
            EnableService.UseVisualStyleBackColor = true;
            EnableService.Click += ClickEnableService;
            // 
            // disableService
            // 
            disableService.Location = new Point(12, 12);
            disableService.Name = "disableService";
            disableService.Size = new Size(106, 23);
            disableService.TabIndex = 1;
            disableService.Text = "禁用冲突服务";
            disableService.UseVisualStyleBackColor = true;
            disableService.Click += disableService_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { NowVersion, NewVesion });
            statusStrip1.Location = new Point(0, 115);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(289, 22);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // NowVersion
            // 
            NowVersion.Name = "NowVersion";
            NowVersion.Size = new Size(40, 17);
            NowVersion.Text = "wait...";
            // 
            // NewVesion
            // 
            NewVesion.Name = "NewVesion";
            NewVesion.Size = new Size(80, 17);
            NewVesion.Text = "点我检查更新";
            NewVesion.Click += NewVesion_Click;
            // 
            // btnStart
            // 
            btnStart.Location = new Point(12, 53);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(75, 23);
            btnStart.TabIndex = 3;
            btnStart.Text = "开始";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click_1;
            // 
            // Pbar
            // 
            Pbar.Location = new Point(114, 53);
            Pbar.Name = "Pbar";
            Pbar.Size = new Size(137, 23);
            Pbar.TabIndex = 4;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(153, 79);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(43, 17);
            lblStatus.TabIndex = 5;
            lblStatus.Text = "label1";
            // 
            // Home
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(289, 137);
            Controls.Add(lblStatus);
            Controls.Add(Pbar);
            Controls.Add(btnStart);
            Controls.Add(statusStrip1);
            Controls.Add(disableService);
            Controls.Add(EnableService);
            Name = "Home";
            Text = "c";
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button EnableService;
        public Button disableService;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel NowVersion;
        private ToolStripStatusLabel NewVesion;
        private Button btnStart;
        private ProgressBar Pbar;
        private Label lblStatus;
    }
}
