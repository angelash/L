using System.Collections.Generic;
using Mogo.Util;
using System;

namespace Mogo.RPC
{
    public class OtherAttriSyncPluto : Pluto
    {
        public byte[] ServerEncode(CellAttachedInfo info)
        {
            Push(VUInt16.Instance.Encode(MSGIDType.CLIENT_OTHER_ENTITY_ATTRI_SYNC));
            Push(VUInt32.Instance.Encode(info.id));
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
            var info = new AttachedInfo();
            info.id = (uint) VUInt32.Instance.Decode(data, ref unLen); //entity unique id
            info.props = new List<EntityPropertyValue>();
            if (GetEntity != null)
            {
                var entity = GetEntity(info.id);
                if (entity != null)
                    while (unLen < data.Length)
                    {
//还有数据就解析
                        var index = VUInt16.Instance.Decode(data, ref unLen);
                        EntityDefProperties prop;
                        var flag = entity.Properties.TryGetValue((ushort) index, out prop);
                        if (flag)
                            info.props.Add(new EntityPropertyValue(prop, prop.VType.Decode(data, ref unLen)));
                    }
            }
            Arguments = new object[1] {info};
            if (RPCMsgLogManager.IsRecord)
            {
                RPCMsgLogManager.Receive(MSGIDType.CLIENT_OTHER_ENTITY_ATTRI_SYNC, Arguments);
            }
        }

        public override void HandleData()
        {
            var info = Arguments[0] as AttachedInfo;
            EventDispatcher.TriggerEvent(Events.FrameWorkEvent.OtherAttriSync, info);
        }

        /// <summary>
        ///     创建新EntityAttachedPluto实例。
        /// </summary>
        /// <returns>EntityAttachedPluto实例</returns>
        internal static Pluto Create()
        {
            return new OtherAttriSyncPluto();
        }
    }
}