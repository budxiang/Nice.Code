using Nice.DataAccess.Model.Page;
using Nice.DataAccess.Models;
using System.Collections.Generic;

namespace Nice.DataAccess.DAL
{
    /// <summary>
    /// 数据访问层基础类
    /// </summary>
    /// <typeparam name="T">TEntity实体</typeparam>
    public interface IGeneralDAL<T> where T : TEntity
    {
        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        bool Insert(T t);
        /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        bool Insert(IList<T> list);
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        bool Delete(T t);
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="idValue"></param>
        /// <returns></returns>
        bool Delete(object idValue);
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        bool Update(T t);
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        bool Update(IList<T> list);
        /// <summary>
        /// 获取实体
        /// </summary>
        /// <returns></returns>
        T Get(object id);
        /// <summary>
        /// 获取实体集合
        /// </summary>
        /// <returns></returns>
        IList<T> GetList();
        /// <summary>
        /// 获取实体分页集合
        /// </summary>
        /// <returns></returns>
        IList<T> GetList(PageInfo page);

        /// <summary>
        /// 是否存在
        /// </summary>
        /// <returns></returns>
        bool IsExist(string PropertyName, object PropertyValue, object IdValue);


        /// <summary>
        /// 是否存在
        /// </summary>
        /// <returns></returns>
        bool VirtualDelete(object IdValue);
        /// <summary>
        /// 插入数据并返回实体
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        bool InsertAndGet(T t);
    }
}
