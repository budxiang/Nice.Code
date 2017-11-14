namespace Nice.Network.Web
{
    public class HttpReplyResult
    {
        private bool _result;
        private string _describe;
        private object _data;
        private bool _redirect;
        private string _redirectURL;

        public bool result
        {
            get
            {
                return _result;
            }

            set
            {
                _result = value;
            }
        }

        public object data
        {
            get
            {
                return _data;
            }

            set
            {
                _data = value;
            }
        }

        public string describe
        {
            get
            {
                return _describe;
            }

            set
            {
                _describe = value;
            }
        }
        public bool redirect
        {
            get { return _redirect; }
            set { _redirect = value; }
        }
        public string redirectURL
        {
            get { return _redirectURL; }
            set { _redirectURL = value; }
        }
    }
}
