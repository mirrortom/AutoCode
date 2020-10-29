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
        /// API版本 1=.netcore 2=.net
        /// </summary>
        public int ApiVersion=1;
        /// <summary>
        /// 输出目录
        /// </summary>
        public string OutPutDir = "CreateCode";
        /// <summary>
        /// add.html页面表单项布局 1=标签表单单独一行 2=标签表单在一行,使用栅格布局
        /// </summary>
        public int FormLayout = 1;
    }
}
