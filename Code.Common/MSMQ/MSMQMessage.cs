using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Common.MSMQ
{
    public class MSMQMessage
    {

        public static void SendMessage(string data)
        {
            //System.Net.ServicePointManager.Expect100Continue = false;

            //string SendMessagepath = AppSettingUtility.SendMessagePath;
            //Uri address = new Uri(SendMessagepath);

            //// Create the web request
            //HttpWebRequest request = WebRequest.Create(address) as HttpWebRequest;
            //// Set type to POST  
            //request.Method = "POST";
            //request.ContentType = "application/json";
            //// Create a byte array of the data we want to send  
            //byte[] byteData = UTF8Encoding.UTF8.GetBytes(data);

            //// Set the content length in the request headers  
            //request.ContentLength = byteData.Length;

            //// Write data  
            //using (Stream postStream = request.GetRequestStream())
            //{
            //    postStream.Write(byteData, 0, byteData.Length);
            //}

            //// Get response  
            //using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            //{
            //    // Get the response stream  
            //    StreamReader reader = new StreamReader(response.GetResponseStream());

            //    // Console application output  
            //    var a = reader.ReadToEnd();
            //    //Console.WriteLine(reader.ReadToEnd());
            //}

        }
        //public static string GetMessage(Message message)
        //{
        //    //序列化数据
        //    return JsonConvert.SerializeObject(message);

        //}
    }
}
