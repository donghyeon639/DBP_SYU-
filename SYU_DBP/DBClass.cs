using System;
using System.Data;
using Oracle.DataAccess.Client;

namespace SYU_DBP
{
    public class DBClass : IDisposable
    {
        private readonly string _connectionString;
        private OracleConnection _connection;

        public DBClass(string userId, string password, string host = "localhost", int port = 1521, string serviceName = "xe")
        {
            _connectionString = $"User Id={userId};Password={password};Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={host})(PORT={port}))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={serviceName})));";
            _connection = new OracleConnection(_connectionString);
        }

        public OracleConnection Connection => _connection;

        public void Open()
        {
            if (_connection == null)
                _connection = new OracleConnection(_connectionString);
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
        }

        public void Close()
        {
            if (_connection != null && _connection.State != ConnectionState.Closed)
                _connection.Close();
        }

        private OracleCommand CreateCommand(string sql, params OracleParameter[] parameters)
        {
            Open();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;
            if (parameters != null)
            {
                foreach (var p in parameters)
                    cmd.Parameters.Add(p);
            }
            return cmd;
        }

        public object ExecuteScalar(string sql, params OracleParameter[] parameters)
        {
            using (var cmd = CreateCommand(sql, parameters))
            {
                return cmd.ExecuteScalar();
            }
        }

        public int ExecuteNonQuery(string sql, params OracleParameter[] parameters)
        {
            using (var cmd = CreateCommand(sql, parameters))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public DataTable GetDataTable(string sql, params OracleParameter[] parameters)
        {
            using (var cmd = CreateCommand(sql, parameters))
            using (var da = new OracleDataAdapter(cmd))
            {
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public void Dispose()
        {
            Close();
            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}
