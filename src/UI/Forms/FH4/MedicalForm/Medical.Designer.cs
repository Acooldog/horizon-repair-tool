namespace test.src.UI.Forms.FH4.MedicalForm
{
    partial class Medical
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tableLayoutPanel1 = new TableLayoutPanel();
            progressBar = new ProgressBar();
            step1Label = new Label();
            step2Label = new Label();
            CancelBtn = new Button();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(progressBar, 0, 2);
            tableLayoutPanel1.Controls.Add(step1Label, 0, 0);
            tableLayoutPanel1.Controls.Add(step2Label, 0, 1);
            tableLayoutPanel1.Controls.Add(CancelBtn, 0, 3);
            tableLayoutPanel1.Location = new Point(12, 12);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(284, 426);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(3, 380);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(278, 14);
            progressBar.TabIndex = 0;
            // 
            // step1Label
            // 
            step1Label.AutoSize = true;
            step1Label.Dock = DockStyle.Top;
            step1Label.Location = new Point(3, 0);
            step1Label.Name = "step1Label";
            step1Label.Size = new Size(278, 17);
            step1Label.TabIndex = 1;
            step1Label.Text = "1. 检查Teredo和Xbox Networking";
            step1Label.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // step2Label
            // 
            step2Label.AutoSize = true;
            step2Label.Dock = DockStyle.Top;
            step2Label.Location = new Point(3, 17);
            step2Label.Name = "step2Label";
            step2Label.Size = new Size(278, 17);
            step2Label.TabIndex = 2;
            step2Label.Text = "2. aa";
            step2Label.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // CancelBtn
            // 
            CancelBtn.Dock = DockStyle.Fill;
            CancelBtn.Location = new Point(3, 400);
            CancelBtn.Name = "CancelBtn";
            CancelBtn.Size = new Size(278, 23);
            CancelBtn.TabIndex = 3;
            CancelBtn.Text = "取消";
            CancelBtn.UseVisualStyleBackColor = true;
            CancelBtn.Click += CancelBtn_Click;
            // 
            // Medical
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(308, 450);
            Controls.Add(tableLayoutPanel1);
            Name = "Medical";
            Text = "Medical";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private ProgressBar progressBar;
        private Label step1Label;
        private Label step2Label;
        private Button CancelBtn;
    }
}