namespace Nice.Network.Base.Protocol
{
    public class Header
    {
        private bool start;

        public bool Start
        {
            get { return start; }
            set { start = value; }
        }
        private bool end;
        public bool End
        {
            get { return end; }
            set { end = value; }
        }    

        private SocketCommand cmd;

        public SocketCommand Cmd
        {
            get { return cmd; }
            set { cmd = value; }
        }
        private int dataLength;

        public int DataLength
        {
            get { return dataLength; }
            set { dataLength = value; }
        }

        private int transferred;

        public int Transferred
        {
            get { return transferred; }
            set { transferred = value; }
        }
    }
}
