using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Code.Framework.Extension;

namespace Code.Framework.Db
{
    public class DbHelper
    {
        public DbHelper()
        { }

        /// <summary>
        /// 获取DbHelperService实例化对象
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static DbHelperService GetInstance(string connectionString)
        {
            return new DbHelperService(DatabaseType.MSSqlserver, connectionString);
        }
        /// <summary>
        /// 根据model生成update语句如果值为当前类型默认值时不会更新比如(int=0时,不会在update中加入当前字段及值),表名和类名不一样时可以用[Description(表名)]特性标识表名,会有一定性能损失,在更新量不大时可以使用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="key"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static SqlParameter[] DynamicBuildUpdateSql<T>(T model, string key, ref string sql)
        {
            List<SqlParameter> list = new List<SqlParameter>();
            var updatesql = new StringBuilder();
            var type = typeof(T);
            var tableName = string.IsNullOrEmpty(type.GetDescription()) ? type.Name : type.GetDescription();

            updatesql.AppendFormat(@"UPDATE [zhaopin].[{0}] SET ", tableName);

            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(string) && property.GetValue(model, null) != null && !string.IsNullOrEmpty(property.GetValue(model, null).ToString()) && property.Name != key)
                {
                    updatesql.AppendFormat("[{0}] = @{1},", property.Name, property.Name);
                    list.Add(new SqlParameter(string.Format("@{0}", property.Name), property.GetValue(model, null)));
                }
                if (property.PropertyType == typeof(int) && property.GetValue(model, null) != null && property.GetValue(model, null).ToInt32(0) != 0 && property.Name != key)
                {
                    updatesql.AppendFormat("[{0}] = @{1},", property.Name, property.Name);
                    list.Add(new SqlParameter(string.Format("@{0}", property.Name), property.GetValue(model, null)));
                }
                if (property.PropertyType == typeof(Int64) && property.GetValue(model, null) != null && property.GetValue(model, null).ToInt64(0) != 0 && property.Name != key)
                {
                    updatesql.AppendFormat("[{0}] = @{1},", property.Name, property.Name);
                    list.Add(new SqlParameter(string.Format("@{0}", property.Name), property.GetValue(model, null)));
                }
                if (property.PropertyType == typeof(DateTime) && property.GetValue(model, null) != null && property.GetValue(model, null).ConvertDate(DateTime.Parse("0001-01-01")) != DateTime.Parse("0001-01-01"))
                {
                    updatesql.AppendFormat("[{0}] = @{1},", property.Name, property.Name);
                    list.Add(new SqlParameter(string.Format("@{0}", property.Name), property.GetValue(model, null)));
                }
                if (property.PropertyType == typeof(byte) && property.GetValue(model, null) != null && property.GetValue(model, null).ToByte() != 0)
                {
                    updatesql.AppendFormat("[{0}] = @{1},", property.Name, property.Name);
                    list.Add(new SqlParameter(string.Format("@{0}", property.Name), property.GetValue(model, null)));
                }
            }
            var keyProperty = properties.First(p => p.Name == key);
            list.Add(new SqlParameter(string.Format("@{0}", keyProperty.Name), keyProperty.GetValue(model, null)));
            sql = updatesql.Remove(updatesql.Length - 1, 1).AppendFormat(" where {0} = @{1}", keyProperty.Name, keyProperty.Name).ToString();
            return list.ToArray();
        }
        /// <summary>
        /// 根据model生成update语句,使用参数安全,会有一定性能损失,在更新量不大时可以使用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static SqlParameter[] DynamicBuildInsertSql<T>(T model, ref string sql)
        {
            List<SqlParameter> list = new List<SqlParameter>();
            var fieldSql = new StringBuilder();
            var paraSql = new StringBuilder();
            var type = typeof(T);
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(string) && property.GetValue(model, null) != null && !string.IsNullOrEmpty(property.GetValue(model, null).ToString()))
                {
                    fieldSql.AppendFormat("[{0}] ,", property.Name);
                    paraSql.AppendFormat("@{0} ,", property.Name);

                    list.Add(new SqlParameter(string.Format("@{0}", property.Name), property.GetValue(model, null)));
                }
                if (property.PropertyType == typeof(int) && property.GetValue(model, null) != null && property.GetValue(model, null).ToInt32(0) != 0)
                {
                    fieldSql.AppendFormat("[{0}] ,", property.Name);
                    paraSql.AppendFormat("@{0} ,", property.Name);
                    list.Add(new SqlParameter(string.Format("@{0}", property.Name), property.GetValue(model, null)));
                }
                if (property.PropertyType == typeof(Int64) && property.GetValue(model, null) != null && property.GetValue(model, null).ToInt64(0) != 0)
                {
                    fieldSql.AppendFormat("[{0}] ,", property.Name);
                    paraSql.AppendFormat("@{0} ,", property.Name);
                    list.Add(new SqlParameter(string.Format("@{0}", property.Name), property.GetValue(model, null)));
                }
                if (property.PropertyType == typeof(byte) && property.GetValue(model, null) != null && property.GetValue(model, null).ToByte(0) != 0)
                {
                    fieldSql.AppendFormat("[{0}] ,", property.Name);
                    paraSql.AppendFormat("@{0} ,", property.Name);
                    list.Add(new SqlParameter(string.Format("@{0}", property.Name), property.GetValue(model, null)));
                }
                if (property.PropertyType == typeof(DateTime) && property.GetValue(model, null) != null && property.GetValue(model, null).ConvertDate(DateTime.Parse("0001-01-01")) != DateTime.Parse("0001-01-01"))
                {
                    fieldSql.AppendFormat("[{0}] ,", property.Name);
                    paraSql.AppendFormat("@{0} ,", property.Name);
                    list.Add(new SqlParameter(string.Format("@{0}", property.Name), property.GetValue(model, null)));
                }
            }
            var tableName = string.IsNullOrEmpty(type.GetDescription()) ? type.Name : type.GetDescription();
            sql = string.Format("INSERT INTO [zhaopin].{0}({1})VALUES({2})", tableName, fieldSql.Remove(fieldSql.Length - 1, 1), paraSql.Remove(paraSql.Length - 1, 1));
            return list.ToArray();
        }
    }
}
