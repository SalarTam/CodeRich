using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

namespace Code.Framework.Db
{
    public sealed class SqlHelperParameterCache
    {
        private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());
        private SqlHelperParameterCache()
        {
        }
        /// <summary>
        ///       Resolve at run time the appropriate set of SqlParameters for a stored procedure
        ///       </summary>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">Whether or not to include their return value parameter</param>
        /// <returns>The parameter array discovered.</returns>
        private static SqlParameter[] DiscoverSpParameterSet(SqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            SqlCommand cmd = new SqlCommand(spName, connection);
            cmd.CommandType = CommandType.StoredProcedure;
            connection.Open();
            SqlCommandBuilder.DeriveParameters(cmd);
            connection.Close();
            if (!includeReturnValueParameter)
            {
                cmd.Parameters.RemoveAt(0);
            }
            SqlParameter[] discoveredParameters = new SqlParameter[cmd.Parameters.Count];
            cmd.Parameters.CopyTo(discoveredParameters, 0);
            SqlParameter[] array = discoveredParameters;
            for (int i = 0; i < array.Length; i++)
            {
                SqlParameter discoveredParameter = array[i];
                discoveredParameter.Value = DBNull.Value;
            }
            return discoveredParameters;
        }
        /// <summary>
        ///       Deep copy of cached SqlParameter array
        ///       </summary>
        /// <param name="originalParameters">
        /// </param>
        /// <returns>
        /// </returns>
        private static SqlParameter[] CloneParameters(SqlParameter[] originalParameters)
        {
            SqlParameter[] clonedParameters = new SqlParameter[originalParameters.Length];
            int i = 0;
            int j = originalParameters.Length;
            while (i < j)
            {
                clonedParameters[i] = (SqlParameter)((ICloneable)originalParameters[i]).Clone();
                i++;
            }
            return clonedParameters;
        }
        /// <summary>
        ///       Add parameter array to the cache
        ///       </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters to be cached</param>
        public static void CacheParameterSet(string connectionString, string commandText, params SqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (commandText == null || commandText.Length == 0)
            {
                throw new ArgumentNullException("commandText");
            }
            string hashKey = connectionString + ":" + commandText;
            SqlHelperParameterCache.paramCache[hashKey] = commandParameters;
        }
        /// <summary>
        ///       Retrieve a parameter array from the cache
        ///       </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An array of SqlParamters</returns>
        public static SqlParameter[] GetCachedParameterSet(string connectionString, string commandText)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (commandText == null || commandText.Length == 0)
            {
                throw new ArgumentNullException("commandText");
            }
            string hashKey = connectionString + ":" + commandText;
            SqlParameter[] cachedParameters = SqlHelperParameterCache.paramCache[hashKey] as SqlParameter[];
            SqlParameter[] result;
            if (cachedParameters == null)
            {
                result = null;
            }
            else
            {
                result = SqlHelperParameterCache.CloneParameters(cachedParameters);
            }
            return result;
        }
        /// <summary>
        ///       Retrieves the set of SqlParameters appropriate for the stored procedure
        ///       </summary>
        /// <remarks>
        ///       This method will query the database for this information, and then store it in a cache for future requests.
        ///       </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <returns>An array of SqlParameters</returns>
        public static SqlParameter[] GetSpParameterSet(string connectionString, string spName)
        {
            return SqlHelperParameterCache.GetSpParameterSet(connectionString, spName, false);
        }
        /// <summary>
        ///       Retrieves the set of SqlParameters appropriate for the stored procedure
        ///       </summary>
        /// <remarks>
        ///       This method will query the database for this information, and then store it in a cache for future requests.
        ///       </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of SqlParameters</returns>
        public static SqlParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            SqlParameter[] spParameterSetInternal;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                spParameterSetInternal = SqlHelperParameterCache.GetSpParameterSetInternal(connection, spName, includeReturnValueParameter);
            }
            return spParameterSetInternal;
        }
        /// <summary>
        ///       Retrieves the set of SqlParameters appropriate for the stored procedure
        ///       </summary>
        /// <remarks>
        ///       This method will query the database for this information, and then store it in a cache for future requests.
        ///       </remarks>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <returns>An array of SqlParameters</returns>
        internal static SqlParameter[] GetSpParameterSet(SqlConnection connection, string spName)
        {
            return SqlHelperParameterCache.GetSpParameterSet(connection, spName, false);
        }
        /// <summary>
        ///       Retrieves the set of SqlParameters appropriate for the stored procedure
        ///       </summary>
        /// <remarks>
        ///       This method will query the database for this information, and then store it in a cache for future requests.
        ///       </remarks>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of SqlParameters</returns>
        internal static SqlParameter[] GetSpParameterSet(SqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            SqlParameter[] spParameterSetInternal;
            using (SqlConnection clonedConnection = (SqlConnection)((ICloneable)connection).Clone())
            {
                spParameterSetInternal = SqlHelperParameterCache.GetSpParameterSetInternal(clonedConnection, spName, includeReturnValueParameter);
            }
            return spParameterSetInternal;
        }
        /// <summary>
        ///       Retrieves the set of SqlParameters appropriate for the stored procedure
        ///       </summary>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of SqlParameters</returns>
        private static SqlParameter[] GetSpParameterSetInternal(SqlConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            string hashKey = connection.ConnectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : "");
            SqlParameter[] cachedParameters = SqlHelperParameterCache.paramCache[hashKey] as SqlParameter[];
            if (cachedParameters == null)
            {
                SqlParameter[] spParameters = SqlHelperParameterCache.DiscoverSpParameterSet(connection, spName, includeReturnValueParameter);
                SqlHelperParameterCache.paramCache[hashKey] = spParameters;
                cachedParameters = spParameters;
            }
            return SqlHelperParameterCache.CloneParameters(cachedParameters);
        }
    }
}
