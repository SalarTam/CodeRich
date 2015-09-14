using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Code.Common
{
    public static class StrHelper
    {
        /// <summary>
        /// 字节截取cwj
        /// </summary>
        /// <param name="s"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static String ByteSubstring(this string s, int length)
        {
            byte[] bytes = System.Text.Encoding.Unicode.GetBytes(s);
            int n = 0;  //  表示当前的字节数
            int i = 0;  //  要截取的字节数
            for (; i < bytes.GetLength(0) && n < length; i++)
            {
                //  偶数位置，如0、2、4等，为UCS2编码中两个字节的第一个字节
                if (i % 2 == 0)
                {
                    n++;      //  在UCS2第一个字节时n加1
                }
                else
                {
                    //  当UCS2编码的第二个字节大于0时，该UCS2字符为汉字，一个汉字算两个字节
                    if (bytes[i] > 0)
                    {
                        n++;
                    }
                }
            }
            //  如果i为奇数时，处理成偶数
            if (i % 2 == 1)
            {
                //  该UCS2字符是汉字时，去掉这个截一半的汉字 
                if (bytes[i] > 0)
                    i = i - 1;
                //  该UCS2字符是字母或数字，则保留该字符
                else
                    i = i + 1;
            }
            return System.Text.Encoding.Unicode.GetString(bytes, 0, i);
        }
        /// <summary>
        /// 截取字符方法新重构
        /// </summary>
        /// <param name="s"></param>
        /// <param name="length"></param>
        /// <param name="addstr"></param>
        /// <returns></returns>
        public static String ByteSubstring(this string s, int length, string addstr)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            //s = s.Replace(" ", "");
            string returnString = String.Empty;
            byte[] buffer = System.Text.Encoding.GetEncoding("gb2312").GetBytes(s);
            var res = new byte[buffer.Length];

            int upBound = 0;

            if (length >= buffer.Length)
            {
                upBound = buffer.Length;
                addstr = String.Empty;
            }
            else
            {
                upBound = length;
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
                    if (i + 1 < length)
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
                var trueRes = new byte[byteTrueLength];
                for (int i = 0; i < trueRes.Length; i++)
                {
                    trueRes[i] = res[i];

                }
                returnString = System.Text.Encoding.GetEncoding("gb2312").GetString(trueRes) + addstr;
            }
            return returnString;
        }
        /// <summary>
        /// 去除html标签
        /// </summary>
        /// <param name="html">源html字符串</param>
        /// <param name="holdTags">需要保留的标签</param>
        /// <returns></returns>
        public static string NoHtml(this string html, string[] holdTags = null)
        {
            string strStripped = html;
            /*
            string[] aryRegex ={@"<%=[\w\W]*?%>",    @"<script[\w\W]*?</script>",     @"<style[\w\W]*?</style>",   @"<[/]?[\w\W]*?>",   @"([\r\n])[\s]+",
                              @"&(nbsp|#160);",    @"&(iexcl|#161);",               @"&(cent|#162);",            @"&(pound|#163);",   @"&(copy|#169);",
                              @"&#(\d+);",         @"-->",                          @"<!--.*\n"};
            
            string[] aryRegex ={@"<%=[\w\W]*?%>",    @"<script[\w\W]*?</script>",     @"<style[\w\W]*?</style>",     @"([\r\n])[\s]+",@"<form[\w\W]*?</form>",@"style[^><]*",
                              @"&(nbsp|#160);",    @"&(iexcl|#161);",               @"&(cent|#162);",            @"&(pound|#163);",  
                              @"&#(\d+);",         @"-->",                          @"<!--.*\n"};
            string[] aryReplacment = { "", "", "", "", "", " ", "", "", "", "", "", "", "" };
           

            for (int i = 0; i < aryRegex.Length; i++)
            {
                Regex regex = new Regex(aryRegex[i], RegexOptions.IgnoreCase);
                strStripped = regex.Replace(strStripped, aryReplacment[i]);
            }
            * */
            /* * */
            // HoldTags = { "p", "a", "ul", "li", "br", "div", "img" };//保留的关键字
            string RegStr = "";
            strStripped.Replace("\r\n", "");
            strStripped.Replace("\t", "");
            RegStr = @"<script[\w\W]*?</script>";
            strStripped = Regex.Replace(strStripped, RegStr, string.Empty, RegexOptions.IgnoreCase);
            RegStr = @"<form[\w\W]*?</form>";
            strStripped = Regex.Replace(strStripped, RegStr, string.Empty, RegexOptions.IgnoreCase);
            RegStr = @"style[^><]*";
            strStripped = Regex.Replace(strStripped, RegStr, string.Empty, RegexOptions.IgnoreCase);
            RegStr = string.Format(@"<(?!((/?\s?{0})))[^>]+>",
                                   holdTags != null ? string.Join(@"\b)|(/?\s?", holdTags) : "");
            strStripped = Regex.Replace(strStripped, RegStr, string.Empty, RegexOptions.IgnoreCase);

            return strStripped;


        }
    }
}
