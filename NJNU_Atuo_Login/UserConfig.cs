using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NJNU_Atuo_Login
{
    public class UserConfig
    {
        private string key = "19921206";

        public string username { get; set; }
        public string password { get; set; }
        public string netType { get; set; }
        public bool tray { get; set; }
        public bool okMessage { get; set; }
        public bool faileMessage { get; set; }
        public bool reconnect { get; set; }
        public int reconnectTime { get; set; }
        public int recoonceMaxNum { get; set; }
        public bool keepOnline { get; set; }
        public int keepIntvl { get; set; }
        public bool autoRun { get; set; }
        public bool autoLogin { get; set; }
        public bool rememberPassword { get; set; }


        public void getConfigure()
        {
            username = DESEncryption.Decrypt(ConfigHelper.GetAppConfig("username"),key);
            password = DESEncryption.Decrypt(ConfigHelper.GetAppConfig("password"),key);
            netType = ConfigHelper.GetAppConfig("netType");
            tray = bool.Parse(ConfigHelper.GetAppConfig("tray"));
            okMessage = bool.Parse(ConfigHelper.GetAppConfig("okMessage"));
            faileMessage = bool.Parse(ConfigHelper.GetAppConfig("faileMessage"));
            reconnect = bool.Parse(ConfigHelper.GetAppConfig("reconnect"));
            reconnectTime = int.Parse(ConfigHelper.GetAppConfig("reconnectTime"));
            recoonceMaxNum = int.Parse(ConfigHelper.GetAppConfig("recoonceMaxNum"));
            keepOnline = bool.Parse(ConfigHelper.GetAppConfig("keepOnline"));
            keepIntvl = int.Parse(ConfigHelper.GetAppConfig("keepIntvl"));
            autoRun = bool.Parse(ConfigHelper.GetAppConfig("autoRun"));
            autoLogin = bool.Parse(ConfigHelper.GetAppConfig("autoLogin"));
            rememberPassword = bool.Parse(ConfigHelper.GetAppConfig("rememberPassword"));
        }
        public void updateConfigure()
        {
             ConfigHelper.UpdateAppConfig("username", DESEncryption.Encrypt(username, key));
             if(rememberPassword)
                 ConfigHelper.UpdateAppConfig("password", DESEncryption.Encrypt(password,key));
             else
                 ConfigHelper.UpdateAppConfig("password", DESEncryption.Encrypt("", key));
             ConfigHelper.UpdateAppConfig("netType", netType);
             ConfigHelper.UpdateAppConfig("tray", tray.ToString());
             ConfigHelper.UpdateAppConfig("okMessage", okMessage.ToString());
             ConfigHelper.UpdateAppConfig("faileMessage", faileMessage.ToString());
             ConfigHelper.UpdateAppConfig("reconnect", reconnect.ToString());
             ConfigHelper.UpdateAppConfig("reconnectTime", reconnectTime.ToString());
             ConfigHelper.UpdateAppConfig("recoonceMaxNum", recoonceMaxNum.ToString());
             ConfigHelper.UpdateAppConfig("keepOnline", keepOnline.ToString());
             ConfigHelper.UpdateAppConfig("keepIntvl", keepIntvl.ToString());
             ConfigHelper.UpdateAppConfig("autoRun", autoRun.ToString());
             ConfigHelper.UpdateAppConfig("autoLogin", autoLogin.ToString());
             ConfigHelper.UpdateAppConfig("rememberPassword", rememberPassword.ToString());
        }
    }
}
