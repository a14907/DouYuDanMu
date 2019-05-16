using DouYu.DanMu;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DouYu
{
    public partial class Form1 : Form
    {
        private IPAddress _serverIP;
        private int _port = 8601;
        private Socket _socket;
        public Form1()
        {
            InitializeComponent();
            var ips = Dns.GetHostAddresses("openbarrage.douyutv.com");
            _serverIP = ips[0];
        }

        private async void btnGetCate_ClickAsync(object sender, EventArgs e)
        {
            var cates = await DouYuApiHelper.GetAllGameCate();
            ShowData(cates);
        }

        private async void btnCateDetail_ClickAsync(object sender, EventArgs e)
        {
            TextBoxEnsureNum(tbCateId, "分类id必须是数字", out int cateId);
            var res = await DouYuApiHelper.GetRoomListByCateId(cateId, 0, 99);
            ShowData(res);
        }


        private async void btnRoomDetail_ClickAsync(object sender, EventArgs e)
        {
            TextBoxEnsureNum(tbRoomId, "房间id必须是数字", out int roomId);
            var res = await DouYuApiHelper.GetRoomDetail(roomId);
            ShowData(res);
        }

        public void ShowData<T>(ResponseListData<T> data)
        {
            if (data.Error != 0)
            {
                MessageBox.Show(data.ErrorMsg);
                return;
            }
            tblogs.Clear();
            foreach (var item in data.Data)
            {
                tblogs.AppendText(item.ToString());
            }
        }

        public void ShowData<T>(ResponseData<T> data)
        {
            if (data.Error != 0)
            {
                MessageBox.Show(data.ErrorMsg);
                return;
            }
            tblogs.Clear();
            tblogs.AppendText(data.Data.ToString());
        }

        public void TextBoxEnsureNum(TextBox tb, string errMsg, out int num)
        {
            var str = tb.Text.Trim();
            num = 0;
            if (!string.IsNullOrEmpty(str))
            {
                if (!int.TryParse(str, out num))
                {
                    MessageBox.Show(errMsg);
                    return;
                }
            }
        }

        private const int MAX_SIZE = 1024 * 1024;
        private byte[] _lenBuff = new byte[4];
        private byte[] _conttentBuff = new byte[MAX_SIZE];
        private bool isStop = false; 
        private void btnConnect_Click(object sender, EventArgs e)
        {
            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(_serverIP,_port);

            Task.Run((Action)DoWork);
            Task.Run((Func<Task>)HeartWork);
            isStop = false;
            btnConnect.Enabled = false;
            btnClose.Enabled = true;
        }

        private async Task HeartWork()
        {
            //   type@=keeplive/tick@=1439802131/
            BaseDanMu dm = new BaseDanMu()
            {
                Head = { MsgType = 689 },
                Body =
                {
                    { "type","keeplive"},
                    { "tick",DateTimeOffset.Now.ToUnixTimeSeconds()}
                }
            };
            var heartbuf = dm.Serialize();
            
            while (!isStop)
            {
                _socket.Send(heartbuf);
                tblogs.Clear();
                await Task.Delay(TimeSpan.FromSeconds(40));
            }
        }

        private void DoWork()
        {
            //登陆
            //type@=loginreq/roomid@=301712/
            BaseDanMu dm = new BaseDanMu()
            {
                Head = { MsgType = 689 },
                Body =
                {
                    { "type","loginreq"},
                    { "roomid",tbRoomId.Text.Trim()}
                }
            }; 

            var loginbuf = dm.Serialize();
            _socket.Send(loginbuf);
            //判断登陆成功
            ReceiveLenSize(_socket,4,_lenBuff);
            var receiveLen = BitConverter.ToInt32(_lenBuff, 0);
            ReceiveContentSize(_socket, receiveLen, _conttentBuff);
            var resstr=Encoding.UTF8.GetString(_conttentBuff,8,receiveLen-9);

            //加入组
            //type@=joingroup/rid@=59872/gid@=0/
            BaseDanMu dmJoin = new BaseDanMu()
            {
                Head = { MsgType = 689 },
                Body =
                {
                    { "type","joingroup"},
                    { "rid",tbRoomId.Text.Trim()},
                    { "gid",-9999},
                }
            };
            _socket.Send(dmJoin.Serialize());

            //接受弹幕消息
            while (!isStop)
            {
                //只抓取弹幕消息，其他消息不管
                ReceiveLenSize(_socket, 4, _lenBuff);
                receiveLen = BitConverter.ToInt32(_lenBuff, 0);
                ReceiveContentSize(_socket, receiveLen, _conttentBuff);
                resstr = Encoding.UTF8.GetString(_conttentBuff, 8, receiveLen - 9);
                if (resstr.Contains("type@=chatmsg"))
                {
                    //格式: type@=chatmsg/rid@=301712/gid@=-9999/uid@=123456/nn@=test/txt@=666/level@=1/ 不止这些
                    var tokens=resstr.Split(new[] { '/'},StringSplitOptions.RemoveEmptyEntries);
                    StringBuilder sb = new StringBuilder();
                    string name = "";
                    string level = "";
                    string txt = "";
                    foreach (var token in tokens)
                    {
                        //nn 
                        //发送者昵称
                        //txt
                        //弹幕文本内容 
                        //level
                        //用户等级
                        if (token.StartsWith("nn@=") )
                        {
                            name=token.Substring(4);
                        }
                        if ( token.StartsWith("txt@=")  )
                        {
                            txt=token.Substring(5);
                        }
                        if ( token.StartsWith("level@="))
                        {
                            level=token.Substring(7) ;
                        }
                    }
                    sb.Append($"{name}({level}):{txt}\r\n\r\n");
                    tblogs.Invoke((Action<StringBuilder>)(prop=> {
                        tblogs.AppendText(sb.ToString());
                    }),sb);
                }
            }
        }



        private void ReceiveLenSize(Socket socket, int size, byte[] buf)
        {
            var retlen = socket.Receive(buf, size, SocketFlags.None);
            int sum = retlen;
            while (sum != size)
            {
                sum += socket.Receive(buf, sum, size - sum, SocketFlags.None);
            }
        }
        private void ReceiveContentSize(Socket socket, int size, byte[] buf)
        {            
            var retlen = socket.Receive(buf, size, SocketFlags.None);
            int sum = retlen;
            while (sum != size)
            {
                sum += socket.Receive(buf, sum, size - sum, SocketFlags.None);
            }
        }

        private async void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                //  type@=logout/ 
                isStop = true;
                await Task.Delay(TimeSpan.FromSeconds(2));
                BaseDanMu dmJoin = new BaseDanMu()
                {
                    Head = { MsgType = 689 },
                    Body =
                {
                    { "type","logout"},
                }
                };
                _socket.Send(dmJoin.Serialize());
                _socket.Close();
                _socket.Dispose();
                btnConnect.Enabled = true;
                btnClose.Enabled = false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string msg = tbMsg.Text.Trim();
            // 
        }
    }
}
