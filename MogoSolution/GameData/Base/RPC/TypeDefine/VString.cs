#region 模块信息

/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：VString
// 创建者：Ash Tang
// 修改者列表：
// 创建日期：2013.1.16
// 模块描述：字符串（String）。
//----------------------------------------------------------------*/

#endregion

using System;
using System.Text;

namespace Mogo.RPC
{
    /// <summary>
    ///     字符串（String）。
    /// </summary>
    public class VString : VObject
    {
        private static readonly Encoding m_encoding = Encoding.UTF8;
        private static readonly VString m_instance = new VString();

        public VString()
            : base(typeof (String), VType.V_STR, 0)
        {
        }

        public VString(Object vValue)
            : base(typeof (String), VType.V_STR, vValue)
        {
        }

        public static VString Instance
        {
            get { return m_instance; }
        }

        public override byte[] Encode(Object vValue)
        {
            var value = (String) vValue;
            //byte[] encodeValues = m_encoding.GetBytes(value);
            //Array.Reverse(encodeValues);
            var ec = m_encoding.GetEncoder(); //获取字符编码
            var charArray = value.ToCharArray();
            var length = ec.GetByteCount(charArray, 0, charArray.Length, false); //获取字符串转换为二进制数组后的长度，用于申请存放空间
            var encodeValues = new Byte[length]; //申请存放空间
            ec.GetBytes(charArray, 0, charArray.Length, encodeValues, 0, true); //将字符串按照特定字符编码转换为二进制数组

            return Utils.FillLengthHead(encodeValues);
        }

        public override Object Decode(byte[] data, ref Int32 index)
        {
            var l = 0;
            var strData = CutLengthHead(data, ref index, out l);
            return m_encoding.GetString(strData, 0, l);
        }
    }
}