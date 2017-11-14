using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nice.DataAccess.Attributes
{
    /// <summary>
    /// 暂存、附加属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class TransientAttribute : Attribute
    {

    }
}
