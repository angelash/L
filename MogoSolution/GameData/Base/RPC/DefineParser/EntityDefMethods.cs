#region 模块信息

/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：EntityDefMethod
// 创建者：Ash Tang
// 修改者列表：
// 创建日期：2013.1.16
// 模块描述：实体方法声明。
//----------------------------------------------------------------*/

#endregion

using System.Collections.Generic;

namespace Mogo.RPC
{
    /// <summary>
    ///     实体方法声明。
    /// </summary>
    public class EntityDefMethod
    {
        public ushort FuncID { get; set; }
        public string FuncName { get; set; }
        public List<VObject> ArgsType { get; set; }

        public override string ToString()
        {
            return FuncName;
        }
    }
}