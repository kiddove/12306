using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;

namespace _12306
{
    public partial class Form1 : Form
    {
        //cookie
        //设置cookie
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InternetSetCookie(string lpszUrlName, string lpszCookieName, string lpszCookieData);
        ////读取cookie
        //[DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //public static extern bool InternetGetCookie(string url, string name, StringBuilder data, ref int dataSize);




        string strQueryDate = "2012-09-28";
        string strOrderDate = "2012-09-28";
        CookieContainer cookies = new CookieContainer();
        string strLoginRand;
        string strRandError;
        // 登录验证码
        string strLoginCode;
        // 下单验证码
        string strOrderCode;
        string strToken;
        string strTicketLeft;

        string strypDetail;
        string strmmStr;

        public Form1()
        {
            InitializeComponent();
            GetLoginRand();
        }


        //登录页面
        //http://dynamic.12306.cn/otsweb/main.jsp
        //登录页面验证码
        //http://dynamic.12306.cn/otsweb/passCodeAction.do?rand=sjrand
        //登录请求1
        //http://dynamic.12306.cn/otsweb/loginAction.do?method=loginAysnSuggest
        //返回1 ---- {"loginRand":"515","randError":"Y"}
        //登录请求2
        //参数 --- loginRand=515&refundLogin=N&refundFlag=Y&loginUser.user_name=kiddove&nameErrorFocus=&user.password=jason55&passwordErrorFocus=&randCode=n22c&randErrorFocus=
        //http://dynamic.12306.cn/otsweb/loginAction.do?method=login


        // 试试 用程序登陆，然后用IE打开，同一个cookie
        public bool Login()
        {
            strRandError = "";
            strLoginCode = textBoxLoginCode.Text;

            if (0 == strLoginCode.Length)
            {
                MessageBox.Show("请输入验证码");
                return false;
            }

            while ("Y" != strRandError)
            {
                PreLogin();
            }

            // 做登录
            string strLoginURL = @"http://dynamic.12306.cn/otsweb/loginAction.do?method=login";
            HttpWebRequest loginReq = (HttpWebRequest)WebRequest.Create(strLoginURL);
            loginReq.Timeout = 10 * 1000; // 10s 超时
            loginReq.Accept = "*/*";
            loginReq.Method = "POST";
            loginReq.CookieContainer = cookies;
            loginReq.ContentType = "application/x-www-form-urlencoded";
            loginReq.Referer = "http://dynamic.12306.cn/otsweb/loginAction.do?method=init";

            // 填充post数据

            StringBuilder sb = new StringBuilder();
            //loginRand=515&refundLogin=N&refundFlag=Y&loginUser.user_name=kiddove&nameErrorFocus=&user.password=jason55&passwordErrorFocus=&randCode=n22c&randErrorFocus=

            //loginRand
            sb.AppendFormat("{0}={1}", "loginRand", strLoginRand);
            //是否退票登录
            sb.AppendFormat("&{0}={1}", "refundLogin", "N");
            sb.AppendFormat("&{0}={1}", "refundFlag", "Y");
            sb.AppendFormat("&{0}={1}", "loginUser.user_name", "kiddove");
            sb.AppendFormat("&{0}={1}", "nameErrorFocus", "");
            sb.AppendFormat("&{0}={1}", "user.password", "jason55");
            sb.AppendFormat("&{0}={1}", "passwordErrorFocus", "");
            // 验证码
            sb.AppendFormat("&{0}={1}", "randCode", strLoginCode);
            sb.AppendFormat("&{0}={1}", "randErrorFocus", "");

            byte[] postData = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            try
            {
                Stream sm = loginReq.GetRequestStream();
                sm.Write(postData, 0, postData.Length);
                sm.Close();
                HttpWebResponse loginRes = (HttpWebResponse)loginReq.GetResponse();

                Stream smRet = loginRes.GetResponseStream();
                StreamReader sr = new StreamReader(smRet);
                string strRet = sr.ReadToEnd();

                loginRes.Close();
                if (strRet.Contains(@"系统消息"))
                {
                    // 在IE中打开
                    //InternetSetCookie(cookies);
                    buttonOrderRand.Enabled = true;
                    GetOrderRand();
                    CookieCollection cc = cookies.GetCookies(loginReq.RequestUri);
                    foreach (Cookie c in cc)
                    {
                        InternetSetCookie("http://" + c.Domain, c.Name, c.Value + ";expires=Sun,22-Feb-2099 00:00:00 GMT");
                    }
                    System.Diagnostics.Process.Start(@"IEXPLORE.EXE", "http://dynamic.12306.cn/otsweb/main.jsp");
                    return true;
                }
                else if (strRet.Contains("请输入正确的验证码"))
                {
                    MessageBox.Show("请输入正确的验证码");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return false;
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            // 开线程...
            Login();
            // query 2 get mmstr & ypdetail
            QueryZ38();
            // submutorder -- must be done
            SubmutOrderZ38();
            GetTokenAndLeftTicketStr();
        }

        // 获得订单验证码
        private void GetOrderRand()
        {
            // http://dynamic.12306.cn/otsweb/passCodeAction.do?rand=randp
            string randURL = @"http://dynamic.12306.cn/otsweb/passCodeAction.do?rand=randp";
            HttpWebRequest randReq = (HttpWebRequest)WebRequest.Create(randURL);
            randReq.Timeout = 10 * 1000; // 10s 超时
            randReq.Accept = "*/*";
            randReq.Method = "GET";
            randReq.CookieContainer = cookies;
            try
            {
                HttpWebResponse randRes = (HttpWebResponse)randReq.GetResponse();

                Stream randStream = randRes.GetResponseStream();

                // 是图片
                if (randRes.ContentType.ToLower().Equals("image/jpeg"))
                    // 展示图片
                    pictureBoxOrder.Image = Image.FromStream(randStream);

                randStream.Close();
                randRes.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        // 获得登录验证码
        private void GetLoginRand()
        {
            string randURL = @"http://dynamic.12306.cn/otsweb/passCodeAction.do?rand=sjrand";
            HttpWebRequest randReq = (HttpWebRequest)WebRequest.Create(randURL);
            randReq.Timeout = 10 * 1000; // 10s 超时
            randReq.Accept = "*/*";
            randReq.Method = "GET";
            randReq.CookieContainer = cookies;
            try
            {
                HttpWebResponse randRes = (HttpWebResponse)randReq.GetResponse();

                Stream randStream = randRes.GetResponseStream();

                // 是图片
                if (randRes.ContentType.ToLower().Equals("image/jpeg"))
                    // 展示图片
                    pictureBoxLogin.Image = Image.FromStream(randStream);

                randStream.Close();
                randRes.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void buttonLoginRand_Click(object sender, EventArgs e)
        {
            GetLoginRand();
            //GetOrderRand();
        }

        // 登录前的准备 -- 拿一个loginrand值
        //http://dynamic.12306.cn/otsweb/loginAction.do?method=loginAysnSuggest
        //返回 ---- {"loginRand":"515","randError":"Y"}
        // Y或许有用,待验证
        private void PreLogin()
        {
            string PreLoginURL = @"http://dynamic.12306.cn/otsweb/loginAction.do?method=loginAysnSuggest";
            HttpWebRequest randPreLogin = (HttpWebRequest)WebRequest.Create(PreLoginURL);
            randPreLogin.Timeout = 10 * 1000; // 10s 超时
            randPreLogin.Accept = "*/*";
            randPreLogin.Method = "GET";
            randPreLogin.CookieContainer = cookies;
            try
            {
                HttpWebResponse preLoginRes = (HttpWebResponse)randPreLogin.GetResponse();

                Stream preLoginStream = preLoginRes.GetResponseStream();
                StreamReader sr = new StreamReader(preLoginStream);
                string strResult = sr.ReadToEnd();
                preLoginRes.Close();
                sr.Close();

                strLoginRand = GetLoginRand(strResult);
                strRandError = GetRandError(strResult);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private string GetLoginRand(string str)
        {
            string[] strRet = str.Split('"');

            Debug.Assert(strRet.Length == 9);
            return strRet[3];
        }

        private string GetRandError(string str)
        {
            string[] strRet = str.Split('"');

            Debug.Assert(strRet.Length == 9);
            return strRet[7];
        }

        private void buttonOrderRand_Click(object sender, EventArgs e)
        {
            GetOrderRand();
        }

        private void QueryTrain()
        {
            // 返回是个json
            // http://dynamic.12306.cn/otsweb/order/querySingleAction.do?method=queryststrainall
            // post -- date=2012-09-25&fromstation=BJP&tostation=WCN&starttime=00%3A00--24%3A00 urlencode
        }

        private bool MakeOrder()
        {

            return false;
        }

        private void GetTokenAndLeftTicketStr()
        {
            strToken = strTicketLeft = "";
            // http://dynamic.12306.cn/otsweb/order/confirmPassengerAction.do?method=init
            // get
            string strURL = @"http://dynamic.12306.cn/otsweb/order/confirmPassengerAction.do?method=init";
            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(strURL);
            httpReq.Timeout = 10 * 1000;
            httpReq.Accept = "*/*";
            httpReq.Method = "GET";
            httpReq.CookieContainer = cookies;
            httpReq.Referer = "http://dynamic.12306.cn/otsweb/order/querySingleAction.do?method=init";

            try
            {
                HttpWebResponse httpRes = (HttpWebResponse)httpReq.GetResponse();
                Stream preLoginStream = httpRes.GetResponseStream();
                StreamReader sr = new StreamReader(preLoginStream);
                string strResult = sr.ReadToEnd();
                httpRes.Close();
                sr.Close();

                // 查找
                // leftTicketStr
                int iStart = strResult.IndexOf("leftTicketStr", 0);
                if (iStart < 0)
                    return;
                int i1 = strResult.IndexOf("value=\"", iStart);
                if (i1 < 0)
                    return;
                int i2 = strResult.IndexOf("\" />", i1);
                if (i2 < 0)
                    return;

                int ilen = i2 - i1 - 7;
                strTicketLeft = strResult.Substring(i1 + 7, ilen);

                iStart = strResult.IndexOf("org.apache.struts.taglib.html.TOKEN", 0);
                if (iStart < 0)
                    return;
                i1 = strResult.IndexOf("value=\"", iStart);
                if (i1 < 0)
                    return;
                i2 = strResult.IndexOf("\">", i1);
                if (i2 < 0)
                    return;
                ilen = i2 - i1 - 7;
                strToken = strResult.Substring(i1 + 7, ilen);
                // org.apache.struts.taglib.html.TOKEN
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        // 查询Z38
        private void QueryZ38()
        {
            strypDetail = strmmStr = "";
            //// http://dynamic.12306.cn/otsweb/order/querySingleAction.do?method=queryLeftTicket&orderRequest.train_date=2012-10-01&orderRequest.from_station_telecode=WHN&orderRequest.to_station_telecode=BJP&orderRequest.train_no=3900000Z3821&trainPassType=QB&trainClass=QB%23D%23Z%23T%23K%23QT%23&includeStudent=00&seatTypeAndNum=&orderRequest.start_time_str=00%3A00--24%3A00
            //// refer http://dynamic.12306.cn/otsweb/order/querySingleAction.do?method=init
            string strURL = "http://dynamic.12306.cn/otsweb/order/querySingleAction.do?method=queryLeftTicket&orderRequest.train_date=" + strQueryDate + "&orderRequest.from_station_telecode=WHN&orderRequest.to_station_telecode=BJP&orderRequest.train_no=3900000Z3821&trainPassType=QB&trainClass=QB%23D%23Z%23T%23K%23QT%23&includeStudent=00&seatTypeAndNum=&orderRequest.start_time_str=00%3A00--24%3A00";
            string strRefer = "http://dynamic.12306.cn/otsweb/order/querySingleAction.do?method=init";
            string strResult = GetHttpRequest(strURL, strRefer);
            ////string strResult = "0,<span id='id_3900000Z3821' class='base_txtdiv' onmouseover=javascript:onStopHover('3900000Z3821#WCN#BXP') onmouseout='onStopOut()'>Z38</span>,<img src='/otsweb/images/tips/first.gif'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;武昌&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<br>&nbsp;&nbsp;&nbsp;&nbsp;21:03,<img src='/otsweb/images/tips/last.gif'>&nbsp;&nbsp;&nbsp;&nbsp;北京西&nbsp;&nbsp;&nbsp;&nbsp;<br>&nbsp;&nbsp;&nbsp;&nbsp;07:00,09:57,--,--,--,--,2,11,<font color='darkgray'>无</font>,--,--,--,--,<input type='button' class='yuding_u' onmousemove=this.className='yuding_u_over' onmousedown=this.className='yuding_u_down' onmouseout=this.className='yuding_u' onclick=javascript:getSelected('Z38#09:57#21:03#3900000Z3821#WCN#BXP#07:00#武昌#北京西#6*****00024*****00113*****0000#F2D3A3C67807B84F8CB9B825359E562DC36DBCBD09520BAD7C8C49FB') value='预订'></input>";
            try
            {
                int iStart = strResult.IndexOf("getSelected(", 0);
                if (iStart < 0)
                {
                    MessageBox.Show("没票");
                    return;
                }
                int i1 = strResult.IndexOf("') value=", iStart);
                if (i1 < 0)
                    return;
                int ilen = i1 - iStart - 12;
                string strMid = strResult.Substring(iStart + 12, ilen);

                string[] strInfo = strMid.Split('#');
                if (strInfo.Length < 11)
                    return;
                strypDetail = strInfo[9];
                strmmStr = strInfo[10];
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            // ypdetail
            // mmstr
        }

        private string GetHttpRequest(string strURL, string strRefer)
        {
            string strResult = "";
            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(strURL);
            httpReq.Timeout = 10 * 1000;
            httpReq.Accept = "*/*";
            httpReq.Method = "GET";
            httpReq.CookieContainer = cookies;
            if (strRefer.Length > 0)
                httpReq.Referer = strRefer;

            try
            {
                HttpWebResponse httpRes = (HttpWebResponse)httpReq.GetResponse();
                Stream preLoginStream = httpRes.GetResponseStream();
                StreamReader sr = new StreamReader(preLoginStream);
                strResult = sr.ReadToEnd();
                httpRes.Close();
                sr.Close();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return strResult;
        }

        private void SubmutOrderZ38()
        {
            //http://dynamic.12306.cn/otsweb/order/querySingleAction.do?method=submutOrderRequest

            string strURL = @"http://dynamic.12306.cn/otsweb/order/querySingleAction.do?method=submutOrderRequest";
            HttpWebRequest postReq = (HttpWebRequest)WebRequest.Create(strURL);
            postReq.Timeout = 10 * 1000; // 10s 超时
            postReq.Accept = "*/*";
            postReq.Method = "POST";
            postReq.CookieContainer = cookies;
            postReq.ContentType = "application/x-www-form-urlencoded";
            //postReq.Referer = "http://dynamic.12306.cn/otsweb/loginAction.do?method=init";

            // 填充post数据

            StringBuilder sb = new StringBuilder();
            //loginRand=515&refundLogin=N&refundFlag=Y&loginUser.user_name=kiddove&nameErrorFocus=&user.password=jason55&passwordErrorFocus=&randCode=n22c&randErrorFocus=
            sb.AppendFormat("{0}={1}", "station_train_code", "Z38");
            sb.AppendFormat("&{0}={1}", "train_date", "2012-9-27");
            sb.AppendFormat("&{0}={1}", "seattype_num", "");
            sb.AppendFormat("&{0}={1}", "from_station_telecode", "WCN");
            sb.AppendFormat("&{0}={1}", "to_station_telecode", "BXP");
            sb.AppendFormat("&{0}={1}", "include_student", "00");
            sb.AppendFormat("&{0}={1}", "from_station_telecode_name", "武汉");
            sb.AppendFormat("&{0}={1}", "to_station_telecode_name", "北京");
            sb.AppendFormat("&{0}={1}", "round_train_date", "2012-9-27");
            sb.AppendFormat("&{0}={1}", "round_start_time_str", "00:00--24:00");
            sb.AppendFormat("&{0}={1}", "single_round_type", "1");
            sb.AppendFormat("&{0}={1}", "train_pass_type", "QB");
            sb.AppendFormat("&{0}={1}", "train_class_arr", "QB#D#Z#T#K#QT#");
            sb.AppendFormat("&{0}={1}", "start_time_str", "00:00--24:00");
            sb.AppendFormat("&{0}={1}", "lishi", "09:57");
            sb.AppendFormat("&{0}={1}", "train_start_time", "21:03");
            sb.AppendFormat("&{0}={1}", "trainno4", "3900000Z3821");
            sb.AppendFormat("&{0}={1}", "arrive_time", "07:00");
            sb.AppendFormat("&{0}={1}", "from_station_name", "武昌");
            sb.AppendFormat("&{0}={1}", "to_station_name", "北京西");
            sb.AppendFormat("&{0}={1}", "ypInfoDetail", strypDetail);
            sb.AppendFormat("&{0}={1}", "mmStr", strmmStr);

            byte[] postData = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            try
            {
                Stream sm = postReq.GetRequestStream();
                sm.Write(postData, 0, postData.Length);
                sm.Close();
                HttpWebResponse postRes = (HttpWebResponse)postReq.GetResponse();

                Stream smRet = postRes.GetResponseStream();
                StreamReader sr = new StreamReader(smRet);
                string strRet = sr.ReadToEnd();
                postRes.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void OrderZ38()
        {
            string strURL = @"http://dynamic.12306.cn/otsweb/order/confirmPassengerAction.do?method=confirmSingleForQueueOrder";
            HttpWebRequest postReq = (HttpWebRequest)WebRequest.Create(strURL);
            postReq.Timeout = 10 * 1000; // 10s 超时
            postReq.Accept = "*/*";
            postReq.Method = "POST";
            postReq.CookieContainer = cookies;
            postReq.ContentType = "application/x-www-form-urlencoded";
            postReq.Referer = "http://dynamic.12306.cn/otsweb/order/confirmPassengerAction.do?method=init";

            // 填充post数据

            StringBuilder sb = new StringBuilder();
            //loginRand=515&refundLogin=N&refundFlag=Y&loginUser.user_name=kiddove&nameErrorFocus=&user.password=jason55&passwordErrorFocus=&randCode=n22c&randErrorFocus=
            sb.AppendFormat("{0}={1}", "org.apache.struts.taglib.html.TOKEN", strToken);
            sb.AppendFormat("&{0}={1}", "leftTicketStr", strTicketLeft);
            sb.AppendFormat("&{0}={1}", "textfield", "中文或拼音首字母");
            sb.AppendFormat("&{0}={1}", "checkbox0", "0");
            sb.AppendFormat("&{0}={1}", "checkbox2", "2");
            sb.AppendFormat("&{0}={1}", "orderRequest.train_date", strOrderDate);
            sb.AppendFormat("&{0}={1}", "orderRequest.train_no", "3900000Z3821");
            sb.AppendFormat("&{0}={1}", "orderRequest.station_train_code", "Z38");
            sb.AppendFormat("&{0}={1}", "orderRequest.from_station_telecode", "WCN");
            sb.AppendFormat("&{0}={1}", "orderRequest.to_station_telecode", "BXP");
            sb.AppendFormat("&{0}={1}", "orderRequest.seat_type_code", "");
            sb.AppendFormat("&{0}={1}", "orderRequest.seat_detail_type_code", "");
            sb.AppendFormat("&{0}={1}", "orderRequest.ticket_type_order_num", "");
            sb.AppendFormat("&{0}={1}", "orderRequest.bed_level_order_num", "000000000000000000000000000000");
            sb.AppendFormat("&{0}={1}", "orderRequest.start_time", "21:03");
            sb.AppendFormat("&{0}={1}", "orderRequest.end_time", "07:00");
            sb.AppendFormat("&{0}={1}", "orderRequest.from_station_name", "武昌");
            sb.AppendFormat("&{0}={1}", "orderRequest.to_station_name", "北京西");
            sb.AppendFormat("&{0}={1}", "orderRequest.cancel_flag", "1");
            sb.AppendFormat("&{0}={1}", "orderRequest.id_mode", "Y");
            sb.AppendFormat("&{0}={1}", "passengerTickets", "4,0,1,周鹏,1,420111198304077314,13146949593,Y");
            sb.AppendFormat("&{0}={1}", "oldPassengers", "周鹏,1,420111198304077314");
            sb.AppendFormat("&{0}={1}", "passenger_1_seat", "4");
            sb.AppendFormat("&{0}={1}", "passenger_1_seat_detail_select", "0");
            sb.AppendFormat("&{0}={1}", "passenger_1_seat_detail", "0");
            sb.AppendFormat("&{0}={1}", "passenger_1_ticket", "1");
            sb.AppendFormat("&{0}={1}", "passenger_1_name", "周鹏");
            sb.AppendFormat("&{0}={1}", "passenger_1_cardtype", "1");
            sb.AppendFormat("&{0}={1}", "passenger_1_cardno", "420111198304077314");
            sb.AppendFormat("&{0}={1}", "passenger_1_mobileno", "13146949593");
            sb.AppendFormat("&{0}={1}", "checkbox9", "Y");
            sb.AppendFormat("&{0}={1}", "passengerTickets", "4,0,1,张文,1,420111198310255543,13810297079,Y");
            sb.AppendFormat("&{0}={1}", "oldPassengers", "张文,1,420111198310255543");
            sb.AppendFormat("&{0}={1}", "passenger_2_seat", "4");
            sb.AppendFormat("&{0}={1}", "passenger_2_seat_detail_select", "0");
            sb.AppendFormat("&{0}={1}", "passenger_2_seat_detail", "0");
            sb.AppendFormat("&{0}={1}", "passenger_2_ticket", "1");
            sb.AppendFormat("&{0}={1}", "passenger_2_name", "张文");
            sb.AppendFormat("&{0}={1}", "passenger_2_cardtype", "1");
            sb.AppendFormat("&{0}={1}", "passenger_2_cardno", "420111198310255543");
            sb.AppendFormat("&{0}={1}", "passenger_2_mobileno", "13810297079");
            sb.AppendFormat("&{0}={1}", "checkbox9", "Y");
            sb.AppendFormat("&{0}={1}", "oldPassengers", "");
            sb.AppendFormat("&{0}={1}", "checkbox9", "Y");
            sb.AppendFormat("&{0}={1}", "oldPassengers", "");
            sb.AppendFormat("&{0}={1}", "checkbox9", "Y");
            sb.AppendFormat("&{0}={1}", "oldPassengers", "");
            sb.AppendFormat("&{0}={1}", "checkbox9", "Y");
            sb.AppendFormat("&{0}={1}", "randCode", strOrderCode);
            sb.AppendFormat("&{0}={1}", "orderRequest.reserve_flag", "A");

            //string str = System.Web.HttpUtility.UrlEncode(sb.ToString(), Encoding.UTF8);
            //byte[] postData = System.Text.Encoding.UTF8.GetBytes(str);

            byte[] postData = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            try
            {
                Stream sm = postReq.GetRequestStream();
                sm.Write(postData, 0, postData.Length);
                sm.Close();
                HttpWebResponse postRes = (HttpWebResponse)postReq.GetResponse();

                Stream smRet = postRes.GetResponseStream();
                StreamReader sr = new StreamReader(smRet);
                string strRet = sr.ReadToEnd();
                postRes.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void buttonOrder_Click(object sender, EventArgs e)
        {
            strOrderCode = textBoxOrderCode.Text;
            //查余票
            string strURL = "http://dynamic.12306.cn/otsweb/order/confirmPassengerAction.do?method=getQueueCount&train_date=" + strOrderDate + "&station=Z38&seat=4&from=WCN&to=BXP&ticket=" + strTicketLeft;
            string strRefer = "http://dynamic.12306.cn/otsweb/order/confirmPassengerAction.do?method=init#";
            string strResult = GetHttpRequest(strURL, strRefer);

            try
            {
                JObject o = JObject.Parse(strResult);
                // {"countT":0,"count":48,"ticket":34,"op_1":true,"op_2":false}
                int ticketcount = (int)o["ticket"];
                Boolean op_1 = (Boolean)o["op_1"];

                if (op_1 == true && ticketcount > 1)
                {
                    // 有票
                    // 下单
                    // 循环
                    OrderZ38();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
