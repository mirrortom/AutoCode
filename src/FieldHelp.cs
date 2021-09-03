using System;
using System.Collections.Generic;

namespace AutoCode
{
    /// <summary>
    /// 数据表列处理
    /// </summary>
    class FieldHelp
    {
        /// <summary>
        /// 数据表字段类型转换为C#类型
        /// db field type transfer c# type
        /// </summary>
        /// <param name="dbtype"></param>
        /// <returns></returns>
        public static string ToCsType(string dbtype)
        {
            if (dbtype.Contains("char"))
                return "string";
            if (dbtype.Contains("bigint"))
                return "long";
            if (dbtype.Contains("int") || dbtype.Contains("bit"))
                return "int";
            // 使用DateTimeOffset时间类型时,sqlserver对应时间类型要使用datetimeoffset(7),否则转实体类失败
            if (dbtype.Contains("datetimeoffset"))
                return nameof(DateTimeOffset);
            // DateTime类型可以对应数据库时间类型 mssql:datetime2(0~7) maria:datetime(0~6)
            if (dbtype.Contains("datetime"))
                return nameof(DateTime);
            if (dbtype.Contains("decimal") || dbtype.Contains("money") || dbtype.Contains("float") || dbtype.Contains("double"))
                return "decimal";
            return "string";
        }


        /// <summary>
        /// 数据表字段名字,转换为c#类名字.首字母大写
        /// </summary>
        public static void ToCsName(Dictionary<string, string>[] columns)
        {
            foreach (var item in columns)
            {
                // 属性名首字母大写
                item.Add("fieldName", item["name"].Substring(0, 1).ToUpper() + item["name"][1..]);
            }
        }

        /// <summary>
        /// 数据表字段的标题.取注释第一个词(空格分开的),没有就是字段名字
        /// </summary>
        public static void AddTitle(Dictionary<string, string>[] columns)
        {
            foreach (var item in columns)
            {
                string title = item["name"];
                if (!string.IsNullOrWhiteSpace(item["info"]))
                {
                    title = item["info"].Split(' ')[0];
                }
                item.Add("fieldTitle", title);
            }
        }

        /// <summary>
        /// 数据表字段的长度,用于页面验证参考.
        /// </summary>
        public static void AddValidMaxLen(Dictionary<string, string>[] columns)
        {
            foreach (var item in columns)
            {
                string dbtype = item["dbtype"];
                // maxlen的值,mariadb的数值类型时,查询为空值
                _ = int.TryParse(item["maxlen"], out int maxlen);
                item.Add("validMaxLen", maxlen.ToString());
                // 字符串类型的属性,注释加上长度值信息,用于表单验证参考
                if (dbtype.Contains("char"))
                {
                    // 对于sqlserver: 如果是n开头的char或者varchar.长度要减半.
                    // 在表设计器里设置长度为10,通过查询列信息得到的长度值是20.所以要除以2
                    if (dbtype.Substring(0, 1) == "n")
                    {
                        item["validMaxLen"] = (maxlen / 2).ToString();
                    }
                }
            }
        }

        /// <summary>
        /// 数据库字段类型和精度信息格式化,MSSQL/MARIA
        /// 例如 char 3, CHAR(3). datetime 6, DATETIME(6)
        /// decimal 10,2 ,DECIMAL(10,2)
        /// </summary>
        /// <param name="columns"></param>
        public static void DbTypeAndLengthFormat(Dictionary<string, string>[] columns)
        {
            foreach (var item in columns)
            {
                string dbtype = item["dbtype"];
                _ = int.TryParse(item["validMaxLen"], out int charlen);
                _ = int.TryParse(item["decimal_precision"], out int decimalM);
                _ = int.TryParse(item["decimal_scale"], out int decimalD);
                _ = int.TryParse(item["datetime_precision"], out int datetime_precision);
                item.Add("mssqlDbType", FieldHelp.MssqlTypeFormat(dbtype, charlen, decimalM, decimalD, datetime_precision));
                item.Add("mariaDbType", FieldHelp.MssqlToMariaType(dbtype, charlen, decimalM, decimalD, datetime_precision));
            }
        }

        /// <summary>
        /// mssql转maria数据库类型.只实现了有限类型个数转换
        /// </summary>
        /// <param name="dbtype"></param>
        /// <param name="charlen"></param>
        /// <returns></returns>
        private static string MssqlToMariaType(string dbtype, int charlen, int decimalM, int decimalD, int datetime_precision)
        {
            if (dbtype == "char" || dbtype == "nchar")
                return $"CHAR({charlen})";
            if (dbtype == "varchar" || dbtype == "nvarchar")
            {
                // 这种是varcahr(max)的情况
                if (charlen == -1)
                    return "TEXT";
                return $"VARCHAR({charlen})";
            }
            if (dbtype == "bigint" || dbtype == "int" || dbtype == "bit" || dbtype == "tinyint" || dbtype == "smallint")
                return dbtype.ToUpper();
            if (dbtype == "decimal")
                return $"DECIMAL({decimalM},{decimalD})";
            // datetimeoffset,datetime2,datetime都对应maria的datetime
            if (dbtype.Contains("datetime"))
            {
                if(datetime_precision>6)
                    return $"DATETIME(6)";
                return $"DATETIME({datetime_precision})";
            }
            throw new Exception($"mssql类型 [{dbtype}] 转换maria失败!");
        }

        /// <summary>
        /// mssql数据类型和长度信息格式化.例如: decimal 18 2 格式化为 decimal(18,2)
        /// </summary>
        /// <param name="dbtype"></param>
        /// <param name="charlen"></param>
        /// <param name="decimalM"></param>
        /// <param name="decimalD"></param>
        /// <param name="datetime_precision"></param>
        /// <returns></returns>
        private static string MssqlTypeFormat(string dbtype, int charlen, int decimalM, int decimalD, int datetime_precision)
        {
            if (dbtype == "char" || dbtype == "nchar")
                return $"{dbtype}({charlen})";
            if (dbtype == "varchar" || dbtype == "nvarchar")
            {
                // 这种是varcahr(max)的情况
                if (charlen == -1)
                    return $"{dbtype}(MAX))";
                return $"{dbtype}({charlen})";
            }
            if (dbtype == "bigint" || dbtype == "int" || dbtype == "bit" || dbtype == "tinyint" || dbtype == "smallint")
                return dbtype.ToUpper();
            if (dbtype == "decimal")
                return $"DECIMAL({decimalM},{decimalD})";
            if (dbtype == "datetime")
                return "DATETIME";
            if (dbtype == "datetime2")
                return $"DATETIME2({datetime_precision})";
            if (dbtype == "datetimeoffset")
                return $"datetimeoffset({datetime_precision})";
            throw new Exception($"mssql类型 [{dbtype}] 格式化失败!");
        }
    }
}
