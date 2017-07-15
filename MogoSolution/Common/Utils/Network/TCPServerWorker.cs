using System;
using System.Net;
using System.Net.Sockets;
using Mogo.RPC;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Mogo.GameData;

namespace Mogo.Util
{
    public class TCPServerWorker
    {

        private static readonly object m_clientListLocker = new object();
        private Thread newt;
        private ManualResetEvent allDone = new ManualResetEvent(false);
        private IPAddress ipAddress;
        private int portNum;
        private bool m_isRunning = true;
        private Socket listener;

        public string Start()
        {
            InitData();
            var ip = UtilsEx.GetIP();
            LoggerHelper.Debug(ip);
            ipAddress = ip;
            portNum = 43998;

            /*创建一个服务器线程并启动*/
            newt = new Thread(new ThreadStart(StartListening));
            newt.IsBackground = true;
            newt.Start();

            return ip.ToString();
        }

        public void Stop()
        {
            m_isRunning = false;
            listener.Close();
        }

        /// <summary>
        /// 临时遍历用客户端列表，依赖Process()每帧刷新，其他地方不要处理
        /// </summary>
        private List<ClientListener> tempList = new List<ClientListener>();

        public void Process()
        {
            tempList.Clear();
            lock (m_clientListLocker)
                tempList.AddRange(allClientList);
            foreach (var item in tempList)
            {
                item.Process();
                while (true)
                {
                    var data = item.Recv();
                    if (data == null || data.Length == 0)
                    {
                        break;
                    }
                    else
                    {
                        OnDataReceive(item, data);
                    }
                }
            }
        }

        /*初始化服务器*/
        private void StartListening()
        {
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, portNum);


            // 创建TCP/IP socket
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //绑定Socket到本地端口和监听输入连接
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);
                LoggerHelper.Debug("Waiting for a connection...");
                while (m_isRunning)
                {
                    allDone.Reset();
                    //结束回调，只能连接一台客户端
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    //手动阻塞
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                LoggerHelper.Except(e);
            }
        }

        /*连接回调*/
        private void AcceptCallback(IAsyncResult ar)
        {
            // 主线程继续
            allDone.Set();

            // 得到客户端Socket请求
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            AddClient(handler);
            LoggerHelper.Debug("Client linked: " + handler.RemoteEndPoint.ToString());
        }

        private void client_Closed(object sender, EventArgs e)
        {
            var c = sender as ClientListener;
            c.ClientData.BelongRoom.RemoveClient(c);
            AOIDelete(c);
            RemoveClient(c);
            //var u = new RpcCallPluto();
            //u.FuncName = "Logout";
            //var result = u.Encode(c.SessionID);
            //lock (m_clientListLocker)
            //    foreach (var item in clientList)
            //    {
            //        item.Send(result);
            //    }
            LoggerHelper.Debug("client_Closed: " + c.ClientData.Id);
        }

        private void OnDataReceive(ClientListener client, Byte[] data)
        {
            var pluto = Pluto.Decode(data);
            var type = pluto.GetType();
            if (type == typeof(LoginPluto))
            {
                HandleLogin(client, pluto);
            }
            else if (type == typeof(MovePluto))
            {
                HandleMove(client, pluto);
            }
            else if (type == typeof(RpcCallPluto))
            {
                HandleRPCCall(client, pluto);
            }
            else
            {
                LoggerHelper.Debug("Unkown type pluto: " + pluto.GetType().ToString());
            }
        }

        #region 客户端管理

        /// <summary>
        /// 实际客户端列表
        /// </summary>
        private List<ClientListener> allClientList = new List<ClientListener>();
        private Dictionary<uint, ClientListener> allClientDic = new Dictionary<uint, ClientListener>();

        private Dictionary<uint, RoomData> m_roomsData = new Dictionary<uint, RoomData>();

        private ushort Avatar = 2;

        private void InitData()
        {
            Pluto.CurrentEntity = DefParser.Instance.GetEntityByID(Avatar);

            RoomData.InitData();
        }

        private void HandleMove(ClientListener client, Pluto pluto)
        {
            if (pluto.Arguments.Length > 0)
            {
                var cell = pluto.Arguments[0] as CellAttachedInfo;
                if (cell != null)
                {
                    //client.ClientData.face = cell.face;
                    //client.ClientData.Position = cell.position;
                    //UpdatePos(client);
                }
            }
            //LoggerHelper.Debug("HandleMove: " + pluto.Arguments.Length);
        }

        private void HandleLogin(ClientListener client, Pluto pluto)
        {
            client = InitClientData(client, pluto.Arguments[0].ToString());

            var eap = new EntityAttachedPluto();
            var res = eap.ServerEncode(new BaseAttachedInfo()
            {
                typeId = Avatar,
                id = client.ClientData.Id,
                dbid = client.ClientData.Dbid,
                props = new List<EntityPropertyValue>() 
                {                    
                    new EntityPropertyValue(GetEntityDefPropertyByName("Name"), client.ClientData.Name),
                }
            });
            client.Send(res);

            //AOINew(client);
            //AOIEntityies(client);
            LoggerHelper.Debug("Client Login: " + pluto.Arguments[0]);
        }

        private void AOINew(ClientListener client)
        {
            var pluto = new AOINewEntityPluto();
            var res = pluto.ServerEncode(GetCellData(client, needName: true));

            for (int i = 0; i < client.ClientData.BelongRoom.MemberList.Count; i++)
            {
                var c = client.ClientData.BelongRoom.MemberList[i];
                if (c != client)
                {
                    c.Send(res);
                }
            }
        }

        private void AOIEntityies(ClientListener client)
        {
            var pluto = new AOIEntitiesPluto();
            var list = new List<CellAttachedInfo>();
            for (int i = 0; i < client.ClientData.BelongRoom.MemberList.Count; i++)
            {
                var c = client.ClientData.BelongRoom.MemberList[i];
                if (c != client)
                {
                    list.Add(GetCellData(c, isNewCell: true, needName: true));
                }
            }

            if (list.Count == 0)
                return;
            var res = pluto.ServerEncode(list);
            //LoggerHelper.Debug("tempList.Count: " + tempList.Count + " list.Count: " + list.Count + " res: " + res.Length);
            client.Send(res);
        }

        private void AOIDelete(ClientListener client)
        {
            var pluto = new AOIDelEntityPluto();
            var res = pluto.ServerEncode(GetCellData(client, false));

            for (int i = 0; i < client.ClientData.BelongRoom.MemberList.Count; i++)
            {
                var c = client.ClientData.BelongRoom.MemberList[i];
                if (c != client)
                {
                    c.Send(res);
                }
            }
        }

        private ClientListener AddClient(Socket handler, bool isDummy = false)
        {
            var client = new ClientListener(handler, isDummy);
            client.ClientData.Id = (uint)Guid.NewGuid().GetHashCode();
            client.ClientData.Dbid = client.ClientData.Id;
            client.Closed += client_Closed;
            lock (m_clientListLocker)
            {
                allClientList.Add(client);
                allClientDic.Add(client.ClientData.Id, client);
            }
            return client;
        }

        private void RemoveClient(ClientListener c)
        {
            c.Closed -= client_Closed;
            lock (m_clientListLocker)
            {
                allClientList.Remove(c);
                allClientDic.Remove(c.ClientData.Id);
            }
        }

        private void UpdatePos(ClientListener client)
        {
            GetCellData(client, false);

            PosSync(client);
        }

        private CellAttachedInfo cellInfoTemplate = new CellAttachedInfo();

        private CellAttachedInfo GetCellData(ClientListener client, bool needProps = true, bool isNewCell = false, bool needName = false)
        {
            CellAttachedInfo cellData;
            if (isNewCell)
            {
                cellData = new CellAttachedInfo();
            }
            else
            {
                cellData = cellInfoTemplate;
            }
            List<EntityPropertyValue> props = null;
            if (needProps)
            {
                props = new List<EntityPropertyValue>()
                {
                    new EntityPropertyValue(GetEntityDefPropertyByName("TeamId"), client.ClientData.TeamId),
                    new EntityPropertyValue(GetEntityDefPropertyByName("MapId"), client.ClientData.MapId),
                    new EntityPropertyValue(GetEntityDefPropertyByName("HeadQuarterId"), client.ClientData.HeadQuarterId),
                };
                if (needName)
                {
                    props.Add(new EntityPropertyValue(GetEntityDefPropertyByName("Name"), client.ClientData.Name));
                }
            }
            cellData.SetData(Avatar, client.ClientData.Id, props);
            return cellData;
        }

        private EntityDefProperties GetEntityDefPropertyByName(string propName)
        {
            var id = Pluto.CurrentEntity.PropertiesIdMapping[propName];
            return Pluto.CurrentEntity.Properties[id];
        }

        private ClientListener InitClientData(ClientListener client, string name)
        {
            client.ClientData.Name = name;
            return client;
        }

        #endregion

        #region RPCCALL

        private void HandleRPCCall(ClientListener client, Pluto pluto)
        {
            switch (pluto.FuncName)
            {
                case "HeartBeat":
                    break;
                case "StartMatch":
                    StartMatch(client, pluto);
                    break;
                case "CreateAvatarResp":
                    CreateAvatar();
                    break;
                default:
                    AOIRPC(client, pluto);
                    break;
            }
            //LoggerHelper.Debug("HandleRPCCall: " + pluto.Arguments.Length);
        }

        private void AOIRPC(ClientListener client, Pluto pluto)
        {
            var res = pluto.Content;

            for (int i = 0; i < client.ClientData.BelongRoom.MemberList.Count; i++)
            {
                client.ClientData.BelongRoom.MemberList[i].Send(res);
                //var c = tempList[i];
                //c.Send(res);
            }
        }

        private List<ClientListener> m_dummies = new List<ClientListener>();

        public void CreateAvatar()
        {
            var client = AddClient(null, true);
            client = InitClientData(client, "dummy");
            m_dummies.Add(client);
            AOINew(client);
        }

        public void StartMatch(ClientListener client, Pluto pluto)
        {
            LoggerHelper.Debug("StartMatch");
            foreach (var item in m_roomsData.Values)
            {
                if (!item.IsBusy)
                {
                    var canStart = item.AddClient(client);
                    var ecap = new EntityCellAttachedPluto();
                    var res = ecap.ServerEncode(GetCellData(client));
                    client.Send(res);
                    AOINew(client);
                    AOIEntityies(client);
                    if (canStart)
                    {
                        StartBattle(client, pluto);
                    }
                    LoggerHelper.Debug("StartMatch " + canStart);
                    return;
                }
            }
            CreatRoom(client, (int)pluto.Arguments[0], (int)pluto.Arguments[1]);
            if ((int)pluto.Arguments[0] == 1)//单人模式
            {
                StartBattle(client, pluto);
            }
        }

        public void StartBattle(ClientListener client, Pluto pluto)
        {
            AOIRPC(client, pluto);
        }

        public void CreatRoom(ClientListener client, int teamCount, int clientCount)
        {
            LoggerHelper.Debug("CreatRoom " + teamCount + " " + clientCount);
            var room = new RoomData(teamCount, clientCount);
            room.Id = (uint)Guid.NewGuid().GetHashCode();
            room.MapId = GetRandomMapId();
            room.AddClient(client);
            var ecap = new EntityCellAttachedPluto();
            var res = ecap.ServerEncode(GetCellData(client));
            client.Send(res);

            m_roomsData.Add(room.Id, room);
        }

        public int GetRandomMapId()
        {
            return 1;
        }

        public void PosSync(ClientListener client)
        {
            var selfPluto = new EntityPosSyncPluto();
            var selfRes = selfPluto.ServerEncode(cellInfoTemplate);
            var otherPluto = new OtherEntityPosSyncPluto();
            var otherRes = otherPluto.ServerEncode(cellInfoTemplate);

            for (int i = 0; i < tempList.Count; i++)
            {
                var c = tempList[i];
                if (c != client)
                {
                    c.Send(otherRes);
                }
                else
                {
                    c.Send(selfRes);
                }
            }
        }

        public void TeleportSync(ClientListener client)
        {
            var selfPluto = new EntityPosTeleportPluto();
            var selfRes = selfPluto.ServerEncode(cellInfoTemplate);
            var otherPluto = new OtherEntityPosTeleportPluto();
            var otherRes = otherPluto.ServerEncode(cellInfoTemplate);

            for (int i = 0; i < tempList.Count; i++)
            {
                var c = tempList[i];
                if (c != client)
                {
                    c.Send(otherRes);
                }
                else
                {
                    c.Send(selfRes);
                }
            }
        }

        public void AttrSync(ClientListener client)
        {
            var selfPluto = new AvatarAttriSyncPluto();
            var selfRes = selfPluto.ServerEncode(cellInfoTemplate);
            var otherPluto = new OtherAttriSyncPluto();
            var otherRes = otherPluto.ServerEncode(cellInfoTemplate);

            for (int i = 0; i < tempList.Count; i++)
            {
                var c = tempList[i];
                if (c != client)
                {
                    c.Send(otherRes);
                }
                else
                {
                    c.Send(selfRes);
                }
            }
        }

        #endregion
    }
}