using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace NJNU_Atuo_Login
{
    static class LoginHelper
    {


        public static object Login(string username, string password,string netType)
        {
            object result = null;
            if (!checkNetType(netType)) return result;
            if (netType == "实验室网络")
                result = labNetLogin(username, password);
            else if ((netType == "联通服务" || netType == "电信服务" || netType == "校园网服务"))
                result = dormNetLogin(username, password, netType);
            return result;
        }

        public static object getInfo(string username, string password, string netType)
        {
            object result = null;
            if (isOnline())
            {
                result = Login(username, password, netType);
            }
            return result;
        }

        private static bool checkNetType(string netType)
        {
            bool result = true;
            if (netType == "实验室网络" && islabNetType()) 
                result = true;
            else if ((netType == "联通服务" || netType == "电信服务" || netType == "校园网服务") && isdormNetType())
                result = true;
            else result = false;

            return result;

        }

        public static dormUserInfo  getUserInfo()
        {

            string url = "http://223.2.10.172/eportal/InterFace.do?method=getOnlineUserInfo";
            //url加密两次
            string param = "";
            string resStr = HttpPost(url, param);
            //JObject json = JObject.Parse(resStr);
            dormUserInfo useInfo = JsonConvert.DeserializeObject<dormUserInfo>(resStr);
            useInfo.BI = JsonConvert.DeserializeObject<List<BallInfo>>(useInfo.ballInfo);
            return useInfo;
        }

        private static dormNetResponse dormNetLogin(string username, string password, string netType)
        {

            string url = "http://223.2.10.172/eportal/InterFace.do?method=login";
            //url加密两次
            string param = "userId="+myUrlEncode(username)+
                "&password="+myUrlEncode(password)+
                "&service="+myUrlEncode(netType)+
                "&queryString="+myUrlEncode(getQueryString())+
                "&operatorPwd=&operatorUserId=&validcode=";
            string resStr = HttpPost(url, param);
            dormNetResponse netResponse = JsonConvert.DeserializeObject<dormNetResponse>(resStr);
            return netResponse;
        }
        private static string myUrlEncode(string url)
        {
            return WebUtility.UrlEncode(WebUtility.UrlEncode(url));
        }
        private static labNetResponse labNetLogin(string username, string password)
        {
            string url = "http://portal.njnu.edu.cn/portal_io/login";
            string param = "username=" + username + "&password=" + password;
            string resStr = HttpPost(url, param);
            labNetResponse netResponse = JsonConvert.DeserializeObject<labNetResponse>(resStr);
            return netResponse;
        }


        public static bool Logout(string netType)
        {
            bool result = false;
            if (!checkNetType(netType)) return result;
            if (netType == "实验室网络")
                result = labNetLogout();
            else if ((netType == "联通服务" || netType == "电信服务" || netType == "校园网服务"))
                result = dormNetLogout(); ;
            return result;

        }

        private static bool dormNetLogout()
        {
            bool result = false;
            string url = "http://223.2.10.172/eportal/InterFace.do?method=logout";
            string resStr = HttpPost(url, "");
            dormNetResponse netResponse = JsonConvert.DeserializeObject<dormNetResponse>(resStr);
            if (netResponse.result == "success")
                result = true;
            return result;
        }
        private static bool labNetLogout()
        {
            bool result = false;
            string url = "http://portal.njnu.edu.cn/portal_io/logout";
            string resStr = HttpPost(url, "");
            labNetResponse netResponse = JsonConvert.DeserializeObject<labNetResponse>(resStr);
            if (netResponse.reply_code == 101)
                result = true;
            return result;
        }


        /// <summary>
        /// 判断是否联网状态
        /// </summary>
        /// <returns></returns>
        public static bool isOnline()
        {
            bool result = true;
            if (!IsNetWorkConnect()) return false;
            string url = "http://www.baidu.com";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = 3000;
            request.AllowAutoRedirect = false;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Stream stream = response.GetResponseStream();
                    try
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string ResStr = reader.ReadToEnd();
                            //返回值包含http://223.2.10.172/eportal/index.jsp  外网不可用
                            if ((int)response.StatusCode == 200 && ResStr.Contains("http://223.2.10.172/eportal/index.jsp"))
                                result = false;
                        }
                    }
                    finally
                    {
                        stream.Close();
                    }
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }

        [DllImport("wininet")]
        private extern static bool InternetGetConnectedState(out int connectionDescription, int reservedValue);
        //判断网络是连接到互联网
        public static bool IsNetWorkConnect()
        {
            int i = 0;
            return InternetGetConnectedState(out i, 0) ? true : false;
        }


        private static string getQueryString()
        {
            string result = "wlanuserip=f02d3ceebe91a66d24e3f38f5bea9d58&wlanacname=b58fcda622d8bb5b&ssid=52eefd2d44d14e03&nasip=e54b2b351c575112839c042a61804a4c&snmpagentip=&mac=e7c1d32b7bccc6b225098b32cbc1258a&t=wireless-v2&url=2c0328164651e2b4f13b933ddf36628bea622dedcc302b30&apmac=&nasid=b58fcda622d8bb5b&vid=0174be557adff7e1&port=3f75b73f368e9840&nasportid=a4ea9de28cbfe9fc659fd94ce1242135dea10bd719bd8f88e774ef83abd9d6d59281769eacd2d93b";
            try
            {
                string str = HttpGet("http://223.2.10.172");
                result = str.Split('\'')[1].Split('?')[1];
            }
            catch { }
            return result;

        }

        private static bool islabNetType()
        {
            bool result = false;
            string Url = "http://portal.njnu.edu.cn";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
            req.Method = "GET";
            req.Timeout = 1000;
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
            req.ContentType = "application/x-www-form-urlencoded";
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
                {
                    Stream stream = response.GetResponseStream();
                    try
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string ResStr = reader.ReadToEnd();
                            if ((int)response.StatusCode == 200 && !ResStr.Contains("http://223.2.10.172/eportal/index.jsp"))
                                result = true;
                        }
                    }
                    finally
                    {
                        stream.Close();
                    }
                }
            }
            catch
            {
                result = false;
            }
            return result;
            
        }

        private static bool isdormNetType()
        {
            bool result = false;
            string Url = "http://223.2.10.172";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
            req.Method = "GET";
            req.Timeout = 1000;
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
            req.ContentType = "application/x-www-form-urlencoded";
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
                {
                    Stream stream = response.GetResponseStream();
                    try
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string ResStr = reader.ReadToEnd();
                            //返回值包含http://223.2.10.172/eportal  表示是宿舍网络
                            if ((int)response.StatusCode == 200 && ResStr.Contains("http://223.2.10.172/eportal/"))
                                result = true;
                        }
                    }
                    finally
                    {
                        stream.Close();
                    }
                }
            }
            catch
            {
                result = false;
            }
            return result;
            
        }

        public static string HttpPost(string Url, string postDataStr)
        {
            string value = "";
            byte[] bs = Encoding.UTF8.GetBytes(postDataStr);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
            req.Method = "POST";
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = bs.Length;
            req.Timeout = 3000;

            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
            }
            using (WebResponse wr = req.GetResponse())
            {
                Stream stream = wr.GetResponseStream();
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                value = reader.ReadToEnd();
            }
            return value;
        }

        public static string HttpGet(string Url)
        {
            string result = "";

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
            req.Method = "GET";
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.115 Safari/537.36";
            req.ContentType = "application/x-www-form-urlencoded";

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream stream = resp.GetResponseStream();
            try
            {
                //获取内容  
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }
            finally
            {
                stream.Close();
                resp.Close();
            }
            return result;
        }

        //宿舍网络登录返回值
        public class dormNetResponse
        {
            public string userIndex { get; set; }
            public string result { get; set; }
            public string message { get; set; }
            public int keepaliveInterval { get; set; }
            public string validCodeUrl { get; set; }
        }
        //实验室网络登录返回值
        public class labNetResponse
        {
            public int reply_code { get; set; }
            public string reply_msg { get; set; }

            public userinfo userinfo ;
    
        }
        public class userinfo
        {
            public string username { get; set; }
            public string fullname { get; set; }
            public string service_name { get; set; }
            public string area_name { get; set; }
            public string acctstarttime { get; set; }
            public string balance { get; set; }
            public string useripv4 { get; set; }
            public string useripv6 { get; set; }
            public string mac { get; set; }
        };

        public class dormUserInfo
        {
            public string userIndex { get; set; }
            public string result { get; set; }
            public string message { get; set; }
            public string userName { get; set; }
            public string userId { get; set; }
            public string userIp { get; set; }
            public string userMac { get; set; }
            public string service { get; set; }
            public string welcomeTip { get; set; }
            public string ballInfo { get; set; }

            public List<BallInfo> BI = new List<BallInfo>();
            
        }


        public class BallInfo
        {
            public string alertContent { get; set; }
            public string alertContentMobile { get; set; }
            public string alertUrl { get; set; }
            public string displayName { get; set; }
            public string displayNameOnMoveIn { get; set; }
            public string id { get; set; }
            public string jsonValue { get; set; }
            public string needAlert { get; set; }
            public string type { get; set; }
            public string typeOnMoveIn { get; set; }
            public string url { get; set; }
            public string value { get; set; }
            public string valueOnMoveIn { get; set; }
        }



    }
}
