using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;

namespace myWebAPI
{
    public interface IBaseDao
    {
        Task ExecuteAsync(string sqlstatement, Dictionary<string, object> paras);
        Task<IEnumerable<T>> Query<T>(string sqlstatement, Dictionary<string, object> paras);
    }
    public class BaseDao : IBaseDao
    {
        private SqlConnection _sqlConnection;
        public SqlConnection SqlConnection { get; internal set; }
        public BaseDao(string connectionString)
        {
            _sqlConnection = new SqlConnection(connectionString);
            SqlConnection = _sqlConnection;
        }

        public Task<IEnumerable<T>> Query<T>(string sqlstatement, Dictionary<string, object> paras)
        {
            try
            {
                using (_sqlConnection)
                {
                    using (SqlCommand cmd = new SqlCommand(sqlstatement, _sqlConnection))
                    {
                        foreach (var para in paras)
                        {
                            var sqlPara = new SqlParameter
                            {
                                ParameterName = $"@{para.Key}",
                                Value = para.Value,
                                DbType = SqlMapper.Find(para.Value.GetType())
                            };
                            cmd.Parameters.Add(sqlPara);
                        }
                        if (_sqlConnection.State == ConnectionState.Closed)
                        {
                            _sqlConnection.OpenAsync();
                        }
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);
                            return (Task<IEnumerable<T>>)ConvertTo<T>(dt);
                        }
                    }
                }
            }
            catch
            { throw; }
        }

        public Task ExecuteAsync(string sqlstatement, Dictionary<string, object> paras)
        {
            try
            {
                using (_sqlConnection)
                {
                    using (SqlCommand cmd = new SqlCommand(sqlstatement, _sqlConnection))
                    {
                        foreach (var para in paras)
                        {
                            var sqlPara = new SqlParameter
                            {
                                ParameterName = $"@{para.Key}",
                                Value = para.Value,
                                DbType = SqlMapper.Find(para.Value.GetType())
                            };
                            cmd.Parameters.Add(sqlPara);
                        }
                        if (_sqlConnection.State == ConnectionState.Closed)
                        {
                            _sqlConnection.OpenAsync();
                        }
                        return cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private IList<T> ConvertTo<T>(DataTable table)
        {
            if (table == null)
            {
                return null;
            }

            var rows = new List<DataRow>();
            foreach (DataRow row in table.Rows)
            {
                rows.Add(row);
            }

            return ConvertTo<T>(rows);

        }

        private IList<T> ConvertTo<T>(IList<DataRow> rows)
        {
            IList<T> list = null;

            if (rows != null)
            {
                list = new List<T>();

                foreach (DataRow row in rows)
                {
                    T item = CreateItem<T>(row);
                    list.Add(item);
                }
            }

            return list;
        }

        private T CreateItem<T>(DataRow row)
        {
            T obj = default;
            if (row != null)
            {
                obj = Activator.CreateInstance<T>();

                foreach (DataColumn column in row.Table.Columns)
                {
                    PropertyInfo prop = obj.GetType().GetProperty(column.ColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    try
                    {
                        object value = row[column.ColumnName];
                        prop.SetValue(obj, value, null);
                    }
                    catch
                    {
                        //throw;    
                    }
                }
            }

            return obj;
        }

        static class SqlMapper
        {
            private static Dictionary<Type, DbType> typeMap;
            static SqlMapper()
            {
                typeMap = new Dictionary<Type, DbType>
                {
                    [typeof(byte)] = DbType.Byte,
                    [typeof(sbyte)] = DbType.SByte,
                    [typeof(short)] = DbType.Int16,
                    [typeof(ushort)] = DbType.UInt16,
                    [typeof(int)] = DbType.Int32,
                    [typeof(uint)] = DbType.UInt32,
                    [typeof(long)] = DbType.Int64,
                    [typeof(ulong)] = DbType.UInt64,
                    [typeof(float)] = DbType.Single,
                    [typeof(double)] = DbType.Double,
                    [typeof(decimal)] = DbType.Decimal,
                    [typeof(bool)] = DbType.Boolean,
                    [typeof(string)] = DbType.String,
                    [typeof(char)] = DbType.StringFixedLength,
                    [typeof(Guid)] = DbType.Guid,
                    [typeof(DateTime)] = DbType.DateTime,
                    [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
                    [typeof(TimeSpan)] = DbType.Time,
                    [typeof(byte[])] = DbType.Binary,
                    [typeof(byte?)] = DbType.Byte,
                    [typeof(sbyte?)] = DbType.SByte,
                    [typeof(short?)] = DbType.Int16,
                    [typeof(ushort?)] = DbType.UInt16,
                    [typeof(int?)] = DbType.Int32,
                    [typeof(uint?)] = DbType.UInt32,
                    [typeof(long?)] = DbType.Int64,
                    [typeof(ulong?)] = DbType.UInt64,
                    [typeof(float?)] = DbType.Single,
                    [typeof(double?)] = DbType.Double,
                    [typeof(decimal?)] = DbType.Decimal,
                    [typeof(bool?)] = DbType.Boolean,
                    [typeof(char?)] = DbType.StringFixedLength,
                    [typeof(Guid?)] = DbType.Guid,
                    [typeof(DateTime?)] = DbType.DateTime,
                    [typeof(DateTimeOffset?)] = DbType.DateTimeOffset,
                    [typeof(TimeSpan?)] = DbType.Time,
                    [typeof(object)] = DbType.Object
                };
            }
            static DbType ToDbType(Type type)
            {
                return Find(type);
            }
            internal static DbType Find(Type type)
            {
                object retObj = null;
                foreach (var item in typeMap)
                {
                    if (item.Key == type)
                    {
                        return item.Value;
                    }
                }
                if (retObj == null)
                {
                    throw new ApplicationException("Referenced an unsupported Type");
                }
                return (DbType)retObj;
            }
        }

    }


}
