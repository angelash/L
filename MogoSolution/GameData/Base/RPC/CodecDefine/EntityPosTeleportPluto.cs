using System;
using System.Text;
using Mogo.Util;

namespace Mogo.RPC
{
    public class EntityPosTeleportPluto : Pluto
    {
        private readonly CellAttachedInfo info = new CellAttachedInfo();

        public byte[] ServerEncode(CellAttachedInfo info)
        {
            Push(VUInt16.Instance.Encode(MSGIDType.CLIENT_ENTITY_POS_TELEPORT));
            Push(VUInt8.Instance.Encode(info.face));
            Push(VUInt16.Instance.Encode(info.x));
            Push(VUInt16.Instance.Encode(info.y));
            Push(VUInt16.Instance.Encode(info.z));

            var result = new Byte[m_unLen];
            Buffer.BlockCopy(m_szBuff, 0, result, 0, m_unLen);
            EndPluto(result);
            return result;
        }

        protected override void DoDecode(byte[] data, ref int unLen)
        {
            info.face = (byte)VUInt8.Instance.Decode(data, ref unLen);
            var x = (UInt16)VUInt16.Instance.Decode(data, ref unLen);
            var y = (UInt16)VUInt16.Instance.Decode(data, ref unLen);
            var z = (UInt16)VUInt16.Instance.Decode(data, ref unLen);
            info.x = (short)x;
            info.y = (short)y;
            info.z = (short)z;

            Arguments = new object[1] { info };
            if (RPCMsgLogManager.IsRecord)
            {
                RPCMsgLogManager.Receive(MSGIDType.CLIENT_ENTITY_POS_TELEPORT, Arguments);
            }
        }

        public override void HandleData()
        {
//用于同场景跳点传送
            var info = Arguments[0] as CellAttachedInfo;
            EventDispatcher.TriggerEvent(Events.FrameWorkEvent.EntityPosTeleport, info);
        }

        internal static Pluto Create()
        {
            return new EntityPosTeleportPluto();
        }
    }
}