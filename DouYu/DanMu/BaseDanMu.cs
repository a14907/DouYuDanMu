using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DouYu.DanMu
{
    public class BaseDanMu
    {
        /// <summary>
        /// 报文长度
        /// </summary>
        public int Length { get; set; }
        /// <summary>
        /// 报文头
        /// </summary>
        public Head Head { get; set; } = new Head();
        /// <summary>
        /// 数据部分，结尾必须是'\0'
        /// </summary>
        public Dictionary<string, object> Body { get; set; } = new Dictionary<string, object>();

        

        public byte[] Serialize()
        {
            using (var ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                StringBuilder sb = new StringBuilder();
                FillData(sb,Body);
                var str = sb.ToString();
                var bodybuf = Encoding.UTF8.GetBytes(str);
                var totalLen = 4 + 4 + 1 + bodybuf.Length;
                this.Head.Length = totalLen;
                bw.Write(totalLen);
                bw.Write(this.Head.Length);
                bw.Write(this.Head.MsgType);
                bw.Write(this.Head.EncField);
                bw.Write(this.Head.RemaindField);
                bw.Write(bodybuf);
                bw.Write('\0');
                return ms.ToArray();
            }

        }

        private void FillData(StringBuilder sb,Dictionary<string,object> body)
        {
            foreach (var item in body)
            {
                sb.Append(item.Key.Encode())
                    .Append("@=");
                var newsb = new StringBuilder();
                FillData(newsb, item.Value);
                sb.Append(newsb.ToString().Encode());


                sb.Append("/");
            }
        }

        public void FillData(StringBuilder sb, object val)
        {
            if (val is string)
            {
                sb.Append((val as string).Encode());
            }
            else if(val is Dictionary<string,object>)
            {
                FillData(sb, val as Dictionary<string, object>);
            }else if (val is List<object>)
            {
                FillData(sb,val as List<object>);
            }
            else
            {
                sb.Append(val.ToString());
            }
        }

        public void FillData(StringBuilder sb, List<object> val)
        {
            foreach (var item in val)
            {
                var newsb = new StringBuilder();
                FillData(newsb, item);
                sb.Append(newsb.ToString().Encode());

                sb.Append("/");
            }
        }
    }

    public class Head
    {
        /// <summary>
        /// 保温长度
        /// </summary>
        public int Length { get; set; }
        /// <summary>
        /// 消息类型
        /// 689 客户端发送给弹幕服务器的文本格式数据
        /// 690 弹幕服务器发送给客户端的文本格式数据
        /// </summary>
        public short MsgType { get; set; }
        /// <summary>
        /// 加密字段
        /// </summary>
        public byte EncField { get; set; }
        /// <summary>
        /// 保留字段
        /// </summary>
        public byte RemaindField { get; set; }
    }
}
