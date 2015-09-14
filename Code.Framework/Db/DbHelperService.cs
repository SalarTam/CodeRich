using System;
using System.Data;
using System.Data.Common;
using System.Xml;

namespace Code.Framework.Db
{
    public class DbHelperService
    {
        private IDbHelper _dbHelper = null;
        private string _strConnectionString;
        /// <summary>
        ///       构造方法
        ///       </summary>
        /// <param name="dt">数据库类型</param>
        public DbHelperService(DatabaseType dt)
        {
            this.GetInstance(dt);
        }
        /// <summary>
        ///       构造方法
        ///       </summary>
        /// <param name="dt">数据库类型</param>
        /// <param name="dbConnectionString">数据库连接串</param>
        public DbHelperService(DatabaseType dt, string dbConnectionString)
        {
            if (string.IsNullOrEmpty(dbConnectionString))
            {
                throw new ArgumentNullException("dbConnectionString");
            }
            this._strConnectionString = dbConnectionString;
            this.GetInstance(dt);
        }
        /// <summary>
        ///       本构造函数,数据库类型默认为MS SQLServer
        ///       </summary>
        /// <param name="dbConnectionString">数据库连接串</param>
        public DbHelperService(string dbConnectionString) : this(DatabaseType.MSSqlserver, dbConnectionString)
        {
        }
        private void GetInstance(DatabaseType dt)
        {
            if (dt == DatabaseType.MSSqlserver)
            {
                this._dbHelper = new SqlHelper();
            }
        }
        /// <summary>
        ///       执行指定连接字符串,类型的SqlCommand.
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        int result = ExecuteNonQuery(CommandType.StoredProcedure, "PublishOrders");
        ///       </remarks>
        /// <param name="commandType">命令类型 (存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回命令影响的行数</returns>
        public int ExecuteNonQuery(CommandType commandType, string commandText)
        {
            return this.ExecuteNonQuery(this._strConnectionString, commandType, commandText);
        }
        /// <summary>
        ///       执行指定数据库连接对象的命令
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders");
        ///       </remarks>
        /// <param name="connectionString">一个有效的数据库连接字符串</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <returns>返回命令影响的行数</returns>
        public int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            return this._dbHelper.ExecuteNonQuery(connectionString, commandType, commandText);
        }
        /// <summary>
        ///       执行指定数据库连接对象的命令 
        ///       使用提供的参数
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        int result = ExecuteNonQuery(CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="commandType">命令类型 (存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">SqlParamter参数数组</param>
        /// <returns>返回影响的行数</returns>
        public int ExecuteNonQuery(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return this.ExecuteNonQuery(this._strConnectionString, commandType, commandText, commandParameters);
        }
        /// <summary>
        ///       使用提供的参数执行指定数据库连接对象的命令 
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="connectionString">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本, 其它.)</param>
        /// <param name="commandText">存储过程名称或SQL语句</param>
        /// <param name="commandParameters">SqlParamter参数数组</param>
        /// <returns>返回影响的行数</returns>
        public int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return this._dbHelper.ExecuteNonQuery(connectionString, commandType, commandText, commandParameters);
        }
        /// <summary>
        ///       执行数据库连接对象的命令,将对象数组的值赋给存储过程参数.
        ///       </summary>
        /// <remarks>
        ///       此方法不提供访问存储过程输出参数和返回值
        ///
        ///       e.g.:  
        ///        int result = ExecuteNonQuery("PublishOrders", 24, 36);
        ///       </remarks>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回影响的行数</returns>
        public int ExecuteNonQuery(string spName, params object[] parameterValues)
        {
            return this.ExecuteNonQuery(this._strConnectionString, spName, parameterValues);
        }
        /// <summary>
        ///       执行指定数据库连接对象的命令,将对象数组的值赋给存储过程参数.
        ///       </summary>
        /// <remarks>
        ///       此方法不提供访问存储过程输出参数和返回值
        ///
        ///       e.g.:  
        ///        int result = ExecuteNonQuery(connString, "PublishOrders", 24, 36);
        ///       </remarks>
        /// <param name="connectionString">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回影响的行数</returns>
        public int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        {
            return this._dbHelper.ExecuteNonQuery(connectionString, spName, parameterValues);
        }
        /// <summary>
        ///       执行指定数据库连接对象的命令. 
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders");
        ///       </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <returns>返回影响的行数</returns>
        public int ExecuteNonQuery(DbConnection connection, CommandType commandType, string commandText)
        {
            return this._dbHelper.ExecuteNonQuery(connection, commandType, commandText);
        }
        /// <summary>
        ///       执行指定数据库连接对象的命令
        ///       using the provided parameters.
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <param name="commandParameters">SqlParamter参数数组</param>
        /// <returns>返回影响的行数</returns>
        public int ExecuteNonQuery(DbConnection connection, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return this._dbHelper.ExecuteNonQuery(connection, commandType, commandText, commandParameters);
        }
        /// <summary>
        ///       执行指定数据库连接对象的命令,将对象数组的值赋给存储过程参数.
        ///       </summary>
        /// <remarks>
        ///       此方法不提供访问存储过程输出参数和返回值
        ///
        ///       e.g.:  
        ///        int result = ExecuteNonQuery(conn, "PublishOrders", 24, 36);
        ///       </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">命令类型(存储过程,命令文本或其它.)</param>
        /// <param name="parameterValues">SqlParamter参数数组</param>
        /// <returns>返回影响的行数</returns>
        public int ExecuteNonQuery(DbConnection connection, string spName, params object[] parameterValues)
        {
            return this._dbHelper.ExecuteNonQuery(connection, spName, parameterValues);
        }
        /// <summary>
        ///       执行带事务的SqlCommand.  
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders");
        ///       </remarks>
        /// <param name="transaction">一个有效的事物</param>
        /// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <returns>返回影响的行数</returns>
        public int ExecuteNonQuery(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return this._dbHelper.ExecuteNonQuery(transaction, commandType, commandText);
        }
        /// <summary>
        ///       执行带事务的SqlCommand(指定参数).
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="transaction">一个有效的数据库事物</param>
        /// <param name="commandType">命令类型(存储过程,命令文本或其它.)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <param name="commandParameters">SqlParamter参数数组</param>
        /// <returns>返回影响的行数</returns>
        public int ExecuteNonQuery(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return this._dbHelper.ExecuteNonQuery(transaction, commandType, commandText, commandParameters);
        }
        /// <summary>
        ///       执行带事务的SqlCommand(指定参数值).
        ///       </summary>
        /// <remarks>
        ///       此方法不提供访问存储过程输出参数和返回值
        ///       示例:  
        ///       int result = ExecuteNonQuery(conn, trans, "PublishOrders", 24, 36);
        ///       </remarks>
        /// <param name="transaction">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回受影响的行数</returns>
        public int ExecuteNonQuery(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            return this._dbHelper.ExecuteNonQuery(transaction, spName, parameterValues);
        }
        /// <summary>
        ///       根据命令类型和命令内容返回DataSet. 
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        DataSet ds = ExecuteDataset(CommandType.StoredProcedure, "GetOrders");
        ///       </remarks>
        /// <param name="commandType">命令类型(存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public DataSet ExecuteDataset(CommandType commandType, string commandText)
        {
            return this.ExecuteDataset(this._strConnectionString, commandType, commandText);
        }
        /// <summary>
        ///       根据命令类型和命令内容返回DataSet. 
        ///       </summary>
        /// <remarks>
        ///       示例:  
        ///       DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders");
        ///       </remarks>
        /// <param name="connectionString">一个有效的数据库连接字符串</param>
        /// <param name="commandType">命令类型(存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        {
            return this._dbHelper.ExecuteDataset(connectionString, commandType, commandText);
        }
        /// <summary>
        ///       执行指定数据库连接字符串的命令,返回DataSet.
        ///       </summary>
        /// <remarks>
        ///       示例:  
        ///       DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="commandType">命令类型(存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <param name="commandParameters">SqlParamters参数数组</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public DataSet ExecuteDataset(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return this.ExecuteDataset(this._strConnectionString, commandType, commandText, commandParameters);
        }
        /// <summary>
        ///       执行指定数据库连接字符串的命令,返回DataSet.
        ///       </summary>
        /// <remarks>
        ///       示例:  
        ///       DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="connectionString">一个有效的数据库连接字符串</param>
        /// <param name="commandType">命令类型(存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <param name="commandParameters">SqlParamters参数数组</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return this._dbHelper.ExecuteDataset(connectionString, commandType, commandText, commandParameters);
        }
        /// <summary>
        ///       执行数据库连接对象的命令,指定参数值,返回DataSet.
        ///       </summary>
        /// <remarks>
        ///       此方法不提供访问存储过程输入参数和返回值.
        ///       示例.: 
        ///        DataSet ds = ExecuteDataset("GetOrders", 24, 36);
        ///       </remarks>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public DataSet ExecuteDataset(string spName, params object[] parameterValues)
        {
            return this.ExecuteDataset(this._strConnectionString, spName, parameterValues);
        }
        /// <summary>
        ///       执行指定数据库连接对象的命令,指定参数值,返回DataSet.
        ///       </summary>
        /// <remarks>
        ///       此方法不提供访问存储过程输入参数和返回值.
        ///       示例.: 
        ///        DataSet ds = ExecuteDataset(conn, "GetOrders", 24, 36);
        ///       </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
        {
            return this._dbHelper.ExecuteDataset(connectionString, spName, parameterValues);
        }
        /// <summary>
        ///       执行指定数据库连接对象的命令,返回DataSet. 
        ///       </summary>
        /// <remarks>
        ///       示例:  
        ///       DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders");
        ///       </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或T-SQL语句</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public DataSet ExecuteDataset(DbConnection connection, CommandType commandType, string commandText)
        {
            return this._dbHelper.ExecuteDataset(connection, commandType, commandText);
        }
        /// <summary>
        ///       执行指定数据库连接对象的命令,指定存储过程参数,返回DataSet.
        ///       </summary>
        /// <remarks>
        ///       示例:  
        ///       DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或T-SQL语句</param>
        /// <param name="commandParameters">SqlParamter参数数组</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public DataSet ExecuteDataset(DbConnection connection, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return this._dbHelper.ExecuteDataset(connection, commandType, commandText, commandParameters);
        }
        /// <summary>
        ///       执行指定数据库连接字符串的命令,直接提供参数值,返回DataSet.
        ///       </summary>
        /// <remarks>
        ///       此方法不提供访问存储过程输出参数和返回值.
        ///       示例:  
        ///       DataSet ds = ExecuteDataset(connString, "GetOrders", 24, 36);
        ///       </remarks>
        /// <param name="connection">一个有效的数据库连接字符串</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public DataSet ExecuteDataset(DbConnection connection, string spName, params object[] parameterValues)
        {
            return this._dbHelper.ExecuteDataset(connection, spName, parameterValues);
        }
        /// <summary>
        ///       执行指定事务的命令,返回DataSet.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
        ///       </remarks>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或T-SQL语句</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public DataSet ExecuteDataset(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return this._dbHelper.ExecuteDataset(transaction, commandType, commandText);
        }
        /// <summary>
        ///       执行指定事务的命令,指定参数,返回DataSet.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或T-SQL语句</param>
        /// <param name="commandParameters">SqlParamter参数数组</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public DataSet ExecuteDataset(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return this._dbHelper.ExecuteDataset(transaction, commandType, commandText, commandParameters);
        }
        /// <summary>
        ///       执行指定事务的命令,指定参数,返回DataSet.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       DataSet ds = ExecuteDataset(trans, "GetOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="transaction">事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandParameters">SqlParamter参数数组</param>
        /// <returns>返回一个包含结果集的DataSet</returns>
        public DataSet ExecuteDataset(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            return this._dbHelper.ExecuteDataset(transaction, spName, parameterValues);
        }
        /// <summary>
        ///       执行数据库连接字符串的数据阅读器.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       SqlDataReader dr = ExecuteReader(CommandType.StoredProcedure, "GetOrders");
        ///       </remarks>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或T-SQL语句</param>
        /// <returns>返回包含结果集的SqlDataReader</returns>
        public DbDataReader ExecuteReader(CommandType commandType, string commandText)
        {
            return this.ExecuteReader(this._strConnectionString, commandType, commandText);
        }
        /// <summary>
        ///       执行指定数据库连接字符串的数据阅读器.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       SqlDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders");
        ///       </remarks>
        /// <param name="connectionString">一个有效的数据库连接字符串</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或T-SQL语句</param>
        /// <returns>返回包含结果集的SqlDataReader</returns>
        public DbDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        {
            return this._dbHelper.ExecuteReader(connectionString, commandType, commandText);
        }
        /// <summary>
        ///       执行指定数据库连接字符串的数据阅读器,指定参数.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///        SqlDataReader dr = ExecuteReader(CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或T-SQL语句</param>
        /// <param name="commandParameters">SqlParamter参数数组(new SqlParameter("@prodid", 24))</param>
        /// <returns>返回包含结果集的SqlDataReader</returns>
        public DbDataReader ExecuteReader(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return this.ExecuteReader(this._strConnectionString, commandType, commandText, commandParameters);
        }
        /// <summary>
        ///       执行指定数据库连接字符串的数据阅读器,指定参数.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///        SqlDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="connectionString">一个有效的数据库连接字符串</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或T-SQL语句</param>
        /// <param name="commandParameters">SqlParamter参数数组(new SqlParameter("@prodid", 24))</param>
        /// <returns>返回包含结果集的SqlDataReader</returns>
        public DbDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return this._dbHelper.ExecuteReader(connectionString, commandType, commandText, commandParameters);
        }
        /// <summary>
        ///       执行指定数据库连接字符串的数据阅读器,指定参数值.
        ///       </summary>
        /// <remarks>
        ///       此方法不提供访问存储过程输出参数和返回值参数.
        ///       示例: 
        ///       SqlDataReader dr = ExecuteReader("GetOrders", 24, 36);
        ///       </remarks>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回包含结果集的SqlDataReader</returns>
        public DbDataReader ExecuteReader(string spName, params object[] parameterValues)
        {
            return this.ExecuteReader(this._strConnectionString, spName, parameterValues);
        }
        /// <summary>
        ///       执行指定数据库连接字符串的数据阅读器,指定参数值.
        ///       </summary>
        /// <remarks>
        ///       此方法不提供访问存储过程输出参数和返回值参数.
        ///       示例: 
        ///       SqlDataReader dr = ExecuteReader(connString, "GetOrders", 24, 36);
        ///       </remarks>
        /// <param name="connectionString">一个有效的数据库连接字符串</param>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回包含结果集的SqlDataReader</returns>
        public DbDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        {
            return this._dbHelper.ExecuteReader(connectionString, spName, parameterValues);
        }
        /// <summary>
        ///       执行指定数据库连接对象的数据阅读器.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       SqlDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders");
        ///       </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或T-SQL语句</param>
        /// <returns>返回包含结果集的SqlDataReader</returns>
        public DbDataReader ExecuteReader(DbConnection connection, CommandType commandType, string commandText)
        {
            return this._dbHelper.ExecuteReader(connection, commandType, commandText);
        }
        /// <summary>
        ///       [调用者方式]执行指定数据库连接对象的数据阅读器,指定参数.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       SqlDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名或T-SQL语句</param>
        /// <param name="commandParameters">SqlParamter参数数组</param>
        /// <returns>返回包含结果集的SqlDataReader</returns>
        public DbDataReader ExecuteReader(DbConnection connection, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return this._dbHelper.ExecuteReader(connection, commandType, commandText, commandParameters);
        }
        /// <summary>
        ///       [调用者方式]执行指定数据库连接对象的数据阅读器,指定参数值.
        ///       </summary>
        /// <remarks>
        ///       此方法不提供访问存储过程输出参数和返回值参数.
        ///       示例: 
        ///       SqlDataReader dr = ExecuteReader(conn, "GetOrders", 24, 36);
        ///       </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">T存储过程名</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回包含结果集的SqlDataReader</returns>
        public DbDataReader ExecuteReader(DbConnection connection, string spName, params object[] parameterValues)
        {
            return this._dbHelper.ExecuteReader(connection, spName, parameterValues);
        }
        /// <summary>
        ///       [调用者方式]执行指定数据库事务的数据阅读器,指定参数值.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       SqlDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders");
        ///       </remarks>
        /// <param name="transaction">一个有效的连接事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <returns>返回包含结果集的SqlDataReader</returns>
        public DbDataReader ExecuteReader(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return this._dbHelper.ExecuteReader(transaction, commandType, commandText);
        }
        /// <summary>
        ///       [调用者方式]执行指定数据库事务的数据阅读器,指定参数.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///        SqlDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="transaction">一个有效的连接事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
        /// <returns>返回包含结果集的SqlDataReader</returns>
        public DbDataReader ExecuteReader(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return this._dbHelper.ExecuteReader(transaction, commandType, commandText, commandParameters);
        }
        /// <summary>
        ///       [调用者方式]执行指定数据库事务的数据阅读器,指定参数值.
        ///       </summary>
        /// <remarks>
        ///       此方法不提供访问存储过程输出参数和返回值参数.
        ///       示例: 
        ///       SqlDataReader dr = ExecuteReader(trans, "GetOrders", 24, 36);
        ///       </remarks>
        /// <param name="transaction">一个有效的连接事务</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回包含结果集的SqlDataReader</returns>
        public DbDataReader ExecuteReader(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            return this._dbHelper.ExecuteReader(transaction, spName, parameterValues);
        }
        /// <summary>
        ///       执行指定数据库连接字符串的命令,返回结果集中的第一行第一列.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       int orderCount = (int)ExecuteScalar(CommandType.StoredProcedure, "GetOrderCount");
        ///       </remarks>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public object ExecuteScalar(CommandType commandType, string commandText)
        {
            return this.ExecuteScalar(this._strConnectionString, commandType, commandText);
        }
        /// <summary>
        ///       执行指定数据库连接字符串的命令,返回结果集中的第一行第一列.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount");
        ///       </remarks>
        /// <param name="connectionString">一个有效的数据库连接字符串</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        {
            return this._dbHelper.ExecuteScalar(connectionString, commandType, commandText);
        }
        /// <summary>
        ///       执行指定数据库连接字符串的命令,指定参数,返回结果集中的第一行第一列.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       int orderCount = (int)ExecuteScalar(CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public object ExecuteScalar(CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return this.ExecuteScalar(this._strConnectionString, commandType, commandText, commandParameters);
        }
        /// <summary>
        ///       执行指定数据库连接字符串的命令,指定参数,返回结果集中的第一行第一列.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="connectionString">一个有效的数据库连接字符串</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return this._dbHelper.ExecuteScalar(connectionString, commandType, commandText, commandParameters);
        }
        /// <summary>
        ///       执行指定数据库连接字符串的命令,指定参数值,返回结果集中的第一行第一列.
        ///       </summary>
        /// <remarks>
        ///       此方法不提供访问存储过程输出参数和返回值参数.
        ///       示例: 
        ///       int orderCount = (int)ExecuteScalar("GetOrderCount", 24, 36);
        ///       </remarks>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public object ExecuteScalar(string spName, params object[] parameterValues)
        {
            return this.ExecuteScalar(this._strConnectionString, spName, parameterValues);
        }
        /// <summary>
        ///       执行指定数据库连接字符串的命令,指定参数值,返回结果集中的第一行第一列.
        ///       </summary>
        /// <remarks>
        ///       此方法不提供访问存储过程输出参数和返回值参数.
        ///       示例: 
        ///       int orderCount = (int)ExecuteScalar(connString, "GetOrderCount", 24, 36);
        ///       </remarks>
        /// <param name="connectionString">一个有效的数据库连接字符串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
        {
            return this._dbHelper.ExecuteScalar(connectionString, spName, parameterValues);
        }
        /// <summary>
        ///       执行指定数据库连接对象的命令,返回结果集中的第一行第一列.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount");
        ///       </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public object ExecuteScalar(DbConnection connection, CommandType commandType, string commandText)
        {
            return this._dbHelper.ExecuteScalar(connection, commandType, commandText);
        }
        /// <summary>
        ///       执行指定数据库连接对象的命令,指定参数,返回结果集中的第一行第一列.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public object ExecuteScalar(DbConnection connection, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return this._dbHelper.ExecuteScalar(connection, commandType, commandText, commandParameters);
        }
        /// <summary>
        ///       执行指定数据库连接对象的命令,指定参数值,返回结果集中的第一行第一列.
        ///       </summary>
        /// <remarks>
        ///       此方法不提供访问存储过程输出参数和返回值参数.
        ///       示例: 
        ///       int orderCount = (int)ExecuteScalar(conn, "GetOrderCount", 24, 36);
        ///       </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public object ExecuteScalar(DbConnection connection, string spName, params object[] parameterValues)
        {
            return this._dbHelper.ExecuteScalar(connection, spName, parameterValues);
        }
        /// <summary>
        ///       执行指定数据库事务的命令,返回结果集中的第一行第一列.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount");
        ///       </remarks>
        /// <param name="transaction">一个有效的连接事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public object ExecuteScalar(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return this._dbHelper.ExecuteScalar(transaction, commandType, commandText);
        }
        /// <summary>
        ///       执行指定数据库事务的命令,指定参数,返回结果集中的第一行第一列.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="transaction">一个有效的连接事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public object ExecuteScalar(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return this._dbHelper.ExecuteScalar(transaction, commandType, commandText, commandParameters);
        }
        /// <summary>
        ///       执行指定数据库事务的命令,指定参数值,返回结果集中的第一行第一列.
        ///       </summary>
        /// <remarks>
        ///       此方法不提供访问存储过程输出参数和返回值参数.
        ///       示例: 
        ///       int orderCount = (int)ExecuteScalar(trans, "GetOrderCount", 24, 36);
        ///       </remarks>
        /// <param name="transaction">一个有效的连接事务</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public object ExecuteScalar(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            return this._dbHelper.ExecuteScalar(transaction, spName, parameterValues);
        }
        /// <summary>
        ///       执行指定数据库连接对象的SqlCommand命令,并产生一个XmlReader对象做为结果集返回.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       XmlReader r = ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders");
        ///       </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句 using "FOR XML AUTO"</param>
        /// <returns>返回XmlReader结果集对象.</returns>
        public XmlReader ExecuteXmlReader(DbConnection connection, CommandType commandType, string commandText)
        {
            return this._dbHelper.ExecuteXmlReader(connection, commandType, commandText);
        }
        /// <summary>
        ///       执行指定数据库连接对象的SqlCommand命令,并产生一个XmlReader对象做为结果集返回,指定参数.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       XmlReader r = ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句 using "FOR XML AUTO"</param>
        /// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
        /// <returns>返回XmlReader结果集对象.</returns>
        public XmlReader ExecuteXmlReader(DbConnection connection, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return this._dbHelper.ExecuteXmlReader(connection, commandType, commandText, commandParameters);
        }
        /// <summary>
        ///       执行指定数据库连接对象的SqlCommand命令,并产生一个XmlReader对象做为结果集返回,指定参数值.
        ///       </summary>
        /// <remarks>
        ///       此方法不提供访问存储过程输出参数和返回值参数.
        ///       示例: 
        ///       XmlReader r = ExecuteXmlReader(conn, "GetOrders", 24, 36);
        ///       </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称 using "FOR XML AUTO"</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回XmlReader结果集对象.</returns>
        public XmlReader ExecuteXmlReader(DbConnection connection, string spName, params object[] parameterValues)
        {
            return this._dbHelper.ExecuteXmlReader(connection, spName, parameterValues);
        }
        /// <summary>
        ///       执行指定数据库事务的SqlCommand命令,并产生一个XmlReader对象做为结果集返回.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       XmlReader r = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders");
        ///       </remarks>
        /// <param name="transaction">一个有效的连接事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句 using "FOR XML AUTO"</param>
        /// <returns>返回XmlReader结果集对象.</returns>
        public XmlReader ExecuteXmlReader(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return this._dbHelper.ExecuteXmlReader(transaction, commandType, commandText);
        }
        /// <summary>
        ///       执行指定数据库事务的SqlCommand命令,并产生一个XmlReader对象做为结果集返回,指定参数.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       XmlReader r = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="transaction">一个有效的连接事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句 using "FOR XML AUTO"</param>
        /// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
        /// <returns>返回XmlReader结果集对象.</returns>
        public XmlReader ExecuteXmlReader(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return this._dbHelper.ExecuteXmlReader(transaction, commandType, commandText, commandParameters);
        }
        /// <summary>
        ///       执行指定数据库事务的SqlCommand命令,并产生一个XmlReader对象做为结果集返回,指定参数值.
        ///       </summary>
        /// <remarks>
        ///       此方法不提供访问存储过程输出参数和返回值参数.
        ///       示例: 
        ///       XmlReader r = ExecuteXmlReader(trans, "GetOrders", 24, 36);
        ///       </remarks>
        /// <param name="transaction">一个有效的连接事务</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        /// <returns>返回一个包含结果集的DataSet.</returns>
        public XmlReader ExecuteXmlReader(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            return this._dbHelper.ExecuteXmlReader(transaction, spName, parameterValues);
        }
        /// <summary>
        ///       执行指定数据库连接字符串的命令,映射数据表并填充数据集.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       FillDataset(CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        ///       </remarks>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <param name="dataSet">要填充结果集的DataSet实例</param>
        /// <param name="tableNames">表映射的数据表数组
        ///       用户定义的表名 (可有是实际的表名.)</param>
        public void FillDataset(CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            this.FillDataset(this._strConnectionString, commandType, commandText, dataSet, tableNames);
        }
        /// <summary>
        ///       执行指定数据库连接字符串的命令,映射数据表并填充数据集.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        ///       </remarks>
        /// <param name="connectionString">一个有效的数据库连接字符串</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <param name="dataSet">要填充结果集的DataSet实例</param>
        /// <param name="tableNames">表映射的数据表数组
        ///       用户定义的表名 (可有是实际的表名.)</param>
        public void FillDataset(string connectionString, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            this._dbHelper.FillDataset(connectionString, commandType, commandText, dataSet, tableNames);
        }
        /// <summary>
        ///       执行指定数据库连接字符串的命令,映射数据表并填充数据集.指定命令参数.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       FillDataset(CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <param name="dataSet">分配给命令的SqlParamter参数数组</param>
        /// <param name="tableNames">要填充结果集的DataSet实例</param>
        /// <param name="commandParameters">表映射的数据表数组
        ///       用户定义的表名 (可有是实际的表名.)
        ///       </param>
        public void FillDataset(CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params DbParameter[] commandParameters)
        {
            this.FillDataset(this._strConnectionString, commandType, commandText, dataSet, tableNames, commandParameters);
        }
        /// <summary>
        ///       执行指定数据库连接字符串的命令,映射数据表并填充数据集.指定命令参数.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="connectionString">一个有效的数据库连接字符串</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <param name="dataSet">分配给命令的SqlParamter参数数组</param>
        /// <param name="tableNames">要填充结果集的DataSet实例</param>
        /// <param name="commandParameters">表映射的数据表数组
        ///       用户定义的表名 (可有是实际的表名.)
        ///       </param>
        public void FillDataset(string connectionString, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params DbParameter[] commandParameters)
        {
            this._dbHelper.FillDataset(connectionString, commandType, commandText, dataSet, tableNames, commandParameters);
        }
        /// <summary>
        ///       执行指定数据库连接字符串的命令,映射数据表并填充数据集,指定存储过程参数值.
        ///       </summary>
        /// <remarks>
        ///       此方法不提供访问存储过程输出参数和返回值参数.
        ///       示例: 
        ///       FillDataset(CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, 24);
        ///       </remarks>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataSet">要填充结果集的DataSet实例</param>
        /// <param name="tableNames">表映射的数据表数组
        ///       用户定义的表名 (可有是实际的表名.)
        ///       </param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        public void FillDataset(string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            this.FillDataset(this._strConnectionString, spName, dataSet, tableNames, parameterValues);
        }
        /// <summary>
        ///       执行指定数据库连接字符串的命令,映射数据表并填充数据集,指定存储过程参数值.
        ///       </summary>
        /// <remarks>
        ///       此方法不提供访问存储过程输出参数和返回值参数.
        ///       示例: 
        ///       FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, 24);
        ///       </remarks>
        /// <param name="connectionString">一个有效的数据库连接字符串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataSet">要填充结果集的DataSet实例</param>
        /// <param name="tableNames">表映射的数据表数组
        ///       用户定义的表名 (可有是实际的表名.)
        ///       </param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        public void FillDataset(string connectionString, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            this._dbHelper.FillDataset(connectionString, spName, dataSet, tableNames, parameterValues);
        }
        /// <summary>
        ///       执行指定数据库连接对象的命令,映射数据表并填充数据集.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        ///       </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <param name="dataSet">要填充结果集的DataSet实例</param>
        /// <param name="tableNames">表映射的数据表数组
        ///       用户定义的表名 (可有是实际的表名.)
        ///       </param>
        public void FillDataset(DbConnection connection, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            this._dbHelper.FillDataset(connection, commandType, commandText, dataSet, tableNames);
        }
        /// <summary>
        ///       执行指定数据库连接对象的命令,映射数据表并填充数据集,指定参数.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <param name="dataSet">要填充结果集的DataSet实例</param>
        /// <param name="tableNames">表映射的数据表数组
        ///       用户定义的表名 (可有是实际的表名.)
        ///       </param>
        /// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
        public void FillDataset(DbConnection connection, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params DbParameter[] commandParameters)
        {
            this._dbHelper.FillDataset(connection, commandType, commandText, dataSet, tableNames, commandParameters);
        }
        /// <summary>
        ///       执行指定数据库连接对象的命令,映射数据表并填充数据集,指定存储过程参数值.
        ///       </summary>
        /// <remarks>
        ///       此方法不提供访问存储过程输出参数和返回值参数.
        ///       示例: 
        ///       FillDataset(conn, "GetOrders", ds, new string[] {"orders"}, 24, 36);
        ///       </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataSet">要填充结果集的DataSet实例</param>
        /// <param name="tableNames">表映射的数据表数组
        ///       用户定义的表名 (可有是实际的表名.)
        ///       </param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        public void FillDataset(DbConnection connection, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            this._dbHelper.FillDataset(connection, spName, dataSet, tableNames, parameterValues);
        }
        /// <summary>
        ///       执行指定数据库事务的命令,映射数据表并填充数据集.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       FillDataset(trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        ///       </remarks>
        /// <param name="transaction">一个有效的连接事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <param name="dataSet">要填充结果集的DataSet实例</param>
        /// <param name="tableNames">表映射的数据表数组
        ///       用户定义的表名 (可有是实际的表名.)
        ///       </param>
        public void FillDataset(DbTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            this._dbHelper.FillDataset(transaction, commandType, commandText, dataSet, tableNames);
        }
        /// <summary>
        ///       执行指定数据库事务的命令,映射数据表并填充数据集,指定参数.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       FillDataset(trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="transaction">一个有效的连接事务</param>
        /// <param name="commandType">命令类型 (存储过程,命令文本或其它)</param>
        /// <param name="commandText">存储过程名称或T-SQL语句</param>
        /// <param name="dataSet">要填充结果集的DataSet实例</param>
        /// <param name="tableNames">表映射的数据表数组
        ///       用户定义的表名 (可有是实际的表名.)
        ///       </param>
        /// <param name="commandParameters">分配给命令的SqlParamter参数数组</param>
        public void FillDataset(DbTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params DbParameter[] commandParameters)
        {
            this._dbHelper.FillDataset(transaction, commandType, commandText, dataSet, tableNames, commandParameters);
        }
        /// <summary>
        ///       执行指定数据库事务的命令,映射数据表并填充数据集,指定存储过程参数值.
        ///       </summary>
        /// <remarks>
        ///       此方法不提供访问存储过程输出参数和返回值参数.
        ///       示例: 
        ///       FillDataset(trans, "GetOrders", ds, new string[]{"orders"}, 24, 36);
        ///       </remarks>
        /// <param name="transaction">一个有效的连接事务</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataSet">要填充结果集的DataSet实例</param>
        /// <param name="tableNames">表映射的数据表数组
        ///       用户定义的表名 (可有是实际的表名.)
        ///       </param>
        /// <param name="parameterValues">分配给存储过程输入参数的对象数组</param>
        public void FillDataset(DbTransaction transaction, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            this._dbHelper.FillDataset(transaction, spName, dataSet, tableNames, parameterValues);
        }
        /// <summary>
        ///       执行数据集更新到数据库,指定inserted, updated, or deleted命令.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///       UpdateDataset(conn, insertCommand, deleteCommand, updateCommand, dataSet, "Order");
        ///       </remarks>
        /// <param name="insertCommand">[追加记录]一个有效的T-SQL语句或存储过程</param>
        /// <param name="deleteCommand">[删除记录]一个有效的T-SQL语句或存储过程</param>
        /// <param name="updateCommand">[更新记录]一个有效的T-SQL语句或存储过程</param>
        /// <param name="dataSet">要更新到数据库的DataSet</param>
        /// <param name="tableName">要更新到数据库的DataTable</param>
        public void UpdateDataset(DbCommand insertCommand, DbCommand deleteCommand, DbCommand updateCommand, DataSet dataSet, string tableName)
        {
            this._dbHelper.UpdateDataset(insertCommand, deleteCommand, updateCommand, dataSet, tableName);
        }
        /// <summary>
        ///       创建SqlCommand命令,指定数据库连接对象,存储过程名和参数.
        ///       </summary>
        /// <remarks>
        ///       示例: 
        ///        SqlCommand command = CreateCommand(conn, "AddCustomer", "CustomerID", "CustomerName");
        ///       </remarks>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="sourceColumns">源表的列名称数组</param>
        /// <returns>返回SqlCommand命令</returns>
        public DbCommand CreateCommand(DbConnection connection, string spName, params string[] sourceColumns)
        {
            return this._dbHelper.CreateCommand(connection, spName, sourceColumns);
        }
        /// <summary>
        ///       执行指定连接数据库连接对象的存储过程,使用DataRow做为参数值,返回受影响的行数.
        ///       </summary>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回影响的行数</returns>
        public int ExecuteNonQueryTypedParams(string spName, DataRow dataRow)
        {
            return this.ExecuteNonQueryTypedParams(this._strConnectionString, spName, dataRow);
        }
        /// <summary>
        ///       执行指定连接数据库连接对象的存储过程,使用DataRow做为参数值,返回受影响的行数.
        ///       </summary>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回影响的行数</returns>
        public int ExecuteNonQueryTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            return this._dbHelper.ExecuteNonQueryTypedParams(connectionString, spName, dataRow);
        }
        /// <summary>
        ///       执行指定连接数据库连接对象的存储过程,使用DataRow做为参数值,返回受影响的行数.
        ///       </summary>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回影响的行数</returns>
        public int ExecuteNonQueryTypedParams(DbConnection connection, string spName, DataRow dataRow)
        {
            return this._dbHelper.ExecuteNonQueryTypedParams(connection, spName, dataRow);
        }
        /// <summary>
        ///       执行指定连接数据库事物的存储过程,使用DataRow做为参数值,返回受影响的行数.
        ///       </summary>
        /// <param name="transaction">一个有效的连接事务 object</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回影响的行数</returns>
        public int ExecuteNonQueryTypedParams(DbTransaction transaction, string spName, DataRow dataRow)
        {
            return this._dbHelper.ExecuteNonQueryTypedParams(transaction, spName, dataRow);
        }
        /// <summary>
        ///       执行指定连接数据库连接对象的存储过程,使用DataRow做为参数值,返回DataSet.
        ///       </summary>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回一个包含结果集的DataSet.</returns>
        public DataSet ExecuteDatasetTypedParams(string spName, DataRow dataRow)
        {
            return this.ExecuteDatasetTypedParams(this._strConnectionString, spName, dataRow);
        }
        /// <summary>
        ///       执行指定连接数据库连接对象的存储过程,使用DataRow做为参数值,返回DataSet.
        ///       </summary>
        /// <param name="connectionString">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回一个包含结果集的DataSet.</returns>
        public DataSet ExecuteDatasetTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            return this._dbHelper.ExecuteDatasetTypedParams(connectionString, spName, dataRow);
        }
        /// <summary>
        ///       执行指定连接数据库连接对象的存储过程,使用DataRow做为参数值,返回DataSet.
        ///       </summary>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回一个包含结果集的DataSet.</returns>
        public DataSet ExecuteDatasetTypedParams(DbConnection connection, string spName, DataRow dataRow)
        {
            return this._dbHelper.ExecuteDatasetTypedParams(connection, spName, dataRow);
        }
        /// <summary>
        ///       执行指定连接数据库事务的存储过程,使用DataRow做为参数值,返回DataSet.
        ///       </summary>
        /// <param name="transaction">一个有效的连接事务 object</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回一个包含结果集的DataSet.</returns>
        public DataSet ExecuteDatasetTypedParams(DbTransaction transaction, string spName, DataRow dataRow)
        {
            return this._dbHelper.ExecuteDatasetTypedParams(transaction, spName, dataRow);
        }
        /// <summary>
        ///       执行指定连接数据库连接字符串的存储过程,使用DataRow做为参数值,返回DataReader.
        ///       </summary>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回包含结果集的SqlDataReader</returns>
        public DbDataReader ExecuteReaderTypedParams(string spName, DataRow dataRow)
        {
            return this.ExecuteReaderTypedParams(this._strConnectionString, spName, dataRow);
        }
        /// <summary>
        ///       执行指定连接数据库连接字符串的存储过程,使用DataRow做为参数值,返回DataReader.
        ///       </summary>
        /// <param name="connectionString">一个有效的数据库连接字符串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回包含结果集的SqlDataReader</returns>
        public DbDataReader ExecuteReaderTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            return this._dbHelper.ExecuteReaderTypedParams(connectionString, spName, dataRow);
        }
        /// <summary>
        ///       执行指定连接数据库连接字符串的存储过程,使用DataRow做为参数值,返回DataReader.
        ///       </summary>
        /// <param name="connection">一个有效的数据库连接字符串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回包含结果集的SqlDataReader</returns>
        public DbDataReader ExecuteReaderTypedParams(DbConnection connection, string spName, DataRow dataRow)
        {
            return this._dbHelper.ExecuteReaderTypedParams(connection, spName, dataRow);
        }
        /// <summary>
        ///       执行指定连接数据库事物的存储过程,使用DataRow做为参数值,返回DataReader.
        ///       </summary>
        /// <param name="transaction">一个有效的连接事务 object</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回包含结果集的SqlDataReader</returns>
        public DbDataReader ExecuteReaderTypedParams(DbTransaction transaction, string spName, DataRow dataRow)
        {
            return this._dbHelper.ExecuteReaderTypedParams(transaction, spName, dataRow);
        }
        /// <summary>
        ///       执行指定连接数据库连接字符串的存储过程,使用DataRow做为参数值,返回结果集中的第一行第一列.
        ///       </summary>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public object ExecuteScalarTypedParams(string spName, DataRow dataRow)
        {
            return this.ExecuteScalarTypedParams(this._strConnectionString, spName, dataRow);
        }
        /// <summary>
        ///       执行指定连接数据库连接字符串的存储过程,使用DataRow做为参数值,返回结果集中的第一行第一列.
        ///       </summary>
        /// <param name="connectionString">一个有效的数据库连接字符串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public object ExecuteScalarTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            return this._dbHelper.ExecuteScalarTypedParams(connectionString, spName, dataRow);
        }
        /// <summary>
        ///       执行指定连接数据库连接字符串的存储过程,使用DataRow做为参数值,返回结果集中的第一行第一列.
        ///       </summary>
        /// <param name="connection">一个有效的数据库连接字符串</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public object ExecuteScalarTypedParams(DbConnection connection, string spName, DataRow dataRow)
        {
            return this._dbHelper.ExecuteScalarTypedParams(connection, spName, dataRow);
        }
        /// <summary>
        ///       执行指定连接数据库事务的存储过程,使用DataRow做为参数值,返回结果集中的第一行第一列.
        ///       </summary>
        /// <param name="transaction">一个有效的连接事务 object</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回结果集中的第一行第一列</returns>
        public object ExecuteScalarTypedParams(DbTransaction transaction, string spName, DataRow dataRow)
        {
            return this._dbHelper.ExecuteScalarTypedParams(transaction, spName, dataRow);
        }
        /// <summary>
        ///       执行指定连接数据库连接对象的存储过程,使用DataRow做为参数值,返回XmlReader类型的结果集.
        ///       </summary>
        /// <param name="connection">一个有效的数据库连接对象</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回XmlReader结果集对象.</returns>
        public XmlReader ExecuteXmlReaderTypedParams(DbConnection connection, string spName, DataRow dataRow)
        {
            return this._dbHelper.ExecuteXmlReaderTypedParams(connection, spName, dataRow);
        }
        /// <summary>
        ///       执行指定连接数据库事务的存储过程,使用DataRow做为参数值,返回XmlReader类型的结果集.
        ///       </summary>
        /// <param name="transaction">一个有效的连接事务 object</param>
        /// <param name="spName">存储过程名称</param>
        /// <param name="dataRow">使用DataRow作为参数值</param>
        /// <returns>返回XmlReader结果集对象.</returns>
        public XmlReader ExecuteXmlReaderTypedParams(DbTransaction transaction, string spName, DataRow dataRow)
        {
            return this._dbHelper.ExecuteXmlReaderTypedParams(transaction, spName, dataRow);
        }
    }
}
