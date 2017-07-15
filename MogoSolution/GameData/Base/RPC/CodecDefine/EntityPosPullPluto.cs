using System;

namespace Mogo.RPC
{
    internal class EntityPosPullPluto : Pluto
    {
        protected override void DoDecode(byte[] data, ref int unLen)
        {
            var info = new CellAttachedInfo();
            //info.id = (uint)VUInt32.Instance.Decode(data, ref unLen); // eid
            //info.face = (byte)VUInt8.Instance.Decode(data, ref unLen);// rotation
            //info.x = (short)VInt16.Instance.Decode(data, ref unLen); //x
            //info.y = (short)VInt16.Instance.Decode(data, ref unLen); //y
            var x = (UInt16) VUInt16.Instance.Decode(data, ref unLen);
            var y = (UInt16) VUInt16.Instance.Decode(data, ref unLen);
            info.x = (short) x;
            info.y = (short) y;

            Arguments = new Object[1];
            Arguments[0] = info;
            if (RPCMsgLogManager.IsRecord)
            {
                RPCMsgLogManager.Receive(MSGIDType.CLIENT_ENTITY_POS_PULL, Arguments);
            }
        }

        public override void HandleData()
        {
            //MogoWorldEx.thePlayer.SetEntityCellInfo(Arguments[0] as CellAttachedInfo);
        }

        internal static Pluto Create()
        {
            return new EntityPosPullPluto();
        }
    }
}