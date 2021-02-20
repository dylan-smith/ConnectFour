using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ConnectFour.Strategy.BruteForce
{
    public class SqlServerDatabaseLayer : IDisposable
    {
        private SqlConnection _connection;
        private SqlTransaction _transaction;
        private bool _disposedValue;
        private string _connectionString;

        public SqlServerDatabaseLayer(string connectionString)
        {
            ConnectionString = connectionString;
        }

        ~SqlServerDatabaseLayer()
        {
            Dispose(false);
        }

        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }

            set
            {
                _connectionString = value;
                _connection = new SqlConnection(_connectionString);
            }
        }

        public IEnumerable<DataRow> GetDataTable(string sql)
        {
            return GetDataTable(sql, new object[0]);
        }

        public IEnumerable<DataRow> GetDataTable(string sql, params object[] sqlArgs)
        {
            var myCommand = PrepareCommand(sql, sqlArgs);

            var result = new DataTable();

            try
            {
                ExecuteWithConnection(() =>
                {
                    var reader = myCommand.ExecuteReader();
                    result.Load(reader);
                    reader.Close();
                });

                return result.Rows.Cast<DataRow>();
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        public async Task<IEnumerable<DataRow>> GetDataTableAsync(string sql)
        {
            return await GetDataTableAsync(sql, new object[0]);
        }

        public async Task<IEnumerable<DataRow>> GetDataTableAsync(string sql, params object[] sqlArgs)
        {
            var myCommand = PrepareCommand(sql, sqlArgs);

            var result = new DataTable();

            try
            {
                await ExecuteWithConnectionAsync(async () =>
                {
                    var reader = await myCommand.ExecuteReaderAsync();
                    result.Load(reader);
                    reader.Close();
                });

                return result.Rows.Cast<DataRow>();
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        public void BulkCopy(string tableName, DataTable data)
        {
            BulkCopy(tableName, data, false);
        }

        public void BulkCopy(string tableName, DataTable data, bool retryOnFailure)
        {
            var retryCount = 0;

            while (retryCount < 3)
            {
                try
                {
                    ExecuteWithConnection(() =>
                    {
                        var bulkCopy = new SqlBulkCopy(_connection, SqlBulkCopyOptions.TableLock, null);
                        bulkCopy.DestinationTableName = tableName;
                        bulkCopy.BulkCopyTimeout = 0;

                        bulkCopy.WriteToServer(data);
                    });

                    return;
                }
                catch (SqlException)
                {
                    retryCount++;

                    if (retryCount >= 3 || !retryOnFailure)
                    {
                        throw;
                    }
                }
            }
        }

        public async Task BulkCopyAsync(string tableName, DataTable data)
        {
            await ExecuteWithConnectionAsync(async () =>
            {
                var bulkCopy = new SqlBulkCopy(_connection, SqlBulkCopyOptions.TableLock, null);
                bulkCopy.DestinationTableName = tableName;
                bulkCopy.BulkCopyTimeout = 600;

                await bulkCopy.WriteToServerAsync(data);
            });
        }

        public int ExecuteNonQuery(string sql)
        {
            return ExecuteNonQuery(sql, new object[0]);
        }

        public int ExecuteNonQuery(string sql, params object[] sqlArgs)
        {
            var myCommand = PrepareCommand(sql, sqlArgs);
            int result = default(int);

            ExecuteWithConnection(() => result = myCommand.ExecuteNonQuery());

            return result;
        }

        public async Task<int> ExecuteNonQueryAsync(string sql)
        {
            return await ExecuteNonQueryAsync(sql, new object[0]);
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, params object[] sqlArgs)
        {
            var myCommand = PrepareCommand(sql, sqlArgs);
            int result = default(int);

            await ExecuteWithConnectionAsync(async () => result = await myCommand.ExecuteNonQueryAsync());

            return result;
        }

        public object ExecuteScalar(string sql)
        {
            return ExecuteScalar(sql, new object[0]);
        }

        public object ExecuteScalar(string sql, params object[] sqlArgs)
        {
            var myCommand = PrepareCommand(sql, sqlArgs);
            object result = default(object);

            ExecuteWithConnection(() => result = myCommand.ExecuteScalar());

            return result;
        }

        public async Task<object> ExecuteScalarAsync(string sql)
        {
            return await ExecuteScalarAsync(sql, new object[0]);
        }

        public async Task<object> ExecuteScalarAsync(string sql, params object[] sqlArgs)
        {
            var myCommand = PrepareCommand(sql, sqlArgs);
            object result = default(object);

            await ExecuteWithConnectionAsync(async () => result = await myCommand.ExecuteScalarAsync());

            return result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void ExecuteInTransaction(Action work)
        {
            _connection.Open();
            _transaction = _connection.BeginTransaction();

            try
            {
                work();
                _transaction.Commit();
            }
            catch
            {
                _transaction.Rollback();
                throw;
            }
            finally
            {
                _connection.Close();

                _transaction.Dispose();
                _transaction = null;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_connection != null)
                    {
                        if (_connection.State == ConnectionState.Open)
                        {
                            _connection.Close();
                        }

                        _connection.Dispose();
                        _transaction?.Dispose();
                    }
                }
            }

            _disposedValue = true;
        }

        private void CloseConnection()
        {
            if (_transaction == null)
            {
                _connection.Close();
            }
        }

        private void OpenConnection()
        {
            if (_transaction == null)
            {
                _connection.Open();
            }
        }

        private void ExecuteWithConnection(Action work)
        {
            OpenConnection();

            try
            {
                work();
            }
            finally
            {
                CloseConnection();
            }
        }

        private async Task ExecuteWithConnectionAsync(Func<Task> work)
        {
            OpenConnection();

            try
            {
                await work();
            }
            finally
            {
                CloseConnection();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "It will be the responsibility of the caller to ensure they aren't vulnerable to SQL Injection")]
        private SqlCommand PrepareCommand(string sql, params object[] sqlArgs)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException("The SQL statement was blank. A valid SQL Statement must be provided.", "sql");
            }

            var myCommand = new SqlCommand(sql, _connection);

            try
            {
                myCommand.CommandTimeout = 0;
                myCommand.CommandType = CommandType.Text;

                for (int i = 0; i < sqlArgs.Length; i += 2)
                {
                    myCommand.Parameters.AddWithValue((string)sqlArgs[i], sqlArgs[i + 1]);
                }

                myCommand.Transaction = _transaction;

                return myCommand;
            }
            catch
            {
                myCommand.Dispose();
                throw;
            }
        }
    }
}
