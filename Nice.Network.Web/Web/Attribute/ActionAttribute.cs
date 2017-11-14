using System;

namespace Nice.Network.Web.Attributes
{

    [AttributeUsage(AttributeTargets.Method)]
    public class ActionAttribute : Attribute
    {
        public ActionAttribute(MimeType mimeType)
        {
            this.mimeType = mimeType;
        }

        public ActionAttribute(string name, string contentType)
        {
            this.Name = name;
        }
        private string name;
        /// <summary>
        /// Action标识
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

        private MimeType mimeType;
        public MimeType MimeType
        {
            get
            {
                return mimeType;
            }
            set
            {
                mimeType = value;
            }
        }
    }
}
