using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.ComponentModel;
using System.Collections;


/// <summary>
/// 改编自Richard同学的大力支持, powered by longtombbj(斗鱼直播间ID 315092)
/// 帖子地址 http://dev-bbs.douyutv.com/forum.php?mod=viewthread&tid=115&extra=page%3D1
/// 斗鱼开放平台技术交流QQ群：478182780
/// </summary>

namespace DYBullet
{
    public class DyBulletHelper
    {
        //设置需要访问的房间ID信息
        private int _roomId;
        //弹幕池分组号，海量模式使用-9999
        private int _groupId = -9999;

        public event EventHandler<DyBulletShoutArgs> DyBulletShout;
        public class DyBulletShoutArgs : EventArgs
        {
            public Bullet bullet;
            public DyBulletShoutArgs(Bullet bullet)
            {
                this.bullet = bullet;
            }
        }

        public event EventHandler<DyBulletHelperEventArisenArgs> DyBulletHelperEventArisen;
        public class DyBulletHelperEventArisenArgs : EventArgs
        {
            public string msg;
            public DyBulletHelperEventArisenArgs(string msg)
            {
                this.msg = msg;
            }
        }
        public event EventHandler<DyBulletHelperErrorArgs> DyBulletHelperError;
        public class DyBulletHelperErrorArgs : EventArgs
        {
            public Exception err;
            public DyBulletHelperErrorArgs(Exception err)
            {
                this.err = err;
            }
        }

        public class NamedBackgroundWorker : BackgroundWorker
        {
            private string name;
            public string Name
            {
                get { return name; }
            }
            public NamedBackgroundWorker(string myName)
            {
                name = myName;
            }
        }
        public static string BGW_NAME_KEEPALIVE = "bgWorkerKeepAlive";
        public static string BGW_NAME_KEEPGETMSG = "bgWorkerKeepGetMsg";
        private NamedBackgroundWorker bgWorkerKeepAlive;
        private NamedBackgroundWorker bgWorkerKeepGetMsg;

        public enum iState
        {
            Waiting,
            Starting,
            Stoped,
            Ready,
            Disposed,
        }
        private iState _State = iState.Waiting;
        public iState State
        {
            get
            {
                return _State;
            }
        }
        public class orkingStateChangedArgs : EventArgs
        {
            private iState curState;
            public iState CurState
            {
                get { return curState; }
            }
            public orkingStateChangedArgs(iState curState)
            {
                this.curState = curState;
            }
        }
        public event EventHandler<orkingStateChangedArgs> WorkingStateChanged;
        public DyBulletHelper(int roomId)
        {
            Init(roomId, -9999);
        }
        public DyBulletHelper(int roomId, int groupId)
        {
            Init(roomId, groupId);
        }
        private DyBulletScreenClient danmuClient;
        private void Init(int roomId, int groupId)
        {
            _State = iState.Starting;
            if (WorkingStateChanged != null)
            {
                WorkingStateChanged(this, new orkingStateChangedArgs(_State));
            }
            bgWorkerKeepAlive = new NamedBackgroundWorker(BGW_NAME_KEEPALIVE);
            bgWorkerKeepGetMsg = new NamedBackgroundWorker(BGW_NAME_KEEPGETMSG);

            _roomId = roomId;
            _groupId = groupId;

            bgWorkerKeepAlive.WorkerReportsProgress = true;
            bgWorkerKeepAlive.WorkerSupportsCancellation = true;
            bgWorkerKeepAlive.ProgressChanged += BgWorker_ProgressChanged;
            bgWorkerKeepAlive.DoWork += BgWorkerKeepAlive_DoWork;

            bgWorkerKeepGetMsg.WorkerReportsProgress = true;
            bgWorkerKeepGetMsg.WorkerSupportsCancellation = true;
            bgWorkerKeepGetMsg.ProgressChanged += BgWorker_ProgressChanged;
            bgWorkerKeepGetMsg.DoWork += BgWorkerKeepGetMsg_DoWork;


            //初始化弹幕Client
            danmuClient = DyBulletScreenClient.getInstance(bgWorkerKeepAlive, true);
            //设置需要连接和访问的房间ID，以及弹幕池分组号
            if (danmuClient.init(roomId, groupId) == false)
            {
                _State = iState.Stoped;
                if (WorkingStateChanged != null)
                {
                    WorkingStateChanged(this, new orkingStateChangedArgs(_State));
                }
                return;
            }

            threadKeepAlive = new KeepAlive(bgWorkerKeepAlive);
            threadKeepGetMsg = new KeepGetMsg(bgWorkerKeepGetMsg);
            //保持弹幕服务器心跳
            bgWorkerKeepAlive.RunWorkerAsync();

            //获取弹幕服务器发送的所有信息
            //KeepGetMsg keepGetMsg = new KeepGetMsg();
            //Thread keepGetMsgThread = new Thread(new ThreadStart(keepGetMsg.run));
            //keepGetMsgThread.Start();
            bgWorkerKeepGetMsg.RunWorkerAsync();


            _State = iState.Ready;
            if (WorkingStateChanged != null)
            {
                WorkingStateChanged(this, new orkingStateChangedArgs(_State));
            }
        }

        public void Dispose()
        {
            //threadKeepAlive.Stop();
            bgWorkerKeepAlive.CancelAsync();
            bgWorkerKeepGetMsg.CancelAsync();
            bgWorkerKeepAlive.Dispose();
            bgWorkerKeepGetMsg.Dispose();
            bgWorkerKeepAlive = null;
            bgWorkerKeepGetMsg = null;
            danmuClient.Dispose();
            //threadKeepAlive
        }

        private KeepAlive threadKeepAlive;
        private KeepGetMsg threadKeepGetMsg;
        private void BgWorkerKeepAlive_DoWork(object sender, DoWorkEventArgs e)
        {
            threadKeepAlive.run();
        }
        private void BgWorkerKeepGetMsg_DoWork(object sender, DoWorkEventArgs e)
        {
            threadKeepGetMsg.run();
        }

        private void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // check e.ProgressPercentage, 0 for debug info, 1 for msg, 2 for Error
            switch (e.ProgressPercentage)
            {
                case 0:
                    if (DyBulletHelperEventArisen != null)
                    {
                        DyBulletHelperEventArisen(this, new DyBulletHelperEventArisenArgs((string)e.UserState));
                    }
                    break;
                case 1:
                    if (DyBulletShout != null)
                    {
                        DyBulletShout(this, new DyBulletShoutArgs((Bullet)e.UserState));
                    }
                    break;
                case 2: // error!
                    if (DyBulletHelperError != null)
                    {
                        //bgWorkerKeepAlive.CancelAsync();
                        //bgWorkerKeepGetMsg.CancelAsync();
                        DyBulletHelperError(this, new DyBulletHelperErrorArgs((Exception)e.UserState));
                    }
                    _State = iState.Stoped;
                    if (WorkingStateChanged != null)
                    {
                        WorkingStateChanged(this, new orkingStateChangedArgs(_State));
                    }
                    break;
                default:
                    break;
            }
        }
        public void Test_Stop()
        {
            bgWorkerKeepAlive.CancelAsync();
            bgWorkerKeepGetMsg.CancelAsync();

            _State = iState.Stoped;
            if (WorkingStateChanged != null)
            {
                WorkingStateChanged(this, new orkingStateChangedArgs(_State));
            }
        }

        //弹幕客户端类
        public class DyBulletScreenClient
        {
            public Dictionary<string, NamedBackgroundWorker> BGWorkers
                = new Dictionary<string, NamedBackgroundWorker>();
            //Logger logger = Logger.getLogger(DyBulletScreenClient.class);
            private static DyBulletScreenClient instance;

            //第三方弹幕协议服务器地址
            private static string hostName = "openbarrage.douyutv.com";

            //第三方弹幕协议服务器端口
            private static int port = 8601;

            //设置字节获取buffer的最大值
            private static int MAX_BUFFER_LENGTH = 4096;

            //socket相关配置
            private Socket sock;
            //private BufferedOutputStream bos;
            //private BufferedInputStream bis;
            private NetworkStream netStream;

            //获取弹幕线程及心跳线程运行和停止标记
            private bool readyFlag = false;

            private DyBulletScreenClient() { }

            /**
             * 单例获取方法，客户端单例模式访问
             * @return
             */
            private static NamedBackgroundWorker bgWorker_conn;
            public static DyBulletScreenClient getInstance(NamedBackgroundWorker bgWorker, bool isConnReporter)
            {
                if (null == instance)
                {
                    instance = new DyBulletScreenClient();
                    instance.BGWorkers.Add(bgWorker.Name, bgWorker);
                    if (isConnReporter)
                    {
                        bgWorker_conn = bgWorker;
                    }
                }
                return instance;
            }

            /**
             * 客户端初始化，连接弹幕服务器并登陆房间及弹幕池
             * @param roomId 房间ID
             * @param groupId 弹幕池分组ID
             */
            public bool init(int roomId, int groupId)
            {
                //连接弹幕服务器
                if (this.connectServer() == false)
                {
                    return false;
                }
                //登陆指定房间
                if (this.loginRoom(roomId) == false)
                {
                    return false;
                }
                //加入指定的弹幕池
                this.joinGroup(roomId, groupId);
                //设置客户端就绪标记为就绪状态
                readyFlag = true;
                return true;
            }
            public void Dispose()
            {
                BGWorkers.Clear();
                sock.Close();
                instance = null;
            }

            /**
             * 获取弹幕客户端就绪标记
             * @return
             */
            public bool getReadyFlag()
            {
                return readyFlag;
            }

            /**
             * 连接弹幕服务器
             */
            private bool connectServer()
            {
                try
                {
                    //获取弹幕服务器访问host
                    //string host = InetAddress.getByName(hostName).getHostAddress();
                    //string host = Dns.GetHostAddresses(hostName)[0].MapToIPv4().ToString();
                    //建立socke连接
                    //sock = new Socket(host, port);
                    IPEndPoint ipe = new IPEndPoint(Dns.GetHostAddresses(hostName)[0].MapToIPv4(), port);
                    sock = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    sock.Connect(ipe);
                    netStream = new NetworkStream(sock);



                    if (sock.Connected == false)
                    {
                        throw new Exception(ipe.ToString() + "\r\nNot Connected!");
                        //return false;
                    }

                    //设置socket输入及输出
                    //bos = new BufferedOutputStream(sock.getOutputStream());
                    //bis = new BufferedInputStream(sock.getInputStream());
                }
                catch (Exception e)
                {
                    throw e;
                }

                //logger.debug("Server Connect Successfully!");
                bgWorker_conn.ReportProgress(0, "Server Connect Successfully!");
                return true;
            }

            /**
             * 登录指定房间
             * @param roomId
             */
            private bool loginRoom(int roomId)
            {
                //获取弹幕服务器登陆请求数据包
                byte[] loginRequestData = DyMessage.getLoginRequestData(roomId);


                try
                {
                    //发送登陆请求数据包给弹幕服务器
                    //bos.write(loginRequestData, 0, loginRequestData.Length);
                    //bos.flush();
                    netStream.Write(loginRequestData, 0, loginRequestData.Length);
                    netStream.Flush();

                    //string tmp = "";
                    //foreach (byte b in loginRequestData)
                    //{
                    //    tmp += b + ", ";
                    //}

                    //初始化弹幕服务器返回值读取包大小
                    byte[] recvByte = new byte[MAX_BUFFER_LENGTH];

                    //获取弹幕服务器返回值
                    netStream.Read(recvByte, 0, recvByte.Length);

                    //解析服务器返回的登录信息
                    if (DyMessage.parseLoginRespond(recvByte))
                    {
                        bgWorker_conn.ReportProgress(0, "Receive login response successfully!");
                        return true;
                    }
                    else
                    {
                        bgWorker_conn.ReportProgress(0, "Receive login response failed!");
                    }
                }
                catch (Exception e)
                {
                    //e.printStackTrace();
                    throw e;
                }
                return false;
            }

            /**
             * 加入弹幕分组池
             * @param roomId
             * @param groupId
             */
            private void joinGroup(int roomId, int groupId)
            {
                //获取弹幕服务器加弹幕池请求数据包
                byte[] joinGroupRequest = DyMessage.getJoinGroupRequest(roomId, groupId);

                try
                {
                    //想弹幕服务器发送加入弹幕池请求数据
                    //bos.write(joinGroupRequest, 0, joinGroupRequest.Length);
                    //bos.flush();
                    netStream.Write(joinGroupRequest, 0, joinGroupRequest.Length);
                    netStream.Flush();
                    //logger.debug("Send join group request successfully!");
                    bgWorker_conn.ReportProgress(0, "Send join group request successfully!");

                }
                catch (Exception)
                {
                    //e.printStackTrace();
                    //throw e;
                    //logger.error("Send join group request failed!");
                    bgWorker_conn.ReportProgress(0, "Send join group request failed!");
                }
            }

            /**
             * 服务器心跳连接
             */
            public void keepAlive(NamedBackgroundWorker bgWorker)
            {
                //获取与弹幕服务器保持心跳的请求数据包
                //byte[] keepAliveRequest = DyMessage.getKeepAliveData((int)(System.currentTimeMillis() / 1000));
                byte[] keepAliveRequest = DyMessage.getKeepAliveData((int)(DateTime.Now.Ticks / 10000000));

                try
                {
                    //向弹幕服务器发送心跳请求数据包
                    //bos.write(keepAliveRequest, 0, keepAliveRequest.Length);
                    //bos.flush();
                    netStream.Write(keepAliveRequest, 0, keepAliveRequest.Length);
                    netStream.Flush();
                    //logger.debug("Send keep alive request successfully!");
                    bgWorker.ReportProgress(0, "Send keep alive request successfully!");

                }
                catch (Exception)
                {
                    //e.printStackTrace();
                    //throw e;
                    //logger.error("Send keep alive request failed!");
                    bgWorker.ReportProgress(0, "Send keep alive request failed!");
                }
            }

            /**
             * 获取服务器返回信息
             */
            public void getServerMsg(NamedBackgroundWorker bgWorker, out bool errArise)
            {
                //初始化获取弹幕服务器返回信息包大小
                byte[] recvByte = new byte[MAX_BUFFER_LENGTH];
                //定义服务器返回信息的字符串
                String dataStr;
                errArise = false;

                //读取服务器返回信息，并获取返回信息的整体字节长度
                //int recvLen = bis.read(recvByte, 0, recvByte.Length);
                int recvLen = 0;
                try
                {
                    recvLen = netStream.Read(recvByte, 0, recvByte.Length);
                }
                catch (Exception err)
                {
                    errArise = true;
                    bgWorker.ReportProgress(2, err);
                    return;
                }
                if (recvLen < 12)
                {
                    return;
                }

                //根据实际获取的字节数初始化返回信息内容长度
                byte[] realBuf = new byte[recvLen];
                //按照实际获取的字节长度读取返回信息
                //System.arraycopy(recvByte, 0, realBuf, 0, recvLen);
                Array.Copy(recvByte, realBuf, recvLen);
                //根据TCP协议获取返回信息中的字符串信息
                //dataStr = new String(realBuf, 12, realBuf.Length - 12);
                dataStr = Encoding.UTF8.GetString(realBuf).Substring(12);

                //循环处理socekt黏包情况
                MsgView msgView;
                while (dataStr.LastIndexOf("type@=") > 5)
                {
                    //对黏包中最后一个数据包进行解析
                    //MsgView msgView = new MsgView(StringUtils.substring(dataStr, dataStr.LastIndexOf("type@=")));
                    msgView = new MsgView(dataStr.Substring(dataStr.LastIndexOf("type@=")));
                    //分析该包的数据类型，以及根据需要进行业务操作
                    parseServerMsg(msgView.getMessageList(), bgWorker);
                    //处理黏包中的剩余部分
                    //dataStr = StringUtils.substring(dataStr, 0, dataStr.LastIndexOf("type@=") - 12);
                    //dataStr = dataStr.Substring(0, dataStr.LastIndexOf("type@=") - 12);

                    dataStr = dataStr.Substring(0, dataStr.LastIndexOf("type@="));
                }

                //对单一数据包进行解析
                if (dataStr.Contains("type@="))
                {
                    //msgView = new MsgView(StringUtils.substring(dataStr, dataStr.LastIndexOf("type@=")));
                    int lstIdx = dataStr.LastIndexOf("type@=");
                    if (lstIdx == 0)
                    {
                        msgView = new MsgView(dataStr);
                    }
                    else
                    {
                        msgView = new MsgView(dataStr.Substring(lstIdx));
                    }
                    //分析该包的数据类型，以及根据需要进行业务操作
                    parseServerMsg(msgView.getMessageList(), bgWorker);
                }
            }

            /**
             * 解析从服务器接受的协议，并根据需要订制业务需求
             * @param msg
             */
            private void parseServerMsg(Dictionary<string, object> msg, NamedBackgroundWorker bgWorker)
            {
                //if (msg.get("type") != null)
                string msg_type = (string)msg["type"];
                if (msg_type != null)
                {

                    //服务器反馈错误信息
                    if (msg_type == "error")
                    {
                        //logger.debug(msg.toString());
                        bgWorker.ReportProgress(0, msg.ToString());
                        //结束心跳和获取弹幕线程
                        this.readyFlag = false;
                    }

                    /***@TODO 根据业务需求来处理获取到的所有弹幕及礼物信息***********/
                    Bullet bullet = new Bullet(msg);
                    bgWorker.ReportProgress(1, bullet);


                    ////判断消息类型
                    //if (msg_type == "chatmsg")
                    //{//弹幕消息
                    // //logger.debug("弹幕消息===>" + msg.toString());
                    //    bgWorker.ReportProgress(0, "弹幕消息===>" + bullet.MsgToString());
                    //}
                    //else if (msg_type == "dgb")
                    //{//赠送礼物信息
                    // //logger.debug("礼物消息===>" + msg.toString());
                    //    bgWorker.ReportProgress(0, "礼物消息===>" + bullet.MsgToString());
                    //}
                    //else {
                    //    //logger.debug("其他消息===>" + msg.toString());
                    //    bgWorker.ReportProgress(0, "其他消息===>" + bullet.MsgToString());
                    //}

                    //@TODO 其他业务信息根据需要进行添加

                    /*************************************************************/
                }
            }

        }



        /// <summary>
        /// 基于 《斗鱼弹幕服务器第三方接入协议v1.4.1.pdf》， 2016 0818
        /// by longtombbj
        /// </summary>
        public class Bullet
        {
            public class iStructedMsg
            {
                public enum iType
                {
                    Login, // loginres  登入
                    Logout, // logout  登出
                    HeartBeat, // keeplive  心跳，保证连接持续
                    Chat, // chatmsg  聊天消息
                    GiftPickedUp, // onlinegift  用户领取鱼丸（暴击）
                    GiftDonated, // dgb  用户赠送礼物
                    AudienceSeatTaken, // uenter  用户进入
                    DeserveBought, // bc_buy_deserve  用户赠送酬勤
                    CameraOnOff, // rss  开播提醒
                    RankList, // ranklist 排行榜
                    SuperMsg, // ssd 超级弹幕，  用于群发的广告？？？？？？
                    Defiance, // spbc   房间内礼物， 用于 两个观众之间送礼 ？？？？？？
                    RedPacket, // ggbb   红包， 谁发的？ 谁抢的？
                    RankUp, // rankup  用户在前十排行中发生变化（只发布提升变化）
                    Error, // error 报错  code  0-ok  51-传输错误  52服务器关闭  204房间id错误  其他---其他错误
                    Ohter,
                }
                private iType _Type;
                public iType Type
                {
                    get
                    {
                        return _Type;
                    }
                }


                #region All Structure & Init
                public class iLogin
                {
                    public string type;
                    public string userid;
                    public string roomgroup;
                    public string pg;
                    public string sessionid;
                    public string username;
                    public string nickname;
                    public string is_signed;
                    public string signed_count;
                    public string live_stat;
                    public string npv;
                    public string best_dlev;
                    public string cur_lev;

                    public iLogin(Dictionary<string, object> message)
                    {
                        object tmp;
                        message.TryGetValue("type", out tmp); type = (string)tmp;
                        message.TryGetValue("userid", out tmp); userid = (string)tmp;
                        message.TryGetValue("roomgroup", out tmp); roomgroup = (string)tmp;
                        message.TryGetValue("pg", out tmp); pg = (string)tmp;
                        message.TryGetValue("sessionid", out tmp); sessionid = (string)tmp;
                        message.TryGetValue("username", out tmp); username = (string)tmp;
                        message.TryGetValue("nickname", out tmp); nickname = (string)tmp;
                        message.TryGetValue("is_signed", out tmp); is_signed = (string)tmp;
                        message.TryGetValue("signed_count", out tmp); signed_count = (string)tmp;
                        message.TryGetValue("live_stat", out tmp); live_stat = (string)tmp;
                        message.TryGetValue("npv", out tmp); npv = (string)tmp;
                        message.TryGetValue("best_dlev", out tmp); best_dlev = (string)tmp;
                        message.TryGetValue("cur_lev", out tmp); cur_lev = (string)tmp;
                    }

                }
                private iLogin _Login = null;
                public iLogin Login
                {
                    get { return _Login; }
                }



                public class iLogout
                {
                    public string type;

                    public iLogout(Dictionary<string, object> message)
                    {
                        object tmp;
                        message.TryGetValue("type", out tmp); type = (string)tmp;
                    }
                }
                private iLogout _Logout = null;
                public iLogout Logout
                {
                    get { return _Logout; }
                }



                public class iHeartBeat
                {
                    public string type;
                    public string tick;

                    public iHeartBeat(Dictionary<string, object> message)
                    {
                        object tmp;
                        message.TryGetValue("type", out tmp); type = (string)tmp;
                        message.TryGetValue("tick", out tmp); tick = (string)tmp;
                    }
                }
                private iHeartBeat _HeartBeat = null;
                public iHeartBeat HeartBeat
                {
                    get { return _HeartBeat; }
                }



                public class iChat
                {
                    public string type;
                    /// <summary>
                    /// 礼物id号
                    /// </summary>
                    public string gid;
                    /// <summary>
                    /// 房间id号
                    /// </summary>
                    public string rid;
                    /// <summary>
                    /// 用户id号
                    /// </summary>
                    public string uid;
                    /// <summary>
                    /// 用户昵称
                    /// </summary>
                    public string nn;
                    /// <summary>
                    /// 消息内容
                    /// </summary>
                    public string txt;
                    public string cid;
                    public string level;
                    public string gt;
                    public string col;
                    public string ct;
                    public string rg;
                    public string pg;
                    public string dlv;
                    public string dc;
                    public string bdlv;

                    public iChat(Dictionary<string, object> message)
                    {
                        object tmp;
                        message.TryGetValue("type", out tmp); type = (string)tmp;
                        message.TryGetValue("gid", out tmp); gid = (string)tmp;
                        message.TryGetValue("rid", out tmp); rid = (string)tmp;
                        message.TryGetValue("uid", out tmp); uid = (string)tmp;
                        message.TryGetValue("nn", out tmp); nn = (string)tmp;
                        message.TryGetValue("txt", out tmp); txt = (string)tmp;
                        message.TryGetValue("cid", out tmp); cid = (string)tmp;
                        message.TryGetValue("level", out tmp); level = (string)tmp;
                        message.TryGetValue("gt", out tmp); gt = (string)tmp;
                        message.TryGetValue("col", out tmp); col = (string)tmp;
                        message.TryGetValue("ct", out tmp); ct = (string)tmp;
                        message.TryGetValue("rg", out tmp); rg = (string)tmp;
                        message.TryGetValue("pg", out tmp); pg = (string)tmp;
                        message.TryGetValue("dlv", out tmp); dlv = (string)tmp;
                        message.TryGetValue("dc", out tmp); dc = (string)tmp;
                        message.TryGetValue("bdlv", out tmp); bdlv = (string)tmp;
                    }
                    public override string ToString()
                    {
                        string result = "type:" + type + ", ";
                        result += "gid:" + gid + ", ";
                        result += "rid:" + rid + ", ";
                        result += "uid:" + uid + ", ";
                        result += "nn:" + nn + ", ";
                        result += "txt:" + txt + ", ";
                        result += "cid:" + cid + ", ";
                        result += "level:" + level + ", ";
                        result += "gt:" + gt + ", ";
                        result += "col:" + col + ", ";
                        result += "ct:" + ct + ", ";
                        result += "rg:" + rg + ", ";
                        result += "pg:" + pg + ", ";
                        result += "dlv:" + dlv + ", ";
                        result += "dc:" + dc + ", ";
                        result += "bdlv:" + bdlv + "";
                        return result;
                    }
                }
                private iChat _Chat = null;
                public iChat Chat
                {
                    get { return _Chat; }
                }



                public class iGiftPickedUp
                {
                    public string type;
                    public string rid;
                    public string uid;
                    public string gid;
                    public string sil;
                    public string _if;
                    public string ct;
                    public string nn;

                    public iGiftPickedUp(Dictionary<string, object> message)
                    {
                        object tmp;
                        message.TryGetValue("type", out tmp); type = (string)tmp;
                        message.TryGetValue("rid", out tmp); rid = (string)tmp;
                        message.TryGetValue("uid", out tmp); uid = (string)tmp;
                        message.TryGetValue("gid", out tmp); gid = (string)tmp;
                        message.TryGetValue("sil", out tmp); sil = (string)tmp;
                        message.TryGetValue("if", out tmp); _if = (string)tmp;
                        message.TryGetValue("ct", out tmp); ct = (string)tmp;
                        message.TryGetValue("nn", out tmp); nn = (string)tmp;
                    }
                }
                private iGiftPickedUp _GiftPickedUp = null;
                public iGiftPickedUp GiftPickedUp
                {
                    get { return _GiftPickedUp; }
                }



                public class iGiftDonated
                {
                    public string type;
                    public string rid;
                    public string gid;
                    public string gfid;
                    public string gs;
                    public string uid;
                    public string nn;
                    public string str;
                    public string level;
                    public string dw;
                    public string gfcnt;
                    public string hits;
                    public string dlv;
                    public string dc;
                    public string bdl;
                    public string rg;
                    public string pg;
                    public string rpid;
                    public string slt;
                    public string elt;

                    public iGiftDonated(Dictionary<string, object> message)
                    {
                        object tmp;
                        message.TryGetValue("type", out tmp); type = (string)tmp;
                        message.TryGetValue("rid", out tmp); rid = (string)tmp;
                        message.TryGetValue("gid", out tmp); gid = (string)tmp;
                        message.TryGetValue("gfid", out tmp); gfid = (string)tmp;
                        message.TryGetValue("gs", out tmp); gs = (string)tmp;
                        message.TryGetValue("uid", out tmp); uid = (string)tmp;
                        message.TryGetValue("nn", out tmp); nn = (string)tmp;
                        message.TryGetValue("str", out tmp); str = (string)tmp;
                        message.TryGetValue("level", out tmp); level = (string)tmp;
                        message.TryGetValue("dw", out tmp); dw = (string)tmp;
                        message.TryGetValue("gfcnt", out tmp); gfcnt = (string)tmp;
                        message.TryGetValue("hits", out tmp); hits = (string)tmp;
                        message.TryGetValue("dlv", out tmp); dlv = (string)tmp;
                        message.TryGetValue("dc", out tmp); dc = (string)tmp;
                        message.TryGetValue("bdl", out tmp); bdl = (string)tmp;
                        message.TryGetValue("rg", out tmp); rg = (string)tmp;
                        message.TryGetValue("pg", out tmp); pg = (string)tmp;
                        message.TryGetValue("rpid", out tmp); rpid = (string)tmp;
                        message.TryGetValue("slt", out tmp); slt = (string)tmp;
                        message.TryGetValue("elt", out tmp); elt = (string)tmp;
                    }
                }
                private iGiftDonated _GiftDonated = null;
                public iGiftDonated GiftDonated
                {
                    get { return _GiftDonated; }
                }



                public class iAudienceSeatTaken
                {
                    public string type;
                    public string rid;
                    public string gid;
                    public string uid;
                    public string nn;
                    public string str;
                    public string level;
                    public string gt;
                    public string rg;
                    public string pg;
                    public string dlv;
                    public string dc;
                    public string bdlv;

                    public iAudienceSeatTaken(Dictionary<string, object> message)
                    {
                        object tmp;
                        message.TryGetValue("type", out tmp); type = (string)tmp;
                        message.TryGetValue("rid", out tmp); rid = (string)tmp;
                        message.TryGetValue("gid", out tmp); gid = (string)tmp;
                        message.TryGetValue("uid", out tmp); uid = (string)tmp;
                        message.TryGetValue("nn", out tmp); nn = (string)tmp;
                        message.TryGetValue("str", out tmp); str = (string)tmp;
                        message.TryGetValue("level", out tmp); level = (string)tmp;
                        message.TryGetValue("gt", out tmp); gt = (string)tmp;
                        message.TryGetValue("rg", out tmp); rg = (string)tmp;
                        message.TryGetValue("pg", out tmp); pg = (string)tmp;
                        message.TryGetValue("dlv", out tmp); dlv = (string)tmp;
                        message.TryGetValue("dc", out tmp); dc = (string)tmp;
                        message.TryGetValue("bdlv", out tmp); bdlv = (string)tmp;
                    }
                }
                private iAudienceSeatTaken _AudienceSeatTaken = null;
                public iAudienceSeatTaken AudienceSeatTaken
                {
                    get { return _AudienceSeatTaken; }
                }



                public class iDeserveBought
                {
                    public string type;
                    public string rid;
                    public string gid;
                    public string level;
                    public string cnt;
                    public string hits;
                    public string lev;
                    public Dictionary<string, object> sui_origin;
                    private iDeserveBought_sui _sui;
                    public iDeserveBought_sui sui
                    {
                        set
                        {
                            _sui = value;
                        }
                        get
                        {
                            return _sui;
                        }
                    }

                    public iDeserveBought(Dictionary<string, object> message)
                    {
                        object tmp;
                        message.TryGetValue("type", out tmp); type = (string)tmp;
                        message.TryGetValue("rid", out tmp); rid = (string)tmp;
                        message.TryGetValue("gid", out tmp); gid = (string)tmp;
                        message.TryGetValue("level", out tmp); level = (string)tmp;
                        message.TryGetValue("cnt", out tmp); cnt = (string)tmp;
                        message.TryGetValue("hits", out tmp); hits = (string)tmp;
                        message.TryGetValue("lev", out tmp); lev = (string)tmp;

                        message.TryGetValue("sui", out tmp); sui_origin = (Dictionary<string, object>)tmp;
                        sui = new iDeserveBought_sui(sui_origin);
                    }
                }
                private iDeserveBought _DeserveBought = null;
                public iDeserveBought DeserveBought
                {
                    get { return _DeserveBought; }
                }
                public class iDeserveBought_sui
                {
                    public string id;
                    public string nick;
                    public string rg;
                    public string cur_lev;
                    public string cq_cnt;
                    public string best_dlev;
                    public string level;
                    public string gt;

                    public iDeserveBought_sui(Dictionary<string, object> originObject)
                    {
                        if (originObject != null)
                        {
                            object tmp;
                            originObject.TryGetValue("id", out tmp); id = (string)tmp;
                            originObject.TryGetValue("nick", out tmp); nick = (string)tmp;
                            originObject.TryGetValue("rg", out tmp); rg = (string)tmp;
                            originObject.TryGetValue("cur_lev", out tmp); cur_lev = (string)tmp;
                            originObject.TryGetValue("cq_cnt", out tmp); cq_cnt = (string)tmp;
                            originObject.TryGetValue("best_dlev", out tmp); best_dlev = (string)tmp;
                            originObject.TryGetValue("level", out tmp); level = (string)tmp;
                            originObject.TryGetValue("gt", out tmp); gt = (string)tmp;
                        }
                    }
                }


                public class iCameraOnOff
                {
                    public string type;
                    public string rid;
                    public string gid;
                    public string ss;
                    public string code;
                    public string rt;
                    public string notify;
                    public string endtime;

                    public iCameraOnOff(Dictionary<string, object> message)
                    {
                        object tmp;
                        message.TryGetValue("type", out tmp); type = (string)tmp;
                        message.TryGetValue("rid", out tmp); rid = (string)tmp;
                        message.TryGetValue("gid", out tmp); gid = (string)tmp;
                        message.TryGetValue("ss", out tmp); ss = (string)tmp;
                        message.TryGetValue("code", out tmp); code = (string)tmp;
                        message.TryGetValue("rt", out tmp); rt = (string)tmp;
                        message.TryGetValue("notify", out tmp); notify = (string)tmp;
                        message.TryGetValue("endtime", out tmp); endtime = (string)tmp;
                    }
                }
                private iCameraOnOff _CameraOnOff = null;
                public iCameraOnOff CameraOnOff
                {
                    get { return _CameraOnOff; }
                }



                public class iRankList
                {
                    public string type;
                    public string rid;
                    public string ts;
                    public string gid;
                    public iRankList_list_all list_all { set; get; }
                    public iRankList_list list { set; get; }
                    public iRankList_list_dat list_day { set; get; }

                    public Dictionary<string, object> list_all_origin;
                    public Dictionary<string, object> list_origin;
                    public Dictionary<string, object> list_day_origin;


                    public iRankList(Dictionary<string, object> message)
                    {
                        object tmp;
                        message.TryGetValue("type", out tmp); type = (string)tmp;
                        message.TryGetValue("rid", out tmp); rid = (string)tmp;
                        message.TryGetValue("ts", out tmp); ts = (string)tmp;
                        message.TryGetValue("gid", out tmp); gid = (string)tmp;

                        message.TryGetValue("list_all", out tmp);
                        if (tmp != null && tmp.GetType() == typeof(Dictionary<string, object>))
                            list_all_origin = (Dictionary<string, object>)tmp;
                        else list_all_origin = new Dictionary<string, object>();

                        message.TryGetValue("list", out tmp);
                        if (tmp != null && tmp.GetType() == typeof(Dictionary<string, object>))
                            list_origin = (Dictionary<string, object>)tmp;
                        else list_origin = new Dictionary<string, object>();

                        message.TryGetValue("list_day", out tmp);
                        if (tmp != null && tmp.GetType() == typeof(Dictionary<string, object>))
                            list_day_origin = (Dictionary<string, object>)tmp;
                        else list_day_origin = new Dictionary<string, object>();

                        list_all = new iRankList_list_all(list_all_origin);
                        list = new iRankList_list(list_origin);
                        list_day = new iRankList_list_dat(list_day_origin);
                    }
                }
                private iRankList _RankList = null;
                public iRankList RankList
                {
                    get { return _RankList; }
                }
                public class iRankList_list_all
                {
                    public string uid;
                    public string crk;
                    public string lrk;
                    public string rs;
                    public string gold_cost;

                    public iRankList_list_all(Dictionary<string, object> originObject)
                    {
                        if (originObject != null)
                        {
                            object tmp;
                            originObject.TryGetValue("uid", out tmp); uid = (string)tmp;
                            originObject.TryGetValue("crk", out tmp); crk = (string)tmp;
                            originObject.TryGetValue("lrk", out tmp); lrk = (string)tmp;
                            originObject.TryGetValue("rs", out tmp); rs = (string)tmp;
                            originObject.TryGetValue("gold_cost", out tmp); gold_cost = (string)tmp;
                        }
                    }
                }
                public class iRankList_list
                {
                    public string uid;
                    public string crk;
                    public string lrk;
                    public string rs;
                    public string gold_cost;

                    public iRankList_list(Dictionary<string, object> originObject)
                    {
                        if (originObject != null)
                        {
                            object tmp;
                            originObject.TryGetValue("uid", out tmp); uid = (string)tmp;
                            originObject.TryGetValue("crk", out tmp); crk = (string)tmp;
                            originObject.TryGetValue("lrk", out tmp); lrk = (string)tmp;
                            originObject.TryGetValue("rs", out tmp); rs = (string)tmp;
                            originObject.TryGetValue("gold_cost", out tmp); gold_cost = (string)tmp;
                        }
                    }
                }
                public class iRankList_list_dat
                {
                    public string uid;
                    public string crk;
                    public string lrk;
                    public string rs;
                    public string gold_cost;

                    public iRankList_list_dat(Dictionary<string, object> originObject)
                    {
                        if (originObject != null)
                        {
                            object tmp;
                            originObject.TryGetValue("uid", out tmp); uid = (string)tmp;
                            originObject.TryGetValue("crk", out tmp); crk = (string)tmp;
                            originObject.TryGetValue("lrk", out tmp); lrk = (string)tmp;
                            originObject.TryGetValue("rs", out tmp); rs = (string)tmp;
                            originObject.TryGetValue("gold_cost", out tmp); gold_cost = (string)tmp;
                        }
                    }
                }


                public class iSuperMsg
                {
                    public string type;
                    public string rid;
                    public string gid;
                    public string sdid;
                    public string trid;
                    public string content;

                    public iSuperMsg(Dictionary<string, object> message)
                    {
                        object tmp;
                        message.TryGetValue("type", out tmp); type = (string)tmp;
                        message.TryGetValue("rid", out tmp); rid = (string)tmp;
                        message.TryGetValue("gid", out tmp); gid = (string)tmp;
                        message.TryGetValue("sdid", out tmp); sdid = (string)tmp;
                        message.TryGetValue("trid", out tmp); trid = (string)tmp;
                        message.TryGetValue("content", out tmp); content = (string)tmp;
                    }
                }
                private iSuperMsg _SuperMsg = null;
                public iSuperMsg SuperMsg
                {
                    get { return _SuperMsg; }
                }



                public class iDefiance
                {
                    public string type;
                    public string rid;
                    public string gid;
                    public string sn;
                    public string dn;
                    public string gn;
                    public string gc;
                    public string drid;
                    public string gs;
                    public string gb;
                    public string es;
                    public string gfid;
                    public string eid;

                    public iDefiance(Dictionary<string, object> message)
                    {
                        object tmp;
                        message.TryGetValue("type", out tmp); type = (string)tmp;
                        message.TryGetValue("rid", out tmp); rid = (string)tmp;
                        message.TryGetValue("gid", out tmp); gid = (string)tmp;
                        message.TryGetValue("sn", out tmp); sn = (string)tmp;
                        message.TryGetValue("dn", out tmp); dn = (string)tmp;
                        message.TryGetValue("gn", out tmp); gn = (string)tmp;
                        message.TryGetValue("gc", out tmp); gc = (string)tmp;
                        message.TryGetValue("gs", out tmp); gs = (string)tmp;
                        message.TryGetValue("gb", out tmp); gb = (string)tmp;
                        message.TryGetValue("es", out tmp); es = (string)tmp;
                        message.TryGetValue("gfid", out tmp); gfid = (string)tmp;
                        message.TryGetValue("eid", out tmp); eid = (string)tmp;
                    }
                }
                private iDefiance _Defiance = null;
                public iDefiance Defiance
                {
                    get { return _Defiance; }
                }



                public class iRedPacket
                {
                    public string type;
                    public string rid;
                    public string gid;
                    public string sl;
                    public string sid;
                    public string did;
                    public string snk;
                    public string dnk;
                    public string rpt;

                    public iRedPacket(Dictionary<string, object> message)
                    {
                        object tmp;
                        message.TryGetValue("type", out tmp); type = (string)tmp;
                        message.TryGetValue("rid", out tmp); rid = (string)tmp;
                        message.TryGetValue("gid", out tmp); gid = (string)tmp;
                        message.TryGetValue("sl", out tmp); sl = (string)tmp;
                        message.TryGetValue("sid", out tmp); sid = (string)tmp;
                        message.TryGetValue("did", out tmp); did = (string)tmp;
                        message.TryGetValue("snk", out tmp); snk = (string)tmp;
                        message.TryGetValue("dnk", out tmp); dnk = (string)tmp;
                        message.TryGetValue("rpt", out tmp); rpt = (string)tmp;
                    }
                }
                private iRedPacket _RedPacket = null;
                public iRedPacket RedPacket
                {
                    get { return _RedPacket; }
                }



                public class iRankUp
                {
                    public string type;
                    public string rid;
                    public string gid;
                    public string uid;
                    public string drid;
                    public string rt;
                    public string bt;
                    public string sz;
                    public string nk;
                    public string rkt;
                    public string rn;

                    public iRankUp(Dictionary<string, object> message)
                    {
                        object tmp;
                        message.TryGetValue("type", out tmp); type = (string)tmp;
                        message.TryGetValue("rid", out tmp); rid = (string)tmp;
                        message.TryGetValue("gid", out tmp); gid = (string)tmp;
                        message.TryGetValue("uid", out tmp); uid = (string)tmp;
                        message.TryGetValue("drid", out tmp); drid = (string)tmp;
                        message.TryGetValue("rt", out tmp); rt = (string)tmp;
                        message.TryGetValue("bt", out tmp); bt = (string)tmp;
                        message.TryGetValue("sz", out tmp); sz = (string)tmp;
                        message.TryGetValue("nk", out tmp); nk = (string)tmp;
                        message.TryGetValue("rkt", out tmp); rkt = (string)tmp;
                        message.TryGetValue("rn", out tmp); rn = (string)tmp;
                    }
                }
                private iRankUp _RankUp = null;
                public iRankUp RankUp
                {
                    get { return _RankUp; }
                }



                public class iError
                {
                    public string type;
                    private string _code;
                    public string code
                    {
                        get
                        {
                            return _code;
                        }
                        set
                        {
                            _code = value;
                            switch (_code)
                            {
                                case "0":
                                    _codeMsg = "操作成功";
                                    break;
                                case "51":
                                    _codeMsg = "数据传输错误";
                                    break;
                                case "52":
                                    _codeMsg = "服务器关闭";
                                    break;
                                case "204":
                                    _codeMsg = "房间id错误";
                                    break;
                                default:
                                    _codeMsg = "服务器内部异常（未知错误）";
                                    break;
                            }
                        }
                    }
                    private string _codeMsg;
                    public string codeMsg { get { return _codeMsg; } }

                    public iError(Dictionary<string, object> message)
                    {
                        object tmp;
                        message.TryGetValue("type", out tmp); type = (string)tmp;
                        message.TryGetValue("code", out tmp); code = (string)tmp;
                    }
                }
                private iError _Error = null;
                public iError Error
                {
                    get { return _Error; }
                }

                #endregion




                public iStructedMsg(Dictionary<string, object> message)
                {
                    string msg_type = (string)message["type"];
                    switch (msg_type)
                    {
                        case "loginres":
                            this._Type = iType.Login; // loginres  登入
                            _Login = new iLogin(message);
                            break;


                        case "logout":
                            this._Type = iType.Logout; // logout  登出
                            _Logout = new iLogout(message);
                            break;


                        case "keeplive":
                            this._Type = iType.HeartBeat; // keeplive  心跳，保证连接持续
                            _HeartBeat = new iHeartBeat(message);
                            break;


                        case "chatmsg":
                            this._Type = iType.Chat; // chatmsg  聊天消息
                            _Chat = new iChat(message);
                            break;


                        case "onlinegift":
                            this._Type = iType.GiftPickedUp; // onlinegift  用户领取鱼丸（暴击）
                            _GiftPickedUp = new iGiftPickedUp(message);
                            break;


                        case "dgb":
                            this._Type = iType.GiftDonated; // dgb  用户赠送礼物
                            _GiftDonated = new iGiftDonated(message);
                            break;


                        case "uenter":
                            this._Type = iType.AudienceSeatTaken; // uenter  用户进入
                            _AudienceSeatTaken = new iAudienceSeatTaken(message);
                            break;


                        case "bc_buy_deserve":
                            this._Type = iType.DeserveBought; // bc_buy_deserve  用户赠送酬勤
                            _DeserveBought = new iDeserveBought(message);
                            break;


                        case "rss":
                            this._Type = iType.CameraOnOff; // rss  开播提醒
                            _CameraOnOff = new iCameraOnOff(message);
                            break;


                        case "ranklist":
                            this._Type = iType.RankList; // ranklist 排行榜
                            _RankList = new iRankList(message);
                            break;


                        case "ssd":
                            this._Type = iType.SuperMsg; // ssd 超级弹幕，  用于群发的广告？？？？？？
                            _SuperMsg = new iSuperMsg(message);
                            break;


                        case "spbc":
                            this._Type = iType.Defiance; // spbc   房间内礼物， 用于 两个观众之间送礼 ？？？？？？
                            _Defiance = new iDefiance(message);
                            break;


                        case "ggbb":
                            this._Type = iType.RedPacket; // ggbb   红包， 谁发的？ 谁抢的？
                            _RedPacket = new iRedPacket(message);
                            break;


                        case "rankup":
                            this._Type = iType.RankUp; // rankup  用户在前十排行中发生变化（只发布提升变化）
                            _RankUp = new iRankUp(message);
                            break;


                        case "error":
                            this._Type = iType.Error; // error 报错  code  0-ok  51-传输错误  52服务器关闭  204房间id错误  其他---其他错误
                            _Error = new iError(message);
                            break;


                        default: // other
                            this._Type = iType.Ohter;
                            break;
                    }
                }
            }
            private iStructedMsg _StructedMsg;
            public iStructedMsg StructedMsg
            {
                get
                {
                    return _StructedMsg;
                }
            }

            public Dictionary<string, object> message;

            public Bullet(Dictionary<string, object> message)
            {
                this.message = message;
                _StructedMsg = new iStructedMsg(message);
            }
            public string MsgToString()
            {
                return _MsgToString(message);
            }
            private string _MsgToString(Dictionary<string, object> dict)
            {
                if (dict == null) return "";
                StringBuilder builder = new StringBuilder();
                foreach (KeyValuePair<string, object> pair in dict)
                {
                    if (pair.Value.GetType() == typeof(string))
                    {
                        // value is a simple string
                        builder.Append(pair.Key).Append(":").Append(pair.Value).Append(", ");
                    }
                    else if (pair.Value.GetType() == typeof(Dictionary<string, object>))
                    {
                        // value is a dictionary(msg set)
                        builder.Append(pair.Key).Append(":[").Append(_MsgToString((Dictionary<string, object>)pair.Value)).Append("], ");
                    }
                }
                string result = builder.ToString();
                // Remove the final delimiter
                result = result.TrimEnd().TrimEnd(',');
                return result;
            }
        }





        public class KeepAlive
        {
            private NamedBackgroundWorker bgWorker;

            public KeepAlive(NamedBackgroundWorker bgWorker)
            {
                this.bgWorker = bgWorker;
            }
            //private Timer iTimer;
            //public void Run()
            //{
            //    DyBulletScreenClient danmuClient = DyBulletScreenClient.getInstance(bgWorker);
            //    iTimer = new Timer(iTimerTick, danmuClient, 0, 45000);
            //}
            //public void Stop()
            //{
            //    if (iTimer != null) iTimer.Dispose();
            //}
            //private void iTimerTick(object danmuClientObj)
            //{
            //    DyBulletScreenClient danmuClient = (DyBulletScreenClient)danmuClientObj;
            //    if (danmuClient.getReadyFlag())
            //    {
            //        if (bgWorker.CancellationPending == true)
            //        {
            //            iTimer.Dispose();
            //        }
            //        //发送心跳保持协议给服务器端
            //        danmuClient.keepAlive();
            //    }
            //}

            public void run()
            {
                //获取弹幕客户端
                DyBulletScreenClient danmuClient = DyBulletScreenClient.getInstance(bgWorker, false);

                //判断客户端就绪状态
                int counter;
                while (danmuClient.getReadyFlag())
                {
                    if (bgWorker.CancellationPending == true)
                    {
                        break;
                    }
                    //发送心跳保持协议给服务器端
                    danmuClient.keepAlive(bgWorker);
                    counter = 450;
                    while (counter > 0)
                    {
                        try
                        {
                            //设置间隔45秒再发送心跳协议
                            Thread.Sleep(100);        //keep live at least once per minute
                            if (bgWorker.CancellationPending == true)
                            {
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                        counter--;
                    }
                }
            }
        }





        public class KeepGetMsg
        {
            private NamedBackgroundWorker bgWorker;

            public KeepGetMsg(NamedBackgroundWorker bgWorker)
            {
                this.bgWorker = bgWorker;
            }
            public void run()
            {
                ////获取弹幕客户端
                DyBulletScreenClient danmuClient = DyBulletScreenClient.getInstance(bgWorker, false);
                bool errArise;
                //判断客户端就绪状态
                while (danmuClient.getReadyFlag())
                {
                    if (bgWorker.CancellationPending == true)
                    {
                        break;
                    }
                    //获取服务器发送的弹幕信息
                    danmuClient.getServerMsg(bgWorker, out errArise);
                    if (errArise == true)
                    {
                        break;
                    }
                }
            }
        }










        public class DyMessage
        {
            //弹幕客户端类型设置
            public static int DY_MESSAGE_TYPE_CLIENT = 689;

            /**
             * 生成登录请求数据包
             * @param roomId
             * @return
             */
            public static byte[] getLoginRequestData(int roomId)
            {
                //编码器初始化
                DyEncoder enc = new DyEncoder();
                //添加登录协议type类型
                enc.addItem("type", "loginreq");
                //添加登录房间ID
                enc.addItem("roomid", roomId);

                //返回登录协议数据
                return DyMessage.getByte(enc.getResult());
            }

            /**
             * 解析登录请求返回结果
             * @param respond
             * @return
             */
            public static bool parseLoginRespond(byte[] respond)
            {
                bool rtn = false;

                //返回数据不正确（仅包含12位信息头，没有信息内容）
                if (respond.Length <= 12)
                {
                    return rtn;
                }

                //解析返回信息包中的信息内容
                //string dataStr = new string(respond, 12, respond.Length - 12);
                string dataStr = Encoding.UTF8.GetString(respond.Skip(12).ToArray<byte>());
                //string dataStr = Encoding.Unicode.GetString((byte[])respond.Skip(12));

                //针对登录返回信息进行判断
                if (dataStr.Contains("type@=loginres"))
                {
                    rtn = true;
                }

                //返回登录是否成功判断结果
                return rtn;
            }

            /**
             * 生成加入弹幕分组池数据包
             * @param roomId
             * @param groupId
             * @return
             */
            public static byte[] getJoinGroupRequest(int roomId, int groupId)
            {
                //编码器初始化
                DyEncoder enc = new DyEncoder();
                //添加加入弹幕池协议type类型
                enc.addItem("type", "joingroup");
                //添加房间id信息
                enc.addItem("rid", roomId);
                //添加弹幕分组池id信息
                enc.addItem("gid", groupId);

                //返回加入弹幕池协议数据
                return DyMessage.getByte(enc.getResult());
            }

            /**
             * 生成心跳协议数据包
             * @param timeStamp
             * @return
             */
            public static byte[] getKeepAliveData(int timeStamp)
            {
                //编码器初始化
                DyEncoder enc = new DyEncoder();
                //添加心跳协议type类型
                enc.addItem("type", "keeplive");
                //添加心跳时间戳
                enc.addItem("tick", timeStamp);

                //返回心跳协议数据
                return DyMessage.getByte(enc.getResult());
            }

            /**
             * 通用方法，将数据转换为小端整数格式
             * @param data
             * @return
             */
            private static byte[] getByte(string data)
            {
                //ByteArrayOutputStream boutput = new ByteArrayOutputStream();
                //DataOutputStream doutput = new DataOutputStream(boutput);
                ////string resultStr, tmp;
                List<byte> result = new List<byte>();

                try
                {
                    //resultStr = "";
                    //resultStr += Encoding.Default.GetString(ToLH(data.Length + 8).Take(4).ToArray<byte>()); // 4 bytes packet length
                    //resultStr += Encoding.Default.GetString(ToLH(data.Length + 8).Take(4).ToArray<byte>()); // 4 bytes packet length
                    ////doutput.write(ToLH(DY_MESSAGE_TYPE_CLIENT), 0, 2);   // 2 bytes message type
                    //resultStr += Encoding.Default.GetString(ToLH(DY_MESSAGE_TYPE_CLIENT).Take(2).ToArray<byte>());
                    ////doutput.writeByte(0);                                               // 1 bytes encrypt
                    ////doutput.writeByte(0);                                               // 1 bytes reserve
                    //byte zero = 0;
                    //resultStr += zero + zero;
                    ////doutput.writeBytes(data);
                    //resultStr += data;

                    byte[] tmpBytes = ToLH(data.Length + 8);
                    result.AddRange(tmpBytes);
                    result.AddRange(tmpBytes);
                    result.AddRange(ToLH(DY_MESSAGE_TYPE_CLIENT).Take(2));
                    result.Add(0);
                    result.Add(0);
                    result.AddRange(Encoding.UTF8.GetBytes(data));


                }
                catch (Exception e)
                {
                    throw e;
                }

                //return boutput.toByteArray();

                //return Encoding.Unicode.GetBytes(resultStr);
                ////return Encoding.Default.GetBytes(resultStr);
                return result.ToArray<byte>();


            }
            private static byte[] ToLH(int n)
            {
                byte[] b = new byte[4];
                b[0] = (byte)(n & 0xff);
                b[1] = (byte)(n >> 8 & 0xff);
                b[2] = (byte)(n >> 16 & 0xff);
                b[3] = (byte)(n >> 24 & 0xff);
                return b;
            }
        }










        public class DyEncoder
        {
            private string buf;

            /**
             * 返回弹幕协议格式化后的结果
             * @return
             */
            public String getResult()
            {
                //数据包末尾必须以'\0'结尾
                buf += '\0';
                return buf;
            }

            /**
             * 添加协议参数项
             * @param key
             * @param value
             */
            public void addItem(String key, Object value)
            {
                //根据斗鱼弹幕协议进行相应的编码处理
                buf += key.Replace("/", "@S").Replace("@", "@A");
                buf += "@=";
                if (value.GetType() == typeof(string))
                {
                    buf += (value.ToString().Replace("/", "@S").Replace("@", "@A"));
                }
                else if (value.GetType() == typeof(int))
                {
                    buf += value;
                }
                buf += "/";
            }
        }












        public class MsgView
        {

            private Dictionary<string, object> messageList;

            public MsgView(String data)
            {
                this.messageList = parseRespond(data);
            }

            /**
             * 获取弹幕信息对象
             * @return
             */
            public Dictionary<string, object> getMessageList()
            {
                return messageList;
            }

            /**
             * 设置弹幕信息对象
             * @param messageList
             */
            public void setMessageList(Dictionary<string, object> messageList)
            {
                this.messageList = messageList;
            }

            /**
             * 解析弹幕服务器接收到的协议数据
             * @param data
             * @return
             */
            public Dictionary<string, object> parseRespond(string data)
            {
                Dictionary<string, object> rtnMsg = new Dictionary<string, object>();

                //处理数据字符串末尾的'/0字符'
                if (data.Contains("/"))
                {
                    data = data.Substring(0, data.LastIndexOf("/"));
                }

                //对数据字符串进行拆分
                string[] buff = data.Split(new string[] { "/" }, StringSplitOptions.None);

                //分析协议字段中的key和value值
                string tmp1;
                object tmp2;
                foreach (string tmp in buff)
                {
                    //获取key值
                    //string key = StringUtils.substringBefore(tmp, "@=");
                    tmp1 = tmp;
                    if (tmp1.Contains("@=") == false)
                    {
                        tmp1 = tmp1.Replace("@S", "/").Replace("@A", "@");
                    }
                    if (tmp1.Contains("@=") == false)
                    {
                        return null;
                    }
                    string key = tmp1.Substring(0, tmp1.LastIndexOf("@="));
                    //获取对应的value值
                    //object value = StringUtils.substringAfter(tmp, "@=");
                    object value = tmp1.Substring(tmp1.IndexOf("@=") + 2);

                    //如果value值中包含子序列化值，则进行递归分析
                    if (((string)value).Contains("@A"))
                    {
                        value = ((string)value).Replace("@S", "/").Replace("@A", "@");
                        tmp2 = this.parseRespond((string)value);
                        if (tmp2 != null) value = tmp2;
                    }

                    //将分析后的键值对添加到信息列表中
                    rtnMsg.Add(key, value);
                }

                return rtnMsg;

            }

            /**
             * 调试信息输出
             * @return
             */
            public String printStr()
            {
                return messageList.ToString();
            }

        }













    }
}
