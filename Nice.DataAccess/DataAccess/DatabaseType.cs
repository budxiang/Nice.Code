namespace Nice.DataAccess
{
    /// <summary>
    /// 连接数据库类型
    /// </summary>
    public enum DatabaseType
    {
        SqlServer = 1,
        Oracle = 2,
        MySQL = 3,
        Postgres = 4,
        Odbc = 5,
        Unknown = 0
    }

    public class DatabaseTypeEx
    {
        /// <summary>
        /// 将驱动器名转化为连接数据库类型
        /// </summary>
        /// <param name="providerName"></param>
        /// <returns></returns>
        public static DatabaseType Convert(string providerName)
        {
            providerName = providerName.ToUpper();
            if (providerName == "" || providerName == "SYSTEM.DATA.SQLCLIENT") return DatabaseType.SqlServer;
            else if (providerName == "NPGSQL") return DatabaseType.Postgres;
            else if (providerName == "MYSQL") return DatabaseType.MySQL;
            else if (providerName == "Oracle.ManagedDataAccess.Client") return DatabaseType.Oracle;
            else if (providerName == "SYSTEM.DATA.ODBC") return DatabaseType.Odbc;
            else return DatabaseType.Unknown;
        }
       
    }
}
