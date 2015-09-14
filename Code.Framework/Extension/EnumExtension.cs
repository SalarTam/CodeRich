using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;

namespace Code.Framework.Extension
{
    public static class EnumExtension
    {
        public static string GetDescription(this Enum data)
        {
            string name = data.ToString();
            MemberInfo[] members = data.GetType().GetMember(name);
            foreach (MemberInfo member in members)
            {
                if (member.Name == name)
                {
                    foreach (DescriptionAttribute attr in member.GetCustomAttributes(typeof(DescriptionAttribute), false))
                    {
                        return attr.Description;
                    }
                }
            }
            return name;
        }
        /// <summary>
        /// 得到类型成员的注释
        /// </summary>
        /// <param name="aType">类型定义</param>
        /// <param name="aName">成员名</param>
        /// <returns>注释,如果无注释则返回成员名</returns>
        public static string GetDescription(this Type aType, string aName)
        {
            MemberInfo[] minfos = aType.GetMember(aName);
            foreach (MemberInfo info in minfos)
            {
                foreach (DescriptionAttribute attr in info.GetCustomAttributes(typeof(DescriptionAttribute), false))
                {
                    return attr.Description;
                }
            }
            return aName;
        }
        public static string GetDescription(this Type aType)
        {
            var attr = aType.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attr == null || attr.Length == 0)
            {
                return "";
            }
            return (attr[0] as DescriptionAttribute).Description;
        }
        /// <summary>
        /// 获取类型中所有属性的Description
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static List<string> GetDescriptions(this Type type)
        {
            List<string> comments = new List<string>();
            if (type.IsEnum)
            {
                string[] names = Enum.GetNames(type);
                foreach (string name in names)
                {
                    comments.Add(type.GetDescription(name));
                }
                return comments;
            }
            else if (type.IsClass)
            {
                foreach (FieldInfo fi in type.GetFields())
                {
                    foreach (DescriptionAttribute attr in fi.GetCustomAttributes(typeof(DescriptionAttribute), false))
                    {
                        comments.Add(attr.Description);
                    }
                }
            }
            return comments;
        }
        /// <summary>
        /// 将枚举类型转换成数组
        /// 格式：string[]{Description,Name,Value}
        /// </summary>
        /// <returns></returns>
        public static string[] ToArgments(this Enum data)
        {
            string[] args = new string[3];
            args[0] = data.GetDescription();
            args[1] = data.ToString();
            args[2] = data.GetHashCode().ToString();
            return args;
        }


    }

}
