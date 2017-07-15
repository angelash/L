using System;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using Mogo.Game;
using Mogo.Util;
using System.Threading;
using System.Text;

namespace Mogo.Game
{
    public class FindServerEvent
    {
        public readonly static string FindServerStart = "FindServerEvent.FindServerStart";
        public readonly static string FindServerStop = "FindServerEvent.FindServerStop";
        public readonly static string FindServerSucc = "FindServerEvent.FindServerSucc";
    }

    public class FindServer
    {
        public static int PORT = 43998;
        private Thread thread;                                     //线程
        private bool m_isStop = false;

        public delegate void CallBackDelegate(string ip); 

        #region 初始化
        public FindServer()
        {
            AddListeners();
        }

        public void Release()
        {
            RemoveListeners();
            isRun = false;
            if (serverSock != null)
            {
                //serverSock.Disconnect(false);
                //serverSock.Shutdown(SocketShutdown.Both);
                serverSock.Close();
                serverSock = null;
            }
            CloseServerFind();
        }

        private void AddListeners()
        {
            EventDispatcher.AddEventListener(FindServerEvent.FindServerStart, FindServerUdp);
            EventDispatcher.AddEventListener(FindServerEvent.FindServerStop, FindServerStop);
        }

        public void RemoveListeners()
        {
            EventDispatcher.RemoveEventListener(FindServerEvent.FindServerStart, FindServerUdp);
            EventDispatcher.AddEventListener(FindServerEvent.FindServerStop, FindServerStop);
        }
        #endregion

        #region tcp
        private void FindServerStart()
        {
            thread = null;
            m_isStop = false;
            CallBackDelegate cbd = CallBack;
            thread = new Thread(TryConnetServer);
            thread.Start(cbd);

        }

        private void FindServerStop()
        {
            m_isStop = true;
            CloseServerFind();
        }

        private List<string> GetAllLocalIp()
        {
            List<string> ipList = new List<string>();
            IPHostEntry ieh = Dns.GetHostByName(Dns.GetHostName());
            string myip = ieh.AddressList[0].ToString();
            string ipDuan = myip.Remove(myip.LastIndexOf("."));
            
            ipList.Add(myip);
            for (int i = 1; i <= 255; i++)
            {
                string pingIp = string.Concat(ipDuan, ".", i);
                if (myip != pingIp)
                    ipList.Add(pingIp);
            }
            return ipList;
        }

        public void TryConnetServer(object action)
        {
            List<string> ipList = GetAllLocalIp();
            bool status = false;
            string ip = null;
            for (int i = 0; i < ipList.Count; i++)
            {
                if (m_isStop)
                    break;
                try
                {
                    TcpClient tcp = new TcpClient(ipList[i], PORT);
                    //tcp.GetStream();
                    
                    status = true;
                    tcp.Close();
                    ip = ipList[i];
                    break;
                }
                catch (Exception ex)
                {
                    //LoggerHelper.Debug("===========fail:" + ipList[i]);
                    //LoggerHelper.Debug("======ex:" + ex);
                    status = false;
                }
            }
            CallBackDelegate cdb = action as CallBackDelegate;
            if (status)
            {
                LoggerHelper.Debug("====FindServer ok:" + ip);
                //cdb.Invoke(ip);
                DriverLib.Invoke(() => { cdb(ip); });
                //cdb(ip);
                //EventDispatcher.TriggerEvent<string>(FindServerEvent.FindServerSucc, ip);
            }
            else
            {
                LoggerHelper.Debug("====FindServer fail");
                DriverLib.Invoke(() => { cdb(ip); });
                //cdb(ip);
                //EventDispatcher.TriggerEvent<string>(FindServerEvent.FindServerSucc, ip);
            }
        }

        private void CallBack(string ip)
        {
            LoggerHelper.Debug("===========CallBack");
            EventDispatcher.TriggerEvent<string>(FindServerEvent.FindServerSucc, ip);
            stopFindThread();
        }

        /**停止线程**/
        private void stopFindThread()
        {
            try
            {
                if (thread != null)
                    thread.Abort();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("-stopFindThread() 线程终止失败，reason：" + ex.StackTrace);
            }
            finally
            {
                thread = null;
            }
        }

        #endregion

        #region UDP

        /// <summary>
        /// 创建服务器
        /// </summary>
        private Socket serverSock;
        private bool isRun = true;
        public void CreateUDP()
        {
            int recv;
            byte[] data = new byte[8096];

            //得到本机IP，设置TCP端口号         
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 13245);
            if (serverSock == null)
            {
                serverSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                //绑定网络地址
                serverSock.Bind(ipep);
            }

            //得到客户机IP
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);
            Action action = () =>
            {
                while (isRun)
                {
                    recv = serverSock.ReceiveFrom(data, ref Remote);
                    AddMsg(Encoding.UTF8.GetString(data, 0, recv));
                }
            };
            action.BeginInvoke(null, null);
        }

        public void AddMsg(string msg)
        {
             var action = new Action(() =>
            {
                LoggerHelper.Debug("=====================AddMsg:" + msg);
                Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(msg), 13246);
                IPHostEntry ieh = Dns.GetHostByName(Dns.GetHostName());
                string myip = ieh.AddressList[0].ToString();
                byte[] data = Encoding.UTF8.GetBytes(myip);
                server.SendTo(data, data.Length, SocketFlags.None, ipep);
                //server.Disconnect(true);
                //server.Shutdown(SocketShutdown.Both);
                
                server.Close();
            });
             action.BeginInvoke(null, null);
        }

        private static Socket serverFindSend;
        private uint timeOutTimer;
        public void FindServerUdp()
        {
            m_isStop = false;
            CreateFindUdp();
            if (serverFindSend == null)
            {
                //定义网络类型，数据连接类型和网络协议UDP
                serverFindSend = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            }

            List<string> ipList = GetAllLocalIp();
            LoggerHelper.Debug("===ipList===count:" + ipList.Count);
            for (int i = 0; i < ipList.Count; i++)
            {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ipList[i]), 13245);
                
                IPHostEntry ieh = Dns.GetHostByName(Dns.GetHostName());
                string myip = ieh.AddressList[0].ToString();
                SendMsg(myip, ipep);
                LoggerHelper.Debug("===SendMsg===ip:" + ipList[i] + ", msg:" + myip);
            }
            TimerHeap.DelTimer(timeOutTimer);
            timeOutTimer = TimerHeap.AddTimer(20000, 0, TimeOutHandle);
        }

        private void SendMsg(string msg, IPEndPoint ipep)
        {
            if (serverFindSend != null)
            {
                byte[] data = Encoding.UTF8.GetBytes(msg);
                serverFindSend.SendTo(data, data.Length, SocketFlags.None, ipep);
            }
        }

        private Socket serverFindReceive;
        private void CreateFindUdp()
        {
            int recv;
            byte[] data = new byte[8096];

            //得到本机IP，设置TCP端口号         
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 13246);
            if (serverFindReceive == null)
            {
                serverFindReceive = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                //绑定网络地址
                serverFindReceive.Bind(ipep);
            }

            

            //得到客户机IP
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);
            Action action = () =>
            {
                while (!m_isStop)
                {
                    recv = serverFindReceive.ReceiveFrom(data, ref Remote);
                    FindSucc(Encoding.UTF8.GetString(data, 0, recv));
                }
            };
            action.BeginInvoke(null, null);
        }

        public void FindSucc(string msg)
        {
            var action = new Action(() =>
            {
                LoggerHelper.Debug("=====================FindSucc:" + msg);
                m_isStop = true; //停止监听
                CloseServerFind();
                EventDispatcher.TriggerEvent<string>(FindServerEvent.FindServerSucc, msg);
            });

            DriverLib.Invoke(action);
        }

        private void TimeOutHandle()
        {
            m_isStop = true;
            CloseServerFind();
            EventDispatcher.TriggerEvent<string>(FindServerEvent.FindServerSucc, "");
        }

        private void CloseServerFind()
        {
            TimerHeap.DelTimer(timeOutTimer);
            if (serverFindSend != null)
            {
                //serverFind.Disconnect(true);
                //serverFindSend.Shutdown(SocketShutdown.Both);
                serverFindSend.Close();
                serverFindSend = null;
            }

            if (serverFindReceive != null)
            {
                //serverFindReceive.Disconnect(true);
                //serverFindReceive.Shutdown(SocketShutdown.Receive);
                serverFindReceive.Close();
                serverFindReceive = null;
            }
        }

        #endregion
    }
}


