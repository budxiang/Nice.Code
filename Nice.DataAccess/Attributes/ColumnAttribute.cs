using System;
using System.Data;

namespace Nice.DataAccess.Attributes
{
    /// <summary>
    /// 数据库表字段备注
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        private string name;
        /// <summary>
        /// 列名
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        private bool isReadOnly;
        /// <summary>
        /// 只读，此字段创建时可写入，不能更新
        /// </summary>
        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set { isReadOnly = value; }
        }
    }
}
