using test.tools;


namespace test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Form1_Load();
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void click_test(object sender, EventArgs e)
        {
            Form2 dlog = new Form2();
            dlog.ShowDialog();
        }

        private void Form1_Load()
        {
            new IconCon(this);
        }
    }

    
}
