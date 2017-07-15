#region 模块信息

/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：EntityDefProperties
// 创建者：Ash Tang
// 修改者列表：
// 创建日期：2013.1.16
// 模块描述：实体属性声明。
//----------------------------------------------------------------*/

#endregion

namespace Mogo.RPC
{
    /// <summary>
    ///     实体属性声明。
    /// </summary>
    public class EntityDefProperties
    {
        public string Name { get; set; }
        public VObject VType { get; set; }
        public bool BSaveDb { get; set; }
        public string DefaultValue { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}