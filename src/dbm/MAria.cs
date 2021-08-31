using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AutoCode
{
    /// <summary>
    /// MariaDB用于MariaDB的使用.这是由mysql之父建立的一个替代作品 
    /// </summary>
    public class MAria
    {

        #region 属性
        /// <summary>
        /// 当前连接串
        /// </summary>
        private string ConnectionString;
        /// <summary>
        /// 当前连接
        /// </summary>
        private MySqlConnection conn;

        /// <summary>
        /// 当前命令
        /// </summary>
        private MySqlCommand cmd;

        /// <summary>
        /// 当前事务
        /// </summary>
        private MySqlTransaction tran;

        /// <summary>
        /// 表示异常信息
        /// </summary>
        private string message;
        /// <summary>
        /// 用于匹配SQL语句中的参数.该正则 不匹配 @@xxx @1 匹配@a_1
        /// </summary>
        private readonly string mathprarRege = @"@[a-zA-Z]+[a-zA-Z\d_]*";
        #endregion

        /// <summary>
        /// 构造
        /// </summary>
        public MAria()
        {

        }
        public MAria(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        #region 执行方法并返回结果
        /// <summary>
        /// 执行查询,返回字典数组形式的查询结果.无值或出错返回null
        /// 查询参数注意
        /// 1.参数占位标记的顺序和参数数组元素顺序保持一致
        /// 2.至少写两个参数,如果只有一个参数,那么再加一个参数,否则将会判定到泛型重载
        /// 3.或者传数组参数加强制object类型,例如 (object)string[]{3}
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="paravalues">参数值,0个或多个</param>
        /// <returns></returns>
        public Dictionary<string, object>[] ExecuteQuery(string sql, params object[] paravalues)
        {
            this.InItCmd(sql, paravalues);
            return this.Select();
        }
        /// <summary>
        /// 重载1: 执行查询,返回字典数组形式的查询结果.无值或出错返回null
        /// 参数是一个字典,其中的键于参数名字相同,不带@符号
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paradict"></param>
        /// <returns></returns>
        public Dictionary<string, object>[] ExecuteQuery(string sql, Dictionary<string, object> paradict)
        {
            this.InItCmd(sql, paradict);
            return this.Select();
        }

        /// <summary>
        /// 重载2: 执行查询返回对象数组.没有值返回null
        /// 查询参数注意
        /// 1.参数占位标记的顺序和参数数组元素顺序保持一致
        /// 2.至少写两个参数,如果只有一个参数,那么再加一个参数,否则将会判定到泛型重载
        /// 3.或者传数组参数加强制object类型,例如 (object)string[]{3}
        /// 返回类型T注意:
        /// 对象的属性或者成员名称和数据库查询语句字段的别名必须一样,大小写不限.
        /// </summary>
        /// <typeparam name="T">结果对象类型</typeparam>
        /// <param name="sql"></param>
        /// <param name="paravalues"></param>
        /// <returns></returns>
        public T[] ExecuteQuery<T>(string sql, params object[] paravalues)
        {
            this.InItCmd(sql, paravalues);
            return this.Select<T>();
        }
        /// <summary>
        /// 重载3: 执行查询返回对象数组.没有值返回null
        /// 参数是一个字典,其中的键于参数名字相同,不带@符号
        /// 返回类型T注意:
        /// 对象的属性或者成员名称和数据库查询语句字段的别名必须一样,大小写不限.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="paradict"></param>
        /// <returns></returns>
        public T[] ExecuteQuery<T>(string sql, Dictionary<string, object> paradict)
        {
            this.InItCmd(sql, paradict);
            return this.Select<T>();
        }
        /// <summary>
        /// 重载4: 执行查询,返回实体对象数组集合.无值和错误返回null
        /// 查询参数Q注意:
        /// 参数对象的属性或者成员名称必须和参数名字一样,大小写不限.
        /// 返回类型T注意:
        /// 对象的属性或者成员名称和数据库查询语句字段的别名必须一样,大小写不限.
        /// </summary>
        /// <typeparam name="T">返回结果集实体对象类型</typeparam>
        /// <typeparam name="Q">包含参数值的实体对象类型</typeparam>
        /// <param name="sql">查询语句</param>
        /// <param name="instance">含有查询参数值实体对象的实例</param>
        /// <returns></returns>
        public T[] ExecuteQuery<T, Q>(string sql, Q instance)
        {
            this.InItCmd<Q>(sql, instance);
            return this.Select<T>();
        }

        /// <summary>
        /// 执行一个标量查询,返回一个值,无值或发生异常都返回null
        /// 查询参数注意
        /// 1.参数占位标记的顺序和参数数组元素顺序保持一致
        /// 2.至少写两个参数,如果只有一个参数,那么再加一个参数,否则将会判定到泛型重载
        /// 3.或者传数组参数加强制object类型,例如 (object)string[]{3}
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <param name="paravalue">参数值数组(参数占位标记的顺序和参数数组元素顺序保持一致)</param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, params object[] paravalue)
        {
            this.InItCmd(sql, paravalue);
            return this.SelectScalar();
        }

        /// <summary>
        /// 重载1: 执行一个标量查询,返回一个值,无值或发生异常都返回null
        /// 参数是一个字典,其中的键于参数名字相同,不带@符号
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paradict"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, Dictionary<string, object> paradict)
        {
            this.InItCmd(sql, paradict);
            return this.SelectScalar();
        }

        /// <summary>
        /// 重载2: 执行一个标量查询,返回一个值,无值或发生异常都返回null
        /// 查询参数Q注意:
        /// 参数对象的属性或者成员名称必须和参数名字一样,大小写不限.
        /// </summary>
        /// <typeparam name="Q">参数实体对象类型</typeparam>
        /// <param name="sql">查询语句</param>
        /// <param name="instance">含有查询参数值实体对象的实例(该实例属性名与参数名须一致)</param>
        /// <returns></returns>
        public object ExecuteScalar<Q>(string sql, Q instance)
        {
            this.InItCmd<Q>(sql, instance);
            return this.SelectScalar();
        }

        /// <summary>
        /// 执行一个非查询,返回受影响的行数,发生异常返回-999
        /// 查询参数注意
        /// 1.参数占位标记的顺序和参数数组元素顺序保持一致
        /// 2.至少写两个参数,如果只有一个参数,那么再加一个参数,否则将会判定到泛型重载
        /// 3.或者传数组参数加强制object类型,例如 (object)string[]{3}
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paravalue"></param>
        /// <returns></returns>
        public int ExecuteNoQuery(string sql, params object[] paravalue)
        {
            this.InItCmd(sql, paravalue);
            return this.NoSelect();
        }

        /// <summary>
        /// 重载1: 执行一个非查询,返回受影响的行数,发生异常返回-999
        /// 参数是一个字典,其中的键于参数名字相同,不带@符号
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paradict"></param>
        /// <returns></returns>
        public int ExecuteNoQuery(string sql, Dictionary<string, object> paradict)
        {
            this.InItCmd(sql, paradict);
            return this.NoSelect();
        }

        /// <summary>
        /// 重载2: 执行一个非查询,返回受影响的行数,发生异常返回-999
        /// 查询参数Q注意:
        /// 参数对象的属性或者成员名称必须和参数名字一样,大小写不限.
        /// </summary>
        /// <typeparam name="Q">参数对象类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="instance">参数对象</param>
        /// <returns></returns>
        public int ExecuteNoQuery<Q>(string sql, Q instance)
        {
            this.InItCmd<Q>(sql, instance);
            return this.NoSelect();
        }
        #endregion

        #region 存储过程执行
        /********************/
        /******存储过程******/
        /*******************/

        /// <summary>
        /// 执行(无参)存储过程 返回受影响行数,如果异常则返回-999
        /// 参数1:存储过程名
        /// </summary>
        /// <param name="proc">存储过程名</param>
        /// <returns></returns>
        public int ExecuteProcedure(string proc)
        {
            this.InItCmdProc(proc, null, null);
            return this.Procedure(null);
        }

        /// <summary>
        /// 执行(1入参0出参)只含有一个输入参数无输出参数的存储过程.返回受影响行数,异常返回-999
        /// <para>0.过程名 1.参数名 2.参数值 </para>
        /// </summary>
        /// <param name="proc">储存过程名</param>
        /// <param name="paraname">参数名</param>
        /// <param name="paravalue">参数值</param>
        /// <returns></returns>
        public int ExecuteProcedure(string proc, string paraname, object paravalue)
        {
            this.InItCmdProc(proc, new Dictionary<string, object>() { { paraname, paravalue } }, null);
            return this.Procedure(null);
        }

        /// <summary>
        /// 执行(0入参1出参)没有输入参数只含有一个输出参数的存储过程.返回受影响行数,异常返回-999
        /// <para>0.过程名 1.输出参数名 2.输出参数引用 3.输出参数类型</para>
        /// </summary>
        /// <param name="proc">存储过程名</param>
        /// <param name="outparaname">输出参数名</param>
        /// <param name="outparatype">输出参数类型</param>
        /// <param name="outparavalue">传出的输出参数值</param>
        /// <returns></returns>
        public int ExecuteProcedure(string proc, string outparaname, MySqlDbType outparatype, out object outparavalue)
        {

            this.InItCmdProc(proc, null, new Dictionary<string, MySqlDbType>() { { outparaname, outparatype } });
            // 执行过程
            int reint = this.Procedure(null);
            // 将传出参数赋值(这里)
            outparavalue = this.cmd.Parameters[outparaname].Value;
            return reint;
        }

        /// <summary>
        /// 执行(1入参1出参)有一个输入参数和一个输出参数的存储过程.返回受影响行数,异常返回-999
        /// <para>0.过程名 1.输入参数名 2.输入参数值 3.输出参数名 4.输出参数引用 5.输出参数类型</para>
        /// </summary>
        /// <param name="proc">存储过程名</param>
        /// <param name="paraname">输入参数名</param>
        /// <param name="paravalue">输入参数值</param>
        /// <param name="outparaname">输出参数名</param>
        /// <param name="outparatype">输出参数类型</param>
        /// <param name="outparavalue">输出参数引用</param>
        /// <returns></returns>
        public int ExecuteProcedure(string proc, string paraname, object paravalue,
            string outparaname, MySqlDbType outparatype, out object outparavalue)
        {
            this.InItCmdProc(proc, new Dictionary<string, object>() { { paraname, paravalue } },
                new Dictionary<string, MySqlDbType>() { { outparaname, outparatype } });
            // 执行过程
            int reint = this.Procedure(null);
            // 输出参数赋值
            outparavalue = this.cmd.Parameters[outparaname].Value;
            return reint;
        }

        /// <summary>
        /// 执行(多入参0出参)有多个输入参数无输出参数的存储过程.返回受影响行数,异常返回-999
        /// <para>0.过程名 1.输入参数字典</para>
        /// <para></para>
        /// </summary>
        /// <param name="proc">存储过程名</param>
        /// <param name="paranames">输入参数字典</param>
        /// <returns></returns>
        public int ExecuteProcedure(string proc, Dictionary<string, object> paradict)
        {
            this.InItCmdProc(proc, paradict, null);
            // 执行过程
            return this.Procedure(null);
        }

        /// <summary>
        /// 执行(多入参1出参)有多个输入参数和一个输出参数的存储过程.返回受影响行数,异常返回-999
        /// <para>0.过程名 1.输入参数字典 2.输出参数名 3.输出参数类型 4.输出参数引用 </para>
        /// </summary>
        /// <param name="proc">存储过程名</param>
        /// <param name="paradict">输入参数字典</param>
        /// <param name="outparaname">输出参数名</param>
        /// <param name="outparatype">输出参数类型</param>
        /// <param name="outparavalue">输出参数引用</param>
        /// <returns></returns>
        public int ExecuteProcedure(string proc, Dictionary<string, object> paradict,
            string outparaname, MySqlDbType outparatype, out object outparavalue)
        {
            this.InItCmdProc(proc, paradict, new Dictionary<string, MySqlDbType>() { { outparaname, outparatype } });
            // 执行过程
            int reint = this.Procedure(null);
            // 输出参数赋值
            outparavalue = this.cmd.Parameters[outparaname].Value;
            return reint;
        }

        /// <summary>
        /// 执行(0入参多出参)
        /// <para>参数0.过程名 1.传出参数字典 2.传出参数值字典引用</para>
        /// </summary>
        /// <param name="proc">存储过程名</param>
        /// <param name="outparadict">传出参数字典</param>
        /// <param name="outparavaluedict">传出参数值字典引用</param>
        /// <returns></returns>
        public int ExecuteProcedure(string proc, Dictionary<string, MySqlDbType> outparadict,
            out Dictionary<string, object> outparavaluedict)
        {
            // 初始化命令
            this.InItCmdProc(proc, null, outparadict);
            outparavaluedict = outparadict.Keys.ToDictionary(k => k.ToString(), v => v as object);
            return this.Procedure(outparavaluedict);
        }

        /// <summary>
        /// 执行(1入参多出参)有多个输入参数和一个输出参数的存储过程.返回受影响行数,异常返回-999
        /// <para>0.过程名 1.输入参数名 2.输入参数值 3.输出参数字典 4.输出参数值字典引用</para>
        /// </summary>
        /// <param name="proc">存储过程名</param>
        /// <param name="paraname">输入参数名</param>
        /// <param name="paravalue">输入参数值</param>
        /// <param name="outparadict">输出参数字典</param>
        /// <param name="outparavaluedict">输出参数值字典引用</param>
        /// <returns></returns>
        public int ExecuteProcedure(string proc, string paraname, object paravalue,
            Dictionary<string, MySqlDbType> outparadict, out Dictionary<string, object> outparavaluedict)
        {
            // 初始化命令
            this.InItCmdProc(proc, new Dictionary<string, object>() { { paraname, paravalue } }, outparadict);
            outparavaluedict = outparadict.Keys.ToDictionary(k => k.ToString(), v => v as object);
            // 执行过程
            return this.Procedure(outparavaluedict);
        }


        /// <summary>
        /// 执行(多入参多出参)有多个输入参数和一个输出参数的存储过程.返回受影响行数,异常返回-999
        /// <para>0.过程名 1.输入参数字典 2.输出参数字典 3.输出参数字典引用 </para>
        /// </summary>
        /// <param name="proc">存储过程名</param>
        /// <param name="paradict">输入参数字典</param>
        /// <param name="outparadict">输出参数字典</param>
        /// <param name="outparavaluedict">输出参数字典引用</param>
        /// <returns></returns>
        public int ExecuteProcedure(string proc, Dictionary<string, object> paradict,
            Dictionary<string, MySqlDbType> outparadict, out Dictionary<string, object> outparavaluedict)
        {
            // 初始化命令
            this.InItCmdProc(proc, paradict, outparadict);
            outparavaluedict = outparadict.Keys.ToDictionary(k => k.ToString(), v => v as object);
            return this.Procedure(outparavaluedict);
        }
        #endregion

        #region 在事务中执行
        /********************************************************************************************************
        *                                                                                                      *
        * 开始事务方法和提交事务方法.如果需要在事务中进行,应先调用此方法,最后调用提交事务*****************************
        *                                                                                                      *
        * ******************************************************************************************************/
        /// <summary>
        /// 开始一个事务,成功返回真(该操作不会关闭数据库连接,请在后续使用"提交"或者"回滚")
        /// </summary>
        public bool BeginTransaction()
        {
            try
            {
                this.OpenDB(true);
                this.tran = this.conn.BeginTransaction();
                return true;
            }
            catch (Exception e)
            {
                this.message = e.Message;
                return false;
            }
            finally
            {
                // 记录事务日志
                // if (this.message != null)
                //LoggerHelp.AddDBLog(this.message);
            }
        }
        /// <summary>
        /// 回滚一个事务.(完成后数据连接会关闭)
        /// </summary>
        /// <returns></returns>
        public bool RollBackTransaction()
        {
            try
            {
                this.tran.Rollback();
                return true;
            }
            catch (Exception e)
            {
                this.message = e.Message;
                return false;
            }
            finally
            {
                this.tran.Dispose();
                this.tran = null;
                this.CloseDB();
            }
        }
        /// <summary>
        /// 提交事务,成功返回真,发生异常时回滚(完成后数据连接会关闭)
        /// </summary>
        public bool CommitTransaction()
        {
            try
            {
                this.tran.Commit();
                return true;
                //
            }
            catch (Exception e)
            {
                this.tran.Rollback();
                this.message = e.Message;
                return false;
            }
            finally
            {
                this.tran.Dispose();
                this.tran = null;
                this.CloseDB();
            }
        }
        #endregion

        #region 参数命令初始化
        /******************************************************************************************************************/
        /*初始化命令:(类私用)四个方法和一个辅助方法
         *任务: 新建SqlCommand对象1.设置为当前连接对象2.如果有参数则加入参数3.如果有事务则设定到事务中
         * 加入参数(1):会先查找sql语句中的@开头的参数名,然后再比较传入的参数数组是否个数一样,是则加入,否则不加.如果在调用时未让参数名和值
         * 个数相等,则会查询失败,异常日志将被记录.[注意:参数值数组元素顺序须按sql中参数名顺序]
         * 加入参数(2):根据传入对象的属性和sql语句中@开头的参数名(不包含@,剩下部分与对象属性名相同)来匹配出参数值.如果在调用时没能将
         * 参数名和属性值对应,则会查询失败,异常日志将被记录.[注意:对象属性有默认值,如果未正确赋值,可能会造成插入错误数据]
         ****************************************************************************************************************/
        /// <summary>
        /// 初始化命令,并且添加参数
        /// 参数1:查询语句
        /// 参数2:参数值数组
        /// </summary>
        /// <param name="paraname"></param>
        /// <param name="paravalue"></param>
        private void InItCmd(string sql, object[] paravalues)
        {
            this.cmd = new MySqlCommand(sql);
            // 如果无参数,则不动作
            if (paravalues.Length == 0) return;
            // 如果有参数,匹配出在SQL语句中的参数名,然后以找到的参数名个数加入相应个数的值.如果值少于参数个数,将发生异常(参数命名:字母开头可包含数字和下划线)
            MatchCollection paranames = Regex.Matches(sql, this.mathprarRege);

            for (int i = 0; i < paranames.Count; i++)
            {
                object paraitem = paravalues[i] == null ? DBNull.Value : paravalues[i];
                this.cmd.Parameters.Add(new MySqlParameter(paranames[i].Value, paraitem));
            }

        }
        /// <summary>
        /// 重载1: 初始化命令,并且添加参数
        /// 参数1:查询语句
        /// 参数2:参数值字典
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paradict"></param>
        private void InItCmd(string sql, Dictionary<string, object> paradict)
        {
            this.cmd = new MySqlCommand(sql);
            if (paradict == null) return;
            MatchCollection paranames = Regex.Matches(sql, this.mathprarRege);
            for (int i = 0; i < paranames.Count; i++)
            {
                string parakey = paranames[i].Value.Substring(1);
                // 如果参数字典里有对应参数名字的成员,则加入参数.
                if (paradict.ContainsKey(parakey))
                {
                    object paraitem = paradict[parakey] ?? DBNull.Value;
                    this.cmd.Parameters.Add(new MySqlParameter(paranames[i].Value, paraitem));
                }
            }
        }
        /// <summary>
        /// 泛型重载:
        /// <para>初始化命令:根据传入对象属性名和参数名匹配,自动化查找参数值</para>
        /// <para>注意:对象属性名和数据库字段名必须一样,不区分大小写.</para>
        /// <para>参数1:查询语句</para>
        /// 参数2:对象实例,该实例有与参数名匹配的属性,并且该属性已经赋值.对象不要传null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="instance"></param>
        private void InItCmd<Q>(string sql, Q instance)
        {
            this.cmd = new MySqlCommand(sql);

            // 如果传空对象,则没意义
            if (instance == null) return;
            // 匹配出参数名后得到参数集合,使用该集合匹配对象中的属性,找到则赋值否则忽略(参数命名:字母开头可包含数字和下划线)
            MatchCollection paranames = Regex.Matches(sql, this.mathprarRege);
            for (int i = 0; i < paranames.Count; i++)
            {
                string currpara = paranames[i].Value;
                GetPropValueAddParameters<Q>(instance, currpara);
            }
        }

        /// <summary>
        /// 初始化命令,为存储过程
        /// <para>参数0.过程名 1.输入参数字典 2.输出参数字典(如果没有任何参数,传入null)</para>
        /// </summary>
        /// <param name="proc">存储过程名</param>
        /// <param name="parasdict">输入参数字典,参数名string:参数值object</param>
        /// <param name="outparasdict">输出参数字典,输出参数名string:数据库类型SqlDbTyep</param>
        private void InItCmdProc(string proc, Dictionary<string, object> parasdict,
            Dictionary<string, MySqlDbType> outparasdict)
        {
            this.cmd = new MySqlCommand(proc);

            // 加入参数
            if (parasdict != null && parasdict.Keys.Count > 0)
            {
                foreach (string key in parasdict.Keys)
                {
                    this.cmd.Parameters.Add(new MySqlParameter(key, parasdict[key]));
                }
            }
            // 加入输出参数
            if (outparasdict != null && outparasdict.Keys.Count > 0)
            {
                // 不能使用foreach,否则报错集合成员已改变
                for (int i = 0; i < outparasdict.Keys.Count; i++)
                {
                    string key = outparasdict.Keys.ElementAt(i);
                    MySqlParameter outpara = new MySqlParameter(key, outparasdict[key]);
                    outpara.Direction = System.Data.ParameterDirection.Output;
                    this.cmd.Parameters.Add(outpara);
                }
            }
        }

        /// <summary>
        /// 初始化参数的辅助方法:
        /// <para>根据参数名匹配出实体对象里相应的属性值,然后加入到当前命令参数对象</para>
        /// 参数Q:实体对象类型 参数1:对象实例,该实例有与参数名匹配的属性.参数1:参数名
        /// </summary>
        /// <typeparam name="Q"></typeparam>
        /// <param name="instance"></param>
        /// <param name="currpara"></param>
        private void GetPropValueAddParameters<Q>(Q instance, string currpara)
        {
            // 找出该参数名对应的public成员
            System.Reflection.FieldInfo field = instance.GetType().GetField(currpara.Substring(1),
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.IgnoreCase);
            if (field != null)
            {
                object paravalue = field.GetValue(instance);
                if (paravalue == null)
                    paravalue = DBNull.Value;
                this.cmd.Parameters.Add(new MySqlParameter(currpara, paravalue));
                return;
            }

            // 如果没有,则再找出该参数名对应的public属性
            System.Reflection.PropertyInfo prop = instance.GetType().GetProperty(currpara.Substring(1),
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.IgnoreCase);

            // 如果找到了属性,则加入参数对象.没找到则不动作(没找到后执行Sql时就会发生参数未绑定异常)
            if (prop != null)
            {
                object paravalue = prop.GetValue(instance);
                if (paravalue == null)
                    paravalue = DBNull.Value;
                this.cmd.Parameters.Add(new MySqlParameter(currpara, paravalue));
            }
        }
        #endregion

        #region 执行增删改查
        /******************************************************************************************************
         * 
         * 实际执行的方法(1.结果集查询 2.非查询 3.标量查询 4.存储过程 ) 类私用*************************************
         * 
         * *****************************************************************************************************/

        /// <summary>
        /// 执行查询,返回字典数组查询结果(类内部实际干活方法)
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private Dictionary<string, object>[] Select()
        {
            try
            {
                this.OpenDB();
                using (MySqlDataReader dr = this.cmd.ExecuteReader())
                {
                    if (dr.HasRows)
                    {
                        List<Dictionary<string, object>> re = new List<Dictionary<string, object>>();
                        while (dr.Read())
                        {
                            Dictionary<string, object> tmp = new Dictionary<string, object>();
                            for (int i = 0; i < dr.FieldCount; i++)
                            {
                                tmp.Add(dr.GetName(i), dr[i]);
                            }
                            re.Add(tmp);
                        }
                        return re.ToArray();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                this.message = e.Message;
                return null;
            }
            finally
            {
                this.CloseDB();
            }
        }

        /// <summary>
        /// 执行查询,查询结果绑定到实体对象上.操作成功返回该对象数组(类内部实际干活方法)
        /// 参数T:集合中元素的类型
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private T[] Select<T>()
        {
            try
            {
                this.OpenDB();
                using (MySqlDataReader dr = this.cmd.ExecuteReader())
                {
                    if (dr.HasRows)
                    {
                        List<T> re = new List<T>();
                        while (dr.Read())
                        {
                            // 创建一个实例
                            T tmp = System.Activator.CreateInstance<T>();
                            // 循环当行数据行的所有字段
                            for (int i = 0; i < dr.FieldCount; i++)
                            {
                                // 如果没有找到实体上的属性,则找成员
                                System.Reflection.FieldInfo field = tmp.GetType().GetField(dr.GetName(i),
                                    System.Reflection.BindingFlags.Public |
                                    System.Reflection.BindingFlags.Instance |
                                    System.Reflection.BindingFlags.IgnoreCase);
                                if (field != null)
                                {
                                    field.SetValue(tmp, dr[i] == DBNull.Value
                                         ? null : Convert.ChangeType(dr[i], field.FieldType));
                                    continue;
                                }
                                // 使用反射找到出该字段名称相同的对象的属性
                                System.Reflection.PropertyInfo prop = tmp.GetType().GetProperty(dr.GetName(i),
                                    System.Reflection.BindingFlags.Public |
                                    System.Reflection.BindingFlags.Instance |
                                    System.Reflection.BindingFlags.IgnoreCase);
                                // 如果找到了属性,则设置值.没找到则不动作
                                // 这里的类型一般情况可以转为实体对象属性,前提是对象属性与数据库字段类型一致,否则异常
                                if (prop != null)
                                {
                                    prop.SetValue(tmp, dr[i] == DBNull.Value
                                         ? null : Convert.ChangeType(dr[i], prop.PropertyType));

                                }
                            }
                            re.Add(tmp);
                        }
                        return re.ToArray();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                this.message = e.Message;
                return null;
            }
            finally
            {
                this.CloseDB();
            }
        }

        /// <summary>
        /// 执行非查询,返回受影响行数(类内部实际干活方法)
        /// </summary>
        /// <returns></returns>
        private int NoSelect()
        {
            try
            {
                this.OpenDB();
                return this.cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                this.message = e.Message;
                return -999;
            }
            finally
            {
                this.CloseDB();
            }
        }

        /// <summary>
        /// 执行一个标量查询 如果异常或者未查询到值都返回 null;(内部干活方法)
        /// </summary>
        /// <returns></returns>
        private object SelectScalar()
        {
            try
            {
                this.OpenDB();
                object re = this.cmd.ExecuteScalar();
                if (Convert.IsDBNull(re))
                {
                    return null;
                }
                return re;
            }
            catch (Exception e)
            {
                this.message = e.Message;
                return null;
            }
            finally
            {
                this.CloseDB();
            }
        }

        /// <summary>
        /// 执行存储过程 异常返回-999;(如果返回值可能为-999,则请注意)(内部干活方法)
        /// <para>参数: 一个字典实例,其中键名就是输出参数名.方法执行之后,这些键将被赋值</para>
        /// 没有参数,传入null
        /// </summary>
        /// <returns></returns>
        private int Procedure(Dictionary<string, object> outparasdict)
        {
            try
            {
                this.OpenDB();
                // 存储过程加了这句
                this.cmd.CommandType = System.Data.CommandType.StoredProcedure;
                // 执行过程
                int reint = this.cmd.ExecuteNonQuery();
                // 返回之前需要给输出参数赋值
                if (outparasdict != null && outparasdict.Keys.Count > 0)
                {
                    foreach (string key in outparasdict.Keys)
                    {
                        outparasdict[key] = this.cmd.Parameters[key].Value;
                    }
                }
                return reint;
            }
            catch (Exception e)
            {
                this.message = e.Message;
                return -999;
            }
            finally
            {
                this.CloseDB();
            }
        }

        #endregion

        /// <summary>
        /// 打开一个连接
        /// 开启事物方法调用本方法时,要传入true
        /// </summary>
        private void OpenDB(bool isTranStart = false)
        {
            try
            {
                if (this.tran == null)
                {
                    // 建立
                    this.conn = new MySqlConnection(this.ConnectionString);
                    // 打开连接
                    this.conn.Open();
                    // 将参数应用于此连接
                    if (!isTranStart)
                        this.cmd.Connection = this.conn;
                }
                else
                {
                    // 将参数应用于此连接
                    this.cmd.Connection = this.conn;
                    // 事务用于此
                    this.cmd.Transaction = this.tran;
                }
            }
            catch (Exception e)
            {
                //LogHelp.DBLog(e.Message);
            }
        }
        /// <summary>
        /// 关闭数据库(类内用于自动关闭使用)
        /// 条件解释:与.连接存在,当前是状态打开,并且不是事务状态
        /// </summary>
        private void CloseDB()
        {
            if (this.message != null)
            {
                // 参数对信息
                StringBuilder paras = new StringBuilder();
                for (int i = 0; i < this.cmd.Parameters.Count; i++)
                {
                    paras.AppendFormat("{0}={1} | ", this.cmd.Parameters[i], this.cmd.Parameters[i].Value);
                }
                //LogHelp.DBLog(string.Format("错误信息:{0} \r\n SQL语句:[ {1} ]\r\n 参数值:[ {2} ]"
                //  , this.message, this.cmd.CommandText, paras.ToString()));
            }
            if (this.conn != null && this.tran == null)
            {
                this.conn.Close();
                this.conn.Dispose();
                this.conn = null;
                this.cmd = null;
            }
        }

        #region 补齐insert,update语句的 values部分,set部分
        /// <summary>
        /// 执行insert语句,返回受影响行数.传入的SQL语句不需要写VALUES部分.程序将自动补上.否则出错
        /// 注意:1.参数占位标记的顺序和参数数组元素顺序保持一致
        ///     2.至少写两个参数,如果只有一个参数,那么再加一个参数,否则将会判定到泛型重载
        /// </summary>
        /// <param name="sqlhalf"></param>
        /// <param name="paravalue"></param>
        /// <returns></returns>
        public int Insert(string sqlhalf, params object[] paravalue)
        {
            string sql = this.SqlAutoCmptInsert(sqlhalf);
            this.InItCmd(sql, paravalue);
            return this.NoSelect();
        }
        /// <summary>
        /// 执行update语句,返回受影响行数.SQL语句不需要写SET部分.使用(col1,col2)形式表示.程序将自动补齐成set col1=@col1,col2=@col2. 否则出错
        /// 注意:1.参数占位标记的顺序和参数数组元素顺序保持一致
        ///     2.至少写两个参数,如果只有一个参数,那么再加一个参数,否则将会判定到泛型重载
        /// </summary>
        /// <param name="sqlhalf"></param>
        /// <param name="paravalue"></param>
        /// <returns></returns>
        public int Update(string sqlhalf, params object[] paravalue)
        {
            string sql = this.SqlAutoCmptUpdate(sqlhalf);
            this.InItCmd(sql, paravalue);
            return this.NoSelect();
        }
        /// <summary>
        /// 重载 执行update语句,返回受影响行数.SQL语句不需要写SET部分.使用(col1,col2)形式表示.程序将自动补齐成set col1=@col1,col2=@col2. 否则出错
        /// </summary>
        /// <typeparam name="Q"></typeparam>
        /// <param name="sqlhalf"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public int Update<Q>(string sqlhalf, Q instance)
        {
            string sql = this.SqlAutoCmptUpdate(sqlhalf);
            this.InItCmd<Q>(sql, instance);
            return this.NoSelect();
        }
        /// <summary>
        /// 重载 执行insert语句,返回受影响行数.SQL语句不需要写VALUES部分.程序将自动补上.否则出错
        /// 参数来自对象
        /// </summary>
        /// <typeparam name="Q"></typeparam>
        /// <param name="sqlhalf"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public int Insert<Q>(string sqlhalf, Q instance)
        {
            string sql = this.SqlAutoCmptInsert(sqlhalf);
            this.InItCmd<Q>(sql, instance);
            return this.NoSelect();
        }

        /// <summary>
        /// 自动补全INSERT SQL语句
        /// 指定表名列名,省略VALUES(),方法将自动补全,然后返回完整INSERT SQL
        /// </summary>
        /// <param name="insertSql"></param>
        /// <returns></returns>
        private string SqlAutoCmptInsert(string insertSql)
        {
            // 找出SQL中的(COL1,COL2,...)片段,可用此正则(换行了也能) @"\(.*?\)|\((.*(\n|\r|\r\n))+\)"
            // 不用正则,1.找出第一个和第二个圆括号的位置2.取出中间的字段名,去掉空白后,逗号分组即可
            int sindex = insertSql.IndexOf('(');
            int eindex = insertSql.IndexOf(')');
            if (sindex == -1 || eindex == -1)
                throw new Exception($"请检查insertsql语句是否缺少左右括号:{insertSql}");
            string cols = insertSql.Substring(sindex + 1, eindex - sindex - 1);
            // 去掉空白
            cols = Regex.Replace(cols, @"\s", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            string[] colarr = cols.Split(new char[] { ',' });
            StringBuilder sqlpart = new StringBuilder(" VALUES(");
            foreach (var item in colarr)
            {
                sqlpart.AppendFormat("@{0},", item.Replace("[", "").Replace("]", ""));
            }
            sqlpart = sqlpart.Remove(sqlpart.Length - 1, 1);
            sqlpart.Append(")");
            return string.Format("{0}{1}", insertSql, sqlpart.ToString());
        }
        /// <summary>
        /// 自动补全UPDATE SQL语句
        /// 指定表名列名,省略SET,方法将自动补全,然后返回完整UPDATE SQL
        /// </summary>
        /// <param name="updateSql"></param>
        /// <returns></returns>
        private string SqlAutoCmptUpdate(string updateSql)
        {
            // 传入的SQL应该如此  UPDATE TABLE(COL1,COL2,COL3..) where
            // 1.找出第一个和第二个圆括号的位置2.取出中间的字段名,去掉空白后,逗号分组即可
            int sindex = updateSql.IndexOf('(');
            int eindex = updateSql.IndexOf(')');
            if (sindex == -1 || eindex == -1)
                throw new Exception($"请检查updatesql语句是否缺少左右括号:{updateSql}");
            string cols = updateSql.Substring(sindex + 1, eindex - sindex - 1);
            // 去掉空白
            cols = Regex.Replace(cols, @"\s", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            string[] colarr = cols.Split(new char[] { ',' });
            StringBuilder sqlpart = new StringBuilder();
            foreach (var item in colarr)
            {
                sqlpart.AppendFormat("{0}=@{1},", item, item.Replace("[", "").Replace("]", ""));
            }
            sqlpart = sqlpart.Remove(sqlpart.Length - 1, 1);
            string updatestart = updateSql.Substring(0, sindex).Trim();
            // 以括号为分界点,前面0-sindex部分是update table 中间是拼成的col=@col eindex-最后,是where部分
            return string.Format("{0} SET {1} {2}", updateSql.Substring(0, sindex).Trim(), sqlpart.ToString(), updateSql.Substring(eindex + 1).Trim());
        }
        #endregion

    }
}
