using System;

namespace AutoCode
{
    class Program
    {
        static void Main(string[] args)
        {
            // sqlserver数据库
            string sqlserver = @"server=(localdb)\.\mylocaldb;uid=sa;pwd=123456;Initial Catalog=test";
            // mariadb数据库
            string mariadb = @"server=localhost;port=3306;database=mirror;user=root;password=123456;SslMode=none;";
            Cfg cfg = new Cfg()
            {
                DataBaseType = DataBaseSrv.sqlserver,
                ConnStr = sqlserver,
                TableName = "emp",
                NS = "testproj",
                WebApiVersion = ApiVersion.netcore,
                FormLayout = FormLayoutStyle.list
            };
            Console.WriteLine($"开始生成程序,wait...");
            CreateCode.Run(cfg);
            Console.WriteLine("完成...");
            Console.ReadKey();
        }

    }
}