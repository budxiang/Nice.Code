using System;

namespace Nice.Network.Web.Attributes
{
    /// <summary>
    /// Controller注解
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ControllerAttribute : Attribute
    {
        public ControllerAttribute()
        {

        }
        public ControllerAttribute(string route)
        {
            this.route = route;
        }
        private string route;
        /// <summary>
        /// 路由路径
        /// </summary>
        public string Route
        {
            get
            {
                return route;
            }
            set
            {
                route = value;
            }
        }
    }
}
