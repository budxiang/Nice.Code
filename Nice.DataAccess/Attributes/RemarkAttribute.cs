using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nice.DataAccess.Attributes.Entity
{

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class RemarkAttribute : Attribute
    {
        public RemarkAttribute(string remark)
        {
            this.text = remark;
        }
        private string text;
        /// <summary>
        /// 显示
        /// </summary>
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                this.text = value;
            }
        }
    }
}
