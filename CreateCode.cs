﻿using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AutoCode
{

    /// <summary>
    /// 根据一个数据表,生成entity,dal,bll,webapi,page代码,数据表文档.输出到文件
    /// </summary>
    public class CreateCode
    {
        #region 模板文件路径,输出目录
        // 模板文件路径
        private const string entityTemp = "tplcshtml/Entity.cshtml";
        private const string dalTemp = "tplcshtml/Dal.cshtml";
        private const string bllTemp = "tplcshtml/Bll.cshtml";
        private const string apiTemp = "tplcshtml/Api.cshtml";
        private const string apiCoreTemp = "tplcshtml/ApiCore.cshtml";
        private const string tableDocTemp = "tplcshtml/TableDoc.cshtml";
        private const string listTemp = "tplcshtml/List.cshtml";
        private const string addTemp = "tplcshtml/Add.cshtml";
        private const string addGridLayoutTemp = "tplcshtml/Add_gridlayout.cshtml";
        private const string detailTemp = "tplcshtml/detail.cshtml";
        private const string toolTemp = "tplcshtml/tool.cshtml";
        // 输出根目录
        private static string outRootDir = "CreateCode";
        // 根目录下以表名建立一个目录
        private static string outFileDir;
        #endregion

        #region 表名,命名空间,实体类名,dal,bll,api层类名
        private static string tableName;
        /// <summary>
        /// 首字母大写的表名
        /// </summary>
        private static string TableName;
        private static string nameSpace;
        private static string entityTypeName;
        private static string dalTypeName;
        private static string bllTypeName;
        private static string apiTypeName;
        #endregion

        /// <summary>
        /// 查库后含有键: name,dbtype,maxlen,info,increment,ispk,benull (不要修改这些,要转换另外加键)
        /// </summary>
        private static Dictionary<string, string>[] columns;

        /// <summary>
        /// 生成CRUD代码.(只用于sqlserver)
        /// </summary>
        /// <param name="tabName">表名</param>
        /// <param name="nSpace">程序命名空间</param>
        /// <param name="connStr">DB连接串</param>
        /// <param name="apiVersion">可选值"apicore"(.net core版本,默认是framework版本)</param>
        /// <param name="outDir">输出目录</param>
        public static void Run(Cfg cfg)
        {
            CheckCfg(cfg);
            // 
            Init(cfg.TableName, cfg.NS, cfg.OutPutDir);

            // table info 取得数据表的列字段信息
            SQLServer db = new SQLServer(cfg.ConnStr);
            GetColumns(db);
            //
            FieldNameToCsName();
            FieldTitle();
            FieldValidMaxLen();

            // create codes 生成各代码文件
            CreateDal();
            CreateEntity();
            CreateBll();
            CreateApi(cfg.ApiVersion);
            CreateList();
            CreateAdd(cfg.FormLayout);
            CreateTabDoc();
            CreateDetail();
            CreateTool();

            // 打开目录 (这个方法在windows平台有效)
            Process.Start(new ProcessStartInfo()
            {
                FileName = outRootDir,
                UseShellExecute = true
            });
            // 这个方法在.net core3.1 无法工作
            //Process.Start("C:/WINDOWS/system32/explorer.exe", outPutDir);
        }

        /// <summary>
        /// 检查配置,出错丢异常
        /// </summary>
        /// <param name="cfg"></param>
        private static void CheckCfg(Cfg cfg)
        {
            if (cfg == null) throw new Exception("配置无效!");
            if (string.IsNullOrWhiteSpace(cfg.ConnStr))
                throw new Exception("未填写数据库连接字符!");
            if (string.IsNullOrWhiteSpace(cfg.TableName))
                throw new Exception("未填写表名!");
            if (string.IsNullOrWhiteSpace(cfg.NS))
                throw new Exception("未填写命名空间!");
            if (!(cfg.ApiVersion == 1 || cfg.ApiVersion == 2))
                throw new Exception("api版本选项无效");
            if (string.IsNullOrWhiteSpace(cfg.OutPutDir))
                throw new Exception("未填写输出目录!");
            if (!(cfg.FormLayout == 1 || cfg.FormLayout == 2))
                throw new Exception("add页面表单布局选项无效");
        }

        private static void Init(string tabName, string nSpace, string outDir)
        {
            // data init
            tableName = tabName;
            // 命名空间和类名首字母大写
            nameSpace = nSpace.Substring(0, 1).ToUpper() + nSpace.Substring(1);
            TableName = tableName.Substring(0, 1).ToUpper() + tableName.Substring(1);
            entityTypeName = TableName + 'M';
            dalTypeName = TableName + "Dal";
            bllTypeName = TableName + "Bll";
            apiTypeName = TableName + "Api";

            // outputdir 输出根目录
            if (!string.IsNullOrWhiteSpace(outDir))
                outRootDir = outDir;
            // 以表名建立一个子目录
            outFileDir = $"{outRootDir}/{tableName}";
            Directory.CreateDirectory(outFileDir);

        }
        /// <summary>
        /// list列表页
        /// </summary>
        private static void CreateList()
        {
            StringBuilder heads = new StringBuilder();
            StringBuilder cols = new StringBuilder();
            foreach (var item in columns)
            {
                heads.Append($"<th>{item["fieldTitle"]}</th>");
                cols.Append($"<td>${{item.{item["fieldName"]}}}</td>");
            }
            var viewdata = new
            {
                tableName = tableName,
                TableName,
                tableDataHeads = heads.ToString(),
                tableDataCols = cols.ToString(),
            };
            BuildAndOutPutTemp(listTemp, viewdata, $"{outFileDir}/{tableName}list.html");
        }

        /// <summary>
        /// add/edit编辑页
        /// </summary>
        private static void CreateAdd(int formLayout)
        {
            var viewdata = new
            {
                tableName = tableName,
                TableName,
                columns = columns
            };
            BuildAndOutPutTemp(formLayout == 1 ? addTemp : addGridLayoutTemp, viewdata, $"{outFileDir}/{tableName}add.html");
        }
        /// <summary>
        /// detail详情页
        /// </summary>
        private static void CreateDetail()
        {
            var viewdata = new
            {
                tableName = tableName,
                TableName,
                columns = columns
            };
            BuildAndOutPutTemp(detailTemp, viewdata, $"{outFileDir}/{tableName}detail.html");
        }
        /// <summary>
        /// 建立数据表文档. 一个HTML表格
        /// </summary>
        private static void CreateTabDoc()
        {
            var viewdata = new
            {
                tableName = tableName,
                columns = columns
            };
            BuildAndOutPutTemp(tableDocTemp, viewdata, $"{outFileDir}/{tableName}.doc.html");
        }
        private static void CreateApi(int apiVersion)
        {
            // json字段,按需返回. data.Select(o=>new{以下拼接字段内容,默认所有列名})
            StringBuilder sb = new StringBuilder();
            StringBuilder sb1 = new StringBuilder();
            foreach (var col in columns)
            {
                string fieldName = col["fieldName"];
                // list
                sb.AppendLine($"{new string(' ', 20)}o.{fieldName},");
                // item
                sb1.AppendLine($"{new string(' ', 20)}data.{fieldName},");
            }
            string fields_list = sb.ToString().TrimEnd('\r', '\n', ',');
            string fields_item = sb1.ToString().TrimEnd('\r', '\n', ',');

            var viewdata = new
            {
                tableName = tableName,
                nameSpace = nameSpace,
                className = apiTypeName,
                entityName = entityTypeName,
                dalName = dalTypeName,
                bllName = bllTypeName,
                fieldList = fields_list,
                fieldItem = fields_item
            };
            BuildAndOutPutTemp(apiVersion == 1 ? apiCoreTemp : apiTemp, viewdata, $"{outFileDir}/{apiTypeName}.cs");
        }

        private static void CreateBll()
        {
            var viewdata = new
            {
                tableName = tableName,
                nameSpace = nameSpace,
                className = bllTypeName,
                entityName = entityTypeName,
                dalName = dalTypeName
            };
            BuildAndOutPutTemp(bllTemp, viewdata, $"{outFileDir}/{bllTypeName}.cs");
        }

        private static void CreateEntity()
        {
            foreach (var item in columns)
            {
                string dbtype = item["dbtype"];
                // 属性/成员字段的注释
                string comment = item["info"];
                if (dbtype.Contains("char"))
                {
                    // 字符串类型的属性,注释加上长度,用于表单验证参考.如果是n开头的char.长度减半
                    comment = $"{comment} maxlen={item["validMaxLen"]}";
                }
                item.Add("comment", comment);
                // 数据库字段类型转C#类型
                item.Add("fieldType", FieldTypeToCsType(dbtype));
            }
            //
            var viewdata = new
            {
                tableName = tableName,
                nameSpace = nameSpace,
                className = entityTypeName,
                columns = columns
            };
            BuildAndOutPutTemp(entityTemp, viewdata, $"{outFileDir}/{entityTypeName}.cs");
        }


        // 生成 dal
        private static void CreateDal()
        {
            var viewdata = new
            {
                tableName = tableName,
                nameSpace = nameSpace,
                className = dalTypeName,
                entityName = entityTypeName,
                queryFields = string.Join(",", columns.Select(o => o["fieldName"]))
            };
            BuildAndOutPutTemp(dalTemp, viewdata, $"{outFileDir}/{dalTypeName}.cs");
        }

        // 其它工具代码
        private static void CreateTool()
        {
            var viewdata = new
            {
                tableName = tableName,
                nameSpace = nameSpace,
                className = entityTypeName,
                columns = columns
            };
            BuildAndOutPutTemp(toolTemp, viewdata, $"{outFileDir}/{tableName}Tools.cs");
        }
        /// <summary>
        /// 编译模板,输出到文件
        /// </summary>
        /// <param name="cshtmlPath"></param>
        /// <param name="cshtmlData"></param>
        /// <param name="outputPath"></param>
        private static void BuildAndOutPutTemp(string cshtmlPath, object cshtmlData, string outputPath)
        {
            // templete
            StreamReader sr = new StreamReader(cshtmlPath);
            string template = sr.ReadToEnd();
            sr.Dispose();

            // build
            string result = Engine.Razor.RunCompile(template, cshtmlPath, null, cshtmlData);

            // output
            StreamWriter sw = new StreamWriter(outputPath);
            sw.Write(result);
            sw.Dispose();
        }

        /// <summary>
        /// 查询数据库表,得到所有的列.是一个字典数组
        /// </summary>
        private static void GetColumns(SQLServer db)
        {
            string sql = $@"
SELECT
    CASE WHEN pk.COLUMN_NAME IS NULL THEN '' ELSE 'Y' END as ispk,
	CASE(c.is_identity) WHEN 1 THEN 'Y' ELSE '' END as increment,
	c.name,
	t.name dbtype,
	c.max_length maxlen,
	CASE(c.is_nullable) WHEN 0 THEN 'NOT NULL' ELSE 'NULL' END AS benull,
	p.value info
FROM sys.columns c
LEFT JOIN sys.systypes  t ON t.xusertype=c.system_type_id
LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE pk ON pk.COLUMN_NAME=c.name AND pk.TABLE_NAME='{tableName}'
LEFT JOIN sys.extended_properties p ON p.minor_id=c.column_id AND p.major_id=c.object_id
WHERE c.object_id = 
    (SELECT object_id FROM sys.tables WHERE type='U' AND name='{tableName}')";
            var data = db.ExecuteQuery(sql);
            // 查库后含有键: name,dbtype,maxlen,info (不要修改这些,要转换另外加键)
            columns = new Dictionary<string, string>[data.Length];
            for (int i = 0, len = data.Length; i < len; i++)
            {
                columns[i] = new Dictionary<string, string>();
                foreach (var key in data[i].Keys)
                {
                    columns[i].Add(key, data[i][key].ToString());
                }
            }
        }

        /// <summary>
        /// 数据表字段类型转换为C#类型
        /// db field type transfer c# type
        /// </summary>
        /// <param name="dbtype"></param>
        /// <returns></returns>
        private static string FieldTypeToCsType(string dbtype)
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
            if (dbtype.Contains("decimal") || dbtype.Contains("money") || dbtype.Contains("float"))
                return "decimal";
            return "string";
        }

        /// <summary>
        /// 数据表字段名字,转换为c#类名字.首字母大写
        /// </summary>
        private static void FieldNameToCsName()
        {
            foreach (var item in columns)
            {
                // 属性名首字母大写
                item.Add("fieldName", item["name"].Substring(0, 1).ToUpper() + item["name"].Substring(1));
            }
        }

        /// <summary>
        /// 数据表字段的标题.取注释第一个词(空格分开的),没有就是字段名字
        /// </summary>
        private static void FieldTitle()
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
        /// 数据表字段的长度,用于页面验证参考.如果是char类型的,并且是n开头的char类型,长度要减半.
        /// </summary>
        private static void FieldValidMaxLen()
        {
            foreach (var item in columns)
            {
                string dbtype = item["dbtype"];
                int maxlen = int.Parse(item["maxlen"]);
                item.Add("validMaxLen", item["maxlen"]);
                if (dbtype.Contains("char"))
                {
                    // 字符串类型的属性,注释加上长度,用于表单验证参考.如果是n开头的char.长度减半
                    if (dbtype.Substring(0, 1) == "n")
                    {
                        item["validMaxLen"] = (maxlen / 2).ToString();
                    }
                }
            }
        }
    }
}
