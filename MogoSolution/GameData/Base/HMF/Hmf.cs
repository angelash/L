/**
The MIT License (MIT)

Copyright(C) 2013 <Hooke HU>

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
**/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using Object = System.Object;

namespace HMF
{
    public class Hmf
    {
        protected List<bool> bools = new List<bool>();
        protected List<double> doubles = new List<double>();
        protected List<int> ints = new List<int>();
        protected Type m_baseType = typeof (Object);
        protected Stream stream;
        protected List<string> strs = new List<string>();

        public Hmf()
        {
        }

        public Hmf(Type baseType)
        {
            m_baseType = baseType;
        }

        protected void Reset()
        {
            ints = new List<int>();
            doubles = new List<double>();
            strs = new List<string>();
            bools = new List<bool>();
            bools.Add(false);
            bools.Add(true);
            stream = null;
        }

        protected void SetStream(Stream stream)
        {
            this.stream = stream;
        }

        public void WriteObject(object obj, Stream stream)
        {
            SetStream(stream);
            RealWrite(obj);
            MergeAll();
        }

        protected void RealWrite(object obj)
        {
            var type = obj.GetType();
            var bt = type;
            while (bt != typeof (Object))
            {
                if (bt == m_baseType)
                {
                    WriteObj(obj);
                    return;
                }
                bt = bt.BaseType;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Dictionary<,>))
            {
                WriteDict(obj as IDictionary);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (List<>))
            {
                WriteArray(obj as IList);
            }
            else if (type == typeof (int))
            {
                WriteInt(Tag.INT32_TAG, Convert.ToInt32(obj));
            }
            else if (type == typeof (UInt32))
            {
                WriteInt(Tag.UINT32_TAG, Convert.ToInt32(obj));
            }
            else if (type == typeof (byte))
            {
                WriteInt(Tag.BYTE_TAG, Convert.ToInt32(obj));
            }
            else if (type == typeof (sbyte))
            {
                WriteInt(Tag.SBYTE_TAG, Convert.ToInt32(obj));
            }
            else if (type == typeof (Int16))
            {
                WriteInt(Tag.INT16_TAG, Convert.ToInt32(obj));
            }
            else if (type == typeof (UInt16))
            {
                WriteInt(Tag.UINT16_TAG, Convert.ToInt32(obj));
            }
            else if (type == typeof (bool))
            {
                WriteInt(Tag.BOOL_TAG, Convert.ToInt32(obj));
            }
            else if (type == typeof (double))
            {
                WriteDouble(Tag.DOUBLE_TAG, (double) obj);
            }
            else if (type == typeof (float))
            {
                WriteDouble(Tag.FLOAT_TAG, Convert.ToDouble(obj.ToString()));
            }
            else if (type == typeof (string))
            {
                WriteString((string) obj);
            }
            else if (type.BaseType == typeof (Enum))
            {
                WriteInt(Tag.INT32_TAG, Convert.ToInt32(obj));
            }
            else if (type == typeof (Vector3))
            {
                WriteVector((Vector3) obj);
            }
            else if (type == typeof (Color))
            {
                WriteColor((Color) obj);
            }
            else
            {
                Debug.Log("RealWrite error data: " + obj + " type: " + type);
            }
        }

        protected bool IsDefault(object value)
        {
            var type = value.GetType();

            //if (type.BaseType == typeof(Enum))
            //{
            //    type = Enum.GetUnderlyingType(type);
            //    if(value == Enum.)
            //}


            if (type == typeof (int) || type == typeof (byte) || type == typeof (sbyte) || type == typeof (Int16) ||
                type == typeof (UInt32) ||
                type == typeof (UInt16) || value.GetType() == typeof (double) || value.GetType() == typeof (float) ||
                type.BaseType == typeof (Enum))
            {
                if (Convert.ToDouble(value) == 0)
                    return true;
                return false;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Dictionary<,>))
            {
                var dic = value as IDictionary;
                if (dic == null || dic.Count == 0)
                    return true;
                return false;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (List<>))
            {
                var list = value as IList;
                if (list == null || list.Count == 0)
                    return true;
                return false;
            }
            if (type == typeof (string))
            {
                var str = value as string;
                return String.IsNullOrEmpty(str);
            }
            if (type == typeof (Vector3))
            {
                if ((Vector3) value == Vector3.zero)
                    return true;
                return false;
            }
            if (type == typeof (bool))
            {
                if ((bool) value == false)
                    return true;
                return false;
            }
            if (type == typeof (Color))
            {
                if ((Color) value == new Color(0, 0, 0, 0))
                    return true;
                return false;
            }
            Debug.Log("IsDefault error data: " + value + " type: " + type);

            return false;
        }

        protected void WriteInt(byte tag, int v)
        {
            Util.WriteVarint(tag, stream);
            var idx = ints.IndexOf(v);
            if (idx != -1)
            {
                Util.WriteVarint(idx, stream);
                return;
            }
            ints.Add(v);
            idx = ints.Count - 1;
            Util.WriteVarint(idx, stream);
        }

        protected void WriteDouble(byte tag, double v)
        {
            Util.WriteVarint(tag, stream);
            var idx = doubles.IndexOf(v);
            if (idx != -1)
            {
                Util.WriteVarint(idx, stream);
                return;
            }
            doubles.Add(v);
            idx = doubles.Count - 1;
            Util.WriteVarint(idx, stream);
        }

        protected void WriteString(string v)
        {
            Util.WriteVarint(Tag.STRING_TAG, stream);
            var idx = strs.IndexOf(v);
            if (idx != -1)
            {
                Util.WriteVarint(idx, stream);
                return;
            }
            strs.Add(v);
            idx = strs.Count - 1;
            Util.WriteVarint(idx, stream);
        }

        protected void WriteArray(IList v)
        {
            Util.WriteVarint(Tag.ARRAY_TAG, stream);
            var len = v.Count;
            Util.WriteVarint(len, stream);
            for (var i = 0; i < len; i++)
            {
                RealWrite(v[i]);
            }
        }

        protected void WriteVector(Vector3 obj)
        {
            Util.WriteVarint(Tag.VECTOR_TAG, stream);


            WriteDoubleWithoutTag(obj.x);
            WriteDoubleWithoutTag(obj.y);
            WriteDoubleWithoutTag(obj.z);
        }

        protected void WriteColor(Color obj)
        {
            Util.WriteVarint(Tag.COLOR_TAG, stream);


            WriteDoubleWithoutTag(obj.a);
            WriteDoubleWithoutTag(obj.r);
            WriteDoubleWithoutTag(obj.b);
            WriteDoubleWithoutTag(obj.g);
        }

        protected void WriteDoubleWithoutTag(double x)
        {
            var idx = doubles.IndexOf(x);
            if (idx != -1)
            {
                Util.WriteVarint(idx, stream);
            }
            else
            {
                doubles.Add(x);
                idx = doubles.Count - 1;
                Util.WriteVarint(idx, stream);
            }
        }

        protected void WriteDict(IDictionary v)
        {
            Util.WriteVarint(Tag.OBJECT_TAG, stream);
            var len = v.Count;
            Util.WriteVarint(len, stream);
            foreach (var i in v.Keys)
            {
                RealWrite(i);
                RealWrite(v[i]);
            }
        }

        protected void WriteObj(object obj)
        {
            Util.WriteVarint(Tag.OBJECT_TAG, stream);
            var props = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public); //获取实体属性
            var temp = new Dictionary<string, object>();
            foreach (var p in props)
            {
                if (p.GetSetMethod(true) == null)
                    continue;
                var value = p.GetValue(obj, null);
                if (value != null && !IsDefault(value))
                {
                    temp.Add(p.Name, value);
                }
            }

            Util.WriteVarint(temp.Count, stream);
            foreach (var item in temp)
            {
                RealWrite(item.Key);
                RealWrite(item.Value);
            }
        }

        protected void MergeAll()
        {
            var st = new MemoryStream();
            Util.WriteVarint(ints.Count, st);
            for (var i = 0; i < ints.Count; i++)
            {
                Util.WriteVarint(ints[i], st);
            }
            Util.WriteVarint(doubles.Count, st);
            for (var i = 0; i < doubles.Count; i++)
            {
                Util.WriteDobule(doubles[i], st);
            }
            Util.WriteVarint(strs.Count, st);
            for (var i = 0; i < strs.Count; i++)
            {
                var s = Encoding.UTF8.GetBytes(strs[i]);
                Util.WriteVarint(s.Length, st);
                Util.WriteStr(s, st);
            }
            stream.Position = 0;
            st.Position = 0;
            var stLen = (int) st.Length;
            var streamLen = (int) stream.Length; //(int)st.Length;
            var stBytes = new byte[stLen];
            var streamBytes = new byte[streamLen];
            stream.Read(streamBytes, 0, streamLen);
            st.Read(stBytes, 0, stLen);
            stream.Position = 0;
            stream.Write(stBytes, 0, stLen);
            stream.Write(streamBytes, 0, streamLen);
        }

        public object ReadObject(Stream stream)
        {
            SetStream(stream);
            InitPool();
            var tag = (byte) Util.ReadVarint(stream);
            object rst = null;
            if (tag == Tag.ARRAY_TAG)
            {
                rst = ReadArray();
                Reset();
                return rst;
            }
            if (tag == Tag.OBJECT_TAG)
            {
                rst = ReadDict();
                Reset();
                return rst;
            }
            Reset();
            return null;
        }

        protected void InitPool()
        {
            var len_ints = Util.ReadVarint(stream);
            for (var i = 0; i < len_ints; i++)
            {
                ints.Add(Util.ReadVarint(stream));
            }
            var len_doubles = Util.ReadVarint(stream);
            for (var i = 0; i < len_doubles; i++)
            {
                doubles.Add(Util.ReadDouble(stream));
            }
            var len_strs = Util.ReadVarint(stream);
            for (var i = 0; i < len_strs; i++)
            {
                var l = Util.ReadVarint(stream);
                strs.Add(Util.ReadStr(l, stream));
            }
        }

        protected List<object> ReadArray()
        {
            var rst = new List<object>();
            var len = Util.ReadVarint(stream);
            for (var i = 0; i < len; i++)
            {
                var tag = (byte) Util.ReadVarint(stream);
                if (tag == Tag.ARRAY_TAG)
                {
                    rst.Add(ReadArray());
                }
                else if (tag == Tag.OBJECT_TAG)
                {
                    rst.Add(ReadDict());
                }
                else if (tag == Tag.VECTOR_TAG)
                {
                    rst.Add(ReadVector3(stream));
                }
                else if (tag == Tag.INT32_TAG)
                {
                    rst.Add(ints[Util.ReadVarint(stream)]);
                }
                else if (tag == Tag.UINT32_TAG)
                {
                    rst.Add((uint) ints[Util.ReadVarint(stream)]);
                }
                else if (tag == Tag.BYTE_TAG)
                {
                    rst.Add((byte) ints[Util.ReadVarint(stream)]);
                }
                else if (tag == Tag.SBYTE_TAG)
                {
                    rst.Add((sbyte) ints[Util.ReadVarint(stream)]);
                }
                else if (tag == Tag.INT16_TAG)
                {
                    rst.Add((Int16) ints[Util.ReadVarint(stream)]);
                }
                else if (tag == Tag.UINT16_TAG)
                {
                    rst.Add((UInt16) ints[Util.ReadVarint(stream)]);
                }
                else if (tag == Tag.BOOL_TAG)
                {
                    rst.Add(Convert.ToBoolean(ints[Util.ReadVarint(stream)]));
                }
                else if (tag == Tag.STRING_TAG)
                {
                    rst.Add(strs[Util.ReadVarint(stream)]);
                }
                else if (tag == Tag.DOUBLE_TAG)
                {
                    rst.Add(doubles[Util.ReadVarint(stream)]);
                }
                else if (tag == Tag.FLOAT_TAG)
                {
                    rst.Add((float) doubles[Util.ReadVarint(stream)]);
                }
                else if (tag == Tag.COLOR_TAG)
                {
                    rst.Add(ReadColor(stream));
                }
            }
            return rst;
        }

        protected Vector3 ReadVector3(Stream stream)
        {
            var v = new Vector3();
            v.x = (float) doubles[Util.ReadVarint(stream)];
            v.y = (float) doubles[Util.ReadVarint(stream)];
            v.z = (float) doubles[Util.ReadVarint(stream)];
            return v;
        }

        protected Color ReadColor(Stream stream)
        {
            var v = new Color();
            v.a = (float) doubles[Util.ReadVarint(stream)];
            v.r = (float) doubles[Util.ReadVarint(stream)];
            v.b = (float) doubles[Util.ReadVarint(stream)];
            v.g = (float) doubles[Util.ReadVarint(stream)];
            return v;
        }

        protected Dictionary<object, object> ReadDict()
        {
            var rst = new Dictionary<object, object>();
            var len = Util.ReadVarint(stream);
            for (var i = 0; i < len; i++)
            {
                var tag = (byte) Util.ReadVarint(stream);
                object k = null;
                if (tag == Tag.ARRAY_TAG)
                {
                    k = ReadArray();
                }
                else if (tag == Tag.OBJECT_TAG)
                {
                    k = ReadDict();
                }
                else if (tag == Tag.VECTOR_TAG)
                {
                    k = ReadVector3(stream);
                }
                else if (tag == Tag.INT32_TAG)
                {
                    k = ints[Util.ReadVarint(stream)];
                }
                else if (tag == Tag.UINT32_TAG)
                {
                    k = (uint) ints[Util.ReadVarint(stream)];
                }
                else if (tag == Tag.BYTE_TAG)
                {
                    k = (byte) ints[Util.ReadVarint(stream)];
                }
                else if (tag == Tag.SBYTE_TAG)
                {
                    k = (sbyte) ints[Util.ReadVarint(stream)];
                }
                else if (tag == Tag.INT16_TAG)
                {
                    k = (Int16) ints[Util.ReadVarint(stream)];
                }
                else if (tag == Tag.UINT16_TAG)
                {
                    k = (UInt16) ints[Util.ReadVarint(stream)];
                }
                else if (tag == Tag.BOOL_TAG)
                {
                    k = Convert.ToBoolean(ints[Util.ReadVarint(stream)]);
                }
                else if (tag == Tag.STRING_TAG)
                {
                    k = strs[Util.ReadVarint(stream)];
                }
                else if (tag == Tag.DOUBLE_TAG)
                {
                    k = doubles[Util.ReadVarint(stream)];
                }
                else if (tag == Tag.FLOAT_TAG)
                {
                    k = (float) doubles[Util.ReadVarint(stream)];
                }

                //	if(k==null)
                //	UnityEngine.Debug.Log("is  Null");

                tag = (byte) Util.ReadVarint(stream);
                object v = null;
                if (tag == Tag.ARRAY_TAG)
                {
                    v = ReadArray();
                }
                else if (tag == Tag.OBJECT_TAG)
                {
                    v = ReadDict();
                }
                else if (tag == Tag.INT32_TAG)
                {
                    v = ints[Util.ReadVarint(stream)];
                }
                else if (tag == Tag.UINT32_TAG)
                {
                    v = (uint) ints[Util.ReadVarint(stream)];
                }
                else if (tag == Tag.BYTE_TAG)
                {
                    v = (byte) ints[Util.ReadVarint(stream)];
                }
                else if (tag == Tag.SBYTE_TAG)
                {
                    v = (sbyte) ints[Util.ReadVarint(stream)];
                }
                else if (tag == Tag.INT16_TAG)
                {
                    v = (Int16) ints[Util.ReadVarint(stream)];
                }
                else if (tag == Tag.UINT16_TAG)
                {
                    v = (UInt16) ints[Util.ReadVarint(stream)];
                }
                else if (tag == Tag.BOOL_TAG)
                {
                    v = Convert.ToBoolean(ints[Util.ReadVarint(stream)]);
                }
                else if (tag == Tag.STRING_TAG)
                {
                    v = strs[Util.ReadVarint(stream)];
                }
                else if (tag == Tag.DOUBLE_TAG)
                {
                    v = doubles[Util.ReadVarint(stream)];
                }
                else if (tag == Tag.FLOAT_TAG)
                {
                    v = (float) doubles[Util.ReadVarint(stream)];
                }
                else if (tag == Tag.VECTOR_TAG)
                {
                    v = ReadVector3(stream);
                }
                else if (tag == Tag.COLOR_TAG)
                {
                    v = ReadColor(stream);
                }

                //if(rst.ContainsKey(k))
                //UnityEngine.Debug.Log("has key:"+k);
                rst.Add(k, v);
            }
            return rst;
        }
    }
}