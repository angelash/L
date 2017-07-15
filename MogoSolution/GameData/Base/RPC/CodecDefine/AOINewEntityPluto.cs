using System;
using System.Collections.Generic;
using Mogo.Util;

namespace Mogo.RPC
{
    public class AOINewEntityPluto : Pluto
    {
        public byte[] ServerEncode(CellAttachedInfo info)
        {
            Push(VUInt16.Instance.Encode(MSGIDType.CLIENT_AOI_NEW_ENTITY));
            Push(VUInt16.Instance.Encode(info.typeId));
            Push(VUInt32.Instance.Encode(info.id));
            Push(VUInt8.Instance.Encode(info.face));
            Push(VUInt16.Instance.Encode(info.x));
            Push(VUInt16.Instance.Encode(info.y));
            Push(VUInt16.Instance.Encode(info.z));
            if (info.props != null)
                for (int i = 0; i < info.props.Count; i++)
                {
                    var prop = info.props[i];
                    Push(VUInt16.Instance.Encode(CurrentEntity.PropertiesIdMapping[prop.Property.Name]));
                    Push(prop.Property.VType.Encode(prop.Value));
                }

            var result = new Byte[m_unLen];
            Buffer.BlockCopy(m_szBuff, 0, result, 0, m_unLen);
            EndPluto(result);
            return result;
        }

        protected override void DoDecode(byte[] data, ref int unLen)
        {
            var info = new CellAttachedInfo();
            info.typeId = (ushort)VUInt16.Instance.Decode(data, ref unLen);
            info.id = (uint)VUInt32.Instance.Decode(data, ref unLen); // eid
            info.face = (byte)VUInt8.Instance.Decode(data, ref unLen);// rotation
            var x = (UInt16)VUInt16.Instance.Decode(data, ref unLen);
            var y = (UInt16)VUInt16.Instance.Decode(data, ref unLen);
            var z = (UInt16)VUInt16.Instance.Decode(data, ref unLen);
            info.x = (short)x;
            info.y = (short)y;
            info.z = (short)z;
            var entity = DefParser.Instance.GetEntityByID(info.typeId);
            if (entity != null)
            {
                info.entity = entity;
                info.props = new List<EntityPropertyValue>();
                while (unLen < data.Length)
                {
                    //还有数据就解析
                    var index = VUInt16.Instance.Decode(data, ref unLen);
                    EntityDefProperties prop;
                    var flag = entity.Properties.TryGetValue((ushort)index, out prop);
                    if (flag)
                        info.props.Add(new EntityPropertyValue(prop, prop.VType.Decode(data, ref unLen)));
                }
            }
            Arguments = new object[1] { info };
            if (RPCMsgLogManager.IsRecord)
            {
                RPCMsgLogManager.Receive(MSGIDType.CLIENT_AOI_NEW_ENTITY, Arguments);
            }
        }

        public override void HandleData()
        {
            var info = Arguments[0] as CellAttachedInfo;
            EventDispatcher.TriggerEvent(Events.FrameWorkEvent.AOINewEntity, info);
        }

        internal static Pluto Create()
        {
            return new AOINewEntityPluto();
        }
    }
}