using Nice.Core.Log;
using System;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace Nice.DataAccess
{
    /// <summary>
    /// 数据库帮助类
    /// </summary>
    public class DataHelper
    {
        private static DatabaseType databaseType;
        private static DataProvider dataProvider = null;
        private static int CommandTimeout = 30;

        public static void Create()
        {
            Create(DatabaseSettings.BbConnString, DatabaseSettings.ProviderName, DatabaseSettings.CommandTimeOut);
        }

        /// <summary>
        /// 初始化数据库连接参数，在程序启动时初始化
        /// </summary>
        /// <param name="providerName">数据库驱动名</param>
        /// <param name="dbConnString">连接字符串</param>
        public static void Create(string dbConnString, string providerName, int _CommandTimeout)
        {
            object[] args = new object[] { dbConnString };
            databaseType = DatabaseTypeEx.Convert(providerName);
            dataProvider = (DataProvider)Assembly.Load(DatabaseSettings.DataProviderAssembly).CreateInstance(DatabaseSettings.DataProviderTypeName,true, BindingFlags.Default, null, args, null, null);
            CommandTimeout = _CommandTimeout;
        }

        public static DatabaseType DatabaseType
        {
            get
            {
                return databaseType;
            }
        }
        /// <summary>
        /// 判断当前数据库是否成功连接，无DbException
        /// </summary>
        /// <returns></returns>
        public static bool IsConnection()
        {
            IDbConnection conn = null;
            try
            {
                conn = dataProvider.GetConnection();
                conn.Open();
                return true;
            }
            catch (DbException ex)
            {
                Logging.Error(ex);
                return false;
            }
            catch (Exception ex)
            {
                Logging.Error(ex);
                return false;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }
        /// <summary>
        /// 根据提供的数据库驱动名和连接字符串判断是否成功连接
        /// </summary>
        /// <returns></returns>
        public static bool IsConnection(DatabaseType providerName, string dbConnStrings)
        {
            IDbConnection conn = null;
            try
            {
                conn = dataProvider.GetConnection(providerName, dbConnStrings);
                conn.Open();
                return true;
            }
            catch (DbException ex)
            {
                Logging.Error(ex);
                return false;
            }
            catch (Exception ex)
            {
                Logging.Error(ex);
                return false;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }
        /// <summary>
        /// 执行IDbCommand类的ExecuteNonQuery方法，返回受影响的行数
        /// </summary>
        /// <param name="cmdText">需要执行的sql语句</param>
        /// <param name="dbps">sql语句需要的参数，没有参数则传入null</param>
        /// <param name="commandType">执行方式，默认CommandType.Text</param>
        /// <returns>返回受影响的行数，失败返回0</returns>
        public static int ExecuteNonQuery(string cmdText, CommandType commandType, params IDataParameter[] dbps)
        {
            IDbCommand cmd = null;
            try
            {
                cmd = dataProvider.GetCommand();
                cmd.CommandText = cmdText;
                if (dbps != null)
                {
                    dataProvider.AttachParameters(cmd, dbps);
                }
                cmd.CommandType = commandType;
                cmd.CommandTimeout = CommandTimeout;
                return cmd.ExecuteNonQuery(); ;
            }
            catch (DbException ex)
            {
                Logging.Error(ex, cmdText);
                return 0;
            }
            catch (Exception ex)
            {
                Logging.Error(ex);
                return 0;
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Connection.Close();
                    cmd.Connection.Dispose();
                }
            }
        }
        /// <summary>
        /// 执行IDbCommand类的ExecuteScalar方法,返回object
        /// </summary>
        /// <param name="cmdText">需要执行的sql语句</param>
        /// <param name="dbps">sql语句需要的参数，没有参数则传入null</param>
        /// <param name="commandType">执行方式，默认CommandType.Text</param>
        /// <returns>返回object类型数据，失败返回null</returns>
        public static object ExecuteScalar(string cmdText, CommandType commandType = CommandType.Text, params IDataParameter[] dbps)
        {
            IDbCommand cmd = null;
            try
            {
                cmd = dataProvider.GetCommand();
                cmd.CommandText = cmdText;

                if (dbps != null)
                {
                    dataProvider.AttachParameters(cmd, dbps);
                }
                cmd.CommandType = commandType;
                cmd.CommandTimeout = CommandTimeout;
                return cmd.ExecuteScalar();
            }
            catch (DbException ex)
            {
                Logging.Error(ex, cmdText);
                return null;
            }
            catch (Exception ex)
            {
                Logging.Error(ex);
                return null;
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Connection.Close();
                    cmd.Connection.Dispose();
                }
            }
        }
        /// <summary>
        /// 执行IDbCommand类的ExecuteReader方法,返回SqlDataReader
        /// [调用完成后，请关闭IDataReader对象]
        /// </summary>
        /// <param name="cmdText">需要执行的sql语句</param>
        /// <param name="dbps">sql语句需要的参数，没有参数则传入null</param>
        /// <param name="commandType">执行方式，默认CommandType.Text</param>
        /// <returns>返回SqlDataReader</returns>
        public static IDataReader ExecuteReader(string cmdText, CommandType commandType = CommandType.Text, params IDataParameter[] dbps)
        {
            IDbCommand cmd = null;
            try
            {
                cmd = dataProvider.GetCommand();
                cmd.CommandText = cmdText;
                if (dbps != null)
                {
                    dataProvider.AttachParameters(cmd, dbps);
                }
                cmd.CommandTimeout = CommandTimeout;
                cmd.CommandType = commandType;
                //CommandBehavior.CloseConnection在执行该命令时，如果关闭关联的 DataReader 对象，则关联的 Connection 对象也将关闭。
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (DbException ex)
            {
                Logging.Error(ex, cmdText);
                return null;
            }
            catch (Exception ex)
            {
                Logging.Error(ex);
                return null;
            }
            //finally
            //{
            //    cmd.Connection.Close();
            //    cmd.Connection.Dispose();
            //}
        }
        /// <summary>
        /// 执行DataSet.Fill,返回DataSet
        /// </summary>
        /// <param name="cmdText">需要执行的sql语句</param>
        /// <param name="dbps">sql语句需要的参数，没有参数则传入null</param>
        /// <param name="commandType">执行方式，默认CommandType.Text</param>
        /// <returns></returns>
        public static DataSet ExecuteDataSet(string cmdText, CommandType commandType = CommandType.Text, params IDataParameter[] dbps)
        {
            IDbCommand cmd = null;
            try
            {
                cmd = dataProvider.GetCommand();
                cmd.CommandText = cmdText;
                if (dbps != null)
                {
                    dataProvider.AttachParameters(cmd, dbps);
                }
                cmd.CommandTimeout = CommandTimeout;
                cmd.CommandType = commandType;

                IDbDataAdapter da = dataProvider.GetDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                da = null;
                return ds;
            }
            catch (DbException ex)
            {
                Logging.Error(ex, cmdText);
                return null;
            }
            catch (Exception ex)
            {
                Logging.Error(ex);
                return null;
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Connection.Close();
                    cmd.Connection.Dispose();
                }
            }
        }
        /// <summary>
        /// 执行DataSet.Fill,返回DataSet
        /// </summary>
        /// <param name="cmdText">需要执行的sql语句</param>
        /// <param name="dbps">sql语句需要的参数，没有参数则传入null</param>
        /// <param name="commandType">执行方式，默认CommandType.Text</param>
        /// <returns></returns>
        public static DataTable ExecuteDataTable(string cmdText, CommandType commandType = CommandType.Text, params IDataParameter[] dbps)
        {
            IDbCommand cmd = null;
            try
            {
                cmd = dataProvider.GetCommand();
                cmd.CommandText = cmdText;
                if (dbps != null)
                {
                    dataProvider.AttachParameters(cmd, dbps);
                }
                cmd.CommandType = commandType;
                cmd.CommandTimeout = CommandTimeout;
                IDataAdapter da = dataProvider.GetDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                //da.Dispose();
                da = null;
                return ds.Tables[0];
            }
            catch (DbException ex)
            {
                Logging.Error(ex, cmdText);
                return null;
            }
            catch (Exception ex)
            {
                Logging.Error(ex);
                return null;
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Connection.Close();
                    cmd.Connection.Dispose();
                }
            }
        }
        /// <summary>
        /// 批处理 ExecuteNonQuery方法，返回受影响的行数
        /// </summary>
        /// <param name="cmdText">需要执行的sql语句</param>
        /// <param name="dbps">sql语句需要的参数，没有参数则传入null</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string[] cmdText, IDataParameter[][] dbps)
        {
            IDbConnection conn = dataProvider.GetConnection();
            IDbTransaction trans = null;
            int n = 0;
            try
            {
                conn.Open();
                IDbCommand cmd = conn.CreateCommand();
                cmd.Connection = conn;
                trans = conn.BeginTransaction();
                cmd.Transaction = trans;
                cmd.CommandTimeout = CommandTimeout;
                int count = 0;
                for (; n < cmdText.Length; n++)
                {
                    cmd.CommandText = cmdText[n];
                    if (dbps != null && dbps[n] != null)
                        dataProvider.AttachParameters(cmd, dbps[n]);
                    count += cmd.ExecuteNonQuery();

                }
                trans.Commit();
                return count;
            }
            catch (DbException ex)
            {
                if (trans != null)
                    trans.Rollback();
                Logging.Error(ex, cmdText[n]);
                return -1;
            }
            catch (Exception ex)
            {
                if (trans != null)
                    trans.Rollback();
                Logging.Error(ex);
                return -1;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }


        #region Dispose
        private bool isDisposed = false;

        /// <summary>
        /// 实现IDisposable中的Dispose方法
        /// </summary>
        public void Dispose()
        {
            //必须为true
            Dispose(true);
            //通知垃圾回收机制不再调用终结器（析构器）
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 不是必要的，提供一个Close方法仅仅是为了更符合其他语言（如C++）的规范
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// 必须，以备程序员忘记了显式调用Dispose方法
        /// </summary>
        ~DataHelper()
        {
            //必须为false
            Dispose(false);
        }

        /// <summary>
        /// 非密封类修饰用protected virtual
        /// 密封类修饰用private
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }
            if (disposing)
            {
                // 清理托管资源
                //if (DbFactory != null)
                //{
                //    DbFactory = null;
                //}

            }
            //清理非托管资源……

            //让类型知道自己已经被释放
            isDisposed = true;
        }
        #endregion
    }
}
