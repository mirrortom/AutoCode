using System;

namespace AutoCode
{
    class Program
    {
        static void Main(string[] args)
        {
            Cfg cfg = new Cfg()
            {
                ConnStr = @"server=(localdb)\.\mylocaldb;uid=sa;pwd=123456;Initial Catalog=mldco",
                NS = "mld",
                TableName = "ordercstr",
                ApiVersion = 1,
                FormLayout = 2
            };
            Console.WriteLine($"开始生成程序,wait...");
            CreateCode.Run(cfg);
            Console.WriteLine("完成...");
            Console.ReadKey();
        }

    }
}