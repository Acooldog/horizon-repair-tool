namespace test.src.UI.Forms.FH5.MedicalForm
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
            step3Label = new Label();
            step4Label = new Label();
            CancelBtn = new Button();
            step5Label = new Label();
            step6Label = new Label();
            TISHI = new Label();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(progressBar, 0, 8);
            tableLayoutPanel1.Controls.Add(step1Label, 0, 0);
            tableLayoutPanel1.Controls.Add(step2Label, 0, 1);
            tableLayoutPanel1.Controls.Add(step3Label, 0, 2);
            tableLayoutPanel1.Controls.Add(step4Label, 0, 3);
            tableLayoutPanel1.Controls.Add(CancelBtn, 0, 9);
            tableLayoutPanel1.Controls.Add(step5Label, 0, 4);
            tableLayoutPanel1.Controls.Add(step6Label, 0, 5);
            tableLayoutPanel1.Controls.Add(TISHI, 0, 7);
            tableLayoutPanel1.Location = new Point(12, 12);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 10;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
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
            step2Label.Location = new Point(3, 20);
            step2Label.Name = "step2Label";
            step2Label.Size = new Size(278, 17);
            step2Label.TabIndex = 2;
            step2Label.Text = "2. DNS和IP配置诊断";
            step2Label.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // step3Label
            // 
            step3Label.AutoSize = true;
            step3Label.Dock = DockStyle.Fill;
            step3Label.Location = new Point(3, 40);
            step3Label.Name = "step3Label";
            step3Label.Size = new Size(278, 20);
            step3Label.TabIndex = 3;
            step3Label.Text = "3. Xbox服务状态查询";
            step3Label.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // step4Label
            // 
            step4Label.AutoSize = true;
            step4Label.Dock = DockStyle.Fill;
            step4Label.Location = new Point(3, 60);
            step4Label.Name = "step4Label";
            step4Label.Size = new Size(278, 20);
            step4Label.TabIndex = 4;
            step4Label.Text = "4. VPN和防火墙检测";
            step4Label.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // CancelBtn
            // 
            CancelBtn.Dock = DockStyle.Fill;
            CancelBtn.Location = new Point(3, 400);
            CancelBtn.Name = "CancelBtn";
            CancelBtn.Size = new Size(278, 23);
            CancelBtn.TabIndex = 5;
            CancelBtn.Text = "取消";
            CancelBtn.UseVisualStyleBackColor = true;
            CancelBtn.Click += CancelBtn_Click_1;
            // 
            // step5Label
            // 
            step5Label.AutoSize = true;
            step5Label.Dock = DockStyle.Fill;
            step5Label.Location = new Point(3, 80);
            step5Label.Name = "step5Label";
            step5Label.Size = new Size(278, 20);
            step5Label.TabIndex = 6;
            step5Label.Text = "5. 查看是否有冲突的服务正在运行";
            step5Label.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // step6Label
            // 
            step6Label.AutoSize = true;
            step6Label.Dock = DockStyle.Fill;
            step6Label.Location = new Point(3, 100);
            step6Label.Name = "step6Label";
            step6Label.Size = new Size(278, 20);
            step6Label.TabIndex = 7;
            step6Label.Text = "6. 查看必要服务是否开启";
            step6Label.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // TISHI
            // 
            TISHI.AutoSize = true;
            TISHI.Dock = DockStyle.Fill;
            TISHI.Location = new Point(3, 360);
            TISHI.Name = "TISHI";
            TISHI.Size = new Size(278, 17);
            TISHI.TabIndex = 8;
            TISHI.Text = "生成诊断报告中，可能需要1-2分钟...";
            TISHI.TextAlign = ContentAlignment.MiddleCenter;
            TISHI.Visible = false;
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
        private Label step3Label;
        private Label step4Label;
        private Button CancelBtn;
        private Label step5Label;
        private Label step6Label;
        private Label TISHI;
    }
}