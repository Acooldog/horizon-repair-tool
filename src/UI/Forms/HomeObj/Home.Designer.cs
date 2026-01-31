
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
            Pbar = new ProgressBar();
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
            statusStrip1.Location = new Point(0, 70);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(274, 22);
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
            // Pbar
            // 
            Pbar.Location = new Point(12, 41);
            Pbar.Name = "Pbar";
            Pbar.Size = new Size(248, 23);
            Pbar.TabIndex = 4;
            Pbar.Visible = false;
            // 
            // Home
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(274, 92);
            Controls.Add(Pbar);
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
        private ProgressBar Pbar;
    }
}
