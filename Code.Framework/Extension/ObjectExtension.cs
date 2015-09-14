using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Code.Framework.Serializer;

namespace Code.Framework.Extension
{
    public static class ObjectExtension
    {
        private static readonly Type ValueTypeType = typeof(ValueType);

        /// <summary>
        /// 将字符转换成自己的类型
        /// </summary>
        /// <param name="val">System.String</param>
        /// <returns>如果转换失败将返回 T 的默认值</returns>
        public static T ToT<T>(this object val)
        {
            if (val != null)
            {
                return val.ToT<T>(default(T));
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// 当前对象转换成特定类型，如果转换失败或者对象为null，返回defaultValue。
        /// 如果传入的是可空类型：会试着转换成其真正类型后返回。
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="value">原类型对象</param>
        /// <param name="defaultValue">转换出错或者对象为空的时候的默认返回值</param>
        /// <returns>转换后的值</returns>
        public static T ToT<T>(this object value, T defaultValue)
        {
            if (value == null)
            {
                return defaultValue;
            }
            else if (value is T)
            {
                return (T)value;
            }
            try
            {
                Type typ = typeof(T);
                if (typ.BaseType == ObjectExtension.ValueTypeType && typ.IsGenericType)//可空泛型
                {
                    Type[] typs = typ.GetGenericArguments();
                    return (T)Convert.ChangeType(value, typs[0]);
                }
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string DToString(this DateTime? value, string format)
        {
            return value.DToString("", format);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string DToString(this DateTime? value, string defaultValue, string format)
        {
            if (value == null)
            {
                return defaultValue;
            }

            return String.IsNullOrEmpty(format) ? ((DateTime)value).ToString() :
                ((DateTime)value).ToString(format);

        }

        /// <summary>
        /// Creator: edmund.li
        /// Date: 2012-6-27
        /// Function: 自定义拷贝方法
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="value"> 可实例化的类</param>
        /// <returns>返回拷贝后的新对象</returns>
        public static T CustomCopy<T>(this T value) where T : class
        {
            if (value == null)
            {
                return default(T);
            }

            BinarySerializer<T> ser = new BinarySerializer<T>();
            var obj = ser.Serialize(value);

            return ser.Deserialize(obj);
        }
        /// <summary>
        /// 限制字符串的长度,超过length长度的部分截断加...
        /// </summary>
        /// <param name="str"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string LimitStringLength(this string str, int length)
        {
            return str.Length <= length ? str : str.Substring(0, length) + "...";
        }
        /// <summary>
        /// 获取MD5值
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        //public static string GetMd5(this string val)
        //{
        //    return string.IsNullOrEmpty(val) ? "" : DataEncryption.MD5EncrptyData(val);
        //}
        public static DateTime ConvertDate(this string val, DateTime? defaults = null)
        {
            DateTime dateTime = DateTime.MinValue;
            if (!DateTime.TryParse(val, out dateTime))
            {
                dateTime = defaults ?? DateTime.Parse("1800-01-01");
            }
            return dateTime;
        }
        /// <summary>
        /// 将对象转换为时间类型
        /// </summary>
        /// <param name="val"></param>
        /// <param name="defaults"></param>
        /// <returns></returns>
        public static DateTime ConvertDate(this object val, DateTime? defaults = null)
        {
            DateTime dateTime = defaults == null ? DateTime.MinValue : defaults.Value;
            if (val == null)
            {
                return dateTime;
            }

            if (!DateTime.TryParse(val.ToString(), out dateTime))
            {
                dateTime = defaults ?? DateTime.Parse("1800-01-01");
            }
            return dateTime;
        }
        /// <summary>
        /// 将对象转换为int32类型
        /// </summary>
        /// <param name="val"></param>
        /// <param name="defaults"></param>
        /// <returns></returns>
        public static int ToInt32(this object val, int? defaults = null)
        {
            int result = defaults == null ? -1 : defaults.Value;
            if (val == null)
            {
                return result;
            }

            if (!Int32.TryParse(val.ToString(), out result))
            {
                result = defaults ?? 0;
            }
            return result;
        }
        public static byte ToByte(this object val, byte? defaults = 0)
        {
            byte result = defaults.Value;
            if (val == null)
            {
                return result;
            }

            if (!byte.TryParse(val.ToString(), out result))
            {
                result = defaults ?? 0;
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <param name="defaults"></param>
        /// <returns></returns>
        public static Int64 ToInt64(this object val, Int64? defaults = null)
        {
            Int64 result = defaults == null ? -1 : defaults.Value;
            if (val == null)
            {
                return result;
            }

            if (!Int64.TryParse(val.ToString(), out result))
            {
                result = defaults ?? 0;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this DataTable table)
        {
            var list = new List<T>();
            if (table != null && table.Rows.Count > 0)
            {
                var type = typeof(T);
                foreach (DataRow row in table.Rows)
                {
                    T item = System.Activator.CreateInstance<T>();
                    foreach (var property in type.GetProperties())
                    {
                        if (table.Columns.Contains(property.Name))
                        {

                            if (property.PropertyType == typeof(Int32))
                            {
                                property.SetValue(item, row[property.Name].ToInt32(), null);
                            }
                            if (property.PropertyType == typeof(Int64))
                            {
                                property.SetValue(item, row[property.Name].ToInt64(), null);
                            }
                            if (property.PropertyType == typeof(string))
                            {
                                property.SetValue(item, row[property.Name].ToString(), null);
                            }
                            if (property.PropertyType == typeof(DateTime))
                            {
                                property.SetValue(item, row[property.Name].ConvertDate(), null);
                            }


                        }
                    }
                    list.Add(item);

                }
            }
            return list;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <returns></returns>
        public static T ToModel<T>(this DataRow row)
        {
            var type = typeof(T);
            var item = System.Activator.CreateInstance<T>();
            foreach (var property in type.GetProperties())
            {
                if (row.Table.Columns.Contains(property.Name))
                {
                    if (property.PropertyType == typeof(Int32))
                    {
                        property.SetValue(item, row[property.Name].ToInt32(), null);
                    }
                    if (property.PropertyType == typeof(Int64))
                    {
                        property.SetValue(item, row[property.Name].ToInt64(), null);
                    }
                    if (property.PropertyType == typeof(string))
                    {
                        property.SetValue(item, row[property.Name].ToString(), null);
                    }
                    if (property.PropertyType == typeof(DateTime))
                    {
                        property.SetValue(item, row[property.Name].ConvertDate(), null);
                    }
                }
            }
            return item;
        }
    }
}
