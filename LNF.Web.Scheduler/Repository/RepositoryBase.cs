using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace LNF.Web.Scheduler.Repository
{
    public abstract class RepositoryBase
    {
        protected DbConnection CreateConnection()
        {
            var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
            var conn = factory.CreateConnection();
            conn.ConnectionString = ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString;
            return conn;
        }

        protected DbCommand CreateCommand(DbConnection conn, string sql, CommandType commandType = CommandType.StoredProcedure)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandType = commandType;
            cmd.CommandText = sql;
            return cmd;
        }

        protected DbCommand CreateCommand(DbConnection conn, string sql, DbTransaction tx, CommandType commandType = CommandType.StoredProcedure)
        {
            var cmd = CreateCommand(conn, sql, commandType);
            cmd.Transaction = tx;
            return cmd;
        }

        protected DbParameter AddParameter(DbCommand cmd, string name, object value, DbType type)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            p.DbType = type;
            cmd.Parameters.Add(p);
            return p;
        }

        protected DbParameter AddParameter(DbCommand cmd, string name, DbType type, ParameterDirection dir)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.DbType = type;
            p.Direction = dir;
            cmd.Parameters.Add(p);
            return p;
        }

        protected DataTable FillDataTable(DbCommand cmd)
        {
            using (var adap = DbProviderFactories.GetFactory("System.Data.SqlClient").CreateDataAdapter())
            {
                adap.SelectCommand = cmd;
                var dt = new DataTable();
                adap.Fill(dt);
                return dt;
            }
        }

        protected IEnumerable<T> AutoMap<T>(DataTable dt) where T : class, new()
        {
            var result = new List<T>();

            foreach (DataRow dr in dt.Rows)
            {
                var item = new T();

                foreach (var prop in typeof(T).GetProperties().Where(x => x.CanWrite))
                {
                    var col = prop.Name;
                    if (dt.Columns.Contains(col))
                    {
                        if (dt.Columns[col].DataType == prop.PropertyType)
                        {
                            prop.SetValue(item, dr[col]);
                        }
                    }
                }

                result.Add(item);
            }

            return result;
        }
    }
}
