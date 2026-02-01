
namespace test.src.UI.Forms.FH4.CustomRepair
{
    partial class Fh4CustomRepair
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
            Pbar = new ProgressBar();
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
            // Pbar
            // 
            Pbar.Location = new Point(12, 41);
            Pbar.Name = "Pbar";
            Pbar.Size = new Size(248, 23);
            Pbar.TabIndex = 4;
            Pbar.Visible = false;
            // 
            // Fh4CustomRepair
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(274, 92);
            Controls.Add(Pbar);
            Controls.Add(disableService);
            Controls.Add(EnableService);
            Name = "Fh4CustomRepair";
            Text = "c";
            ResumeLayout(false);
        }

        #endregion

        private Button EnableService;
        public Button disableService;
        private ProgressBar Pbar;
    }
}
