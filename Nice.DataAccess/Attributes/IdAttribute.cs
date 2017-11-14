using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nice.DataAccess.Attributes
{
    /// <summary>
    /// 主键备注
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IdAttribute : Attribute
    {
        private IdGenerateType generateType;

        public IdGenerateType GenerateType
        {
            get
            {
                return generateType;
            }

            set
            {
                generateType = value;
            }
        }
    }

    public enum IdGenerateType
    {
        /// <summary>
        /// 数据库本身维护的主键
        /// </summary>
        Native = 1,
        /// <summary>
        /// 自增
        /// </summary>
        Increment = 2,
        /// <summary>
        /// 程序维护的主键
        /// </summary>
        Assign = 3
    }
}
