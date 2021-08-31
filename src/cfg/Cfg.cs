using System;
using System.Collections.Generic;
using System.Text;

namespace AutoCode
{
    /// <summary>
    /// 配置
    /// </summary>
    public class Cfg
    {
        /// <summary>
        /// DB连接字符串
        /// </summary>
        public string ConnStr;
        /// <summary>
        /// 数据表名字,最好全部小写
        /// </summary>
        public string TableName;
        /// <summary>
        /// 程序项目命名空间名字
        /// </summary>
        public string NS;
        /// <summary>
        /// Web API版本 1=.netcore(默认) 2=.netframework
        /// </summary>
        public int WebApiVersion = 1;
        /// <summary>
        /// 数据库类型 1=mariadb(默认) ,2=sqlserver 
        /// </summary>
        public int DataBaseType = 1;
        /// <summary>
        /// 输出目录 默认运行目录下的CreateCode文件夹
        /// </summary>
        public string OutPutDir = "CreateCode";
        /// <summary>
        /// add.html页面表单项布局 1=标签,输入框单独一行(默认) 2=标签,输入框同一行,使用栅格布局
        /// </summary>
        public int FormLayout = 1;
    }
}
