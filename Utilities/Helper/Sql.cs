using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Utilities.Helper
{
    public static class Sql
    {
        private static string _connectionString;

        public static void Initialize(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static DataTable GetQueryResult(string query)
        {
            try
            {
                if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(_connectionString))
                {
                    return null;
                }
                else
                {
                    var connection = new SqlConnection(_connectionString);
                    var command = new SqlCommand(query, connection) { Connection = connection, CommandText = query, CommandTimeout = 600 };
                    if (connection.State == ConnectionState.Closed)
                    {
                        connection.Open();
                    }
                    var dataReader = command.ExecuteReader();
                    var dataTable = new DataTable();
                    dataTable.Load(dataReader);
                    connection.Close();
                    return dataTable;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static DataTable GetStoredProcedure(string procedureName, string[] parameters, object[] values)
        {
            try
            {
                var dataTable = new DataTable();
                if (parameters.Length.Equals(values.Length))
                {
                    var connection = new SqlConnection(_connectionString);
                    var command = new SqlCommand(procedureName, connection) { CommandType = CommandType.StoredProcedure, CommandTimeout = 600 };
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var value = values[i].GetType();
                        var type = SqlDbType.Int;
                        switch (value.Name)
                        {
                            case "Int32":
                                type = SqlDbType.Int;
                                break;
                            case "String":
                                type = SqlDbType.NVarChar;
                                break;
                            case "DateTime":
                                type = SqlDbType.DateTime;
                                break;
                            case "Double":
                                type = SqlDbType.Float;
                                break;
                        }
                        command.Parameters.Add("@" + parameters[i] + "", type).Value = values[i];
                    }
                    var dataAdapter = new SqlDataAdapter(command);
                    dataAdapter.Fill(dataTable);
                    connection.Close();
                    return dataTable;
                }
                return dataTable;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
