namespace DouYu
{
    /// <summary>
    /// 房间列表项
    /// </summary>
    public class RoomListInfoItem
    {
        /// <summary>
        /// 房间 ID
        /// </summary>
        public int Room_id  {get;set;}
        /// <summary>
        /// 房间图片，大小 320*180  
        /// </summary>
        public string Room_src {get;set;}
        /// <summary>
        /// 房间名称
        /// </summary>
        public string Room_name{get;set;}
        /// <summary>
        /// 房间所属用户的 UID
        /// </summary>
        public int Owner_uid {get;set;}
        /// <summary>
        /// 原人气字段， 现在与热度值同步 （后续可能会依据情况废弃该字段） 
        /// </summary>
        public int Online   {get;set;}
        /// <summary>
        /// 在线热度值
        /// </summary>
        public int Hn       {get;set;}
        /// <summary>
        /// 房间所属用户的账号
        /// </summary>
        public string Nickname {get;set;}
        /// <summary>
        /// 房间的网址
        /// </summary>
        public string Url { get; set; }

        public override string ToString()
        {
            return $"{Room_id}\t{Room_name}\t{Hn}\r\n";
        }
    }
}

