using System;
using System.IO;

namespace Code.Common.File
{
    /// <summary>
    ///  单例文件读写的类
    /// </summary>
    public class FileHelper
    {
        private FileHelper()
        {

        }

        private static FileHelper instance = null;
        /// <summary>
        /// 获取实例
        /// </summary>
        /// <returns></returns>
        public static FileHelper GetInstance()
        {
            return instance ?? new FileHelper();
        }
        /// <summary>
        /// 读文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="isThrowException">是否抛出错误,便于写错误日志,默认不抛出</param>
        /// <returns></returns>
        public string FileRead(string filePath, bool isThrowException = false)
        {
            string result = string.Empty;
            try
            {
                using (var sr = new StreamReader(filePath))
                {
                    result = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                if (isThrowException)
                {
                    throw ex;
                }
                result = "";
            }

            return result;
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="isThrowException">是否抛出错误,便于写错误日志,默认不抛出</param>
        public bool DeleteFile(string filePath, bool isThrowException = false)
        {
            bool isSuccess = true;
            try
            {
                System.IO.File.Delete(filePath);
            }
            catch (Exception ex)
            {
                if (isThrowException)
                {
                    throw ex;
                }
                isSuccess = false;
            }
            return isSuccess;

        }
        /// <summary>
        /// 写入文本文件内容
        /// </summary>
        /// <param name="filePath">文件绝对路径</param>
        /// <param name="str">要写入文本</param>
        /// <param name="isThrowException">是否抛出错误,便于写错误日志,默认不抛出</param>
        /// <returns></returns>
        public bool WriteFile(string filePath, string str, bool isThrowException = false)
        {
            bool isSuccess = true;
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write,
                                                            FileShare.ReadWrite))
                {
                    using (var streamWriter = new StreamWriter(fileStream, System.Text.Encoding.UTF8))
                    {
                        streamWriter.Write(str);
                        streamWriter.Flush();
                    }

                }
            }
            catch (Exception ex)
            {
                if (isThrowException)
                {
                    throw ex;
                }
                isSuccess = false;
            }
            return isSuccess;

        }
        /// <summary>
        /// 创建或者附件文本
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="str"></param>
        /// <param name="isThrowException"></param>
        /// <returns></returns>
        public bool WriteOrAppendFile(string filePath, string str, bool isThrowException = false)
        {
            bool isSuccess = true;
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write,
                                                            FileShare.ReadWrite))
                {
                    using (var streamWriter = new StreamWriter(fileStream))
                    {
                        streamWriter.Write(str);
                        streamWriter.Flush();
                    }

                }
            }
            catch (Exception ex)
            {
                if (isThrowException)
                {
                    throw ex;
                }
                isSuccess = false;
            }
            return isSuccess;

        }
    }
}
