namespace DouYu
{
    /// <summary>
    /// 返回值类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResponseListData<T> : BaseResponse
    {

        public T[] Data { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class ResponseData<T> : BaseResponse
    {

        public T Data { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class BaseResponse
    {
        public int Error { get; set; }
    }

    public class ErrorResponse: BaseResponse
    {
        public string Data { get; set; }
    }
}

