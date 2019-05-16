namespace DouYu
{
    public class GameCateItem
    {
        /// <summary>
        /// 游戏分类 ID
        /// </summary>
        public int Cate_id    { get; set; }
        /// <summary>
        /// 游戏分类的名称
        /// </summary>
        public string Game_name  { get; set; }
        /// <summary>
        /// 游戏分类的别名
        /// </summary>
        public string Short_name { get; set; }
        /// <summary>
        /// 游戏分类的网址
        /// </summary>
        public string Game_url   { get; set; }
        /// <summary>
        /// 游戏分类的封面图片，大小 140*195 
        /// </summary>
        public string Game_src   { get; set; }
        /// <summary>
        /// 游戏分类的小图标图片，大小 16*16 
        /// </summary>
        public string Game_icon { get; set; }

        public override string ToString()
        {
            return $"{Cate_id}\t{Game_name}\r\n";
        }
    }
}

