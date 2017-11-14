using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using Nice.DataAccess.Attributes;
using Nice.DataAccess.Exceptions;
using Nice.DataAccess.TypeEx;

namespace Nice.DataAccess.DAL
{
    public class BaseDAL<T>
    {
        /// <summary>
        /// 类名
        /// </summary>
        protected string ClassName;
        /// <summary>
        /// 表名
        /// </summary>
        protected string TableName;
        /// <summary>
        /// 主键列名
        /// </summary>
        protected string IdColomnName;
        /// <summary>
        /// 主键属性
        /// </summary>
        protected PropertyInfo IdProperty;
        /// <summary>
        /// ID生成类型
        /// </summary>
        protected IdGenerateType thisIdGenerateType;

        /// <summary>
        /// 类型属性集合
        /// </summary>
        protected PropertyInfo[] tableProperties;
        /// <summary>
        /// 属性名与数据库字段名对应
        /// </summary>
        protected Mapping<string, string> propertyAndColumn;

        protected const string flagWhere = " WHERE ";
        protected const string flagFrom = " FROM ";
        protected const string flagAnd = " AND ";
        protected string GetColumnText;//获取列数据列名
        protected string SetColumnText;//设置列数据列名
        protected string InsertColumnText;//插入列数据列名
        protected DbParameter ParamFilterValid = null;
        protected DbParameter ParamFilterInValid = null;
        protected string ValidColumnName = null;

        protected BaseDAL()
        {
            GetPropertyInfo();
        }
        #region 过滤属性和字段名信息
        private void GetPropertyInfo()
        {
            Type type = typeof(T);
            ClassName = type.Name;
            PropertyInfo[] properties = type.GetProperties();

            //表名
            TableAttribute tableAttri = type.GetCustomAttribute<TableAttribute>();
            if (tableAttri == null)
                TableName = ClassName;
            else
                TableName = tableAttri.Name;

            //属性与字段名对应
            propertyAndColumn = new Mapping<string, string>(properties.Length);

            StringBuilder sbGetColumns = new StringBuilder(50);
            StringBuilder sbSetColumns = new StringBuilder(50);
            StringBuilder sbInsertColumn = new StringBuilder(50);
            IList<PropertyInfo> propertyArray = new List<PropertyInfo>();
            GetPropertyInfo(properties, propertyArray, sbGetColumns, sbSetColumns, sbInsertColumn);
           
            tableProperties = propertyArray.ToArray();
            GetColumnText = sbGetColumns.ToString().Trim(',');
            SetColumnText = sbSetColumns.ToString().Trim(',');
            InsertColumnText = sbInsertColumn.ToString().Trim(',');
            if (IdColomnName == null)
                throw new IdNotImplementedException();
        }

        /// <summary>
        /// 获取类型属性和字段名信息
        /// </summary>

        private void GetPropertyInfo(PropertyInfo[] properties, IList<PropertyInfo> propertyArray, StringBuilder sbGetColumns, StringBuilder sbSetColumns, StringBuilder sbInsertColumn)
        {
            string propertyName = null;
            string columnName = null;
            foreach (PropertyInfo pi in properties)
            {
                propertyArray.Add(pi);
                //过滤暂存、附加字段
                if (pi.GetCustomAttribute<TransientAttribute>() != null) continue;

                ColumnAttribute attri = pi.GetCustomAttribute<ColumnAttribute>();
                propertyName = pi.Name;
                if (attri == null)
                    columnName = propertyName;
                else
                    columnName = attri.Name;
                propertyAndColumn.Add(propertyName.ToUpper(), columnName);
                sbGetColumns.AppendFormat("{0}.{1} {2},", ClassName, columnName, propertyName);

                IdAttribute idAttri = pi.GetCustomAttribute<IdAttribute>();

                if (idAttri == null)
                {
                    sbInsertColumn.AppendFormat(" {0},", columnName);
                    if (attri == null || !attri.IsReadOnly)
                        sbSetColumns.AppendFormat(" {0}=@{1},", columnName, propertyName);
                    GetParamFilterValid(pi, columnName);
                }
                else //主键
                {
                    if (attri == null)
                        IdColomnName = pi.Name;
                    else
                        IdColomnName = attri.Name;
                    IdProperty = pi;
                    thisIdGenerateType = idAttri.GenerateType;
                    if (thisIdGenerateType == IdGenerateType.Assign)
                    {
                        sbInsertColumn.AppendFormat(" {0},", columnName);
                        sbSetColumns.AppendFormat(" {0}=@{1},", columnName, propertyName);
                    }

                }
            }
        }

        protected virtual void GetParamFilterValid(PropertyInfo pi, string columnName)
        {

        }
        #endregion
    }
}
