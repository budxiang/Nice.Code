namespace Nice.DataAccess.Model.Page
{
    /// <summary>
    /// 分页信息
    /// </summary>
    public class PageInfo
    {
        private int startIndex;
        private int pageSize;
        private string orderColName;
        private string orderStr;
        /// <summary>
        /// 查询起始索引
        /// </summary>
        public int StartIndex
        {
            get
            {
                return startIndex;
            }
            set
            {
                startIndex = value;
            }
        }
        /// <summary>
        /// 页面大小
        /// </summary>
        public int PageSize
        {
            get
            {
                return pageSize;
            }

            set
            {
                pageSize = value;
            }
        }

        private int totalCount;
        /// <summary>
        /// 排序列名
        /// </summary>
        public string OrderColName
        {
            get
            {
                return orderColName;
            }

            set
            {
                orderColName = value;
            }
        }
        /// <summary>
        ///排序方式 ASC或DESC
        /// </summary>
        public string OrderStr
        {
            get
            {
                return orderStr;
            }

            set
            {
                orderStr = value;
            }
        }
        /// <summary>
        /// 总条数
        /// </summary>
        public int TotalCount
        {
            get
            {
                return totalCount;
            }

            set
            {
                totalCount = value;
            }
        }

   

        private int pageIndex;
        /// <summary>
        /// 页面大小
        /// </summary>
        public int PageIndex
        {
            get
            {
                return pageIndex;
            }

            set
            {
                pageIndex = value;
                this.startIndex = this.pageIndex * this.pageSize;
            }
        }
        /// <summary>
        /// 通用分页参数结构 构造函数
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页面大小</param>
        public PageInfo(int startIndex, int pageSize)
        {
            this.startIndex = startIndex;
            this.pageSize = pageSize;
        }
        /// <summary>
        /// 通用分页参数结构 构造函数
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页面大小</param>
        public PageInfo(int startIndex, int pageSize, string OrderColName, string OrderStr)
        {
            this.startIndex = startIndex;
            this.pageSize = pageSize;
            this.orderColName = OrderColName;
            this.orderStr = OrderStr;
        }
    }
}
