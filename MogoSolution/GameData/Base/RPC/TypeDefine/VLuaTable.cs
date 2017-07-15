#region 模块信息

/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：VLuaTable
// 创建者：Ash Tang
// 修改者列表：
// 创建日期：2013.3.29
// 模块描述：Lua Table（LuaTable）。
//----------------------------------------------------------------*/

#endregion

using System;
using System.Text;
using Mogo.Util;

namespace Mogo.RPC
{
    /// <summary>
    ///     Lua Table（LuaTable）。
    /// </summary>
    public class VLuaTable : VObject
    {
        private static readonly VLuaTable m_instance = new VLuaTable();

        public VLuaTable()
            : base(typeof (LuaTable), VType.V_LUATABLE, 0)
        {
        }

        public VLuaTable(Object vValue)
            : base(typeof (LuaTable), VType.V_LUATABLE, vValue)
        {
        }

        public static VLuaTable Instance
        {
            get { return m_instance; }
        }

        public override byte[] Encode(Object vValue)
        {
            LuaTable value;
            Utils.PackLuaTable(vValue, out value);
            var s = Utils.PackLuaTable(value);
            return VString.Instance.Encode(s);
        }

        public override Object Decode(byte[] data, ref Int32 index)
        {
            var l = 0;
            var strData = CutLengthHead(data, ref index, out l);
            LuaTable luaTable;
            //替换LoadLib 的Utils 里面的 相关函数支持长度
            return ParseLuaTable(strData, l, out luaTable) ? luaTable : null;
        }

        /// <summary>
        ///     将 Byte流 转换为 Lua table 实体
        /// </summary>
        /// <param name="inputString">Byte[]</param>
        /// <param name="result">实体对象</param>
        /// <returns>返回 true/false 表示是否成功</returns>
        private static bool ParseLuaTable(byte[] inputString, int l, out LuaTable result)
        {
            if (inputString[0] != '{' || inputString[l - 1] != '}')
            {
                result = null;
                return false;
            }
            if (l == 2)
            {
                result = new LuaTable();
                return true;
            }

            var index = 0;
            object obj;
            var flag = DecodeLuaTable(inputString, l, ref index, out obj);
            if (flag)
                result = obj as LuaTable;
            else
                result = null;
            return flag;
        }

        private static bool DecodeLuaTable(byte[] inputString, int len, ref int index, out object result)
        {
            var luaTable = new LuaTable();
            result = luaTable;
            if (!Utils.WaitChar(inputString, '{', ref index))
            {
                return false;
            }
            try
            {
                if (Utils.WaitChar(inputString, '}', ref index)) //如果下一个字符为右大括号表示为空Lua table
                    return true;
                while (index < len)
                {
                    string key;
                    bool isString;
                    object value;
                    Utils.DecodeKey(inputString, ref index, out key, out isString); //匹配键
                    Utils.WaitChar(inputString, '=', ref index); //匹配键值对分隔符
                    var flag = DecodeLuaValue(inputString, ref index, out value); //转换实体
                    if (flag)
                    {
                        luaTable.Add(key, isString, value);
                    }
                    if (!Utils.WaitChar(inputString, ',', ref index))
                        break;
                }
                Utils.WaitChar(inputString, '}', ref index);
                return true;
            }
            catch (Exception e)
            {
                LoggerHelper.Error("Parse LuaTable error: " + inputString + e);
                return false;
            }
        }

        private static bool DecodeLuaValue(byte[] inputString, ref int index, out object value)
        {
            var firstChar = inputString[index];
            if (firstChar == 's')
            {
                var szLen = Encoding.UTF8.GetString(inputString, index + 1, 3);
                var length = Int32.Parse(szLen);
                index += 4;
                if (length > 0)
                {
                    value = Encoding.UTF8.GetString(inputString, index, length);
                    index += length;
                    return true;
                }
                value = "";
                return true;
            }
            if (firstChar == '{') //如果第一个字符为花括号，表示接下来的内容为列表或实体类型
            {
                return DecodeLuaTable(inputString, inputString.Length, ref index, out value);
            }
            var i = index;
            while (++index < inputString.Length)
            {
                if (inputString[index] == ',' || inputString[index] == '}')
                {
                    if (index > i)
                    {
                        value = Encoding.UTF8.GetString(inputString, i, index - i);
                        return true;
                    }
                }
            }
            LoggerHelper.Error("Decode Lua Table Value Error: " + index + " " + inputString);
            value = null;
            return false;
        }
    }
}