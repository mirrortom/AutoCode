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
            if (dbtype.Contains("datetime"))
                return nameof(DateTime);
            if (dbtype.Contains("decimal") || dbtype.Contains("money") || dbtype.Contains("float"))
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
    }
}
