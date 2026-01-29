using test.tools;
using Newtonsoft.Json;

namespace test
{   
    internal static class Program
    {   
        
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Logs.SetupTraceListener();
            Logs.LogInfo("程序启动");
            string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Logs.LogInfo(logDir);
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            try
            {
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                Logs.LogError("程序启动异常", ex);
                MessageBox.Show($"程序启动异常: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error
                    );
            }
            finally
            {
                Logs.LogInfo("程序退出");
            }
        }

        static void check_admin()
        {
            bool Yon = ServiceManager.CheckAdministratorPrivileges();
            if (!Yon)
            {
                MessageBox.Show("请以管理员身份运行本程序！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }
    }
}