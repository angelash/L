#region 模块信息

/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：Pluto
// 创建者：Ash Tang
// 修改者列表：
// 创建日期：2013.1.16
// 模块描述：数据包编解码处理类。
//----------------------------------------------------------------*/

#endregion

using System;
using System.Collections.Generic;
using Mogo.Util;
using UnityEngine;
using Object = System.Object;

#if UNITY_EDITOR
public class RpcEvent
{
    public const string DecodeRPC = "DecodeRPC";    
    public const string SendRPC = "SendRPC";    
}
#endif

namespace Mogo.RPC
{
    /// <summary>
    ///     数据包编解码处理类。
    /// </summary>
    public abstract class Pluto
    {
        //protected static BitCryto encryto = new BitCryto(new short[] { 0, 0 });

        private const Int32 DEFAULT_PLUTO_BUFF_SIZE = 1024;

        //protected static BitCryto encryto =
        //    new BitCryto(new short[] {15, 180, 213, 37, 40, 98, 85, 7, 152, 223, 48, 168, 71, 102, 191, 194});

        public static Func<uint, EntityDef> GetEntity;
        private static readonly Dictionary<MSGIDType, Pluto> msgIdToPluto = new Dictionary<MSGIDType, Pluto>();
        protected Byte[] m_szBuff;
        protected Int32 m_unLen;
        private Int32 m_unMaxLen;

        /// <summary>
        ///     默认构造函数
        /// </summary>
        protected Pluto()
        {
            m_szBuff = new Byte[DEFAULT_PLUTO_BUFF_SIZE];
            m_unMaxLen = DEFAULT_PLUTO_BUFF_SIZE;
        }

        /// <summary>
        ///     方法标识
        /// </summary>
        public ushort EntityID { get; set; }

        /// <summary>
        ///     实体名称
        /// </summary>
        public String EntityName { get; set; }

        /// <summary>
        ///     方法标识
        /// </summary>
        public ushort FuncID { get; set; }

        /// <summary>
        ///     方法名称
        /// </summary>
        public String FuncName { get; set; }

        /// <summary>
        ///     参数列表
        /// </summary>
        public Object[] Arguments { get; protected set; }

        public Byte[] Content
        {
            get
            {
                //var result = new Byte[m_unLen];
                //Buffer.BlockCopy(m_szBuff, 0, result, 0, m_unLen);
                return m_szBuff;
            }
        }

        /// <summary>
        ///     当前实体对象
        /// </summary>
        public static EntityDef CurrentEntity { get; set; }

        /// <summary>
        ///     处理解码数据。
        /// </summary>
        public abstract void HandleData();

        /// <summary>
        ///     将远程调用的方法解码为具体的方法调用。
        /// </summary>
        /// <param name="data">远程调用方法的二进制数组</param>
        /// <param name="unLen">数据偏移量</param>
        protected abstract void DoDecode(Byte[] data, ref int unLen);

        private static Pluto GetPluto(MSGIDType msgId)
        {
            if (msgIdToPluto.ContainsKey(msgId))
            {
                return msgIdToPluto[msgId];
            }
            Pluto pluto;
            switch (msgId)
            {
                case MSGIDType.BASEAPP_CLIENT_MOVE_REQ:
                    pluto = MovePluto.Create();
                    break;
                case MSGIDType.BASEAPP_CLIENT_RPCALL:
                case MSGIDType.CLIENT_RPC_RESP:
                    pluto = RpcCallPluto.Create();
                    break;
                case MSGIDType.LOGINAPP_LOGIN:
                case MSGIDType.CLIENT_LOGIN_RESP:
                    pluto = LoginPluto.Create();
                    break;
                case MSGIDType.CLIENT_NOTIFY_ATTACH_BASEAPP:
                    pluto = BaseLoginPluto.Create();
                    break;
                case MSGIDType.CLIENT_ENTITY_ATTACHED:
                    pluto = EntityAttachedPluto.Create();
                    break;
                case MSGIDType.CLIENT_AOI_NEW_ENTITY:
                    pluto = AOINewEntityPluto.Create();
                    break;
                case MSGIDType.CLIENT_AOI_DEL_ENTITY:
                    pluto = AOIDelEntityPluto.Create();
                    break;
                case MSGIDType.CLIENT_OTHER_ENTITY_POS_SYNC:
                    pluto = OtherEntityPosSyncPluto.Create();
                    break;
                case MSGIDType.CLIENT_OTHER_ENTITY_POS_PULL:
                    pluto = OtherEntityPosPullPluto.Create();
                    break;
                case MSGIDType.CLIENT_OTHER_ENTITY_TELEPORT:
                    pluto = OtherEntityPosTeleportPluto.Create();
                    break;
                case MSGIDType.CLIENT_ENTITY_CELL_ATTACHED:
                    pluto = EntityCellAttachedPluto.Create();
                    break;
                case MSGIDType.CLIENT_AOI_ENTITIES:
                    pluto = AOIEntitiesPluto.Create();
                    break;
                case MSGIDType.CLIENT_AVATAR_ATTRI_SYNC:
                    pluto = AvatarAttriSyncPluto.Create();
                    break;
                case MSGIDType.CLIENT_OTHER_ENTITY_ATTRI_SYNC:
                    pluto = OtherAttriSyncPluto.Create();
                    break;
                case MSGIDType.CLIENT_ENTITY_POS_SYNC:
                    pluto = EntityPosSyncPluto.Create();
                    break;
                case MSGIDType.CLIENT_ENTITY_POS_PULL:
                    pluto = EntityPosPullPluto.Create();
                    break;
                case MSGIDType.CLIENT_ENTITY_POS_TELEPORT:
                    pluto = EntityPosTeleportPluto.Create();
                    break;
                case MSGIDType.CLIENT_CHECK_RESP:
                    pluto = CheckDefMD5Pluto.Create();
                    break;
                case MSGIDType.BASEAPP_CLIENT_REFUSE_RELOGIN:
                    pluto = ReConnectRefusePluto.Create();
                    break;
                case MSGIDType.NOTIFY_CLIENT_DEFUSE_LOGIN:
                    pluto = DefuseLoginPluto.Create();
                    break;
                default:
                    pluto = NotImplementedPluto.Create();
                    break;
            }
            msgIdToPluto[msgId] = pluto;
            return pluto;
        }

        ///// <summary>
        ///// 将远程调用的方法解码为具体的方法调用。
        ///// </summary>
        ///// <param name="data">远程调用方法的二进制数组</param>
        public static Pluto Decode(Byte[] data)
        {
            var unLen = 0;
            Pluto pluto;
            var msgId = (MSGIDType)VUInt16.Instance.Decode(data, ref unLen);
            //encryto.Reset();
            //var len = data.Length;
            //for (var i = unLen; i < len; ++i)
            //{
            //    data[i] = encryto.Decode(data[i]);
            //}
            //Profiler.BeginSample("DoDecode " + msgId);
            //LoggerHelper.Info("msgid: " + msgId);
            pluto = GetPluto(msgId);

            pluto.DoDecode(data, ref unLen);
            pluto.m_szBuff = data;
            pluto.m_unLen = unLen;
            //Profiler.EndSample();
            return pluto;
        }

        protected void Push(Byte[] data)
        {
            if (m_unLen + data.Length > m_unMaxLen)
            {
                m_unMaxLen = m_unMaxLen + DEFAULT_PLUTO_BUFF_SIZE;
                var newArray = new Byte[m_unMaxLen];
                Buffer.BlockCopy(m_szBuff, 0, newArray, 0, m_unLen);
                m_szBuff = newArray;
            }
            Buffer.BlockCopy(data, 0, m_szBuff, m_unLen, data.Length);
            m_unLen += data.Length;
        }

        protected void EndPluto(byte[] bytes)
        {
            //encryto.Reset();
            //var len = bytes.Length;
            //for (var i = 2; i < len; i++)
            //{
            //    bytes[i] = encryto.Encode(bytes[i]);
            //}
        }
    }
}