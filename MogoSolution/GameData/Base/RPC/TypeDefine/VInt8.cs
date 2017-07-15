#region 模块信息

/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：VInt8
// 创建者：Ash Tang
// 修改者列表：
// 创建日期：2013.1.16
// 模块描述：8位整数（char）。
//----------------------------------------------------------------*/

#endregion

using System;
using System.Runtime.InteropServices;

namespace Mogo.RPC
{
    /// <summary>
    ///     8位整数（char）。
    /// </summary>
    public class VInt8 : VObject
    {
        private static readonly VInt8 m_instance = new VInt8();

        public VInt8()
            : base(typeof(sbyte), VType.V_INT8, 1)//Marshal.SizeOf(typeof (sbyte))
        {
        }

        public VInt8(Object vValue)
            : base(typeof (sbyte), VType.V_INT8, vValue)
        {
        }

        public static VInt8 Instance
        {
            get { return m_instance; }
        }

        public override byte[] Encode(object vValue)
        {
            var b = (byte) Convert.ToSByte(vValue);
            return new Byte[1] {b};
        }

        public override Object Decode(byte[] data, ref Int32 index)
        {
            //Byte[] result = new Byte[1];
            Buffer.BlockCopy(data, index, result, 0, 1);
            index += 1;

            return Convert.ToSByte(result[0]);
        }
    }
}