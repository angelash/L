using System;

namespace Mogo.RPC
{
    internal class OtherEntityPosPullPluto : Pluto
    {
        protected override void DoDecode(byte[] data, ref int unLen)
        {
            var info = new CellAttachedInfo();
            info.id = (uint) VUInt32.Instance.Decode(data, ref unLen); // eid
            //info.face = (byte)VUInt8.Instance.Decode(data, ref unLen);// rotation
            var x = (UInt16) VUInt16.Instance.Decode(data, ref unLen);
            var y = (UInt16) VUInt16.Instance.Decode(data, ref unLen);
            info.x = (short) x;
            info.y = (short) y;

            Arguments = new object[1] {info};
            if (RPCMsgLogManager.IsRecord)
            {
                RPCMsgLogManager.Receive(MSGIDType.CLIENT_OTHER_ENTITY_POS_PULL, Arguments);
            }
        }

        public override void HandleData()
        {
        }

        internal static Pluto Create()
        {
            return new OtherEntityPosPullPluto();
        }
    }
}