using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Code.Common.Http
{
    public enum Method { GET, POST, PUT, DELETE };
    public class WebPublic
    {
        /// <summary>
        /// 截取字符串
        /// </summary>
        /// <param name="val"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static bool CutOff(string val, int length)
        {
            if (string.IsNullOrEmpty(val) || length == 0) return false;

            if (val.Length > length)
            {
                val = val.Substring(0, length);
                return true;
            }

            return false;
        }


        /// <summary>
        /// 作者：邓佑华
        /// 时间：2012-5-18
        /// 功能：按字节长度截取字符串 适应中文 
        /// </summary>
        /// <param name="scrString">要截断的字符串</param>
        /// <param name="byteLength">截断的字节长度</param>
        /// <param name="addString">截断后末尾追加的字符串</param>
        /// <returns></returns>
        public static string CutStringByBytesLength(string scrString, int byteLength, string addString)
        {

            if (string.IsNullOrEmpty(scrString)) return string.Empty;
            scrString = scrString.Replace(" ", "");
            string returnString = String.Empty;
            byte[] buffer = System.Text.Encoding.GetEncoding("gb2312").GetBytes(scrString);
            byte[] res = new byte[buffer.Length];

            int upBound = 0;

            if (byteLength >= buffer.Length)
            {
                upBound = buffer.Length;
                addString = String.Empty;
            }
            else
            {
                upBound = byteLength;
            }

            int byteTrueLength = -1;
            for (int i = 0; i < upBound; i++)
            {
                if (buffer[i] < 128)
                {
                    res[i] = buffer[i];
                }
                else
                {
                    if (i + 1 < byteLength)
                    {
                        res[i] = buffer[i++];
                        res[i] = buffer[i];
                    }
                    else
                    {
                        break;
                    }
                }
                byteTrueLength = i;
            }
            //0开始的  但是这长度
            if (byteTrueLength > -1)
            {
                byteTrueLength++;
                byte[] trueRes = new byte[byteTrueLength];
                for (int i = 0; i < trueRes.Length; i++)
                {
                    trueRes[i] = res[i];
                }
                returnString = System.Text.Encoding.GetEncoding("gb2312").GetString(trueRes) + addString;
            }
            return returnString;
        }

        /// <summary>
        /// 作者：邓佑华
        /// 时间：2012-5-17
        /// 功能：截取字符串
        /// </summary>
        /// <param name="val">原始字符串</param>
        /// <param name="length">截取长度</param>
        /// <returns></returns>
        public static string CutOffString(string val, int length)
        {
            string strRes = null;
            if (!string.IsNullOrEmpty(val) && length > 0)
            {
                if (val.Length > length)
                {
                    strRes = val.Substring(0, length) + "...";
                }
                else
                {
                    strRes = val;
                }
            }
            return strRes;
        }

        /// <summary>
        /// 作者：jianquan.zheng
        /// 时间：2012-08-13
        /// 功能：截取字符串,区分中英文
        /// </summary>
        /// <param name="s">原始字符串</param>
        /// <param name="l">截取长度(如入英文需要长度，两个英文等于一个中文)</param>
        /// <returns></returns>
        public static string CutOffStr(string s, int l)
        {
            string temp = s;
            if (Regex.Replace(temp, "[\u4e00-\u9fa5]", "zz", RegexOptions.IgnoreCase).Length <= l)
            {
                return temp;
            }
            for (int i = temp.Length; i >= 0; i--)
            {
                temp = temp.Substring(0, i);
                if (Regex.Replace(temp, "[\u4e00-\u9fa5]", "zz", RegexOptions.IgnoreCase).Length <= l - 3)
                {
                    return temp + "...";
                }
            }
            return "...";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encoding"></param>
        /// <param name="timeout">超时时间.默认十秒</param>
        /// <returns></returns>
        public static string HttpRequest(string url, Encoding encoding = null)
        {
            return HttpRequest(url, encoding, true);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encoding"></param>
        /// <param name="hadProxy"></param>
        /// <param name="timeout">超时时间.默认十秒</param>
        /// <returns></returns>
        public static string HttpRequest(string url, Encoding encoding, bool hadProxy)
        {
            #region old
            /*try
            {
                HttpWebRequest wReq = (HttpWebRequest)WebRequest.Create(url);
                wReq.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; Maxthon; .NET CLR 1.1.4322)";

                if (AppSettingUtility.IsOpenProxy)
                {
                    if (hadProxy)
                    {
                        wReq.Proxy = new WebProxy(AppSettingUtility.WebProxy);
                    }
                }


                // Get the response instance.
                WebResponse wResp = wReq.GetResponse();
                Stream respStream = wResp.GetResponseStream();
                // Dim reader As StreamReader = New StreamReader(respStream)
                using (System.IO.StreamReader reader = new System.IO.StreamReader(respStream, encoding))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                //var message = string.Format("HttpRequest出错，url:{0}", url);
                //LogHelper.Error(message, ex);
                LogHelper.Error(new ExceptionForHttpRequest(url, ex));
                return "";
            }*/
            #endregion
            return HttpRequest(Method.GET, url, null, hadProxy, null, encoding);
        }


        /// <summary>
        /// HTTP请求
        /// </summary>
        /// <param name="method"></param>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="hadProxy"></param>
        /// <param name="header"></param>
        /// <param name="encoding"></param>
        /// <param name="timeout">超时时间.默认十秒</param>
        /// <returns></returns>
        public static string HttpRequest(Method method, string url, string data, bool hadProxy, string header = null, Encoding encoding = null)
        {
            HttpWebRequest webRequest = null;

            //url = "http://www.baidu.com";

            StreamWriter sw = null;
            webRequest = WebRequest.Create(url) as HttpWebRequest;

            //webRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; Maxthon; .NET CLR 1.1.4322)";
            webRequest.Method = method.ToString();

            //webRequest.Timeout = timeout;
            //if (AppSettingUtility.IsOpenProxy)
            //{
            //    if (hadProxy)
            //    {
            //        webRequest.Proxy = new WebProxy(AppSettingUtility.WebProxy);
            //    }
            //}
            //webRequest.Proxy = new WebProxy("http://192.168.20.6:3128");
            if (!String.IsNullOrEmpty(header))
                webRequest.Headers.Add(header);
            if (method == Method.POST || method == Method.PUT)
            {
                webRequest.KeepAlive = true;

                webRequest.ContentType = "application/x-www-form-urlencoded";
                sw = new StreamWriter(webRequest.GetRequestStream());
                try
                {
                    sw.Write(data);
                }
                catch
                {
                    //throw;
                }
                finally
                {
                    sw.Close();
                    sw = null;
                }
            }

            string resultData = GetData(webRequest, encoding);
            webRequest = null;
            return resultData;
        }

        public static string HttpRequest(string url, Encoding encoding, int timeout = 10000)
        {
            return HttpRequest(url, encoding, true, timeout);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encoding"></param>
        /// <param name="hadProxy"></param>
        /// <param name="timeout">超时时间.默认十秒</param>
        /// <returns></returns>
        public static string HttpRequest(string url, Encoding encoding, bool hadProxy, int timeout = 10000)
        {
            return HttpRequest(Method.GET, url, null, hadProxy, null, encoding, timeout);
        }

        /// <summary>
        /// HTTP请求
        /// </summary>
        /// <param name="method"></param>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="hadProxy"></param>
        /// <param name="header"></param>
        /// <param name="encoding"></param>
        /// <param name="timeout">超时时间.默认十秒</param>
        /// <param name="contentType">慎用啊，我没检查格式，一定要自己检测</param>
        /// <returns></returns>
        public static string HttpRequest(Method method, string url, string data, string header = null, Encoding encoding = null, string contentType = "application/x-www-form-urlencoded")
        {
            HttpWebRequest webRequest = null;

            //url = "http://www.baidu.com";

            StreamWriter sw = null;
            webRequest = WebRequest.Create(url) as HttpWebRequest;

            //webRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; Maxthon; .NET CLR 1.1.4322)";
            webRequest.Method = method.ToString();
            //webRequest.Proxy = new WebProxy("http://192.168.20.6:3128");
            if (!String.IsNullOrEmpty(header))
                webRequest.Headers.Add(header);
            if (method == Method.POST || method == Method.PUT)
            {
                webRequest.KeepAlive = true;

                webRequest.ContentType = string.IsNullOrEmpty(contentType) ? "application/x-www-form-urlencoded" : contentType;
                sw = new StreamWriter(webRequest.GetRequestStream());
                try
                {
                    sw.Write(data);
                }
                catch
                {
                    //throw;
                }
                finally
                {
                    sw.Close();
                    sw = null;
                }
            }

            string resultData = GetData(webRequest, encoding);
            webRequest = null;
            return resultData;
        }
        private static void SetCache(Method method, string url, string data, bool hadProxy, string header, Encoding encoding = null, int cacheTime = 1800, int timeout = 10000, bool isSendMessage = true)
        {
            //string result = string.Empty;
            //string key = url.GetMd5();

            //result = HttpRequest(Method.GET, url, null, hadProxy, null, encoding, timeout);
            //if (!string.IsNullOrEmpty(result) && result.IsXml())
            //{
            //    CacheFactory.GetInstance().Set(CacheKeyHelper.CreateKeyWithSuffix(CacheType.html, CacheFunctionType.others,
            //                                                        ModuleType.httprequestchche, key), result, DateTime.Now.AddSeconds(cacheTime));
            //    FileHelper.GetInstance().WriteFile(GetCacheDirectory(key), result);
            //    if (isSendMessage)
            //    {
            //        SendMessage(url, key);
            //    }
            //}
        }
        /// <summary>此处用来统计有多少缓存,清除缓存时用
        /// 此处用来统计有多少缓存,清除缓存时用
        /// </summary>
        /// <param name="url"></param>
        /// <param name="key"></param>
        private static void SendMessage(string url, string key)
        {
            //MsmgHelper msmgHelper = new MsmgHelper(AppSettingUtility.CampusBIImportMsgServer);
            //string msg = string.Format("Cache|{0}|{1}|{2}", url, key, DateTime.Now);
            //var formatter = new System.Messaging.ActiveXMessageFormatter();
            //msmgHelper.Send<string>(msg, formatter, true);
        }

        //得到缓存的目录
        private static string GetCacheDirectory(string key)
        {
            string cacheDirectory = string.Empty;
            //if (!string.IsNullOrEmpty(AppSettingUtility.PageCacheAbsolutePath))
            //{
            //    cacheDirectory = AppSettingUtility.PageCacheAbsolutePath;
            //    cacheDirectory = Path.Combine(cacheDirectory, key + ".html");
            //}

            return cacheDirectory;
        }

        private delegate void AsyncDelegate(Method method, string url, string data, bool hadProxy, string header, Encoding encoding = null, int cacheTime = 1800, int timeout = 10000, bool isSendMessage = true);
        /// <summary>
        /// HTTP请求
        /// </summary>
        /// <param name="method"></param>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="hadProxy"></param>
        /// <param name="header"></param>
        /// <param name="encoding"></param>
        /// <param name="timeout">超时时间.默认十秒</param>
        /// <returns></returns>
        public static string HttpRequest(Method method, string url, string data, bool hadProxy, string header, Encoding encoding = null, int timeout = 10000)
        {
            HttpWebRequest webRequest = null;

            StreamWriter sw = null;
            webRequest = WebRequest.Create(url) as HttpWebRequest;

            //webRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; Maxthon; .NET CLR 1.1.4322)";
            webRequest.Method = method.ToString();
            webRequest.Timeout = timeout;
            //if (AppSettingUtility.IsOpenProxy)
            //{
            //    if (hadProxy)
            //    {
            //        webRequest.Proxy = new WebProxy(AppSettingUtility.WebProxy);
            //    }
            //}
            //webRequest.Proxy = new WebProxy("http://192.168.20.6:3128");
            if (!String.IsNullOrEmpty(header))
                webRequest.Headers.Add(header);
            if (method == Method.POST)
            {
                webRequest.KeepAlive = true;

                webRequest.ContentType = "application/x-www-form-urlencoded";
                sw = new StreamWriter(webRequest.GetRequestStream());
                try
                {
                    sw.Write(data);
                }
                catch
                {
                    //throw;
                }
                finally
                {
                    sw.Close();
                    sw = null;
                }
            }

            string resultData = GetData(webRequest, encoding);
            webRequest = null;
            return resultData;
        }

        /// <summary>
        /// creater:wenjun.cheng
        /// date:2013-5-21
        /// Get方式向服务器获取数据
        /// </summary>
        /// <param name="webRequest">http请求流</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        private static string GetData(HttpWebRequest webRequest, Encoding encoding)
        {
            StreamReader responseReader = null;
            string responseData = String.Empty;
            HttpWebResponse httpWeb = null;
            try
            {
                //向服务器的请求都用流的形式写入或读取
                httpWeb = (HttpWebResponse)webRequest.GetResponse();
                responseReader = new StreamReader(httpWeb.GetResponseStream(), encoding ?? Encoding.Default);
                responseData = responseReader.ReadToEnd();
            }
            catch (Exception e)
            {
                //LogHelper.Error(e);
            }
            finally
            {
                //关闭,此处若有null会报错
                if (httpWeb != null && httpWeb.GetResponseStream() != null)
                {
                    httpWeb.GetResponseStream().Close();
                }
                if (responseReader != null)
                {
                    responseReader.Close();
                }

                responseReader = null;
            }

            return responseData;
        }

        /// <summary>
        /// 作者：邓佑华
        /// 时间：2012-5-15
        /// 功能：替换字符串
        /// </summary>
        /// <param name="strInfo">原字符串</param>
        /// <param name="strOld">需要替换的字符串</param>
        /// <param name="strNew">替换成该字符串</param>
        /// <returns></returns>
        public static string Replace(string strInfo, string strOld, string strNew)
        {
            string strRes = null;
            if (!string.IsNullOrEmpty(strInfo) && strInfo.Trim().Length > 0)
            {
                strRes = strInfo.Replace(strOld, strNew);
            }
            return strRes;
        }
        /// <summary>
        /// 作者：邓佑华
        /// 时间：2012-5-15
        /// 功能：将html实体名（实体号）转换为显示标记
        /// </summary>
        /// <param name="strInfo">原字符串</param>
        /// <returns></returns>
        public static string ReplaceForHtml(string strInfo)
        {
            string strRes = null;
            if (!string.IsNullOrEmpty(strInfo) && strInfo.Trim().Length > 0)
            {
                strRes = strInfo.Replace("&lt;", "<");
                strRes = strRes.Replace("&#60;", "<");
                strRes = strRes.Replace("&gt;", ">");
                strRes = strRes.Replace("&#62;", ">");
            }
            return strRes;
        }
        /// <summary>
        /// 作者：邓佑华
        /// 时间：2012-5-16
        /// 功能：去除字符串中的html标记(返回值：null或字符串)
        /// </summary>
        /// <param name="Htmlstring">需要去除标记的字符串</param>
        /// <returns>null或字符串</returns>
        public static string NoHtml(string Htmlstring) //去除HTML标记   
        {
            string strRes = null;
            if (!string.IsNullOrEmpty(Htmlstring))
            {
                //处理字符串
                Htmlstring = Regex.Replace(Htmlstring, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
                Htmlstring = Regex.Replace(Htmlstring, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
                Htmlstring = Regex.Replace(Htmlstring, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
                Htmlstring = Regex.Replace(Htmlstring, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
                Htmlstring = Regex.Replace(Htmlstring, @"&(nbsp|#160);", " ", RegexOptions.IgnoreCase);
                Htmlstring = Regex.Replace(Htmlstring, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
                Htmlstring = Regex.Replace(Htmlstring, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
                Htmlstring = Regex.Replace(Htmlstring, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
                Htmlstring = Regex.Replace(Htmlstring, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
                Htmlstring = Regex.Replace(Htmlstring, @"&#(\d+);", "", RegexOptions.IgnoreCase);

                //删除脚本   
                Htmlstring = Regex.Replace(Htmlstring, @"<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);
                //删除HTML   
                Htmlstring = Regex.Replace(Htmlstring, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);
                Htmlstring = Regex.Replace(Htmlstring, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
                Htmlstring = Regex.Replace(Htmlstring, @"-->", "", RegexOptions.IgnoreCase);
                Htmlstring = Regex.Replace(Htmlstring, @"<!--.*", "", RegexOptions.IgnoreCase);

                Htmlstring = Htmlstring.Replace("<", "");
                Htmlstring = Htmlstring.Replace(">", "");
                Htmlstring = Htmlstring.Replace("\r\n", "");

                strRes = Htmlstring.Trim();
            }
            return strRes;
        }

        /// <summary>
        /// Creator:light.hao
        /// Date:2013.4.27
        /// Fun:过滤字符串中的html标签，可排除一些标签不过滤
        /// </summary>
        /// <param name="Htmlstring">要过滤的字符串</param>
        /// <param name="acceptags">这些html标签不需要过滤，例如：script|link|title</param>
        /// <returns></returns>
        //public static string NoHtml(string Htmlstring, string acceptags) //去除HTML标记   
        //{
        //    string strRes = null;
        //    if (!string.IsNullOrEmpty(Htmlstring))
        //    {
        //        string stringPattern = @"</?(?(?=" + acceptags +
        //                               @")notag|[a-zA-Z0-9]+)(?:\s[a-zA-Z0-9\-]+=?(?:([""']?).*?\1?)?)*\s*/?>";
        //        strRes = Regex.Replace(Htmlstring, stringPattern, "");
        //    }
        //    return strRes;
        //}

        public static string NoHtml(string Htmlstring, string acceptags) //去除HTML标记   
        {
            Htmlstring = Htmlstring.Replace("<br>", "newline");
            //删除脚本
            Htmlstring = Regex.Replace(Htmlstring, @"<script[^>]*?>.*?</script>", "",
            RegexOptions.IgnoreCase);
            //删除HTML 
            Htmlstring = Regex.Replace(Htmlstring, @"<(.[^>]*)>", "",
            RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"([\r\n])[\s]+", "",
            RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"–>", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"<!–.*", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(quot|#34);", "\"",
            RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(amp|#38);", "&",
            RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(lt|#60);", "<",
            RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(gt|#62);", ">",
            RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(nbsp|#160);", "   ",
            RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&#(\d+);", "", RegexOptions.IgnoreCase);
            Htmlstring.Replace("<", "");
            Htmlstring.Replace(">", "");
            Htmlstring.Replace("\r\n", "");
            Htmlstring = Htmlstring.Replace("newline", "<br>");
            //Htmlstring = HttpContext.Current.Server.HtmlEncode(Htmlstring).Trim();
            return Htmlstring;
        }

        /// <summary>
        /// Creator:light.hao
        /// Date:2013.4.27
        /// Fun:排除br标签的过滤字符串中的html标签
        /// </summary>
        /// <param name="Htmlstring"></param>
        /// <returns></returns>
        public static string NoHtmlForCompany(string Htmlstring)
        {
            return NoHtml(Htmlstring, "br|a");
        }

        /// <summary>
        /// Creator: Li Yang
        /// Data:    2012-5-16
        /// Function:过滤职位小类的生僻字，并取第一个职位小类
        /// </summary>
        /// <param name="subJobType"></param>
        /// <returns></returns>
        public static string FilterSubJobType(string subJobType)
        {
            try
            {
                var jobType = string.Empty;
                if (!string.IsNullOrEmpty(subJobType.Trim()))
                {
                    jobType = subJobType.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
                }

                return string.IsNullOrEmpty(jobType) ? "-1" : jobType.Substring(1, jobType.Length - 1);
            }
            catch (Exception ex)
            {
                return "-1";
            }
        }

        public static string NoStyle(string htmlString)
        {
            string strRes = null;
            if (!string.IsNullOrEmpty(htmlString))
            {
                string stringPattern = @"<style>[\s\S]*</style>";
                Regex.IsMatch(htmlString, stringPattern);
                strRes = Regex.Replace(htmlString, stringPattern, "");
            }
            return strRes;
        }

        #region 获取服务器ip

        /// <summary>
        /// 返回服务器IP
        /// </summary>
        /// <returns></returns>
        public static string GetServerIp()
        {
            try
            {
                IPAddress[] addressList = Dns.GetHostByName(Dns.GetHostName()).AddressList;

                if (addressList != null)
                    return addressList.Length > 0 ? addressList[0].ToString() : string.Empty;
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }

        }

        #endregion

        #region 获取字符首字母

        //得到单个字符的首字母 
        ///<summary>
        ///</summary>
        ///<param name="str"></param>
        ///<returns></returns>
        public static String GetOneIndex(String str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;

            var oneIndexTxt = str.Substring(0, 1);

            if (Convert.ToChar(oneIndexTxt) >= 0 && Convert.ToChar(oneIndexTxt) < 256)
                return oneIndexTxt;
            else
                return GetGbkX(oneIndexTxt);
        }

        //根据汉字拼音排序得到首字母 
        private static string GetGbkX(string str)
        {
            if (str.CompareTo("吖") < 0)
            {
                return str;
            }
            if (str.CompareTo("八") < 0)
            {
                return "A";
            }

            if (str.CompareTo("嚓") < 0)
            {
                return "B";
            }

            if (str.CompareTo("咑") < 0)
            {
                return "C";
            }
            if (str.CompareTo("妸") < 0)
            {
                return "D";
            }
            if (str.CompareTo("发") < 0)
            {
                return "E";
            }
            if (str.CompareTo("旮") < 0)
            {
                return "F";
            }
            if (str.CompareTo("铪") < 0)
            {
                return "G";
            }
            if (str.CompareTo("讥") < 0)
            {
                return "H";
            }
            if (str.CompareTo("咔") < 0)
            {
                return "J";
            }
            if (str.CompareTo("垃") < 0)
            {
                return "K";
            }
            if (str.CompareTo("呒") < 0)
            {
                return "L";
            }
            if (str.CompareTo("拏") < 0)
            {
                return "M";
            }
            if (str.CompareTo("噢") < 0)
            {
                return "N";
            }
            if (str.CompareTo("妑") < 0)
            {
                return "O";
            }
            if (str.CompareTo("七") < 0)
            {
                return "P";
            }
            if (str.CompareTo("亽") < 0)
            {
                return "Q";
            }
            if (str.CompareTo("仨") < 0)
            {
                return "R";
            }
            if (str.CompareTo("他") < 0)
            {
                return "S";
            }
            if (str.CompareTo("哇") < 0)
            {
                return "T";
            }
            if (str.CompareTo("夕") < 0)
            {
                return "W";
            }
            if (str.CompareTo("丫") < 0)
            {
                return "X";
            }
            if (str.CompareTo("帀") < 0)
            {
                return "Y";
            }
            if (str.CompareTo("咗") < 0)
            {
                return "Z";
            }
            return str;
        }

        #endregion
    }
}
