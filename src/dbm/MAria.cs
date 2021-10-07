using MySql.Data.MySqlClient;
using System;
using System.Data.Common;

namespace AutoCode
{
    public class MAria : DBMO
    {
        public MAria(string connectionString = null)
        {
            this.connString = connectionString ?? @"server=localhost;port=3306;database=test;user=root;password=123456;SslMode = none";
        }
        protected override void ConnInstance()
        {
            this.conn = new MySqlConnection();
        }

        protected override void CmdInstance(string sql)
        {
            this.cmd = new MySqlCommand(sql);
        }

        protected override DbParameter ParaInstance(string name, object val)
        {
            MySqlParameter para = new()
            {
                ParameterName = name,
                Value = val ?? DBNull.Value
            };
            return para;
        }
        protected override DbParameter OutParaInstance(string name, int dbType)
        {
            MySqlParameter para = new()
            {
                Direction = System.Data.ParameterDirection.Output,
                ParameterName = name,
                MySqlDbType = (MySqlDbType)dbType
            };
            return para;
        }
    }
}
