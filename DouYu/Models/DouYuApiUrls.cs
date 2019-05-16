namespace DouYu
{
    /// <summary>
    /// api地址
    /// </summary>
    public class DouYuApiUrls
    {
        /// <summary>
        ///  获取直播房间列表信息，模板参数：分类ID或分类别名
        /// </summary>
        public const string GetRoomListUrl = "http://open.douyucdn.cn/api/RoomApi/live/{0}";

        /// <summary>
        ///   获取所有游戏分类
        /// </summary>
        public const string GetRoomCateUrl = "http://open.douyucdn.cn/api/RoomApi/game";

        /// <summary>
        ///  获取直播房间详情信息，模板参数：房间Id或者房间别名
        /// </summary>
        public const string GetRoomDetail = "http://open.douyucdn.cn/api/RoomApi/room/{0}";
    }
}

