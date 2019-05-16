namespace DouYu.DanMu
{
    public static class StringExt
    {
        public static string Encode(this string str,bool isneed=true)
        {
            if (isneed==false)
            {
                return str;
            }
            return str.Replace("@", "@A").Replace("/", "@S");
        }
    }
}
