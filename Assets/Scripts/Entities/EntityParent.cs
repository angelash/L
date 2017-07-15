using Mogo.FSM;
using Mogo.RPC;
using Mogo.Util;
using Mogo.GameData;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mogo.Game
{
    public partial class EntityParent
    {
        #region RPC 属性

        private uint m_teamId;

        public uint TeamId
        {
            get { return m_teamId; }
            set { m_teamId = value; }
        }

        private string m_name;

        public string Name
        {
            get { return m_name; }
            set
            {
                m_name = value;
                //LoggerHelper.Debug(value);
            }
        }

        private int m_mapId;

        public int MapId
        {
            get { return m_mapId; }
            set
            {
                m_mapId = value;
            }
        }

        private int m_headQuarterId;

        public int HeadQuarterId
        {
            get { return m_headQuarterId; }
            set
            {
                m_headQuarterId = value;
            }
        }

        #endregion

        /// <summary>
        /// 实体定义名。
        /// </summary>
        public string entityType = "Avatar";
        private UInt32 _id;
        public UInt32 ID
        {
            get { return _id; }
            set
            {
                _id = value;
            }
        }
        private UInt64 _dbid = 0;
        public UInt64 dbid
        {
            get { return _dbid; }
            set
            {
                _dbid = value;
            }
        }

        public EntityDef entity;
        public Transform ViewTransform;
        public Transform CtrlTransform;
        public Transform BillboardTransform { get; set; }
        public GameObject ViewGameObject;
        public GameObject CtrlGameObject;
        public Vector3 position = Vector3.zero;
        public Vector3 rotation = Vector3.zero;
        public Vector3 scale = Vector3.one;

        public Animator animator;
        public ActorParent actor;
        public CharacterController ViewController;
        public CharacterController CtrlController;


        protected FSMMotion fsmMotion = new FSMMotion();
        protected string currentMotionState = MotionState.IDLE;
        public string CurrentMotionState
        {
            get { return currentMotionState; }
            set
            {
                currentMotionState = value;
            }
        }

        virtual public void CreateModel()
        {
        }

        virtual public void OnEnterWorld()
        {
            AddListener();
            //LoggerHelper.Debug(this.GetType() + " " + ID);
        }

        virtual public void OnLeaveWorld()
        {
            GameObject.Destroy(ViewGameObject);
        }

        virtual public void AddSpeedUpFx()
        {
        }

        virtual public void DestroySpeedUpFx()
        {
        }

        /// <summary>
        /// 强制设置模型坐标和朝向
        /// </summary>
        public virtual void UpdatePosition()
        {
            var rot = Quaternion.Euler(rotation);
            if (ViewTransform)
            {
                ViewTransform.rotation = rot;
                ViewTransform.position = position;
            }
            if (CtrlTransform)
            {
                CtrlTransform.rotation = rot;
                CtrlTransform.position = position;
            }
        }

        public virtual void FaceTo(Vector3 face)
        {
        }

        virtual public void MoveTo(Vector3 pos)
        {
        }

        public void RpcCall(string func, params object[] args)
        {
            ServerProxy.Instance.RpcCall(func, args);
        }

        virtual public void ChangeMotionState(string newState, params System.Object[] args)
        {
            fsmMotion.ChangeStatus(this, newState, args);
        }

        public virtual void OnDeath()
        {
        }

        public virtual void OnReborn()
        {
        }

        #region 属性方法同步

        private Dictionary<string, object> objectAttrs = new Dictionary<string, object>();
        private Dictionary<string, int> intAttrs = new Dictionary<string, int>();
        private Dictionary<string, double> doubleAttrs = new Dictionary<string, double>();
        private Dictionary<string, string> stringAttrs = new Dictionary<string, string>();
        public Dictionary<string, object> ObjectAttrs
        {
            get { return objectAttrs; }
            set { objectAttrs = value; }
        }

        public Dictionary<string, int> IntAttrs
        {
            get { return intAttrs; }
            set { intAttrs = value; }
        }

        public Dictionary<string, double> DoubleAttrs
        {
            get { return doubleAttrs; }
            set { doubleAttrs = value; }
        }

        public Dictionary<string, string> StringAttrs
        {
            get { return stringAttrs; }
            set { stringAttrs = value; }
        }
        private readonly static HashSet<TypeCode> m_intSet = new HashSet<TypeCode>() { TypeCode.SByte, TypeCode.Byte, TypeCode.Int16, TypeCode.UInt16, TypeCode.Int32 };
        private readonly static HashSet<TypeCode> m_doubleSet = new HashSet<TypeCode>() { TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Single, TypeCode.Double };

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        protected void SetAttr(EntityDefProperties propInfo, object value, Type type)
        {
            var prop = type.GetProperty(propInfo.Name);
            try
            {
                if (prop != null)
                {
                    prop.SetValue(this, value, null);
                }
                else
                {
                    var typeCode = Type.GetTypeCode(propInfo.VType.VValueType);
                    if (m_intSet.Contains(typeCode))
                        intAttrs[propInfo.Name] = Convert.ToInt32(value);
                    else if (m_doubleSet.Contains(typeCode))
                        doubleAttrs[propInfo.Name] = Convert.ToDouble(value);
                    else if (propInfo.VType.VValueType == typeof(string))
                        stringAttrs[propInfo.Name] = value as string;
                    else
                        objectAttrs[propInfo.Name] = value;
                    //LoggerHelper.Info("Static property not found: " + propInfo.Name);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("SetAttr error: " + propInfo.VType.VValueType + ":" + propInfo.Name + " " + value.GetType() + ":" + value + "\n" + ex);
                LoggerHelper.Error("prop: " + prop + " this: " + this.GetType());
            }
        }

        /// <summary>
        /// 回调方法缓存
        /// </summary>
        private List<KeyValuePair<string, Action<object[]>>> m_respMethods = new List<KeyValuePair<string, Action<object[]>>>();

        /// <summary>
        /// 订阅Define文件里网络回调函数。
        /// 注册了大量的方法事件
        /// RPC_方法名字----》方法调用
        /// </summary>
        protected void AddListener()
        {
            var ety = Mogo.RPC.DefParser.Instance.GetEntityByName(entityType);
            if (ety == null)
            {
                LoggerHelper.Warning("Entity not found: " + entityType);
                return;
            }
            foreach (var item in ety.ClientMethodsByName)
            {
                var methodName = item.Key;
                var method = this.GetType().GetMethod(methodName, ~System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    var e = new KeyValuePair<string, Action<object[]>>(String.Concat(Mogo.Util.Utils.RPC_HEAD, methodName), (args) =>
                    {//RPC回调事件处理
                        try
                        {
                            method.Invoke(this, args);
                        }
                        catch (Exception ex)
                        {
                            var sb = new System.Text.StringBuilder();
                            sb.Append("method paras are: ");
                            foreach (var methodPara in method.GetParameters())
                            {
                                sb.Append(methodPara.ParameterType + " ");
                            }
                            sb.Append(", rpc resp paras are: ");
                            foreach (var realPara in args)
                            {
                                sb.Append(realPara.GetType() + " ");
                            }

                            Exception inner = ex;
                            while (inner.InnerException != null)
                            {
                                inner = inner.InnerException;
                            }
                            LoggerHelper.Error(String.Format("RPC resp error: method name: {0}, message: {1} {2} {3}", methodName, sb.ToString(), inner.Message, inner.StackTrace));
                        }
                    });
                    EventDispatcher.AddEventListener<object[]>(e.Key, e.Value);
                    m_respMethods.Add(e);
                }
                //else
                //    LoggerHelper.Warning("Method not found: " + item.Key);
            }

        }

        /// <summary>
        /// 移除订阅Define文件里网络回调函数。
        /// </summary>
        protected void RemoveListener()
        {
            //LoggerHelper.Warning("EventDispatcher.TheRouter.Count: " + EventDispatcher.TheRouter.Count);

            //LoggerHelper.Warning("m_respMethods: " + m_respMethods.Count);
            foreach (var e in m_respMethods)
            {
                EventDispatcher.RemoveEventListener<object[]>(e.Key, e.Value);
            }
            m_respMethods.Clear();
            //LoggerHelper.Warning("EventDispatcher.TheRouter.Count: " + EventDispatcher.TheRouter.Count);
        }

        public int GetIntAttr(string attrName)
        {
            return intAttrs.GetValueOrDefault(attrName, 0);
        }

        public void SetIntAttr(string attrName, int value)
        {
            intAttrs[attrName] = value;
        }

        public void SetDoubleAttr(string attrName, double value)
        {
            doubleAttrs[attrName] = value;
        }

        public double GetDoubleAttr(string attrName)
        {
            return doubleAttrs.GetValueOrDefault(attrName, 0);
        }

        public string GetStringAttr(string attrName)
        {
            return stringAttrs.GetValueOrDefault(attrName, "");
        }

        public object GetObjectAttr(string attrName)
        {
            return objectAttrs.GetValueOrDefault(attrName, null);
        }

        public void SetObjectAttr(string attrName, object value)
        {
            objectAttrs[attrName] = value;
        }

        /// <summary>
        /// 根据网络数据设置实体属性值。
        /// </summary>
        /// <param name="args"></param>
        public void SetEntityInfo(BaseAttachedInfo info)
        {
            ID = info.id;
            dbid = info.dbid;
            entity = info.entity;
            SynEntityAttrs(info);
        }

        /// <summary>
        /// 根据网络数据设置实体属性值。
        /// </summary>
        /// <param name="args"></param>
        public void SetEntityCellInfo(CellAttachedInfo info)
        {
            position = info.position;
            rotation = new Vector3(0, info.face * 2, 0);
            SynEntityAttrs(info);
            //重新开放以下代码，因为方法中已加加载场景容错处理
            //LoggerHelper.Debug(position);
            UpdatePosition();
        }

        public void SynEntityAttrs(AttachedInfo info)
        {
            if (info.props == null)
                return;
            var type = this.GetType();
            foreach (var prop in info.props)
            {
                SetAttr(prop.Property, prop.Value, type);
            }
        }

        #endregion
    }
}