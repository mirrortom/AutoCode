using System.Collections.Generic;

namespace AutoCode
{
    /// <summary>
    /// 查询数据库获取数据表信息
    /// </summary>
    class QueryHelp
    {
        /// <summary>
        /// 查询数据库表,得到所有的列.是一个字典数组.
        /// 含有键:name(字段名字),dbtype(类型),maxlen(长度,字符串类型适用),info(说明),
        /// decimal_precision(decimal(M,D)的M值),decimal_scale(decimal的D值)
        /// datetime_precision(datetime2(n)的n值,mssql中,datetime的n默认是3)
        /// ispk(是否主键[Y/'']),increment(是否自增长[Y/'']),benull(是否可空[NOT NULL/NULL])
        /// </summary>
        public static Dictionary<string, string>[] GetColumns(Cfg cfg)
        {
            Dictionary<string, object>[] data;
            if (cfg.DataBaseType == 1)
            {
                string sql = MariaSql(cfg.TableName);
                MAria db = new(cfg.ConnStr);
                data = db.ExecuteQuery(sql);
            }
            else
            {
                string sql = MSSql(cfg.TableName);
                SQLServer db = new(cfg.ConnStr);
                data = db.ExecuteQuery(sql);
            }

            // 查库后含有键: name,dbtype,maxlen,info (不要修改这些,要转换另外加键)
            var columns = new Dictionary<string, string>[data.Length];
            for (int i = 0, len = data.Length; i < len; i++)
            {
                columns[i] = new Dictionary<string, string>();
                foreach (var key in data[i].Keys)
                {
                    columns[i].Add(key, data[i][key].ToString());
                }
            }
            return columns;
        }

        /// <summary>
        /// sqlserver,表信息查询语句
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private static string MSSql(string tableName)
        {
            string sql = $@"
SELECT
    CASE WHEN pk.COLUMN_NAME IS NULL THEN '' ELSE 'Y' END AS ispk,
	CASE(c.is_identity) WHEN 1 THEN 'Y' ELSE '' END AS increment,
	c.name,
	t.name AS dbtype,
	c.max_length AS maxlen,
    c.precision AS decimal_precision,
    c.scale AS decimal_scale,
    c.scale AS datetime_precision,
	CASE(c.is_nullable) WHEN 0 THEN 'NOT NULL' ELSE 'NULL' END AS benull,
	p.value AS info
FROM sys.columns c
LEFT JOIN sys.systypes  t ON t.xusertype=c.system_type_id
LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE pk ON pk.COLUMN_NAME=c.name AND pk.TABLE_NAME='{tableName}'
LEFT JOIN sys.extended_properties p ON p.minor_id=c.column_id AND p.major_id=c.object_id
WHERE c.object_id = 
    (SELECT object_id FROM sys.tables WHERE type='U' AND name='{tableName}')";
            return sql;
        }

        /// <summary>
        /// mariadb表信息查询语句
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private static string MariaSql(string tableName)
        {
            string sql = $@"
SELECT
    CASE column_key WHEN 'PRI' THEN 'Y' ELSE '' END AS ispk,
    CASE extra WHEN 'auto_increment' THEN 'Y' ELSE '' END AS increment,
    COLUMN_NAME AS name,
    data_type AS dbtype,
    character_maximum_length AS maxlen,
    NUMERIC_precision AS decimal_precision,
    numeric_scale AS decimal_scale,
    datetime_precision AS datetime_precision,
    CASE is_nullable WHEN 'NO' THEN 'NOT NULL' ELSE 'NULL' END AS benull,
    column_comment AS info
FROM information_schema.`COLUMNS`
WHERE TABLE_NAME='{tableName}'";
            return sql;
        }
    }
}
