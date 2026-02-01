using test.src.Services.FH4.Managers;
using test.src.Services.FH4.Internetwork;
using test.src.Services.PublicFuc.Managers;

namespace test.src.UI.Forms.FH4.CustomRepair
{   
    /// <summary>
    /// 自定义修复界面
    /// </summary>
    public partial class Fh4CustomRepair : Form
    {
        public Fh4CustomRepair()
        {
            string SoftName = string.Empty;
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedDialog;  // 固定对话框，不可调整大小
            Form1_Load();

            SetupControls();
            SetupBackgroundWorker();

        }

        #region 窗体加载事件

        /// <summary>
        /// 加载窗体的函数
        /// </summary>
        private void Form1_Load()
        {   
            // 自动调整大小
            this.AutoSize = true;

            new IconCon(this);
            // 设置窗口启动位置为屏幕中央
            this.StartPosition = FormStartPosition.CenterScreen;

            this.Text = "地平线修复工具";
        }

        #endregion 窗体加载事件
        
    }
}

    

