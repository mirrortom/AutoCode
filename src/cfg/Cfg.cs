using System;
using System.Collections.Generic;
using System.Text;

namespace AutoCode
{
    public enum DataBaseSrv
    {
        maria = 1,
        sqlserver = 2
    }
    public enum ApiVersion
    {
        netcore = 1,
        netframework = 2
    }
    /// <summary>
    /// 表单布局方式
    /// </summary>
    public enum FormLayoutStyle
    {
        /// <summary>
        /// 垂直列,标题和输入框都独占一行
        /// </summary>
        list=1,
        /// <summary>
        /// 网格,标题和输入框并列一行
        /// </summary>
        grid=2
    }
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
        public ApiVersion WebApiVersion = ApiVersion.netcore;
        /// <summary>
        /// 数据库类型 1=mariadb(默认) ,2=sqlserver 
        /// </summary>
        public DataBaseSrv DataBaseType = DataBaseSrv.maria;
        /// <summary>
        /// 输出目录 默认运行目录下的CreateCode文件夹
        /// </summary>
        public string OutPutDir = "CreateCode";
        /// <summary>
        /// add.html页面表单项布局 1=标签,输入框单独一行(默认) 2=标签,输入框同一行,使用栅格布局
        /// </summary>
        public FormLayoutStyle FormLayout = FormLayoutStyle.list;
    }
}
