using Mogo.FSM;
using Mogo.Util;
using System;
using UnityEngine;

namespace Mogo.Game
{
    public class EntityPlayer : EntityParent
    {
        public Transform SlotCamera;
        public Transform SlotHead;
        public Transform SlotBillboard;

        public override void CreateModel()
        {
            LoggerHelper.Debug(Name + " " + MapId + " " + HeadQuarterId);
        }

        public override void FaceTo(Vector3 face)
        {
            rotation = face;
            //LoggerHelper.Debug(rotation);
            if (ViewGameObject)
                TweenRotation.Begin(ViewGameObject, 0.1f, Quaternion.Euler(rotation));
        }

        //向坐标有过程地移动过去
        public override void MoveTo(Vector3 pos)
        {
            position = pos;
            //LoggerHelper.Debug(position); 
            if (ViewGameObject)
                TweenPosition.Begin(ViewGameObject, 0.2f, position);
        }

        protected void InitComponent(UnityEngine.Object obj)
        {
        }

    }
}
