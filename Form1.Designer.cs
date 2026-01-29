
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
            button1 = new Button();
            disableService = new Button();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(271, 146);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 0;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += click_test;
            // 
            // disableService
            // 
            disableService.Location = new Point(277, 76);
            disableService.Name = "disableService";
            disableService.Size = new Size(106, 23);
            disableService.TabIndex = 1;
            disableService.Text = "禁用冲突服务";
            disableService.UseVisualStyleBackColor = true;
            disableService.Click += disableService_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(674, 416);
            Controls.Add(disableService);
            Controls.Add(button1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private Button button1;
        private Button disableService;
    }
}
