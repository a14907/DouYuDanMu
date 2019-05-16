namespace DouYu
{
    /// <summary>
    /// 礼物
    /// </summary>
    public class Gift
    {
        /// <summary>
        /// 礼物 id
        /// </summary>
        public int Id     { get; set; }
        /// <summary>
        ///  礼物名称
        /// </summary>
        public string  Name   { get; set; }
        /// <summary>
        /// 礼物类型 1（鱼丸礼物）/ 2（鱼翅礼物） 
        /// </summary>
        public int Type   { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal Pc     { get; set; }
        /// <summary>
        /// 贡献值
        /// </summary>
        public int Gx     { get; set; }
        /// <summary>
        /// 礼物描述
        /// </summary>
        public string  Desc   { get; set; }
        /// <summary>
        /// 礼物介绍
        /// </summary>
        public string  Intro  { get; set; }
        /// <summary>
        ///  礼物图标静态图片地址
        /// </summary>
        public string  Mimg   { get; set; }
        /// <summary>
        /// 礼物图标动态图片地址
        /// </summary>
        public string Himg { get; set; }        
    }
}

