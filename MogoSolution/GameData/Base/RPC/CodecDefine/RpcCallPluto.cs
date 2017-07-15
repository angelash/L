using System;
using System.Collections.Generic;
using Mogo.Util;

namespace Mogo.RPC
{
    public class RpcCallPluto : Pluto
    {
        public EntityDefMethod svrMethod;

        /// <summary>
        ///     将远程方法调用编码为二进制数组。
        /// </summary>
        /// <param name="args">参数列表</param>
        /// <returns>编码后的二进制数组</returns>
        public Byte[] Encode(params Object[] args)
        {
            var entityDef = CurrentEntity; //DefParser.Instance.GetEntityByName(EntityName);
            if (entityDef == null)
            {
                throw new DefineParseException(String.Format("Encode error: CurrentEntity is null."));
            }

            var method = svrMethod;
            FuncID = method.FuncID;
            FuncName = method.FuncName;

            if (method.ArgsType.Count != args.Length)
            {
                throw new DefineParseException(
                    String.Format(
                        "Encode error: The number of parameters is not match. func: {0}, require num: {1}, current num: {2}.",
                        FuncID, method.ArgsType.Count, args.Length));
            }
            Push(VUInt16.Instance.Encode(MSGIDType.BASEAPP_CLIENT_RPCALL)); // 指定 pluto 类型为 rpc
            Push(VUInt16.Instance.Encode(FuncID)); // 指定 调用的 func 标识
            for (var i = 0; i < args.Length; i++)
            {
                Push(method.ArgsType[i].Encode(args[i])); // 增加参数
            }

            var result = new Byte[m_unLen];
            Buffer.BlockCopy(m_szBuff, 0, result, 0, m_unLen);
            EndPluto(result);
            if (RPCMsgLogManager.IsRecord)
            {
                var list = new List<object>(2 + args.Length);
                list.Add(FuncID);
                list.Add(FuncName);
                list.AddRange(args);
                RPCMsgLogManager.Send(MSGIDType.BASEAPP_CLIENT_RPCALL, list.ToArray());
            }
            return result;
        }

        /// <summary>
        ///     将远程调用的方法解码为RpcCall调用。
        /// </summary>
        /// <param name="data">远程调用方法的二进制数组</param>
        /// <param name="unLen">数据偏移量</param>
        protected override void DoDecode(byte[] data, ref int unLen)
        {
            FuncID = (ushort) VUInt16.Instance.Decode(data, ref unLen);
            if (CurrentEntity == null)
                throw new DefineParseException(String.Format("Decode error: Current Entity is not set."));
            var method = CurrentEntity.TryGetClientMethod(FuncID);
            if (method == null)
                throw new DefineParseException(
                    String.Format("Decode error: Can not find function '{0}' in entity '{1}'.", FuncID,
                        CurrentEntity.Name));

            FuncName = method.FuncName;
            Arguments = new Object[method.ArgsType.Count];
            for (var i = 0; i < method.ArgsType.Count; i++)
            {
                Arguments[i] = method.ArgsType[i].Decode(data, ref unLen);
            }
            if (RPCMsgLogManager.IsRecord)
            {
                var list = new List<object>(2 + Arguments.Length);
                list.Add(FuncID);
                list.Add(FuncName);
                for (var i = 0; i < Arguments.Length; i++)
                {
                    if (method.ArgsType[i].VType == VType.V_BLOB)
                    {
                        list.Add(Arguments[i] != null ? (Arguments[i] as byte[]).PackArray() : null);
                    }
                    else
                    {
                        list.Add(Arguments[i]);
                    }
                }
                RPCMsgLogManager.Receive(MSGIDType.CLIENT_RPC_RESP, list.ToArray());
            }
        }

        public override void HandleData()
        {
            if (!String.IsNullOrEmpty(FuncName) && Arguments != null)
            {
                EventDispatcher.TriggerEvent(Util.Utils.RPC_HEAD + FuncName, Arguments);
            }
        }

        /// <summary>
        ///     创建新RpcCallPluto实例。
        /// </summary>
        /// <returns>RpcCallPluto实例</returns>
        internal static Pluto Create()
        {
            return new RpcCallPluto();
        }
    }
}