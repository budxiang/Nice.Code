using System;

namespace Nice.DataAccess.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ValidAttribute : Attribute
    {
        private object available;

        public ValidAttribute(object available, object invalid)
        {
            this.available = available;
            this.invalid = invalid;
        }
        public object Available
        {
            get
            {
                return available;
            }

            set
            {
                this.available = value;
            }
        }
        private object invalid;
        public object Invalid
        {
            get
            {
                return invalid;
            }

            set
            {
                this.invalid = value;
            }
        }

    }
}
