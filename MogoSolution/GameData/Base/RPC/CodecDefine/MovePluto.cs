using System;

namespace Mogo.RPC
{
    public class MovePluto : Pluto
    {
        private readonly CellAttachedInfo info = new CellAttachedInfo();
        /// <summary>
        ///     将远程方法调用编码为二进制数组。
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        /// <returns>编码后的二进制数组</returns>
        public Byte[] Encode(byte face, ushort x, ushort y, ushort z)
        {
            Push(VUInt16.Instance.Encode(MSGIDType.BASEAPP_CLIENT_MOVE_REQ));
            Push(VUInt8.Instance.Encode(face));
            Push(VUInt16.Instance.Encode(x));
            Push(VUInt16.Instance.Encode(y));
            Push(VUInt16.Instance.Encode(z));

            var result = new Byte[m_unLen];
            Buffer.BlockCopy(m_szBuff, 0, result, 0, m_unLen);
            EndPluto(result);
            if (RPCMsgLogManager.IsRecord)
            {
                RPCMsgLogManager.Send(MSGIDType.BASEAPP_CLIENT_MOVE_REQ, x, y);
            }
            return result;
        }

        /// <summary>
        ///     将远程调用的方法解码为MovePluto调用。
        /// </summary>
        /// <param name="data">远程调用方法的二进制数组</param>
        /// <param name="unLen">数据偏移量</param>
        protected override void DoDecode(byte[] data, ref int unLen)
        {
            info.face = (byte)VUInt8.Instance.Decode(data, ref unLen);
            var x = (UInt16)VUInt16.Instance.Decode(data, ref unLen);
            var y = (UInt16)VUInt16.Instance.Decode(data, ref unLen);
            var z = (UInt16)VUInt16.Instance.Decode(data, ref unLen);
            info.x = (short)x;
            info.y = (short)y;
            info.z = (short)z;

            Arguments = new object[1] { info };
        }

        public override void HandleData()
        {
        }

        /// <summary>
        ///     创建新MovePluto实例。
        /// </summary>
        /// <returns>MovePluto实例</returns>
        internal static Pluto Create()
        {
            return new MovePluto();
        }
    }
}