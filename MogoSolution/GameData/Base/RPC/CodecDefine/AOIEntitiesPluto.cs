using System;
using System.Collections.Generic;
using Mogo.Util;

namespace Mogo.RPC
{
    /// <summary>
    ///     一组entity数据
    /// </summary>
    public class AOIEntitiesPluto : Pluto
    {
        public byte[] ServerEncode(List<CellAttachedInfo> list)
        {
            Push(VUInt16.Instance.Encode(MSGIDType.CLIENT_AOI_ENTITIES));
            for (int i = 0; i < list.Count; i++)
            {
                var info = list[i];
                var bytes = new List<byte>();

                bytes.AddRange(VUInt16.Instance.Encode(info.typeId));
                bytes.AddRange(VUInt32.Instance.Encode(info.id));
                bytes.AddRange(VUInt8.Instance.Encode(info.face));
                bytes.AddRange(VUInt16.Instance.Encode(info.x));
                bytes.AddRange(VUInt16.Instance.Encode(info.y));
                bytes.AddRange(VUInt16.Instance.Encode(info.z));
                if (info.props != null)
                    for (int j = 0; j < info.props.Count; j++)
                    {
                        var prop = info.props[j];
                        bytes.AddRange(VUInt16.Instance.Encode(CurrentEntity.PropertiesIdMapping[prop.Property.Name]));
                        bytes.AddRange(prop.Property.VType.Encode(prop.Value));
                    }

                Push(VUInt16.Instance.Encode(bytes.Count));
                Push(bytes.ToArray());
            }

            var result = new Byte[m_unLen];
            Buffer.BlockCopy(m_szBuff, 0, result, 0, m_unLen);
            EndPluto(result);
            return result;
        }


        protected override void DoDecode(byte[] data, ref int unLen)
        {
            /*[UINT16][UINT16][UINT32][UINT8][INT16][INT16] [UINT16]  [UINT16][UINT32][UINT8][INT16][INT16] [....]  [UINT16]  [UINT16][UINT32][UINT8][INT16][INT16] [....] ...
               总长度   etype   eid     face    x      y    单个aoi长度 etype    eid     face   x       y     属性    单个aoi长度 etype    eid     face   x       y     属性
            */
            var list = new List<CellAttachedInfo>();
            while (unLen < data.Length)
            {
                var entityInfo = new CellAttachedInfo();
                var entityLength = (ushort)VUInt16.Instance.Decode(data, ref unLen); //单个entity数据总长度
                var endIdx = unLen + entityLength; //结束位置
                entityInfo.typeId = (ushort)VUInt16.Instance.Decode(data, ref unLen);
                entityInfo.id = (uint)VUInt32.Instance.Decode(data, ref unLen); // eid
                entityInfo.face = (byte)VUInt8.Instance.Decode(data, ref unLen);// rotation
                var entityX = (UInt16)VUInt16.Instance.Decode(data, ref unLen);
                var entityY = (UInt16)VUInt16.Instance.Decode(data, ref unLen);
                var entityZ = (UInt16)VUInt16.Instance.Decode(data, ref unLen);
                entityInfo.x = (short)entityX;
                entityInfo.y = (short)entityY;
                entityInfo.z = (short)entityZ;
                var entity = DefParser.Instance.GetEntityByID(entityInfo.typeId);
                if (entity != null)
                {
                    entityInfo.entity = entity;
                    entityInfo.props = new List<EntityPropertyValue>();
                    while (unLen < endIdx)
                    {
                        //还有数据就解析
                        var index = VUInt16.Instance.Decode(data, ref unLen);
                        EntityDefProperties prop;
                        var flag = entity.Properties.TryGetValue((ushort)index, out prop);
                        if (flag)
                            entityInfo.props.Add(new EntityPropertyValue(prop, prop.VType.Decode(data, ref unLen)));
                    }
                }
                list.Add(entityInfo);
            }
            Arguments = new Object[1];
            Arguments[0] = list;
            //LoggerHelper.Debug("list: " + list.Count);
            if (RPCMsgLogManager.IsRecord)
            {
                RPCMsgLogManager.Receive(MSGIDType.CLIENT_AOI_ENTITIES, list.PackList());
            }
        }

        public override void HandleData()
        {
            if (Arguments == null || Arguments.Length == 0)
            {
                return;
            }
            var list = (List<CellAttachedInfo>)Arguments[0];
            foreach (var info in list)
            {
                EventDispatcher.TriggerEvent(Events.FrameWorkEvent.AOINewEntity, info);
            }
        }

        /// <summary>
        ///     创建新EntityAttachedPluto实例。
        /// </summary>
        /// <returns>EntityAttachedPluto实例</returns>
        internal static Pluto Create()
        {
            return new AOIEntitiesPluto();
        }
    }
}