using System;
using Mogo.Util;

namespace Mogo.RPC
{
    internal class DefuseLoginPluto : Pluto
    {
        public Byte[] Encode()
        {
            return null;
        }

        protected override void DoDecode(byte[] data, ref int unLen)
        {
        }

        public override void HandleData()
        {
            EventDispatcher.TriggerEvent(Events.FrameWorkEvent.DefuseLogin);
        }

        internal static Pluto Create()
        {
            return new DefuseLoginPluto();
        }
    }
}