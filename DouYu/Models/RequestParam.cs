namespace DouYu
{
    /// <summary>
    /// 请求参数
    /// </summary>
    public class RequestParam
    {
        public int Offset { get; set; }
        private int _limit=30;

        public int Limit
        {
            get { return _limit; }
            set {
                if (value<1 || value>100) 
                    _limit = 30; 
                else
                    _limit = value;
            }
        }
    }
}

