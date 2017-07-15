// 模块名   :  TCPClientWorker
// 创建者   :  Steven Yang
// 创建日期 :  2012-12-8
// 描    述 :  客户端网络接收类

using System;
using System.Net;
using System.Net.Sockets;
using Mogo.RPC;
using System.Collections.Generic;
using System.Threading;

namespace Mogo.Util
{
    /**网络状态监听**/
    public delegate void NetStatusListener(String status);

    /**TCPClient类，用于异步收发包**/
    public class TCPClientWorker
    {
        private uint timeId = 0;                                   //定时ID 
        private int readNum = 0;                                   //读取字节数
        private int msgLen = 0;                                    //当前接收包的包体长度
        private int tryNum = 0;                                    //已重试数
        private int beginTime;                                     //最后收包时间
        private Socket client;                                     //当前连接句柄
        private Thread thread;                                     //接收线程

        private byte[] headBuffer;                                 //包头缓冲区
        private byte[] bodyBuffer;                                 //包体缓冲区
        private byte[] reserveBuffer;                              //预留域缓冲区
        private bool isConnecting;								//正在连接中
        private bool isReceiving;								 //接收数据状态
        private bool isTryLinking;								//是否重连中
        private bool isPrintClose;								//是否打印网络关闭时日志

        private readonly ConcurrentQueue<byte[]> receiveQueue;   //已收包队列 
        private readonly Queue<byte[]> waitSendQueue;             //等待发送包队列 
        private NetStatusListener netStatusListener;               //网络状态回调

        private Int32 m_sendSerialNumber = -1;                     //包序号,从1开始,达到最大值后又重复从1开始累加,如此反复
        private VObject m_headLengthConverter;                     //消息包长度类型转换器
        private readonly byte[] m_baseRpcCallHead;
        private readonly byte[] m_cellRpcCallHead;

        private const int RESERVE_SIZE = 2;                        //预留域长度
        private const string LINKING = "linking";                  //连接中常量  
        private const string LINK_OK = "linkOk";                   //连接OK常量   
        private const string LINK_FAIL = "linkFail";               //连接失败常量
        public static int CLOSE_TIMEOUT = 30 * 1000;
        private int _closeTimeout = int.MaxValue;                  //网络断开超时(毫秒) 

        public Action OnNetworkDisconnected;                       //连接关闭监听

        public TCPClientWorker()
        {
            maxTryNum = 3;
            isPrintClose = false;
            isTryLinking = false;
            batchNum = 10;
            receiveQueue = new ConcurrentQueue<byte[]>();
            waitSendQueue = new Queue<byte[]>();
            m_headLengthConverter = VUInt32.Instance;
            headBuffer = new byte[m_headLengthConverter.VTypeLength];
            reserveBuffer = new byte[RESERVE_SIZE];
            m_baseRpcCallHead = VUInt16.Instance.Encode(MSGIDType.BASEAPP_CLIENT_RPCALL);
            m_cellRpcCallHead = VUInt16.Instance.Encode(MSGIDType.BASEAPP_CLIENT_RPC2CELL_VIA_BASE);
        }

        public int closeTimeout
        {
            set { beginTime = Environment.TickCount; _closeTimeout = value; }
            get { return _closeTimeout; }
        }

        /**连接服务器
         * @param ip   服务器IP
         * @param port 服务器断端口
         * **/
        public void Connect(string ip, int port)
        {
            try
            {
                this.ip = ip;
                this.port = port;
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.NoDelay = true;            //Negal
                client.SendBufferSize = 0xfa00;
                client.ReceiveBufferSize = 0xfa00;
                // client.Connect(ip, port);
                isConnecting = true;
                client.BeginConnect(ip, port, onConnected, null);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("-connect() 连接服务器[" + ip + ":" + port + "]失败,reason:" + ex.ToString());
                tryConnect();
            }
        }

        private void onConnected(IAsyncResult ar)
        {
            try
            {
                isConnecting = false;
                resetWhileConnected();
                client.EndConnect(ar);
                startReceive();
                if (netStatusListener != null) netStatusListener(LINK_OK);
                timeId = TimerHeap.AddTimer(1000, 3000, closeNotice);                                                                 //连接关闭通知
                sendWaitPack();                                                                                                       //发送由于网络中断的滞留包
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("-connect() 连接服务器[" + ip + ":" + port + "]失败,reason:" + ex.ToString());
                tryConnect();
            }
        }

        /**连接成功后，重置参数值**/
        private void resetWhileConnected()
        {
            tryNum = 0;
            msgLen = 0;
            readNum = 0;
            resetSerialNumber();
            isTryLinking = false;
            isPrintClose = false;
            beginTime = Environment.TickCount;
        }

        /**启动数据接收线程**/
        private void startReceive()
        {
            isReceiving = true;
            if (thread == null)
            {
                thread = new Thread(receive);
                thread.Start();
            }
        }

        /**停止数据接收线程**/
        private void stopReceive()
        {
            try
            {
                isReceiving = false;
                if (thread != null) thread.Abort();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("-stopReceive() 接收线程终止失败，reason：" + ex.StackTrace);
            }
            finally
            {
                thread = null;
            }
        }

        /**网络关闭通知**/
        private void closeNotice()
        {
            if (Environment.TickCount - beginTime > closeTimeout)
            {//网络已断开超时 
                closeCallback();
                Close();
            }
        }

        /**服务器关闭回调**/
        private void closeCallback()
        {
            try
            {
                if (OnNetworkDisconnected != null)
                {
                    DriverLib.Invoke(OnNetworkDisconnected);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Except(ex, "服务器[" + ip + ":" + port + "]关闭回调出错!");
            }
        }

        /**发送滞留包**/
        private void sendWaitPack()
        {
            while (Connected() && waitSendQueue.Count > 0)
            {
                sendData(waitSendQueue.Dequeue());
            }
        }

        /**发送包
         * @param data 发送数据
         * **/
        public void Send(byte[] data)
        {
            if (data == null || data.Length < 0)
            {
                LoggerHelper.Error("发送包为空或长度为零！data:" + data);
                return;
            }

            if (!Connected())
            {
                if (isConnecting || isOpenTryLink) waitSendQueue.Enqueue(data);  //如果正在连接，或者允许重连，将保存滞留包
                if (isTryLinking) return;
                if (!isOpenTryLink) return;
                if (netStatusListener != null) netStatusListener(LINKING);
                LoggerHelper.Error("-send() 已断开与服务器[" + ip + ":" + port + "]连接,将发起重连[Start]");
                isTryLinking = true;
                tryConnect();
            }
            else
            {
                sendData(data);
            }
        }

        /**进行发包
         * 发包组成:包头+序号+包体
         * 1、包头内容由：‘序号长度+包头长度+包体长度’组成
         * **/
        private void sendData(byte[] data)
        {
            //int seriNum = -1;                                                                                          //序号   
            byte[] headBytes = BitConverter.GetBytes(m_headLengthConverter.VTypeLength + RESERVE_SIZE + data.Length);  //包头
            byte[] sendBytes = new byte[headBytes.Length + RESERVE_SIZE + data.Length];                               //发送包
            //if (CompareMsgHead(m_baseRpcCallHead, data) || CompareMsgHead(m_cellRpcCallHead, data))
            //{//加序号，防重发
            //    seriNum = getSerialNumber();
            //}
            headBytes.CopyTo(sendBytes, 0);
            //if (seriNum >= 0) VUInt16.Instance.Encode(seriNum).CopyTo(sendBytes, headBytes.Length);                    //需发送序号
            data.CopyTo(sendBytes, headBytes.Length + RESERVE_SIZE);
            if (Connected())
            {
                //client.Send(sendBytes, 0, sendBytes.Length, SocketFlags.None);
                client.BeginSend(sendBytes, 0, sendBytes.Length, SocketFlags.None, null, null);  //new AsyncCallback(onSend), seriNum
                //LoggerHelper.Info("send: " + data.PackArray());
                //LoggerHelper.Info("发送包Len:" + sendBytes.Length + ",bodyLen:" + data.Length + ",thread:" + (thread != null ? thread.IsAlive : false) + ",timeId:" + timeId);
            }
            else
            {
                if (!isPrintClose) { isPrintClose = true; LoggerHelper.Debug("-sendData() 服务器[" + ip + ":" + port + "] 已关闭!"); }
            }
        }

        private void onSend(IAsyncResult result)
        {
            int seriNum = (int)result.AsyncState;
            if (seriNum > 0) LoggerHelper.Debug("seriNum:" + seriNum + " 包已发送,connected:" + Connected());
        }

        /**执行收包,收包组成：4字节包头+2字节预留域+2字节消息ID+包体**/
        private void receive()
        {
            while (isReceiving)
            {
                try
                {
                    if (!Connected()) continue;
                    if (msgLen == 0)
                    {//接收包头
                        if (client.Available >= headBuffer.Length + RESERVE_SIZE)
                        {
                            client.Receive(headBuffer);
                            client.Receive(reserveBuffer);
                            msgLen = BitConverter.ToInt32(headBuffer, 0);
                            msgLen = msgLen - headBuffer.Length - RESERVE_SIZE;
                            bodyBuffer = new byte[msgLen];
                            readNum = 0;
                        }
                        else
                        {
                            Thread.Sleep(10);
                        }
                    }
                    else
                    {//接收包体
                        //方法一：收包
                        int size = msgLen - readNum;
                        if (size > 0)
                        {//包读取中
                            readNum += client.Receive(bodyBuffer, readNum, size, SocketFlags.None);
                        }
                        else
                        {//包读取完
                            msgLen = 0;
                            readNum = 0;
                            beginTime = Environment.TickCount;
                            receiveQueue.Enqueue(bodyBuffer);
                        }

                        //方法二：收包
                        /*if (msgLen > 0 && client.Available >= msgLen)
                        {
                            byte[] bodyBuffer = new byte[msgLen];
                            msgLen = 0;
                            client.Receive(bodyBuffer, 0, bodyBuffer.Length, SocketFlags.None);
                            LoggerHelper.Info("-receive() 收到数据 bodyLen:" + bodyBuffer.Length); 
                            lock (recvLocker) { receiveQueue.Enqueue(bodyBuffer); }
                        }*/
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error("-receive() 接收包出错, reason:" + ex.Message);
                }
            }
        }

        /**提供给上层提取一个接收包**/
        public byte[] Recv()
        {
            return receiveQueue.TryDequeue();
        }

        /**重试连接**/
        private void tryConnect()
        {
            if (Connected()) return;
            isReceiving = false;
            if (!isOpenTryLink)
            {
                closeCallback();
                return;
            }

            if (tryNum++ < maxTryNum)
            {//重连
                LoggerHelper.Debug("-connect() [" + ip + ":" + port + "] 第" + tryNum + "次重连接");
                Connect(ip, port);
            }
            else
            {//连接失败
                LoggerHelper.Error("-tryConnect() [" + ip + ":" + port + "][Fail] 已超过最大重连次数maxTryNum:" + maxTryNum + ",tryNum:" + tryNum);
                if (netStatusListener != null) netStatusListener(LINK_FAIL);
            }
        }

        /**连接IP**/
        public string ip { get; set; }
        /**连接端口**/
        public int port { get; set; }
        /**设置|取得最大重连次数**/
        public int maxTryNum { get; set; }
        /**设置|取得每帧处理包数**/
        public int batchNum { get; set; }
        /**开启重连状态[true:开启重连,false:关闭重连]**/
        public bool isOpenTryLink { get; set; }
        /**取得连接状态**/
        public bool Connected() { return client != null && client.Connected; }

        /**关闭连接**/
        public void Close()
        {
            stopReceive();
            //OnNetworkDisconnected = null;
            if (client != null) client.Close();
            if (timeId > 0) TimerHeap.DelTimer(timeId);
            LoggerHelper.Debug("-close() connected:" + Connected() + " 已断开与服务器[" + ip + ":" + port + "]连接！");
        }

        /**网络状态监听**/
        public void statusListener(NetStatusListener listener)
        {
            netStatusListener = listener;
        }

        /**返回当前序号并对当前序号+1
         * 1、序号范围在[0,65535]间，达到最大值后又从0开始，如此反复
         * @return 当前序号(从0开始，最大65535)
         * **/
        private UInt16 getSerialNumber()
        {
            if (m_sendSerialNumber == UInt16.MaxValue) m_sendSerialNumber = -1;
            return Convert.ToUInt16(Interlocked.Increment(ref m_sendSerialNumber));
        }

        private void resetSerialNumber()
        {
            m_sendSerialNumber = -1;
        }

        private static bool CompareMsgHead(byte[] headCode, byte[] msg)
        {
            if (msg.Length < headCode.Length) return false;
            for (var i = 0; i < headCode.Length; i++)
            {
                if (headCode[i] != msg[i]) return false;
            }
            return true;
        }

        #region 暂时无用
        public void Process()
        {
            //DoSend();
        }

        /// <summary>
        /// 强迫中断线程
        /// </summary>
        public void Release()
        {//关闭发送死循环
            //m_asynSendSwitch = false;
        }
        #endregion
    }
}