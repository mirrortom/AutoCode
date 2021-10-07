using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace AutoCode
{
    /// <summary>
    /// 数据库管理CRUD操作. Database Management Operator
    /// </summary>
    abstract public class DBMO
    {
        #region 成员

        /// <summary>
        /// sql参数名字前缀符号.默认@,oracle要用:
        /// </summary>
        protected char paraPrefixChar = '@';

        /// <summary>
        /// sql参数匹配正则
        /// 默认值适用于sqlserver,maria,sqlite
        /// 命名规则:一个@开头,1字母,后面字母数字.
        /// 该正则 不匹配 @@xxx @1 匹配@a_1.
        /// </summary>
        protected string paraRege = @"(?<!@)@[a-zA-Z]+[a-zA-Z\d_]*";

        /// <summary>
        /// 当前连接串
        /// </summary>
        protected string connString;

        /// <summary>
        /// 当前连接
        /// </summary>
        protected DbConnection conn;

        /// <summary>
        /// 当前命令
        /// </summary>
        protected DbCommand cmd;

        /// <summary>
        /// 当前事务
        /// </summary>
        protected DbTransaction tran;

        /// <summary>
        /// 表示异常信息
        /// </summary>
        private string message;

        #endregion

        #region 建立与关闭链接

        /// <summary>
        /// 打开一个连接
        /// 开启事物方法调用本方法时,要传入true
        /// </summary>
        protected void OpenDB(bool isTranStart = false)
        {
            try
            {
                if (this.tran == null)
                {
                    // 建立链接对象,打开连接
                    this.ConnInstance();
                    this.conn.ConnectionString = this.connString;
                    this.conn.Open();
                    // 将命令适用于此连接
                    if (!isTranStart)
                        this.cmd.Connection = this.conn;
                }
                else
                {
                    // 将命令适用于此连接
                    this.cmd.Connection = this.conn;
                    // 事务用于此
                    this.cmd.Transaction = this.tran;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        /// <summary>
        /// 关闭数据库(类内用于自动关闭使用)
        /// 条件解释:连接存在,当前是状态打开,并且不是事务状态(事物状态时,在提交和回滚时关闭)
        /// </summary>
        private void CloseDB()
        {
            // 如果执行有错误,记录日志
            if (this.message != null)
            {
                // 参数对信息
                StringBuilder paras = new();
                for (int i = 0; i < this.cmd.Parameters.Count; i++)
                {
                    paras.Append($"{this.cmd.Parameters[i]}={this.cmd.Parameters[i].Value} | ");
                }
                throw new Exception($"错误信息:{this.message} \r\n SQL语句:[ {this.cmd.CommandText} ]\r\n 参数值:[ {paras} ]");
            }
            if (this.conn != null && this.tran == null)
            {
                this.conn.Close();
                this.conn.Dispose();
                this.conn = null;
                this.cmd = null;
                this.message = null;
            }
        }
        #endregion

        #region 执行查询
        /// <summary>
        /// 执行查询[数组式参数] [字典数组式结果集],字段名是键.无值或出错返回null
        /// <para>参数占位标记的顺序和参数数组元素顺序保持一致</para>
        /// <para>至少写两个参数(可加一个无用参数)否则将会判定到泛型重载.</para>
        /// <para>如果参数就是数组,强制object类型,例如 (object)string[]{3}</para>
        /// </summary>
        public Dictionary<string, object>[] ExecuteQuery(string sql, params object[] paras)
        {
            this.InItCmd(sql, paras);
            return this.Select();
        }
        /// <summary>
        /// 执行查询[字典式参数] [字典数组式结果集],字段名是键.无值或出错返回null
        /// <para>字典键与参数名字相同(注意大小写也相同),不带@或者:前缀符号</para>
        /// </summary>
        public Dictionary<string, object>[] ExecuteQuery(string sql, Dictionary<string, object> parasdict)
        {
            this.InItCmd(sql, parasdict);
            return this.Select();
        }

        /// <summary>
        /// 执行查询[数组式参数] [实体对象数组式结果集],没有值返回null
        /// <para>T类型是实体对象,成员或属性名与查询语句字段的别名一样,大小写不限.C#默认构造函数和字段值</para>
        /// <para>如果对应字段的数据值是DBNULL,那么T的该字段/属性将设置C#系统默认值.</para>
        /// </summary>
        public T[] ExecuteQuery<T>(string sql, params object[] paras) where T : new()
        {
            this.InItCmd(sql, paras);
            return this.Select<T>();
        }
        /// <summary>
        /// 执行查询[字典式参数] [实体对象数组式结果集],没有值返回null
        /// </summary>
        public T[] ExecuteQuery<T>(string sql, Dictionary<string, object> parasdict) where T : new()
        {
            this.InItCmd(sql, parasdict);
            return this.Select<T>();
        }
        /// <summary>
        /// 执行查询[对象式参数] [实体对象数组式结果集],没有值返回null
        /// <para>sql参数值对象Q,参数对象的属性或者成员名称必须和参数名字一样,大小写不限.</para>
        /// </summary>
        public T[] ExecuteQuery<T, Q>(string sql, Q paraentity) where T : new()
        {
            this.InItCmd<Q>(sql, paraentity);
            return this.Select<T>();
        }

        /// <summary>
        /// 执行查询[数组式参数] [单一结果值],无值或发生异常都返回null
        /// </summary>
        public object ExecuteScalar(string sql, params object[] paras)
        {
            this.InItCmd(sql, paras);
            return this.SelectScalar();
        }

        /// <summary>
        /// 执行查询[字典式参数] [单一结果值],无值或发生异常都返回null
        /// </summary>
        public object ExecuteScalar(string sql, Dictionary<string, object> parasdict)
        {
            this.InItCmd(sql, parasdict);
            return this.SelectScalar();
        }

        /// <summary>
        /// 执行查询[对象式参数] [单一结果值],无值或发生异常都返回null
        /// </summary>
        public object ExecuteScalar<Q>(string sql, Q paraentity)
        {
            this.InItCmd<Q>(sql, paraentity);
            return this.SelectScalar();
        }

        /// <summary>
        /// 执行非查询[数组式参数],返回受影响的行数,发生异常返回-999
        /// </summary>
        public int ExecuteNoQuery(string sql, params object[] paras)
        {
            this.InItCmd(sql, paras);
            return this.SelectNon();
        }

        /// <summary>
        /// 执行非查询[字典式参数],返回受影响的行数,发生异常返回-999
        /// </summary>
        public int ExecuteNoQuery(string sql, Dictionary<string, object> parasdict)
        {
            this.InItCmd(sql, parasdict);
            return this.SelectNon();
        }

        /// <summary>
        /// 执行非查询[对象式参数],返回受影响的行数,发生异常返回-999
        /// </summary>
        public int ExecuteNoQuery<Q>(string sql, Q paraentity)
        {
            this.InItCmd<Q>(sql, paraentity);
            return this.SelectNon();
        }

        /// <summary>
        /// 执行INSERT[数组式参数],返回受影响行数,发生异常返回-999
        /// <para>INSERT语句不需要写VALUES部分.程序将自动补上,否则出错.</para>
        /// <para>例: insert into tab(col1,col2,..) </para>
        /// </summary>
        public int Insert(string sqlhalf, params object[] paras)
        {
            string sql = DBMO.AutoCmptInsertSql(sqlhalf, this.paraPrefixChar);
            this.InItCmd(sql, paras);
            return this.SelectNon();
        }

        /// <summary>
        /// 执行INSERT[字典式参数],返回受影响行数,发生异常返回-999
        /// </summary>
        public int Insert(string insertHalf, Dictionary<string, object> parasdict)
        {
            string sql = DBMO.AutoCmptInsertSql(insertHalf, this.paraPrefixChar);
            this.InItCmd(sql, parasdict);
            return this.SelectNon();
        }

        /// <summary>
        /// 执行INSERT[对象式参数],返回受影响行数,发生异常返回-999
        /// </summary>
        public int Insert<Q>(string insertHalf, Q paraentity)
        {
            string sql = DBMO.AutoCmptInsertSql(insertHalf, this.paraPrefixChar);
            this.InItCmd<Q>(sql, paraentity);
            return this.SelectNon();
        }

        /// <summary>
        /// 执行UPDATE[数组式参数],返回受影响行数,发生异常返回-999
        /// <para>UPDATE语句不需要写SET部分.写出要赋值的字段.程序将自动补齐SET部分否则出错</para>
        /// <para>例: update tab (col1,col2,...) where id=1</para>
        /// </summary>
        public int Update(string sqlhalf, params object[] paras)
        {
            string sql = DBMO.AutoCmptUpdateSql(sqlhalf, this.paraPrefixChar);
            this.InItCmd(sql, paras);
            return this.SelectNon();
        }
        /// <summary>
        /// 执行UPDATE[字典式参数],返回受影响行数,发生异常返回-999
        /// </summary>
        public int Update(string sqlhalf, Dictionary<string, object> parasdict)
        {
            string sql = DBMO.AutoCmptUpdateSql(sqlhalf, this.paraPrefixChar);
            this.InItCmd(sql, parasdict);
            return this.SelectNon();
        }
        /// <summary>
        /// 执行UPDATE[对象式参数],返回受影响行数,发生异常返回-999
        /// </summary>
        /// <returns></returns>
        public int Update<Q>(string sqlhalf, Q paraentity)
        {
            string sql = DBMO.AutoCmptUpdateSql(sqlhalf, this.paraPrefixChar);
            this.InItCmd<Q>(sql, paraentity);
            return this.SelectNon();
        }
        #endregion

        #region 执行存储过程

        /// <summary>
        /// 执行存储过程.返回受影响行数,异常返回-999
        /// <para>0.过程名 1.输入参数字典 2.输出参数字典 3.输出参数结果值字典引用 </para>
        /// <para>由于各数据库存储过程参数命名方式不同,注意参数名字与字典键名对应关系.</para>
        /// <para>出参字典的int值是数据字段类型,枚举形式,为了方法统一适用,改为int.传值例子:</para>
        /// <para> (int)MySql.Data.MySqlClient.MySqlDbType.Int32 </para>
        /// </summary>
        public int ExecuteProcedure(string proc, Dictionary<string, object> paradict,
            Dictionary<string, int> outparadict, out Dictionary<string, object> outparavaluedict)
        {
            // 初始化命令
            this.InItCmdProc(proc, paradict, outparadict);
            outparavaluedict = new();
            return this.Procedure(outparavaluedict);
        }

        #endregion

        #region 执行事务
        // 开始事务方法和提交事务方法.如果需要在事务中进行,应先调用此方法,最后调用提交事务

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
                if (this.message != null)
                    throw new Exception(this.message);
            }
        }
        /// <summary>
        /// 回滚一个事务.在执行不达预期时可调用此方法撤回执行.(完成后数据连接会关闭)
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
        /// 提交事务,成功返回true,发生异常时回滚(完成后数据连接会关闭)
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

        #region sql命令和参数初始化 [内部方法]

        // 新建SqlCommand对象1.设置为当前连接对象2.如果有参数则加入参数3.如果有事务则设定到事务中
        // 1.数组参数:sql语句中的参数占位变量与参数值数组按位置一一对应,例如参数 @a,@b,参数数组值
        //   ["vala","valb"],那么@a对应vala,@b对应valb.
        // 2.对象参数:sql语句中的参数名字(去掉@或者:参数名字前缀),与参数值对象字段或者属性名对应
        //   例如参数 @a,@b,参数对象值 {a=1,b=2},那么@a对应1,@b对应2
        // 3.字典参数:与对象参数类似,sql语句中的参数名字与字典参数值的键名对应
        //   例如参数@a,@b,字典参数值{a:"a",b:"b"},那么@a对应"a",@b对应"b"

        /// <summary>
        /// 初始化命令,添加参数.[数组式参数]
        /// <para>参数1:查询语句</para>
        /// <para>参数2:参数值数组.长度不能少于参数个数</para>
        /// </summary>
        private void InItCmd(string sql, object[] paras)
        {
            this.CmdInstance(sql);

            // 匹配出在SQL语句中的参数名,然后以找到的参数名个数加入相应个数的值.
            MatchCollection paraNames = Regex.Matches(sql, this.paraRege);
            if (paraNames.Count == 0) return;
            // 参数不够异常掉
            if (paraNames.Count > paras.Length)
            {
                throw new Exception($"数组参数少于参数占位符.[sql语句: {sql}]");
            }
            for (int i = 0; i < paraNames.Count; i++)
            {
                DbParameter para = this.ParaInstance(paraNames[i].Value, paras[i]);
                this.cmd.Parameters.Add(para);
            }
        }
        /// <summary>
        /// 初始化命令,添加参数.[字典式参数]
        /// <para>参数1:查询语句</para>
        /// <para>参数2:参数值字典</para>
        /// </summary>
        private void InItCmd(string sql, Dictionary<string, object> parasdict)
        {
            this.CmdInstance(sql);

            MatchCollection paraNames = Regex.Matches(sql, this.paraRege);
            if (paraNames.Count == 0) return;
            if (parasdict == null || paraNames.Count > parasdict.Count)
            {
                throw new Exception($"字典参数值少于参数占位符.[sql语句: {sql}]");
            }
            for (int i = 0; i < paraNames.Count; i++)
            {
                string parakey = paraNames[i].Value[1..];
                // 如果参数字典里有对应参数名字的成员,则加入参数.
                if (parasdict.ContainsKey(parakey))
                {
                    DbParameter para = this.ParaInstance(paraNames[i].Value, parasdict[parakey]);
                    this.cmd.Parameters.Add(para);
                    continue;
                }
                throw new Exception($"参数占位符 [{paraNames[i]}] 未提供值.[sql语句: {sql}]");
            }
        }
        /// <summary>
        /// 初始化命令,添加参数.[对象式参数]
        /// <para>初始化命令:根据传入对象字段/属性名和参数名匹配,自动化查找参数值</para>
        /// <para>注意:对象字段/属性名和数据库字段名必须一样,不区分大小写.</para>
        /// <para>参数1:查询语句</para>
        /// <para>参数2:对象实例,该实例有与参数名匹配的字段/属性,并且已赋值.</para>
        /// </summary>
        private void InItCmd<Q>(string sql, Q entity)
        {
            this.CmdInstance(sql);

            // 匹配出参数名后得到参数集合,使用该集合匹配对象中的属性,找到则赋值否则忽略(参数命名:字母开头可包含数字和下划线)
            MatchCollection paraNames = Regex.Matches(sql, this.paraRege);
            if (paraNames.Count == 0) return;
            if (entity == null)
            {
                throw new Exception($"对象参数值是null.[sql语句: {sql}]");
            }

            for (int i = 0; i < paraNames.Count; i++)
            {
                string nameItem = paraNames[i].Value;
                object paraVal;
                FieldInfo f = DBMO.FieldScan<Q>(nameItem[1..], entity);
                if (f != null)
                {
                    paraVal = f.GetValue(entity);
                }
                else
                {
                    PropertyInfo p = DBMO.PropScan<Q>(nameItem[1..], entity);
                    if (p != null)
                        paraVal = p.GetValue(entity);
                    else
                        throw new Exception($"参数占位符 [{paraNames[i]}] 未提供值.[sql语句: {sql}]");
                }
                DbParameter para = this.ParaInstance(nameItem, paraVal);
                this.cmd.Parameters.Add(para);
            }
        }


        /// <summary>
        /// 初始化命令,添加参数.为了存储过程
        /// <para>parasdict为入参(in),outparasdict为出参(out).无参数时传null</para>
        /// </summary>
        private void InItCmdProc(string proc, Dictionary<string, object> parasdict,
            Dictionary<string, int> outparasdict)
        {
            this.CmdInstance(proc);

            // 加入in参数
            if (parasdict != null && parasdict.Count > 0)
            {
                foreach (string key in parasdict.Keys)
                {
                    DbParameter para = this.ParaInstance(key, parasdict[key]);
                    this.cmd.Parameters.Add(para);
                }
            }
            // 加入输出参数
            if (outparasdict != null && outparasdict.Count > 0)
            {
                foreach (string key in outparasdict.Keys)
                {
                    DbParameter outpara = this.OutParaInstance(key, outparasdict[key]);
                    this.cmd.Parameters.Add(outpara);
                }
            }
        }

        /// <summary>
        /// 辅助方法: 实例化db DbConnection对象
        /// </summary>
        /// <returns></returns>
        abstract protected void ConnInstance();

        /// <summary>
        /// 辅助方法: 实例化sql命令对象
        /// </summary>
        abstract protected void CmdInstance(string sql);

        /// <summary>
        /// 辅助方法: 实例化sql参数对象,
        /// </summary>
        abstract protected DbParameter ParaInstance(string name, object val);

        /// <summary>
        /// 辅助方法: 实例化sql参数对象,为了存储过程的传出参数
        /// </summary>
        abstract protected DbParameter OutParaInstance(string name, int dbType);

        #endregion

        #region 执行增删改查存储过程 [内部方法]
        // 实际干活的方法(1.结果集查询 2.非查询 3.标量查询 4.存储过程 )

        /// <summary>
        /// 执行查询,返回字典数组查询结果.
        /// <para>字典的键是表字段名字或别名,值是字段值</para>
        /// </summary>
        private Dictionary<string, object>[] Select()
        {
            try
            {
                this.OpenDB();
                using DbDataReader dr = this.cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    List<Dictionary<string, object>> re = new();
                    while (dr.Read())
                    {
                        Dictionary<string, object> tmp = new();
                        for (int i = 0; i < dr.FieldCount; i++)
                        {
                            tmp.Add(dr.GetName(i), dr[i]);
                        }
                        re.Add(tmp);
                    }
                    return re.ToArray();
                }
                else
                    return null;
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
        /// 执行查询,生成对象集合
        /// <para>T类型是一个实体对象,字段或属性名字与表字段名字对应.C#默认构造函数和字段值</para>
        /// <para>如果对应字段的数据值是DBNULL,那么T的该字段/属性将设置C#默认值.</para>
        /// </summary>
        private T[] Select<T>() where T : new()
        {
            try
            {
                this.OpenDB();
                using DbDataReader dr = this.cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    List<T> redatalist = new();
                    while (dr.Read())
                    {
                        // 创建一个实例
                        T tmp = new();
                        // 循环当行数据行的所有字段
                        for (int i = 0; i < dr.FieldCount; i++)
                        {
                            string name = dr.GetName(i);
                            // 优先查成员
                            FieldInfo field = DBMO.FieldScan<T>(name, tmp);
                            if (field != null)
                            {
                                field.SetValue(tmp, Convert.IsDBNull(dr[i])
                                     ? default : Convert.ChangeType(dr[i], field.FieldType));
                                continue;
                            }
                            // 再属性
                            PropertyInfo prop = DBMO.PropScan<T>(name, tmp);
                            if (prop != null)
                            {
                                prop.SetValue(tmp, Convert.IsDBNull(dr[i])
                                     ? default : Convert.ChangeType(dr[i], prop.PropertyType));
                            }
                            // 成员和属性都没有,无法设置,数据值丢弃
                        }
                        redatalist.Add(tmp);
                    }
                    return redatalist.ToArray();
                }
                else
                    return null;
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
        private int SelectNon()
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
        /// 执行存储过程 异常返回-999;
        /// <para>参数outparasdict: 字典实例.方法成功执行之后,字典会包含输出参数键值对.以输出参数名字为键名</para>
        /// </summary>
        private int Procedure(Dictionary<string, object> outparasdict)
        {
            try
            {
                this.OpenDB();
                // 存储过程加了这句
                this.cmd.CommandType = System.Data.CommandType.StoredProcedure;
                // 执行过程
                int re = this.cmd.ExecuteNonQuery();
                // 找出当前命令参数集合里方向为out的全部参数,设置到outparasdict字典
                for (int i = 0; i < this.cmd.Parameters.Count; i++)
                {
                    var item = this.cmd.Parameters[i];
                    if (item.Direction == System.Data.ParameterDirection.Output)
                    {
                        outparasdict[item.ParameterName] = item.Value;
                    }
                }
                return re;
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

        #region 其它辅助方法

        /// <summary>
        /// 扫描实体类的指定名字public字段,返回字段对象.没找到返回null
        /// <para>name 字段/属性名字,不区分大小写</para>
        /// </summary>
        private static FieldInfo FieldScan<T>(string name, T entity)
        {
            FieldInfo field = entity.GetType().GetField(name,
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.IgnoreCase);
            return field;
        }

        /// <summary>
        /// 扫描实体类的指定名字public属性,返回属性对象.没找到返回null
        /// <para>name 属性名字,不区分大小写</para>
        /// </summary>
        private static PropertyInfo PropScan<T>(string name, T entity)
        {
            PropertyInfo prop = entity.GetType().GetProperty(name,
            BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.IgnoreCase);
            return prop;
        }

        /// <summary>
        /// 自动补全INSERT SQL语句
        /// 指定表名列名,省略VALUES(),方法将自动补全,然后返回完整INSERT SQL
        /// </summary>
        /// <returns></returns>
        private static string AutoCmptInsertSql(string insertSql, char prefixChar)
        {
            string[] colarr = DBMO.FindSqlFieldPart(insertSql);
            StringBuilder sqlpart = new();
            foreach (var item in colarr)
            {
                sqlpart.Append($"{prefixChar}{item.Trim('[', ']')},");
            }
            return $"{insertSql} VALUES({sqlpart.ToString().TrimEnd(',')})";
        }

        /// <summary>
        /// 自动补全UPDATE SQL语句
        /// 指定表名列名,省略SET,方法将自动补全,然后返回完整UPDATE SQL
        /// </summary>
        /// <returns></returns>
        private static string AutoCmptUpdateSql(string updateSql, char prefixChar)
        {
            int sindex = updateSql.IndexOf('(');
            int eindex = updateSql.IndexOf(')');
            string[] colarr = DBMO.FindSqlFieldPart(updateSql);
            StringBuilder sqlpart = new();
            foreach (var item in colarr)
            {
                sqlpart.Append($"{item}={prefixChar}{item.Trim('[', ']')},");
            }
            // 以括号为分界点,前面0-sindex部分是update table 中间是拼成的col=@col eindex-最后,是where部分
            return $"{updateSql.Substring(0, sindex).Trim()} SET {sqlpart.ToString().TrimEnd(',')} {updateSql.Substring(eindex + 1).Trim()}";
        }

        /// <summary>
        /// 找出insert,update语句的字段部分,返回一个数组.例如insert tab (f1,f2),返回[f1,f2]
        /// </summary>
        /// <param name="sqlStr"></param>
        /// <returns></returns>
        private static string[] FindSqlFieldPart(string sqlStr)
        {
            // 传入的update例子 UPDATE TABLE(COL1,COL2,COL3..) where
            // 传入的insert例子 INSERT INTO TABLE(COL1,COL2,COL3..)
            // 1.找出第一个和第二个圆括号的位置2.取出中间的字段名,去掉空白后,逗号分组即可
            int sindex = sqlStr.IndexOf('(');
            int eindex = sqlStr.IndexOf(')');
            if (sindex == -1 || eindex == -1)
                throw new Exception($"请检查update/insert语句是否缺少左右括号:{sqlStr}");
            string cols = sqlStr.Substring(sindex + 1, eindex - sindex - 1);
            // 去掉空白
            cols = Regex.Replace(cols, @"\s", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return cols.Split(',');
        }
        #endregion
    }
}
