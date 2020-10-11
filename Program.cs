using System;

namespace AutoCode
{
    class Program
    {
        static void Main(string[] args)
        {
            string connstr = @"server=(localdb)\.\mylocaldb;uid=sa;pwd=123456;Initial Catalog=mldco";
            Console.WriteLine($"开始生成程序,wait...");
            CreateCode.Run("loginlog", "mld", connstr, "apicore");
            Console.WriteLine("完成...");
            Console.ReadKey();
        }
    }
}
