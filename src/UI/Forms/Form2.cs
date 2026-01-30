using test.src.Services.Managers;

namespace test
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            Form2_Load();
        }

        /// <summary>
        /// 初始化窗口
        /// </summary>
        /// <param name="sender">测试</param>
        /// <param name="e"></param>
        private void Form2_Load()
        {
            this.Text = "测试窗口";
            new IconCon(this);
        }

        private void click_close(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
