using RazorEngine;
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
        private const string listTemp = "tplcshtml/List.cshtml";
        private const string addTemp = "tplcshtml/Add.cshtml";
        private const string addGridLayoutTemp = "tplcshtml/Add_gridlayout.cshtml";
        private const string detailTemp = "tplcshtml/detail.cshtml";
        private const string toolTemp = "tplcshtml/tool.cshtml";
        private const string tableDocTemp = "tplcshtml/TableDoc.cshtml";
        private const string tableCreateMariaTemp = "tplcshtml/TableCreateMaria.cshtml";
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
        /// 查库得到的表字段信息数据.
        /// </summary>
        private static Dictionary<string, string>[] columns;

        /// <summary>
        /// 生成CRUD代码.C# .net项目
        /// </summary>
        public static void Run(Cfg cfg)
        {
            CheckCfg(cfg);
            // 
            Init(cfg);

            // table info 取得数据表的列字段信息
            columns = QueryHelp.GetColumns(cfg);
            // 字段转换
            FieldHelp.ToCsName(columns);
            FieldHelp.AddTitle(columns);
            FieldHelp.AddValidMaxLen(columns);
            FieldHelp.DbTypeAndLengthFormat(columns);

            // create codes 生成代码文件
            CreateDal(cfg.DataBaseType);
            CreateEntity();
            CreateBll();
            CreateApi(cfg.WebApiVersion);
            CreateList();
            CreateAdd(cfg.FormLayout);
            CreateDetail();
            CreateTool();
            //CreateTabCreateMariaSql();
            CreateTabDoc();

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

            if (!(cfg.DataBaseType == DataBaseSrv.maria || cfg.DataBaseType == DataBaseSrv.sqlserver))
                throw new Exception("数据库类型选项无效");
            if (!(cfg.WebApiVersion == ApiVersion.netcore || cfg.WebApiVersion == ApiVersion.netframework))
                throw new Exception("api版本选项无效");
            if (string.IsNullOrWhiteSpace(cfg.OutPutDir))
                throw new Exception("未填写输出目录!");
            if (!(cfg.FormLayout == FormLayoutStyle.list || cfg.FormLayout == FormLayoutStyle.grid))
                throw new Exception("新增/编辑页面表单布局选项无效");
        }

        private static void Init(Cfg cfg)
        {
            // data init
            tableName = cfg.TableName;
            // 命名空间和类名首字母大写
            nameSpace = cfg.NS.Substring(0, 1).ToUpper() + cfg.NS[1..];
            TableName = tableName.Substring(0, 1).ToUpper() + tableName[1..];
            entityTypeName = TableName + 'M';
            dalTypeName = TableName + "Dal";
            bllTypeName = TableName + "Bll";
            apiTypeName = TableName + "Api";

            // outputdir 输出根目录
            if (!string.IsNullOrWhiteSpace(cfg.OutPutDir))
                outRootDir = cfg.OutPutDir;
            // 以表名建立一个子目录
            outFileDir = $"{outRootDir}/{tableName}";
            Directory.CreateDirectory(outFileDir);

        }

        //-- 模板页面生成 --//

        /// <summary>
        /// 生成maria的建表sql语句.(主要用于已有mssql转maria)
        /// </summary>
        private static void CreateTabCreateMariaSql()
        {
            var viewdata = new
            {
                tableName = tableName,
                columns = columns
            };
            BuildAndOutPutTemp(tableCreateMariaTemp, viewdata, $"{outFileDir}/{tableName}.create.maria.sql");
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
        private static void CreateAdd(FormLayoutStyle formLayout)
        {
            var viewdata = new
            {
                tableName = tableName,
                TableName,
                columns = columns
            };
            BuildAndOutPutTemp(formLayout == FormLayoutStyle.list ? addTemp : addGridLayoutTemp, viewdata, $"{outFileDir}/{tableName}add.html");
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

        private static void CreateApi(ApiVersion apiVersion)
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
            BuildAndOutPutTemp(apiVersion == ApiVersion.netcore ? apiCoreTemp : apiTemp, viewdata, $"{outFileDir}/{apiTypeName}.cs");
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
                item.Add("fieldType", FieldHelp.ToCsType(dbtype));
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
        private static void CreateDal(DataBaseSrv databaseType)
        {
            var viewdata = new
            {
                tableName = tableName,
                nameSpace = nameSpace,
                className = dalTypeName,
                entityName = entityTypeName,
                databaseSrv = databaseType,
                queryFields = string.Join(",", columns.Select(o => o["fieldName"]))
            };
            BuildAndOutPutTemp(dalTemp, viewdata, $"{outFileDir}/{dalTypeName}.cs");
        }

        // 其它工具类代码
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





    }
}
