using System;

namespace DouYu
{
    /// <summary>
    /// 房间详情
    /// </summary>
    public class RoomDetail
    {
        /// <summary>
        ///  房间 ID
        /// </summary>
        public int Room_id { get; set; }
        /// <summary>
        /// 房间图片，大小 320*180  
        /// </summary>
        public string Room_thumb { get; set; }
        /// <summary>
        /// 房间所属分类 ID
        /// </summary>
        public int Cate_id { get; set; }
        /// <summary>
        /// 房间所属分类名称
        /// </summary>
        public string Cate_name { get; set; }
        /// <summary>
        /// 房间名称
        /// </summary>
        public string Room_name { get; set; }
        /// <summary>
        /// 房间开播状态： 1（表示开播） 2（表示关播） 
        /// </summary>
        public int Room_status { get; set; }
        /// <summary>
        /// 最近开播时间
        /// </summary>
        public DateTimeOffset Start_time { get; set; }
        /// <summary>
        /// 房间所属主播昵称
        /// </summary>
        public string Owner_name { get; set; }
        /// <summary>
        /// 房间所属主播头像地址
        /// </summary>
        public string Avatar { get; set; }
        /// <summary>
        /// 原人气字段，现在与热度值同步（后续可能会依据情况废弃该字
        /// </summary>
        public int Online { get; set; }
        /// <summary>
        /// 在线热度值
        /// </summary>
        public int Hn { get; set; }
        /// <summary>
        /// 直播间主播体重
        /// </summary>
        public int Owner_weight { get; set; }
        /// <summary>
        /// 直播间关注数
        /// </summary>
        public int Fans_num { get; set; }
        /// <summary>
        /// 直播间礼物信息数组列表
        /// </summary>
        public Gift[] Gift { get; set; }
        public override string ToString()
        {
            return $"{Cate_name}    {Room_name}    {(Room_status == 1 ? "开播" : "关播")}    {Start_time}    {Owner_name}    {"热度" + Hn}    {"粉丝数" + Fans_num}\r\n";
        }
    }
}

