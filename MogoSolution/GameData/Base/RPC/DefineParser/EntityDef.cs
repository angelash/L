#region 模块信息

/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：EntityDef
// 创建者：Ash Tang
// 修改者列表：
// 创建日期：2013.1.16
// 模块描述：实体类声明。
//----------------------------------------------------------------*/

#endregion

using System.Collections.Generic;

namespace Mogo.RPC
{
    /// <summary>
    ///     实体类声明。
    /// </summary>
    public class EntityDef
    {
        public ushort ID { get; set; }
        public string Name { get; set; }
        public string ParentName { get; set; }
        public EntityDef Parent { get; set; }
        public string StrUniqueIndex { get; set; }
        public bool BHasCellClient { get; set; }
        public Dictionary<ushort, EntityDefProperties> Properties { get; set; }
        public List<EntityDefProperties> PropertiesList { get; set; }
        public Dictionary<string, ushort> PropertiesIdMapping { get; set; }

        /// <summary>
        ///     服务端回调的方法
        /// </summary>
        public Dictionary<string, EntityDefMethod> ClientMethodsByName { get; set; }

        /// <summary>
        ///     客户端调用服务端的方法
        /// </summary>
        public Dictionary<string, EntityDefMethod> BaseMethodsByName { get; set; }

        /// <summary>
        ///     客户端调用服务端的方法
        /// </summary>
        public Dictionary<string, EntityDefMethod> CellMethodsByName { get; set; }

        public Dictionary<ushort, EntityDefMethod> ClientMethodsByID { get; set; }
        public Dictionary<ushort, EntityDefMethod> BaseMethodsByID { get; set; }
        public Dictionary<ushort, EntityDefMethod> CellMethodsByID { get; set; }

        public EntityDefMethod TryGetBaseMethod(string name)
        {
            return TryGetBaseMethod(name, this);
        }

        public EntityDefMethod TryGetBaseMethod(string name, EntityDef entity)
        {
            if (entity == null)
            {
                return null;
            }
            if (entity.BaseMethodsByName != null && entity.BaseMethodsByName.ContainsKey(name))
            {
                return entity.BaseMethodsByName[name];
            }
            return TryGetBaseMethod(name, entity.Parent);
        }

        public EntityDefMethod TryGetClientMethod(string name)
        {
            return TryGetClientMethod(name, this);
        }

        public EntityDefMethod TryGetClientMethod(string name, EntityDef entity)
        {
            if (entity == null)
            {
                return null;
            }
            if (entity.ClientMethodsByName != null && entity.ClientMethodsByName.ContainsKey(name))
            {
                return entity.ClientMethodsByName[name];
            }
            return TryGetClientMethod(name, entity.Parent);
        }

        public EntityDefMethod TryGetBaseMethod(ushort id)
        {
            return TryGetBaseMethod(id, this);
        }

        public EntityDefMethod TryGetBaseMethod(ushort id, EntityDef entity)
        {
            if (entity == null)
            {
                return null;
            }
            if (entity.BaseMethodsByID != null && entity.BaseMethodsByID.ContainsKey(id))
            {
                return entity.BaseMethodsByID[id];
            }
            return TryGetBaseMethod(id, entity.Parent);
        }

        public EntityDefMethod TryGetClientMethod(ushort id)
        {
            return TryGetClientMethod(id, this);
        }

        public EntityDefMethod TryGetClientMethod(ushort id, EntityDef entity)
        {
            if (entity == null)
            {
                return null;
            }
            if (entity.ClientMethodsByID != null && entity.ClientMethodsByID.ContainsKey(id))
            {
                return entity.ClientMethodsByID[id];
            }
            return TryGetClientMethod(id, entity.Parent);
        }

        public EntityDefMethod TryGetCellMethod(ushort id)
        {
            return TryGetCellMethod(id, this);
        }

        public EntityDefMethod TryGetCellMethod(ushort id, EntityDef entity)
        {
            if (entity == null)
            {
                return null;
            }
            if (entity.CellMethodsByID != null && entity.CellMethodsByID.ContainsKey(id))
            {
                return entity.CellMethodsByID[id];
            }
            return TryGetCellMethod(id, entity.Parent);
        }

        public EntityDefMethod TryGetCellMethod(string name)
        {
            return TryGetCellMethod(name, this);
        }

        public EntityDefMethod TryGetCellMethod(string name, EntityDef entity)
        {
            if (entity == null)
            {
                return null;
            }
            if (entity.CellMethodsByName != null && entity.CellMethodsByName.ContainsKey(name))
            {
                return entity.CellMethodsByName[name];
            }
            return TryGetCellMethod(name, entity.Parent);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}