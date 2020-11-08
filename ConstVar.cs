using System;
using System.Collections.Generic;
using System.Text;

namespace AutoCode
{
    /// <summary>
    ///自定义错误代码,用于API和页面
    /// </summary>
    public class ErrCode
    {
        /// <summary>
        /// 操作成功
        /// </summary>
        public const int Success = 200;
        /// <summary>
        /// 服务器异常
        /// </summary>
        public const int SrvExp = 500;
        /// <summary>
        /// 拒绝请求
        /// </summary>
        public const int SrvDeny = 510;
        /// <summary>
        /// 操作失败
        /// </summary>
        public const int Falid = 600;
        /// <summary>
        /// 没有数据
        /// </summary>
        public const int NoData = 601;
        /// <summary>
        /// 参数错误
        /// </summary>
        public const int ParaErr = 602;
    }
    public enum AlertMsg
    {
        没有数据,
        服务器错误,
        数据库错误,
        当前时段禁止该操作
    }
}
