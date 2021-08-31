using System;

namespace AutoCode
{
    class Program
    {
        static void Main(string[] args)
        {
            // sqlserver数据库
            string sqlserver = @"server=(localdb)\.\mylocaldb;uid=sa;pwd=123456;Initial Catalog=mldco";
            // mariadb数据库
            string mariadb = @"server=localhost;port=3306;database=test;user=root;password=123456;SslMode = none;";
            Cfg cfg = new Cfg()
            {
                DataBaseType = 1,
                ConnStr = mariadb,
                TableName = "emp",
                NS = "testproj",
                WebApiVersion = 1,
                FormLayout = 2
            };
            Console.WriteLine($"开始生成程序,wait...");
            CreateCode.Run(cfg);
            Console.WriteLine("完成...");
            Console.ReadKey();
        }

    }
}