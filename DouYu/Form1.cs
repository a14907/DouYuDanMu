using DouYu.DanMu;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DouYu
{
    public partial class Form1 : Form
    {
        private ClientWebSocket _socket;
        public Form1()
        {
            InitializeComponent();
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
            StringBuilder sb = new StringBuilder();
            foreach (var item in data.Data)
            {
                sb.Append(item.ToString());
            }
            tblogs.AppendText(sb.ToString());
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

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            _socket = new ClientWebSocket();
            await _socket.ConnectAsync(new Uri("wss://danmuproxy.douyu.com:8506/"), CancellationToken.None);

            _ = Task.Run((Action)DoWork);
            _ = Task.Run((Func<Task>)HeartWork);
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
                await Task.Delay(TimeSpan.FromSeconds(40));
                await _socket.SendAsync(new ArraySegment<byte>(heartbuf), WebSocketMessageType.Binary, false, CancellationToken.None);
                if (tblogs.Text.Length > 100000)
                {
                    tblogs.Clear();
                }                
            }
        }

        private async void DoWork()
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
            await _socket.SendAsync(new ArraySegment<byte>(loginbuf), WebSocketMessageType.Binary, false, CancellationToken.None);
            //判断登陆成功
            await ReceiveLenSizeAsync(_socket, 4, _lenBuff);
            var receiveLen = BitConverter.ToInt32(_lenBuff, 0);
            await ReceiveContentSize(_socket, receiveLen, _conttentBuff);
            var resstr = Encoding.UTF8.GetString(_conttentBuff, 8, receiveLen - 9);

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
            await _socket.SendAsync(new ArraySegment<byte>(dmJoin.Serialize()), WebSocketMessageType.Binary, false, CancellationToken.None);

            //接受弹幕消息
            while (!isStop)
            {
                try
                {
                    //只抓取弹幕消息，其他消息不管
                    await ReceiveLenSizeAsync(_socket, 4, _lenBuff);
                    receiveLen = BitConverter.ToInt32(_lenBuff, 0);
                    await ReceiveContentSize(_socket, receiveLen, _conttentBuff);
                    resstr = Encoding.UTF8.GetString(_conttentBuff, 8, receiveLen - 9);
                    if (resstr.Contains("type@=chatmsg"))
                    {
                        //格式: type@=chatmsg/rid@=301712/gid@=-9999/uid@=123456/nn@=test/txt@=666/level@=1/ 不止这些
                        var tokens = resstr.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
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
                            if (token.StartsWith("nn@="))
                            {
                                name = token.Substring(4);
                            }
                            if (token.StartsWith("txt@="))
                            {
                                txt = token.Substring(5);
                            }
                            if (token.StartsWith("level@="))
                            {
                                level = token.Substring(7);
                            }
                        }
                        sb.Append($"{name}({level}):{txt}\r\n\r\n");
                        tblogs.Invoke((Action<StringBuilder>)(prop =>
                        {
                            tblogs.AppendText(sb.ToString());
                        }), sb);
                    }
                }
                catch (Exception ex)
                {
                    if (_socket.State == WebSocketState.Open)
                    {
                        MessageBox.Show(ex.Message + ex.StackTrace);
                    }
                }
            }
        }



        private async Task ReceiveLenSizeAsync(WebSocket socket, int size, byte[] buf)
        {
            var retlen = await socket.ReceiveAsync(new ArraySegment<byte>(buf, 0, size), CancellationToken.None);
            int sum = retlen.Count;
            while (sum != size)
            {
                sum += (await socket.ReceiveAsync(new ArraySegment<byte>(buf, sum, size - sum), CancellationToken.None)).Count;
            }
        }
        private async Task ReceiveContentSize(WebSocket socket, int size, byte[] buf)
        {
            var retlen = await socket.ReceiveAsync(new ArraySegment<byte>(buf, 0, size), CancellationToken.None);
            int sum = retlen.Count;
            while (sum != size)
            {
                sum += (await socket.ReceiveAsync(new ArraySegment<byte>(buf, sum, size - sum), CancellationToken.None)).Count;
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
                await _socket.SendAsync(new ArraySegment<byte>(dmJoin.Serialize()), WebSocketMessageType.Binary, true, CancellationToken.None);
                await _socket.CloseOutputAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None);
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
