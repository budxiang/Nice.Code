
using System;
using System.Data;
using System.Data.SqlClient;


namespace Nice.DataAccess.SqlClient
{
    /// <summary>
    /// MsSql数据库操作的帮助类
    /// </summary>
    public class SqlDataProvider : DataProvider, IDisposable
    {
        private readonly string dbConnString;

        public SqlDataProvider(string _dbConnString)
        {
            dbConnString = _dbConnString;
        }

        public override IDbConnection GetConnection()
        {
            return new SqlConnection(dbConnString);
        }

        public override IDbConnection GetConnection(DatabaseType databaseType, string _dbConnString)
        {
            return new SqlConnection(_dbConnString); 
        }

        public override IDbCommand GetCommand()
        {
            IDbConnection conn = new SqlConnection(dbConnString);
            conn.Open();
            return conn.CreateCommand();
        }

        public override void AttachParameters(IDbCommand command, IDataParameter[] dbps)
        {
            command.Parameters.Clear();
            foreach (IDataParameter p in dbps)
            {
                if (p.Value == null)
                {
                    p.Value = DBNull.Value;
                }
                command.Parameters.Add(p);
            }
        }

        public override IDbDataAdapter GetDataAdapter(IDbCommand command)
        {
            return new SqlDataAdapter((SqlCommand)command);
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
        ~SqlDataProvider()
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
            // 清理非托管资源……

            //让类型知道自己已经被释放
            isDisposed = true;
        }
        #endregion
    }
}
