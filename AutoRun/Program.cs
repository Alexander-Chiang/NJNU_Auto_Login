using Microsoft.Win32;
using System;
using System.Windows.Forms;

namespace AutoRun
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        //static void Main()
        //{
        //    Application.EnableVisualStyles();
        //    Application.SetCompatibleTextRenderingDefault(false);
        //    Application.Run(new Form1());
        //}
        static int Main(string[] args)
        {

            RegistryKey reg = null;
            string fileName = "";
            for (int i = 0; i < args.Length - 1; i++)
            {
                fileName += args[i] + " ";
            }
            fileName.Trim();
            bool isAutoRun;
            try
            {
                isAutoRun = bool.Parse(args[args.Length - 1]);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("参数类型错误！" + args[args.Length - 1] + "需要为True或者False！\r\n"+ex.Message);
            }

            try
            {
                if (!System.IO.File.Exists(fileName))
                    throw new Exception("该文件不存在!");
                string name = fileName.Substring(fileName.LastIndexOf(@"\") + 1).Split('.')[0];
                reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (reg == null)
                    reg = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

                if (isAutoRun)
                {

                    reg.CreateSubKey(name);
                    reg.SetValue(name, fileName);
                    MessageBox.Show("已设置开机自启动！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return 0;
                }
                else
                {

                    if (reg.GetValue(name) != null)
                        reg.DeleteValue(name);
                    MessageBox.Show("已取消开机自启动！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return 0;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(),"错误",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return 1;

                //throw new Exception(ex.ToString());  
            }
            finally
            {
                if (reg != null)
                    reg.Close();
            }
        }
    }
}
