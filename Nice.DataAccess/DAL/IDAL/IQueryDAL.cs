using Nice.DataAccess.Model.Page;
using System.Collections.Generic;

namespace Nice.DataAccess.DAL
{
    public interface IQueryDAL<T> where T : new()
    {
        T GetBySQL(string cmdText);
        T GetBySQL(string cmdText, object[] parmsValue);
        IList<T> GetListBySQL(string cmdText);
        /// <summary>
        /// SQL查询列表
        /// </summary>
        /// <param name="cmdText">SQL</param>
        /// <returns></returns>
        IList<T> GetListBySQL(string cmdText, PageInfo page);
        /// <summary>
        /// SQL查询列表
        /// </summary>
        /// <param name="IdValue">主键</param>
        /// <returns></returns>
        IList<T> GetListBySQL(string cmdText, object[] parmsValue);
        /// <summary>
        /// SQL查询列表
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parmsValue"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        IList<T> GetListBySQL(string cmdText, object[] parmsValue, PageInfo page);
    }

    public interface IQueryDAL
    {

        #region  执行SQL
        /// <summary>
        /// 执行SQL
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parmsValue"></param>
        /// <returns></returns>
        int ExecuteNonQuery(string cmdText, object[] parmsValue);
        /// <summary>
        /// 执行SQL返回第一行第一列
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parmsValue"></param>
        /// <returns></returns>
        object ExecuteScalar(string cmdText, object[] parmsValue);

        /// <summary>
        /// 执行SQL事务
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parmsValue"></param>
        /// <returns></returns>
        int ExecuteNonQuery(string[] cmdText, object[][] parmsValue);

        /// <summary>
        /// 返回Dataset
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parmsValue"></param>
        /// <returns></returns>
        //DataSet ExecuteDataSet(string cmdText, object[] parmsValue);
        #endregion
    }
}
