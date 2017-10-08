using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace NJNU_Atuo_Login
{

    public partial class frmMain : Form
    {
        public UserConfig userconfig = new UserConfig();
        public bool beenLogin = false;

        public bool programLoad = true;  //标识程序启动，防止触发开机自启时触发CheckBoxChange事件

        int seconds = 0; //seconds秒后重新连接
        int times = 0;   //连接失败尝试times次
        public frmMain()
        {
            InitializeComponent();
            this.Size = new Size(428, 316);
            
        }


        private void Form1_Load(object sender, EventArgs e)
        {

            //this.Size = new Size(428, 316);
            switchPanel(beenLogin);
            userconfig.getConfigure();
            tbUsername.Text = userconfig.username;
            tbPassword.Text = userconfig.password;
            ckbRememberpwd.Checked = userconfig.rememberPassword;
            ckbAutoLogin.Checked = userconfig.autoLogin;
            cbNetType.Items.Add("实验室网络");
            cbNetType.Items.Add("联通服务");
            cbNetType.Items.Add("校园网服务");
            cbNetType.Items.Add("电信服务");
            cbNetType.SelectedText = userconfig.netType;

            ckbAutoRun.Checked = userconfig.autoRun;
            ckbReConnect.Checked = userconfig.reconnect;
            nudReconnMaxNum.Value = userconfig.recoonceMaxNum;
            nudReconnTime.Value = userconfig.reconnectTime;
            ckbTray.Checked = userconfig.tray;
            ckbKeepOnline.Checked = userconfig.keepOnline;
            nudKeepIntvl.Value = userconfig.keepIntvl;
            checkKeepOnline();
            checkReconnect();

            SetAboutInfo();

            programLoad = false;

            //是否自动登录
            if (ckbAutoLogin.Checked)
            {
                LoginAction();
            }


        }



        public void switchPanel(bool beenLogin)
        {
            if (!beenLogin)
            {
                pnLogin.Visible = true;
                pnLogout.Visible = false;
                pnSetting.Visible = false;
                pnAbout.Visible = false;
            }
            else
            {
                pnLogin.Visible = false;
                pnLogout.Visible = true;
                pnSetting.Visible = false;
                pnAbout.Visible = false;
                pnLogout.Location = pnLogin.Location;
            }
        }
        public void loginPanel()
        {
            pnLogin.Visible = true;
            pnLogout.Visible = false;
            pnSetting.Visible = false;
            pnAbout.Visible = false;
        }
        public void logoutPanel(bool beenLogin)
        {
            pnLogin.Visible = false;
            pnLogout.Visible = true;
            pnSetting.Visible = false;
            pnAbout.Visible = false;
            pnLogout.Location = pnLogin.Location;
            btnLogout.Text = beenLogin ? "注 销" : "返 回";
        }
        public void settingPanel()
        {
            pnLogin.Visible = false;
            pnLogout.Visible = false;
            pnSetting.Visible = true;
            pnAbout.Visible = false;
            pnSetting.Location = pnLogin.Location;
        }

        public void aboutPanel()
        {
            pnLogin.Visible = false;
            pnLogout.Visible = false;
            pnSetting.Visible = false;
            pnAbout.Visible = true;
            pnAbout.Location = pnLogin.Location;
        }

        private void tbUsername_TextChanged(object sender, EventArgs e)
        {
            if (tbUsername.Text.Trim() == String.Empty)
            {
                pcbUblank.Visible = true;
                return;
            }
            if (tbUsername.Text.Trim() != String.Empty)
            {
                pcbUblank.Visible = false;
                userconfig.username = tbUsername.Text;
                return;
            }
        }

        private void tbPassword_TextChanged(object sender, EventArgs e)
        {
            if (tbPassword.Text.Trim() == String.Empty)
            {
                pcbPblank.Visible = true;
                return;
            }
            if (tbPassword.Text.Trim() != String.Empty)
            {
                pcbPblank.Visible = false;
                userconfig.password = tbPassword.Text;
                return;
            }
        }

        private void cbNetType_SelectedIndexChanged(object sender, EventArgs e)
        {
            userconfig.netType = cbNetType.Text;
        }

        private void ckbRememberpwd_CheckedChanged(object sender, EventArgs e)
        {
            userconfig.rememberPassword = ckbRememberpwd.Checked;
            if (!ckbRememberpwd.Checked)
            {
                ckbAutoLogin.Checked = false;
            }
        }

        private void ckbAutoLogin_CheckedChanged(object sender, EventArgs e)
        {
            userconfig.autoLogin = ckbAutoLogin.Checked;
            if (ckbAutoLogin.Checked)
            {
                ckbRememberpwd.Checked = true;
            }
        }




        private void btnLogin_Click(object sender, EventArgs e)
        {
            //LoginHelper.Logout(cbNetType.Text);
            #region 检测输入信息是否完整
            if (tbUsername.Text.Trim() == String.Empty)
            {
                pcbUblank.Visible = true;
                return;
            }
            if (tbPassword.Text.Trim() == String.Empty)
            {
                pcbPblank.Visible = true;
                return;
            }

            #endregion

            btnLogin.Enabled = false;
            if (!LoginHelper.IsNetWorkConnect())
            {
                MessageBox.Show("网络故障,请检查网络连接!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnLogin.Enabled = true;
                return;
            }
            //调用登录函数
            LoginAction();

            //更新配置文件
            userconfig.updateConfigure();

            btnLogin.Enabled = true;

        }

        private void LoginAction()
        {     
            //网络登录
            object obj = LoginHelper.Login(tbUsername.Text.Trim(), tbPassword.Text.Trim(), cbNetType.Text);
            if (obj == null)
            {
                logoutPanel(false);
                setLoginResult(false, "连接失败！请检查网络类型后重新尝试！");
                btnLogin.Enabled = true;
                return;
            }

            if (cbNetType.Text == "实验室网络")
            {
                LoginHelper.labNetResponse netResponse = (LoginHelper.labNetResponse)obj;
                labAnalysis(netResponse);
            }
            else if (cbNetType.Text == "联通服务" || cbNetType.Text == "校园网服务" || cbNetType.Text == "电信服务")
            {
                LoginHelper.dormNetResponse netResponse = (LoginHelper.dormNetResponse)obj;
                dormAnalysis(netResponse);
            }
            if (ckbReConnect.Checked && !beenLogin)
            {
                tSecond.Enabled = true;
            }
            else
            {
                tSecond.Enabled = false;
                seconds = userconfig.reconnectTime;
                times = userconfig.recoonceMaxNum;
                lblReconnMess.Text = "";
            }
            
        }

        //解析实验室网络登录结果
        private void labAnalysis(LoginHelper.labNetResponse netResponse)
        {
            string message = "";
            if (netResponse.reply_code == 1 || netResponse.reply_code == 6)
            {
                logoutPanel(beenLogin = true);
                long unixTimeStamp = long.Parse(netResponse.userinfo.acctstarttime);
                DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)); // 当地时区
                DateTime dt = startTime.AddSeconds(unixTimeStamp);
                message = "您好！欢迎进入南京师范大学校园网\r\n" + 
                    "登录时间：" + dt.ToString("yyyy/MM/dd HH:mm")+
                    "\r\n当前IP："+IntToIp(long.Parse(netResponse.userinfo.useripv4));
            }
            else
            {
                logoutPanel(beenLogin = false);
                message = "code " + netResponse.reply_code + ":" + netResponse.reply_msg;
            }
            setLoginResult(beenLogin, message);
        }
        private static string IntToIp(long ipInt)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append((ipInt >> 24) & 0xFF).Append(".");
            sb.Append((ipInt >> 16) & 0xFF).Append(".");
            sb.Append((ipInt >> 8) & 0xFF).Append(".");
            sb.Append(ipInt & 0xFF);
            return sb.ToString();
        }
        //解析宿舍网络登录结果
        private void dormAnalysis(LoginHelper.dormNetResponse netResponse)
        {
            string message = "";
            if (netResponse.result == "success")
            {
                logoutPanel(beenLogin = true);
                LoginHelper.dormUserInfo userinfo = LoginHelper.getUserInfo();
                message = userinfo.userName + "," + userinfo.welcomeTip + "  您已成功连接校园网！\r\n" +
                    "当前IP：" + userinfo.userIp +
                    "\r\n在线设备：" + ((userinfo.ballInfo!=null)?(userinfo.BI[2].value + "台"):"unknow") ;
            }
            else
            {
                logoutPanel(beenLogin = false);
                message = netResponse.result + ":" + netResponse.message;
            }
            setLoginResult(beenLogin, message);
        }

        private void setLoginResult(bool OK,string message)
        {
            lblResult.Text = OK ? "登录成功" : "登录失败";
            lblResult.ForeColor = OK ? Color.Blue : Color.Red;
            lblLoginMess.Text = message;
        }

        private void pbSetting_Click(object sender, EventArgs e)
        {
            settingPanel();
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            settingPanel();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            switchPanel(beenLogin);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            switchPanel(beenLogin);
        }


        private void ckbAutoRun_CheckedChanged(object sender, EventArgs e)
        {
            userconfig.autoRun = ckbAutoRun.Checked;
            if (!programLoad)
            {

                Process myProcess = new Process();
                string fileName = Application.StartupPath + "\\" + "AutoRun.exe";
                string para = Application.ExecutablePath + " " + userconfig.autoRun;
                ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(fileName, para);
                myProcessStartInfo.Verb = "runas";
                myProcess.StartInfo = myProcessStartInfo;
                
                try
                {
                    myProcess.Start();

                    while (!myProcess.HasExited)
                    {
                        myProcess.WaitForExit();
                    }
                    int returnValue = myProcess.ExitCode;

                    if (returnValue != 0)
                    {
                        userconfig.autoRun = !userconfig.autoRun;
                        programLoad = true;
                        ckbAutoRun.Checked = userconfig.autoRun;
                        programLoad = false;
                    }
                }
                catch 
                {
                    userconfig.autoRun = !userconfig.autoRun;
                    programLoad = true;
                    ckbAutoRun.Checked = userconfig.autoRun;
                    programLoad = false;
                }

            }
        }

        private void ckbReConnect_CheckedChanged(object sender, EventArgs e)
        {
            userconfig.reconnect = ckbReConnect.Checked;
            checkReconnect();
        }

        public void checkReconnect()
        {
            if (ckbReConnect.Checked)
            {
                lblReconnMaxNum.Enabled = true;
                lblReconnTime.Enabled = true;
                nudReconnMaxNum.Enabled = true;
                nudReconnTime.Enabled = true;
            }
            else
            {
                lblReconnMaxNum.Enabled = false;
                lblReconnTime.Enabled = false;
                nudReconnMaxNum.Enabled = false;
                nudReconnTime.Enabled = false;
            }

        }
        public void checkKeepOnline()
        {
            if (ckbKeepOnline.Checked)
            {
                lblKeepIntvl.Enabled = true;
                nudKeepIntvl.Enabled = true;
            }
            else
            {
                lblKeepIntvl.Enabled = false;
                nudKeepIntvl.Enabled = false;
            }
        }
        private void ckbTray_CheckedChanged(object sender, EventArgs e)
        {
            userconfig.tray = ckbTray.Checked;
            if (!ckbTray.Checked)
            {
                ckbKeepOnline.Checked = false;
            }
        }

        private void ckbKeepOnline_CheckedChanged(object sender, EventArgs e)
        {
            userconfig.keepOnline = ckbKeepOnline.Checked;
            if (ckbKeepOnline.Checked)
            {
                ckbTray.Checked = true;
            }
            checkKeepOnline();
            if (ckbKeepOnline.Checked)
            {
                tIntvl.Enabled = true;
                tIntvl.Interval = 1000 * (int)nudKeepIntvl.Value;
            }
            else
                tIntvl.Enabled = true;
        }

        private void nudReconnMaxNum_ValueChanged(object sender, EventArgs e)
        {
            userconfig.recoonceMaxNum = (int)nudReconnMaxNum.Value;
            times = userconfig.recoonceMaxNum;
        }

        private void nudReconnTime_ValueChanged(object sender, EventArgs e)
        {
            userconfig.reconnectTime = (int)nudReconnTime.Value;
            seconds = userconfig.reconnectTime;
        }

        private void nudKeepIntvl_ValueChanged(object sender, EventArgs e)
        {
            userconfig.keepIntvl = (int)nudKeepIntvl.Value;
            tIntvl.Interval = 1000 * (int)nudKeepIntvl.Value;
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            switchPanel(beenLogin);
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            switchPanel(beenLogin);
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            aboutPanel();
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            aboutPanel();
        }

        private void pictureBox8_Click_1(object sender, EventArgs e)
        {
            switchPanel(beenLogin);
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            userconfig.updateConfigure();
            if (ckbTray.Checked == true)
            {
                //取消"关闭窗口"事件
                e.Cancel = true; // 取消关闭窗体 
                //使关闭时窗口向右下角缩小的效果
                WindowState = FormWindowState.Minimized;
                mainNotifyIcon.Visible = true;
                //this.m_cartoonForm.CartoonClose();
                Hide();
                return;
            }
            else mainNotifyIcon.Visible = false;
        }

        private void mainNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.Visible)
            {
                this.WindowState = FormWindowState.Minimized;
                this.mainNotifyIcon.Visible = true;
                this.Hide();
            }
            else
            {
                this.Visible = true;
                this.WindowState = FormWindowState.Normal;
                this.Activate();
            }
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.mainNotifyIcon.Visible = false;
            this.Close();
            this.Dispose();
            Environment.Exit(Environment.ExitCode);
        }

        private void 显示窗体ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Visible = true;
            WindowState = FormWindowState.Normal;
            Activate();
        }

        private void 设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Visible = true;
            WindowState = FormWindowState.Normal;
            Activate();
            settingPanel();
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Visible = true;
            WindowState = FormWindowState.Normal;
            Activate();
            aboutPanel();
        }

        public void SetAutoRun(string fileName, bool isAutoRun)
        {
            RegistryKey reg = null;
            try
            {
                if (!System.IO.File.Exists(fileName))
                    throw new Exception("该文件不存在!");
                String name = fileName.Substring(fileName.LastIndexOf(@"\") + 1);
                reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (reg == null)
                    reg = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                if (isAutoRun)
                {

                    reg.SetValue(name, fileName);
                    MessageBox.Show("设置开机自启动成功", "提示");
                }
                else
                { 
                    reg.SetValue(name,false);
                    MessageBox.Show("取消开机自启动成功", "提示");
                }

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                         
                //throw new Exception(ex.ToString());  
            }
            finally
            {
                if (reg != null)
                    reg.Close();
            }
        }


        public void SetAboutInfo()
        {
            lblVersion.Text = AssemblyVersion;
            lblCopyright.Text = AssemblyCopyright;
        }

        #region 程序集属性访问器

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }



        #endregion

        private void linkEmail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("mailto:alex.chiang@jiangyayu.cn");
            linkEmail.LinkVisited = true;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://blog.jiangyayu.cn");
            linkBlog.LinkVisited = true;
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (btnLogout.Text == "注 销")
            { 
                beenLogin = !LoginHelper.Logout(cbNetType.Text);
                switchPanel(beenLogin);
            }
            else if (btnLogout.Text == "返 回")
                loginPanel();
                
        }

        private void tIntvl_Tick(object sender, EventArgs e)
        {
            if (LoginHelper.isOnline() && beenLogin) return;
            else LoginAction();
        }

        int ConnectCount = 1;
        private void tSecond_Tick(object sender, EventArgs e)
        {
            if (seconds == 0)
            {
                lblReconnMess.Text = "";
                LoginAction();
                times--;
                ConnectCount++;
                seconds = userconfig.reconnectTime;
                if (times == 0)
                {
                    tSecond.Enabled = false;
                    ConnectCount = 1;
                    times = userconfig.recoonceMaxNum;
                }
                return;
            }
            lblReconnMess.Text = "登录失败，" + seconds + "秒后进行第" + ConnectCount + "次尝试...";
            seconds--;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Process.Start("https://blog.jiangyayu.cn/archives/NJNU-Auto-Login-Csharp.html/");
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            Process.Start("https://blog.jiangyayu.cn/archives/NJNU-Auto-Login-Csharp.html/");
        }
    }
}
