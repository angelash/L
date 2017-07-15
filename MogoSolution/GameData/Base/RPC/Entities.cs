#region 模块信息

/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：AttachedEntity
// 创建者：Ash Tang
// 修改者列表：
// 创建日期：2013.4.17
// 模块描述：实体获取数据结构定义。
//----------------------------------------------------------------*/

#endregion

using System;
using System.Collections.Generic;
using Mogo.Util;
using UnityEngine;

namespace Mogo.RPC
{
    public class AttachedInfo
    {
        public uint id { get; set; }
        public ushort typeId { get; set; }
        public EntityDef entity { get; set; }
        public List<EntityPropertyValue> props { get; set; }

        public override string ToString()
        {
            return String.Format("ai-id:{0},type:{1},ety:{2},props:{3}", id, typeId, entity,
                props == null ? null : props.PackList());
        }
    }

    public class CellAttachedInfo : AttachedInfo
    {
        public byte face { get; set; }
        public short x { get; set; }
        public short y { get; set; }
        public short z { get; set; }
        public uint checkFlag { get; set; }

        public void SetData(ushort _typeId, uint _id, List<EntityPropertyValue> _props = null)
        {
            typeId = _typeId;
            id = _id;
            props = _props;
        }

        public void SetProp(uint _id, List<EntityPropertyValue> _props)
        {
            id = _id;
            props = _props;
        }

        public Vector3 position
        {
            get { return new Vector3(x * 0.01f, y * 0.01f, z * 0.01f); }
            set
            {
                x = (short)(value.x * 100);
                y = (short)(value.y * 100);
                z = (short)(value.z * 100);
            }
        } //服务器坐标以厘米为单位，客户端以米为单位
        public UnityEngine.Vector3 rotation { get { return new UnityEngine.Vector3(0, face * 2, 0); } }//服务器朝向值为0-180，客户端直接放大一倍

        public override string ToString()
        {
            return String.Format("cai-id:{0},type:{1},ety:{2},x:{3},y:{4},cf:{5},pos:{6},props:{7}", id, typeId, entity,
                x, y, checkFlag, position, props == null ? null : props.PackList());
        }
    }

    public class BaseAttachedInfo : AttachedInfo
    {
        public ulong dbid { get; set; }

        public override string ToString()
        {
            return String.Format("bai-id:{0},type:{1},ety:{2},dbid:{3},props:{4}", id, typeId, entity, dbid,
                props == null ? null : props.PackList());
        }
    }

    public class EntityPropertyValue
    {
        public EntityPropertyValue(EntityDefProperties property, object value)
        {
            Property = property;
            Value = value;
        }

        public EntityDefProperties Property { get; set; }
        public object Value { get; set; }

        public override string ToString()
        {
            if (Property.VType == VBLOB.Instance)
                return String.Format("{0}:{1}", Property, (Value as byte[]).PackArray());
            return String.Format("{0}:{1}", Property, Value);
        }
    }
}