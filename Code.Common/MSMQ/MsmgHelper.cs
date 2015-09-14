using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Code.Common.MSMQ
{
    public class MsmgHelper
    {

        private string _ServiceName;

        /// <summary>
        /// 消息队列帮助类构造函数
        /// </summary>
        /// <param name="serverName"></param>
        public MsmgHelper(string serverName)
        {
            _ServiceName = serverName;
        }

        /// <summary>
        /// 发送消息队列
        /// </summary>
        /// <param name="t">消息</param>
        public void Send<T>(T t)
        {

            Send<T>(t, new BinaryMessageFormatter(), false);
        }

        /// <summary>
        /// 发送消息队列
        /// </summary>
        /// <param name="t">消息</param>
        /// <param name="messageFormatter"></param>
        public void Send<T>(T t, IMessageFormatter messageFormatter)
        {

            Send<T>(t, messageFormatter, false);
        }

        /// <summary>
        /// 发送消息队列
        /// </summary>
        /// <param name="t">消息</param>
        /// <param name="messageFormatter"></param>
        /// <param name="userTransaction"></param>
        public void Send<T>(T t, IMessageFormatter messageFormatter, bool userTransaction)
        {

            //连接到本地的队列
            MessageQueue myQueue = new MessageQueue(_ServiceName);

            Message myMessage = new Message();
            myMessage.Body = t;
            if (messageFormatter != null) myMessage.Formatter = messageFormatter;

            if (userTransaction)
            {
                var myTransaction = new MessageQueueTransaction();
                myTransaction.Begin();
                //发送消息到队列中
                myQueue.Send(myMessage, myTransaction);

                myTransaction.Commit();
            }
            else
            {
                //发送消息到队列中
                myQueue.Send(myMessage);
            }


        }

        /// <summary>
        /// 获取消息队列
        /// </summary>
        /// <returns></returns>
        public T Recive<T>(TimeSpan timeSpan)
        {
            return Recive<T>(new BinaryMessageFormatter(), timeSpan);

        }

        /// <summary>
        /// 获取消息队列
        /// </summary>
        /// <param name="messageFormatter"></param>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public T Recive<T>(IMessageFormatter messageFormatter, TimeSpan timeSpan)
        {

            MessageQueue myQueue = new MessageQueue(_ServiceName);

            if (messageFormatter != null) myQueue.Formatter = messageFormatter;

            if (myQueue.CanRead)
            {
                //从队列中接收消息
                Message myMessage = null;

                try
                {
                    myMessage = myQueue.Receive(timeSpan);
                }
                catch (MessageQueueException e)
                {
                    // Handle no message arriving in the queue.
                    if (e.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                    {
                        return default(T);
                    }
                    else
                    {
                        throw;
                    }
                }

                if (myMessage == null) return default(T);

                return (T)myMessage.Body;
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// 获取消息队列
        /// </summary>
        /// <returns></returns>
        public T Recive<T>()
        {
            return Recive<T>(new BinaryMessageFormatter());

        }

        /// <summary>
        /// 获取消息队列
        /// </summary>
        /// <param name="messageFormatter"></param>
        /// <returns></returns>
        public T Recive<T>(IMessageFormatter messageFormatter)
        {

            MessageQueue myQueue = new MessageQueue(_ServiceName);

            if (messageFormatter != null) myQueue.Formatter = messageFormatter;

            if (myQueue.CanRead)
            {
                //从队列中接收消息
                Message myMessage = myQueue.Receive(new TimeSpan(0, 0, 5));

                return (T)myMessage.Body;
            }
            else
            {
                return default(T);
            }
        }
        /// <summary>
        /// 查看队列中是否有消息
        /// </summary>
        /// <returns></returns>
        public bool Any()
        {
            MessageQueue myQueue = new MessageQueue(_ServiceName);
            return myQueue.GetAllMessages().Any();

        }
        /// <summary>
        /// 获取当前消息队列消息总数量
        /// </summary>
        /// <returns></returns>
        public long GetCurrQueueMsgCount()
        {
            //只要改第三个参数, [hostname]\private$\[queuename]
            PerformanceCounter objCounter = new PerformanceCounter("MSMQ Queue", "Messages in Queue", _ServiceName);//.Replace("FormatName:DIRECT=TCP:", ""));
            return long.Parse(objCounter.NextValue().ToString());
        }
    }
}
