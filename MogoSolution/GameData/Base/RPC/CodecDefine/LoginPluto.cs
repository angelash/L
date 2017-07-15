using System;
using Mogo.Util;

namespace Mogo.RPC
{
    public class LoginPluto : Pluto
    {
        /// <summary>
        ///     将远程方法调用编码为二进制数组。
        /// </summary>
        /// <param name="passport">登录帐号</param>
        /// <param name="password">登录密码</param>
        /// <param name="loginArgs">登录类型参数</param>
        /// <returns>编码后的二进制数组</returns>
        public Byte[] Encode(String passport, String password, String loginArgs)
        {
            Push(VUInt16.Instance.Encode(MSGIDType.LOGINAPP_LOGIN));
            Push(VString.Instance.Encode(passport));
            Push(VString.Instance.Encode(password));
            Push(VString.Instance.Encode(loginArgs));

            var result = new Byte[m_unLen];
            Buffer.BlockCopy(m_szBuff, 0, result, 0, m_unLen);
            EndPluto(result);
            if (RPCMsgLogManager.IsRecord)
            {
                RPCMsgLogManager.Send(MSGIDType.LOGINAPP_LOGIN, passport, loginArgs);
            }
            return result;
        }

        /// <summary>
        ///     将远程方法调用编码为二进制数组。
        /// </summary>
        /// <returns>编码后的二进制数组</returns>
        public Byte[] Encode(params string[] args)
        {
            Push(VUInt16.Instance.Encode(MSGIDType.LOGINAPP_LOGIN));
            foreach (var item in args)
            {
                Push(VString.Instance.Encode(item));
            }

            var result = new Byte[m_unLen];
            Buffer.BlockCopy(m_szBuff, 0, result, 0, m_unLen);
            EndPluto(result);
            if (RPCMsgLogManager.IsRecord)
            {
                RPCMsgLogManager.Send(MSGIDType.LOGINAPP_LOGIN, args.PackArray());
            }
            return result;
        }

        /// <summary>
        ///     将远程调用的方法解码为Login调用。
        /// </summary>
        /// <param name="data">远程调用方法的二进制数组</param>
        /// <param name="unLen">数据偏移量</param>
        protected override void DoDecode(byte[] data, ref int unLen)
        {
            FuncName = MSGIDType.CLIENT_LOGIN_RESP.ToString();

            Arguments = new Object[1];
            Arguments[0] = VString.Instance.Decode(data, ref unLen); 
            if (RPCMsgLogManager.IsRecord)
            {
                RPCMsgLogManager.Receive(MSGIDType.LOGINAPP_LOGIN, Arguments);
            }
        }

        public override void HandleData()
        {
            var result = (LoginResult) (byte) Arguments[0];
            EventDispatcher.TriggerEvent(Events.FrameWorkEvent.Login, result);
        }

        /// <summary>
        ///     创建新LoginPluto实例。
        /// </summary>
        /// <returns>LoginPluto实例</returns>
        internal static Pluto Create()
        {
            return new LoginPluto();
        }
    }
}