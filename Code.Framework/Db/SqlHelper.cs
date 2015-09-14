using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Xml;

namespace Code.Framework.Db
{
    internal sealed class SqlHelper : IDbHelper
    {
        /// <summary>
        ///       This enum is used to indicate whether the connection was provided by the caller, or created by SqlHelper, so that
        ///       we can set the appropriate CommandBehavior when calling ExecuteReader()
        ///       </summary>
        private enum SqlConnectionOwnership
        {
            Internal,
            External
        }
        /// <summary>
        ///       This method is used to attach array of SqlParameters to a SqlCommand.
        ///
        ///       This method will assign a value of DbNull to any parameter with a direction of
        ///       InputOutput and a value of null.  
        ///
        ///       This behavior will prevent default values from being used, but
        ///       this will be the less common case than an intended pure output parameter (derived as InputOutput)
        ///       where the user provided no input value.
        ///       </summary>
        /// <param name="command">The command to which the parameters will be added</param>
        /// <param name="commandParameters">An array of SqlParameters to be added to command</param>
        private void AttachParameters(SqlCommand command, SqlParameter[] commandParameters)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (commandParameters != null)
            {
                for (int i = 0; i < commandParameters.Length; i++)
                {
                    SqlParameter p = commandParameters[i];
                    if (p != null)
                    {
                        if ((p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Input) && p.Value == null)
                        {
                            p.Value = DBNull.Value;
                        }
                        command.Parameters.Add(p);
                    }
                }
            }
        }
        /// <summary>
        ///       This method assigns dataRow column values to an array of SqlParameters
        ///       </summary>
        /// <param name="commandParameters">Array of SqlParameters to be assigned values</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values</param>
        private void AssignParameterValues(SqlParameter[] commandParameters, DataRow dataRow)
        {
            if (commandParameters != null && dataRow != null)
            {
                int i = 0;
                for (int j = 0; j < commandParameters.Length; j++)
                {
                    SqlParameter commandParameter = commandParameters[j];
                    if (commandParameter.ParameterName == null || commandParameter.ParameterName.Length <= 1)
                    {
                        throw new Exception(string.Format("Please provide a valid parameter name on the parameter #{0}, the ParameterName property has the following value: '{1}'.", i, commandParameter.ParameterName));
                    }
                    if (dataRow.Table.Columns.IndexOf(commandParameter.ParameterName.Substring(1)) != -1)
                    {
                        commandParameter.Value = dataRow[commandParameter.ParameterName.Substring(1)];
                    }
                    i++;
                }
            }
        }
        /// <summary>
        ///       This method assigns an array of values to an array of SqlParameters
        ///       </summary>
        /// <param name="commandParameters">Array of SqlParameters to be assigned values</param>
        /// <param name="parameterValues">Array of objects holding the values to be assigned</param>
        private void AssignParameterValues(SqlParameter[] commandParameters, object[] parameterValues)
        {
            if (commandParameters != null && parameterValues != null)
            {
                if (commandParameters.Length != parameterValues.Length)
                {
                    throw new ArgumentException("Parameter count does not match Parameter Value count.");
                }
                int i = 0;
                int j = commandParameters.Length;
                while (i < j)
                {
                    if (parameterValues[i] is IDbDataParameter)
                    {
                        IDbDataParameter paramInstance = (IDbDataParameter)parameterValues[i];
                        if (paramInstance.Value == null)
                        {
                            commandParameters[i].Value = DBNull.Value;
                        }
                        else
                        {
                            commandParameters[i].Value = paramInstance.Value;
                        }
                    }
                    else if (parameterValues[i] == null)
                    {
                        commandParameters[i].Value = DBNull.Value;
                    }
                    else
                    {
                        commandParameters[i].Value = parameterValues[i];
                    }
                    i++;
                }
            }
        }
        /// <summary>
        ///       This method opens (if necessary) and assigns a connection, transaction, command type and parameters 
        ///       to the provided command
        ///       </summary>
        /// <param name="command">The SqlCommand to be prepared</param>
        /// <param name="connection">A valid SqlConnection, on which to execute this command</param>
        /// <param name="transaction">A valid SqlTransaction, or 'null'</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParameters to be associated with the command or 'null' if no parameters are required</param>
        /// <param name="mustCloseConnection">
        ///   <c>true</c> if the connection was opened by the method, otherwose is false.</param>
        private void PrepareCommand(DbCommand command, DbConnection connection, DbTransaction transaction, CommandType commandType, string commandText, DbParameter[] commandParameters, out bool mustCloseConnection)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (commandText == null || commandText.Length == 0)
            {
                throw new ArgumentNullException("commandText");
            }
            if (connection.State != ConnectionState.Open)
            {
                mustCloseConnection = true;
                connection.Open();
            }
            else
            {
                mustCloseConnection = false;
            }
            command.Connection = connection;
            command.CommandText = commandText;
            if (transaction != null)
            {
                if (transaction.Connection == null)
                {
                    throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
                }
                command.Transaction = transaction;
            }
            command.CommandType = commandType;
            if (commandParameters != null)
            {
                this.AttachParameters(command as SqlCommand, commandParameters as SqlParameter[]);
            }
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns no resultset and takes no parameters) against the database specified in 
        ///       the connection string
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders");
        ///       </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            return this.ExecuteNonQuery(connectionString, commandType, commandText, null);
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns no resultset) against the database specified in the connection string 
        ///       using the provided parameters
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            int result;
            try
            {
                if (connectionString == null || connectionString.Length == 0)
                {
                    throw new ArgumentNullException("connectionString");
                }
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    result = this.ExecuteNonQuery(connection, commandType, commandText, commandParameters);
                }
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message);
            }
            return result;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in 
        ///       the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <remarks>
        ///       This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        ///       e.g.:  
        ///        int result = ExecuteNonQuery(connString, "PublishOrders", 24, 36);
        ///       </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored prcedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            int result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                this.AssignParameterValues(commandParameters, parameterValues);
                result = this.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns no resultset and takes no parameters) against the provided SqlConnection. 
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders");
        ///       </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public int ExecuteNonQuery(DbConnection connection, CommandType commandType, string commandText)
        {
            return this.ExecuteNonQuery(connection, commandType, commandText, null);
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns no resultset) against the specified SqlConnection 
        ///       using the provided parameters.
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        int result = ExecuteNonQuery(conn, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public int ExecuteNonQuery(DbConnection connection, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection = false;
            this.PrepareCommand(cmd, connection as SqlConnection, null, commandType, commandText, commandParameters as SqlParameter[], out mustCloseConnection);
            int retval = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            if (mustCloseConnection)
            {
                connection.Close();
            }
            return retval;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified SqlConnection 
        ///       using the provided parameter values.  This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <remarks>
        ///       This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        ///       e.g.:  
        ///        int result = ExecuteNonQuery(conn, "PublishOrders", 24, 36);
        ///       </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public int ExecuteNonQuery(DbConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            int result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection as SqlConnection, spName);
                this.AssignParameterValues(commandParameters, parameterValues);
                result = this.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns no resultset and takes no parameters) against the provided SqlTransaction. 
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "PublishOrders");
        ///       </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public int ExecuteNonQuery(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return this.ExecuteNonQuery(transaction, commandType, commandText, null);
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns no resultset) against the specified SqlTransaction
        ///       using the provided parameters.
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        int result = ExecuteNonQuery(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public int ExecuteNonQuery(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection = false;
            this.PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
            int retval = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return retval;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified 
        ///       SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <remarks>
        ///       This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        ///       e.g.:  
        ///        int result = ExecuteNonQuery(conn, trans, "PublishOrders", 24, 36);
        ///       </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public int ExecuteNonQuery(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            int result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection as SqlConnection, spName);
                this.AssignParameterValues(commandParameters, parameterValues);
                result = this.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in 
        ///       the connection string. 
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders");
        ///       </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        {
            return this.ExecuteDataset(connectionString, commandType, commandText, null);
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a resultset) against the database specified in the connection string 
        ///       using the provided parameters.
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            DataSet result;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                result = this.ExecuteDataset(connection, commandType, commandText, commandParameters);
            }
            return result;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
        ///       the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <remarks>
        ///       This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        ///       e.g.:  
        ///        DataSet ds = ExecuteDataset(connString, "GetOrders", 24, 36);
        ///       </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            DataSet result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                this.AssignParameterValues(commandParameters, parameterValues);
                result = this.ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders");
        ///       </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet ExecuteDataset(DbConnection connection, CommandType commandType, string commandText)
        {
            return this.ExecuteDataset(connection, commandType, commandText, null);
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
        ///       using the provided parameters.
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet ExecuteDataset(DbConnection connection, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection = false;
            this.PrepareCommand(cmd, connection, null, commandType, commandText, commandParameters, out mustCloseConnection);
            DataSet result;
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();
                da.Fill(ds);
                cmd.Parameters.Clear();
                if (mustCloseConnection)
                {
                    connection.Close();
                }
                result = ds;
            }
            return result;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
        ///       using the provided parameter values.  This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <remarks>
        ///       This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        ///       e.g.:  
        ///        DataSet ds = ExecuteDataset(conn, "GetOrders", 24, 36);
        ///       </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet ExecuteDataset(DbConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            DataSet result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection as SqlConnection, spName);
                this.AssignParameterValues(commandParameters, parameterValues);
                result = this.ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteDataset(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
        ///       </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet ExecuteDataset(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return this.ExecuteDataset(transaction, commandType, commandText, null);
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
        ///       using the provided parameters.
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        DataSet ds = ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet ExecuteDataset(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection = false;
            this.PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
            DataSet result;
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();
                da.Fill(ds);
                cmd.Parameters.Clear();
                result = ds;
            }
            return result;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified 
        ///       SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <remarks>
        ///       This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        ///       e.g.:  
        ///        DataSet ds = ExecuteDataset(trans, "GetOrders", 24, 36);
        ///       </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet ExecuteDataset(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            DataSet result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection as SqlConnection, spName);
                this.AssignParameterValues(commandParameters, parameterValues);
                result = this.ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Create and prepare a SqlCommand, and call ExecuteReader with the appropriate CommandBehavior.
        ///       </summary>
        /// <remarks>
        ///       If we created and opened the connection, we want the connection to be closed when the DataReader is closed.
        ///
        ///       If the caller provided the connection, we want to leave it to them to manage.
        ///       </remarks>
        /// <param name="connection">A valid SqlConnection, on which to execute this command</param>
        /// <param name="transaction">A valid SqlTransaction, or 'null'</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParameters to be associated with the command or 'null' if no parameters are required</param>
        /// <param name="connectionOwnership">Indicates whether the connection parameter was provided by the caller, or created by SqlHelper</param>
        /// <returns>SqlDataReader containing the results of the command</returns>
        private SqlDataReader ExecuteReader(SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters, SqlHelper.SqlConnectionOwnership connectionOwnership)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            bool mustCloseConnection = false;
            SqlCommand cmd = new SqlCommand();
            SqlDataReader result;
            try
            {
                this.PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
                SqlDataReader dataReader;
                if (connectionOwnership == SqlHelper.SqlConnectionOwnership.External)
                {
                    dataReader = cmd.ExecuteReader();
                }
                else
                {
                    dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
                bool canClear = true;
                foreach (SqlParameter commandParameter in cmd.Parameters)
                {
                    if (commandParameter.Direction != ParameterDirection.Input)
                    {
                        canClear = false;
                    }
                }
                if (canClear)
                {
                    cmd.Parameters.Clear();
                }
                result = dataReader;
            }
            catch
            {
                if (mustCloseConnection)
                {
                    connection.Close();
                }
                throw;
            }
            return result;
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in 
        ///       the connection string. 
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        SqlDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders");
        ///       </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public DbDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        {
            return this.ExecuteReader(connectionString, commandType, commandText, null);
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a resultset) against the database specified in the connection string 
        ///       using the provided parameters.
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        SqlDataReader dr = ExecuteReader(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public DbDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            SqlConnection connection = null;
            DbDataReader result;
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();
                result = this.ExecuteReader(connection, null, commandType, commandText, commandParameters as SqlParameter[], SqlHelper.SqlConnectionOwnership.Internal);
            }
            catch
            {
                if (connection != null)
                {
                    connection.Close();
                }
                throw;
            }
            return result;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
        ///       the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <remarks>
        ///       This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        ///       e.g.:  
        ///        SqlDataReader dr = ExecuteReader(connString, "GetOrders", 24, 36);
        ///       </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public DbDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            DbDataReader result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                this.AssignParameterValues(commandParameters, parameterValues);
                result = this.ExecuteReader(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        SqlDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders");
        ///       </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public DbDataReader ExecuteReader(DbConnection connection, CommandType commandType, string commandText)
        {
            return this.ExecuteReader(connection, commandType, commandText, null);
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
        ///       using the provided parameters.
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        SqlDataReader dr = ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public DbDataReader ExecuteReader(DbConnection connection, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            return this.ExecuteReader(connection as SqlConnection, null, commandType, commandText, commandParameters as SqlParameter[], SqlHelper.SqlConnectionOwnership.External);
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
        ///       using the provided parameter values.  This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <remarks>
        ///       This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        ///       e.g.:  
        ///        SqlDataReader dr = ExecuteReader(conn, "GetOrders", 24, 36);
        ///       </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public DbDataReader ExecuteReader(DbConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            DbDataReader result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection as SqlConnection, spName);
                this.AssignParameterValues(commandParameters, parameterValues);
                result = this.ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteReader(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        SqlDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders");
        ///       </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public DbDataReader ExecuteReader(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return this.ExecuteReader(transaction, commandType, commandText, null);
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
        ///       using the provided parameters.
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///         SqlDataReader dr = ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public DbDataReader ExecuteReader(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            return this.ExecuteReader(transaction.Connection as SqlConnection, transaction as SqlTransaction, commandType, commandText, commandParameters as SqlParameter[], SqlHelper.SqlConnectionOwnership.External);
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified
        ///       SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <remarks>
        ///       This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        ///       e.g.:  
        ///        SqlDataReader dr = ExecuteReader(trans, "GetOrders", 24, 36);
        ///       </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public DbDataReader ExecuteReader(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            DbDataReader result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection as SqlConnection, spName);
                this.AssignParameterValues(commandParameters, parameterValues);
                result = this.ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteReader(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the database specified in 
        ///       the connection string. 
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount");
        ///       </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        {
            return this.ExecuteScalar(connectionString, commandType, commandText, null);
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a 1x1 resultset) against the database specified in the connection string 
        ///       using the provided parameters.
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        int orderCount = (int)ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            object result;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                result = this.ExecuteScalar(connection, commandType, commandText, commandParameters);
            }
            return result;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the database specified in 
        ///       the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <remarks>
        ///       This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        ///       e.g.:  
        ///        int orderCount = (int)ExecuteScalar(connString, "GetOrderCount", 24, 36);
        ///       </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            object result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                this.AssignParameterValues(commandParameters, parameterValues);
                result = this.ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the provided SqlConnection. 
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount");
        ///       </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public object ExecuteScalar(DbConnection connection, CommandType commandType, string commandText)
        {
            return this.ExecuteScalar(connection, commandType, commandText, null);
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a 1x1 resultset) against the specified SqlConnection 
        ///       using the provided parameters.
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        int orderCount = (int)ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public object ExecuteScalar(DbConnection connection, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection = false;
            this.PrepareCommand(cmd, connection, null, commandType, commandText, commandParameters, out mustCloseConnection);
            object retval = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            if (mustCloseConnection)
            {
                connection.Close();
            }
            return retval;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified SqlConnection 
        ///       using the provided parameter values.  This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <remarks>
        ///       This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        ///       e.g.:  
        ///        int orderCount = (int)ExecuteScalar(conn, "GetOrderCount", 24, 36);
        ///       </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public object ExecuteScalar(DbConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            object result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection as SqlConnection, spName);
                this.AssignParameterValues(commandParameters, parameterValues);
                result = this.ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteScalar(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a 1x1 resultset and takes no parameters) against the provided SqlTransaction. 
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount");
        ///       </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public object ExecuteScalar(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return this.ExecuteScalar(transaction, commandType, commandText, null);
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a 1x1 resultset) against the specified SqlTransaction
        ///       using the provided parameters.
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        int orderCount = (int)ExecuteScalar(trans, CommandType.StoredProcedure, "GetOrderCount", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public object ExecuteScalar(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection = false;
            this.PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
            object retval = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return retval;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified
        ///       SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <remarks>
        ///       This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        ///       e.g.:  
        ///        int orderCount = (int)ExecuteScalar(trans, "GetOrderCount", 24, 36);
        ///       </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public object ExecuteScalar(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            object result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection as SqlConnection, spName);
                this.AssignParameterValues(commandParameters, parameterValues);
                result = this.ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        XmlReader r = ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders");
        ///       </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO"</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public XmlReader ExecuteXmlReader(DbConnection connection, CommandType commandType, string commandText)
        {
            return this.ExecuteXmlReader(connection, commandType, commandText, null);
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
        ///       using the provided parameters.
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        XmlReader r = ExecuteXmlReader(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO"</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public XmlReader ExecuteXmlReader(DbConnection connection, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            bool mustCloseConnection = false;
            SqlCommand cmd = new SqlCommand();
            XmlReader result;
            try
            {
                this.PrepareCommand(cmd, connection, null, commandType, commandText, commandParameters, out mustCloseConnection);
                XmlReader retval = cmd.ExecuteXmlReader();
                cmd.Parameters.Clear();
                result = retval;
            }
            catch
            {
                if (mustCloseConnection)
                {
                    connection.Close();
                }
                throw;
            }
            return result;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
        ///       using the provided parameter values.  This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <remarks>
        ///       This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        ///       e.g.:  
        ///        XmlReader r = ExecuteXmlReader(conn, "GetOrders", 24, 36);
        ///       </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="spName">The name of the stored procedure using "FOR XML AUTO"</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public XmlReader ExecuteXmlReader(DbConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            XmlReader result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection as SqlConnection, spName);
                this.AssignParameterValues(commandParameters, parameterValues);
                result = this.ExecuteXmlReader(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteXmlReader(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        XmlReader r = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders");
        ///       </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO"</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public XmlReader ExecuteXmlReader(DbTransaction transaction, CommandType commandType, string commandText)
        {
            return this.ExecuteXmlReader(transaction, commandType, commandText, null);
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
        ///       using the provided parameters.
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        XmlReader r = ExecuteXmlReader(trans, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command using "FOR XML AUTO"</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public XmlReader ExecuteXmlReader(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection = false;
            this.PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
            XmlReader retval = cmd.ExecuteXmlReader();
            cmd.Parameters.Clear();
            return retval;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified 
        ///       SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <remarks>
        ///       This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        ///       e.g.:  
        ///        XmlReader r = ExecuteXmlReader(trans, "GetOrders", 24, 36);
        ///       </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public XmlReader ExecuteXmlReader(DbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            XmlReader result;
            if (parameterValues != null && parameterValues.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection as SqlConnection, spName);
                this.AssignParameterValues(commandParameters, parameterValues);
                result = this.ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a resultset and takes no parameters) against the database specified in 
        ///       the connection string. 
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        ///       </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        ///       by a user defined name (probably the actual table name)</param>
        public void FillDataset(string connectionString, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                this.FillDataset(connection, commandType, commandText, dataSet, tableNames);
            }
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a resultset) against the database specified in the connection string 
        ///       using the provided parameters.
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        ///       by a user defined name (probably the actual table name)
        ///       </param>
        public void FillDataset(string connectionString, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params DbParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                this.FillDataset(connection, commandType, commandText, dataSet, tableNames, commandParameters);
            }
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
        ///       the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <remarks>
        ///       This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        ///       e.g.:  
        ///        FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, 24);
        ///       </remarks>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        ///       by a user defined name (probably the actual table name)
        ///       </param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        public void FillDataset(string connectionString, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                this.FillDataset(connection, spName, dataSet, tableNames, parameterValues);
            }
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlConnection. 
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        ///       </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        ///       by a user defined name (probably the actual table name)
        ///       </param>
        public void FillDataset(DbConnection connection, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            this.FillDataset(connection, commandType, commandText, dataSet, tableNames, null);
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
        ///       using the provided parameters.
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        ///       by a user defined name (probably the actual table name)
        ///       </param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        public void FillDataset(DbConnection connection, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params DbParameter[] commandParameters)
        {
            this.FillDataset(connection, null, commandType, commandText, dataSet, tableNames, commandParameters);
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
        ///       using the provided parameter values.  This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <remarks>
        ///       This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        ///       e.g.:  
        ///        FillDataset(conn, "GetOrders", ds, new string[] {"orders"}, 24, 36);
        ///       </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        ///       by a user defined name (probably the actual table name)
        ///       </param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        public void FillDataset(DbConnection connection, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            if (parameterValues != null && parameterValues.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection as SqlConnection, spName);
                this.AssignParameterValues(commandParameters, parameterValues);
                this.FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames, commandParameters);
            }
            else
            {
                this.FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames);
            }
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a resultset and takes no parameters) against the provided SqlTransaction. 
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        FillDataset(trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        ///       </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        ///       by a user defined name (probably the actual table name)
        ///       </param>
        public void FillDataset(DbTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            this.FillDataset(transaction, commandType, commandText, dataSet, tableNames, null);
        }
        /// <summary>
        ///       Execute a SqlCommand (that returns a resultset) against the specified SqlTransaction
        ///       using the provided parameters.
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        FillDataset(trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        ///       by a user defined name (probably the actual table name)
        ///       </param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        public void FillDataset(DbTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params DbParameter[] commandParameters)
        {
            this.FillDataset(transaction.Connection, transaction, commandType, commandText, dataSet, tableNames, commandParameters);
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified 
        ///       SqlTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <remarks>
        ///       This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        ///       e.g.:  
        ///        FillDataset(trans, "GetOrders", ds, new string[]{"orders"}, 24, 36);
        ///       </remarks>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        ///       by a user defined name (probably the actual table name)
        ///       </param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        public void FillDataset(DbTransaction transaction, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            if (parameterValues != null && parameterValues.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection as SqlConnection, spName);
                this.AssignParameterValues(commandParameters, parameterValues);
                this.FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames, commandParameters);
            }
            else
            {
                this.FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames);
            }
        }
        /// <summary>
        ///       Private helper method that execute a SqlCommand (that returns a resultset) against the specified SqlTransaction and SqlConnection
        ///       using the provided parameters.
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        FillDataset(conn, trans, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, new SqlParameter("@prodid", 24));
        ///       </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="transaction">A valid SqlTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        ///       by a user defined name (probably the actual table name)
        ///       </param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        private void FillDataset(DbConnection connection, DbTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params DbParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            SqlCommand command = new SqlCommand();
            bool mustCloseConnection = false;
            this.PrepareCommand(command, connection, transaction, commandType, commandText, commandParameters, out mustCloseConnection);
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(command))
            {
                if (tableNames != null && tableNames.Length > 0)
                {
                    string tableName = "Table";
                    for (int index = 0; index < tableNames.Length; index++)
                    {
                        if (tableNames[index] == null || tableNames[index].Length == 0)
                        {
                            throw new ArgumentException("The tableNames parameter must contain a list of tables, a value was provided as null or empty string.", "tableNames");
                        }
                        dataAdapter.TableMappings.Add(tableName, tableNames[index]);
                        tableName += (index + 1).ToString();
                    }
                }
                dataAdapter.Fill(dataSet);
                command.Parameters.Clear();
            }
            if (mustCloseConnection)
            {
                connection.Close();
            }
        }
        /// <summary>
        ///       Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        UpdateDataset(conn, insertCommand, deleteCommand, updateCommand, dataSet, "Order");
        ///       </remarks>
        /// <param name="insertCommand">A valid transact-SQL statement or stored procedure to insert new records into the data source</param>
        /// <param name="deleteCommand">A valid transact-SQL statement or stored procedure to delete records from the data source</param>
        /// <param name="updateCommand">A valid transact-SQL statement or stored procedure used to update records in the data source</param>
        /// <param name="dataSet">The DataSet used to update the data source</param>
        /// <param name="tableName">The DataTable used to update the data source.</param>
        public void UpdateDataset(DbCommand insertCommand, DbCommand deleteCommand, DbCommand updateCommand, DataSet dataSet, string tableName)
        {
            if (insertCommand == null)
            {
                throw new ArgumentNullException("insertCommand");
            }
            if (deleteCommand == null)
            {
                throw new ArgumentNullException("deleteCommand");
            }
            if (updateCommand == null)
            {
                throw new ArgumentNullException("updateCommand");
            }
            if (tableName == null || tableName.Length == 0)
            {
                throw new ArgumentNullException("tableName");
            }
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter())
            {
                dataAdapter.UpdateCommand = (updateCommand as SqlCommand);
                dataAdapter.InsertCommand = (insertCommand as SqlCommand);
                dataAdapter.DeleteCommand = (deleteCommand as SqlCommand);
                dataAdapter.Update(dataSet, tableName);
                dataSet.AcceptChanges();
            }
        }
        /// <summary>
        ///       Simplify the creation of a Sql command object by allowing
        ///       a stored procedure and optional parameters to be provided
        ///       </summary>
        /// <remarks>
        ///       e.g.:  
        ///        SqlCommand command = CreateCommand(conn, "AddCustomer", "CustomerID", "CustomerName");
        ///       </remarks>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="sourceColumns">An array of string to be assigned as the source columns of the stored procedure parameters</param>
        /// <returns>A valid SqlCommand object</returns>
        public DbCommand CreateCommand(DbConnection connection, string spName, params string[] sourceColumns)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            SqlCommand cmd = new SqlCommand(spName, connection as SqlConnection);
            cmd.CommandType = CommandType.StoredProcedure;
            if (sourceColumns != null && sourceColumns.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection as SqlConnection, spName);
                for (int index = 0; index < sourceColumns.Length; index++)
                {
                    commandParameters[index].SourceColumn = sourceColumns[index];
                }
                this.AttachParameters(cmd, commandParameters);
            }
            return cmd;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns no resultset) against the database specified in 
        ///       the connection string using the dataRow column values as the stored procedure's parameters values.
        ///       This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        ///       </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public int ExecuteNonQueryTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            int result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                this.AssignParameterValues(commandParameters, dataRow);
                result = this.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified SqlConnection 
        ///       using the dataRow column values as the stored procedure's parameters values.  
        ///       This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        ///       </summary>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public int ExecuteNonQueryTypedParams(DbConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            int result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection as SqlConnection, spName);
                this.AssignParameterValues(commandParameters, dataRow);
                result = this.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns no resultset) against the specified
        ///       SqlTransaction using the dataRow column values as the stored procedure's parameters values.
        ///       This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        ///       </summary>
        /// <param name="transaction">A valid SqlTransaction object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        public int ExecuteNonQueryTypedParams(DbTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            int result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection as SqlConnection, spName);
                this.AssignParameterValues(commandParameters, dataRow);
                result = this.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
        ///       the connection string using the dataRow column values as the stored procedure's parameters values.
        ///       This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        ///       </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet ExecuteDatasetTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            DataSet result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                this.AssignParameterValues(commandParameters, dataRow);
                result = this.ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
        ///       using the dataRow column values as the store procedure's parameters values.
        ///       This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        ///       </summary>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet ExecuteDatasetTypedParams(DbConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            DataSet result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection as SqlConnection, spName);
                this.AssignParameterValues(commandParameters, dataRow);
                result = this.ExecuteDataset(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteDataset(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction 
        ///       using the dataRow column values as the stored procedure's parameters values.
        ///       This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        ///       </summary>
        /// <param name="transaction">A valid SqlTransaction object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public DataSet ExecuteDatasetTypedParams(DbTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            DataSet result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection as SqlConnection, spName);
                this.AssignParameterValues(commandParameters, dataRow);
                result = this.ExecuteDataset(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a resultset) against the database specified in 
        ///       the connection string using the dataRow column values as the stored procedure's parameters values.
        ///       This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public DbDataReader ExecuteReaderTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            DbDataReader result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                this.AssignParameterValues(commandParameters, dataRow);
                result = this.ExecuteReader(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
        ///       using the dataRow column values as the stored procedure's parameters values.
        ///       This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public DbDataReader ExecuteReaderTypedParams(DbConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            DbDataReader result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection as SqlConnection, spName);
                this.AssignParameterValues(commandParameters, dataRow);
                result = this.ExecuteReader(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteReader(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction 
        ///       using the dataRow column values as the stored procedure's parameters values.
        ///       This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <param name="transaction">A valid SqlTransaction object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A SqlDataReader containing the resultset generated by the command</returns>
        public DbDataReader ExecuteReaderTypedParams(DbTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            DbDataReader result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection as SqlConnection, spName);
                this.AssignParameterValues(commandParameters, dataRow);
                result = this.ExecuteReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteReader(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the database specified in 
        ///       the connection string using the dataRow column values as the stored procedure's parameters values.
        ///       This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <param name="connectionString">A valid connection string for a SqlConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public object ExecuteScalarTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if (connectionString == null || connectionString.Length == 0)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            object result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connectionString, spName);
                this.AssignParameterValues(commandParameters, dataRow);
                result = this.ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified SqlConnection 
        ///       using the dataRow column values as the stored procedure's parameters values.
        ///       This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public object ExecuteScalarTypedParams(DbConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            object result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection as SqlConnection, spName);
                this.AssignParameterValues(commandParameters, dataRow);
                result = this.ExecuteScalar(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteScalar(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a 1x1 resultset) against the specified SqlTransaction
        ///       using the dataRow column values as the stored procedure's parameters values.
        ///       This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <param name="transaction">A valid SqlTransaction object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        public object ExecuteScalarTypedParams(DbTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            object result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection as SqlConnection, spName);
                this.AssignParameterValues(commandParameters, dataRow);
                result = this.ExecuteScalar(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlConnection 
        ///       using the dataRow column values as the stored procedure's parameters values.
        ///       This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <param name="connection">A valid SqlConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public XmlReader ExecuteXmlReaderTypedParams(DbConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            XmlReader result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(connection as SqlConnection, spName);
                this.AssignParameterValues(commandParameters, dataRow);
                result = this.ExecuteXmlReader(connection, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteXmlReader(connection, CommandType.StoredProcedure, spName);
            }
            return result;
        }
        /// <summary>
        ///       Execute a stored procedure via a SqlCommand (that returns a resultset) against the specified SqlTransaction 
        ///       using the dataRow column values as the stored procedure's parameters values.
        ///       This method will query the database to discover the parameters for the 
        ///       stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        ///       </summary>
        /// <param name="transaction">A valid SqlTransaction object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An XmlReader containing the resultset generated by the command</returns>
        public XmlReader ExecuteXmlReaderTypedParams(DbTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
            }
            if (spName == null || spName.Length == 0)
            {
                throw new ArgumentNullException("spName");
            }
            XmlReader result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                SqlParameter[] commandParameters = SqlHelperParameterCache.GetSpParameterSet(transaction.Connection as SqlConnection, spName);
                this.AssignParameterValues(commandParameters, dataRow);
                result = this.ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName, commandParameters);
            }
            else
            {
                result = this.ExecuteXmlReader(transaction, CommandType.StoredProcedure, spName);
            }
            return result;
        }
    }
}
