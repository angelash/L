using System;
using Mogo.Util;

namespace Mogo.RPC
{
    public class CheckDefMD5Pluto : Pluto
    {
        public Byte[] Encode(Byte[] bytes)
        {
            Push(VUInt16.Instance.Encode(MSGIDType.LOGINAPP_CHECK));
            var str = Util.Utils.FormatMD5(bytes);
            Push(VString.Instance.Encode(str));

            var result = new Byte[m_unLen];
            Buffer.BlockCopy(m_szBuff, 0, result, 0, m_unLen);
            EndPluto(result);
            if (RPCMsgLogManager.IsRecord)
            {
                RPCMsgLogManager.Send(MSGIDType.LOGINAPP_CHECK, str);
            }
            return result;
        }

        protected override void DoDecode(byte[] data, ref int unLen)
        {
            FuncName = MSGIDType.CLIENT_CHECK_RESP.ToString();

            Arguments = new Object[1];
            Arguments[0] = VUInt8.Instance.Decode(data, ref unLen);
            if (RPCMsgLogManager.IsRecord)
            {
                RPCMsgLogManager.Receive(MSGIDType.CLIENT_CHECK_RESP, Arguments);
            }
        }

        public override void HandleData()
        {
            var result = (DefCheckResult) (byte) Arguments[0];
            EventDispatcher.TriggerEvent(Events.FrameWorkEvent.CheckDef, result);
        }

        internal static Pluto Create()
        {
            return new CheckDefMD5Pluto();
        }
    }
}