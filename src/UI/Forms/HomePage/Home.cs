namespace test
{
    /// <summary>
    /// 主页
    /// </summary>
    public partial class Home : Form
    {
        public Home()
        {
            InitializeComponent();
            // 加载
            HomeLoad();
        }

        // 定义副标题
        private string WinTitle = "主页";
    }
}
