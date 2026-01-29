
namespace test
{
    partial class Form1
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
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            toolStripStatusLabel2 = new ToolStripStatusLabel();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // EnableService
            // 
            EnableService.Location = new Point(142, 21);
            EnableService.Name = "EnableService";
            EnableService.Size = new Size(90, 23);
            EnableService.TabIndex = 0;
            EnableService.Text = "启用必要服务";
            EnableService.UseVisualStyleBackColor = true;
            EnableService.Click += ClickEnableService;
            // 
            // disableService
            // 
            disableService.Location = new Point(12, 21);
            disableService.Name = "disableService";
            disableService.Size = new Size(106, 23);
            disableService.TabIndex = 1;
            disableService.Text = "禁用冲突服务";
            disableService.UseVisualStyleBackColor = true;
            disableService.Click += disableService_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1, toolStripStatusLabel2 });
            statusStrip1.Location = new Point(0, 48);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(242, 22);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(14, 17);
            toolStripStatusLabel1.Text = "v";
            // 
            // toolStripStatusLabel2
            // 
            toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            toolStripStatusLabel2.Size = new Size(65, 17);
            toolStripStatusLabel2.Text = "新版本yon";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(242, 70);
            Controls.Add(statusStrip1);
            Controls.Add(disableService);
            Controls.Add(EnableService);
            Name = "Form1";
            Text = "Form1";
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button EnableService;
        public Button disableService;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel toolStripStatusLabel2;
    }
}
