using System.Collections.Generic;

namespace Nice.DataAccess.Model.Page
{
    public class PageTable<T>
    {
        public PageTable()
        {
            this._start = 0;
            this._length = 10;
            this._draw = 1;
        }

        private int _start;

        /// <summary>
        ///  开始索引
        /// </summary>
        public int start
        {
            get
            {
                return _start;
            }
            set
            {
                _start = value;
            }
        }
        private int _length;
        /// <summary>
        /// 页面大小
        /// </summary>
        public int length
        {
            get
            {
                return _length;
            }
            set
            {
                _length = value;
            }
        }

        public int _draw;
        /// <summary>
        /// 请求次数
        /// </summary>
        public int draw
        {
            get
            {
                return _draw;
            }
            set
            {
                _draw = value;
            }
        }

        private int _recordsTotal;
        /// <summary>
        /// 数据表中数据总和
        /// </summary>
        public int recordsTotal
        {
            get
            {
                return _recordsTotal;
            }
            set
            {
                _recordsTotal = value;
            }
        }
        public int _recordsFiltered;
        /// <summary>
        /// 显示的数据总和
        /// </summary>
        public int recordsFiltered
        {
            get
            {
                return _recordsFiltered;
            }
            set
            {
                _recordsFiltered = value;
            }
        }


        private DataTablesOrder[] _order;
        public DataTablesOrder[] order
        {
            get { return _order; }
            set { _order = value; }
        }

        private DataTablesColumn[] _columns;
        public DataTablesColumn[] columns
        {
            get { return _columns; }
            set { _columns = value; }
        }
        private DataTablesSearch _search;
        public DataTablesSearch search
        {
            get { return _search; }
            set { _search = value; }
        }

        /// <summary>
        /// 数据
        /// </summary>
        private IList<T> _data;
        public IList<T> data
        {
            get
            {
                if (_data == null)
                    _data = new List<T>();
                return _data;
            }
            set
            {
                _data = value;
            }
        }

        public string GetOrderColName()
        {
            int index = order[0].column;
            return columns[index].data;
        }

        public string GetOrderStr()
        {
            return order[0].dir;
        }

    }

    public sealed class DataTablesSearch
    {
        public string value { get; set; }

        public bool regex { get; set; }
    }

    public sealed class DataTablesOrder
    {
        public int column { get; set; }
        public string dir { get; set; }
    }

    public sealed class DataTablesColumn
    {
        public string data { get; set; }
        public string name { get; set; }
        public bool orderable { get; set; }
        public bool searchable { get; set; }
        public DataTablesSearch search { get; set; }
    }
}
