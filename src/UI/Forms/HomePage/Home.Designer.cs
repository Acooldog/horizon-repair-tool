namespace test
{
    partial class Home
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
            statusStrip1 = new StatusStrip();
            NowVersion = new ToolStripStatusLabel();
            NewVersion = new ToolStripStatusLabel();
            FH4Repair = new Button();
            FH4CustomRepair = new Button();
            FH4GroupBox = new GroupBox();
            FH5GroupBox = new GroupBox();
            FH5Repair = new Button();
            FH5CustomRepair = new Button();
            statusStrip1.SuspendLayout();
            FH4GroupBox.SuspendLayout();
            FH5GroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { NowVersion, NewVersion });
            statusStrip1.Location = new Point(0, 87);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(300, 22);
            statusStrip1.TabIndex = 0;
            statusStrip1.Text = "statusStrip1";
            // 
            // NowVersion
            // 
            NowVersion.Name = "NowVersion";
            NowVersion.Size = new Size(40, 17);
            NowVersion.Text = "wait...";
            // 
            // NewVersion
            // 
            NewVersion.Name = "NewVersion";
            NewVersion.Size = new Size(92, 17);
            NewVersion.Text = "点我获取新版本";
            NewVersion.Click += NewVersion_Click;
            // 
            // FH4Repair
            // 
            FH4Repair.Location = new Point(6, 22);
            FH4Repair.Name = "FH4Repair";
            FH4Repair.Size = new Size(126, 23);
            FH4Repair.TabIndex = 1;
            FH4Repair.Text = "一键修复";
            FH4Repair.UseVisualStyleBackColor = true;
            FH4Repair.Click += FH4Repair_Click;
            // 
            // FH4CustomRepair
            // 
            FH4CustomRepair.Location = new Point(6, 49);
            FH4CustomRepair.Name = "FH4CustomRepair";
            FH4CustomRepair.Size = new Size(126, 23);
            FH4CustomRepair.TabIndex = 2;
            FH4CustomRepair.Text = "自定义修复";
            FH4CustomRepair.UseVisualStyleBackColor = true;
            FH4CustomRepair.Click += FH4CustomRepair_Click;
            // 
            // FH4GroupBox
            // 
            FH4GroupBox.Controls.Add(FH4Repair);
            FH4GroupBox.Controls.Add(FH4CustomRepair);
            FH4GroupBox.Location = new Point(0, 3);
            FH4GroupBox.Name = "FH4GroupBox";
            FH4GroupBox.Size = new Size(142, 81);
            FH4GroupBox.TabIndex = 3;
            FH4GroupBox.TabStop = false;
            FH4GroupBox.Text = "地平线4";
            // 
            // FH5GroupBox
            // 
            FH5GroupBox.Controls.Add(FH5Repair);
            FH5GroupBox.Controls.Add(FH5CustomRepair);
            FH5GroupBox.Location = new Point(158, 3);
            FH5GroupBox.Name = "FH5GroupBox";
            FH5GroupBox.Size = new Size(142, 81);
            FH5GroupBox.TabIndex = 4;
            FH5GroupBox.TabStop = false;
            FH5GroupBox.Text = "地平线5";
            // 
            // FH5Repair
            // 
            FH5Repair.Location = new Point(6, 22);
            FH5Repair.Name = "FH5Repair";
            FH5Repair.Size = new Size(126, 23);
            FH5Repair.TabIndex = 1;
            FH5Repair.Text = "一键修复";
            FH5Repair.UseVisualStyleBackColor = true;
            FH5Repair.Click += FH5Repair_Click;
            // 
            // FH5CustomRepair
            // 
            FH5CustomRepair.Location = new Point(6, 49);
            FH5CustomRepair.Name = "FH5CustomRepair";
            FH5CustomRepair.Size = new Size(126, 23);
            FH5CustomRepair.TabIndex = 2;
            FH5CustomRepair.Text = "自定义修复";
            FH5CustomRepair.UseVisualStyleBackColor = true;
            FH5CustomRepair.Click += FH5CustomRepair_Click;
            // 
            // Home
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(300, 109);
            Controls.Add(FH5GroupBox);
            Controls.Add(FH4GroupBox);
            Controls.Add(statusStrip1);
            Name = "Home";
            Text = "Home";
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            FH4GroupBox.ResumeLayout(false);
            FH5GroupBox.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStrip1;
        private ToolStripStatusLabel NowVersion;
        private ToolStripStatusLabel NewVersion;
        private Button FH4Repair;
        private Button FH4CustomRepair;
        private GroupBox FH4GroupBox;
        private GroupBox FH5GroupBox;
        private Button FH5Repair;
        private Button FH5CustomRepair;
    }
}