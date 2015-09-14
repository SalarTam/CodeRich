using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Common.Cookie
{
    public class UserCookieEncrypt
    {
        /// <summary>
        /// 默认cookie加密密钥
        /// </summary>
        private static string[] DEFAULT_COOKIE_KEYS = {
                "sdfdsgweg",
                "asdfssgjweh",
                "asdfssgjweh",
                "asdsdfsdfssgjweh",
                "sadf",
                "asdfiulssgjweh",
                "dfmasrtdfsdfssgjweh",
                "gm765",
                "m,yuliuy4",
                "a,56i6k",
                "457hjk"};

        /// <summary>
        /// 将cookie值中的百分号(%), 逗号(,), 等号(=), 分号(;)进行编码
        /// </summary>
        /// <param name="cookieValue">cookie值</param>
        /// <returns>返回编码后的cookie值</returns>
        public static string CookieEncode(string cookieValue)
        {
            string _retValue = cookieValue.Trim();

            if (string.IsNullOrEmpty(_retValue))
                return _retValue;

            _retValue.Replace("%", "%25");
            _retValue.Replace("=", "%3d");
            _retValue.Replace(",", "%2c");
            _retValue.Replace(";", "%3b");

            return _retValue;
        }

        /// <summary>
        /// 将cookie值中的百分号(%), 逗号(,), 等号(=), 分号(;)进行解码
        /// </summary>
        /// <param name="cookieValue">cookie值</param>
        /// <returns>返回解码后的cookie值</returns>
        public static string CookieDecode(string cookieValue)
        {
            string _retValue = cookieValue.Trim();

            if (string.IsNullOrEmpty(_retValue))
                return _retValue;

            _retValue.Replace("%25", "%");
            _retValue.Replace("%3d", "=");
            _retValue.Replace("%2c", ",");
            _retValue.Replace("%3b", ";");

            return _retValue;
        }

        /// <summary>
        /// 根据cookie密钥加密cookie数据
        /// </summary>
        /// <param name="cookieValue">Cookie值明文</param>
        /// <param name="cookieKey">cookie加密密钥</param>
        /// <returns>加密的cookie数据</returns>
        private static byte[] Encrypt(string cookieValue, string cookieKey)
        {
            byte[] arrValues = Encoding.Unicode.GetBytes(cookieValue);
            if (string.IsNullOrEmpty(cookieKey.Trim()))
                return arrValues;

            byte[] arrKeys = Encoding.UTF8.GetBytes(cookieKey);
            List<byte> ret = new List<byte>();
            byte c;
            int k = 0;

            for (int i = 0; i < arrValues.Length; i++)
            {
                c = (byte)(arrValues[i] ^ arrKeys[k]);
                ret.Add(c);
                k++;
                if (k >= cookieKey.Length)
                    k = 0;
            }
            return ret.ToArray<byte>();
        }

        /// <summary>
        /// 加密数据
        /// </summary>
        /// <param name="cookieValue"></param>
        /// <returns></returns>
        public static string EncryptData(string cookieValue)
        {
            int intRnd;
            int intMaxNumber = 10;
            Random rand = new Random();
            intRnd = rand.Next(intMaxNumber);

            byte[] arrData = Encrypt(cookieValue, DEFAULT_COOKIE_KEYS[intRnd]);
            string ret = "";
            string strHex = "";
            for (int i = 0; i < arrData.Length; i++)
            {
                strHex = Convert.ToByte(arrData[i]).ToString("X2");
                ret += strHex;
            }

            if (string.IsNullOrEmpty(ret)) return string.Empty;

            return ret + intRnd.ToString();
        }

        /// <summary>
        /// 解密数据
        /// </summary>
        /// <param name="cookieValue"></param>
        /// <returns></returns>
        public static string DecryptData(string cookieValue)
        {
            if (string.IsNullOrEmpty(cookieValue))
                return string.Empty;
            int intRnd;

            string strRet = string.Empty;
            if (int.TryParse(cookieValue.Substring(cookieValue.Length - 1), out intRnd))
            {
                byte[] data = new byte[(cookieValue.Length - 1) / 2];
                int k = 0;

                for (int i = 0; i < cookieValue.Length - 1; i = i + 2)
                {
                    try
                    {
                        data[k++] = Convert.ToByte(Convert.ToInt64(cookieValue.Substring(i, 2), 16));
                    }
                    catch (Exception ex)
                    {
                        //LogHelper.Trace("cookie解密出错", new Exception(string.Format("cookie信息{0},异常信息{1}", cookieValue, ex.Message)));
                        return "";
                    }

                }
                //LogHelper.Trace("cookie解密出错", new Exception(string.Format("cookie信息{0}异常信息{1}", cookieValue, sb.ToString())));

                string strData = Encoding.Unicode.GetString(data);
                byte[] ret = Encrypt(strData, DEFAULT_COOKIE_KEYS[intRnd]);
                strRet = Encoding.Unicode.GetString(ret);
            }
            else
            {
                //LogHelper.Trace("cookie解密出错", new Exception(string.Format("cookie信息{0}", cookieValue)));
            }

            return strRet;
        }
    }
}
