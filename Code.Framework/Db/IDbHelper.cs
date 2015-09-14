using System;
using System.Data;
using System.Data.Common;
using System.Xml;

namespace Code.Framework.Db
{
    internal interface IDbHelper
    {
        int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText);
        int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params DbParameter[] commandParameters);
        int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues);
        int ExecuteNonQuery(DbConnection connection, CommandType commandType, string commandText);
        int ExecuteNonQuery(DbConnection connection, CommandType commandType, string commandText, params DbParameter[] commandParameters);
        int ExecuteNonQuery(DbConnection connection, string spName, params object[] parameterValues);
        int ExecuteNonQuery(DbTransaction transaction, CommandType commandType, string commandText);
        int ExecuteNonQuery(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters);
        int ExecuteNonQuery(DbTransaction transaction, string spName, params object[] parameterValues);
        DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText);
        DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params DbParameter[] commandParameters);
        DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues);
        DataSet ExecuteDataset(DbConnection connection, CommandType commandType, string commandText);
        DataSet ExecuteDataset(DbConnection connection, CommandType commandType, string commandText, params DbParameter[] commandParameters);
        DataSet ExecuteDataset(DbConnection connection, string spName, params object[] parameterValues);
        DataSet ExecuteDataset(DbTransaction transaction, CommandType commandType, string commandText);
        DataSet ExecuteDataset(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters);
        DataSet ExecuteDataset(DbTransaction transaction, string spName, params object[] parameterValues);
        DbDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText);
        DbDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params DbParameter[] commandParameters);
        DbDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues);
        DbDataReader ExecuteReader(DbConnection connection, CommandType commandType, string commandText);
        DbDataReader ExecuteReader(DbConnection connection, CommandType commandType, string commandText, params DbParameter[] commandParameters);
        DbDataReader ExecuteReader(DbConnection connection, string spName, params object[] parameterValues);
        DbDataReader ExecuteReader(DbTransaction transaction, CommandType commandType, string commandText);
        DbDataReader ExecuteReader(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters);
        DbDataReader ExecuteReader(DbTransaction transaction, string spName, params object[] parameterValues);
        object ExecuteScalar(string connectionString, CommandType commandType, string commandText);
        object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params DbParameter[] commandParameters);
        object ExecuteScalar(string connectionString, string spName, params object[] parameterValues);
        object ExecuteScalar(DbConnection connection, CommandType commandType, string commandText);
        object ExecuteScalar(DbConnection connection, CommandType commandType, string commandText, params DbParameter[] commandParameters);
        object ExecuteScalar(DbConnection connection, string spName, params object[] parameterValues);
        object ExecuteScalar(DbTransaction transaction, CommandType commandType, string commandText);
        object ExecuteScalar(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters);
        object ExecuteScalar(DbTransaction transaction, string spName, params object[] parameterValues);
        XmlReader ExecuteXmlReader(DbConnection connection, CommandType commandType, string commandText);
        XmlReader ExecuteXmlReader(DbConnection connection, CommandType commandType, string commandText, params DbParameter[] commandParameters);
        XmlReader ExecuteXmlReader(DbConnection connection, string spName, params object[] parameterValues);
        XmlReader ExecuteXmlReader(DbTransaction transaction, CommandType commandType, string commandText);
        XmlReader ExecuteXmlReader(DbTransaction transaction, CommandType commandType, string commandText, params DbParameter[] commandParameters);
        XmlReader ExecuteXmlReader(DbTransaction transaction, string spName, params object[] parameterValues);
        void FillDataset(string connectionString, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames);
        void FillDataset(string connectionString, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params DbParameter[] commandParameters);
        void FillDataset(string connectionString, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues);
        void FillDataset(DbConnection connection, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames);
        void FillDataset(DbConnection connection, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params DbParameter[] commandParameters);
        void FillDataset(DbConnection connection, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues);
        void FillDataset(DbTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames);
        void FillDataset(DbTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params DbParameter[] commandParameters);
        void FillDataset(DbTransaction transaction, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues);
        void UpdateDataset(DbCommand insertCommand, DbCommand deleteCommand, DbCommand updateCommand, DataSet dataSet, string tableName);
        DbCommand CreateCommand(DbConnection connection, string spName, params string[] sourceColumns);
        int ExecuteNonQueryTypedParams(string connectionString, string spName, DataRow dataRow);
        int ExecuteNonQueryTypedParams(DbConnection connection, string spName, DataRow dataRow);
        int ExecuteNonQueryTypedParams(DbTransaction transaction, string spName, DataRow dataRow);
        DataSet ExecuteDatasetTypedParams(string connectionString, string spName, DataRow dataRow);
        DataSet ExecuteDatasetTypedParams(DbConnection connection, string spName, DataRow dataRow);
        DataSet ExecuteDatasetTypedParams(DbTransaction transaction, string spName, DataRow dataRow);
        DbDataReader ExecuteReaderTypedParams(string connectionString, string spName, DataRow dataRow);
        DbDataReader ExecuteReaderTypedParams(DbConnection connection, string spName, DataRow dataRow);
        DbDataReader ExecuteReaderTypedParams(DbTransaction transaction, string spName, DataRow dataRow);
        object ExecuteScalarTypedParams(string connectionString, string spName, DataRow dataRow);
        object ExecuteScalarTypedParams(DbConnection connection, string spName, DataRow dataRow);
        object ExecuteScalarTypedParams(DbTransaction transaction, string spName, DataRow dataRow);
        XmlReader ExecuteXmlReaderTypedParams(DbConnection connection, string spName, DataRow dataRow);
        XmlReader ExecuteXmlReaderTypedParams(DbTransaction transaction, string spName, DataRow dataRow);
    }
}
