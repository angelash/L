using System;
using Mogo.Util;

namespace Mogo.RPC
{
    public class AOIDelEntityPluto : Pluto
    {
        public byte[] ServerEncode(CellAttachedInfo info)
        {
            Push(VUInt16.Instance.Encode(MSGIDType.CLIENT_AOI_DEL_ENTITY));
            Push(VUInt32.Instance.Encode(info.id));

            var result = new Byte[m_unLen];
            Buffer.BlockCopy(m_szBuff, 0, result, 0, m_unLen);
            EndPluto(result);
            return result;
        }
        protected override void DoDecode(byte[] data, ref int unLen)
        {
            Arguments = new Object[1];
            Arguments[0] = VUInt32.Instance.Decode(data, ref unLen);
            if (RPCMsgLogManager.IsRecord)
            {
                RPCMsgLogManager.Receive(MSGIDType.CLIENT_AOI_DEL_ENTITY, Arguments);
            }
        }

        public override void HandleData()
        {
            var entityID = (UInt32) Arguments[0];
            EventDispatcher.TriggerEvent(Events.FrameWorkEvent.AOIDelEvtity, entityID);
        }

        internal static Pluto Create()
        {
            return new AOIDelEntityPluto();
        }
    }
}