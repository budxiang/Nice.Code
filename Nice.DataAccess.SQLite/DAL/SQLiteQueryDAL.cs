using Nice.DataAccess.DAL;
using Nice.DataAccess.Model.Page;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Nice.DataAccess.SQLite.DAL
{
    public class SQLiteQueryDAL<T> : IQueryDAL<T> where T :  new()
    {
        /// <summary>
        /// 类型属性集合
        /// </summary>
        private PropertyInfo[] properties;
        private const string flagWhere = " WHERE ";
        private const string flagFrom = " FROM ";
        private const string flagAnd = " AND ";

        public SQLiteQueryDAL()
        {
            GetPropertyInfo();
        }

        protected void GetPropertyInfo()
        {
            Type type = typeof(T);
            properties = type.GetProperties();
        }

        /// <summary>
        /// 过滤属性
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="propertys"></param>
        private void FilterProperty(DataTable dt, PropertyInfo[] propertys, ref PropertyInfo[] querypropertys)
        {
            IList<PropertyInfo> propertyArray = new List<PropertyInfo>(20);

            foreach (PropertyInfo pi in propertys)
            {
                if (dt.Columns.Contains(pi.Name))
                {
                    propertyArray.Add(pi);
                }
            }

            querypropertys = propertyArray.ToArray();
        }

        #region QL查询
        /// <summary>
        /// QL查询集合
        /// </summary>
        /// <param name="IdValue">主键</param>
        /// <returns></returns>
        public IList<T> GetList(string cmdText)
        {
            IList<T> result = null;
            FilterQueryText(ref cmdText);
            DataTable dt = DataHelper.ExecuteDataTable(cmdText, CommandType.Text, null);
            if (dt != null && dt.Rows.Count > 0)
            {
                T t = default(T);
                object value = null;
                result = new List<T>();
                PropertyInfo[] querypropertys = null;
                FilterProperty(dt, properties, ref querypropertys);
                foreach (DataRow dr in dt.Rows)
                {
                    t = new T();
                    foreach (PropertyInfo pi in querypropertys)
                    {
                        value = dr[pi.Name];
                        if (value != DBNull.Value)
                            pi.SetValue(t, value, null);
                    }
                    result.Add(t);
                }
            }
            return result;
        }

        /// <summary>
        /// QL查询集合
        /// </summary>
        /// <param name="IdValue">主键</param>
        /// <returns></returns>
        public IList<T> GetList(string cmdText, object[] parmsValue)
        {
            IList<T> result = null;
            IDataParameter[] parms = null;
            if (parmsValue != null)
                FilterQueryText(ref cmdText, parmsValue, ref parms);

            DataTable dt = DataHelper.ExecuteDataTable(cmdText, CommandType.Text, parms);
            if (dt != null && dt.Rows.Count > 0)
            {
                T t = default(T);
                object value = null;
                result = new List<T>();
                PropertyInfo[] querypropertys = null;
                FilterProperty(dt, properties, ref querypropertys);
                foreach (DataRow dr in dt.Rows)
                {
                    t = new T();
                    foreach (PropertyInfo pi in querypropertys)
                    {
                        value = dr[pi.Name];
                        if (value != DBNull.Value)
                            pi.SetValue(t, value, null);
                    }
                    result.Add(t);
                }
            }
            return result;
        }

        /// <summary>
        /// 过滤QL
        /// </summary>
        /// <param name="cmdText"></param>
        private void FilterQueryText(ref string cmdText)
        {
            StringBuilder sb = new StringBuilder();
            cmdText = cmdText.ToUpper();
            int index = cmdText.IndexOf(flagWhere);
            string strSelect = cmdText.Remove(index);
            int indexTable = cmdText.IndexOf(flagFrom);
            string strTable = strSelect.Substring(indexTable + flagFrom.Length);
            sb.Append(strSelect.Remove(indexTable));
            sb.Append(flagFrom);
            //类名转表名
            string[] entityNames = strTable.Split(',');
            string entityName = null;
            for (int i = 0; i < entityNames.Length; i++)
            {
                entityName = entityNames[i];
                sb.Append(DataEntityFactory.EntityAndTables[entityName]);
                if (i != entityNames.Length - 1)
                    sb.Append(',');
            }

            string strWhere = cmdText.Substring(index + flagWhere.Length);
            sb.Append(flagWhere);
            PropertyToColumnName(strWhere, sb);
            cmdText = sb.ToString();
        }
        /// <summary>
        /// 过滤QL
        /// </summary>
        /// <param name="cmdText"></param>
        private void FilterQueryText(ref string cmdText, object[] parmsValue, ref IDataParameter[] parms)
        {
            StringBuilder sb = new StringBuilder();
            parms = new MySqlParameter[parmsValue.Length];
            cmdText = cmdText.ToUpper();
            int index = cmdText.IndexOf(flagWhere);
            string strSelect = cmdText.Remove(index);
            int indexTable = cmdText.IndexOf(flagFrom);
            string strTable = strSelect.Substring(indexTable + flagFrom.Length);
            sb.Append(strSelect.Remove(indexTable));
            sb.Append(flagFrom);
            //类名转表名
            string[] entityNames = strTable.Split(',');
            string entityName = null;
            for (int i = 0; i < entityNames.Length; i++)
            {
                entityName = entityNames[i];
                sb.Append(DataEntityFactory.EntityAndTables[entityName]);
                if (i != entityNames.Length - 1)
                    sb.Append(',');
            }

            //将属性名转换为表字段名
            string strWhere = cmdText.Substring(index + flagWhere.Length);
            sb.Append(flagWhere);
            int parmIndex = 0;
            PropertyToColumnName(strWhere, sb, parmsValue, parms, ref parmIndex);
            cmdText = sb.ToString();
        }

        /// <summary>
        /// 将属性名转换为表字段名
        /// </summary>
        private void PropertyToColumnName(string strWhere, StringBuilder sb)
        {

            int indexFlag = strWhere.IndexOf('=');
            string nameAndColumn = strWhere.Substring(0, indexFlag).Trim();
            sb.Append(' ');
            sb.Append(DataEntityFactory.PropertyAndColumns[nameAndColumn]);

            string nextNameAndColumn = strWhere.Substring(indexFlag).Trim();
            indexFlag = nextNameAndColumn.IndexOf(flagAnd);

            sb.Append('=');
            if (indexFlag == -1)
            {
                sb.Append(nextNameAndColumn.Trim().TrimStart('='));
                return;
            }
            else
            {
                sb.Append(nextNameAndColumn.Substring(0, indexFlag).Trim().TrimStart('='));
                sb.Append(flagAnd);
                PropertyToColumnName(nextNameAndColumn, sb);
            }
        }
        /// <summary>
        /// 将属性名转换为表字段名
        /// </summary>
        private void PropertyToColumnName(string strWhere, StringBuilder sb, object[] parmsValue, IDataParameter[] parms, ref int parmIndex)
        {

            int indexFlag = strWhere.IndexOf('=');
            string columnName = strWhere.Substring(0, indexFlag).Trim();
            sb.Append(' ');
            sb.Append(DataEntityFactory.PropertyAndColumns[columnName]);

            string nextColumnName = strWhere.Substring(indexFlag).Trim();
            indexFlag = nextColumnName.IndexOf(flagAnd);
            string columnValue = null;
            sb.Append('=');
            if (indexFlag == -1)
            {
                columnValue = nextColumnName.Trim().TrimStart('=');
                if (columnValue.Equals("?"))
                {
                    sb.AppendFormat("@{0}", parmIndex);
                    parms[parmIndex] = new MySqlParameter("@" + parmIndex, parmsValue[parmIndex]);
                    parmIndex++;
                }
                else
                {
                    sb.Append(columnValue);
                }
                return;
            }
            else
            {
                columnValue = nextColumnName.Substring(0, indexFlag).Trim().TrimStart('=');
                nextColumnName = nextColumnName.Substring(indexFlag + flagAnd.Length);
                if (columnValue.Equals("?"))
                {
                    sb.AppendFormat("@{0}", parmIndex);
                    parms[parmIndex] = new MySqlParameter("@" + parmIndex, parmsValue[parmIndex]);
                    parmIndex++;
                }
                else
                {
                    sb.Append(columnValue);
                }
                sb.Append(flagAnd);
                PropertyToColumnName(nextColumnName, sb, parmsValue, parms, ref parmIndex);
            }
        }

        #endregion

        #region SQL查询
        /// <summary>
        /// SQL查询对象
        /// </summary>
        /// <param name="IdValue">主键</param>
        /// <returns></returns>
        public T GetBySQL(string cmdText)
        {
            T t = default(T);
            DataTable dt = DataHelper.ExecuteDataTable(cmdText, CommandType.Text, null);
            if (dt != null && dt.Rows.Count > 0)
            {
                t = new T();
                object value = null;
                DataRow dr = dt.Rows[0];
                foreach (PropertyInfo pi in properties)
                {
                    if (dt.Columns.Contains(pi.Name))
                    {
                        value = dr[pi.Name];
                        if (value != DBNull.Value)
                            pi.SetValue(t, value, null);
                    }
                }
            }
            return t;
        }
        /// <summary>
        /// SQL查询对象
        /// </summary>
        /// <param name="IdValue">主键</param>
        /// <returns></returns>
        public T GetBySQL(string cmdText, object[] parmsValue)
        {
            T t = default(T);
            IDataParameter[] parms = null;
            if (parmsValue != null)
                FilterSQLParmeters(ref cmdText, parmsValue, ref parms);
            DataTable dt = DataHelper.ExecuteDataTable(cmdText, CommandType.Text, parms);
            if (dt != null && dt.Rows.Count > 0)
            {
                t = new T();
                object value = null;
                DataRow dr = dt.Rows[0];
                //SetValueDelegate setter = null;
                foreach (PropertyInfo pi in properties)
                {
                    if (dt.Columns.Contains(pi.Name))
                    {
                        value = dr[pi.Name];
                        if (value != DBNull.Value)
                        {
                            pi.SetValue(t, value, null);
                            //setter = FastMethodFactory.CreatePropertySetter(pi);
                            //setter(t, value);
                        }
                    }
                }
            }
            return t;
        }
        /// <summary>
        /// SQL查询列表
        /// </summary>
        /// <param name="IdValue">主键</param>
        /// <returns></returns>
        public IList<T> GetListBySQL(string cmdText)
        {
            IList<T> result = null;

            DataTable dt = DataHelper.ExecuteDataTable(cmdText, CommandType.Text, null);
            if (dt != null && dt.Rows.Count > 0)
            {
                T t = default(T);
                object value = null;
                result = new List<T>();
                PropertyInfo[] querypropertys = null;
                FilterProperty(dt, properties, ref querypropertys);
                //SetValueDelegate setter = null;
                foreach (DataRow dr in dt.Rows)
                {
                    t = new T();
                    foreach (PropertyInfo pi in querypropertys)
                    {
                        value = dr[pi.Name];
                        if (value != DBNull.Value)
                        {
                            pi.SetValue(t, value, null);
                            //setter = FastMethodFactory.CreatePropertySetter(pi);
                            //setter(t, value);
                        }
                    }
                    result.Add(t);
                }
            }
            return result;
        }
        /// <summary>
        /// SQL查询列表
        /// </summary>
        /// <param name="cmdText">SQL</param>
        /// <returns></returns>
        public IList<T> GetListBySQL(string cmdText, PageInfo page)
        {
            IList<T> result = null;
            StringBuilder sb = new StringBuilder(100);
            sb.AppendFormat(cmdText);
            if (page != null)
            {
                sb.AppendFormat(" ORDER BY {0} {1} LIMIT {2},{3}; ", page.OrderColName, page.OrderStr, page.StartIndex, page.PageSize);
                sb.AppendFormat(" SELECT COUNT(1) FROM ({0}) T;", cmdText);
            }
            DataSet ds = DataHelper.ExecuteDataSet(sb.ToString(), CommandType.Text, null);
            if (ds != null && ds.Tables.Count == 2)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows.Count > 0)
                {
                    T t = default(T);
                    object value = null;
                    result = new List<T>();
                    PropertyInfo[] querypropertys = null;
                    FilterProperty(dt, properties, ref querypropertys);
                    //SetValueDelegate setter = null;
                    foreach (DataRow dr in dt.Rows)
                    {
                        t = new T();
                        foreach (PropertyInfo pi in querypropertys)
                        {
                            value = dr[pi.Name];
                            if (value != DBNull.Value)
                            {
                                pi.SetValue(t, value, null);
                                //setter = FastMethodFactory.CreatePropertySetter(pi);
                                //setter(t, value);
                            }
                        }
                        result.Add(t);
                    }
                }
                page.TotalCount = int.Parse(ds.Tables[1].Rows[0][0].ToString());
            }
            return result;
        }
        /// <summary>
        /// SQL查询列表
        /// </summary>
        /// <param name="cmdText">SQL</param>
        /// <returns></returns>
        public IList<T> GetListBySQL(string cmdText, object[] parmsValue, PageInfo page)
        {
            IList<T> result = null;
            IDataParameter[] parms = null;
            if (parmsValue != null)
                FilterSQLParmeters(ref cmdText, parmsValue, ref parms);
            StringBuilder sb = new StringBuilder(100);
            sb.AppendFormat(cmdText);
            if (page != null)
            {
                sb.AppendFormat(" ORDER BY {0} {1} LIMIT {2},{3}; ", page.OrderColName, page.OrderStr, page.StartIndex, page.PageSize);
                sb.AppendFormat(" SELECT COUNT(1) FROM ({0}) T;", cmdText);
            }
            DataSet ds = DataHelper.ExecuteDataSet(sb.ToString(), CommandType.Text, parms);
            if (ds != null && ds.Tables.Count == 2)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows.Count > 0)
                {
                    T t = default(T);
                    object value = null;
                    result = new List<T>();
                    PropertyInfo[] querypropertys = null;
                    FilterProperty(dt, properties, ref querypropertys);
                    //SetValueDelegate setter = null;
                    foreach (DataRow dr in dt.Rows)
                    {
                        t = new T();
                        foreach (PropertyInfo pi in querypropertys)
                        {
                            value = dr[pi.Name];
                            if (value != DBNull.Value)
                            {
                                pi.SetValue(t, value, null);
                                //setter = FastMethodFactory.CreatePropertySetter(pi);
                                //setter(t, value);
                            }
                        }
                        result.Add(t);
                    }
                }
                page.TotalCount = int.Parse(ds.Tables[1].Rows[0][0].ToString());
            }
            return result;
        }
        /// <summary>
        /// SQL查询列表
        /// </summary>
        /// <param name="IdValue">主键</param>
        /// <returns></returns>
        public IList<T> GetListBySQL(string cmdText, object[] parmsValue)
        {
            IList<T> result = null;
            IDataParameter[] parms = null;
            if (parmsValue != null)
                FilterSQLParmeters(ref cmdText, parmsValue, ref parms);

            DataTable dt = DataHelper.ExecuteDataTable(cmdText, CommandType.Text, parms);
            if (dt != null && dt.Rows.Count > 0)
            {
                T t = default(T);
                object value = null;
                result = new List<T>();
                PropertyInfo[] querypropertys = null;
                FilterProperty(dt, properties, ref querypropertys);
                //SetValueDelegate setter = null;
                foreach (DataRow dr in dt.Rows)
                {
                    t = new T();
                    foreach (PropertyInfo pi in querypropertys)
                    {
                        value = dr[pi.Name];
                        if (value != DBNull.Value)
                        {
                            pi.SetValue(t, value, null);
                            //setter = FastMethodFactory.CreatePropertySetter(pi);
                            //setter(t, value);
                        }
                    }
                    result.Add(t);
                }
            }
            return result;
        }
        /// <summary>
        /// 过滤SQL参数
        /// </summary>
        /// <param name="cmdText"></param>
        private void FilterSQLParmeters(ref string cmdText, object[] parmsValue, ref IDataParameter[] parms)
        {
            StringBuilder sb = new StringBuilder();
            
            parms = new SQLiteParameter[parmsValue.Length];
            char c;
            int parmIndex = 0;
            for (int i = 0; i < cmdText.Length; i++)
            {
                c = cmdText[i];
                if (c.Equals('?'))
                {
                    sb.AppendFormat("@{0}", parmIndex);
                    parms[parmIndex] = new MySqlParameter("@" + parmIndex, parmsValue[parmIndex]);
                    parmIndex++;
                }
                else
                    sb.Append(c);
            }
            cmdText = sb.ToString();
        }
        #endregion

    }

    public class SQLiteQueryDAL : IQueryDAL
    {

        #region  执行SQL
        /// <summary>
        /// 执行SQL
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parmsValue"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string cmdText, object[] parmsValue)
        {
            IDataParameter[] parms = null;
            if (parmsValue != null)
                FilterSQLParmeters(ref cmdText, parmsValue, ref parms);

            return DataHelper.ExecuteNonQuery(cmdText, CommandType.Text, parms);
        }
        /// <summary>
        /// 执行SQL返回第一行第一列
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parmsValue"></param>
        /// <returns></returns>
        public object ExecuteScalar(string cmdText, object[] parmsValue)
        {
            IDataParameter[] parms = null;
            if (parmsValue != null)
                FilterSQLParmeters(ref cmdText, parmsValue, ref parms);

            return DataHelper.ExecuteScalar(cmdText, CommandType.Text, parms);
        }
        /// <summary>
        /// 执行SQL事务
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parmsValue"></param>
        /// <returns></returns>
        public int ExecuteNonQuery(string[] cmdText, object[][] parmsValue)
        {
            IDataParameter[] parms = null;
            IDataParameter[][] dbps = null;
            string text = null;
            if (parmsValue != null)
            {
                dbps = new MySqlParameter[cmdText.Length][];
                for (int i = 0; i < cmdText.Length; i++)
                {
                    text = cmdText[i];
                    if (parmsValue[i] != null)
                        FilterSQLParmeters(ref text, parmsValue[i], ref parms);
                    cmdText[i] = text;
                    dbps[i] = parms;
                }
            }
            return DataHelper.ExecuteNonQuery(cmdText, dbps);
        }

        /// <summary>
        /// 返回Dataset
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parmsValue"></param>
        /// <returns></returns>
        //public DataSet ExecuteDataSet(string cmdText, object[] parmsValue)
        //{
        //    IDataParameter[] parms = null;
        //    if (parmsValue != null)
        //        FilterSQLParmeters(ref cmdText, parmsValue, ref parms);
        //    return DataHelper.ExecuteDataSet(cmdText, CommandType.Text, parms);
        //}

        /// <summary>
        /// 过滤SQL参数
        /// </summary>
        /// <param name="cmdText"></param>
        private void FilterSQLParmeters(ref string cmdText, object[] parmsValue, ref IDataParameter[] parms)
        {
            StringBuilder sb = new StringBuilder();
            parms = new MySqlParameter[parmsValue.Length];
            char c;
            int parmIndex = 0;
            for (int i = 0; i < cmdText.Length; i++)
            {
                c = cmdText[i];
                if (c.Equals('?'))
                {
                    sb.AppendFormat("@{0}", parmIndex);
                    parms[parmIndex] = new MySqlParameter("@" + parmIndex, parmsValue[parmIndex]);
                    parmIndex++;
                }
                else
                    sb.Append(c);
            }
            cmdText = sb.ToString();
        }
        #endregion
    }
}