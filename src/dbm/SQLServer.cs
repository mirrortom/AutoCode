using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace AutoCode
{
    public class SQLServer : DBMO
    {
        public SQLServer(string connectionString = null)
        {
            this.connString = connectionString ?? @"server=(localdb)\.\mylocaldb;uid=sa;pwd=123456;Initial Catalog=test";
        }
        protected override void ConnInstance()
        {
            this.conn = new SqlConnection();
        }

        protected override void CmdInstance(string sql)
        {
            this.cmd = new SqlCommand(sql);
        }

        protected override DbParameter ParaInstance(string name, object val)
        {
            SqlParameter para = new()
            {
                ParameterName = name,
                Value = val ?? DBNull.Value
            };
            return para;
        }

        protected override DbParameter OutParaInstance(string name, int dbType)
        {
            SqlParameter para = new()
            {
                Direction = System.Data.ParameterDirection.Output,
                ParameterName = name,
                SqlDbType = (System.Data.SqlDbType)dbType
            };
            return para;
        }
    }
}
