#region 模块信息
/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：ServerProxy
// 创建者：Ash Tang
// 修改者列表：
// 创建日期：2013.2.16
// 模块描述：远程服务代理类
//----------------------------------------------------------------*/
#endregion
using System;
using System.Linq;
using Mogo.Util;
using Mogo.RPC;
using System.Collections.Generic;
using Mogo.GameData;

namespace Mogo.Game
{
    /// <summary>
    /// 远程服务控制类。
    /// </summary>
    public abstract class ServerProxy
    {
        public static bool SomeToLocal = false;//标记是否有些请求发给虚拟服
        private static ServerProxy m_instance;
        private static ServerProxy m_remoteProxy;
        protected static ServerProxy m_localProxy;
        protected static TCPClientWorker m_tcpWorker;
        protected static TCPServerWorker m_serverWorker;

        public Action<LoginResult> LoginResp;
        public Action BackToChooseServer;
        public Action OnCloseHandler;
        public Action<Action, Action> onRCHandler;

        /// <summary>
        /// 控制类实例。
        /// </summary>
        public static ServerProxy Instance
        {
            get { return m_instance; }
        }

        public static void InitServerProxy(ServerProxy remoteProxy)
        {
            m_remoteProxy = remoteProxy;
            SwitchToRemote();
        }

        public static void SwitchToLocal()
        {
            m_instance = m_localProxy;
        }

        public static void SwitchToRemote()
        {
            m_instance = m_remoteProxy;
        }

        public void Init(Action callback)
        {
            Action action = () =>
            {
                DefParser.Instance.InitEntityData();
                if (callback != null)
                    callback();
            };
            if (SystemSwitch.ReleaseMode && !UnityEngine.Debug.isDebugBuild)
#if UNITY_IPHONE
                action();
#else
                action.BeginInvoke(null, null);
#endif
            else
                action();
        }

        public abstract string StartServer();

        public abstract void StopServer();

        /// <summary>
        /// 连接远程服务。
        /// </summary>
        /// <param name="ip">服务器IP</param>
        /// <param name="port">服务端口</param>
        public abstract Boolean Connect(string ip, int port);

        public abstract void Disconnect();

        public bool Connected
        {
            get { return m_tcpWorker == null ? false : m_tcpWorker.Connected(); }
        }

        /// <summary>
        /// 调用远程方法。
        /// </summary>
        /// <param name="entityName">实体名称</param>
        /// <param name="funcName">方法名称</param>
        /// <param name="args">参数列表</param>
        public abstract void RpcCall(string funcName, params Object[] args);

        public abstract void Login(params string[] args);

        public abstract void Login(string _passport, string _password, string _loginArgs);

        public abstract void BaseLogin(String token);

        public abstract void Move(byte face, ushort x, ushort y, ushort z);

        public abstract void ReConnect();

        public abstract void SendReConnectKey(string key);

        public abstract void CheckDefMD5(Byte[] bytes);

        public abstract void Process();

        /// <summary>
        /// 监听远程回调。
        /// </summary>
        public abstract void Update();

        public abstract void Release();

        public abstract int closeTimeout { get; set; }
    }

    public class RemoteProxy : ServerProxy
    {
        /// <summary>
        /// 限制了每帧 收发包的数量。 
        /// </summary>
        private const ushort MAX_PACKETS_PER_FRAME = 1000;//放宽每帧处理消息数目，性能问题由逻辑解决
        private Dictionary<string, string> localHandles;

        public RemoteProxy()
        {
            m_tcpWorker = new TCPClientWorker();
            m_tcpWorker.OnNetworkDisconnected = CloseHandler;
            localHandles = new Dictionary<string, string>();
        }

        public override int closeTimeout
        {
            set { if (m_tcpWorker != null) m_tcpWorker.closeTimeout = value; }
            get { return m_tcpWorker != null ? m_tcpWorker.closeTimeout : -1; }
        }

        private int reConnectCnt = 0;
        private float closeTimeStamp = 0;
        private int connectCnt = 0; //防止一连上又断开引起的问题
        private float preCloseTime = 0;
        private void CloseHandler()
        {
            closeTimeStamp = UnityEngine.Time.time;
            if (connectCnt == 0)
            {
                preCloseTime = closeTimeStamp;
            }
            else
            {
                if ((closeTimeStamp - preCloseTime) > (2 * 60))
                {
                    connectCnt = 0;
                }
            }
            //if (!MogoWorld.beKick && (MogoWorld.theAccount != null || MogoWorld.thePlayer != null))
            //{
            //    if (reConnectCnt < 5)
            //    {
            //        ReConnect();
            //        return;
            //    }
            //    MogoGlobleUIManager.Instance.ShowGlobleStaticText(false, "");
            //    MogoWorldEx.rc = false;
            //}
            //MogoGlobleUIManager.Instance.Confirm(LanguageData.GetContent(25558), (ok) =>
            //{
            //    PlatformSdkManager.Instance.RestartGame();
            //    if (ok && BackToChooseServer != null)
            //        BackToChooseServer();
            //    MogoGlobleUIManager.Instance.ConfirmHide();
            //}, LanguageData.GetContent(25561));
            LoggerHelper.Info("proxy close handler");
            //if (onRCHandler != null && MogoWorldEx.CurrentRCFlag != MogoWorldEx.RCFlag.None)
            //{
            //    onRCHandler(ReConnect, BackToChooseServer);
            //}
            //MogoGlobleUIManager.Instance.Confirm(LanguageData.GetContent(25558), confirmCallback, LanguageData.GetContent(25561),
            //                                     LanguageData.GetContent(25584), -1, ButtonBgType.Blue, ButtonBgType.Blue);
            //if (OnCloseHandler != null)
            //    OnCloseHandler();
        }

        public override void ReConnect()
        {
            //float t = UnityEngine.Time.time;
            //if (t - closeTimeStamp > (10 * 60))
            //{//断线后十分钟重启游戏,2分钟内多次重连
            //    LoggerHelper.Info("restart game");
            //    PlatformSdkManager.Instance.RestartGame();
            //    return;
            //}
            //if (MogoWorldEx.isKicked) return;
            LoggerHelper.Info("reconnect " + reConnectCnt);
            m_tcpWorker.Close();
            m_tcpWorker = null;
            m_tcpWorker = new TCPClientWorker();
            m_tcpWorker.OnNetworkDisconnected = CloseHandler;
            //MogoWorldEx.rc = true;
            //TimerHeap.AddTimer(3000, 0, () => EventDispatcher.TriggerEvent(MogoWorldEvents.Login));
            reConnectCnt++;
            connectCnt++;
        }
        /// <summary>
        /// 连接远程服务。
        /// </summary>
        /// <param name="ip">服务器IP</param>
        /// <param name="port">服务端口</param>
        public override Boolean Connect(string ip, int port)
        {
            try
            {
                LoggerHelper.Debug("connect : " + ip + " p: " + port);
                m_tcpWorker.Connect(ip, port);
                reConnectCnt = 0;
                return true;
            }
            catch (Exception ex)
            {
                LoggerHelper.Except(ex);
                CloseHandler();
                return false;
            }
        }

        public override void Disconnect()
        {
            m_tcpWorker.Close();
        }

        private bool ToLocal(string funName, params Object[] args)
        {
            if (!localHandles.ContainsKey(funName))
            {
                return false;
            }
            if (funName != "MissionReq")
            {
                return true;
            }
            switch ((byte)args[0])
            {
                case 2:
                case 20:
                case 71:
                case 222:
                    return false;

                default:
                    return true;
            }
        }

        public override string StartServer()
        {
            m_serverWorker = new TCPServerWorker();
            return m_serverWorker.Start();
        }

        public override void StopServer()
        {
            if (m_serverWorker != null)
                m_serverWorker.Stop();
        }

        /// <summary>
        /// 调用远程方法。
        /// </summary>
        /// <param name="entityName">实体名称</param>
        /// <param name="funcName">方法名称</param>
        /// <param name="args">参数列表</param>
        public override void RpcCall(string funcName, params Object[] args)
        {
            //#if UNITY_EDITOR
            //            EventDispatcher.TriggerEvent(RpcEvent.SendRPC, funcName, args);
            //#endif
            //if (SomeToLocal && ToLocal(funcName, args))
            //{//转发给虚拟服
            //    ServerProxy.m_localProxy.RpcCall(funcName, args);
            //    return;
            //}
            try
            {
                //if (funcName != "GetServerTimeReq")

                if (m_tcpWorker == null)
                {
                    LoggerHelper.Warning("tcpWorker is null !!!");
                    return;
                }

                var entity = Pluto.CurrentEntity;
                if (entity == null)
                {
                    LoggerHelper.Warning("Pluto.CurrentEntity is not set.");
                    return;
                }

                var method = entity.TryGetBaseMethod(funcName);
                if (method == null)
                {
                    LoggerHelper.Warning("RPCCall: Can not find function: " + funcName);
                    return;
                }


                var call = new RpcCallPluto { svrMethod = method };
                var result = call.Encode(args);
                m_tcpWorker.Send(result);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("Rpc call error: " + funcName + " " + ex.Message);
            }
        }

        public override void Login(params string[] args)
        {
            try
            {
                //if (!m_tcpWorker.Connected())
                //    LoggerHelper.Error("Not connected!!");
                var u = new LoginPluto();
                var result = u.Encode(args);
                m_tcpWorker.Send(result);
            }
            catch (Exception ex)
            {
                LoggerHelper.Except(ex);
            }
        }

        public override void Login(string _passport, string _password, string _loginArgs)
        {
            try
            {
                //if (!m_tcpWorker.Connected())
                //    LoggerHelper.Error("Not connected!!");
                var u = new LoginPluto();
                var result = u.Encode(_passport, _password, _loginArgs);
                m_tcpWorker.Send(result);
            }
            catch (Exception ex)
            {
                LoggerHelper.Except(ex);
            }
        }

        public override void BaseLogin(String token)
        {
            var u = new BaseLoginPluto();
            var result = u.Encode(token);
            m_tcpWorker.Send(result);
        }

        public override void Move(byte face, ushort x, ushort y, ushort z)
        {
            try
            {
                var u = new MovePluto();
                var result = u.Encode(face, x, y, z);
                m_tcpWorker.Send(result);
            }
            catch (Exception ex)
            {
                LoggerHelper.Except(ex);
            }
        }

        public override void SendReConnectKey(string key)
        {
            try
            {
                var u = new ReConnectPluto();
                var result = u.Encode(key);
                m_tcpWorker.Send(result);
            }
            catch (Exception ex)
            {
                LoggerHelper.Except(ex);
            }
        }

        public override void CheckDefMD5(Byte[] bytes)
        {
            try
            {
                var u = new CheckDefMD5Pluto();
                var result = u.Encode(bytes);
                m_tcpWorker.Send(result);
            }
            catch (Exception ex)
            {
                LoggerHelper.Except(ex);
            }
        }

        public override void Process()
        {
            m_tcpWorker.Process();
            if (m_serverWorker != null)
            {
                m_serverWorker.Process();
            }
        }

        // public static int count = 0;

        /// <summary>
        /// 监听远程回调。
        /// </summary>
        public override void Update()
        {
            if (m_tcpWorker.Connected())
            {
                var hasRecieved = 0;
                while (hasRecieved < MAX_PACKETS_PER_FRAME)
                {
                    var data = m_tcpWorker.Recv();
                    if (data == null || data.Length == 0)
                    {
                        break;
                    }
                    else
                    {
                        //count++;
                        OnDataReceive(data);
                        hasRecieved++;
                    }
                }
            }
            if (RPCMsgLogManager.IsRecord)
            {
                RPCMsgLogManager.DoRecord();
            }
        }

        public override void Release()
        {
            m_tcpWorker.Release();
        }

        private void OnDataReceive(byte[] data)
        {
            //if (MogoWorldEx.isKicked) return;
            try
            {
                var pluto = Pluto.Decode(data);
                pluto.HandleData();
                //temp 以下代码需放在PRCCallPluto里面
                //if (!String.IsNullOrEmpty(pluto.FuncName) && pluto.Arguments != null)
                //    EventDispatcher.TriggerEvent<object[]>(pluto.FuncName, pluto.Arguments);
                //else
                //    LoggerHelper.Warning(String.Format("Null function in RpcCallPluto."));
            }
            catch (Exception ex)
            {
                LoggerHelper.Except(ex);
            }
        }
    }
}