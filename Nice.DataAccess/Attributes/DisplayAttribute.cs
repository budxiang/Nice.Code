using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nice.DataAccess.Attributes.Entity
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DisplayAttribute : Attribute
    {
        private string text;

        public string Text
        {
            get { return text; }
            set { text = value; }
        }
        public DisplayAttribute(string text)
        {
            this.text = text;
        }
    }
}
